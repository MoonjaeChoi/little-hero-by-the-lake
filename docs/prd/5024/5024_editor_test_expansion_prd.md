# 5024 Sprint PRD — Editor Test Expansion

> 디자인: `docs/prd/5024_editor_test_expansion_prd.md`
> Sprint: 4 / commit: 4 / 시간: ~5h30m
> 의존성: 없음 (독립). 5023 종결 후 권장 (MetaProgression service NUnit 통합 가능).

---

## 1. 전체 개요

| Sprint | service | 신규 NUnit | 누적 | commit | 시간 |
|---|---|---|---|---|---|
| 1 | DamageQuery | 6 | 117 → 123 | 1 | 60분 |
| 2 | EquipmentManager | 7 | 123 → 130 | 1 | 90분 |
| 3 | PlayerStats | 6 | 130 → 136 | 1 | 60분 |
| 4 | ItemDropService | 8 | 136 → 144 | 1 | 120분 |
| **합계** | | **27** | **117 → 144** | **4** | **5h30m** |

---

## 2. Sprint 1 — DamageQueryTests (6 NUnit)

**파일**: `Tests/Editor/DamageQueryTests.cs` (신규)

| Test | 검증 |
|---|---|
| `FindNearest_NoEnemies_ReturnsNull` | 빈 scene → null |
| `FindNearest_SingleEnemy_ReturnsEnemy` | 단일 → 반환 |
| `FindNearest_MultipleEnemies_ReturnsClosest` | 거리 기반 정확성 |
| `FindEnemiesInCircle_OutsideRadius_Returns0` | 반경 외부 |
| `FindEnemiesInCircle_BoundaryCondition_ReturnsCorrect` | 경계 (radius == distance) |
| `FindEnemiesInCircle_BufferOverflow_HandlesGracefully` | 버퍼 64 초과 시 처리 |

**SetUp**: 임시 GameObject + EnemyController 부착, Layer "Enemy" 설정.

### Sprint 1 commit
```
5024 Sprint 1 / T01 — DamageQueryTests 6 (1/4)

[변경]
  + Tests/Editor/DamageQueryTests.cs (~80L NUnit 6)

[검증]
  - C2 NUnit 117 → 123 PASS
  - DamageQuery 정적 service 회귀 안전망 ✅
```

---

## 3. Sprint 2 — EquipmentManagerTests (7 NUnit)

**파일**: `Tests/Editor/EquipmentManagerTests.cs` (신규)

| Test | 검증 |
|---|---|
| `EquipInstance_ValidSlot_Stores` | slot 매칭 |
| `EquipInstance_WrongClass_Refuses` | weapon_v2 직업 검증 (전사가 활 거부) |
| `UnequipSlot_Empty_NoOp` | 빈 slot |
| `EquipInstance_Conflict_Replaces` | 동일 slot 교체 |
| `ApplyAffix_RecalculatesStats` | affix 변경 → PlayerStats 재계산 |
| `StarforceLevel_AffectsBaseStats` | ⭐ baseStats 배율 |
| `WeaponClassBonus_AppliedCorrectly` | Warrior+검 = +Dmg 10% |

**SetUp**: BalanceConfig + EquipmentTemplate 임시 SO + EquipmentInstance.

### Sprint 2 commit
```
5024 Sprint 2 / T02 — EquipmentManagerTests 7 (2/4)

[변경]
  + Tests/Editor/EquipmentManagerTests.cs (~110L NUnit 7)

[검증]
  - C2 NUnit 123 → 130 PASS
  - weapon_v2 직업 검증 회귀 안전망 ✅
```

---

## 4. Sprint 3 — PlayerStatsTests (6 NUnit)

**파일**: `Tests/Editor/PlayerStatsTests.cs` (신규)

| Test | 검증 |
|---|---|
| `Initialize_NullCharacter_DefaultsApplied` | character null 시 default |
| `Initialize_WithWarrior_Str2x` | warrior 보정 |
| `Recalculate_AfterAffix_StatsUpdated` | affix → 파생 stat |
| `L100Piecewise_Below50_FastCurve` | L1~50 빠른 곡선 |
| `L100Piecewise_Above50_SlowCurve` | L51~100 느린 곡선 |
| `L100Piecewise_AtCap_Capped` | L100 cap 동작 |

**SetUp**: BalanceConfig + CharacterData 임시 SO.

### Sprint 3 commit
```
5024 Sprint 3 / T03 — PlayerStatsTests 6 (3/4)

[변경]
  + Tests/Editor/PlayerStatsTests.cs (~90L NUnit 6)

[검증]
  - C2 NUnit 130 → 136 PASS
  - stats_v2 L100 piecewise 회귀 안전망 ✅
```

---

## 5. Sprint 4 — ItemDropServiceTests (8 NUnit)

**파일**: `Tests/Editor/ItemDropServiceTests.cs` (신규)

| Test | 검증 |
|---|---|
| `RollDrop_NoBalance_ReturnsNull` | balance null 시 |
| `RollDrop_Distribution_RareWithinExpectedRange` | 1000 roll 분포 (Common 70% 등) |
| `RollDrop_AffixCount_MatchesPool` | 주옵션 3 + 부옵션 3 |
| `RollDrop_Mythic_OnlyFromMythicPool` | 신화는 별도 풀에서만 |
| `RollDrop_GachaPoolExcluded_FromDrop` | 가챠 무기는 drop 제외 |
| `AffixRoll_Determinism_SameSeedSameResult` | seed 결정론 |
| `SubAffixRoll_Distribution_GlobalPool` | GlobalSubAffixPool 분포 |
| `DropContext_Struct_NoHeapAlloc` | 5017 struct 회귀 |

**SetUp**: SlotAffixPool[] + GlobalSubAffixPool + EquipmentTemplate[].

### Sprint 4 commit
```
5024 Sprint 4 / T04 — ItemDropServiceTests 8 (4/4) — 5024 종결

[변경]
  + Tests/Editor/ItemDropServiceTests.cs (~140L NUnit 8)

[검증]
  - C2 NUnit 136 → 144 PASS
  - 드랍 분포 / affix roll / 5017 GC 회귀 안전망 ✅
  - changelog + CLAUDE.md
```

---

## 6. 의존성 그래프

```
Sprint 1 ──┬→ Sprint 4 (DropContext 회귀)
Sprint 2 ──┤
Sprint 3 ──┘
```

Sprint 1~3 은 병렬 가능 (서로 독립). Sprint 4 는 Sprint 1~3 의 NUnit 패턴 답습 후 권장.

---

## 7. 완료 정의

- [ ] 4 Test 클래스 신규 + NUnit 27 PASS
- [ ] 누적 117 → 144 (목표 140+ 충족)
- [ ] C3 Validator OOP warning 변동 0
- [ ] `/prd-evaluate 5024` PASS

---

## 8. 알려진 위험

| 위험 | 완화 |
|---|---|
| Physics2D 의존 (DamageQuery 의 OverlapCircleNonAlloc) | EditMode 에서 GameObject + Collider2D 임시 생성 |
| RNG 비결정 | InitState(seed) 명시 |
| Test 클래스 100L+ 가능 | Sprint 별 분리, 100~140L 까지 허용 (Test 명세 특성상) |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 Sprint / 4 commit / 5h30m |
