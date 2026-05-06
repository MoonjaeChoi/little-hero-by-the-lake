# Sprint Contract: 5031

## C1 컴파일

errors=0, 신규 6 파일 + facade 갱신, CS0618 warning 0

## C2 NUnit

164 → 169 (+5 IAPServiceTests Mock 활용)

## C3 Validator

OOPPolicyRule warnings 변동 ≤ 0

## C4 Reflection

- `LittleHero.IAPService` (Singleton) — Bootstrap + Instance + 4 메서드 + 3 events 보존
- `LittleHero.IIAPGateway` (interface) — Connect/FetchProducts/PurchaseProduct + 이벤트
- `LittleHero.UnityIAPGateway` IIAPGateway 구현 (v5 StoreController wrapper)
- `LittleHero.MockIAPGateway` IIAPGateway 구현 (Test only)
- `LittleHero.IAPProductCatalog` 정적 — ID 6 + ProductDefinitions + ApplyGrant
- `LittleHero.IAPPurchaseHandler` (POCO)

## C5 SO 검증

영향 0

## C6 Play 시나리오

### 시나리오 1 — 컴파일
errors=0, CS0618 0

### 시나리오 2 — Editor Fake Store
Bootstrap → IsInitialized + 6 product fetched

### 시나리오 3 — Editor 가상 구매
별빛 100 패키지 → ApplyGrant → +100⋆

### 시나리오 4 — Receipt v5 호환 검증
ReceiptValidator.Validate(receipt) v5 PendingOrder 호환 (또는 5031.1 backlog)

### 시나리오 5 — TestFlight (Phase 4)
실 iOS 구매 — TestFlight 단계 후속

## 합격 기준

최소: C1+C3+C4 + 시나리오 1+2
완전: 모든 + 시나리오 3+4
