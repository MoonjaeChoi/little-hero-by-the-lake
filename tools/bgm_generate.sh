#!/usr/bin/env bash
# BGM 생성 helper — ACE-Step v1.5 API 호출
# Usage: ./bgm_generate.sh <name> "<prompt>" [bpm] [keyscale] [duration_sec]
#
# 환경: SSH alias `staging` 가 RTX 5070 서버를 가리켜야 함.
# 출력: ./Assets/_Project/Audio/BGM/<name>.wav (프로젝트 루트 기준 상대경로)

set -e

NAME="${1:?name 필요}"
PROMPT="${2:?prompt 필요}"
BPM="${3:-100}"
KEYSCALE="${4:-C Major}"
DURATION="${5:-60}"

API="http://localhost:7867"
OUT_DIR="$(cd "$(dirname "$0")/.." && pwd)/LittleHero/Assets/_Project/Audio/BGM"
mkdir -p "$OUT_DIR"

echo "[1/4] ComfyUI VRAM 해제 (있을 시)..."
ssh staging "curl -s -X POST http://localhost:8188/free -H 'Content-Type: application/json' -d '{\"unload_models\":true,\"free_memory\":true}'" >/dev/null 2>&1 || true

echo "[2/4] 태스크 제출: $NAME ($BPM BPM, $KEYSCALE, ${DURATION}s)"
TASK_ID=$(ssh staging "curl -s -X POST $API/release_task -H 'Content-Type: application/json' -d @-" <<EOF | python3 -c "import sys,json; print(json.load(sys.stdin)['data']['task_id'])"
{
  "prompt": $(printf '%s' "$PROMPT" | python3 -c "import sys,json; print(json.dumps(sys.stdin.read()))"),
  "lyrics": "[instrumental]",
  "bpm": $BPM,
  "key_scale": "$KEYSCALE",
  "time_signature": "4",
  "audio_duration": $DURATION,
  "audio_format": "wav",
  "inference_steps": 8,
  "thinking": false
}
EOF
)
echo "    task_id=$TASK_ID"

echo "[3/4] 폴링 (최대 5분)..."
for i in $(seq 1 30); do
    sleep 10
    LOG_LINE=$(ssh staging "tail -5 /tmp/acestep_api.log | grep -E 'Saved audio|Generated.*audio tensors|ERROR' | tail -1" 2>/dev/null || true)
    if echo "$LOG_LINE" | grep -q "Saved audio"; then
        WAV_PATH=$(echo "$LOG_LINE" | grep -oE '/home/[^ ]+\.wav' | tail -1)
        echo "    완료: $WAV_PATH (${i}0s)"
        break
    fi
    echo "    ...T+${i}0s"
done

if [ -z "${WAV_PATH:-}" ]; then
    echo "ERROR: 5분 내 완료 안 됨" >&2
    exit 1
fi

echo "[4/4] 다운로드 → $OUT_DIR/${NAME}.wav"
ssh staging "cat $WAV_PATH" > "$OUT_DIR/${NAME}.wav"
echo "    크기: $(du -h "$OUT_DIR/${NAME}.wav" | cut -f1)"
echo "✓ 완료: $OUT_DIR/${NAME}.wav"
