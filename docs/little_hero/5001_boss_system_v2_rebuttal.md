# 5001 Boss System v2 — Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-04
> **입력**: `5001_boss_system_v2_critique.md` (점수 8.6/10, Accept 8 / Reject 0 / Discuss 3)

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| OOP §15.1 SOLID | A1 BossTier enum 4 분리 | High | Accepted | PRD §3.1 그대로 진행 | ✅ |
| OOP §15.1 ISP | A2 IBossPhase State 4 메서드 | High | Accepted | PRD §3.3 그대로 | ✅ |
| OOP §15.1 OCP | A3 BossPatternFactory 분리 | High | Accepted | PRD §3.4 그대로 | ✅ |
| OOP §15.4 SO 우선 | A4 BossLootTable SO 4 인스턴스 | High | Accepted | PRD §3.5 그대로 | ✅ |
| 시스템 통합 | A5 EnemyData 호환 (boolean 잔존 OK) | Medium | Accepted | PRD §3.2 그대로 | ✅ |
| Sprint 분해 | A6 의존성 그래프 명확 | Medium | Accepted | PRD §의존성 그대로 | ✅ |
| 검증 시나리오 | A7 Play 검증 3 시나리오 구체적 | Medium | Accepted | PRD §검증 그대로 | ✅ |
| 마이그레이션 | A8 51 Boss SO 자동 룰 | Medium | Accepted | PRD §5.1 그대로 | ✅ |
| Sprint 분해 | D1 Sprint 7+8 통합 권장 | Medium | **Discuss** | discussion_topics → mediator | △ |
| 디자인 옵션 | D2 Phase 2 1.3× 속도 밸런스 | Medium | **Discuss** | discussion_topics → mediator | △ |
| 디자인 일관성 | D3 MiniBoss Phase 2 도입 여부 | Medium | **Discuss** | discussion_topics → mediator | △ |

## 2. 품질 메트릭

- 총 Discussion Points: **11**
- Accepted (수정 반영): **8** (73%)
- Rejected: **0** (0%)
- Discuss (사용자 결정 필요): **3** (27%)
- §15 정합률: **100%** (Accept 8건 모두 §15 정합 강화)

## 3. Detailed Responses

### 3.1 A1 (High) — BossTier enum 4 Tier 분리

**Original Criticism**: "현재 `isMiniB` / `isBoss` boolean 한계 명확. 4 Tier (MiniBoss / ChapterBoss / DailyBoss / WeeklyBoss) 메이플 식 명료. BossTier.None 으로 비-보스 호환 유지."

- **Rebuttal**: 수용. CLAUDE.md §15.1 SRP + §15.4 SO 우선 정합. enum 4 Tier 분리는 메이플 키우기 톤 + Endgame Daily/Weekly 차별화에 필수.
- **Proposed Action**: PRD §3.1 (Sprint 1.1) 그대로 진행. `Scripts/Data/BossTier.cs` 신규.
- **PRD Impact**: §3.1 변동 X — v2 PRD 에 그대로.
- **Little Hero Context**: 51 Boss SO 의 `isMiniB`/`isBoss` 가 잔존 코드 X (Sprint 2 마이그레이션) → 점진 호환.

### 3.2 A2 (High) — IBossPhase State 패턴 (4 메서드)

**Original Criticism**: "4 메서드 (OnEnter/Tick/OnExit/ShouldTransitionNext) — ISP 정합. BossPhase1Default + BossPhase2Aggressive 구체 구현. HP 50% 기본 임계값 + EnemyData SerializeField."

- **Rebuttal**: 수용. CLAUDE.md §15.1 ISP (작은 인터페이스) + LSP (모든 구현체 호환). State 패턴은 Phase 전환의 표준 OOP 패턴.
- **Proposed Action**: PRD §3.3 (Sprint 1.2) 그대로. `Scripts/Gameplay/Enemies/BossPhases/` 폴더 신규.
- **PRD Impact**: §3.3 변동 X.

### 3.3 A3 (High) — BossPatternFactory 분리

