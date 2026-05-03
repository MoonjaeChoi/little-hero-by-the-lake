# 5001 Boss System v2 — Implementation Breakdown (개요)

> **Phase 3 산출** — v2 PRD (Mediator 확정) → 구현 태스크 10개 분해
> **일시**: 2026-05-04 · **총**: 10 태스크 / 10 commit / 3시간 10분
> **참조**: `5001_boss_system_v2_v2_prd.md` · `5001_boss_system_v2_decisions.md`

> **dry-run 노트**: 5001 = 10 태스크 (중간 규모) → 본 단일 `_breakdown_00.md` 에 통합 명세. 큰 시스템 (20+ 태스크) 은 `_breakdown_01~NN.md` 분리 권장 (prd-breakdown SKILL.md §1).

## 1. 태스크 목록

| ID | 제목 | 레이어 | Sprint | 의존성 | 시간 | 예상 라인 |
|---|---|---|---|---|---|---|
| T01 | BossTier + BossPhase enum 신규 | Data | 1.1 | - | 5분 | 15 |
| T02 | IBossPhase + 2 구현체 (D2 SerializeField) | Logic | 1.2 | T01 | 25분 | 90 |
| T03 | BossPatternFactory 분리 | Logic | 1.3 | - | 15분 | 25 |
| T04 | EnemyData 확장 + 51 SO 마이그레이션 | Data | 2 | T01 | 20분 | 60 |
| T05 | BossLootTable SO + 4 인스턴스 | Data | 3.1 | T01 | 20분 | 50 |
| T06 | EnemyController.Die 보상 hook + BossLootGrantService | Logic | 3.2 | T05 | 15분 | 40 |
| T07 | EnemyController Phase 전환 + BossPhaseManager | Logic | 4 | T02, T04 | 30분 | 80 |
| T08 | BossEncounterService BossTier 활용 | Logic | 5 | T01, T05 | 15분 | 40 |
| T09 | BossSystemTests 10 + ProjectValidator 룰 | Test | 6 | All above | 30분 | 200 |
| T10 | docs 통합 (changelog + CLAUDE.md §6/§11) | Docs | 7 | T09 | 20분 | - |

## 2. 의존성 그래프

```
T01 (BossTier enum) ─┬─→ T02 (IBossPhase) ─┐
                     │                      │
                     ├─→ T03 (Factory) ─────┤
                     │                      ├─→ T07 (Phase 전환) ─┐
                     ├─→ T04 (EnemyData) ───┘                     │
                     │                                            │
                     └─→ T05 (LootTable SO) ─→ T06 (Die hook) ───┤
                                            │                     │
                     T01 + T05 ─────────────→ T08 (Encounter) ───┤
                                                                  │
                                       All above ─→ T09 (Test) ──┴─→ T10 (docs)
```

## 3. 병렬 가능성 (병렬 그룹)

- **그룹 1** (T01 후 병렬 가능): T02 / T03 / T04 / T05
  - T01 (5분) → 그룹 1 (최대 25분) = 30분
- **그룹 2** (그룹 1 후 병렬 가능): T06 (T05 후) / T07 (T02+T04 후) / T08 (T01+T05 후)
  - 30분 + 30분 (max T07) = 60분
- **그룹 3** (직렬): T09 (모두 후) → T10
  - 30분 + 20분 = 50분

**총 (병렬 시)**: 30 + 30 + 50 = **110분 (1h50m)** — 직렬 3h10m 대비 50% 단축

## 4. CLAUDE.md §15 정합 사전 검증

| 태스크 | §15 정합 |
|---|---|
| T01 | §15.4 SO 우선 (enum 명료) ✅ |
| T02 | §15.1 ISP (4 메서드) + §15.4 SerializeField (D2=C) + §15.6 100줄 이내 (Phase1=30 / Phase2=40 라인) ✅ |
| T03 | §15.1 OCP (switch → Factory) + §15.8 Factory 패턴 ✅ |
| T04 | §15.4 SO 우선 + §15.10 워크플로 (Editor 메뉴 자동화) ✅ |
| T05 | §15.4 SO 우선 (4 인스턴스) ✅ |
| T06 | §15.7 Lightweight DI (BossLootGrantService 정적 헬퍼) ✅ |
| T07 | §15.7 DI (BossPhaseManager 정적) + §15.6 100줄 (EnemyController 100줄+ 우려 — 후속 분리 가능) ⚠️ |
| T08 | §15.7 DI 명시 주입 ✅ |
| T09 | §15.9 단위 테스트 (10 NUnit) + §15.12 Validator 룰 ✅ |
| T10 | §15.10 워크플로 (changelog 분리 운영) ✅ |

