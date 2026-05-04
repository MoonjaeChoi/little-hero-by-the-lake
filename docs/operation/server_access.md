# Little Hero — 서버 설정 + 운영 Reference

**최신**: 2026-05-04
**용도**: Little Hero (Unity 게임) 가 사용하는 공유 서버 (192.168.75.194) 의 접속·설정·운영 방법.
**공용성**: Clayve (zine-alpaca) 인프라 와 동일 서버를 공유. 본 문서는 Little Hero 관점 + 공유 자원 사용 패턴.

> ⚠️ **Secret 값** (passwords / API keys / JWT_SECRET) 은 본 문서에 포함 X. 실제 값은 1Password / vault / 서버의 `~/.env` 파일.
> 출처: `/Users/memmem/git/en-zine/docs/operation/infrastructure_reference.md` (2026-04-29) + `008_외부접속정보.md` + 본 프로젝트 BGM/ComfyUI 스킬.

---

## 0. 빠른 참조 (TL;DR)

| 작업 | 명령 |
|---|---|
| ComfyUI 터널 (이미지 생성) | `ssh -L 8188:localhost:8188 staging -N` |
| BGM 서버 시작 | `bash ~/.claude/skills/little-hero-bgm/scripts/bgm_start.sh` |
| BGM 서버 종료 | `bash ~/.claude/skills/little-hero-bgm/scripts/bgm_stop.sh` |
| 서버 SSH | `ssh staging` (alias) 또는 `ssh pro301@192.168.75.194` |
| 외부 SSH (LAN 외) | `ssh -p 29022 pro301@58.227.121.8` |
| ComfyUI health | `curl -s http://localhost:8188/system_stats` (터널 후) |
| ACE-Step health | `bash ~/.claude/skills/little-hero-bgm/scripts/bgm_status.sh` |
| GPU VRAM 상태 | `ssh staging "nvidia-smi"` |

---

## 1. 서버 개요

### 1.1 물리 서버

| 항목 | 값 |
|---|---|
| Hostname (LAN) | `staging`, `staging.local`, `staging-192` (SSH alias 모두 동일) |
| LAN IP | `192.168.75.194` |
| External IP (NAT) | `58.227.121.8` |
| OS | Oracle Linux 9 |
| GPU | NVIDIA RTX 5070 12GB VRAM |
| SSH user | `pro301` (uid 의 git 작업 디렉토리 `/home/pro301/git/`) |
| External SSH port | `29022` (기본 22 X — LAN 22 와 분리) |

### 1.2 SSH 접속

#### LAN 내부 (사무실 / VPN)

```bash
# Alias 사용 (~/.ssh/config 미리 설정됨)
ssh staging

# 또는 직접
ssh pro301@192.168.75.194
```

`~/.ssh/config` 항목 (확인용):
```
Host staging staging.local staging-192
    HostName 192.168.75.194
    User pro301
    Port 22
    IdentityFile ~/.ssh/oracle_linux_key
    StrictHostKeyChecking accept-new
```

#### LAN 외부 (외부 네트워크)

```bash
# 외부 IP + 비표준 포트 29022
ssh -p 29022 pro301@58.227.121.8
```

`~/.ssh/config` 의 `Host clayve.co.kr staging.clayve.co.kr staging-ext` 항목 사용 가능.

#### Post-quantum SSH 경고

OpenSSH 9.x 의 PQ key exchange 경고가 자주 등장. 무시해도 됨 (서버 OpenSSH 가 ML-KEM 미지원). 기존 키 인증 정상 작동.

---

## 2. GPU 자원 — 공유·경합 (중요)

**RTX 5070 12GB 는 서비스 다수가 공유**. VRAM 경합 시 OOM 빈발 — Little Hero 작업 시 반드시 인지.