**Original Criticism**: "EquipmentPatternFactory 패턴 답습 — 일관성 ↑. EnemyController.CreateBossAttack switch 추출 — 신규 패턴 추가 시 1 곳만 수정."

- **Rebuttal**: 수용. CLAUDE.md §15.1 OCP + §15.8 Factory 패턴 정합. 기존 `EquipmentPatternFactory.cs` 사례와 동일.
- **Proposed Action**: PRD §3.4 (Sprint 1.3) 그대로.
- **PRD Impact**: §3.4 변동 X.

### 3.4 A4 (High) — BossLootTable SO 4 인스턴스

**Original Criticism**: "ScriptableObject 우선 정합. Tier별 보상 명확. mythicChance 1.0 (WeeklyBoss) 메이플 키우기 보장 패턴 정합."

- **Rebuttal**: 수용. CLAUDE.md §15.4 SO 우선 절대 원칙. 마법 숫자 X (밸런스는 SO Inspector 에서 디자이너 조정 가능).
- **Proposed Action**: PRD §3.5 (Sprint 3.1) 그대로. `Data/BossLoot/` 4 인스턴스.
- **PRD Impact**: §3.5 변동 X.

### 3.5 A5 (Medium) — EnemyData 호환 유지

**Original Criticism**: "기존 `isMiniB` / `isBoss` boolean 잔존 가능 (bossTier=None 처리). 27 사용처 — 점진 마이그레이션."

- **Rebuttal**: 수용. 점진 마이그레이션 정책 — 기존 코드 깨지 않음. `bossTier != None` 헬퍼 메서드 (IsMiniBoss/IsBoss) 제공.
- **Proposed Action**: PRD §3.2 (Sprint 2) 그대로.
- **PRD Impact**: §3.2 변동 X.

### 3.6 A6 (Medium) — Sprint 의존성 그래프 명확

**Original Criticism**: "Sprint 1 → 2 → 3+4 (병렬) → 5 → 6. 의존성 명시 + 병렬 가능 표시."

- **Rebuttal**: 수용. 병렬 가능 (Sprint 3+4) 명시 → 시간 단축 (병렬 시 30분 절약).
- **Proposed Action**: PRD 의존성 그래프 그대로.
- **PRD Impact**: 변동 X.

### 3.7 A7 (Medium) — 검증 시나리오 3 구체적

**Original Criticism**: "MiniBoss / ChapterBoss Phase 전환 / WeeklyBoss 신화 보장. 콘솔 로그 검증 명시."

- **Rebuttal**: 수용. 시나리오 3 모두 객관 검증 가능 (콘솔 로그 + SaveData). Phase 4 Contract 의 C6 시나리오 기반.
- **Proposed Action**: PRD §검증 그대로 + Phase 4 Contract 에서 C6 으로 자동 변환.
- **PRD Impact**: 변동 X (Contract 단계에서 더 명세화).

### 3.8 A8 (Medium) — 51 Boss SO 자동 마이그레이션

**Original Criticism**: "isMiniB/isBoss + 경로 기반 자동 분류. WeaponMigrationSeed 패턴 답습 — 검증 가능."

- **Rebuttal**: 수용. 자동 분류 룰 명확 (Path Contains "DailyBoss" 등). 누락 시 검증 grep + Validator 룰 (bossTier != None).
- **Proposed Action**: PRD §5.1 (Sprint 2) 그대로 + Phase 4 Contract C5 SO 검증으로 자동화.
- **PRD Impact**: 변동 X.

### 3.9 D1 (Medium) — Sprint 7+8 통합 권장

**Original Criticism**: "Sprint 7 (changelog 1 commit) + Sprint 8 (CLAUDE.md 1 commit) 통합 권장. 두 작업 모두 docs 변경, 의미 단위 동일. 결과: 9 → 8 commit, 시간 30분 → 20분."

