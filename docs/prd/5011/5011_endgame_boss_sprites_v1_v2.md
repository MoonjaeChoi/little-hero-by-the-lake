# 5011 — Endgame Boss Sprites v2 (최종, Mediator 확정)

> **PRD v2 (최종)** · D2=A+ (prompt 강화) + D3=A (GUID 보존)
> 사용자 시각 review: 기존 11 sprite "인형 실사판" 톤 → V5 sticker 강조 + 인형/3D 회피 negative 추가

## 1. 위치 (정정)

| 카테고리 | 경로 | 개수 |
|---|---|---:|
| Daily | `Generated_v2/Bosses/DailyBoss_{00~06}.png` | 7 |
| Weekly | `Generated_v2/Bosses/WeeklyBoss_{00~03}.png` | 4 |

(PRD 원안의 `Enemies/` 는 부정확 — 실제 `Bosses/`)

## 2. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Daily 7 (시드 99000~99006, 요일 톤) | 1 | 35~50분 |
| 2 | Weekly 4 (시드 99100~99103, 4 cycle) | 1 | 25~35분 |
| 3 | Auditor + docs | 1 | 15분 |
| **총** | **11 PNG** | **3** | **~1.5h** |

## 3. Prompt v2 (V5 sticker 강화 + 인형 실사판 회피)

### 3.1 공통 base

```
solo, 1character, alone, single subject,
2D flat cartoon sticker illustration, hand-drawn V5 dynamic tone,
chunky thick white outline, soft cartoon shading, vivid bold colors,
three-quarter view, single character centered in frame,
fully transparent background, isolated subject,
epic boss creature, large impressive character, royal pose, friendly powerful aura, no menace
```

### 3.2 Daily 7 요일 prefix

| Index | 파일 | 요일 | prefix |
|---:|---|---|---|
| 00 | DailyBoss_00 | 월 | `friendly Monday morning blue cartoon boss, dawn aurora colors, calm awakening tone` |
| 01 | DailyBoss_01 | 화 | `vibrant Tuesday red cartoon boss, energetic spark, motivated playful pose` |
| 02 | DailyBoss_02 | 수 | `verdant Wednesday green cartoon boss, fresh leafy growth, nature friendly` |
| 03 | DailyBoss_03 | 목 | `royal Thursday purple cartoon boss, mystical wisdom, flowing robe` |
| 04 | DailyBoss_04 | 금 | `warm Friday pink cartoon boss, gentle joyful smile, soft glow` |
| 05 | DailyBoss_05 | 토 | `bright Saturday yellow cartoon boss, sunny round playful` |
| 06 | DailyBoss_06 | 일 | `golden Sunday cartoon boss, peaceful crown, restful smile` |

### 3.3 Weekly 4 cycle prefix

| Index | 파일 | Realm | prefix |
|---:|---|---|---|
| 00 | WeeklyBoss_00 | Light | `Light Realm cartoon boss, radiant white-gold guardian, divine wings, friendly powerful` |
| 01 | WeeklyBoss_01 | Shadow | `Shadow Realm cartoon boss, deep purple sentinel, mysterious cape, no scary` |
| 02 | WeeklyBoss_02 | Life | `Life Realm cartoon boss, vibrant green forest king, leafy antlers branching` |
| 03 | WeeklyBoss_03 | Cosmic | `Cosmic Realm cartoon boss, deep blue starscape lord, galaxy crown` |

### 3.4 Negative (강화)

```
low quality, blurry, watermark, photorealistic, photo, photograph, 3D render, octane render,
plush toy photography, plushie, soft toy, realistic doll, doll photography, toy photo,
3D model, realistic, photographic, dark, scary, blood, gore,
text, logo, signature,
multiple subjects, multiple characters, twins, duplicate, copies,
two characters, three characters, four characters,
grid, frame, border, comic panel, sprite sheet, character sheet,
2x2 grid, 4 panels, multi-panel, collage, mosaic, isometric grid, tile
```

## 4. ComfyUI 설정 (5008 baseline)

- `sd_xl_base_1.0.safetensors` + LayerDiffuse RGBA
- 768×1024 portrait
- cfg 6.0 / steps 25 / sampler euler / scheduler normal

## 5. GUID 보존 (D3=A)

- 같은 PNG 경로 덮어쓰기 (.meta 보존)
- 11 EnemyData SO icon path 변경 X

## 6. 검증

- 11 PNG transparent 30~80%, rgb_std ≥ 30, asym ≤ 50
- Auditor PASS
- 사용자 review — "인형 실사판" 회피 + V5 cartoon 톤
- 요일 7 / cycle 4 차별성

## 7. 변경 이력

- 2026-05-04: v1 작성
- 2026-05-05: v2 (D2=A+ prompt 강화 + D3=A GUID 보존, 위치 Bosses/ 정정)