⚠️ **T07 주의**: EnemyController 가 신규 phase 필드 + Tick 통합 추가 → 100줄+ 가능. 검증 시 100줄+ 면 후속 사이클에서 PhasedEnemyController 분리 검토.

## 5. Unity MCP 안전 전략

- 한 사이클 = 1 태스크 + 컴파일 검증 + commit + 5~10s 대기
- 도메인 리로드 위험 태스크: T02 (interface 신규), T07 (EnemyController 큰 변경)
  - 각 후 5~10초 대기 + read_console 검증
- Validator 실행: T09 commit 직후 1회 일괄

## 6. 태스크별 상세 명세

### T01: BossTier + BossPhase enum 신규

**파일**: `LittleHero/Assets/_Project/Scripts/Data/BossTier.cs` (신규) + `BossPhase.cs` (신규)

**산출**:
```csharp
namespace LittleHero {
    public enum BossTier {
        None = 0,
        MiniBoss = 1,
        ChapterBoss = 2,
        DailyBoss = 3,
        WeeklyBoss = 4,
    }

    public enum BossPhase {
        Phase1 = 0,
        Phase2 = 1,
    }
}
```

**예상 라인**: 15 (각 7.5)

**검증**:
- C1: 컴파일 errors=0
- C4 Reflection: `LittleHero.BossTier`, `LittleHero.BossPhase` enum 5+2 값 존재

**commit**:
```
5001 Sprint 1.1 — BossTier + BossPhase enum (1/10)

[신규]
  - Scripts/Data/BossTier.cs (5 값)
  - Scripts/Data/BossPhase.cs (2 값)

CLAUDE.md §15.4 SO 우선 정합 — boolean → enum 명료화

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### T02: IBossPhase + 2 구현체

**파일**:
- `Scripts/Gameplay/Enemies/BossPhases/IBossPhase.cs` (신규)
- `Scripts/Gameplay/Enemies/BossPhases/BossPhase1Default.cs` (신규)
- `Scripts/Gameplay/Enemies/BossPhases/BossPhase2Aggressive.cs` (신규)

**산출**:

```csharp
public interface IBossPhase {
    void OnEnter(EnemyController boss);
    void Tick(EnemyController boss, float deltaTime);
    void OnExit(EnemyController boss);
    bool ShouldTransitionNext(EnemyController boss);
}

public sealed class BossPhase1Default : IBossPhase {
    private IBossAttackPattern _pattern;
    public void OnEnter(EnemyController boss) {
        _pattern = BossPatternFactory.Create(boss.Data.bossAttackType);
    }
    public void Tick(EnemyController boss, float deltaTime) {
        _pattern?.Tick(boss, deltaTime);
    }
    public void OnExit(EnemyController boss) { _pattern = null; }
    public bool ShouldTransitionNext(EnemyController boss) {
        return boss.Data.bossTier >= BossTier.ChapterBoss
            && boss.HpRatio <= boss.Data.phase2Threshold;
    }
}

public sealed class BossPhase2Aggressive : IBossPhase {
    [SerializeField] private float _speedMultiplier = 1.3f;  // D2=C
    private IBossAttackPattern _pattern;
    public void OnEnter(EnemyController boss) {
        _pattern = BossPatternFactory.Create(boss.Data.phase2Pattern);
        boss.SetAttackSpeedMultiplier(_speedMultiplier);
    }
    public void Tick(EnemyController boss, float deltaTime) {
        _pattern?.Tick(boss, deltaTime);
    }
    public void OnExit(EnemyController boss) {
        boss.SetAttackSpeedMultiplier(1.0f);
        _pattern = null;
    }
    public bool ShouldTransitionNext(EnemyController boss) => false;
}
```

**예상 라인**: IBossPhase 15 / Phase1Default 30 / Phase2Aggressive 45 = **90**

**검증**:
- C1: 컴파일 errors=0
- C4 Reflection: 3 Type + 4 메서드 (각)
- C2 Test (T09 에서): Phase1.ShouldTransitionNext HP 50% 검증 / Phase2.OnEnter 속도 적용

**commit**:
```
5001 Sprint 1.2 — IBossPhase + 2 구현체 (D2=C SerializeField) (2/10)

