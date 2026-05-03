# 5001 — Boss System v2 PRD

> **번호**: 5001 / **출처**: `5001_boss_system_v2.md` / **목표**: 6 Sprint, 9 commit, 3시간

## 0. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 인프라 (BossTier + BossPhase enum + IBossPhase + Factory) | 3 | 45분 |
| 2 | EnemyData 확장 + 51 SO 자동 마이그레이션 | 1 | 20분 |
| 3 | BossLootTable SO 4종 + 보상 hook | 2 | 30분 |
| 4 | EnemyController Phase 전환 통합 | 1 | 30분 |
| 5 | BossEncounterService 갱신 (BossTier 활용) | 1 | 15분 |
| 6 | Test + ProjectValidator | 1 | 30분 |
| **총** | | **9** | **2시간 50분** |

## Sprint 1 — 인프라 (3 commit)

### 1.1 BossTier + BossPhase enum

**파일**: `Scripts/Data/BossTier.cs` (신규)

**산출물**:
```
public enum BossTier { None, MiniBoss, ChapterBoss, DailyBoss, WeeklyBoss }
public enum BossPhase { Phase1, Phase2 }
```

**완료 조건**: 컴파일 0

### 1.2 IBossPhase + 2 구현체

**파일**: `Scripts/Gameplay/Enemies/BossPhases/` (신규 폴더)

**산출물**:
- `IBossPhase.cs` (interface, 4 메서드)
- `BossPhase1Default.cs` — 기본 attack pattern 실행
- `BossPhase2Aggressive.cs` — phase2Pattern + 1.3× 속도 + 텔레그라프 단축

**검증**: 컴파일 + 인터페이스 구현 검증

### 1.3 BossPatternFactory (정적, SOLID-O)

**파일**: `Scripts/Gameplay/Enemies/BossPatternFactory.cs` (신규)

**산출물**:
```
public static class BossPatternFactory {
    public static IBossAttackPattern Create(BossAttackType type);
}
```

EnemyController.CreateBossAttack switch 추출 (EquipmentPatternFactory 패턴 답습).

**완료**: switch 1 곳 집중 (Open-Closed)

### Sprint 1 commit
```
5001 Sprint 1 — BossTier/Phase enum + IBossPhase + Factory (1/9)

[신규]
  - Data/BossTier.cs (enum) + BossPhase.cs (enum)
  - Gameplay/Enemies/BossPhases/IBossPhase.cs + 2 구현체
  - Gameplay/Enemies/BossPatternFactory.cs

CLAUDE.md §15.1 SOLID-O (switch 추출) + ISP (작은 인터페이스)
```

---

## Sprint 2 — 51 SO 자동 마이그레이션 (1 commit)

### 2.1 EnemyData 확장 + WeaponMigrationSeed 답습

**파일**:
- `Scripts/Data/EnemyData.cs` (갱신) — `bossTier` / `phase2Pattern` / `phase2Threshold` 필드
- `Scripts/Editor/BossTierMigrationSeed.cs` (신규) — Editor 메뉴

**메뉴**: `Tools > LittleHero > Migrate Boss Tier (51)`

**자동 매핑 룰**:
- isMiniB=true → MiniBoss
- isBoss=true + path Contains "DailyBoss" → DailyBoss
- isBoss=true + path Contains "WeeklyBoss" → WeeklyBoss
- isBoss=true + 그 외 → ChapterBoss
- 일반 적 → None

**Phase 2 Pattern 매핑** (디자인 §5.2):
- ChapterBoss / DailyBoss / WeeklyBoss 만
- 현재 패턴 → 보강 패턴 (Charge → Charge / Pulse → Vine 등)

**검증**:
- 51 Boss SO 모두 bossTier wired
- 분포: MiniBoss N / ChapterBoss N / DailyBoss 7 / WeeklyBoss 4

### Sprint 2 commit
```
5001 Sprint 2 — EnemyData 확장 + 51 SO 마이그레이션 (2/9)

[갱신]
  - EnemyData: bossTier + phase2Pattern + phase2Threshold

[자동 마이그레이션]
  - 51 Boss SO 모두 bossTier wired
  - 분포: MiniBoss N / ChapterBoss N / DailyBoss 7 / WeeklyBoss 4
```

---

## Sprint 3 — BossLootTable SO + 보상 hook (2 commit)

### 3.1 BossLootTable SO 클래스 + 4 인스턴스

**파일**:
- `Scripts/Data/BossLootTable.cs` (신규)
- `Editor/BossLootTableSeed.cs` (신규) — 4 SO 자동 생성

**산출물**:
```
[CreateAssetMenu] public class BossLootTable : ScriptableObject {
    public BossTier tier;
    public RewardEntry[] rewards;
    public float guaranteedMythicChance;
}
```

**4 인스턴스** (Data/BossLoot/):
- LootTable_MiniBoss (★ 50 + ✦ 1)
- LootTable_ChapterBoss (★ 200 + ✦ N + ◇ 1)
- LootTable_DailyBoss (★ 100 + ⋆ 5)
- LootTable_WeeklyBoss (★ 500 + ⋆ 20 + mythicChance 1.0)

