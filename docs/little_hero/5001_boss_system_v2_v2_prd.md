# 5001 — Boss System v2 PRD (v2 최종 — Mediator 확정)

> **Phase 2.3 Mediator 단계 산출** — Accept 8건 + Discuss 3건 결정 모두 반영 (확정)
> **원본**: `5001_boss_system_v2_prd.md` (v1)
> **Rebuttal**: `5001_boss_system_v2_rebuttal.md`
> **Decisions**: `5001_boss_system_v2_decisions.md` (D1=A, D2=C, D3=A)
> **다음**: `/prd-breakdown` → 10 태스크 분해

**확정된 결정**:
- **D1=A**: Sprint 7+8 통합 → Sprint 7 (docs, 1 commit, 20분)
- **D2=C**: Phase 2 속도 SerializeField (`_speedMultiplier = 1.3f` 기본)
- **D3=A**: MiniBoss = Phase 1 만 (PRD 원안 그대로)

---

> **번호**: 5001 / **출처**: `5001_boss_system_v2.md` / **목표**: **7 Sprint, 10 commit, 3시간 10분** (mediator 확정)

## 0. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 인프라 (BossTier + BossPhase enum + IBossPhase + Factory) | 3 | 45분 |
| 2 | EnemyData 확장 + 51 SO 자동 마이그레이션 | 1 | 20분 |
| 3 | BossLootTable SO 4종 + 보상 hook | 2 | 30분 |
| 4 | EnemyController Phase 전환 통합 | 1 | 30분 |
| 5 | BossEncounterService 갱신 (BossTier 활용) | 1 | 15분 |
| 6 | Test + ProjectValidator | 1 | 30분 |
| **7** | **docs 통합 (changelog + CLAUDE.md §6/§11)** — **D1=A** | **1** | **20분** |
| **총** | | **10** | **3시간 10분** |

## Sprint 1 — 인프라 (3 commit) — Accept 반영

### 1.1 BossTier + BossPhase enum (Critique A1 ✅)

**파일**: `Scripts/Data/BossTier.cs` + `Scripts/Data/BossPhase.cs` (신규)

**산출물**:
```
public enum BossTier { None, MiniBoss, ChapterBoss, DailyBoss, WeeklyBoss }
public enum BossPhase { Phase1, Phase2 }
```

### 1.2 IBossPhase + 2 구현체 (Critique A2 ✅)

**파일**: `Scripts/Gameplay/Enemies/BossPhases/` (신규 폴더)

**산출물**:
- `IBossPhase.cs` (interface, 4 메서드 — OnEnter/Tick/OnExit/ShouldTransitionNext)
- `BossPhase1Default.cs` — 기본 attack pattern 실행
- `BossPhase2Aggressive.cs` — phase2Pattern + **속도 multiplier SerializeField (D2=C)**:
  ```csharp
  [SerializeField] private float _speedMultiplier = 1.3f;  // 기본값 1.3× — Inspector 조정 가능
  ```
  + 텔레그라프 단축 (1.0 / _speedMultiplier 비례)

### 1.3 BossPatternFactory (Critique A3 ✅)

**파일**: `Scripts/Gameplay/Enemies/BossPatternFactory.cs` (신규)

```
public static class BossPatternFactory {
    public static IBossAttackPattern Create(BossAttackType type);
}
```

EnemyController.CreateBossAttack switch 추출 (CLAUDE.md §15.1 OCP + §15.8 Factory).

### Sprint 1 commit
```
5001 Sprint 1 — BossTier/Phase enum + IBossPhase + Factory (1/{N})
```

---

## Sprint 2 — 51 SO 자동 마이그레이션 (1 commit) — Accept 반영

### 2.1 EnemyData 확장 + WeaponMigrationSeed 답습 (Critique A5/A8 ✅)

**파일**:
- `Scripts/Data/EnemyData.cs` (갱신) — `bossTier` / `phase2Pattern` / `phase2Threshold` 필드
- `Scripts/Editor/BossTierMigrationSeed.cs` (신규)

**자동 매핑 룰** (Critique A8):
- isMiniB=true → MiniBoss
- isBoss=true + path Contains "DailyBoss" → DailyBoss
- isBoss=true + path Contains "WeeklyBoss" → WeeklyBoss
- isBoss=true + 그 외 → ChapterBoss
- 일반 적 → None

