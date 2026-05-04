# 5009 — Chapter Overrides 210 PNG 점진 재생성 v1 (디자인)

> **목적**: `Generated_v2/Enemies/Chapters/CH##_*.png` 210 sprite 를 5008 검증 baseline (sprite_generation_rules.md) 으로 점진 재생성.
> **우선순위**: 사용자 시각 회귀 보고 시 즉시 / V2 archetype 폴백 정상이면 5009 보류 가능.

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5008.1 → 재부여 5009)
- 2026-05-04: 5008 D3=A+B 정책 — skip list (5008 사이클의 0건) 우선 처리

## 1. 현재 시스템 분석

### 1.1 Chapter Override 분포

| 위치 | 개수 | 출처 |
|---|---:|---|
| `Generated_v2/Enemies/Chapters/CH{01~35}_{Small,Medium,Large,MiniA,MiniB,Boss}.png` | **210** | 35 챕터 × 6 archetype |
| 손상 식별 (5008 Auditor) | 3 (수정됨, T05.5 흡수) | CH09×2 + CH22×1 |
| 잔여 healthy | 207 | 주의: 5008 baseline 미적용 (구식 톤) |

### 1.2 V2 시스템과의 관계

- PhaseFContentSeed 가 `EnemyData.icon = null` 로 자동 wiring 안 함 → 챕터별 V2 시스템은 archetype (`Enemy_<Cat>_<Arch>.png`) 직접 사용
- Chapter override 는 **V1 호환성 + 챕터별 시각적 개성** 용 (현재 미사용)
- 5008 EnemySpriteAuditor V2 fallback 검증 결과 = **OK** (이미 통과)

### 1.3 우선순위 재검토

**보류 가능 사유**:
- V2 시스템은 archetype 만 사용 — chapter override 미사용
- 사용자 보고 회귀 발생 시 5008 baseline 으로 즉시 재생성 가능 (배치 단위)
- 5012/5013/5014/5015 (코드 영역) 가 더 게임플레이 영향 ↑

**즉시 진행 사유**:
- chapter 별 시각적 차별성 강화 (style guide v2 의 통일성 ↑)
- 향후 PhaseFContentSeed 변경 시 (chapter override 활용) 미리 준비

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 전체 210 PNG 일괄 재생성 | 완전성 ✓ | 매우 큰 작업 (~3시간 + review) |
| **B (★권장)** | **카테고리별 batch (5 카테고리 × 42 PNG)** | 점진 / 사용자 review 단위 안정 | 5 commit |
| C | 사용자 보고 시에만 즉시 처리 (보류) | ROI 최고, 부담 X | chapter 시각 통일성 지연 |
| D | Chapter 별 batch (35 챕터 × 6 PNG) | 챕터별 통일성 ↑ | 35 commit, 너무 많은 review |

> **권장 B** — 카테고리별 안정 + 5008 baseline 그대로 적용. 5 카테고리 × 42 = 210 PNG.

## 3. 권장 설계

### 3.1 sprite_generation_rules.md baseline 재사용

5008 검증된 설정 그대로:
- SDXL base 1.0, 768×1024 portrait, cfg 6, steps 25, euler/normal
- Negative 17종 grid + 6종 인물 수
- 카테고리 시드 (22222/33333/44444/55555/77777)

### 3.2 챕터별 archetype 매핑

`PhaseFContentSeed.cs` 의 `SelectPoolForChapter()` 매핑:
- CH 1~5 = Meta (시드 22222 base)
- CH 6~14 = Nature (33333)
- CH 15~20 = Animal (44444)
- CH 21~27 = Civ (55555)
- CH 28~35 = Fantasy (77777)

각 챕터의 6 archetype = 카테고리 그대로 (Small/Medium/Large/MiniA/MiniB/Boss). 시드 = 카테고리 시드 + 챕터 offset (예 CH 1 시드 22222 / CH 2 시드 22228 / ... CH 5 시드 22252).

### 3.3 Auditor 사후 검증 (5008 자동 도구 재활용)

각 batch 후 `Tools > LittleHero > Audit Enemy Sprite Quality` 실행 → 손상 0 확인.

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | Meta 5 챕터 × 6 = 30 PNG (CH 1~5) | 1 |
| 2 | Nature 9 챕터 × 6 = 54 PNG (CH 6~14) | 1 |
| 3 | Animal 6 챕터 × 6 = 36 PNG (CH 15~20) | 1 |
| 4 | Civ 7 챕터 × 6 = 42 PNG (CH 21~27) | 1 |
| 5 | Fantasy 8 챕터 × 6 = 48 PNG (CH 28~35) | 1 |
| 6 | Auditor 사후 검증 + docs (changelog + CLAUDE.md) | 1 |
| **총** | **210 PNG** | **6** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 사용자 review 부담 (5 batch × 30~50 PNG) | 5008 baseline 안정성 ↑ — review 시간 단축 (~10분/batch) |
| Multi-character grid 발생 | sprite_generation_rules §7 auto-retry (시드 +1 max 3회) |
| 챕터별 시드 충돌 | 카테고리 시드 + 챕터 offset (예 22222 + chapter*6) |

## 6. Definition of Done

- [ ] 210 PNG 모두 5008 baseline 으로 재생성
- [ ] Auditor 재실행 시 손상 0
- [ ] 사용자 시각 review 5 batch 모두 PASS

## 7. 변경 이력

- 2026-05-04: v1 작성 (5008 후속 anchor 5009).
