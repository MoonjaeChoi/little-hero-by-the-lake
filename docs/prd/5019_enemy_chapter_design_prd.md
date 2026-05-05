# 5019 — 챕터 narrative 정합 적 디자인 패스 (디자인)

> **목적**: 5009 chapter override 210 sprite 가 archetype generic prompt (village kid / lavender witch 등) 로 생성되어 35 챕터 narrative 와 정합 부족.
> 본 PRD 는 35 챕터 × 6 archetype = 210 sprite 를 **챕터 narrative-specific** prompt 로 재생성하여 시각 다양성 + 사용자 인상 ↑.
> **우선순위**: ★★★ (출시 전 시각 품질 핵심 — F1~F3 적 디자인 패스 후속).

## 0. 결정 이력

- 2026-04-30: F1~F3 적 디자인 패스 — 30 archetype → 챕터별 고유 (디자인 작업) 후보
- 2026-05-04: 5008 LayerDiffuse RGBA 30 archetype baseline 생성 + 5009 chapter override 210 (generic prompt 로 hue 변형)
- 2026-05-06: 5019 — chapter narrative-specific prompt 로 210 재생성 PRD (본 문서)

## 1. 현재 시스템 분석

### 1.1 기존 sprite 구조

| 영역 | 개수 | 용도 |
|---|---:|---|
| 30 archetype baseline (Generated_v2/Enemies/) | 30 | 5 카테고리 (Meta/Nature/Animal/Civ/Fantasy) × 6 size |
| **210 chapter override (Generated_v2/Enemies/Chapters/)** | **210** | 35 챕터 × 6 size — **narrative 정합 X** (generic prompt) |
| V1 동화 적 (Generated_v2/Enemies/V1/) | 11 | 5010 baseline |
| 11 endgame boss (Generated_v2/Bosses/) | 11 | 5011 baseline |
| **합계** | **262** | |

### 1.2 35 챕터 narrative 표 (V2)

| 영역 | CH | displayName | 적 narrative 후보 |
|---|---:|---|---|
| **마을 (5)** | 1 | 호숫가 작은 마을 | 마을 강아지 / 호수 물고기 / 작은 새 |
| | 2 | 다락방 깊숙한 곳 | 먼지 정령 / 책 요정 / 거미 친구 |
| | 3 | 잊힌 학교 | 종이비행기 / 분필 인형 / 책가방 정령 |
| | 4 | 마당의 옛 우물 | 두레박 정령 / 우물 개구리 / 이끼 정령 |
| | 5 | 아빠의 비밀 기지 | 종이 비행기 / 캠핑 곰 / 손전등 정령 |
| **자연 (9)** | 6 | 안개 골짜기 | 안개 정령 / 사슴 / 부엉이 |
| | 7 | 깊은 숲 | 다람쥐 / 여우 / 곰 |
| | 8 | 노래하는 폭포 | 물고기 / 새 / 폭포 정령 |
| | 9 | 반딧불 풀밭 | 반딧불 / 풀잎 / 토끼 |
| | 10 | 모래 회오리 사막 | 사막 여우 / 도마뱀 / 사막꽃 |
| | 11 | 얼음 동굴 | 펭귄 / 눈 정령 / 빙하 곰 |
| | 12 | 산호초 바다 | 산호 정령 / 해파리 / 작은 물고기 |
| | 13 | 화산 분화구 | 용암 도롱뇽 / 불꽃 정령 / 까만 새 |
| | 14 | 하늘 구름 다리 | 구름 양 / 하늘 새 / 무지개 정령 |
| **동물 (6)** | 15 | 토끼굴 깊은 곳 | 흰토끼 / 요정 토끼 / 카드 병정 |
| | 16 | 새들의 둥지 도시 | 까치 / 부엉이 / 작은 새 |
| | 17 | 빛나는 해파리 정원 | 해파리 / 발광 물고기 / 산호 |
| | 18 | 고래 노래 바다 | 고래 / 돌고래 / 바다거북 |
| | 19 | 거대 거북이 등 | 거북이 / 도마뱀 / 작은 게 |
| | 20 | 별빛 늑대 산골 | 늑대 / 곰 / 사슴 |
| **문명 (7)** | 21 | 등불 시장 | 등불 정령 / 떡볶이 친구 / 마을 사람 |
| | 22 | 사탕 공장 | 사탕 인형 / 빵 친구 / 캐러멜 |
| | 23 | 인형 공방 | 헝겊 인형 / 나무 인형 / 도자기 인형 |
| | 24 | 우편 비행기 마을 | 종이비행기 / 우편함 정령 / 우체부 |
| | 25 | 두루마리 미로 | 두루마리 / 책 요정 / 잉크 정령 |
| | 26 | 음악 동굴 | 음표 정령 / 작은 새 / 종소리 정령 |
| | 27 | 종이 인형 무대 | 종이 인형 / 무대 마스크 / 풍선 |
| **판타지 (8)** | 28 | 살아있는 정원 | 식물 정령 / 꽃 요정 / 호박 |
| | 29 | 거울 갤러리 | 거울 정령 / 그림자 / 빛 요정 |
| | 30 | 거대한 시계탑 안 | 시계 톱니 / 종소리 정령 / 시간 정령 |
| | 31 | 시간이 멈춘 마을 | 정지 시간 인형 / 멈춘 시간 정령 |
| | 32 | 거꾸로 매달린 지하 도시 | 거꾸로 박쥐 / 동굴 정령 / 광부 |
| | 33 | 달의 뒷면 | 달 토끼 / 달 정령 / 별 |
| | 34 | 노을 들판 | 노을 새 / 해바라기 / 황금 사슴 |
| | 35 | 별과 별 사이 | 별 정령 / 별자리 / 우주 친구 |

