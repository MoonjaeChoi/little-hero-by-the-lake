# 5024 — Editor Test Expansion

> **목적**: NUnit 117 → 140+ 회귀 안전망 강화. 핵심 정적 service 테스트 우선.
> **scope**: 신규 NUnit 작성만. 기존 코드 동작 변경 0.

---

## 1. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | CLAUDE.md §13 #3 "Editor Test 확장" 항목 backlog 등재 → 5024 분리 |

---

## 2. 현재 시스템 분석

### 2.1 NUnit 현황 (117 PASS)

| 클래스 | 메서드 수 | 카테고리 |
|---|---|---|
| EnhancementServiceTests | 6 | service |
| DamageDealerTests | 7 | service |
| StarforceServiceTests | 10 | service |
| BossSystemTests | 10 | service |
| WeaponSystemTests | 13 | service |
| BossSceneSeparationTests | 10 | scene |
| SceneTransitionTests | 5 | scene |
| AtticDecompositionTests | 6 | scene/component |
| AtticEntryUITests | 6 | UI |
| HitEffectV2Tests | 8 | visual |
| EnemyHueVariationTests | 7 | visual |
| EnemyVisibilityCullerTests | 6 | perf |
| LMotionExtensionsTests | 6 | LitMotion |
| LMotionSmokeTests | 2 | LitMotion |
| DamageNumberFloaterFormatTests | 4 | GC |
| CharacterDataIntegrityTests | 4 | 5020 |
| SaveDataMigrationTests | 5 | 5020 |
| **합계** | **115** | (NUnit 어셈블리 자동 +2 = 117) |

### 2.2 미covered 핵심 service

| 영역 | 핵심 클래스 | 현재 NUnit |
|---|---|---|
| **DamageQuery** (정적 service) | `DamageQuery.FindNearest / FindEnemiesInCircle` | ❌ 없음 |
| **EquipmentManager** (slot 관리) | `EquipmentManager.EquipInstance / UnequipSlot` | △ WeaponSystemTests 부분 |
| **PlayerStats** (recalc 곡선) | `PlayerStats.Initialize / RecalculateStats / L100 piecewise` | ❌ 없음 |
| **ItemDropService** (드랍 분포) | `ItemDropService.RollDrop / 가챠 풀 제외` | ❌ 없음 |

§15.9 정책: "핵심 정적 service 우선 테스트". 위 4 영역 미coverage = 회귀 위험 ↑.

---

## 3. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 한 사이클 4 service 일괄 (8h+) | 1 commit per service | 큰 단위 commit, 회수 어려움 |
| B (★권장) | 4 Sprint × service 1개씩 (sprint 당 5~10 NUnit) | 단일 책임 commit, 점진 확장 | 4 commit |
| C | 우선순위만 (DamageQuery + PlayerStats 만, 2 sprint) | 빠른 종결 | 회귀 영역 미coverage 잔존 |

**권장 옵션 B** — 4 Sprint 각 1 commit, 회귀 안전망 점진 강화.

---

## 4. 권장 설계

### 4.1 Sprint 분배

| Sprint | service | 신규 NUnit | 누적 |
|---|---|---|---|
| 1 | DamageQuery | 6 | 117 → 123 |
| 2 | EquipmentManager | 7 | 123 → 130 |
| 3 | PlayerStats | 6 | 130 → 136 |
| 4 | ItemDropService | 8 | 136 → 144 |
| **합계** | | **27** | **117 → 144** |

(**144** > 140 목표 충족)

### 4.2 각 Test 클래스 명세 (Sprint PRD 에서 상세)

#### DamageQueryTests (Sprint 1)
- FindNearest_NoEnemies_ReturnsNull
- FindNearest_SingleEnemy_ReturnsEnemy
- FindNearest_MultipleEnemies_ReturnsClosest
- FindEnemiesInCircle_OutsideRadius_Returns0
- FindEnemiesInCircle_BoundaryCondition_ReturnsCorrect
- FindEnemiesInCircle_BufferOverflow_HandlesGracefully

#### EquipmentManagerTests (Sprint 2)
- EquipInstance_ValidSlot_Stores
- EquipInstance_WrongClass_Refuses
- UnequipSlot_Empty_NoOp
- EquipInstance_Conflict_Replaces
- ApplyAffix_RecalculatesStats
- StarforceLevel_AffectsBaseStats
- WeaponClassBonus_AppliedCorrectly

#### PlayerStatsTests (Sprint 3)
- Initialize_NullCharacter_DefaultsApplied
- Initialize_WithWarrior_Str2x
- Recalculate_AfterAffix_StatsUpdated
- L100Piecewise_Below50_FastCurve
- L100Piecewise_Above50_SlowCurve
- L100Piecewise_AtCap_Capped

#### ItemDropServiceTests (Sprint 4)
- RollDrop_NoBalance_ReturnsNull
- RollDrop_Distribution_RareWithinExpectedRange
- RollDrop_AffixCount_MatchesPool
- RollDrop_Mythic_OnlyFromMythicPool
- RollDrop_GachaPoolExcluded_FromDrop
- AffixRoll_Determinism_SameSeedSameResult
- SubAffixRoll_Distribution_GlobalPool
- DropContext_Struct_NoHeapAlloc

### 4.3 CLAUDE.md §15 정합

| 정책 | 검증 |
|---|---|
| §15.9 단위 테스트 | ✅ 핵심 정적 service 우선 |
| §15.6 100줄 룰 | △ 일부 Test 클래스 100L+ 가능 (NUnit 명세 길어짐) — Sprint 별 분리로 완화 |
| Mock 패턴 | ✅ ScriptableObject.CreateInstance<>() 사용 (5020 패턴 답습) |

---

## 5. 마이그레이션 단계

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | DamageQueryTests 6 | 1 | 60분 |
| 2 | EquipmentManagerTests 7 | 1 | 90분 |
| 3 | PlayerStatsTests 6 | 1 | 60분 |
| 4 | ItemDropServiceTests 8 | 1 | 120분 |
| **합계** | | **4** | **5h30m** |

---

## 6. 알려진 위험

| 위험 | 완화 |
|---|---|
| service 가 MonoBehaviour 의존 (예 Physics2D) → NUnit Edit 모드에서 동작 X | TestMode "EditMode" 한정 — 의존성 mock 또는 GameObject SetUp/TearDown |
| ItemDropService 내부 RNG 비결정성 | UnityEngine.Random.InitState(seed) 명시 |
| EquipmentManager 가 GameManager.Balance 참조 | NUnit SetUp 에서 임시 BalanceConfig SO 생성 |
| 신규 117 → 144 commit 후 회귀 0 보장 | 각 Sprint 직후 batch verify_phase6.sh c2 실행 |

---

## 7. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초기 — CLAUDE.md §13 #3 backlog → 5024 분리 |