| 서비스 | 용도 | VRAM (대략) | 라이프사이클 |
|---|---|---|---|
| **ComfyUI** (8188) | Little Hero 이미지 생성 (sticker, sprite, icon) | ~6~8GB | 항상 가동 (lazy unload 가능) |
| **ACE-Step** (7867) | Little Hero BGM 생성 | ~5GB | 작업 시만 (tmux acestep 세션) |
| **vLLM** (8000) | Clayve LLM (EXAONE 3.5 7.8B AWQ) | ~5GB | 항상 가동 (Clayve 의존) |

### 2.1 VRAM 경합 패턴

- **ComfyUI 와 ACE-Step 동시 가동 → OOM 위험**. ACE-Step 시작 전 ComfyUI VRAM 해제 필수
- **vLLM 은 항상 가동** — Clayve 가 매분 사용. 끄면 안 됨
- 따라서 실제 가용 = `12GB - 5GB(vLLM) = 7GB` → 그 안에서 ComfyUI 와 ACE-Step 가 양보

### 2.2 VRAM 양보 패턴 (BGM 스킬 자동 처리)

```bash
# 1. ACE-Step 시작 전 — ComfyUI free
curl -s -X POST http://localhost:8188/free \
  -H 'Content-Type: application/json' \
  -d '{"unload_models":true,"free_memory":true}'

# 2. ACE-Step 작업 종료 후 — 종료 (ComfyUI 가 다음 요청 시 lazy 재로드)
tmux kill-session -t acestep
```

`bgm_start.sh` 와 `bgm_stop.sh` 가 이 시퀀스를 자동화. 수동 호출 시에도 위 순서 준수.

### 2.3 VRAM 모니터링

```bash
ssh staging "nvidia-smi --query-gpu=memory.used,memory.free,utilization.gpu --format=csv"
```

---

## 3. ComfyUI (이미지 생성)

### 3.1 서버 측

| 항목 | 값 |
|---|---|
| 설치 위치 | `~/ai-tools/ComfyUI/` (서버) |
| Bind 주소 | `0.0.0.0:8188` (LAN 노출 — NAT 외부 차단 의존) |
| Auth | 없음 (LAN/터널 의존) |
| 베이스 모델 | DreamShaperXL Turbo (`DreamShaperXL_Turbo_SFWdpmppSde_half_pruned.safetensors`) |
| Custom Nodes | `huchenlei/ComfyUI-layerdiffuse` (5008 — RGBA 적 sprite) |

### 3.2 SSH 터널 (LAN 외 또는 보안 권장)

ComfyUI 가 `0.0.0.0:8188` 로 bind 되어 LAN 내부에서는 직접 접근 가능 (`http://192.168.75.194:8188`). 다만 보안 강화 + LAN 외부 (Mac dev 환경 등) 에서 SSH 터널 사용 권장:

```bash
# Foreground (터미널 1개 점유, Ctrl+C 로 종료)
ssh -L 8188:localhost:8188 staging -N

# Background daemon (-f 로 백그라운드 + 터미널 반환)
ssh -fN -L 8188:localhost:8188 staging

# autossh (재연결 자동화 — 권장)
autossh -M 0 -fN -o "ServerAliveInterval 30" -o "ServerAliveCountMax 3" \
  -L 8188:localhost:8188 staging
```

#### 터널 살아있는지 확인

```bash
# 로컬 8188 포트 LISTEN 검사
lsof -i :8188 | grep LISTEN

# ComfyUI 응답 직접 확인
curl -s --max-time 3 http://localhost:8188/system_stats | head -c 200
```

#### 터널 재기동 (실패 시)

```bash
# 1. 기존 터널 종료
pkill -f "ssh.*8188:localhost:8188"

# 2. 재시작
ssh -fN -L 8188:localhost:8188 staging
```

### 3.3 사용 — comfyui-image 스킬

본 프로젝트의 `comfyui-image` 스킬이 ComfyUI 호출을 추상화:

```bash
# 단일 sprite 생성 — fallback 워크플로 (default)
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_template.json \
  --prompt "<prompt>" \
  --seed 22222 \
  --output ./output/sprite.png

# RGBA native (LayerDiffuse 권장 — 5008 sprite 작업)
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json \
  --prompt "<prompt>" \
  --seed 22222 \
  --output ./output/sprite.png
```

