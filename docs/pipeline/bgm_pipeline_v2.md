# Little Hero — BGM 파이프라인 v2 (정식)

> 2026-05-01 — 시나리오 A (셀프호스팅 only) 채택. RTX 5070 12GB + ACE-Step v1.5 설치·검증 완료.
>
> 본 문서는 v1 draft (`bgm_pipeline_v1_draft.md`) → 시나리오 A 결정 후 정식 계획.

---

## 1. 환경 (설치 완료)

| 항목 | 값 | 상태 |
|---|---|---|
| 서버 | staging (192.168.75.194), RTX 5070 12GB | 기존 ComfyUI 같이 사용 |
| Python | 3.12.12 (`/usr/bin/python3.12`) | OK |
| uv | 0.11.8 (`~/.local/bin/uv`) | 신규 설치 |
| ACE-Step | v1.5 (`~/ai-tools/ACE-Step-1.5/`) | 신규 설치 |
| PyTorch | 2.10.0+cu128 | venv 격리 (`~/ai-tools/ACE-Step-1.5/.venv/`, 7.6GB) |
| API 서버 | tmux session `acestep`, port 7867 | OK |
| 자동 설정 | tier4 / CPU offload / INT8 quant / 0.6B LM | RTX 5070 12GB 기준 자동 적용 |

### VRAM 사용 패턴 (중요)

- ComfyUI 와 ACE-Step 는 **VRAM 12GB 한계로 동시 사용 불가**.
- ACE-Step 사용 전: ComfyUI VRAM 해제 (`curl -X POST http://localhost:8188/free -d '{"unload_models":true,"free_memory":true}'`)
- ACE-Step 종료 후: ComfyUI 정상 재사용 가능 (프로세스는 그대로)

---

## 2. 검증 — 첫 생성

| 항목 | 값 |
|---|---|
| Prompt | "calm warm acoustic guitar and pan flute, lakeside village morning, peaceful adventure, kid-friendly Studio Ghibli style, instrumental only, no vocals" |
| BPM | 90 |
| Key | C Major |
| Duration | 30s |
| Inference steps | 8 (turbo) |
| 결과 | 5.7MB WAV, 48kHz stereo 16-bit |
| 소요 시간 | ~35초 (모델 다운로드 첫 1회 포함) |
| 결과 위치 | `/tmp/acestep_test/test_hub_lakeside.wav` |

---

## 3. API 워크플로

### 3.1 기본 흐름 (3 단계)

```
[1] POST /release_task (prompt + 메타)  →  task_id
[2] POST /query_result  (status 폴링)   →  status=1 (성공) 시 audio_url
[3] WAV 파일은 서버 .cache/acestep/tmp/api_audio/ 에 저장
```

### 3.2 cURL 예시

```bash
# 1. 태스크 제출
curl -s -X POST http://192.168.75.194:7867/release_task \
  -H "Content-Type: application/json" \
  -d '{
    "prompt": "...",
    "lyrics": "[instrumental]",
    "bpm": 90,
    "key_scale": "C Major",
    "time_signature": "4",
    "audio_duration": 60,
    "audio_format": "wav",
    "inference_steps": 8,
    "thinking": false
  }'

# 2. 결과 조회 (status: 0=running, 1=success, 2=fail)
curl -s -X POST http://192.168.75.194:7867/query_result \
  -d '{"task_ids": ["<task_id>"]}'

# 3. 다운로드 (서버 path → 로컬)
ssh staging "cat /home/pro301/ai-tools/ACE-Step-1.5/.cache/acestep/tmp/api_audio/<file>.wav" > out.wav
```

### 3.3 핵심 파라미터

| 파라미터 | 권장값 | 설명 |
|---|---|---|
| `prompt` | 자연어 (영문 권장) | 톤·악기·분위기·BPM 힌트 포함 |
| `lyrics` | `"[instrumental]"` | 인스트루멘탈 강제 (게임 BGM 표준) |
| `bpm` | 70~120 | 차분=70~90, 모험=100~110, 보스=120~140 |
| `key_scale` | `"C Major"` 등 | 메이저=밝음, 마이너=감성·보스 |
| `audio_duration` | 30~90s | 게임 BGM 한 루프. 길어도 600초까지 |
| `inference_steps` | 8 | turbo 권장. base 모델은 32~64 |
| `thinking` | `false` | true 면 5Hz LM 으로 가사·구조 enhance (인스트루멘탈은 false) |
| `audio_format` | `wav` | mp3/opus/aac/flac/wav32 가능. wav → ogg 변환 후 Unity |

---

## 4. Little Hero — BGM 트랙 계획 (40 트랙)

### 4.1 톤 매트릭스 (CLAUDE.md §2 비폭력 절대 원칙)

| 카테고리 | 트랙 수 | BPM | Key | 분위기 prompt 힌트 |
|---|---|---|---|---|
| **허브** (호숫가 마을) | 1 | 80 | C/G Major | "warm acoustic, lakeside, daylight, gentle breeze" |
| **다락방** | 1 | 70 | F Major | "soft music box, dusty attic, nostalgic, evening" |
| **상점** | 1 | 95 | D Major | "cheerful shop tune, market bells, brief loop" |
| **챕터 정규** (CH 1~35) | 35 | 100~110 | varies | 카테고리별 (Meta/Nature/Animal/Civ/Fantasy) |
| **미니보스** | 1 (공용) | 115 | A minor | "tense but kid-friendly, brief tension" |
| **챕터 보스** | 1 (공용) | 125 | D minor | "epic but cute, orchestral fanfare, no menace" |
| **일일/주간 보스** | 2 | 130 | E minor | "weekly grand, soft brass, hope motif" |
| **사망/Game Over** | 1 | 60 | A minor | "gentle sad chime, encourage retry" |