[신규]
  - Scripts/Gameplay/Enemies/BossPhases/IBossPhase.cs (15 라인)
  - BossPhase1Default.cs (30 라인) — 기본 attack pattern
  - BossPhase2Aggressive.cs (45 라인) — _speedMultiplier 1.3f SerializeField

CLAUDE.md §15.1 ISP + §15.4 SerializeField + §15.6 100줄 이내
```

---

### T03: BossPatternFactory 분리

**파일**: `Scripts/Gameplay/Enemies/BossPatternFactory.cs` (신규)

**산출**:
```csharp
public static class BossPatternFactory {
    public static IBossAttackPattern Create(BossAttackType type) {
        return type switch {
            BossAttackType.Charge => new ChargePattern(),
            BossAttackType.Pulse => new PulsePattern(),
            BossAttackType.Spread => new SpreadPattern(),
            BossAttackType.Summon => new SummonPattern(),
            BossAttackType.Vine => new VinePattern(),
            BossAttackType.Tornado => new TornadoPattern(),
            BossAttackType.BlackHole => new BlackHolePattern(),
            _ => null,
        };
    }
}
```

**예상 라인**: 25

**기존 파일 수정**: `EnemyController.CreateBossAttack` 메서드 → `BossPatternFactory.Create` 호출로 대체 (5라인 감소).

**검증**:
- C1 컴파일
- C4 Reflection: `BossPatternFactory.Create(BossAttackType) → IBossAttackPattern`
- C2 Test (T09): 7 패턴 모두 `Create` 비-null 검증

**commit**:
```
5001 Sprint 1.3 — BossPatternFactory 분리 (3/10)

[신규]
  - Scripts/Gameplay/Enemies/BossPatternFactory.cs (25 라인)

[수정]
  - EnemyController.CreateBossAttack → Factory 호출 (5 라인 감소)

CLAUDE.md §15.1 OCP + §15.8 Factory 패턴 — switch 1 곳 집중
```

---

### T04: EnemyData 확장 + 51 SO 마이그레이션

**파일**:
- `Scripts/Data/EnemyData.cs` (수정)
- `Scripts/Editor/BossTierMigrationSeed.cs` (신규)

**EnemyData 추가 필드**:
```csharp
[Header("Boss System v2")]
public BossTier bossTier;                            // None 기본
public BossAttackType phase2Pattern = BossAttackType.None;  // ChapterBoss+ 만
[Range(0f, 1f)] public float phase2Threshold = 0.5f;
public BossLootTable lootTable;                      // T05 SO 참조
```

**Editor 메뉴**: `Tools/LittleHero/Migrate Boss Tier (51)`

**자동 매핑 룰**:
- `isMiniB == true` → bossTier = MiniBoss (phase2Pattern = None, D3=A)
- `isBoss == true` + path Contains "DailyBoss" → DailyBoss
- `isBoss == true` + path Contains "WeeklyBoss" → WeeklyBoss
- `isBoss == true` + 그 외 → ChapterBoss
- 일반 적 → None

**Phase 2 Pattern 매핑** (ChapterBoss+):
- Charge → Charge / Pulse → Vine / Spread → Spread / Summon → Tornado / Vine → Pulse / Tornado → Tornado / BlackHole → BlackHole

**예상 라인**: EnemyData +6 / MigrationSeed 50 = **60**

**검증**:
- C1 컴파일
- C5 SO 검증: 51 Boss SO 모두 bossTier != None
- 분포: MiniBoss N / ChapterBoss N / DailyBoss 7 / WeeklyBoss 4 (검증 grep)

**commit**:
```
5001 Sprint 2 — EnemyData 확장 + 51 SO 마이그레이션 (4/10)

