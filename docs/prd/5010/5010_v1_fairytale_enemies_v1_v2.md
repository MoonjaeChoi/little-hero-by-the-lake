# 5010 — V1 Fairytale Enemies v2 (최종)

> Mediator 확정 (D1=A 정정 + D2=B 사이즈 통일 + D3=A prompt 강조)

## 1. 실제 V1 11 PNG 명단

| 파일 | 캐릭터 | 동화 | 시드 | prompt form |
|---|---|---|---:|---|
| Enemy_Basic | Slime | 공통 | 11001 | small slime creature, blob shape, gentle smile, light blue tone |
| Enemy_BeanGiant | Bean Giant | Beanstalk | 11002 | friendly Bean Giant character, towering kind giant, beanstalk armor, no menace |
| Enemy_Bear | Bear | 공통 | 11003 | friendly bear character, big round body, waving paw, brown fur |
| Enemy_BigWolf | Big Wolf | Red Hood | 11004 | friendly Big Bad Wolf character, cartoon wolf, gray fur, no fear no menace |
| Enemy_CardSoldier | Card Soldier | Alice | 11005 | Card Soldier character, playing card knight, friendly pose, red and black uniform |
| Enemy_CloudGiant | Cloud Giant | Beanstalk | 11006 | Cloud Giant character, fluffy white cloud creature, gentle giant, no menace |
| Enemy_Firefly | Firefly | 공통 | 11007 | Firefly character, glowing insect, soft yellow light, friendly |
| Enemy_Pumpkin | Pumpkin | Halloween | 11008 | friendly Pumpkin character, cute jack-o-lantern, no scary, orange round |
| Enemy_QueenHearts | Queen of Hearts | Alice | 11009 | Queen of Hearts character, friendly cartoon queen with heart crown, no menace |
| Enemy_Rabbit | White Rabbit | Alice | 11010 | White Rabbit character, cute rabbit in vest with pocket watch |
| Enemy_StarCloud | Star Cloud | Beanstalk | 11011 | Star Cloud character, fluffy cloud with star eyes, sparkling |

## 2. Prompt (5008 baseline + 5009 단일 character 강조)

### 2.1 BASE
```
cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting,
soft cartoon shading, chunky thick white outline,
three-quarter view, single character centered in frame,
fully transparent background, isolated subject, solo, 1character, alone
```

### 2.2 PREFIX
```
fairy tale storybook illustration, classic children's book character
```

### 2.3 NEG (5009 표준)
```
low quality, blurry, watermark, photorealistic, dark, scary, blood, gore,
text, logo, signature,
multiple subjects, multiple characters, twins, duplicate, copies,
two characters, three characters, four characters,
grid, frame, border, comic panel, sprite sheet, character sheet,
2x2 grid, 4 panels, multi-panel, collage, mosaic, isometric grid, tile
```

## 3. ComfyUI 설정

- workflow_layerdiffuse.json
- 768×1024 portrait (사이즈 통일 — 기존 512×512 → 768×1024)
- cfg 6, steps 25, sampler euler / scheduler normal

## 4. Sprint 분해 (최단 처리 — V1 legacy ROI 낮음)

| Sprint | 작업 | commit |
|---|---|---|
| 1 | V1 11 PNG 일괄 재생성 | 1 |
| 2 | Auditor + docs | 1 |
| **총** | **11 PNG** | **2** |

## 5. GUID 보존 (D3 와 별개 — 5009 패턴)

- 같은 PNG 경로 덮어쓰기 → .meta GUID 보존
- 11 EnemyData SO icon path 변경 X

## 6. 검증

- 11 PNG transparent 30~80% + rgb_std ≥ 30 + 768×1024
- Auditor PASS
- 사용자 시각 review

## 7. 변경 이력

- 2026-05-04: v1 작성
- 2026-05-05: v2 (D1=A 명단 정정 + D2=B 사이즈 통일 + D3=A prompt 강조)
