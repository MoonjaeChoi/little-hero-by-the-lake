# Sprint Contract: 5001 — Boss System v2

> **Phase 4 PGE Planner — Sprint Contract**
> **PRD**: `5001_boss_system_v2_v2_prd.md` · **Breakdown**: `5001_boss_system_v2_breakdown_00.md`
> **Decisions**: `5001_boss_system_v2_decisions.md` (D1=A 통합, D2=C SerializeField, D3=A PRD 그대로)
> **생성일**: 2026-05-04
> **검증 대상**: 로컬 Unity Editor (6.3 LTS, URP 2D)

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0
- **C1.2**: 도메인 리로드 5초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: 기존 시스템 회귀 X (weapon_v2 / stats_v2 / Daily/Weekly 동작 유지)

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 5
mcp__UnityMCP__read_console (errors=0)
```

---

## 2. C2 Editor Test (NUnit) 기준

| Test 클래스 | 메서드 수 | 목적 |
|---|---|---|
| `BossSystemTests` | **10** | Factory / Phase / LootTable 단위 검증 |

### 2.1 BossSystemTests 명세 (10 메서드, T09 산출)

```csharp
[Test] public void BossPatternFactory_Create_Charge_ReturnsChargePattern();
[Test] public void BossPatternFactory_Create_Pulse_ReturnsPulsePattern();
[Test] public void BossPatternFactory_Create_Spread_ReturnsSpreadPattern();
[Test] public void BossPatternFactory_Create_Summon_ReturnsSummonPattern();
[Test] public void BossPatternFactory_Create_Vine_ReturnsVinePattern();
[Test] public void BossPatternFactory_Create_Tornado_ReturnsTornadoPattern();
[Test] public void BossPatternFactory_Create_BlackHole_ReturnsBlackHolePattern();
[Test] public void BossPhase1_ShouldTransitionNext_OnHpBelowThreshold_ChapterBoss_ReturnsTrue();
[Test] public void BossPhase2_OnEnter_AppliesSpeedMultiplier_1_3();   // D2=C 기본값 검증
[Test] public void BossLootTable_Tier_MatchesAllInstances();          // 4 SO 검증
```

**검증 방법**: Window > General > Test Runner > Edit Mode > Run All → **10/10 PASS**

**합격**: 10/10 PASS. 1개라도 FAIL 시 C2 FAIL.

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings ≤ 기존 (5001 시작 시점 기준 — 21)
- **C3.3**: 신규 룰 3 PASS:
  - `Boss_BossTier_NotNone` — 모든 isMiniB/isBoss=true SO bossTier != None
  - `Boss_LootTable_NotNull` — ChapterBoss/Daily/Weekly → lootTable != null
  - `Boss_AttackType_NotNone` — bossTier != None → bossAttackType != None

**검증 방법**:
```
mcp__UnityMCP__execute_menu_item: Tools/LittleHero/Validate Project
mcp__UnityMCP__read_console (warnings)
```

**합격**: errors=0 + warnings 변동 ≤ 0 + 신규 룰 3 모두 PASS.

---

## 4. C4 Reflection 검증 (정적)

| Type | Members |
|---|---|
| `LittleHero.BossTier` (enum) | `None / MiniBoss / ChapterBoss / DailyBoss / WeeklyBoss` (5 값) |
| `LittleHero.BossPhase` (enum) | `Phase1 / Phase2` (2 값) |
| `LittleHero.IBossPhase` (interface) | `OnEnter / Tick / OnExit / ShouldTransitionNext` (4 메서드) |
| `LittleHero.BossPhase1Default` (sealed class) | `IBossPhase` 구현 |
| `LittleHero.BossPhase2Aggressive` (sealed class) | `IBossPhase` 구현 + `_speedMultiplier` SerializeField (D2=C) |
| `LittleHero.BossPatternFactory` (static class) | `Create(BossAttackType) → IBossAttackPattern` (정적) |
| `LittleHero.BossLootTable` (SO) | `tier / Rewards / guaranteedMythicChance` |
| `LittleHero.BossPhaseManager` (static class) | `CreateInitial(EnemyController) → IBossPhase`, `Update(EnemyController, float)` |
| `LittleHero.BossLootGrantService` (static class) | `Apply(BossLootTable)` |

**검증 방법** (Editor 시뮬 또는 unity_reflect):
```csharp
foreach (var spec in C4_specs) {
    var t = System.Type.GetType($"{spec.Type}, Assembly-CSharp");
    Assert.IsNotNull(t, $"Type missing: {spec.Type}");
    foreach (var member in spec.Members) {
        Assert.IsNotNull(t.GetMember(member).FirstOrDefault(),
            $"Member missing: {spec.Type}.{member}");
    }
}
```

**합격**: 9 Type + 모든 Member 존재. 누락 1건이라도 FAIL.

---

## 5. C5 SO 검증

| SO | 인스턴스 | 필드 검증 |
|---|---|---|
| `BossLootTable` | 4 | tier ∈ {MiniBoss, ChapterBoss, DailyBoss, WeeklyBoss}, mythicChance ∈ [0, 1] |
| `EnemyData` (보스만) | 51 | bossTier != None, ChapterBoss+ → lootTable != null |

### 5.1 BossLootTable 4 인스턴스 (mythicChance 분포)

| 인스턴스 | tier | mythicChance | rewards 수 |
|---|---|---|---|
| LootTable_MiniBoss | MiniBoss | 0.0 | 2 (★ 50 + ✦ 1) |
| LootTable_ChapterBoss | ChapterBoss | 0.05 | 3 (★ 200 + ✦ 1 + ◇ 1) |
| LootTable_DailyBoss | DailyBoss | 0.1 | 2 (★ 100 + ⋆ 5) |
| LootTable_WeeklyBoss | WeeklyBoss | 1.0 | 2 (★ 500 + ⋆ 20) |

### 5.2 EnemyData 51 보스 분포

- MiniBoss: N (isMiniB=true 자동 분류)
- ChapterBoss: M (isBoss=true + path != Daily/Weekly)
- DailyBoss: 7 (path Contains "DailyBoss")
- WeeklyBoss: 4 (path Contains "WeeklyBoss")
- **합계**: 51

**검증 방법**:
```csharp
// BossLootTable
var lootTables = AssetDatabase.FindAssets("t:BossLootTable")
    .Select(g => AssetDatabase.GUIDToAssetPath(g))
    .Select(p => AssetDatabase.LoadAssetAtPath<BossLootTable>(p))
    .ToList();
