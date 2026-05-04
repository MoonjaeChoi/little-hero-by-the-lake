# T01: Meta CH 1~5 30 PNG

> **레이어**: Asset
> **의존성**: 없음
> **예상 시간**: 30~45분 + review
> **PRD 출처**: 5009_chapter_overrides_v1_v2.md §2 Sprint 1

## 1. 작업 항목

### 1.1 대상 파일

`LittleHero/Assets/_Project/Art/Generated_v2/Enemies/Chapters/CH{01,02,03,04,05}_{Small,Medium,Large,MiniA,MiniB,Boss}.png`

총 5 챕터 × 6 archetype = **30 PNG** 재생성.

### 1.2 시드 매핑 (Rebuttal accepted)

| Chapter | 시드 base | (CH-1)*6 | 최종 시드 base |
|---|---:|---:|---:|
| CH 1 | 22222 | 0 | 22222 |
| CH 2 | 22222 | 6 | 22228 |
| CH 3 | 22222 | 12 | 22234 |
| CH 4 | 22222 | 18 | 22240 |
| CH 5 | 22222 | 24 | 22246 |

각 챕터 내 6 archetype 시드 = base + 0~5 (Small=base / Medium=base+1 / ... / Boss=base+5).

### 1.3 Prompt 매핑 (5008 sprite_generation_rules §3.2 baseline)

각 챕터 6 archetype 별 prompt:

#### Meta 카테고리 prefix
```
friendly village character, warm beige and brown tones, soft cozy folk atmosphere
```

#### Archetype 별
| Archetype | Form | Negative |
|---|---|---|
| Small | `friendly mushroom child, tiny, beige and white spotted cap, oversized cute eyes, simple body` | sprite_generation_rules §5 17 grid keywords + multi-character |
| Medium | `friendly village kid, brown overalls, soft round cheeks, cheerful smile, single character` | 동일 |
| Large | `friendly burly village man, gentle giant, soft beard, warm brown apron, kind eyes` | 동일 |
| MiniA | `friendly village guardian, simple shield and lantern, brown leather, kind protective expression` | 동일 |
| MiniB | `friendly village elder, wise expression, soft white beard, beige robe, gentle posture` | 동일 |
| Boss | `friendly village patriarch, large warm presence, golden brown attire, fatherly aura, large stature` | 동일 |

(5008 검증된 prompt — 사용자 "아주 만족" 승인 baseline)

### 1.4 ComfyUI workflow

`comfyui-image` skill workflow_layerdiffuse.json:
- model: sd_xl_base_1.0.safetensors
- size: 768×1024 portrait
- cfg: 6.0
- steps: 25
- sampler: euler / normal
- LayerDiffuse RGBA (transparent background)

### 1.5 Unity import

각 PNG 생성 후:
- TextureImporter Sprite (Single)
- Pixels Per Unit: 100
- Read/Write Enabled: false (5008 D1=C 메뉴로 후처리 가능)
- AssetDatabase.SaveAssets

EnemyData SO icon wiring 은 변경 X — 기존 chapter override path 유지.

## 2. 검증 기준

### 2.1 sprite_generation_rules §3.6 metric
- Transparent ratio: 30~80%
- Asymmetry: ≤ 50
- 카테고리 톤 일관 (warm beige/brown)
- single character (multi grid 발생 시 시드 +1 max 3 retry)

### 2.2 사용자 review
- batch 완료 후 30 PNG 시각 검토
- 톤 일관 + 챕터별 약간의 변화 확인

## 3. commit 메시지

```
5009 Sprint 1 / T01 — Meta CH 1~5 chapter override 30 PNG (1/6)

[변경]
- Generated_v2/Enemies/Chapters/CH{01~05}_{Small,Medium,Large,MiniA,MiniB,Boss}.png × 30
- 시드 22222 + (CH-1)*6 (CH 1=22222 / CH 5=22246)
- 5008 sprite_generation_rules baseline 재사용 (sd_xl_base + 768×1024 + cfg 6 + LayerDiffuse RGBA)
- prompt: friendly village character 6 archetype × 5 chapters

[검증]
- 30 PNG transparent 30~80%, asym ≤ 50
- 톤 일관 (warm beige/brown)
- 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- ComfyUI 서버 GPU OOM — auto-retry max 3
- 카테고리 톤 inconsistency — 사용자 review 후 시드 변경 가능
- multi-character grid — sprite_generation_rules §7 auto-retry
