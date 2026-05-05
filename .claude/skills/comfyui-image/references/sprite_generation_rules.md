# Sprite Generation Rules — V5 sticker tone, 단일 캐릭터 RGBA

> **출처**: 5008 T05.5 fix 사이클의 검증된 조합. Little Hero 적/캐릭터 sprite + 5006 Attic 버튼 아이콘 등 모든 sprite 작업의 baseline.
> **트리거**: 적/캐릭터/아이콘/sticker 등 **단일 객체 sprite** 를 RGBA (transparent BG) 로 생성할 때.

본 룰은 `comfyui-image/scripts/generate.py` 호출 시 인자/prompt 를 어떻게 구성해야 silhouette / 4-character grid / RGB 디테일 부족 같은 흔한 실패를 회피할 수 있는지 명시한다.

---

## 1. 검증 환경 (서버 측)

| 항목 | 값 | 비고 |
|---|---|---|
| **Checkpoint** | `sd_xl_base_1.0.safetensors` | DreamShaperXL Turbo 사용 X (LayerDiffuse RGB 평탄화) |
| Custom Node | `huchenlei/ComfyUI-layerdiffuse` | RGBA native transparency. 5008 패치 적용 (alpha inversion 수정) |
| 워크플로 | `assets/workflow_layerdiffuse.json` | 본 스킬 번들 |
| LayerDiffuseApply config | `"SDXL, Conv Injection"` | weight 1.0 |
| LayerDiffuseDecode 노드 | `LayeredDiffusionDecodeRGBA` | sd_version "SDXL", sub_batch_size 16 |

> Server checkpoint 미설치 시 → `docs/operation/server_access.md` §3.4 참조.

---

## 2. KSampler 검증 조합

| 파라미터 | 값 | 변경 시 영향 |
|---|---|---|
| `steps` | **20** | <15 시 디테일 손실 |
| `cfg` | **7.0~8.0** | >8.5 시 grid 가능성 ↑ / <6 시 prompt 무시 |
| `sampler_name` | `euler` | dpmpp_sde 도 가능하나 euler 가 검증 |
| `scheduler` | `normal` | karras 도 가능 |
| `denoise` | `1.0` | 신규 sprite 라 항상 1.0 |
| `seed` | 카테고리 통일 | Meta=22222 / Nature=33333 / Animal=44444 / Civ=55555 / Fantasy=66666 |
| `width / height` | 1024×1024 | sprite 표준 해상도 |

> Turbo 모델 사용 시 (steps 8 + cfg 2.0) 는 LayerDiffuse 와 호환 X. 일반 SDXL base 만 사용.

---

## 3. Prompt 구조 (Positive)

3 부분 결합 — 공통 베이스 + 카테고리 prefix + 캐릭터 디테일.

### 3.1 공통 베이스 (모든 sprite 동일)

```
cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting,
soft cartoon shading, chunky thick white outline,
three-quarter view, single character centered in frame,
fully transparent background, isolated subject
```

**중요 변경 — 5008 T05.5 fix 결과**:
- ❌ `isometric playful angle` — 폐기 (grid 유발)
- ✅ `three-quarter view` — 단일 캐릭터 안정
- ❌ `1:1 square composition` — 폐기 (panel 형식 유발)
- ✅ `single character centered in frame` — 명시적 단일 객체

### 3.2 카테고리 prefix (Little Hero 적 sprite 기준)

| 카테고리 | prefix | 시드 |
|---|---|---|
| Meta | `village friendly creature, warm beige and brown tones, cozy meadow vibes` | 22222 |
| Nature | `forest nature spirit, green and earth tones, leafy moss texture, woodland atmosphere` | 33333 |
| Animal | `cute domestic animal companion, warm orange and white tones, fluffy fur, friendly gaze` | 44444 |
| Civ | `urban paper or cardboard creature, soft pink and pastel tones, papercraft texture, gentle origami folds` | 55555 |
| Fantasy | `magical mystical being, purple and indigo tones, sparkles and stardust, ethereal glow` | 66666 |

### 3.3 캐릭터 디테일 (archetype 별)

`docs/little_hero/enemy_sprite_prompts_v1.md` 참조. 30 매핑 (5 × 6 archetype) 명시.

### 3.4 합성 예 — Civ_Small (paper crane)

