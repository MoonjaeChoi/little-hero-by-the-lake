# 5028 oop_class_split_phase2 — 디자인 문서

> **목적**: 5027 후속 — Top 3 production service + UI Panel 5 분리. ProjectValidator 73 → 65 (-8).
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5027 evaluation 후 잔여 73 warnings — 200줄+ 16 파일 source 가 여전히 큼. 5027.1+5027.2 통합 → 5028 단일 PRD |

## 1. 현재 시스템 분석

### 1.1 잔여 200줄+ 16 파일 (5027 후)

| 레이어 | 개수 | 파일 |
|---|---|---|
| Editor | 3 | CurveSimulator 970L / PhaseFContentSeed 212L / LimitedShopSeed 203L (Editor 도구 — 우선순위 X) |
| **Core (Service)** | **3** | **LimitedShopService 303L / StarforceService 255L / StarlightGrantService 214L** |
| **Gameplay** | **5** | **EquipmentManager 326L** / EnemyController 284L / PlayerStats 248L / ItemDropService 232L / ChapterRunner 220L |
| **UI** | **5** | **EquipmentCardPanel 230L / InventorySelectionPanel 224L / GachaResultPanel 213L / GachaPanel 204L / EndgameMenuPanel 203L** |

### 1.2 5028 대상 8 파일 (production 우선)

#### Top 3 Service

| 파일 | 라인 | 책임 그룹 |
|---|---|---|
| EquipmentManager 326L | MonoBehaviour | Equip/Unequip + SyncInstances + ActivatePattern + ResolvePlayerClass (정적) + RegisterTemplates (정적) |
| LimitedShopService 303L | 정적 | Initialize/Reset + Handle* (6 trigger) + IsOfferAvailable/Purchased + ConfirmIAPPurchase + ApplyRewards |
| StarforceService 255L | 정적 | GetLevel/Cost/IsMax + TryEnhance + Shards/GrantShards + StarforceResult struct |

#### UI Panel 5

| 파일 | 라인 | 패턴 |
|---|---|---|
| EquipmentCardPanel 230L | 슬롯 카드 + Detail panel + Enhance modal 진입 |
| InventorySelectionPanel 224L | 페이징 + 썸네일 + 옵션 표시 (~v2) |
| GachaResultPanel 213L | 단판/10연차 결과 슬롯 표시 + 등급별 색상 |
| GachaPanel 204L | 단판/10연차 버튼 + 비용 표시 + 결과 패널 호출 |
| EndgameMenuPanel 203L | Daily 7 + Weekly 4 + Infinite 1 + Hardcore 진입 |

### 1.3 위반 영향

- ProjectValidator OOP warnings 73 (5027 후)
- Top 8 분리 시: -8 (16 → 8 — Editor 도구 3 제외)
- 잔존 Gameplay 5 + UI 5028 외 0 = 8 (Editor 만)

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | Top 3 service + UI Panel 5 모두 분리 (8 파일) | warnings 73 → 65 (-8), 출시 전 큰 진전 | 8 sprint × 30~60분 = ~5h, 회귀 위험 ↑ |
| **B (★권장)** | Top 3 service 만 (-3) | 안전, 5027 답습, UI 후속 | UI 5는 5028.1 후속 필요 |
| C | UI Panel 만 (-5) | UI MVC 정착 | service 효과 우선 (출시 전 안정성) |
| D | 점진 — Top 1 EquipmentManager 만 분리 | 가장 큰 hot path 만 | 효과 -1 미미 |

★ Claude 권장: **B** — 5027 답습 (Top 우선 + facade 보존). UI Panel 분리는 MVC 패턴 정착 필요 (별도 PRD 5028.1 후속).

근거:
- service 분리는 호출처 시그니처 보존 (facade) → 회귀 안전
- UI Panel 분리는 Inspector wiring 변경 위험 (씬 _references mismatch)
- 출시 전 안정성 우선 — UI 분리는 5028.1 또는 출시 후

## 3. 권장 설계 (옵션 B — Top 3 service)

### 3.1 EquipmentManager 326L → 분리