[수정]
  - Scripts/Data/EnemyData.cs: bossTier + phase2Pattern + phase2Threshold + lootTable

[신규]
  - Scripts/Editor/BossTierMigrationSeed.cs (50 라인)

[자동 마이그레이션]
  - 51 Boss SO 모두 bossTier wired
  - 분포: MiniBoss N / ChapterBoss N / DailyBoss 7 / WeeklyBoss 4
```

---

### T05: BossLootTable SO + 4 인스턴스

**파일**:
- `Scripts/Data/BossLootTable.cs` (신규)
- `Scripts/Editor/BossLootTableSeed.cs` (신규) — 4 SO 자동 생성
- `Data/BossLoot/LootTable_*.asset` × 4 (Editor 메뉴 산출)

**BossLootTable 클래스**:
```csharp
[CreateAssetMenu(fileName="LootTable_", menuName="LittleHero/BossLootTable")]
public sealed class BossLootTable : ScriptableObject {
    public BossTier tier;
    [SerializeField] private RewardEntry[] _rewards;
    [Range(0f, 1f)] public float guaranteedMythicChance;

    public IReadOnlyList<RewardEntry> Rewards => _rewards;
}

[Serializable] public struct RewardEntry {
    public CurrencyType type;       // StarDust / RainbowOrb / MagicCrystal / Starlight
    public int amount;
}
```

**4 인스턴스** (`Data/BossLoot/`):
- `LootTable_MiniBoss.asset` — ★ 50 + ✦ 1, mythicChance=0
- `LootTable_ChapterBoss.asset` — ★ 200 + ✦ 1 + ◇ 1, mythicChance=0.05
- `LootTable_DailyBoss.asset` — ★ 100 + ⋆ 5, mythicChance=0.1
- `LootTable_WeeklyBoss.asset` — ★ 500 + ⋆ 20, mythicChance=1.0

**예상 라인**: 50

**검증**:
- C5: 4 인스턴스 tier 매칭 + mythicChance ∈ [0,1]

**commit**:
```
5001 Sprint 3.1 — BossLootTable SO + 4 인스턴스 (5/10)

[신규]
  - Scripts/Data/BossLootTable.cs (30 라인)
  - Scripts/Editor/BossLootTableSeed.cs (50 라인)

[Data]
  - Data/BossLoot/LootTable_{Mini/Chapter/Daily/Weekly}Boss.asset
  - mythicChance: 0 / 0.05 / 0.1 / 1.0
```

---

### T06: EnemyController.Die 보상 hook + BossLootGrantService

**파일**:
- `Scripts/Core/BossLootGrantService.cs` (신규, 정적 헬퍼)
- `Scripts/Gameplay/Enemies/EnemyController.cs` (수정 — Die 진입부 hook)

**BossLootGrantService**:
```csharp
public static class BossLootGrantService {
    public static void Apply(BossLootTable lootTable) {
        if (lootTable == null) return;
        foreach (var reward in lootTable.Rewards) {
            CurrencyService.Grant(reward.type, reward.amount);
        }
        if (lootTable.guaranteedMythicChance >= 1.0f) {
            MythicMileageService.GuaranteeRoll();
        }
    }
}
```

**EnemyController.Die 수정** (3 라인 추가):
```csharp
private void Die() {
    // ... 기존 ItemDropService.Drop 등
    if (data.lootTable != null) {
        BossLootGrantService.Apply(data.lootTable);
    }
    // ...
}
```

**예상 라인**: 40 (Service 25 + Die 수정 3 + 헬퍼 12)

**검증**:
- C1 컴파일
- C2 Test (T09): mock LootTable → CurrencyService.Grant 호출 검증

**commit**:
```
5001 Sprint 3.2 — EnemyController.Die 보상 hook (6/10)

[신규]
  - Scripts/Core/BossLootGrantService.cs (25 라인)

[수정]
  - EnemyController.Die: lootTable null 체크 + BossLootGrantService.Apply

