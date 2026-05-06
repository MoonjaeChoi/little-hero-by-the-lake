# 5031 Sprint PRD — iap_v5_migration

> **PRD v1 (초안)** · 디자인: `docs/prd/5031_iap_v5_migration_prd.md`
> 옵션 B 채택 — v5 전면 재작성 + facade 보존

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | IAPService v5 재작성 (~150L) | 1 | 60분 |
| 2 | 호출처 회귀 검증 + Receipt 검증 | 1 | 30분 |
| 3 | NUnit + #pragma 제거 + docs | 1 | 30분 |
| **합계** | | **3** | **~2h** |

---

## 2. Sprint 1 — IAPService v5 재작성 (1 commit, 60분)

### 2.1 변경 내용

**파일**: `Scripts/Core/IAPService.cs` 142L → ~150L

**보존 (facade 시그니처)**:
- public Singleton + Bootstrap + IsInitialized + 3 events + ID_STARLIGHT_* + IAP 메서드 4개
- 호출처 ~5 파일 변경 0

**변경 (v5 내부)**:
```csharp
public class IAPService
{
    private StoreController _store;  // v5 (was IStoreController _store)

    private async void Initialize()
    {
        _store = UnityIAPServices.StoreController();
        _store.OnPurchasePending += HandlePurchasePending;
        _store.OnPurchaseFailed  += HandlePurchaseFailed;
        _store.OnProductsFetched += HandleProductsFetched;

        try { await _store.Connect(); }
        catch (Exception e) { Debug.LogError($"[IAPService] Connect FAILED — {e}"); return; }

        var products = new List<ProductDefinition>
        {
            new(ID_STARLIGHT_100,   ProductType.Consumable),
            new(ID_STARLIGHT_500,   ProductType.Consumable),
            new(ID_STARLIGHT_1100,  ProductType.Consumable),
            new(ID_STARLIGHT_3500,  ProductType.Consumable),
            new(ID_STARLIGHT_12000, ProductType.Consumable),
            new(ID_SEASON_PASS,     ProductType.NonConsumable),
        };
        _store.FetchProducts(products);
    }

    void HandleProductsFetched(List<Product> products)
    {
        OnReady?.Invoke();
        Debug.Log($"[IAPService] FetchProducts OK — {products.Count}");
    }

    public void InitiatePurchase(string productId)
    {
        if (_store == null) { /* error */ return; }
        _store.PurchaseProduct(productId);  // v5 (was _store.InitiatePurchase(p))
    }

    void HandlePurchasePending(PendingOrder order)
    {
        string productId = order?.Info?.PurchasedProductInfo?.firstOrDefault()?.productId;
        // ... ApplyGrant + LimitedShopService.ConfirmIAPPurchase 보존
        _store.ConfirmPurchase(order);  // v5 — 구매 확정
    }

    void HandlePurchaseFailed(FailedOrder order)
    {
        OnPurchaseFailedEvent?.Invoke(order?.Info?.firstOrDefault()?.productId, order.FailureReason.ToString());
    }
}
```

### 2.2 검증

- 컴파일 errors=0
- v5 API 시그니처 정확 검증 (Unity 6000.4.4f1)
- 호출처 5 파일 (LimitedShop / SeasonPass / Currency / StarlightShopPanel / Bootstrap) 시그니처 변경 0

#### Sprint 1 commit

```
5031 Sprint 1 — IAPService v5 재작성 (1/3)

[변경]
  - Core/IAPService.cs 142L → ~150L (facade 보존)
    * IStoreController → StoreController (v5)
    * IStoreListener 인터페이스 → 이벤트 구독 (OnPurchasePending/Failed/ProductsFetched)
    * UnityPurchasing.Initialize → Connect 비동기 + FetchProducts 3단계
    * ConfigurationBuilder → ProductDefinition list
    * ProcessPurchase → HandlePurchasePending(PendingOrder)
    * _store.InitiatePurchase → _store.PurchaseProduct
    * #pragma warning disable CS0618 제거 가능 (전체 v5 API)
    * Singleton + IsInitialized + 3 events + ID_* const + 메서드 4개 모두 보존

[검증]
  - 컴파일 errors=0
  - 호출처 5 파일 시그니처 변경 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — 호출처 회귀 + Receipt 검증 (1 commit, 30분)

### 3.1 호출처 검증 (코드 변경 0 추정)

| 파일 | 호출 | 호환 |
|---|---|---|
| `LimitedShopService.cs` | `IAPService.Instance.InitiatePurchase(productId)` | ✅ 시그니처 보존 |
| 콜백 `LimitedShopService.ConfirmIAPPurchase(productId)` | `HandlePurchasePending` 내부 호출 | ✅ |
| `IAPService.Bootstrap()` | 게임 시작 1회 | ✅ |
| `StarlightShopPanel` | `IAPService.Instance.GetLocalizedPrice(id)` | ✅ |
| `SeasonPassService.GrantPremium()` | `ApplyGrant(SEASON_PASS)` | ✅ |

### 3.2 Receipt 검증 (CrossPlatformValidator)

v5 호환 가능성 — `Validate(receipt)` 메서드가 PendingOrder 의 receipt 필드 처리. v5 에서는 `order.CartOrderInfo.Receipt` 또는 유사 필드. 검증 후 미호환 시 5031.1 후속.

### 3.3 검증

- 컴파일 errors=0
- LimitedShop 비동기 콜백 정상 작동
- Receipt 검증 호환 (또는 5031.1 backlog)

#### Sprint 2 commit

```
5031 Sprint 2 — 호출처 회귀 + Receipt 검증 (2/3)

