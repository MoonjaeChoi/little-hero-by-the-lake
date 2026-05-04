# 5008 — Chapter Enemy Asset Quality v1 (챕터 적 자산 품질 v1)

> **목적**: 챕터에서 출현하는 적 캐릭터의 시각 자산을 **마음에 드는 수준**으로 끌어올리기. LayerDiffuse 도입 + V5 sticker tone 일관성 + 사용자 검토 반복.
> **범위 축소** (2026-05-04 사용자 결정): 본 PRD 는 **자산 품질** 만 — 성능 (off-screen culling) / 다양성 (MPB hue shift) / 자동화 (LoRA / IPAdapter) 는 별도 후속 사이클 (5008.x / 5009+).
> **연계**: 5007 cinematic effect / comfyui-image 스킬 / Generated_v2/Enemies 251 PNG / 137-262 EnemyData SO

## 0. 결정 이력

- 2026-05-04: 사용자 발화 "PRD 5008 작성" → 첫 디자인 (전체 파이프라인 7 commit)
- 2026-05-04: 사용자 결정 "chapter 캐릭터 마음에 드는 수준 자산 우선, PRD 범위 한정" → 본 디자인 자산 품질로 좁힘

## 1. 현재 시스템 분석

### 1.1 적 자산 분포 (Generated_v2/Enemies)

| 분류 | 위치 | 개수 | 출처 |
|---|---|---|---|
| 30 archetype 베이스 | `Enemy_{Meta,Nature,Animal,Civ,Fantasy}_{Small,Medium,Large,MiniA,MiniB,Boss}.png` | 30 | PhaseFContentSeed |
| Chapter 별 sprite | `Chapters/CH{01~35}_{Small,Medium,Large,MiniA,MiniB,Boss}.png` | 210 | 초기 sprite (chapter 고유) |
| V1 동화 적 | `V1/Enemy_{Basic,BeanGiant,BigWolf,Bear,...}.png` | 11 | V1 Chapter_01_RedHood 등 |
| Daily 보스 | `DailyBoss_{00~06}.png` | 7 | F4 endgame |
| Weekly 보스 | `WeeklyBoss_{00~03}.png` | 4 | F5 endgame |
| **누적** | | **262** | |

### 1.2 자산 품질 현황 (5008 직전 사용자 보고 + 후처리 결과)

| 분류 | 정상 | 처리 완료 | 손상 (재생성 필요) |
|---|---:|---:|---:|
| 30 archetype | 28 | 0 | 2 (Enemy_Nature_Medium, V1/Enemy_StarCloud) |
| Chapter overrides | 196 | 14 (flood-fill) | **9** (CH14/CH22/CH25/CH30/CH33×2/CH34) |
| V1 11종 | 11 | 0 | 0 |
| Daily / Weekly | 11 | 0 | 0 |

> **현재 사용자 우선 관심** = chapter 적 자산 (`Chapters/` 폴더 + 30 archetype) — 1라운드 ~ CH 35 진행 중 출현하는 sprite.

### 1.3 품질 기준 (마음에 드는 수준)

| 기준 | 측정 | 통과 조건 |
|---|---|---|
| **Alpha 정확도** | Pillow transparent ratio | 30~70% (이전 0% / 89% 같은 이상치 X) |
| **V5 sticker tone 일관성** | 시각 — outline + 톤 통일 | 두꺼운 흰 outline + warm color + cartoon shading |
| **PG-12 톤** | 시각 + prompt | 잔인·피·끔찍 X, 만화풍 OK |
| **단일 객체** | 시각 | 1 캐릭터, grid pattern X (5006.1 chibi grid 이슈 회피) |
| **카테고리 일관성** | tint | 같은 카테고리 (Meta/Nature/...) 적은 톤 통일 |
| **사용자 만족** | review | 사용자 직접 검토 + "OK" 응답 |

## 2. 옵션 비교

### 2.1 D1 — 우선순위 자산 그룹

| 옵션 | 설명 | 우선 sprite |
|---|---|---|
| A | 손상 9 PNG 만 재생성 | 9 |
| **B (★권장)** | **30 archetype 베이스만 — 모든 챕터 영향** (PhaseFContentSeed 기반 V2 시스템) | 30 |
| C | 30 archetype + 35 chapter boss | 65 |
| D | 30 archetype + 210 chapter overrides + 11 V1 = 251 | 251 |

> 권장 **B** — V2 시스템에서 30 archetype 가 모든 챕터에서 재사용되므로 ROI 최고. 사용자 만족 시 B → C → D 점진 확장.