**Phase 2 Pattern 매핑** (D3=A — ChapterBoss+ 만):
- MiniBoss → `phase2Pattern = BossAttackType.None` (Phase 2 X)
- ChapterBoss/DailyBoss/WeeklyBoss → 디자인 §5.2 매핑 (Charge → Charge+Summon 등)

### Sprint 2 commit
```
5001 Sprint 2 — EnemyData 확장 + 51 SO 마이그레이션 (2/{N})
```

---

## Sprint 3 — BossLootTable SO + 보상 hook (2 commit) — Accept 반영

### 3.1 BossLootTable SO 클래스 + 4 인스턴스 (Critique A4 ✅)

**파일**:
- `Scripts/Data/BossLootTable.cs` (신규)
- `Editor/BossLootTableSeed.cs` (신규) — 4 SO 자동 생성

```
[CreateAssetMenu] public class BossLootTable : ScriptableObject {
    public BossTier tier;
    public RewardEntry[] rewards;
    public float guaranteedMythicChance;
}
```

**4 인스턴스** (`Data/BossLoot/`):
- LootTable_MiniBoss (★ 50 + ✦ 1)
- LootTable_ChapterBoss (★ 200 + ✦ N + ◇ 1)
- LootTable_DailyBoss (★ 100 + ⋆ 5, mythicChance 0.05)
- LootTable_WeeklyBoss (★ 500 + ⋆ 20, mythicChance 1.0)

### 3.2 EnemyController.Die 보상 hook

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` (갱신)

EnemyData 의 `lootTable: BossLootTable` 필드 추가. Die() 진입부에 `BossLootGrantService.Apply(lootTable)` 호출.

**산출물**:
- `Core/BossLootGrantService.cs` (정적 헬퍼)

### Sprint 3 commit (2)
```
5001 Sprint 3.1 — BossLootTable SO + 4 인스턴스 (3/{N})
5001 Sprint 3.2 — EnemyController.Die 보상 hook (4/{N})
```

---

## Sprint 4 — EnemyController Phase 전환 통합 (1 commit) — Accept 반영

### 4.1 EnemyController _currentPhase 도입

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` (갱신)

```
// 기존
private IBossAttackPattern _bossAttack;

// 신규
private IBossPhase _currentPhase;
_currentPhase = BossPhaseManager.CreateInitial(this);
UpdateAnim → _currentPhase.Tick + ShouldTransitionNext 검사
            → 전환 시 _currentPhase.OnExit + 신규 Phase OnEnter
```

**BossPhaseManager.cs** (정적 헬퍼) 신규.

**검증 로그**: `[Boss] Phase 1 → Phase 2 (HP 50%)`

### Sprint 4 commit
```
5001 Sprint 4 — EnemyController Phase 전환 통합 (5/{N})
```

---

## Sprint 5 — BossEncounterService 갱신 (1 commit) — Accept 반영

### 5.1 BossTier enum 활용

**파일**: `Scripts/Core/BossEncounterService.cs` (갱신)

- `enemy.Data.isBoss` → `enemy.Data.bossTier == BossTier.DailyBoss/WeeklyBoss`
- 신화 보장: `bossTier == WeeklyBoss && lootTable.guaranteedMythicChance >= 1.0`

### Sprint 5 commit
```
5001 Sprint 5 — BossEncounterService BossTier 활용 (6/{N})
```

---

## Sprint 6 — Test + Validator (1 commit) — Accept 반영

### 6.1 BossSystemTests + ProjectValidator 룰

**파일**:
- `Tests/Editor/BossSystemTests.cs` (신규)
- `Scripts/Editor/ProjectValidator.cs` (갱신)

**Test 항목** (10):
- BossPatternFactory.Create 7 패턴 (각 1 Test)
- BossPhase1Default.OnEnter 동작
- BossPhase2Aggressive `_speedMultiplier` 1.3× 적용 검증 (D2=C SerializeField)
- ShouldTransitionNext HP 50% 검증
- BossLootTable.tier 매칭 (4 인스턴스)

**ProjectValidator 신규 룰**:
- 모든 isMiniB=true Boss SO → bossTier == MiniBoss
- ChapterBoss / DailyBoss / WeeklyBoss → lootTable != null
- bossTier != None → bossAttackType != None

누적 NUnit Test 36 + 10 = **46**

### Sprint 6 commit
```
5001 Sprint 6 — BossSystemTests 10 + Validator 룰 (7/{N})
```

---

## Sprint 7 — docs 통합 (1 commit, D1=A) — Mediator 확정

### 7.1 changelog_v1.md 등재 + CLAUDE.md §6/§11 갱신