| 신규 파일 | 책임 | 라인 |
|---|---|---|
| `Gameplay/Equipment/EquipmentClassResolver.cs` (정적) | ResolvePlayerClass + class change 감지 | ~30L |
| `Gameplay/Equipment/EquipmentTemplateRegistry.cs` (정적) | RegisterTemplates + lookup | ~25L |
| `Gameplay/Equipment/EquipmentPatternActivator.cs` (POCO) | ActivatePattern / DeactivatePattern (Strategy hook) | ~80L |
| `Gameplay/Equipment/EquipmentInstanceSyncer.cs` (POCO) | SyncEquippedInstances / EquipInstance / UnequipInstance | ~100L |
| **`Gameplay/Equipment/EquipmentManager.cs`** (facade — MonoBehaviour) | Awake/Update + Equip/Unequip + 위임 | **~95L** |

### 3.2 LimitedShopService 303L → 분리

| 신규 파일 | 책임 | 라인 |
|---|---|---|
| `Core/LimitedShop/LimitedShopTriggerService.cs` (정적) | HandleSummonLevelUp/ChapterFirstClear/NewUser/DailyBoss/WeeklyBoss/CharacterFirstSelect + TriggerByCondition | ~80L |
| `Core/LimitedShop/LimitedShopOfferQuery.cs` (정적) | IsOfferAvailable / IsOfferPurchased + FindTrigger / AppendTrigger | ~60L |
| `Core/LimitedShop/LimitedShopRewardGranter.cs` (정적) | ApplyRewards + GrantSpecificWeapon + GrantWeaponByRarity + MarkPurchased | ~90L |
| **`Core/LimitedShopService.cs`** (facade — 정적) | Initialize/Reset + ConfirmIAPPurchase + 위임 | **~80L** |

### 3.3 StarforceService 255L → 분리

| 신규 파일 | 책임 | 라인 |
|---|---|---|
| `Core/Starforce/StarforceCostTable.cs` (정적) | GetCostForLevel + 곡선 (1.5^N const) | ~45L |
| `Core/Starforce/StarforceProbability.cs` (정적) | P_FLOOR + tier 별 성공률 매핑 | ~50L |
| `Core/Starforce/StarforceResult.cs` (struct) | DTO (previousLevel/newLevel/shardCost) | ~15L |
| **`Core/StarforceService.cs`** (facade — 정적) | GetLevel/IsMaxLevel + TryEnhance (핵심) + Shards | **~140L** |

### 3.4 §15 정합

- §15.1 SRP: ✅ 12 신규 파일 단일 책임
- §15.6 100L: 모두 100~140L (200L 룰 통과 ✅)
- §15.7 DI: facade 가 의존성 주입 — 5023/5027 답습
- §15.9 Test: 기존 NUnit 158 회귀 0 (facade 시그니처 보존)
- §15.11 안티 패턴: ✅

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | EquipmentManager 326L → 4 component + facade | 1 | 60분 |
| 2 | LimitedShopService 303L → 3 service + facade | 1 | 50분 |
| 3 | StarforceService 255L → 3 helper + facade | 1 | 40분 |
| 4 | Validator 회귀 + changelog + CLAUDE.md | 1 | 20분 |
| **합계** | | **4** | **2h 50m** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| EquipmentManager MonoBehaviour Awake/Update 의존 → 분리 시 회귀 | facade 가 Awake/Update 보존, component POCO 가 reference 받음 (5027 답습) |
| LimitedShopService 정적 메서드 22개 → 호출처 grep mismatch | facade 시그니처 보존 + 정적 위임 |
| StarforceService TryEnhance 핵심 로직 응집도 | facade 에 TryEnhance 유지, helper 만 분리 |
| Inventory + Gacha + Character 첫 선택 등 호출처 광범위 | grep 검증 + 컴파일 errors=0 검증 |

## 6. 5028.1 backlog (선택)

UI Panel 5 분리 (-5 warnings):
- EquipmentCardPanel → CardView + EnhanceModalController + DetailViewModel
- InventorySelectionPanel → GridPager + ThumbnailRenderer + OptionsView
- GachaResultPanel → ResultSlotRenderer + RarityColorMap + AnimationController
- GachaPanel → CostDisplayer + RollButtonController
- EndgameMenuPanel → ModeSelector + UnlockGateChecker

총 5 sprint / ~3h. 출시 전 우선순위 ↓ (Inspector wiring 변경 위험).

## 7. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 B 권장 (Top 3 service, 4 sprint / 2h50m). UI Panel 5028.1 후속 |

## 8. Sources

- 5027 evaluation Validator 75 → 73 (-2) 검증
- 5022 Validator 분리 + 5023 MetaProgression 분리 + 5027 GachaService/AudioManager 분리 답습
- §15.1 SRP + §15.6 100줄 룰