```
cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting,
soft cartoon shading, chunky thick white outline,
three-quarter view, single character centered in frame,
fully transparent background, isolated subject,
urban paper or cardboard creature, soft pink and pastel tones,
papercraft texture, gentle origami folds,
paper crane with tiny wings, pastel pink folds
```

---

## 4. Negative Prompt (★ 단일 캐릭터 보장 핵심)

3 부분 — 일반 품질 + **단일 객체 강제** + **grid/panel 회피**.

```
low quality, blurry, watermark, photorealistic, dark, scary, blood, gore,
text, logo, signature, watermark,
multiple subjects, multiple characters, twins, duplicate, copies,
two characters, three characters, four characters,
grid, frame, border, comic panel, sprite sheet, character sheet,
2x2 grid, 4 panels, multi-panel, collage, mosaic,
isometric grid, tile
```

**중요 — 5008 T05.5 fix 의 핵심 수정**:
- ❌ 이전 negative: `"multiple subjects, grid"` 만 — 4-character grid 발생
- ✅ 현재: 11종 grid/panel/duplicate 키워드 + 4종 명시적 인물 수 — 단일 캐릭터 안정

---

## 5. 호출 패턴 (CLI)

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json \
  --prompt "<공통 베이스>, <카테고리 prefix>, <archetype 디테일>" \
  --negative "<§4 negative 전문>" \
  --seed <카테고리 시드> \
  --width 1024 --height 1024 \
  --steps 20 --cfg 7.0 \
  --output <path>
```

**주의**: `--steps` `--cfg` 가 워크플로 JSON 의 default 를 overwrite. JSON default 가 검증된 값이지만 명시 권장 (가독성 + safety).

---

## 6. 결과 검증 (자동)

각 sprite 생성 후 다음 메트릭으로 즉시 검증:

```python
from PIL import Image
import numpy as np

img = Image.open(path).convert("RGBA")
arr = np.array(img)
h, w = arr.shape[:2]

# 1. Alpha 적정성
visible = arr[arr[:,:,3] >= 128]
transparent = (arr[:,:,3] < 16).sum() / (h * w) * 100

# 2. RGB 디테일 (visible 영역만)
rgb_std = visible[:,:3].std(axis=0).mean() if len(visible) else 0