**파일**:
- `docs/little_hero/changelog_v1.md` (등재) — 5001 Boss System v2 항목
- `CLAUDE.md` §6 (장비/능력치 시스템 현황 갱신, BossLootTable 추가)
- `CLAUDE.md` §11 (마일스톤 boss_system_v2 행 추가)

**산출**:
- changelog 항목: "5001 boss_system_v2 — BossTier 4 분리 + IBossPhase + Factory + BossLootTable 4 (10 commit, 3h10m)"
- §11 마일스톤: "boss_system_v2 — 4 Tier + Phase State + Factory ✅ 2026-05-04"

### Sprint 7 commit
```
5001 Sprint 7 — docs 통합 (changelog + CLAUDE.md §6/§11) (10/10)
```

---

## 의존성 그래프

```
Sprint 1 (enum + Phase + Factory)
   ↓
Sprint 2 (EnemyData + 51 SO 마이그레이션)
   ↓
Sprint 3 (LootTable + 보상 hook)  ─┐
   ↓                              │
Sprint 4 (Phase 전환 통합) ←──────┘
   ↓
Sprint 5 (BossEncounterService)
   ↓
Sprint 6 (Test + Validator)
   ↓
Sprint 7 (docs 통합, D1=A)
```

병렬 가능: Sprint 3 + 4 (LootTable 과 Phase 독립)

## 완료 정의

각 Sprint:
- ✅ 컴파일 0
- ✅ 회귀 X (기존 보스 동작 보존)
- ✅ commit 메시지 형식

전체:
- ✅ 51 Boss SO bossTier wired
- ✅ 4 BossLootTable 인스턴스
- ✅ ChapterBoss HP 50% Phase 전환
- ✅ NUnit 10 Test PASS
- ✅ Validator 신규 룰 통과

## 검증 시나리오 (Play) — Critique A7 ✅

### 시나리오 1: 미니보스 처치 (Phase 1 만, D3=A)
1. Chapter 진입 → 5 웨이브 미니보스 처치
2. Phase 전환 X (MiniBoss = Phase 1 만, phase2Pattern=None)
3. 보상 적용: ★ 50 + ✦ 1 (LootTable_MiniBoss)
4. 콘솔 로그: `[BossPhaseManager] MiniBoss defeated (Phase 1)`, `[BossLootGrantService] Applied LootTable_MiniBoss`

### 시나리오 2: 챕터 보스 Phase 전환
1. Chapter 20 웨이브 보스
2. HP 50% 시 콘솔 로그 `[Boss] Phase 1 → Phase 2`
3. 패턴 변경 (예 Pulse → Vine) + `_speedMultiplier=1.3f` 적용 (D2=C)
4. 처치 시 보상: ★ 200 + ✦ N + ◇ 1 (ChapterBoss)

### 시나리오 3: 주간 보스 신화 보장
1. Endgame → 주간 보스 처치
2. mythicChance=1.0 → 신화 발현 100%
3. MythicEffectOverlay 무지개 fade

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 51 SO 마이그레이션 누락 | Editor 메뉴 + 검증 grep + Validator 룰 |
| Phase 2 속도 너무 빠름 | D2=C SerializeField — Inspector 후속 조정 가능, 보스별 차별화 가능 |
| MiniBoss Phase 2 시도 | D3=A bossTier == MiniBoss → Phase 2 X (BossPhaseManager 검증) |
| BossLootTable 미존재 | EnemyData.lootTable=null 시 기본 보상 (5 ★) |

## 변경 이력
- 2026-05-03: 초기 작성 (5001 PRD v1)
- 2026-05-04: Phase 2.2 Rebuttal 산출 (Accept 8 / Reject 0 / Discuss 3 분리)
- 2026-05-04: Phase 2.3 Mediator 확정 (D1=A 통합, D2=C SerializeField, D3=A 그대로)

## 참조
- `5001_boss_system_v2.md` — 디자인
- `5001_boss_system_v2_critique.md` — Phase 2.1 Critique
- `5001_boss_system_v2_rebuttal.md` — Phase 2.2 Rebuttal
- `5001_boss_system_v2_discussion_topics.md` — D1/D2/D3 옵션 비교
- `5001_boss_system_v2_decisions.md` — Mediator 확정 결정
- `boss_attack_v1.md` — 7 패턴 (현재)
- `weapon_system_v2_prd.md` — Sprint 패턴 답습
- `CLAUDE.md` §15 OOP 정책