[검증]
  - 호출처 5 파일 시그니처 변경 0
  - LimitedShopService.ConfirmIAPPurchase 비동기 콜백 정상
  - SeasonPassService.GrantPremium 정상

[변경]
  - (Receipt 검증 v5 호환 시) ReceiptValidator 호출 갱신
  - (미호환 시) 5031.1 backlog

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — NUnit + docs (1 commit, 30분)

### 4.1 NUnit Test (선택)

**파일**: `Tests/Editor/IAPServiceTests.cs` (~30L, NUnit 3+)
- `IAPService_ProductIds_Defined` (6 const)
- `IAPService_GrantStarlight_500_Returns550` (보너스 50)
- `IAPService_GrantStarlight_12000_Returns13500` (보너스 1500)

NUnit 누적: 164 → 167 (+3).

### 4.2 #pragma 제거

5028 cascade 옵션 A 의 `#pragma warning disable CS0618` 제거 (v5 API 전체 → obsolete 0).

### 4.3 docs

- `changelog_v1.md` +5031
- `CLAUDE.md` §10 외부 라이브러리 행 갱신 (com.unity.purchasing 5.0.0 v5 API 사용)
- §14 직전 5건

#### Sprint 3 commit

```
5031 Sprint 3 — NUnit + #pragma 제거 + docs (3/3)

[변경]
  + Tests/Editor/IAPServiceTests.cs (~30L, NUnit 3)
  - Core/IAPService.cs #pragma warning disable CS0618 제거
  - changelog_v1.md +5031
  - CLAUDE.md §10 + §14 갱신

[NUnit 누적] 164 → 167

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. 의존성 그래프

```
Sprint 1 (재작성)
   ↓
Sprint 2 (회귀 검증)
   ↓
Sprint 3 (Test + docs)
```

순차 (Sprint 1 컴파일 정합 필요).

## 6. 완료 정의 (DoD)

- [ ] IAPService v5 API 전환 (StoreController + 이벤트)
- [ ] 호출처 5 파일 시그니처 변경 0
- [ ] CS0618 warning 0 (#pragma 제거)
- [ ] LimitedShopService.ConfirmIAPPurchase 비동기 콜백 정상
- [ ] SeasonPassService.GrantPremium 정상
- [ ] Receipt 검증 v5 호환 (또는 5031.1 backlog)
- [ ] NUnit 164 → 167
- [ ] changelog + CLAUDE.md §10 + §14

## 7. 검증 시나리오

### 시나리오 1 — 컴파일
errors=0 + warnings (CS0618) 0

### 시나리오 2 — Editor Mode Fake Store
1. Bootstrap → IAPService.Instance.IsInitialized
2. Console: `[IAPService] FetchProducts OK — 6`

### 시나리오 3 — Play Mode 가상 구매 (Editor Fake Store)
1. StarlightShopPanel → 별빛 100 패키지 구매
2. ApplyGrant → CurrencyService.GrantStarlight(100)
3. Console: `[IAPService] +100⋆ 적립`

### 시나리오 4 — TestFlight (Phase 4)
실 iOS App Store 구매 검증 (TestFlight 단계로 미룸)

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| v5 API 시그니처 정확 모름 (PRD 추정) | Sprint 1 Editor 컴파일 검증 |
| Receipt 검증 v5 호환 미확정 | Sprint 2 검증, 미호환 시 5031.1 |
| Editor Fake Store v5 동작 | Sprint 2 Editor Mode 검증 |
| iOS App Store 실 구매 | TestFlight 단계 (5028 후속) |

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 B (3 sprint / 2h) |
