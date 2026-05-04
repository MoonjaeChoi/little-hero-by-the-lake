# Enemy Sprite Prompts v1 — 5 카테고리 × 6 archetype = 30 매핑

> **출처**: 5008 chapter_enemy_asset_quality_v1 PRD + 디자인 §3.3
> **사용**: comfyui-image 스킬 + LayerDiffuse 워크플로 (또는 fallback)
> **시드 고정**: Meta=22222 / Nature=33333 / Animal=44444 / Civ=55555 / Fantasy=66666 (카테고리 통일)

## 1. 공통 베이스 prompt

모든 카테고리·archetype 에 공통으로 prepend:

```
cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting,
soft cartoon shading, chunky thick white outline, isometric playful angle,
1:1 square composition, single subject centered, fully transparent background,
isolated subject, single character only, inanimate clothing
```

## 2. 공통 negative prompt

```
low quality, blurry, watermark, photorealistic, dark, scary, blood, gore,
text, multiple subjects, grid, frame, border, logo, signature
```

## 3. 카테고리별 prefix + archetype 매핑

### 3.1 Meta (CH 1~5) — 시드 22222

**카테고리 prefix**: `village friendly creature, warm beige and brown tones, cozy meadow vibes`

| archetype | 캐릭터 prompt |
|---|---|
| Small | `small mushroom child with leaf hat, tiny round body` |
| Medium | `village kid with backpack, friendly waving pose` |
| Large | `burly village man with apron, cheerful expression` |
| MiniA | `village guardian with wooden staff, watchful stance` |
| MiniB | `elder village leader with wise smile, holding lantern` |
| Boss | `ancient village patriarch in ceremonial robes, hands clasped` |

### 3.2 Nature (CH 6~14) — 시드 33333

**카테고리 prefix**: `forest nature spirit, green and earth tones, leafy moss texture, woodland atmosphere`

| archetype | 캐릭터 prompt |
|---|---|
| Small | `tiny moss sprite with twig arms, dewdrop eyes` |
| Medium | `acorn cap forest faerie, green wings folded` |
| Large | `bramble guardian made of vines and leaves, tall stance` |
| MiniA | `forest watcher with antler crown, mushroom shoulders` |
| MiniB | `ancient root spirit with mossy beard, glowing inner light` |
| Boss | `forest king with massive antlers, layered bark armor` |

### 3.3 Animal (CH 15~20) — 시드 44444

**카테고리 prefix**: `cute domestic animal companion, warm orange and white tones, fluffy fur, friendly gaze`

| archetype | 캐릭터 prompt |
|---|---|
| Small | `chubby kitten with bell collar, sitting pose` |
| Medium | `floppy-ear puppy with bandana, tail wagging` |
| Large | `golden retriever in wool sweater, alert ears` |
| MiniA | `wise tabby cat with monocle, regal posture` |
| MiniB | `loyal sheepdog with herding stick, focused look` |
| Boss | `majestic lion in storybook armor, mane like flames` |

### 3.4 Civ (CH 21~27) — 시드 55555

**카테고리 prefix**: `urban paper or cardboard creature, soft pink and pastel tones, papercraft texture, gentle origami folds`

| archetype | 캐릭터 prompt |
|---|---|
| Small | `paper crane with tiny wings, pastel pink folds` |
| Medium | `cardboard robot child with antenna, rosy cheeks` |
| Large | `origami giant with layered paper armor, calm pose` |
| MiniA | `paper knight with folded sword, banner fluttering` |
| MiniB | `cardboard wizard with glasses, paper hat tall` |
| Boss | `pastel mech construct with intricate origami panels, glowing core` |

### 3.5 Fantasy (CH 28~35) — 시드 66666

**카테고리 prefix**: `magical mystical being, purple and indigo tones, sparkles and stardust, ethereal glow`

| archetype | 캐릭터 prompt |
|---|---|
| Small | `tiny pixie with star wand, sparkle trail` |
| Medium | `young apprentice mage with glowing tome, robe flowing` |
| Large | `crystal golem with violet gem heart, geometric form` |
| MiniA | `moon dancer with silver veil, crescent staff` |
| MiniB | `astral seer with floating orbs, third eye glowing` |
| Boss | `cosmic archon with galaxy cape, star crown radiant` |

## 4. 사용 예 (1 sprite 생성)

### 4.1 LayerDiffuse 워크플로 (권장)

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json \
  --prompt "cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting, soft cartoon shading, chunky thick white outline, isometric playful angle, 1:1 square composition, single subject centered, fully transparent background, isolated subject, single character only, inanimate clothing, village friendly creature, warm beige and brown tones, cozy meadow vibes, small mushroom child with leaf hat, tiny round body" \
  --negative "low quality, blurry, watermark, photorealistic, dark, scary, blood, gore, text, multiple subjects, grid, frame, border, logo, signature" \
  --seed 22222 \
  --width 1024 --height 1024 \
  --output LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Enemy_Meta_Small.png
```

### 4.2 Fallback (LayerDiffuse 노드 미설치 시)

```bash
python3 .claude/skills/comfyui-image/scripts/generate.py \
  --workflow .claude/skills/comfyui-image/assets/workflow_template.json \
  --prompt "..." \
  --negative "..." \
  --seed 22222 \
  --output LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Enemy_Meta_Small.png
```

> **fallback 한계** (Rebuttal accepted): DreamShaperXL Turbo 는 native alpha 출력 X. dark BG + flood-fill 후처리 정확도 ~80%. LayerDiffuse 권장.

## 5. 30 sprite batch 생성 패턴

Sprint 3~5 에서 카테고리별 6 sprite batch 단위로 생성. 시드 고정으로 카테고리 톤 통일:

```bash
# Sprint 3: Meta 6 sprite (시드 22222)
for archetype in Small Medium Large MiniA MiniB Boss; do
  python3 .claude/skills/comfyui-image/scripts/generate.py \
    --workflow .claude/skills/comfyui-image/assets/workflow_layerdiffuse.json \
    --prompt "<공통 베이스> + <Meta prefix> + <archetype 캐릭터>" \
    --negative "<공통 negative>" \
    --seed 22222 \
    --output LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Enemy_Meta_${archetype}.png
done
```

Sprint 4 (Nature/Animal), Sprint 5 (Civ/Fantasy) 동일 패턴, 카테고리별 시드 변경.

## 6. 사용자 review 사이클

각 카테고리 batch 생성 후:
- **OK** → 다음 카테고리 진입 (또는 commit)
- **재시도** → prompt 미세 조정 + 시드 동일 또는 변경 → 재생성 (max 3 iteration)
- **iter 3 후 PASS X** → 사용자 결정 (다른 시드 / prompt 대폭 수정 / **skip**)
- **skip 시 (D3=A+B)**: T06 docs skip list 추가 + 5009 anchor 우선 처리 표시

## 7. 변경 이력

- 2026-05-04: v1 초기 작성 (5008 PRD §3.3 + decisions D3=A+B 정책 통합).