- **Status**: **Discuss → discussion_topics.md** 로 이동
- **이유**: 사용자 결정 사안 (단일 commit 추적성 vs 의미 단위 분리)
- **Current Position (PRD 그대로 진행 시)**: Sprint 7+8 분리 (각 1 commit, 총 9 commit)
- **Resolution Path**: mediator 에서 결정. Claude 권장: **통합** (옵션 A) — 두 작업 모두 docs, commit 단위 작음.

### 3.10 D2 (Medium) — Phase 2 1.3× 속도 밸런스

**Original Criticism**: "BossPhase2Aggressive 1.3× 속도 + 텔레그라프 단축. 사용자 체감 너무 빠를 수 있음 (PIVOT v4 cap 300 + Premium 피격 effects 고려). 옵션: A 1.3× 유지 / B 1.2× 안전 / C SerializeField (디자이너 조정)."

- **Status**: **Discuss → discussion_topics.md**
- **이유**: 디자인 밸런스 사안
- **Current Position**: PRD 명시값 1.3× 하드코드
- **Resolution Path**: mediator 에서 결정. Claude 권장: **옵션 C SerializeField** (CLAUDE.md §15.4 SO 우선 + 디자이너 후속 조정 가능).

### 3.11 D3 (Medium) — MiniBoss Phase 2 일관성

**Original Criticism**: "MiniBoss 는 Phase 1 만. ChapterBoss/DailyBoss/WeeklyBoss = Phase 2 지원. 옵션: A PRD 그대로 (단순) / B MiniBoss Phase 2 도입 (HP 30% 시) — 더 도전적."

- **Status**: **Discuss → discussion_topics.md**
- **이유**: 디자인 의도 사안 (난이도 vs 단순성)
- **Current Position**: MiniBoss Phase 2 X (PRD §5.2)
- **Resolution Path**: mediator 에서 결정. Claude 권장: **옵션 A PRD 그대로** (MiniBoss 단순성 유지, 후속 사이클에서 도입 가능).

## 4. PRD v2 적용 변경 요약 (Accept 8 → v2 초안 반영)

Accept 8건 모두 PRD v1 에 이미 명세된 디자인 — **v2 초안은 v1 그대로 + 메타 표기**:

- §3.1 BossTier enum 4 분리 ✅
- §3.3 IBossPhase State 패턴 ✅
- §3.4 BossPatternFactory ✅
- §3.5 BossLootTable SO ✅
- §3.2 EnemyData 호환 ✅
- 의존성 그래프 ✅
- 검증 시나리오 3 ✅
- 51 SO 마이그레이션 ✅

**Discuss 3건은 v2 초안에 미반영** — mediator 단계에서 결정 후 최종 v2 에 반영.

## 5. 자동 §15 검증 (정적)

PRD 텍스트 grep:
- ✅ `public float damage` 발견 X (SerializeField + private 패턴 준수)
- ✅ `switch (...)` 단일 — `BossPatternFactory.Create` 내부 (Factory 패턴 OK)
- ✅ `Update()` in 1500 객체 X (단일 Boss 의 IBossPhase.Tick)
- ✅ `Instantiate()` in Update/Tick X (BossLootTable 보상은 Die 시점 1회)
- ✅ `GameManager.Instance.X` 직접 참조 X (BossPhaseManager 정적 헬퍼)
- ✅ 신규 클래스 100줄 추정 — 모두 통과 (IBossPhase 15라인 / Phase1Default 30 / Phase2Aggressive 40 / Factory 20 / LootTable 25 / PhaseManager 30)

## 6. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal — 8 Accept / 0 Reject / 3 Discuss)

## 7. 다음 단계 (PGE 6-Phase)

```
[현재] Phase 2.2 ✅ — rebuttal.md + v2_prd.md(초안) + discussion_topics.md
[다음] Phase 2.3 Mediator — D1/D2/D3 결정
       Phase 3 Breakdown — 8~9 commit 태스크 분해
       Phase 4 Contract — 6 카테고리 검증 기준
       Phase 5 Implement — 구현
       Phase 6 Evaluate — Contract 기준 검증
```