CLAUDE.md §15.7 정적 헬퍼 (Singleton 직접 참조 X)
```

---

### T07: EnemyController Phase 전환 + BossPhaseManager

**파일**:
- `Scripts/Gameplay/Enemies/BossPhaseManager.cs` (신규)
- `Scripts/Gameplay/Enemies/EnemyController.cs` (수정 — _bossAttack → _currentPhase)

**BossPhaseManager**:
```csharp
public static class BossPhaseManager {
    public static IBossPhase CreateInitial(EnemyController boss) {
        return boss.Data.bossTier == BossTier.None
            ? null
            : new BossPhase1Default();
    }

    public static void Update(EnemyController boss, float deltaTime) {
        boss.CurrentPhase?.Tick(boss, deltaTime);
        if (boss.CurrentPhase != null && boss.CurrentPhase.ShouldTransitionNext(boss)) {
            TransitionToPhase2(boss);
        }
    }

    private static void TransitionToPhase2(EnemyController boss) {
        boss.CurrentPhase.OnExit(boss);
        var p2 = new BossPhase2Aggressive();
        p2.OnEnter(boss);
        boss.SetCurrentPhase(p2);
        Debug.Log($"[Boss] Phase 1 → Phase 2 (HP {boss.HpRatio:P0})");
    }
}
```

**EnemyController 수정**:
- `private IBossAttackPattern _bossAttack` 제거
- `public IBossPhase CurrentPhase { get; private set; }` 추가
- `Initialize` 진입부: `CurrentPhase = BossPhaseManager.CreateInitial(this); CurrentPhase?.OnEnter(this);`
- `UpdateAnim` 갱신: `BossPhaseManager.Update(this, deltaTime);`

**예상 라인**: BossPhaseManager 40 + EnemyController +20-15 = 25 = **80**

⚠️ **EnemyController 100줄+ 우려** (T07 §15.6 검증). 측정 후 100줄+ 면 PhasedEnemyController 분리 후속 검토.

**검증**:
- C1 컴파일
- C2 Test (T09): MiniBoss → CurrentPhase != null && Phase 2 X 검증
- C6 시나리오 2: ChapterBoss HP 50% → 콘솔 로그 `[Boss] Phase 1 → Phase 2`

**commit**:
```
5001 Sprint 4 — EnemyController Phase 전환 + BossPhaseManager (7/10)

[신규]
  - Scripts/Gameplay/Enemies/BossPhaseManager.cs (40 라인)

[수정]
  - EnemyController: _bossAttack → _currentPhase 통합

CLAUDE.md §15.7 정적 헬퍼 + §15.5 EventBus (Phase 전환 로그)

⚠️ EnemyController 100줄+ 시 후속 분리 검토
```

---

### T08: BossEncounterService BossTier 활용

**파일**: `Scripts/Core/BossEncounterService.cs` (수정)

**변경**:
- `enemy.Data.isBoss` → `enemy.Data.bossTier == BossTier.DailyBoss/WeeklyBoss`
- 신화 보장: `bossTier == WeeklyBoss && lootTable.guaranteedMythicChance >= 1.0`

**예상 라인**: 40 변경 (기존 메서드 시그니처는 유지)

**검증**:
- C1 컴파일
- C2 Test (T09): bossTier 분기 호환

**commit**:
```
5001 Sprint 5 — BossEncounterService BossTier 활용 (8/10)

[수정]
  - BossEncounterService: isBoss → bossTier enum 분기
  - 신화 보장: lootTable.guaranteedMythicChance 활용

