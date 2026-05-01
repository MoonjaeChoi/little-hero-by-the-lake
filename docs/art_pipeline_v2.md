# Little Hero — 아트 파이프라인 v2 (정식)

> 2026-05-01 — Phase A·B 환경 검증 + 첫 이미지 생성 검증 완료 후 작성. v1_draft (`art_pipeline_v1_draft.md`)의 일반론을 본 인프라·실측 데이터·사용자 결정사항으로 정식화.
>
> **상태**: 환경 셋업 완료 / 톤 잠금 (Phase A) 시작 직전.

---

## 0. v1_draft 대비 변경 사항 (요약)

| 항목 | v1_draft | v2 (확정) |
|---|---|---|
| GPU 환경 | 미정 | ✅ **RTX 5070 12GB** (192.168.75.194) |
| MCP 서버 | Aseprite MCP / ComfyUI MCP / pixel-plugin 후보 | ✅ **ComfyUI MCP (artokun)** + 직접 API |
| 후처리 도구 | Krita / Photoshop 후보 | ✅ **Krita 5.3.1 + Krita AI Diffusion 1.49.1** |
| LLM 보조 | EXAONE 7.8B 추정 | ✅ vLLM 현재 미가동 — Claude Opus 4.7 한국어로 충분 |
| 시간 추정 | 9~12일 | ✅ **2일** (실측 9.21초/장 기반 재계산) |
| 비용 | $30~60 클라우드 옵션 | ✅ **$0** (자체 서버) |
| 모델 | SDXL base | ✅ **DreamShaperXL Turbo SFW** (캐주얼·만화 톤) |
| 일관성 | IP-Adapter / ControlNet / LoRA 후보 | ✅ **IP-Adapter Plus SDXL + ControlNet Canny + 자체 LoRA** (Day 1 학습) |

---

## 1. 확정 환경

### 서버 (`192.168.75.194`)
- **OS**: Oracle Linux 9
- **GPU**: NVIDIA GeForce RTX 5070, 12GB VRAM, sm_120 (Blackwell)
- **CUDA**: 13.1, Driver 590.48.01
- **Python**: 3.12.12
- **PyTorch**: 2.12 nightly cu128 (sm_120 호환)
- **ComfyUI**: 0.20.1 (tmux session `comfyui`, port 8188)
- **노드**: ComfyUI_IPAdapter_plus / comfyui_controlnet_aux / ComfyUI-Manager
- **vLLM**: 현재 미가동 — GPU 100% ComfyUI 전용

### 모델 카탈로그 (총 12GB)

| 종류 | 파일 | 용도 |
|---|---|---|
| Checkpoint | `DreamShaperXL_Turbo_SFWdpmppSde_half_pruned.safetensors` (6.5GB) | 캐주얼 만화 톤 메인 |
| IP-Adapter | `ip-adapter-plus_sdxl_vit-h.safetensors` (810MB) | reference 톤 강제 |
| CLIP Vision | `CLIP-ViT-H-14-laion2B-s32B-b79K.safetensors` (2.4GB) | IP-Adapter 백엔드 |
| ControlNet | `controlnet-canny-sdxl-1.0.safetensors` (2.4GB) | 외곽선 제어 |

### MacBook 측 (Apple M1 16GB)
- **Krita**: 5.3.1 (`/Applications/krita.app`)
- **AI Diffusion 플러그인**: v1.49.1 (`~/Library/Application Support/krita/pykrita/ai_diffusion/`)
- **SSH 터널**: `localhost:8188 → 192.168.75.194:8188`
- **Claude Code**: ComfyUI MCP 호출 가능

### 운영 패턴
1. 서버: ComfyUI = 생성 공장 (RTX 5070 풀 사용)
2. MacBook: Krita = 후처리 워크스테이션 + Unity Editor + Claude Code IDE
3. SSH 터널로 양쪽 연결 (LAN 직접 접속 X — 방화벽)
4. vLLM 재가동 시 시간대 분리 cron (현재 vLLM 미가동이라 불필요)

---

## 2. 첫 이미지 검증 결과

**프롬프트**: "cute friendly slime creature, large round eyes, soft plush body, pastel green tones..."  
**결과**: `LittleHero/Assets/_Project/Art/References/little_hero_test_00001_.png`  
**시간**: 9.21초 / 1024×1024 / 8-step / cfg 2.0 / dpmpp_sde / karras

