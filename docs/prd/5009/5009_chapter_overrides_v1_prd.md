# 5009 — Chapter Overrides 210 PNG — Sprint PRD

> **출처**: `docs/prd/5009_chapter_overrides_v1_prd.md` (디자인) — 옵션 B (카테고리별 batch)
> **범위**: 35 챕터 × 6 archetype = 210 PNG, sprite_generation_rules.md baseline 재사용

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Meta 5 챕터 × 6 = 30 PNG (CH 1~5) | 1 | 30~45분 + review |
| 2 | Nature 9 챕터 × 6 = 54 PNG (CH 6~14) | 1 | 50~70분 + review |
| 3 | Animal 6 챕터 × 6 = 36 PNG (CH 15~20) | 1 | 35~50분 + review |
| 4 | Civ 7 챕터 × 6 = 42 PNG (CH 21~27) | 1 | 40~55분 + review |
| 5 | Fantasy 8 챕터 × 6 = 48 PNG (CH 28~35) | 1 | 45~65분 + review |
| 6 | Auditor 검증 + docs | 1 | 20분 |
| **총** | **210 PNG** | **6** | **3.5~5h** |

## 2. Sprint 명세

### Sprint 1 — Meta CH 1~5 (30 PNG)

**대상**: `Generated_v2/Enemies/Chapters/CH{01,02,03,04,05}_{Small,Medium,Large,MiniA,MiniB,Boss}.png`

**프로세스**:
- 카테고리 prefix `village friendly creature, warm beige and brown tones`
- 시드 base 22222 + chapter offset (CH 1 = 22222, CH 2 = 22228, ... CH 5 = 22252)
- 각 archetype 별 prompt = 5008 sprite_generation_rules §3.2 Meta 매핑

**검증 기준**:
- 30 PNG transparent 30~80%, asym ≤ 50, 카테고리 톤 일관
- Auditor 재실행 시 손상 0

#### Sprint 1 commit
```
5009 Sprint 1 / T01 — Meta CH 1~5 chapter override 30 PNG (1/6)

[변경]
- Generated_v2/Enemies/Chapters/CH{01~05}_*.png × 30 (시드 22222~22252)
- 5008 sprite_generation_rules baseline 재사용

[검증]
- 30 PNG transparent 30~80%, asym ≤ 50
- 사용자 review PASS

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2~5 — 동일 패턴

| Sprint | 카테고리 | 챕터 | 시드 base | 개수 |
|---|---|---|---:|---:|
| 2 | Nature | CH 6~14 | 33333 | 54 |
| 3 | Animal | CH 15~20 | 44444 | 36 |
| 4 | Civ | CH 21~27 | 55555 | 42 |
| 5 | Fantasy | CH 28~35 | 77777 | 48 |

각 Sprint 동일 프로세스 — 시드 base + chapter offset, 카테고리 prefix, archetype 매핑.

### Sprint 6 — Auditor 검증 + docs

**작업**:
- `Tools > LittleHero > Audit Enemy Sprite Quality` 재실행 → 손상 0 + V2 fallback OK 확인
- `docs/little_hero/changelog_v1.md` 5009 항목 추가
- `CLAUDE.md §11/§12/§14` 갱신 (직전 5건 회전: 5009 추가, 5004 제거)

#### Sprint 6 commit
```
5009 Sprint 6 / T06 — Auditor 검증 + docs (6/6)

[변경]
- Auditor report 검증 (210 PNG transparent 30~80% 모두 통과)
- changelog_v1.md 5009 항목
- CLAUDE.md §11/§12/§14 갱신
- 후속 anchor 5010~5015 명시

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (Meta) → Sprint 2 (Nature) → ... → Sprint 5 (Fantasy) → Sprint 6 (docs)
```

순차 — 사용자 review 사이클 의존.

## 4. 검증 시나리오 (Play 검증)

### S1 — 챕터별 시각 일관성
1. Play Mode 진입 → CH 1~35 sample 5 챕터 (1, 8, 18, 25, 32)
2. 각 챕터의 적 sprite 확인 — chapter override 활용 시 챕터별 톤 차이 명확
- **검증**: 손상 sprite 0건, 카테고리 톤 일관

### S2 — Auditor 재검증
1. `Tools > LittleHero > Audit Enemy Sprite Quality` 실행
- **검증**: report 의 ChapterOverride 카테고리 모두 healthy (transparent 30~80%, damaged 0)

## 5. Definition of Done

- [ ] 210 PNG 재생성 + 사용자 review PASS (5 batch 모두)
- [ ] Auditor report 손상 0 + V2 fallback OK
- [ ] CLAUDE.md §11/§12/§14 + changelog 갱신

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 사용자 review 부담 ↑ (5 batch) | 5008 baseline 안정 — review 시간 ~10분/batch |
| Multi-character grid 일부 sprite | sprite_generation_rules §7 auto-retry |
| Sprint 5 (Fantasy 48 PNG) 가장 큼 | 필요 시 2 batch 분할 (CH 28~31 / CH 32~35) |

## 7. 변경 이력

- 2026-05-04: v1 작성.
