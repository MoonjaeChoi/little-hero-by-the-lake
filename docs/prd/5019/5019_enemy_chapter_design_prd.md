# 5019 — 챕터 narrative 정합 적 디자인 패스 — Sprint PRD

> **출처**: `docs/prd/5019_enemy_chapter_design_prd.md` (디자인) — 옵션 A 35×6 = 210 sprite 재생성
> **목표**: 5009 generic prompt → 35 챕터 narrative-specific (CH1 호숫가 / CH22 사탕공장 / CH33 달의 뒷면 등)

## 1. Sprint 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---:|
| 1 | 35 챕터 × 6 archetype prompt 표 작성 + 사용자 검토 | 1 | 60~90분 |
| 2 | scripts/5019_chapter_narrative_batch.py (ComfyUI batch) | 1 | 20분 |
| 3 | ComfyUI 1차 — Meta/Nature/Animal (CH1~20, 120 sprite) | 1 | 50분 (ComfyUI ~50s/sprite × 120 =100분 자동, commit 50분) |
| 4 | ComfyUI 2차 — Civ/Fantasy (CH21~35, 90 sprite) + EnemySpriteAuditor | 1 | 40분 |
| 5 | 사용자 시각 review + 부분 재생성 + 5018 Texture 자동 적용 + docs | 1 | 60~90분 |
| **총** | **210 sprite + 5 commit** | **5** | **약 3~4h** |

## 2. Sprint 명세

### Sprint 1 — 210 prompt 표 작성

**파일**: `docs/prd/5019/5019_prompt_table.md` (신규)

**산출물** — 35 챕터 × 6 archetype prompt 표:

```markdown
## CH 1 — 호숫가 작은 마을

| Archetype | Prompt narrative |
|---|---|
| Boss | "lake giant guardian, friendly water spirit with reeds" |
| Large | "big lake fish with rainbow scales" |
| Medium | "village dog with brown fur, friendly puppy" |
| MiniA | "small bird, sparrow with white belly" |
| MiniB | "lakeside frog, green with lily pad hat" |
| Small | "firefly, glowing amber light" |

## CH 22 — 사탕 공장
| ... |

(35 챕터 모두)
```

**검증**: 사용자 review + 일부 챕터 narrative 조정

### Sprint 2 — Batch script

**파일**: `scripts/5019_chapter_narrative_batch.py`

5009 답습:
- 5 카테고리 batch (Meta/Nature/Animal/Civ/Fantasy)
- 시드 namespace: 90000+ (90001~90210)
- 768×1024 LayerDiffuse RGBA
- ComfyUI prompt 자동 추출 (Sprint 1 prompt 표)

### Sprint 3 — ComfyUI 1차 (CH1~20, 120 sprite)

영역: Meta (CH1~5 = 30) + Nature (CH6~14 = 54) + Animal (CH15~20 = 36) = **120 sprite**

ComfyUI batch 실행:
```
python scripts/5019_chapter_narrative_batch.py --range CH1-CH20
```

산출: 120 PNG (Generated_v2/Enemies/Chapters/CH01_*.png ~ CH20_*.png)

### Sprint 4 — ComfyUI 2차 (CH21~35, 90 sprite) + Auditor

영역: Civ (CH21~27 = 42) + Fantasy (CH28~35 = 48) = **90 sprite**

ComfyUI batch:
```
python scripts/5019_chapter_narrative_batch.py --range CH21-CH35
```

EnemySpriteAuditor 실행 — healthy sprite 통계.

### Sprint 5 — 사용자 review + 부분 재생성 + docs

1. 사용자 시각 review (10~20 sprite spot check)
2. 낮은 healthy sprite 재생성 (5011 답습)
3. 5018 TextureCompressionConfigurer 자동 적용 (1회)
4. changelog_v1.md / CLAUDE.md §11/§14

## 3. 의존성 그래프

```
Sprint 1 (prompt 표) ──┐
Sprint 2 (batch script) ┴→ Sprint 3 (1차 120) → Sprint 4 (2차 90 + Auditor) → Sprint 5 (review + docs)
```

## 4. Definition of Done

- [ ] 5 commit + 컴파일 errors=0 (스크립트 영역)
- [ ] 210 PNG narrative-specific 재생성
- [ ] healthy sprite ≥ 80% (5008 baseline 대비)
- [ ] EnemySpriteAuditor 통과
- [ ] 5018 ASTC 6×6 자동 적용 (TextureCompressionConfigurer)
- [ ] CLAUDE.md §11/§14 갱신

## 5. 검증 시나리오 (Play 검증)

### S1 — 5 챕터 시각 review (Sprint 5 후)
1. CH 1 (호숫가) wave 1~5 → 호수 강아지 + 물고기 표시 정합
2. CH 22 (사탕 공장) → 사탕 인형 적
3. CH 28 (살아있는 정원) → 식물 정령 적
4. CH 33 (달의 뒷면) → 달 토끼 적
5. CH 35 (별과 별 사이) → 별 정령 적

### S2 — 시각 회귀 X
1. archetype baseline (30) 변경 X
2. Boss / Daily / Weekly / V1 영역 변경 X (5011/5010)

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Sprint 1 prompt 작성 시간 ↑ | Claude 가 35 챕터 narrative 기반 자동 제시, 사용자 review 만 |
| ComfyUI 일부 챕터 healthy 부족 | Sprint 5 부분 재생성 (5011 답습, healthy 6/11 후 retry) |
| Mac/iOS 빌드 용량 ↑ | 5018 ASTC 6×6 자동 적용 (cap 큰 차이 X) |
| Server (RTX 5070) 가용성 | 5014/5009 답습 — SSH automation |

## 7. 변경 이력
- 2026-05-06: 초기 작성
