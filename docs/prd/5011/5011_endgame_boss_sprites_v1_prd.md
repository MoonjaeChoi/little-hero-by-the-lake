# 5011 — Daily 7 + Weekly 4 보스 sprite — Sprint PRD

> **출처**: `docs/prd/5011_endgame_boss_sprites_v1_prd.md` (디자인) — 옵션 B (Daily / Weekly 분리)
> **범위**: DailyBoss_{00~06}.png × 7 + WeeklyBoss_{00~03}.png × 4 = 11 sprite

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Daily 7 (요일별 톤, 시드 99000~99006) | 1 | 35~50분 |
| 2 | Weekly 4 (4 cycle, 시드 99100~99103) | 1 | 25~35분 |
| 3 | Auditor + docs | 1 | 15분 |
| **총** | **11 PNG** | **3** | **약 1.5시간** |

## 2. Sprint 명세

### Sprint 1 — Daily 7 (요일별 톤)

**대상**: `Generated_v2/Enemies/DailyBoss_{00~06}.png`

**프로세스**:
- 공통 epic boss 베이스 + 요일별 톤 prefix (Mon blue / Tue red / Wed green / Thu purple / Fri pink / Sat yellow / Sun gold)
- 시드 99000~99006

**검증 기준**:
- 7 PNG transparent 30~80%, asym ≤ 50, RGB std ≥ 30 (큰 캐릭터라 디테일 풍부)
- 요일별 톤 차별성 명확
- 보스 큰 캐릭터 (filling frame), royal pose

#### Sprint 1 commit
```
5011 Sprint 1 / T01 — Daily 7 보스 sprite 재생성 (1/3)

[변경]
- DailyBoss_{00~06}.png 재생성 (요일별 톤, 시드 99000~99006)
- epic boss + royal pose + 요일 색 (blue/red/green/purple/pink/yellow/gold)

[검증]
- 7 PNG healthy + 요일 톤 차별
- 보스 시각 epic + PG-12 (no menace)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — Weekly 4 (4 cycle)

**대상**: `Generated_v2/Enemies/WeeklyBoss_{00~03}.png`

**프로세스**:
- Light / Shadow / Life / Cosmic Realm 4 cycle
- 시드 99100~99103
- mythicChance 1.0 — 더 epic 분위기

#### Sprint 2 commit
```
5011 Sprint 2 / T02 — Weekly 4 보스 sprite 재생성 (2/3)

[변경]
- WeeklyBoss_{00~03}.png 재생성 (4 cycle: Light/Shadow/Life/Cosmic, 시드 99100~99103)
- divine / mysterious / vibrant / starscape 톤

[검증]
- 4 PNG healthy + cycle 차별성

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 3 — Auditor + docs

**작업**:
- Auditor → DailyBoss + WeeklyBoss 카테고리 healthy 11/11 확인
- changelog_v1.md 5011 항목
- CLAUDE.md §11/§12/§14 갱신

#### Sprint 3 commit
```
5011 Sprint 3 / T03 — Auditor + docs (3/3)

[검증]
- Daily 7 + Weekly 4 = 11 sprite healthy
- changelog + CLAUDE.md 갱신

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (Daily) → Sprint 2 (Weekly) → Sprint 3 (docs)
```

병렬 가능: Sprint 1 + Sprint 2 (서로 독립). 다만 사용자 review 단위 분리 권장.

## 4. 검증 시나리오

### S1 — Daily 보스 요일 진입
1. Endgame 메뉴 → Daily 보스 진입 (월~일 7회 sample)
- **검증**: 요일별 톤 차이 명확, 보스 시각 epic + PG-12

### S2 — Weekly 보스 4 cycle
1. Weekly 보스 진입 (1~4주 cycle sample)
- **검증**: cycle 별 톤 차별 (Light/Shadow/Life/Cosmic)

### S3 — Auditor
1. `Tools > LittleHero > Audit Enemy Sprite Quality`
- **검증**: DailyBoss 7/7 + WeeklyBoss 4/4 healthy

## 5. Definition of Done

- [ ] 11 sprite 재생성 + 사용자 review PASS
- [ ] 요일 7 / cycle 4 톤 차별
- [ ] Auditor PASS

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 보스 사이즈 너무 크거나 frame 초과 | "filling frame" + 여백 보존 prompt |
| 요일별 7 톤 차별 미세 | 시드 + prefix 명확 분리 |
| PG-12 톤 위반 (무서운 보스) | "no menace, friendly powerful, royal" 강조 |

## 7. 변경 이력

- 2026-05-04: v1 작성.
