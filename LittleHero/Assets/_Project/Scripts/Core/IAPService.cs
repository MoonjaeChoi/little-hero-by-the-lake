using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleHero
{
    /// <summary>
    /// 5031 facade — Unity IAP v5 (com.unity.purchasing 5.0.0).
    /// 5028 cascade 후속 — IAP v4 → v5 마이그레이션 + facade 보존.
    ///
    /// 비동기 3단계: Connect → FetchProducts → 이벤트 라우팅.
    /// 위임: IAPProductCatalog (ID + ApplyGrant) + IAPPurchaseHandler (Pending/Failed).
    /// </summary>
    public class IAPService
    {
        // ===== Product IDs (호출처 호환 — IAPService.ID_* 보존) =====
        public const string ID_STARLIGHT_100   = IAPProductCatalog.ID_STARLIGHT_100;
        public const string ID_STARLIGHT_500   = IAPProductCatalog.ID_STARLIGHT_500;
        public const string ID_STARLIGHT_1100  = IAPProductCatalog.ID_STARLIGHT_1100;
        public const string ID_STARLIGHT_3500  = IAPProductCatalog.ID_STARLIGHT_3500;
        public const string ID_STARLIGHT_12000 = IAPProductCatalog.ID_STARLIGHT_12000;
        public const string ID_SEASON_PASS     = IAPProductCatalog.ID_SEASON_PASS;

        public static IAPService Instance { get; private set; }

        public bool IsInitialized => _store != null && _productsFetched;
        public event Action OnReady;
        public event Action<string, string> OnPurchaseFailedEvent;
        public event Action<string> OnPurchaseSucceededEvent;

        private StoreController _store;
        private IAPPurchaseHandler _handler;
        private bool _productsFetched;

        public static void Bootstrap()
        {
            if (Instance != null) return;
            Instance = new IAPService();
            Instance.Initialize();
        }

        private async void Initialize()
        {
            _store = UnityIAPServices.StoreController();
            _handler = new IAPPurchaseHandler(_store);
            _handler.OnPurchaseSucceeded += id => OnPurchaseSucceededEvent?.Invoke(id);
            _handler.OnPurchaseFailed    += (id, reason) => OnPurchaseFailedEvent?.Invoke(id, reason);

            _store.OnPurchasePending += _handler.HandlePending;
            _store.OnPurchaseFailed  += _handler.HandleFailed;
            _store.OnProductsFetched += HandleProductsFetched;

            try
            {
                await _store.Connect();
                _store.FetchProducts(IAPProductCatalog.GetProductDefinitions());
            }
            catch (Exception e)
            {
                Debug.LogError($"[IAPService] Connect/FetchProducts FAILED — {e.Message}");
            }
        }

        private void HandleProductsFetched(System.Collections.Generic.List<Product> products)
        {
            _productsFetched = true;
            OnReady?.Invoke();
            Debug.Log($"[IAPService] FetchProducts OK — {products?.Count ?? 0} products");
        }

        public void InitiatePurchase(string productId)
        {
            if (_store == null) { Debug.LogError("[IAPService] not initialized"); return; }
            _store.PurchaseProduct(productId);
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_store == null) return string.Empty;
            var p = _store.GetProductById(productId);
            return p?.metadata?.localizedPriceString ?? string.Empty;
        }
    }
}
