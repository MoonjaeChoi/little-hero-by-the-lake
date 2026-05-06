# Sprint Contract: 5028

## C1 컴파일

errors=0, 신규 10 파일 + facade 3 갱신

## C2 NUnit

158 회귀 0 (StarforceServiceTests 10 + EquipmentManagerTests 7 보존)

## C3 Validator

OOPPolicyRule warnings 16 → 13 (-3)
종합 73 → 70

## C4 Reflection

- `LittleHero.EquipmentManager` (MonoBehaviour) — Equip/Unequip/SyncEquippedInstances/EquipInstance/UnequipInstance + 정적 ResolvePlayerClass/RegisterTemplates
- `LittleHero.LimitedShopService` (정적) — Initialize/Reset + 6 Handle* + IsOfferAvailable/Purchased + ConfirmIAPPurchase
- `LittleHero.StarforceService` (정적) — GetLevel/GetCost/IsMaxLevel/TryEnhance/Shards/GrantShards
- 신규 10 클래스 모두 type 존재 검증

## C5 SO 검증

영향 0

## C6 Play 시나리오

### 시나리오 1 — 장비 회귀
Inventory → 장비 변경 → SyncEquippedInstances → AttackPattern 활성화

### 시나리오 2 — 한정 상점 회귀
챕터 첫 클리어 → HandleChapterFirstClear → IsOfferAvailable / 구매

### 시나리오 3 — 강화 회귀
Inventory → +N 강화 → TryEnhance → StarforceResult 반환

### 시나리오 4 — Validator
Validate Project → warnings 73 → 70 (-3)

## 합격 기준

최소: C1+C3+C4 + 시나리오 1+2+3
완전: 모든 카테고리 + 시나리오 4