### 3.4 LayerDiffuse Custom Node — **설치 완료** (2026-05-04)

5008 Sprint 1 에서 도입된 `huchenlei/ComfyUI-layerdiffuse` Custom Node — 서버에 **설치 완료**.

**설치 정보**:

| 항목 | 값 |
|---|---|
| 위치 | `~/ai-tools/ComfyUI/custom_nodes/ComfyUI-layerdiffuse/` |
| Repo | `https://github.com/huchenlei/ComfyUI-layerdiffuse` |
| 의존성 추가 | `diffusers 0.38.0` (신규) / `safetensors 0.7.0 → 0.8.0rc0` (업데이트) / `opencv-python` (기존) |
| 등록 노드 | `LayeredDiffusionApply` / `LayeredDiffusionDecode` / `LayeredDiffusionDecodeRGBA` / `LayeredDiffusionDecodeSplit` + Cond/Joint/Diff 변형 7종 (총 11종) |
| Import 시간 | ~1.0초 (ComfyUI 부팅 시) |

**5008 워크플로 매핑**:
- `assets/workflow_layerdiffuse.json` 의 노드 `10` (`LayeredDiffusionApply`) + 노드 `11` (`LayeredDiffusionDecode`) 모두 등록 정상

**검증 명령**:
```bash
ssh staging "curl -s http://localhost:8188/object_info | python3 -c 'import sys,json; d=json.load(sys.stdin); print(\"LayeredDiffusionApply\" in d, \"LayeredDiffusionDecode\" in d)'"
# 기대: True True
```

#### 재설치 / 업데이트 (참고)

```bash
ssh staging
cd ~/ai-tools/ComfyUI/custom_nodes/ComfyUI-layerdiffuse
git pull                                          # 업데이트
cd ~/ai-tools/ComfyUI && source venv/bin/activate
pip install -r custom_nodes/ComfyUI-layerdiffuse/requirements.txt --upgrade

# ComfyUI 재시작 (tmux 세션 재생성 — 기존 세션은 자동 종료될 수 있음)
tmux kill-session -t comfyui 2>/dev/null
tmux new-session -d -s comfyui 'cd ~/ai-tools/ComfyUI && source venv/bin/activate && python main.py --listen 0.0.0.0 --port 8188 2>&1 | tee /tmp/comfyui.log'
sleep 15
grep -q "To see the GUI go to" /tmp/comfyui.log && echo "✓ 재시작 완료" || tail -20 /tmp/comfyui.log
```

> ⚠️ **주의**: tmux session 재생성 시 ComfyUI 가 짧게 다운 (~10~15초). 다른 사용자 작업 중일 가능성 사전 확인 (`tmux capture-pane -t comfyui -p` 로 직전 활동 확인).

#### 미설치 시 (히스토리)

이전 fallback: `workflow_layerdiffuse.json` 호출 → 400 반환 → `workflow_template.json` (DreamShaperXL Turbo + dark BG flood-fill, 정확도 ~80%) 사용. **2026-05-04 설치 완료로 fallback 불필요**.

### 3.5 ComfyUI tmux 라이프사이클 (필요 시)

ComfyUI 가 항상 가동되어 있지만 명시적 재시작 필요 시:

```bash
ssh staging
tmux ls                        # 'comfyui' 세션 확인
tmux attach -t comfyui         # 로그 보기 (Ctrl-B D 로 detach)
tmux kill-session -t comfyui   # 종료
# 재시작 (Custom Node 변경 시 등)
tmux new-session -d -s comfyui 'cd ~/ai-tools/ComfyUI && source venv/bin/activate && python main.py --listen 0.0.0.0 --port 8188 2>&1 | tee /tmp/comfyui.log'
```

---

## 4. ACE-Step (BGM 음악 생성)

### 4.1 서버 측