### Claude Vision 평가 (CLAUDE.md §2 톤 가이드 대조)

| 톤 가이드 | 평가 |
|---|---|
| 비폭력 (피·무기·공포 X) | ✅ 완벽 |
| 둥글둥글 실루엣 | ✅ 완벽 |
| 파스텔 채도 60~70% | ✅ |
| 큰 눈 (kawaii) | ✅ |
| 친근한 표정 | ✅ |
| 4+ 등급 | ✅ |
| **합계** | **9/10** |

**약점**: 텍스처가 약간 "실사 플러시" 느낌. negative `realistic, fluffy texture` 추가 또는 자체 LoRA 학습으로 해결.

---

## 3. 아트 스타일 결정 (사용자 승인 필요)

v1_draft §10 의 5개 미해결 사항 중 4개 여전히 미정. **첫 이미지 결과(9/10)를 보면 사실상 옵션 B (벡터·플랫 일러스트) 가 자연스러운 흐름**.

### 권장 (확정 대기)
- **옵션 B 확정** + DreamShaperXL Turbo 톤 유지
- 픽셀화 옵션은 Aseprite 후처리로 보조 가능 (필요 시)

사용자 최종 승인 후 v2.1 로 확정.

---

## 4. 일관성 메커니즘 (4중 안전장치)

### 4.1 IP-Adapter Plus SDXL (필수)
- 5 카테고리 reference 1장씩 → 모든 후속 생성에 reference 강제
- weight 0.7~0.85 (캐릭터·UI 강하게, 배경 약하게)

### 4.2 ControlNet Canny (포즈·구도 선택)
- 30 archetype 적의 통일된 포즈(정면 + 측면) sketch → 전 적에 강제 적용
- 챕터 배경은 사용 안 함

### 4.3 little_hero_style LoRA (Day 1 후반에 학습)
- Reference 5~10장 학습 → 모든 호출에 weight 0.6 자동 적용
- ComfyUI Manager 의 LoRA Trainer 노드 활용 가능

### 4.4 비폭력 톤 4중 필터

#### 1단계: 모델 (DreamShaperXL Turbo SFW)
- **SFW 변형** 선택 — NSFW 학습 데이터 거의 없음
- 캐주얼·만화 톤 학습 → 4+ 등급 친화

#### 2단계: 표준 Negative Prompt (모든 생성 자동 첨부)
```
blood, gore, violence, weapons, sharp objects, scary, dark, horror,
realistic, photorealistic, gritty, edgy, mature, adult,
muscular, exposed skin, blood splatter,
death, skull, demon, evil, sinister, menacing,
high saturation, neon, oversaturated, harsh shadows
```

#### 3단계: 표준 Positive Prompt 헤더 (CLAUDE.md §2 인용)
```
pastel colors saturation 60-70%, soft rounded silhouettes,
cute kawaii, children-friendly age 4+, Studio Ghibli inspired,
warm gentle palette, soft outlines, plush toy aesthetic,
non-violent, sparkles and hearts and flower petals
```

#### 4단계: Claude Vision 사후 검증 (자동)
- 생성 직후 Opus 4.7 Vision 호출
- 검출 키워드: 피·무기·어두운 색조·무서운 표정·날카로운 부분
- 1점이라도 검출 시 자동 폐기 + 재생성

---

## 5. 130~150 에셋 분류 + 우선순위

### 5.1 카테고리별 에셋 수 (Phase F 시드 결과 기준)

| 카테고리 | 수 | 비고 |
|---|---|---|
| 5 archetype 적 (Meta/Nature/Animal/Civ/Fantasy × 6) | 30 | F1+F2+F3 placeholder |
| 일일 보스 7 + 주간 보스 4 | 11 | F4+F5 placeholder |
| 캐릭터 (소년/소녀/조용한 친구) | 3 | 각 idle/walk/attack 4프레임 |
| 35 챕터 배경 | 35 | F1~F3 placeholder |
| 16 장비 아이콘 | 16 | 기존 PNG 재생성 |
| 12 슬롯 아이콘 + 4 화폐 아이콘 | 16 | 신규 |
| 4 추억 픽업 | 4 | |
| 메타 업그레이드 + 펫 | 6 | |
| VFX 파티클 (별/하트/꽃잎/비눗방울 변형) | 8~12 | |
| **합계** | **약 130** | |