총 **42 트랙**. 모두 인스트루멘탈, 30~60초 루프 베이스.

### 4.2 챕터 카테고리별 prompt 시드 (`docs/little_hero/chapter_structure_v2.md` §3)

| 카테고리 | 챕터 | prompt 시드 |
|---|---|---|
| **Meta** | CH 1~5 (마을·학교·집·다락방·우체국) | "warm acoustic guitar + flute, friendly village, small adventure, 100 BPM" |
| **Nature** | CH 6~12 (호수·숲·들판·산골) | "pizzicato strings + woodwind, sparkling stream, breeze, 105 BPM" |
| **Animal** | CH 13~20 (토끼굴·새둥지·고래·늑대) | "playful percussion + harp, animal village, curious adventure, 108 BPM" |
| **Civilization** | CH 21~28 (도시·항구·기차·놀이공원) | "cheerful brass + glockenspiel, busy market, festive, 110 BPM" |
| **Fantasy** | CH 29~35 (별·달·구름·꿈) | "ethereal pad + chimes + harp, dream realm, magical, 95 BPM" |

---

## 5. 자동화 스크립트

### 5.1 `tools/bgm_generate.sh` (서버 측 helper)

- 입력: prompt + 메타 JSON
- 출력: 로컬 `Assets/_Project/Audio/BGM/<name>.wav`
- 동작: ComfyUI VRAM 해제 → release_task → 폴링 → tar 다운로드

### 5.2 일괄 생성 흐름 (Claude Code 자동화)

```
[1] tools/bgm_prompts.json — 42 트랙 prompt 마스터
[2] Claude Code → 각 트랙 release_task
[3] 폴링 (status=1 까지)
[4] tar 다운로드 → Assets/_Project/Audio/BGM/
[5] ffmpeg crossfade loop 후처리 (선택)
[6] AudioManager 와 ChapterRunner 에 wiring
```

소요 추정: 트랙당 60s 생성 + 다운로드 5s = **42 트랙 ≈ 50분**. 사용자 청취·선별 별도.

---

## 6. 후처리 — Seamless Loop

### 6.1 ffmpeg crossfade (Claude Code Bash)

```bash
# 끝 2초 + 시작 2초 교차 페이드, 60초 → 약 60초 seamless loop
ffmpeg -i in.wav -filter_complex "
  [0:a]atrim=2:60,asetpts=PTS-STARTPTS[main];
  [0:a]atrim=0:2,asetpts=PTS-STARTPTS,afade=t=in:st=0:d=2[head];
  [main][head]concat=n=2:v=0:a=1
" -c:a libvorbis -q:a 4 out.ogg
```

### 6.2 Unity 측 처리

`Core/AudioManager.cs` 확장:

```csharp
// BGM 슬롯 추가
[SerializeField] private AudioClip[] _bgmTracks;
private AudioSource _bgmSrc;

public void PlayBgm(int trackIdx, bool loop = true) { ... }
public IEnumerator CrossFadeBgm(int newIdx, float duration = 1.5f) { ... }
```

ChapterRunner.Start: `AudioManager.Instance.PlayBgm(_chapterData.bgmTrackIdx)`
WaveManager.OnBossWaveStart: `AudioManager.Instance.CrossFadeBgm(BOSS_TRACK_IDX)`

---

## 7. 다음 작업

| 우선 | 작업 | 시간 |
|---|---|---|
| 🔴 1 | 사용자가 첫 테스트 트랙 (`/tmp/acestep_test/test_hub_lakeside.wav`) 청취 | 1분 |
| 🟡 2 | 톤 OK 시 → 42 트랙 prompt JSON 마스터 작성 | 30분 |
| 🟡 3 | 일괄 생성 스크립트 (`tools/bgm_generate.sh`) | 20분 |
| 🟢 4 | 일괄 생성 실행 (~50분) + 사용자 선별 | 60~120분 |
| 🟢 5 | ffmpeg crossfade loop 후처리 | 30분 |
| 🟢 6 | Unity AudioManager BGM 확장 + ChapterRunner wiring | 60분 |
| 🟢 7 | 보스 fanfare CrossFadeBgm 통합 | 30분 |

총 추정 **4~6시간** (사용자 청취·선별 시간 제외).

---

## 8. 알려진 제약

| 제약 | 영향 | 완화 |
|---|---|---|
| 12GB VRAM | ComfyUI 와 동시 X | 작업 전 ComfyUI VRAM 해제 |
| 한국어 가사 | 학습 데이터 부족 추정 | **인스트루멘탈만** (CLAUDE.md §2 일관) |
| seamless loop | 생성 직후 끝-시작 끊김 | ffmpeg crossfade 표준화 |
| 품질 편차 | 동일 prompt 도 매번 다름 | 트랙당 batch_size=2 → 사용자 베스트 선택 |
| 모델 다운로드 첫 1회 | 약 5GB | 이미 완료 |

---

## 9. 출처

- ACE-Step v1.5: `~/ai-tools/ACE-Step-1.5/` (서버) / [GitHub](https://github.com/ace-step/ACE-Step-1.5)
- API 문서: `~/ai-tools/ACE-Step-1.5/docs/en/API.md` (서버)
- v1 draft: `docs/bgm_pipeline_v1_draft.md`
- 톤 가이드: `CLAUDE.md` §2
- 챕터 매핑: `docs/little_hero/chapter_structure_v2.md` §3