| 항목 | 값 |
|---|---|
| 설치 위치 | `~/ai-tools/ACE-Step-1.5/` (서버 venv 7.6GB, PyTorch 2.10+cu128) |
| Bind 주소 | `0.0.0.0:7867` (LAN 노출) |
| tmux 세션명 | `acestep` |
| API 라이프사이클 | 작업 시만 가동 (VRAM 절약) |
| 출력 캐시 | `~/ai-tools/ACE-Step-1.5/.cache/acestep/tmp/api_audio/` |
| ffmpeg | `imageio-ffmpeg` static binary (venv 내) |

### 4.2 라이프사이클 — `little-hero-bgm` 스킬 자동화

```bash
# 시작 — ComfyUI VRAM 해제 + tmux acestep + /health 폴링
bash ~/.claude/skills/little-hero-bgm/scripts/bgm_start.sh

# 상태 — tmux + health + VRAM
bash ~/.claude/skills/little-hero-bgm/scripts/bgm_status.sh

# 종료 — tmux kill, ComfyUI 가 다음 요청 시 재로드
bash ~/.claude/skills/little-hero-bgm/scripts/bgm_stop.sh
```

### 4.3 수동 health check

```bash
# tmux 세션
ssh staging "tmux ls 2>/dev/null | grep acestep"

# API 응답
ssh staging "curl -s --max-time 3 http://localhost:7867/health"
# 기대: {"status":"ok","data":{"loaded_model":"..."}}
```

### 4.4 흔한 실수

- ACE-Step 시작 전 ComfyUI VRAM 해제 누락 → OOM. `bgm_start.sh` 자동 처리 (수동 우회 시 주의)
- 작업 종료 후 `bgm_stop.sh` 안 함 → ComfyUI 가 재시작 시 OOM 위험

---

## 5. SSH 터널 운영 (지속 연결)

ComfyUI 작업이 잦으면 SSH 터널을 데몬으로 유지:

### 5.1 autossh + launchd (macOS)

`~/Library/LaunchAgents/com.user.ssh.tunnel.comfyui.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.user.ssh.tunnel.comfyui</string>
    <key>ProgramArguments</key>
    <array>
        <string>/opt/homebrew/bin/autossh</string>
        <string>-M</string><string>0</string>
        <string>-N</string>
        <string>-o</string><string>ServerAliveInterval=30</string>
        <string>-o</string><string>ServerAliveCountMax=3</string>
        <string>-L</string><string>8188:localhost:8188</string>
        <string>staging</string>
    </array>
    <key>RunAtLoad</key><true/>
    <key>KeepAlive</key><true/>
    <key>StandardOutPath</key>
    <string>/tmp/ssh-tunnel-comfyui.log</string>
    <key>StandardErrorPath</key>
    <string>/tmp/ssh-tunnel-comfyui.err</string>
</dict>
</plist>
```

```bash
# 등록 + 시작
launchctl load -w ~/Library/LaunchAgents/com.user.ssh.tunnel.comfyui.plist

# 중지
launchctl unload ~/Library/LaunchAgents/com.user.ssh.tunnel.comfyui.plist
```

### 5.2 즉석 백그라운드 (간이)

```bash
ssh -fN -o "ServerAliveInterval=30" -o "ServerAliveCountMax=3" \
  -L 8188:localhost:8188 staging
```

세션 종료까지 유지 (재부팅 시 수동 재시작).

### 5.3 Connection refused 트러블슈팅

| 증상 | 원인 | 해결 |
|---|---|---|
| `Connection refused on localhost:8188` | 터널 미가동 | 5.1 또는 5.2 재시작 |
| 터널 살아있으나 ComfyUI 무응답 | 서버 ComfyUI 다운 | `ssh staging "tmux ls"` 후 ComfyUI tmux 재시작 |
| `ssh: Could not resolve hostname staging` | SSH config 미설정 | `~/.ssh/config` Host 항목 추가 |
| `Permission denied (publickey)` | SSH key 누락 | `ssh-add ~/.ssh/oracle_linux_key` |