### 5.2 우선순위 (가장 시각 임팩트 큰 것부터)

| 우선 | 작업 | 수량 | 시간 |
|---|---|---|---|
| 🔴 P0 | 5 reference + LoRA 학습 | 5 + 1 LoRA | 2~3시간 |
| 🔴 P1 | 3 캐릭터 (idle/walk) | 3 | 30분 |
| 🔴 P2 | 30 적 archetype | 30 | 1시간 |
| 🟡 P3 | 11 보스 (일일 + 주간) | 11 | 30분 |
| 🟡 P4 | 35 챕터 배경 | 35 | 2시간 |
| 🟠 P5 | 32 UI 아이콘 (장비·슬롯·화폐) | 32 | 1.5시간 |
| 🟢 P6 | 4 추억 + 6 메타 + 12 파티클 | 22 | 1시간 |
| 🟢 P7 | Vision 일관성 review + drift 재생성 | — | 2시간 |
| 🟢 P8 | Krita 후처리 일괄 | — | 4~6시간 |
| 🟢 P9 | Unity import + 게임 내 검증 | — | 2시간 |

**총 16~20시간 = 2일 풀타임** (1인).

---

## 6. 시간표 — 2일 plan

### Day 1 (8시간)

| 시각 | 작업 | 산출물 |
|---|---|---|
| 09:00~10:30 | P0-1: 5 카테고리 reference 생성 (각 4장 × 5 = 20장) + Vision 톤 review로 1장씩 채택 | 5 reference PNG |
| 10:30~12:00 | P0-2: 5 reference + 첫 테스트 이미지 (총 6장)로 little_hero_style LoRA 학습 | LoRA `.safetensors` |
| 13:00~14:30 | P1: 3 캐릭터 idle/walk/attack 12 프레임 | 12 PNG |
| 14:30~16:00 | P2: 30 적 archetype | 30 PNG |
| 16:00~17:00 | P3: 11 보스 | 11 PNG |
| 17:00~18:00 | P7 1차: 53개 캐릭터·적·보스 Vision review + drift 5~10개 재생성 | drift 처리 |

**Day 1 끝 산출물**: 53 캐릭터·적·보스 PNG + 5 reference + 1 LoRA

### Day 2 (8시간)

| 시각 | 작업 | 산출물 |
|---|---|---|
| 09:00~11:00 | P4: 35 챕터 배경 (1024×768) | 35 PNG |
| 11:00~12:30 | P5: 32 UI 아이콘 (256×256) | 32 PNG |
| 13:30~14:30 | P6: 22 추억·메타·파티클 | 22 PNG |
| 14:30~16:30 | P7 2차 Vision review (전체 142 에셋) + drift 처리 | 일관성 보장 |
| 16:30~18:00 | P8 1차 후처리 (Krita 자동 배경 제거 + 외곽 정리) — 우선순위 높은 53개 | 가공 PNG |

**Day 2 끝 산출물**: 142 PNG 1차 완료. 후처리는 Day 3 일부 잔여.

### Day 3 (4~6시간, 잔여)

| 작업 | 산출물 |
|---|---|
| Krita 후처리 잔여 89개 (배경·UI·파티클) | 가공 PNG |
| Unity import 일괄 (스크립트로) | Sprite 142종 |
| 게임 내 검증 (CH1·CH7·CH35 Play 테스트) | 시각 통과 확인 |

---

## 7. Oracle DB metadata schema

재현성·재생성·패치 영구 자산을 위한 metadata 영구화.

### 7.1 Schema 생성 (1회, Option A)

```sql
-- 신규 schema (production ocr_admin과 격리)
CREATE USER little_hero_admin IDENTIFIED BY <vault>;
GRANT CONNECT, RESOURCE, UNLIMITED TABLESPACE TO little_hero_admin;
ALTER USER little_hero_admin DEFAULT TABLESPACE USERS;
```

### 7.2 테이블