### 2.2 D2 — LayerDiffuse 통합

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **comfyui-image 스킬에 LayerDiffuse 워크플로 추가** + 사용자가 huchenlei/ComfyUI-layerdiffuse 노드 설치 |
| B | 별도 스킬 신규 |
| C | 외부 사용자 작업 (스킬 변경 X) |

### 2.3 D3 — Sticker tone 일관성 보장

| 옵션 | 설명 |
|---|---|
| A | 단일 prompt template (현재 V5 base) — 일관 | 변형 부족 |
| **B (★권장)** | **카테고리별 prompt 변형** (Meta / Nature / Animal / Civ / Fantasy 각 톤 prefix) — 일관 + 카테고리 식별성 ✅ |
| C | LoRA 훈련 (5009+ 후속) | 본 PRD 범위 외 |

### 2.4 D4 — 사용자 검토 사이클

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **카테고리 단위 batch (6 sprite) → 사용자 review → 만족 시 다음 카테고리** | 5 사이클 (5 카테고리) |
| B | 30 sprite 일괄 → 일괄 review | 1 사이클 (큰 부담) |
| C | sprite 단위 (1개씩) → review | 30 사이클 (지나치게 자주) |

### 2.5 D5 — 손상 9 PNG 처리

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **30 archetype 재생성 시 함께 다룸** (Enemy_Nature_Medium 1개), 나머지 8 chapter override 는 D1 = D 확장 시 |
| B | 5008.0 후속 (별도 사이클) |
| C | 즉시 재생성 (Sprint 1) |

### 2.6 D6 — 자동 alpha 검증

| 옵션 | 설명 |
|---|---|
| **A (★권장)** | **Editor 메뉴 `Tools > LittleHero > Audit Enemy Sprite Quality`** — 모든 PNG transparent ratio + 카테고리 검증 + report 생성 |
| B | NUnit Reflection 만 |
| C | 수동 검증 (자동화 X) |

## 3. 권장 설계

### 3.1 신규 파일 (3개)

| 파일 | 책임 | 라인 |
|---|---|---|
| `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json` | SDXL native transparency 워크플로 | (JSON) |
| `Scripts/Editor/EnemySpriteAuditor.cs` | Tools 메뉴 — alpha + V5 톤 자동 검증 + report | ~80 |
| `docs/little_hero/enemy_sprite_prompts_v1.md` | 5 카테고리별 prompt template + 30 archetype 매핑 | (md) |

### 3.2 수정 파일

| 파일 | 변경 |
|---|---|
| `.claude/skills/comfyui-image/SKILL.md` | LayerDiffuse 워크플로 사용 가이드 + 카테고리 prompt 매핑 |
| `.claude/skills/comfyui-image/references/comfyui_api.md` | LayerDiffuse 노드 설치·사용 명세 |

### 3.3 카테고리별 prompt template (D3=B)

```
공통 베이스:
"cute cartoon sticker illustration, V5 dynamic tone, warm cozy lighting,
soft cartoon shading, chunky thick white outline, isometric playful angle,
1:1 square composition, single subject centered, fully transparent background,
isolated subject, single character only, inanimate clothing"

카테고리 prefix:
- Meta (CH 1~5)     → "village friendly creature, warm beige and brown tones"
- Nature (CH 6~14)  → "forest nature spirit, green and earth tones, leafy"
- Animal (CH 15~20) → "cute domestic animal companion, warm orange and white tones"
- Civ (CH 21~27)    → "urban paper or cardboard creature, soft pink and pastel"
- Fantasy (CH 28~35)→ "magical mystical being, purple and indigo, sparkles"
```

각 카테고리 × 6 archetype (Small/Medium/Large/MiniA/MiniB/Boss) 명시:

```
Meta-Small:  "small mushroom child"
Meta-Medium: "village kid with backpack"
Meta-Large:  "burly village man"
Meta-MiniA:  "village guardian"
Meta-MiniB:  "elder village leader"
Meta-Boss:   "ancient village patriarch"

(Nature/Animal/Civ/Fantasy 동일 구조 — enemy_sprite_prompts_v1.md 에 명시)
```

### 3.4 §15 OOP 정합

| 정책 | 검증 |
|---|---|
| §15.4 SO 우선 | prompt 템플릿 = MD 문서 (코드 hardcode X) ✅ |
| §15.6 100L | EnemySpriteAuditor ~80L ✅ |
| §15.9 Test | (선택) NUnit AuditorTests 가능 |
| §15.12 Validator | EnemySpriteAuditor 가 Validator 룰 역할 |

## 4. Sprint 분해 (개요)