# 3. 4 사분면 alpha — grid 패턴 검출
quads = []
for sl in [(slice(None,h//2), slice(None,w//2)),
           (slice(None,h//2), slice(w//2,None)),
           (slice(h//2,None), slice(None,w//2)),
           (slice(h//2,None), slice(w//2,None))]:
    v = (arr[sl][:,:,3] >= 128).sum() / arr[sl].shape[0] / arr[sl].shape[1] * 100
    quads.append(v)

# 판정 기준
PASS_transparent = 30 <= transparent <= 70   # healthy sticker 비율
PASS_rgb_detail = rgb_std >= 25              # 디테일 임계 (단순 캐릭터는 20+ 허용)
PASS_no_grid = max(quads) - min(quads) >= 15 # 4 사분면 차이 >= 15%pt 면 단일 캐릭터
```

### 합격 기준

- transparent ∈ [30%, 70%] (healthy, 단순 캐릭터는 75%까지 허용)
- visible RGB std ≥ 25 (단순 paper sprite 등은 20+ 허용)
- 4 사분면 max-min ≥ 15%pt (grid 면 모두 균등 → 0~10%pt)

---

## 7. 흔한 실패 패턴 + 자동 retry

| 증상 | 진단 | 해결 |
|---|---|---|
| **Silhouette만 보임** (visible RGB std < 20) | LayerDiffuse alpha inversion 또는 Turbo 호환성 | 서버측 layered_diffusion.py 패치 확인 + SDXL base 사용 |
| **4-character grid** (4 사분면 균등 30~50%) | negative prompt 부족 | §4 negative 전문 적용 + 시드 +1 retry |
| **transparent 0%** | LayerDiffuse 미가동 (fallback 사용 중) | `--workflow workflow_layerdiffuse.json` 명시 |
| **transparent 95%+** | 캐릭터 거의 없음 (prompt 너무 추상적) | 캐릭터 디테일 강화 (실물 명사 추가) |
| **카테고리 톤 누락** | 시드 + prefix 누락 | §3 카테고리 prefix + 시드 강제 |
| **VRAM OOM** | ACE-Step 등 동시 실행 | `bgm_stop.sh` 또는 ComfyUI `/free` |

### Auto-retry 스크립트 (선택)

생성 후 §6 검증 → grid 또는 silhouette 감지 시 자동 retry (max 3회, 시드 +1):

```python
def generate_with_retry(prompt, negative, seed, output, max_retry=3):
    for attempt in range(max_retry):
        s = seed + attempt
        run_generate(workflow="workflow_layerdiffuse.json",
                     prompt=prompt, negative=negative, seed=s,
                     steps=20, cfg=7.0, output=output)
        ok, reason = verify(output)  # §6 메트릭
        if ok:
            return s
        print(f"retry {attempt+1}: {reason}")
    raise RuntimeError(f"max retry 소진. 마지막 시도 seed={s}")
```

---

## 9. LoRA 모드 (5014 — V5 sticker LoRA, 선택)

5014 사이클에서 학습된 V5 sticker LoRA 사용 시 prompt 단순화 가능. **선택 사용 — 5008 baseline 이 production 우선**.

### 9.1 모델 + 워크플로

| 항목 | 값 |
|---|---|
| LoRA 파일 | `~/ai-tools/ComfyUI/models/loras/v5_sticker_v1.safetensors` (218MB) |
| 학습 데이터 | 262 PNG (V2 archetype 30 + chapter override 210 + V1 11 + Boss 11) |
| 학습 step | 3000 (~50분 RTX 5070) |
| 권장 weight | **0.7** (model + clip 동일, over-fit 방지) |
| 워크플로 | `assets/workflow_layerdiffuse_lora.json` |
| 백업 | `~/lora_training/v5_sticker/output/v5_sticker_v3_3000step.safetensors` |

### 9.2 Prompt 단순화 (LoRA 모드)

기존 5008 baseline (23+ 토큰) → LoRA 모드 (5~10 토큰) 로 가능:

```
# 5008 baseline (full prompt)
"cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting, soft cartoon shading,
chunky thick white outline, three-quarter view, single character centered in frame,
fully transparent background, isolated subject, solo, 1character, alone,
{카테고리 prefix}, {archetype form}"

# LoRA 모드 단순 prompt
"{카테고리 + archetype 핵심}, {색상 키워드}"
# 예: "tiny mushroom child, warm beige tones"
```

### 9.3 LoRA 효과 한계 (5014 검증)

| 형태 | 효과 |
|---|---|
| **Boss / Medium** | ✅ healthy 4/5 (학습 데이터의 큰 캐릭터 형태 답습) |
| **Small (tiny / kitten / fairy 등)** | ⚠️ frame 작음 (학습 sample 자체가 small frame) |
| Fantasy lavender | △ 학습 데이터 풍부했음에도 약함 |

→ LoRA 는 **선택 가속 도구** (Boss 형태에 적합), production 자산은 **5008 baseline 우선**.

### 9.4 LoRA 사용 호출 패턴

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_layerdiffuse_lora.json \
  --prompt "{단순 prompt}" \
  --negative "{NEG 표준}" \
  --seed {시드} \
  --width 768 --height 1024 --steps 25 --cfg 6.0 \
  --output {경로}
```

### 9.5 Small 형태 frame 강화 (LoRA + prompt 추가)

Small 형태 사용 시 frame 작음 회피:

```
"{Small archetype}, large character filling frame, prominently centered"
```

### 9.6 LoRA weight 조정

- **0.7** (default) — V5 톤 + 5008 baseline 균형
- **0.85** — V5 톤 강조 (5014 검증 결과 0.7 와 동등)
- **1.0** — over-fit 위험 (학습 데이터 답습 ↑↑)

## 10. 변경 이력

- 2026-05-04: v1 초기 작성. 5008 T05.5 fix 사이클 (CH09_Medium / CH09_MiniB 성공 + CH22_Small 실패 → 분석 → 재생성) 의 검증된 조합 정착.
- 2026-05-05: v2 (5014) — LoRA 모드 가이드 §9 추가. v5_sticker_v1.safetensors (3000 step, 218MB) 학습 후 prompt 단순화 옵션. **5008 baseline 이 production 우선** — LoRA 는 선택 가속.
