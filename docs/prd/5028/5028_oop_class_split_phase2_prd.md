# 5028 Sprint PRD — oop_class_split_phase2

> **PRD v1 (초안)** · 디자인: `docs/prd/5028_oop_class_split_phase2_prd.md`
> 옵션 B 채택 — Top 3 production service 분리 (UI Panel 5028.1 후속)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | EquipmentManager 326L → 4 component + facade ~95L | 1 | 60분 |
| 2 | LimitedShopService 303L → 3 service + facade ~80L | 1 | 50분 |
| 3 | StarforceService 255L → 3 helper + facade ~140L | 1 | 40분 |
| 4 | Validator 회귀 + changelog + CLAUDE.md | 1 | 20분 |
| **합계** | | **4** | **2h 50m** |

---

## 2. Sprint 1 — EquipmentManager 분리 (1 commit)

### 2.1 신규 4 파일

**위치**: `Scripts/Gameplay/Equipment/`

| 파일 | 라인 | 책임 |
|---|---|---|
| `EquipmentClassResolver.cs` (정적) | ~30 | `ResolvePlayerClass` (캐릭터 → CharacterClass) + class change 감지 |
| `EquipmentTemplateRegistry.cs` (정적) | ~25 | `RegisterTemplates(IEnumerable)` + Dictionary lookup |
| `EquipmentPatternActivator.cs` (POCO) | ~80 | `ActivatePattern` / `DeactivatePattern` (Strategy hook 8 IAttackPattern) |
| `EquipmentInstanceSyncer.cs` (POCO) | ~100 | `SyncEquippedInstances` / `EquipInstance` / `UnequipInstance` |

### 2.2 EquipmentManager facade (~95L)

```csharp
public class EquipmentManager : MonoBehaviour {
    private EquipmentPatternActivator _activator;
    private EquipmentInstanceSyncer _syncer;

    void Awake() { _activator = new EquipmentPatternActivator(this); _syncer = new EquipmentInstanceSyncer(this, _activator); }
    void Update() { /* tick patterns 위임 */ }

    public void Equip(EquipmentData data) => _syncer.Equip(data);
    public void EquipInstance(EquipmentInstance inst) => _syncer.EquipInstance(inst);
    public void SyncEquippedInstances() => _syncer.SyncEquippedInstances();

    // 정적 위임
    public static CharacterClass ResolvePlayerClass() => EquipmentClassResolver.ResolvePlayerClass();
    public static void RegisterTemplates(IEnumerable<EquipmentTemplate> templates) => EquipmentTemplateRegistry.RegisterTemplates(templates);
}
```

### 2.3 검증 기준

- 컴파일 errors=0
- 호출처 ~30 파일 시그니처 변경 0
- NUnit EquipmentManagerTests (5024) 회귀 0

#### Sprint 1 commit

```
5028 Sprint 1 — EquipmentManager 326L → 4 component + facade ~95L (1/4)

[변경]
  + Gameplay/Equipment/EquipmentClassResolver.cs (~30L) — ResolvePlayerClass
  + Gameplay/Equipment/EquipmentTemplateRegistry.cs (~25L) — RegisterTemplates + lookup
  + Gameplay/Equipment/EquipmentPatternActivator.cs (~80L) — Activate/Deactivate (Strategy 8)
  + Gameplay/Equipment/EquipmentInstanceSyncer.cs (~100L) — Sync/Equip/Unequip Instance
  - Gameplay/Equipment/EquipmentManager.cs 326L → ~95L facade (MonoBehaviour 보존)

[검증]
  - 호출처 ~30 파일 시그니처 변경 0
  - NUnit 158 회귀 0
  - §15.1 SRP — 4 component 단일 책임

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — LimitedShopService 분리 (1 commit)

### 3.1 신규 3 파일

**위치**: `Scripts/Core/LimitedShop/`

| 파일 | 라인 | 책임 |
|---|---|---|
| `LimitedShopTriggerService.cs` (정적) | ~80 | HandleSummonLevelUp/ChapterFirstClear/NewUser/DailyBoss/WeeklyBoss/CharacterFirstSelect + TriggerByCondition |
| `LimitedShopOfferQuery.cs` (정적) | ~60 | IsOfferAvailable / IsOfferPurchased / FindTrigger / AppendTrigger |
| `LimitedShopRewardGranter.cs` (정적) | ~90 | ApplyRewards / GrantSpecificWeapon / GrantWeaponByRarity / MarkPurchased |

### 3.2 LimitedShopService facade (~80L)

기존 시그니처 22개 보존 — 위임 패턴.

#### Sprint 2 commit

```
5028 Sprint 2 — LimitedShopService 303L → 3 service + facade ~80L (2/4)