회귀 X — 기존 Daily/Weekly 처리 호환
```

---

### T09: BossSystemTests 10 + Validator 룰

**파일**:
- `Tests/Editor/BossSystemTests.cs` (신규)
- `Scripts/Editor/ProjectValidator.cs` (수정 — 신규 룰 3)

**BossSystemTests 10 메서드**:
```
1. BossPatternFactory_Create_Charge_ReturnsChargePattern
2. BossPatternFactory_Create_Pulse_ReturnsPulsePattern
3. BossPatternFactory_Create_Spread_ReturnsSpreadPattern
4. BossPatternFactory_Create_Summon_ReturnsSummonPattern
5. BossPatternFactory_Create_Vine_ReturnsVinePattern
6. BossPatternFactory_Create_Tornado_ReturnsTornadoPattern
7. BossPatternFactory_Create_BlackHole_ReturnsBlackHolePattern
8. BossPhase1_ShouldTransitionNext_OnHpBelowThreshold_ChapterBoss_ReturnsTrue
9. BossPhase2_OnEnter_AppliesSpeedMultiplier_1_3
10. BossLootTable_Tier_MatchesAllInstances (4 SO 검증)
```

**ProjectValidator 신규 룰 3**:
- `Boss_BossTier_NotNone` — 모든 isMiniB/isBoss=true SO bossTier != None
- `Boss_LootTable_NotNull` — ChapterBoss/Daily/Weekly → lootTable != null
- `Boss_AttackType_NotNone` — bossTier != None → bossAttackType != None

**예상 라인**: Tests 150 + Validator +50 = **200**

**검증**:
- C2 Editor Test: 10/10 PASS
- C3 Validator: errors=0, warnings 변동 ≤ 0

**commit**:
```
5001 Sprint 6 — BossSystemTests 10 + Validator 룰 3 (9/10)

[신규]
  - Tests/Editor/BossSystemTests.cs (150 라인, 10 NUnit)

[수정]
  - ProjectValidator: 3 신규 룰 (BossTier, LootTable, AttackType)

누적 NUnit Test 36 + 10 = 46
```

---

### T10: docs 통합 (D1=A)

**파일**:
- `docs/little_hero/changelog_v1.md` (등재)
- `CLAUDE.md` §6 (장비 시스템 — BossLootTable 추가)
- `CLAUDE.md` §11 (마일스톤 boss_system_v2 행 추가)

**changelog 항목**:
```markdown
- 2026-05-04: **5001 boss_system_v2** — BossTier 4 분리 (MiniBoss/Chapter/Daily/Weekly) + IBossPhase State 패턴 + BossPatternFactory + BossLootTable 4 인스턴스 (10 commit, PGE 6-Phase 풀파이프라인 첫 시연)
```

**CLAUDE.md §6 추가**:
```markdown
- BossLootTable 4 인스턴스 — Tier별 보상 (★/✦/◇/⋆ + mythicChance)
- IBossPhase State 패턴 (Phase 1/2 전환, ChapterBoss+ HP 50% 임계)
```

**CLAUDE.md §11 행 추가**:
```markdown
| **boss_system_v2** | 4 Tier + Phase State + BossPatternFactory + BossLootTable 4 | ✅ 2026-05-04 |
```

**검증**: 사용자 확인 (docs 변경)

**commit**:
```
5001 Sprint 7 — docs 통합 (changelog + CLAUDE.md §6/§11) (10/10)

[등재]
  - changelog_v1.md: 5001 boss_system_v2 항목

[갱신]
  - CLAUDE.md §6: BossLootTable + IBossPhase 추가
  - CLAUDE.md §11: 마일스톤 boss_system_v2 ✅ 2026-05-04

D1=A — 통합 commit (changelog + CLAUDE.md 동시)
```

---

## 7. 완료 정의 (DoD)

각 태스크:
- ✅ 컴파일 errors=0
- ✅ 회귀 X (기존 weapon_v2 / stats_v2 / Daily/Weekly 동작 유지)
- ✅ commit 메시지 형식 (위 템플릿)

전체:
- ✅ 51 Boss SO bossTier wired (T04)
- ✅ 4 BossLootTable 인스턴스 (T05)
- ✅ ChapterBoss HP 50% Phase 전환 동작 (T07)
- ✅ NUnit 10 PASS (T09)
- ✅ Validator 3 룰 통과 (T09)
- ✅ docs 등재 (T10)

## 8. 변경 이력
- 2026-05-04: 초기 작성 (Phase 3 Breakdown — 10 태스크)

## 9. 다음 단계

```
[Phase 3 ✅] breakdown_00.md 산출 (통합 명세)
[Phase 4 →] /prd-contract — 6 카테고리 검증 기준 (C1~C6)
[Phase 5 →] 구현 (사용자 진행 — little-hero-dev 또는 직접)
[Phase 6 →] /prd-evaluate — Contract 기준 검증 (반복 max 2회)
```
