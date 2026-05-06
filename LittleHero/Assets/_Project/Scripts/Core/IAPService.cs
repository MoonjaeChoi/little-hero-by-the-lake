using System;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5031 facade — Unity IAP v5 (com.unity.purchasing 5.0.0).
    /// IIAPGateway 위임 (D2=B Mock 친화).
    /// 위임: IAPProductCatalog (ID + ApplyGrant) + Gateway (StoreController wrapper).
    /// </summary>
    public class IAPService
    {
        // ===== Product IDs (호출처 호환 보존) =====
        public const string ID_STARLIGHT_100   = IAPProductCatalog.ID_STARLIGHT_100;
        public const string ID_STARLIGHT_500   = IAPProductCatalog.ID_STARLIGHT_500;
        public const string ID_STARLIGHT_1100  = IAPProductCatalog.ID_STARLIGHT_1100;
        public const string ID_STARLIGHT_3500  = IAPProductCatalog.ID_STARLIGHT_3500;
        public const string ID_STARLIGHT_12000 = IAPProductCatalog.ID_STARLIGHT_12000;
        public const string ID_SEASON_PASS     = IAPProductCatalog.ID_SEASON_PASS;

        public static IAPService Instance { get; private set; }

        public bool IsInitialized => _gateway != null && _gateway.IsReady;
        public event Action OnReady;
        public event Action<string, string> OnPurchaseFailedEvent;
        public event Action<string> OnPurchaseSucceededEvent;

        private IIAPGateway _gateway;

        /// <summary>Production entry — UnityIAPGateway 주입.</summary>
        public static void Bootstrap()
        {
            if (Instance != null) return;
            Instance = new IAPService();
            Instance.Initialize(new UnityIAPGateway());
        }

        /// <summary>Test entry — Mock 주입 가능 (D2=B).</summary>
        public static void BootstrapForTest(IIAPGateway gateway)
        {
            Instance = new IAPService();
            Instance.Initialize(gateway);
        }

        private async void Initialize(IIAPGateway gateway)
        {
            _gateway = gateway;
            _gateway.OnReady += HandleReady;
            _gateway.OnPurchaseSucceeded += HandleSucceeded;
            _gateway.OnPurchaseFailed    += HandleFailed;
            await _gateway.InitializeAsync(IAPProductCatalog.GetProductDefinitions());
        }

        private void HandleReady() => OnReady?.Invoke();

        private void HandleSucceeded(string productId, string receipt)
        {
            // E5 — Receipt 검증 (5031 D1=A v5 호환)
            if (!string.IsNullOrEmpty(receipt))
            {
                bool validated = ReceiptValidator.Validate(receipt);
                if (!validated) Debug.LogWarning($"[IAPService] receipt validation FAILED — {productId} (grant 진행)");
            }
            IAPProductCatalog.ApplyGrant(productId);
            LimitedShopService.ConfirmIAPPurchase(productId);
            OnPurchaseSucceededEvent?.Invoke(productId);
        }

        private void HandleFailed(string productId, string reason)
        {
            Debug.LogError($"[IAPService] purchase FAILED — {productId} ({reason})");
            OnPurchaseFailedEvent?.Invoke(productId, reason);
        }

        public void InitiatePurchase(string productId)
        {
            if (_gateway == null) { Debug.LogError("[IAPService] not initialized"); return; }
            _gateway.PurchaseProduct(productId);
        }

        public string GetLocalizedPrice(string productId)
            => _gateway != null ? _gateway.GetLocalizedPrice(productId) : string.Empty;
    }
}