Assert.AreEqual(4, lootTables.Count);
foreach (var lt in lootTables) {
    Assert.AreNotEqual(BossTier.None, lt.tier);
    Assert.IsTrue(lt.guaranteedMythicChance >= 0 && lt.guaranteedMythicChance <= 1);
}

// EnemyData (보스만)
var bosses = AssetDatabase.FindAssets("t:EnemyData")
    .Select(g => AssetDatabase.LoadAssetAtPath<EnemyData>(AssetDatabase.GUIDToAssetPath(g)))
    .Where(e => e.isMiniB || e.isBoss)
    .ToList();
Assert.AreEqual(51, bosses.Count);
foreach (var e in bosses) {
    Assert.AreNotEqual(BossTier.None, e.bossTier);
    if (e.bossTier >= BossTier.ChapterBoss)
        Assert.IsNotNull(e.lootTable);
}
```

**합격**: 모든 SO 인스턴스 필드 wiring OK.

---

## 6. C6 Play 시나리오 (수동 — 사용자 보고)

### 시나리오 1: MiniBoss 처치 (Phase 1 만, D3=A)

**사전 조건**:
- 챕터 1 진입
- Wave 5 (MiniBoss 출현 wave)

**단계**:
1. 챕터 1 진입 → Wave 5 자동 시작
2. MiniBoss 출현 → 자동 공격 받음
3. HP 0 으로 처치
4. Wave 6 자동 진입

**기대 결과**:
- Phase 전환 X (MiniBoss = Phase 1 만, phase2Pattern=None)
- 콘솔 로그:
  - `[BossPhaseManager] MiniBoss initialized (Phase 1)` (없음 — Phase 전환 X)
  - `[BossLootGrantService] Applied LootTable_MiniBoss (★+50, ✦+1)`
- SaveData (MetaProgression):
  - `starDust += 50`
  - `rainbowOrb += 1`
- UI: 보상 토스트 (★/✦ 아이콘)

---

### 시나리오 2: ChapterBoss Phase 전환 (D2=C SerializeField 검증)

**사전 조건**:
- 챕터 1 Wave 20 (ChapterBoss)

**단계**:
1. Wave 20 ChapterBoss 출현 (Phase 1 — `BossPhase1Default`)
2. 자동 공격 받음 (예 Pulse 패턴, 일반 속도)
3. HP 50% 시점 도달
4. `BossPhase1.ShouldTransitionNext == true` 반환
5. `BossPhase2Aggressive.OnEnter` 호출
6. Phase 2 — `phase2Pattern` 적용 (예 Vine), `_speedMultiplier=1.3f` 적용
7. HP 0 으로 처치

**기대 결과**:
- 콘솔 로그:
  - `[Boss] Phase 1 → Phase 2 (HP 50%)`
  - `[BossPhase2.OnEnter] Aggressive mode (×1.3 speed)`
  - `[BossLootGrantService] Applied LootTable_ChapterBoss (★+200, ✦+1, ◇+1)`
- 시각 변화:
  - 공격 속도 변경 (체감 가능)
  - 패턴 변경 (Pulse → Vine)
- 보상: ★ 200 + ✦ 1 + ◇ 1

---

### 시나리오 3: WeeklyBoss 신화 보장 (D2 적용 + 신화 발현)

**사전 조건**:
- Endgame 잠금 해제 (CH 35 클리어)
- 주간 보스 출현 (월요일 KST 0시 리셋 후)

**단계**:
1. Endgame 메뉴 → 주간 보스 진입
2. `BossEncounterRunner.Begin(WeeklyBoss)` 호출
3. WeeklyBoss Phase 1 → 50% Phase 2 전환
4. 처치
5. `LootTable_WeeklyBoss.guaranteedMythicChance == 1.0` → 신화 발현 보장

**기대 결과**:
- 콘솔 로그:
  - `[Boss] Phase 1 → Phase 2 (HP 50%)`
  - `[BossLootGrantService] Applied LootTable_WeeklyBoss (★+500, ⋆+20)`
  - `[MythicMileageService] Mythic guaranteed (chance=1.0)`
- 신화 등록: `MythicMileageService.GuaranteeRoll()` 호출
- UI: 신화 발현 토스트 + MythicEffectOverlay 무지개 fade

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0
- [ ] Editor Test 10/10 PASS (BossSystemTests)
- [ ] 누적 NUnit 36 → 46
- [ ] ProjectValidator warnings 변동 ≤ 0
- [ ] 기존 weapon_v2 가챠 정상 (회귀 검사)
- [ ] 기존 stats_v2 데미지 공식 정상
- [ ] 기존 Daily/Weekly Endgame 정상 (BossEncounterService 호환)
- [ ] 60fps 유지 (Profiler 권장 — 동시 적 cap 300 환경)
- [ ] 빌드 (Unity → Build) 성공 (해당 시)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 8/10 이상 ✅ (1~2 FAIL 허용 — 단 BossPatternFactory 7 패턴 모두 PASS 필수)
- C3 Validator errors=0 ✅
- C4 Reflection 9/9 ✅ (전체 통과 필수)
- C5 SO 검증 51 + 4 모두 ✅
- 시나리오 1, 2 PASS

**완전 합격**:
- 모든 카테고리 ✅ (C2 10/10)
- 시나리오 1, 2, 3 모두 PASS
- 비기능 요구사항 모두 ✅

## 9. Trade-off (Mediator 결정 기반)

### 9.1 D1=A (Sprint 7+8 통합)
- **수용 손실**: 추적성 약간 감소 (changelog vs CLAUDE.md commit 분리 X)
- **수용 이득**: commit 1개 감소, 시간 10분 단축, 의미 단위 동일

### 9.2 D2=C (SerializeField)
- **수용 손실**: 클래스에 필드 1개 추가 (단순성 약간 손실)
- **수용 이득**: 디자이너 후속 조정 가능, 보스별 차별화 가능, §15.4 SO 우선 정합

### 9.3 D3=A (MiniBoss Phase 2 X)
- **수용 손실**: Tier 일관성 약함 (Mini 단일 / Chapter+ Phase 2)
- **수용 이득**: 단순성, 게임플레이 페이스 적절 (5초 처치 시간 고려), 후속 도입 가능

## 10. Generator/Evaluator 분리 (Anthropic PGE)

- **Phase 5 Generator** (구현자): 코드 작성, breakdown 따라 진행. **자기 평가 X**.
- **Phase 6 Evaluator** (검증자): 본 Contract 의 C1~C6 기준만 사용. **scope creep 금지**.
- 같은 Claude 라도, 본 Contract 가 단일 진실원 → 편향 제거.

## 11. 변경 이력
- 2026-05-04: 초기 작성 (Phase 4 Contract — 6 카테고리 + 시나리오 3)

## 12. 다음 단계

```
[Phase 4 ✅] contract.md + contract_checklist.md
[Phase 5 →] 구현 시작 (사용자 진행)
   - 옵션 A: little-hero-dev 스킬 활용 (자동 사이클)
   - 옵션 B: 직접 구현 (T01 부터 순차 / 병렬)
   - 사용자 확인 후 진행
[Phase 6 →] /prd-evaluate --prd 5001 (Contract 기준 자동 검증)
```