→ 5 카테고리 매핑은 5008 archetype 의 Meta(1~5) / Nature(6~14) / Animal(15~20) / Civ(21~27) / Fantasy(28~35) 와 정합. **카테고리는 OK, prompt 만 narrative-specific 갱신 필요**.

### 1.3 5009 prompt 한계 (3 샘플)

```
CH01_Medium: "village friendly creature, warm beige and brown tones, village kid"
  → "호숫가 마을" narrative 정합 ↓ (호수/물고기/물 요소 부재)

CH28_MiniA: "fairy tale character, lavender and pastel purple, young witch"
  → "살아있는 정원" narrative 정합 ↓ (식물 부재, witch 가 무관)

CH35_Boss: "fairy tale character, lavender and pastel purple, wizard king"
  → "별과 별 사이" narrative 정합 ↓ (별/우주 부재, wizard 가 무관)
```

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | **35 챕터 × 6 = 210 sprite narrative-specific 재생성** | 시각 품질 ↑, 사용자 인상 ↑ | 시간 큼 (~3~4h batch + review) |
| B | 핫 챕터만 (CH 1~5 / CH 28~35 = 13 챕터 × 6 = 78 sprite) | 시간 ↓, 핫 영역 정합 ↑ | 중간 챕터 (CH 6~27) generic 잔존 |
| C | 5008 30 archetype 만 다양화 (카테고리 별 6 → 12) — 챕터 매핑 별도 | 60 archetype 대용량 풀 | 챕터 narrative 정합 부족 (배경 변동 X) |
| D | 사용자 시각 review 후 결정 — 사전 sample 5 챕터만 먼저 생성 | 안전, 시각 품질 검증 | 결정 단계 추가 |

> 권장 **A** — 출시 전 시각 품질 핵심 + 5009 batch script 답습 시 자동화.

## 3. 권장 설계

### 3.1 prompt 매핑 표 (5008 baseline 답습 + narrative)

각 sprite 의 prompt:
```
v5 sticker tone, cute cartoon, chunky thick white outline,
soft cartoon shading, single character, transparent background,
{챕터 narrative — 위 표 §1.2 의 적 후보 6 종 중 1},
{챕터 분위기 — 색조 + 환경 hint}
```

예시:
- CH01_Medium: "village dog with brown fur, warm beige and lakeside tones, friendly puppy character"
- CH28_MiniA: "garden plant fairy with green leaves, lavender and emerald tones, plant spirit character"
- CH35_Boss: "starry constellation guardian, deep purple and gold cosmic tones, star spirit character"

### 3.2 35 챕터 × 6 archetype 매핑 (전체)

| 챕터 | Boss | Large | Medium | MiniA | MiniB | Small |
|---:|---|---|---|---|---|---|
| 1 | 호수 거인 | 호수 물고기 | 마을 강아지 | 작은 새 | 호수 개구리 | 반딧불 |
| 2 | 다락방 거인 | 책 요정 | 먼지 정령 | 거미 친구 | 종이 정령 | 작은 쥐 |
| 3 | 종이비행기 거인 | 책가방 정령 | 분필 인형 | 종이비행기 | 잉크 정령 | 지우개 친구 |
| ... (35 챕터 모두 매핑) | | | | | | |

(5019 Sprint 1 에서 35 × 6 = 210 prompt 표 작성)

### 3.3 ComfyUI batch script (5009 답습)

`scripts/5019_chapter_narrative_batch.py`:
- 5 카테고리 batch (Meta CH1~5 / Nature CH6~14 / Animal CH15~20 / Civ CH21~27 / Fantasy CH28~35)
- 시드 namespace: 90000 + (CH × 100) + archetype index
- 768×1024 LayerDiffuse RGBA + V5 sticker tone + Crunched 후속 자동 적용 (5018)

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 35 챕터 × 6 prompt 표 작성 (210 prompt) — narrative 매핑 + 사용자 검토 | 1 | 60~90분 |
| 2 | ComfyUI batch script (`scripts/5019_chapter_narrative_batch.py`) | 1 | 20분 |
| 3 | Sprint 2 batch 실행 (210 sprite, ~70분 ComfyUI) — Meta/Nature/Animal | 1 | 50분 |
| 4 | Sprint 2 batch 계속 — Civ/Fantasy + EnemySpriteAuditor 자동 검증 | 1 | 40분 |
| 5 | 사용자 시각 review + 부분 재생성 (낮은 healthy sprite) + docs | 1 | 60~90분 |
| **총** | **210 sprite + 5 commit** | **5** | **약 3~4시간** |

## 5. CLAUDE.md §15 정합

- §15.4 — Prompt SO 또는 dict 정의 (코드 외부)
- §15.10 — ComfyUI batch script 활용 (외부 자동화)
- §15.12 — EnemySpriteAuditor 자동 검증 (5008 답습)

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 사용자 시각 review 시 일부 sprite 품질 저하 | Sprint 5 부분 재생성 (5011 답습 — 5/11 healthy 후 retry) |
| ComfyUI 시간 큼 (~70분) | 5009 batch 답습 — 자동 진행 |
| narrative 정합 모호한 챕터 (예 CH33 달의 뒷면) | Claude 가 prompt 제시 + 사용자 결정 (Sprint 1) |
| 5008 archetype baseline 의존 | archetype baseline 미수정 (override 만 갱신) — GUID 보존 |
| 5018 Texture compression 자동 적용 | 신규 PNG 도 ASTC 6×6 + Crunched 자동 (TextureCompressionConfigurer 재실행 1회) |

## 7. 변경 이력
- 2026-05-06: 초기 작성 (5009 chapter override generic prompt 한계 발견 후)
