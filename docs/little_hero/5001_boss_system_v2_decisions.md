# 5001 Boss System v2 — Decisions

> **Phase 2.3 Mediator 산출** — D1/D2/D3 미결 항목 결정 기록
> **모드**: ai-assisted · **결정자**: 사용자 (Claude 권장 일괄 채택)
> **일시**: 2026-05-04

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | Sprint 7+8 통합 vs 분리 | **A 통합** (Sprint 7 단일 commit) | Done |
| Medium | D2 | Phase 2 속도 1.3× 밸런스 | **C SerializeField** (`_speedMultiplier = 1.3f` 기본) | Done |
| Medium | D3 | MiniBoss Phase 2 일관성 | **A PRD 그대로** (MiniBoss = Phase 1 만) | Done |

## 2. Detailed Decisions

### 2.1 D1 — Sprint 7+8 통합

- **Decision**: 옵션 A (통합) — Sprint 7 = changelog + CLAUDE.md (1 commit, 20분)
- **Mode**: ai-assisted (Claude 권장 채택)
- **Rationale**:
  - CLAUDE.md §15.10 워크플로 정책: "한 commit = 한 의미 단위"
  - 두 작업 모두 docs 변경 → 의미 단위 동일
  - 작업량 < 30분 (통합 시 20분) → 분리 시 commit 두 개가 모두 작은 docs 변경
- **PRD Impact**:
  - §0 Sprint 표: Sprint 7 신규 추가 (8 → **8** Sprint, Sprint 8 폐기)
  - Sprint 7 명세: changelog_v1.md 등재 + CLAUDE.md §6/§11 갱신 (1 commit, 20분)
  - 총 commit: PRD v1 9 commit + Sprint 7 (1) = **10 commit** (critique 의 "9→8" 표기는 §0 표 오인 — 정정)
  - 총 시간: 2h50m + 20m = **3h10m**
- **Trade-off**:
  - 분리(B) 옵션 손실: 추적성 (어떤 commit 이 changelog 인지 vs CLAUDE.md 인지) — 작은 손실, git diff 로 추적 가능

### 2.2 D2 — Phase 2 속도 SerializeField

- **Decision**: 옵션 C (SerializeField) — `BossPhase2Aggressive._speedMultiplier = 1.3f` 기본값
- **Mode**: ai-assisted
- **Rationale**:
  - CLAUDE.md §15.4 SO 우선 + §15.11 Magic Number 회피
  - 디자이너 (사용자) Inspector 후속 조정 가능
  - 보스별 차별화 가능 (예 ChapterBoss 1.3× / WeeklyBoss 1.5×) — 후속 사이클
  - 기본값 1.3f = PRD 원안 유지 (디자인 의도 손실 X)
  - weapon_v2 직업 보너스 SO 패턴과 동일 (`WeaponClassBonus.cs` 사례)
- **PRD Impact**:
  - §3.3 BossPhase2Aggressive 명세: `[SerializeField] private float _speedMultiplier = 1.3f;`
  - Sprint 1.2 작업 항목: 클래스 명세에 SerializeField 추가 (5분 추가)
- **Trade-off**:
  - A (1.3× 하드코드) 옵션 손실: 단순성 (필드 1개 추가)
  - B (1.2× 안전) 옵션 손실: 안전 마진 (단, SerializeField 가 후속 조정 가능 → 동일 효과)

### 2.3 D3 — MiniBoss Phase 2 PRD 그대로

- **Decision**: 옵션 A (PRD 그대로) — MiniBoss = Phase 1 만, ChapterBoss+ Phase 2
- **Mode**: ai-assisted
- **Rationale**:
  - 미니보스 5/10/15 웨이브 등장, 평균 처치 시간 5초 → Phase 2 진입 (HP 30~50%) 전 처치될 가능성
  - 단순성 우선 (Tier 별 다른 메커니즘 OK — Endgame 차별화)
  - 후속 사이클에서 데이터 기반 도입 가능 — 코드 변경 X (IBossPhase 인터페이스가 모든 Tier 대응 가능), SO 만 phase2Pattern 갱신
- **PRD Impact**:
  - §3.2 EnemyData.phase2Pattern: ChapterBoss+ 만 wired (MiniBoss = None, BossAttackType.None)
  - §5.2 Phase 2 Pattern 매핑: MiniBoss 행 X
  - Sprint 2 마이그레이션 룰: MiniBoss → phase2Pattern = None
  - 변동: 미미 (PRD 원안)
- **Trade-off**:
  - B (Phase 2 도입) 옵션 손실: 일관성 + 도전적 게임플레이 — 단, 후속 사이클에서 데이터 보고 도입 가능

## 3. 통계

- Total Items: **3**
- AI Recommendations Followed: **3 / 3** (100%)
- User Overrides: **0**
- Mode: ai-assisted (사용자 일괄 권장 채택)

## 4. v2_prd.md 변경 diff 요약 (Rebuttal 초안 → Mediator 최종)

| 변경 | 적용 |
|---|---|
| Sprint 표 행 추가 | Sprint 7 (docs 통합) 행 신규 (Sprint 8 폐기, D1=A) |
| 총 commit | 9 (Sprint 1~6) + 1 (Sprint 7) = **10** |
| 총 시간 | **3h10m** |
| Sprint 1.2 명세 | `BossPhase2Aggressive._speedMultiplier = 1.3f` SerializeField (D2=C) |
| Sprint 2 마이그레이션 룰 | MiniBoss.phase2Pattern = None (D3=A) |
| §5.2 Phase 2 Pattern 매핑 | MiniBoss 행 X (D3=A) |
| 검증 시나리오 1 | "MiniBoss = Phase 1 만" 명시 (D3=A) |

## 5. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.3 Mediator — 사용자 일괄 권장 채택)

## 6. 다음 단계

```
[Phase 2.3 ✅] decisions.md + v2_prd.md (최종) 산출
[Phase 3 →] /prd-breakdown — 10 태스크 구현 분해
[Phase 4 →] /prd-contract — 6 카테고리 검증 기준
[Phase 5 →] 구현 (사용자 진행)
[Phase 6 →] /prd-evaluate — Contract 기준 검증
```
