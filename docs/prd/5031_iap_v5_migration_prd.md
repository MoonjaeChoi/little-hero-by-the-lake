# 5031 iap_v5_migration — 디자인 문서

> **목적**: Unity IAP v4 → v5 마이그레이션 — IAPService 142L 전면 재작성
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5028 cascade — IAPService.cs CS0618 obsolete warning 발견. 옵션 A `#pragma warning disable` 임시 suppress + 5031 PRD 작성 결정 |

## 1. 현재 시스템 분석

### 1.1 IAPService.cs 142L (v4 API 사용)

| v4 API | 사용 위치 | v5 대체 |
|---|---|---|
| `IStoreController` | L39, L66 | `StoreController` (UnityIAPServices.StoreController()) |
| `IExtensionProvider` | L40, L64 | (사용 X — 대부분 deprecated) |
| `IDetailedStoreListener` | L22 인터페이스 구현 | 이벤트 핸들러 (선택적) |
| `InitializationFailureReason` | L72, L75 | 신규 reason enum (v5) |
| `ConfigurationBuilder.Instance` | L52 | `CatalogProvider` 또는 `ProductDefinition` 리스트 |
| `UnityPurchasing.Initialize` | L61 | 비동기 3단계 (Connect / FetchProducts / FetchPurchases) |
| `ProcessPurchase(PurchaseEventArgs)` | L93 | `OnPurchasePending(PendingOrder)` 이벤트 |
| `OnPurchaseFailed(Product, PurchaseFailureReason)` | L110 | `OnPurchaseFailed` 이벤트 (v5 시그니처) |
| `_store.InitiatePurchase(p)` | L90 | `m_StoreController.PurchaseProduct(productId)` |
| `_store.products.WithID(productId)` | L83, L142 | v5 `Product` API (호환 가능성) |

### 1.2 호출처 (v5 회귀 위험)

| 호출처 | 의존 |
|---|---|
| `LimitedShopService.ConfirmIAPPurchase(productId)` | L105 — 비동기 콜백, **회귀 0** (시그니처 보존) |
| `SeasonPassService.GrantPremium()` | L127 (시즌 패스 IAP) |
| `CurrencyService.GrantStarlight(amount)` | L132 (별빛 5 패키지) |
| UI Panel (StarlightShopPanel) | `IAPService.Instance.GetLocalizedPrice` (L139) |
| `ReceiptValidator.Validate(receipt)` | L99 — CrossPlatformValidator 의존 (v5 호환 검증 필요) |

### 1.3 v5 핵심 변화

**v5 비동기 3단계 초기화**:
```csharp
async void Initialize() {
    _store = UnityIAPServices.StoreController();
    await _store.Connect();
    var products = new List<ProductDefinition> { ... };
    _store.FetchProducts(products);
}
void OnProductsFetched(List<Product> products) { _store.FetchPurchases(); }
```

**v5 이벤트 기반**:
```csharp
_store.OnPurchasePending += OnPurchasePending;
_store.OnPurchasesFetched += OnPurchasesFetched;
```

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | v5 전면 재작성 (IAPService 142L → ~150L) | 미래 호환, warning 0 | Receipt 검증 정합 검증 필요 |
| **B (★권장)** | v5 전면 재작성 + facade 보존 (호출처 시그니처 0) | 회귀 안전, 점진 검증 | 작업 시간 약간 증가 |
| C | `#pragma` 그대로 (5031 폐기, Phase 4 출시 후 재검토) | 0 작업 | warning 가시성 ↓, v4 폐기 임박 시 부담 |

★ Claude 권장: B — 5028 답습 (facade 보존, 회귀 안전)

## 3. 권장 설계 (옵션 B)

### 3.1 IAPService 재작성 (~150L)

**보존 (호출처 호환)**:
- `IAPService.Instance` Singleton
- `Bootstrap()` 정적
- `IsInitialized`, `OnReady`, `OnPurchaseFailedEvent`, `OnPurchaseSucceededEvent`
- `InitiatePurchase(string productId)`
- `GetLocalizedPrice(string productId)`
- ID_STARLIGHT_* / ID_SEASON_PASS const

**변경 (v5 내부)**:
- `IStoreController _store` → `StoreController _store`
- `IStoreListener` 인터페이스 구현 → 이벤트 구독
- `OnInitialized` / `OnInitializeFailed` 콜백 → `Connect().ContinueWith()`
- `ProcessPurchase` → `OnPurchasePending`
- `_store.InitiatePurchase` → `_store.PurchaseProduct`

### 3.2 Receipt 검증 (E5)

`CrossPlatformValidator` 가 v5 호환 검증 — Unity 5.0.0 기준 backward 가능성. 검증 후속.

### 3.3 §15 정합

- §15.1 SRP: ✅ IAP 단일 책임 유지
- §15.6 100L: △ ~150L 권장 초과 (200L 룰 통과)
- §15.7 DI: ✅ Singleton + Initialize 명시
- §15.9 Test: NUnit IAP Editor Mock (Unity IAP Fake Store 활용)

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | IAPService v5 재작성 (Connect + FetchProducts + 이벤트) | 1 | 60분 |
| 2 | 호출처 회귀 검증 (LimitedShop / SeasonPass / UI) + Receipt 검증 정합 | 1 | 30분 |
| 3 | NUnit + #pragma 제거 + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **3** | **~2h** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| v5 API 시그니처 mismatch (Unity 5.0.0 정확 버전) | Unity Editor 컴파일 검증 우선 |
| CrossPlatformValidator (Receipt) v5 호환 | Sprint 2 검증, 미호환 시 5031.1 후속 |
| LimitedShopService.ConfirmIAPPurchase 비동기 callback | 시그니처 보존 (string productId), 내부 호출 보존 |
| Editor Mode Fake Store v5 동작 | Sprint 2 Editor Mode Play 검증 |
| iOS App Store + Google Play 동시 검증 (Phase 4 TestFlight 시점) | TestFlight 단계 사용자 검증 |

## 6. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 B 권장 (facade 보존, 3 sprint / 2h) |

## 7. Sources

- Unity IAP v5 마이그레이션 가이드: docs.unity.com/ugs/en-us/manual/iap/manual/upgrade-to-iap-v5
- 5028 cascade fix (IAPService 옵션 A pragma suppress)
- monetization_v1.md §7.4 — 5 별빛 패키지 + 1 시즌 패스