---

## 6. 공유 자원 — Little Hero 가 사용 안 하지만 가용한 것

Clayve 인프라가 같은 서버에 가동 중. Little Hero 가 향후 서버 단 기능 도입 시 활용 가능.

### 6.1 Oracle Database (가용)

| 항목 | 값 |
|---|---|
| Edition | Oracle 23ai Free |
| Connection | `192.168.75.194:1521/FREEPDB1` |
| Production schema | `ocr_admin` (Clayve 사용 — Little Hero 가 사용 X 권장) |
| Lab staging schema | `ocr_admin_stg` |
| Driver | `oracledb` Thin client (Node.js) |

#### Little Hero 가 DB 도입 시 권장 패턴 (Schema 격리)

```sql
-- 새 schema 생성 (DBA 권한 필요)
CREATE USER little_hero_admin IDENTIFIED BY <strong-pw>;
GRANT CONNECT, RESOURCE TO little_hero_admin;
GRANT UNLIMITED TABLESPACE TO little_hero_admin;

-- 연결 테스트
sqlplus little_hero_admin/<pw>@192.168.75.194:1521/FREEPDB1
```

**Trade-off**: Unity 클라이언트가 직접 Oracle 접속하는 패턴은 비추 (드라이버 부담 + 보안). 백엔드 API 경유 권장 — 그러면 §6.2 Backend 인프라 활용.

### 6.2 Backend 인프라 (가용, 미사용)

Clayve 의 Node.js + Express + Oracle 백엔드가 가동 중. Little Hero 도 같은 패턴으로 별도 backend 추가 가능:

| Service | Host Port | 가용성 |
|---|---|---|
| Frontend (Clayve) | 4000 | 점유 |
| Backend-1/2/3 (Clayve) | 4001~4003 | 점유 |
| Redis | 6381 | 공유 가능 (DB index 별도, 0/1 사용 중 → 2+) |
| **Little Hero backend (가상)** | 4004 권장 | 미사용 — 신규 추가 가능 |

#### Little Hero 가 backend 신설 시

- 새 Docker network: `little-hero-network`
- 새 compose 파일: `LittleHero/server/docker-compose.production.yml`
- Schema 격리 (`little_hero_admin`)
- Redis DB index `2` (Clayve `0`/`1` 회피)
- BullMQ queue prefix `lh:`
- 외부 도메인 (해당 시): `lh.clayve.co.kr` 또는 신규

### 6.3 vLLM (LLM 서비스, 가용)

| 항목 | 값 |
|---|---|
| Endpoint | `http://192.168.75.194:8000/v1/...` (LAN only) |
| Model | LGAI-EXAONE/EXAONE-3.5-7.8B-Instruct-AWQ |
| Auth | 없음 (LAN 격리) |
| API | OpenAI-compatible (chat/completions) |

**Little Hero 활용 시나리오** (미실시):
- NPC 동적 대사 생성 (보스 도전 인사말 등)
- 사용자 닉네임 PG-12 검열
- 챕터 외 부가 스토리 동적 생성

**제약**:
- 항상 가동 중 (끄면 안 됨, Clayve 의존)
- 타 서비스와 vRAM 5GB 점유 — Little Hero 호출 빈도 ↑ 시 GPU 경합

### 6.4 Artisan / Mobile / Workers (가용 X)

OCR / PWA / BullMQ workers 등은 Little Hero 와 무관 (Clayve 매거진 사업 도메인 전용).

---

## 7. 포트 매핑 (전체)

서버에서 점유 중 + Little Hero 사용 — 포트 충돌 회피:

| 포트 | 프로토콜 | 점유 | 본 프로젝트 사용 |
|---|---|---|---|
| 22 | SSH | Linux SSH | ✅ (LAN) |
| 80, 443 | HTTP/HTTPS | Clayve Nginx | — |
| 1521 | Oracle | Oracle DB | △ (향후 가능) |
| 4000 | HTTP | Clayve Frontend | — |
| 4001~4003 | HTTP | Clayve Backend × 3 | — |
| 5000~5001, 5443 | HTTP/HTTPS | Clayve Lab Staging | — |
| 5200 | HTTP | Clayve Mobile | — |
| 6381 | Redis | Clayve Redis | △ (DB index 2+) |
| 7867 | HTTP | **ACE-Step (Little Hero BGM)** | ✅ |
| 8000 | HTTP | vLLM | △ (향후 가능) |
| 8081~8083 | HTTP/WS | Clayve Artisan | — |
| **8188** | HTTP | **ComfyUI (Little Hero 이미지)** | ✅ (`0.0.0.0` LAN bind + SSH 터널 권장) |
| 29022 | SSH | External SSH (NAT) | ✅ |

---

## 8. Health Monitoring (전체 점검)

### 8.1 빠른 체크 — 본 프로젝트 핵심

```bash
#!/usr/bin/env bash
# Little Hero 인프라 health check

set -e

echo "== SSH =="
ssh -o ConnectTimeout=3 staging "uname -a" || echo "(SSH 실패)"

echo
echo "== ComfyUI 터널 =="
lsof -i :8188 | grep LISTEN || echo "(터널 미가동)"

echo
echo "== ComfyUI 응답 =="
curl -s --max-time 3 http://localhost:8188/system_stats | head -c 200 || echo "(응답 없음)"

echo
echo "== ACE-Step =="
ssh staging "tmux ls 2>/dev/null | grep acestep" || echo "(tmux 미가동 — BGM 작업 시 bgm_start.sh)"
ssh staging "curl -s --max-time 3 http://localhost:7867/health" 2>/dev/null || echo "(API 미응답 — 정상, 작업 시만 가동)"

echo
echo "== GPU VRAM =="
ssh staging "nvidia-smi --query-gpu=memory.used,memory.free,utilization.gpu --format=csv,noheader"
```

### 8.2 Clayve 측 health (참고용 — Little Hero 와 무관)

```bash
# Backend
curl -s http://192.168.75.194:4001/api/v1/health/deep | jq '.checks'

# vLLM
curl -s http://192.168.75.194:8000/v1/models | jq '.data[].id'

# Redis
ssh staging "docker exec zine-redis-prod redis-cli ping"
```

---

## 9. 배포 정책 (Little Hero 측)

### 9.1 서버 측 변경 작업 원칙

- ❌ **서버에서 직접 코드 수정 금지** — 로컬 (Mac) 개발 → git push → 서버 git pull 패턴 (Clayve 와 동일)
- ❌ **Custom Node 무단 설치 금지** — `huchenlei/ComfyUI-layerdiffuse` 등 새 Custom Node 도입 시 본 문서 §3.4 갱신 + commit
- ❌ **ComfyUI tmux 임의 재시작 금지** — 이미지 작업 중인 다른 사용자 영향. 변경 필요 시 사용자 합의

### 9.2 Custom Node 변경 commit 패턴

```bash
# 1. 서버에서 설치 (한 번만)
ssh staging
cd ~/ai-tools/ComfyUI/custom_nodes
git clone <node-repo>
# ... pip install ...

# 2. 로컬 docs 갱신
# - docs/operation/server_access.md §3.4 항목 추가
# - 관련 워크플로 JSON 추가 (.claude/skills/comfyui-image/assets/)

# 3. commit
git add docs/operation/server_access.md .claude/skills/comfyui-image/assets/<new>.json
git commit -m "infra: ComfyUI <node-name> Custom Node 도입 — <목적>"
```

### 9.3 BGM 신규 트랙 commit 패턴

`little-hero-bgm` 스킬 사이클의 Step 6~8 따름. 자세한 내용은 스킬 SKILL.md.

---

## 10. 보안 모델

### 10.1 신뢰 경계