| Sprint | 작업 | 의존성 |
|---|---|---|
| 1 | comfyui-image LayerDiffuse 워크플로 + 카테고리 prompt 가이드 | — |
| 2 | EnemySpriteAuditor Editor 메뉴 + 현재 262 PNG 품질 report | — |
| 3 | Meta 6 archetype 재생성 (CH 1~5 영향) → 사용자 review | 1, 2 |
| 4 | Nature 6 + Animal 6 archetype 재생성 (CH 6~20) → review | 3 PASS |
| 5 | Civ 6 + Fantasy 6 archetype 재생성 (CH 21~35) → review | 4 PASS |
| 6 | docs (changelog + CLAUDE.md §11/§12/§14) | 5 PASS |

> **Sprint 3~5 의 review 단계**: 사용자가 "마음에 든다" 응답 후 다음 카테고리 진입. 불만족 시 prompt 조정 + 재생성 (max 3 iteration).

## 5. 알려진 위험 + 완화

| 위험 | Sprint | 완화 |
|---|---|---|
| LayerDiffuse 노드 설치 사용자 책임 | 1 | SKILL.md 가이드 + URL 명시 (huchenlei/ComfyUI-layerdiffuse) |
| 30 archetype 재생성 시 카테고리 톤 깨짐 | 3~5 | 카테고리별 prompt prefix + 사용자 review 사이클 |
| 사용자 review 사이클 길어짐 | 3~5 | Sprint 단위 review (한 번에 6 sprite), max 3 iteration cap |
| chapter 별 sprite (210 PNG) 미처리 | 본 PRD 외 | 30 archetype 만 우선, Chapter 별 sprite 는 D = 5009 후속 |
| 손상 9 PNG 중 30 archetype 외 8 PNG 미처리 | 본 PRD 외 | Enemy_Nature_Medium 1 개만 본 PRD 범위, 나머지 8 = 5009 |
| LayerDiffuse 가 sticker outline 파괴 | 1 | prompt 에 "chunky thick white outline" 강조 + 사용자 review |
| AI 생성 캐릭터 일관성 부족 | 3~5 | 카테고리별 시드 고정 (Meta=22222, Nature=33333, ...) — 카테고리 통일 |

## 6. Definition of Done

- [ ] comfyui-image LayerDiffuse 워크플로 추가 + SKILL.md 가이드
- [ ] EnemySpriteAuditor Editor 메뉴 + 262 PNG 품질 report
- [ ] enemy_sprite_prompts_v1.md — 5 카테고리 × 6 archetype prompt template (30 매핑)
- [ ] 30 archetype sprite 재생성 (사용자 review PASS)
- [ ] Unity 자동 임포트 + 256 PNG 정상 표시 (Play Mode)
- [ ] CLAUDE.md §11/§12/§14 갱신 + changelog
- [ ] 후속 anchor 명시 (5009: chapter overrides 210 / 5010: V1 11종 / 5011: 보스 11종)

## 7. 본 PRD 범위 외 (후속 사이클 anchor)

| 후속 PRD | 작업 | 우선순위 |
|---|---|---|
| **5009** | Chapter 별 210 sprite (`Chapters/CH##_*.png`) 점진 재생성 | 보통 |
| **5010** | V1 11 동화 적 재생성 (Chapter_01~03 RedHood/Beanstalk/Alice) | 낮음 (legacy) |
| **5011** | Daily 7 + Weekly 4 보스 재생성 | 보통 (endgame 시) |
| 5012 | MPB hue shift 다양성 시스템 (30 → 150 변형) | 본 PRD 와 별도 |
| 5013 | EnemyVisibilityCuller (off-screen culling) | 성능 영역 |
| 5014 | V5 sticker LoRA 훈련 (Kohya_ss) | 자동화 영역 |
| 5015 | LitMotion 도입 (DOTween 5×) | 애니메이션 영역 |

## 8. 변경 이력

- 2026-05-04: 초기 작성 (전체 파이프라인 7 commit, MPB+culling+LayerDiffuse)
- 2026-05-04: **사용자 결정 — 범위 축소 (자산 품질 한정)**. 6 commit / 6 카테고리 review 사이클. MPB / culling / LoRA → 5012~5014 후속.

## 9. 출처

- Unity 적 캐릭터 자산 최신 기법 심층 조사 (2026-05-04, 18 references)
- ComfyUI-layerdiffuse (huchenlei, 2024)
- 5006.1 ComfyUI batch 결과 (Attic 17 PNG, 4 그룹 색 통일 패턴)
- 사용자 1차 보고 (1라운드 적 흰 박스) + 14 PNG flood-fill + 9 PNG 재생성 commit log
- PhaseFContentSeed (5 카테고리 × 6 archetype = 30 base)