[변경]
  + Core/LimitedShop/LimitedShopTriggerService.cs (~80L) — 6 Handle* + TriggerByCondition
  + Core/LimitedShop/LimitedShopOfferQuery.cs (~60L) — IsAvailable/Purchased + FindTrigger/Append
  + Core/LimitedShop/LimitedShopRewardGranter.cs (~90L) — ApplyRewards/GrantWeapon/MarkPurchased
  - Core/LimitedShopService.cs 303L → ~80L facade (시그니처 22개 보존)

[검증]
  - 호출처 ~15 파일 시그니처 변경 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — StarforceService 분리 (1 commit)

### 4.1 신규 3 파일

**위치**: `Scripts/Core/Starforce/`

| 파일 | 라인 | 책임 |
|---|---|---|
| `StarforceCostTable.cs` (정적) | ~45 | GetCostForLevel + 1.5^N 곡선 const |
| `StarforceProbability.cs` (정적) | ~50 | P_FLOOR + tier 별 성공률 매핑 |
| `StarforceResult.cs` (struct) | ~15 | DTO 분리 |

### 4.2 StarforceService facade (~140L)

TryEnhance 핵심 로직 응집 (~80L) + GetLevel/Shards 등 + helper 위임.

#### Sprint 3 commit

```
5028 Sprint 3 — StarforceService 255L → 3 helper + facade ~140L (3/4)

[변경]
  + Core/Starforce/StarforceCostTable.cs (~45L) — GetCostForLevel
  + Core/Starforce/StarforceProbability.cs (~50L) — P_FLOOR + tier 매핑
  + Core/Starforce/StarforceResult.cs (~15L) — struct DTO
  - Core/StarforceService.cs 255L → ~140L facade (TryEnhance 응집 유지)

[검증]
  - StarforceServiceTests 10 NUnit 회귀 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. Sprint 4 — Validator + docs (1 commit)

```
5028 Sprint 4 — Validator 회귀 + changelog + CLAUDE.md (4/4)

[검증]
  - OOPPolicyRule warnings 16 → 13 (-3)
  - 신규 10 파일 모두 200L 이내
  - NUnit 158 회귀 0

[변경]
  - changelog_v1.md +5028 항목
  - CLAUDE.md §14 직전 5건 갱신

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 6. 의존성 그래프

```
Sprint 1 (Equipment) ─┐
Sprint 2 (LimitedShop)├─→ Sprint 4 (docs)
Sprint 3 (Starforce) ─┘
```

Sprint 1+2+3 병렬 가능 (서로 독립). 단 컴파일 안정성 위해 순차 권장.

## 7. 완료 정의 (DoD)

- [ ] EquipmentManager 326L → ~95L facade
- [ ] 4 신규 Equipment component 모두 200L 이내
- [ ] LimitedShopService 303L → ~80L facade
- [ ] 3 신규 LimitedShop service 모두 200L 이내
- [ ] StarforceService 255L → ~140L facade
- [ ] 3 신규 Starforce helper 모두 200L 이내
- [ ] 호출처 시그니처 변경 0
- [ ] NUnit 158 회귀 0
- [ ] Validator warnings 73 → 70 (-3)
- [ ] changelog + CLAUDE.md 갱신

## 8. 검증 시나리오

### 시나리오 1 — 장비 회귀
Inventory → 장비 변경 → SyncEquippedInstances 동작 → AttackPattern 활성화

### 시나리오 2 — 한정 상점 회귀
챕터 첫 클리어 → HandleChapterFirstClear → IsOfferAvailable / 구매 → ApplyRewards

### 시나리오 3 — 강화 회귀
Inventory → +N 강화 → TryEnhance → StarforceResult 반환 + 콘솔 로그

### 시나리오 4 — Validator
Validate Project → warnings 73 → 70 (-3) 확인

## 9. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| EquipmentManager Awake/Update 의존 → 분리 회귀 | facade 가 MonoBehaviour 보존 (5027 AudioManager 답습) |
| LimitedShopService 22 메서드 시그니처 | facade 위임 + grep 검증 |
| StarforceService TryEnhance 응집 → 분리 시 효과 미미 | facade 응집 유지, helper 만 분리 |
| 첫 선택 + 챕터 진입 + 인벤토리 변경 등 광범위 호출처 | 컴파일 검증 + Play 시나리오 4종 |

## 10. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 B (Top 3 service, 4 sprint / 2h50m). 5028.1 UI 후속 |