| 자산 | 보호 방식 |
|---|---|
| ComfyUI (8188) | `0.0.0.0` bind — LAN 내부 직접 접근 + 외부 NAT 차단 (29022 SSH 만 외부 라우팅) |
| ACE-Step (7867) | LAN bind (`0.0.0.0`) — LAN 신뢰 가정. 외부 접근 X |
| vLLM (8000) | LAN bind — 동일 |
| Oracle (1521) | LAN bind + schema 격리 |
| SSH (22 / 29022) | key 기반 인증 (`~/.ssh/oracle_linux_key`) |

### 10.2 Anti-pattern

- ComfyUI 가 현재 `0.0.0.0:8188` bind — LAN 노출 상태 (인증 없음). LAN 신뢰 가정 + 외부 NAT 차단으로 격리. **외부 포트 포워딩 시 즉시 침해 가능**, 절대 추가 X
- Custom Node 무검증 설치 → 임의 Python 코드 실행. 신뢰 author (huchenlei 등) 만
- SSH key 패스프레이즈 없음 → key 도난 시 즉시 침해. `ssh-keygen -p` 로 패스프레이즈 추가 권장

### 10.3 사고 대응

- 침해 의심 시 — 즉시 SSH key 회수 (`~/.ssh/authorized_keys` 의 항목 제거) + 비밀번호 변경
- 사용자: piratecmj@gmail.com (CLAUDE.md userEmail)

---

## 11. 트러블슈팅 인덱스

| 증상 | 원인 후보 | 빠른 진단 |
|---|---|---|
| `Connection refused on localhost:8188` | SSH 터널 미가동 | §5 터널 재시작 |
| `400` from ComfyUI `/prompt` | 워크플로 JSON 노드 invalid | 워크플로 재export (Settings → Save API Format) |
| `Polling timeout` (ComfyUI) | 모델 로딩 / GPU 경합 | `nvidia-smi` 로 VRAM 확인, `--timeout` ↑ |
| ACE-Step OOM | ComfyUI VRAM 미해제 | `bgm_start.sh` 사용 (자동 해제) |
| BGM 가사 합성 (vocal 누수) | prompt 에 `instrumental only, no vocals` 누락 | tracks.json prompt 수정 |
| SSH `Permission denied (publickey)` | key 미로드 | `ssh-add ~/.ssh/oracle_linux_key` |
| `huchenlei nodes not found` | LayerDiffuse 미설치 | §3.4 설치 또는 fallback 워크플로 사용 |
| BGM 트랙 무음 | OGG 변환 실패 | `bgm_status.sh` 로그 확인, WAV 직접 청취 |

---

## 12. 관련 스킬 / 문서

### 본 프로젝트 (Little Hero)

- `.claude/skills/comfyui-image/SKILL.md` — ComfyUI 호출 스킬 (이미지 생성)
- `.claude/skills/comfyui-image/references/comfyui_api.md` — ComfyUI REST API + LayerDiffuse 노드
- `~/.claude/skills/little-hero-bgm/SKILL.md` — ACE-Step BGM 스킬 (user-level)
- `docs/little_hero/enemy_sprite_prompts_v1.md` — 5008 적 sprite prompt 매핑
- `docs/prd/5008/` — 적 자산 품질 PRD 시리즈

### 공유 인프라 (Clayve)

- `/Users/memmem/git/en-zine/docs/operation/infrastructure_reference.md` — Clayve 전체 인프라 reference
- `/Users/memmem/git/en-zine/docs/operation/008_외부접속정보.md` — 외부 SSH/DB 접속
- `/Users/memmem/git/en-zine/docs/operation/010_접속정보.md` — 내부 네트워크 접속
- `/Users/memmem/git/en-zine/docs/operation/011_Oracle_Database_접속정보.md` — Oracle 상세

---

## 13. 변경 이력

- 2026-05-04: v1 초기 작성. Little Hero 관점 정리 + Clayve 공유 인프라 매핑 + BGM/ComfyUI 운영 통합. 출처 — Clayve `infrastructure_reference.md` (2026-04-29) + 본 프로젝트 BGM 스킬.