### 3.2 EnemyController.Die 보상 hook

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` (갱신)

**갱신**:
- EnemyData 에 `lootTable: BossLootTable` 필드 추가
- Die() 진입부에 `BossLootGrantService.Apply(lootTable)` 호출

**산출물**:
- `Core/BossLootGrantService.cs` (정적 헬퍼) — RewardEntry → CurrencyService grant

### Sprint 3 commit (2)
```
5001 Sprint 3.1 — BossLootTable SO + 4 인스턴스 (3/9)
5001 Sprint 3.2 — EnemyController.Die 보상 hook (4/9)
```

---

## Sprint 4 — EnemyController Phase 전환 통합 (1 commit)

### 4.1 EnemyController _currentPhase 도입

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` (갱신)

**기존 → 신규**:
```
// 기존
private IBossAttackPattern _bossAttack;
_bossAttack = CreateBossAttack(data.bossAttackType);
UpdateAnim → _bossAttack.Tick

// 신규
private IBossPhase _currentPhase;
_currentPhase = BossPhaseManager.CreateInitial(this);
UpdateAnim → _currentPhase.Tick + ShouldTransitionNext 검사
            → 전환 시 _currentPhase.OnExit + 신규 Phase OnEnter
```

**BossPhaseManager.cs** (정적 헬퍼) 신규.

**검증**:
- ChapterBoss HP 50% → Phase 2 자동 전환
- MiniBoss → Phase 1 만 (Phase 2 스킵)
- 콘솔 로그 `[Boss] Phase 1 → Phase 2`

### Sprint 4 commit
```
5001 Sprint 4 — EnemyController Phase 전환 통합 (5/9)
```

---

## Sprint 5 — BossEncounterService 갱신 (1 commit)

### 5.1 BossTier enum 활용

**파일**: `Scripts/Core/BossEncounterService.cs` (갱신)

**갱신**:
- `enemy.Data.isBoss` → `enemy.Data.bossTier == BossTier.DailyBoss/WeeklyBoss`
- 신화 보장: `bossTier == WeeklyBoss && lootTable.guaranteedMythicChance >= 1.0` 검증
- BossLootTable.guaranteedMythicChance 활용 → MythicMileageService.GuaranteeRoll

**검증**:
- Daily/Weekly 처리 호환 (회귀 X)
- 신화 보장 lootTable 기반

### Sprint 5 commit
```
5001 Sprint 5 — BossEncounterService BossTier 활용 (6/9)
```

---

## Sprint 6 — Test + Validator (1 commit)

### 6.1 BossSystemTests + ProjectValidator 룰

**파일**:
- `Tests/Editor/BossSystemTests.cs` (신규)
- `Scripts/Editor/ProjectValidator.cs` (갱신)

**Test 항목** (10):
- BossPatternFactory.Create 7 패턴 (각 1 Test)
- BossPhase1Default.OnEnter 동작
- BossPhase2Aggressive 1.3× 속도 검증
- ShouldTransitionNext HP 50% 검증
- BossLootTable.tier 매칭 (4 인스턴스)

**ProjectValidator 신규 룰**:
- 모든 isMiniB=true Boss SO → bossTier == MiniBoss 검증
- ChapterBoss / DailyBoss / WeeklyBoss → lootTable != null 검증
- bossTier != None → bossAttackType != None 검증

### Sprint 6 commit
```
5001 Sprint 6 — BossSystemTests 10 + Validator 룰 (7/9)

누적 NUnit Test 36 + 10 = 46
```

---

## Sprint 7~8 — changelog + CLAUDE.md (2 commit)

### 7.1 changelog_v1.md 등재
### 8.1 CLAUDE.md §6 + §11 갱신 (boss_system_v2 마일스톤)

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
Sprint 7+8 (changelog + CLAUDE.md)
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

## 검증 시나리오 (Play)

### 시나리오 1: 미니보스 처치 (Phase 1 만)
1. Chapter 진입 → 5 웨이브 미니보스 처치
2. Phase 전환 X (MiniBoss = Phase 1 만)
3. 보상 적용: ★ 50 + ✦ 1 (LootTable_MiniBoss)

### 시나리오 2: 챕터 보스 Phase 전환
1. Chapter 20 웨이브 보스
2. HP 50% 시 콘솔 로그 `[Boss] Phase 1 → Phase 2`
3. 패턴 변경 (예 Pulse → Vine)
4. 처치 시 보상: ★ 200 + ✦ N + ◇ 1 (ChapterBoss)

### 시나리오 3: 주간 보스 신화 보장
1. Endgame → 주간 보스 처치
2. mythicChance=1.0 → 신화 발현 100%
3. MythicEffectOverlay 무지개 fade

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 51 SO 마이그레이션 누락 | Editor 메뉴 + 검증 grep |
| Phase 2 1.3× 속도 너무 빠름 | CurveSimulator 후속 시뮬 |
| MiniBoss Phase 2 시도 | bossTier == MiniBoss 시 Phase 2 X (코드 검증) |
| BossLootTable 미존재 | EnemyData.lootTable=null 시 기본 보상 (5 ★) |

## 변경 이력
- 2026-05-03: 초기 작성 (5001 PRD)

## 참조
- `5001_boss_system_v2.md` — 디자인
- `boss_attack_v1.md` — 7 패턴 (현재)
- `weapon_system_v2_prd.md` — Sprint 패턴 답습
- `CLAUDE.md` §15 OOP 정책