```sql
CREATE TABLE asset_generation (
  id NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
  asset_id VARCHAR2(100) NOT NULL,        -- 'Enemy_Nature_Boss'
  category VARCHAR2(20),                  -- Meta/Nature/Animal/Civ/Fantasy/UI/Particle
  prompt_positive CLOB,
  prompt_negative CLOB,
  model VARCHAR2(100),                    -- 'DreamShaperXL_Turbo_SFW'
  loras CLOB,                             -- JSON: ['little_hero_style:0.6']
  ip_adapter_ref VARCHAR2(500),           -- reference 이미지 경로
  ip_adapter_weight NUMBER(3,2),
  controlnet_type VARCHAR2(50),
  controlnet_image VARCHAR2(500),
  seed NUMBER,
  width NUMBER, height NUMBER,
  steps NUMBER, cfg NUMBER(3,1),
  sampler VARCHAR2(50),
  output_path VARCHAR2(500),              -- LittleHero/Assets/_Project/Art/...
  vision_review_score NUMBER(3,1),        -- Claude Opus 자동 review 0~10
  vision_review_notes CLOB,
  approved CHAR(1) DEFAULT 'N',           -- Y/N
  created_at TIMESTAMP DEFAULT SYSTIMESTAMP,
  superseded_by NUMBER                    -- 재생성된 새 row의 id (history)
);

CREATE INDEX idx_asset_id ON asset_generation(asset_id);
CREATE INDEX idx_category ON asset_generation(category);
```

### 7.3 활용 시나리오
- **6개월 후 콘텐츠 패치**: 같은 톤 유지하며 신규 적 30종 추가 — `SELECT prompt_positive FROM asset_generation WHERE category='Nature' AND approved='Y'` 으로 톤 가이드 자동 추출
- **A/B 테스트**: 같은 asset_id 의 여러 row 생성 후 vision_review_score 비교
- **레거시 자산 history**: superseded_by 로 버전 추적

본 schema는 art_pipeline_v2 본체 작업 후 도입 권장 — 우선 PNG + JSON 파일 시스템으로 시작, 안정 후 DB 마이그레이션.

---

## 8. Krita ↔ ComfyUI 통합 흐름

```
[Claude Code IDE]
  ↓ 자연어 호출
[ComfyUI MCP (artokun)]
  ↓ HTTP POST workflow JSON
[localhost:8188 (SSH 터널)]
  ↓
[192.168.75.194:8188 ComfyUI on RTX 5070]
  ↓ 9.21초 생성
[/home/pro301/.../output/*.png]
  ↓ scp 또는 마운트
[MacBook ~/.../Art/References/]
  ↓ Krita 에서 import
[Krita: 배경 제거 + 외곽 정리 + inpaint 보정]
  ↓ Export PNG (transparent)
[LittleHero/Assets/_Project/Art/Enemies/Enemy_Nature_Boss.png]
  ↓ Unity Sprite Editor 자동 import
[Game.scene 에서 검증]
```

### 한 에셋 표준 시간 (실측 보정)
- 생성: 9.21초
- Vision 1차 review: 5~10초
- Krita 후처리: 1~3분
- Unity import + 검증: 30초~1분
- **합계: 약 2~5분/에셋**

130 에셋 × 평균 3.5분 = **약 7.5시간 + buffer 1.5일** 

---

## 9. 비용 비교 (v1 → v2)

| 항목 | v1_draft 추정 | v2 확정 |
|---|---|---|
| GPU 비용 | $30~60 클라우드 | $0 자체 서버 |
| Krita 라이센스 | $0 (오픈소스) | $0 |
| Aseprite | $19.99 (옵션 미선택) | $0 |
| Stable Diffusion 모델 | $0 (오픈소스) | $0 |
| 시간 (1인 풀타임) | 9~12일 | 2~3일 |
| **총 비용** | **$30~80 + 9~12일** | **$0 + 2~3일** |

**3~4배 효율** — RTX 5070 12GB + DreamShaperXL Turbo (8-step) + 자체 인프라의 결합 효과.

---

## 10. 알려진 위험 / 미해결

### 기술적
- **VRAM 12GB 한계**: SDXL + IP-Adapter + ControlNet + LoRA 동시 시 약 11GB. 큰 batch (>2) 위험
- **DreamShaperXL Turbo 8-step 한계**: 미세한 구조 표현 약함. 일부 에셋(특히 텍스트 간판) 재생성 필요할 수 있음
- **Triton sm_120 일부 미지원**: attention 최적화 fallback. 동작은 OK, 5~10% 느림

### 콘텐츠적
- **AI drift**: 4중 안전장치도 100% 막지 못함. 시즌 1 출시 후 패치에서 점진 교체
- **저작권**: SDXL 베이스·DreamShaperXL Turbo 모두 상업 이용 명시 (CreativeML OpenRAIL-M). 본 게임 4+ 등급 + 비폭력 톤 적합
- **Apple App Store**: AI 생성물 명시 의무 정책 — 출시 시 metadata에 "AI-assisted asset" 명시 권장

### 사용자 결정 미완 (v2.1 으로 확정)
1. **아트 스타일 최종 확정** — 첫 이미지 결과상 옵션 B(벡터·플랫 일러스트) 권장
2. **Krita AI Diffusion 플러그인 활성화 + ComfyUI 연결** — 사용자 GUI 단계
3. **LoRA 학습 시점** — Day 1 후반 (오후 시작)
4. **Oracle DB metadata 도입 시점** — v2 본체 작업 후 또는 동시
5. **시작 시점** — 즉시 또는 다른 우선순위?

---

## 11. 즉시 시작 액션

### A. 톤 잠금 (Day 1 09:00~10:30)
```python
# Claude Code 가 ComfyUI MCP 호출 또는 직접 API
for category in ['Meta', 'Nature', 'Animal', 'Civ', 'Fantasy']:
    for variant in range(4):
        submit_workflow(prompt=BUILD_REFERENCE_PROMPT(category, variant))
    # 4장 중 Vision 톤 평가 9+ 1장 채택
```

### B. LoRA 학습 (Day 1 10:30~12:00)
- 채택된 5 reference + 첫 테스트 (총 6장) 학습 데이터셋
- ComfyUI-LoRA-Trainer 노드 또는 kohya_ss
- 학습 약 1~2시간

### C. 본 게임 적용 작업 시작
v2.1 로 확정 후 5.2 우선순위 P1~P9 순차 진행.

---

## 12. 참조

- 본 v2 의 모든 결정은 v1_draft 의 11개 출처 위에 다음 실측 추가:
  - **첫 이미지 검증**: `LittleHero/Assets/_Project/Art/References/little_hero_test_00001_.png` (9.21초, Claude Vision 9/10)
  - **인프라**: `/Users/memmem/git/en-zine/docs/operation/infrastructure_reference.md`
  - **CLAUDE.md §2** 톤 절대 원칙
  - **chapter_structure_v2 §3** 5 카테고리 매핑

### 신규 인용 출처

- [PyTorch sm_120 (Blackwell) 공식 지원](https://github.com/pytorch/pytorch/issues/164342)
- [ComfyUI Blackwell 50XX 가이드 (공식 블로그)](https://blog.comfy.org/p/how-to-get-comfyui-running-on-your)
- [DreamShaper XL Turbo (Lykon, HuggingFace)](https://huggingface.co/Lykon/dreamshaper-xl-turbo)
- [IP-Adapter (h94, HuggingFace)](https://huggingface.co/h94/IP-Adapter)
- [ControlNet Canny SDXL 1.0 (xinsir)](https://huggingface.co/xinsir/controlnet-canny-sdxl-1.0)
- [Krita 5.3.1 Stable](https://krita.org/en/download/)
- [Krita AI Diffusion v1.49.1](https://github.com/Acly/krita-ai-diffusion/releases/tag/v1.49.1)

---

## 13. 다음 액션 (사용자 결정)

위 §10.1~10.5 미해결 5건 중:

1. **즉시 결정 가능** (5분):
   - 아트 스타일 옵션 B 확정 (또는 다른 옵션 선택)
   - LoRA 학습 시점 (Day 1 오전 또는 오후)
   - Oracle DB metadata 도입 시점 (지금 또는 나중)

2. **GUI 단계 필요** (사용자 직접):
   - Krita 첫 실행 + AI Diffusion 플러그인 활성화
   - Krita 의 ComfyUI 연결 (`http://localhost:8188`)
   - 첫 inpaint 테스트

3. **시작 시점**:
   - 즉시 Day 1 P0 (5 reference 생성)
   - 또는 Phase G 다른 우선순위 (사운드·한국어 검수)

확인 후 v2.1 (확정 버전) 작성 + 즉시 P0 시작 가능.
