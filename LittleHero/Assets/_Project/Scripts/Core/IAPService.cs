// 5028 cascade — IAP v5 마이그레이션 5031 후속 (옵션 A suppress).
// CS0618: IStoreController / IExtensionProvider / InitializationFailureReason 등 v4 API obsolete.
// v4 API 정상 작동 (Unity IAP 5.0.0 backward 호환), 5031 PRD 에서 v5 IStoreService 마이그레이션.
#pragma warning disable CS0618
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace LittleHero
{
    /// <summary>
    /// E1+E2 — Unity IAP 통합 (monetization_v1 §7.4).
    /// 5 별빛 패키지 + 1 시즌 패스 등록. App Store/Google Play 양쪽.
    ///
    /// 부트스트랩: 게임 시작 시 1회 <see cref="Bootstrap"/> 호출 → CodelessIAP 가 아닌
    /// 직접 IStoreListener 구현으로 product 카탈로그 등록 + 구매 완료 시 별빛 적립.
    ///
    /// 영수증 검증(E5): <see cref="CrossPlatformValidator"/> 사용 (Tangle 키는 Window > Services > IAP 에서 생성).
    /// </summary>
    public class IAPService : IDetailedStoreListener
    {
        // --- Product IDs (monetization §7.4) ---
        public const string ID_STARLIGHT_100   = "com.littlehero.starlight_100";    // 1,200원 → 100⋆
        public const string ID_STARLIGHT_500   = "com.littlehero.starlight_500";    // 5,500원 → 500+50⋆
        public const string ID_STARLIGHT_1100  = "com.littlehero.starlight_1100";   // 11,000원 → 1100+120⋆
        public const string ID_STARLIGHT_3500  = "com.littlehero.starlight_3500";   // 33,000원 → 3500+400⋆
        public const string ID_STARLIGHT_12000 = "com.littlehero.starlight_12000";  // 110,000원 → 12000+1500⋆
        public const string ID_SEASON_PASS     = "com.littlehero.season_pass";      // 3,300원 → 시즌 패스 프리미엄

        public static IAPService Instance { get; private set; }

        public bool IsInitialized => _store != null;
        public event Action OnReady;
        public event Action<string, string> OnPurchaseFailedEvent; // (productId, reason)
        public event Action<string> OnPurchaseSucceededEvent; // productId

        private IStoreController _store;
        private IExtensionProvider _extensions;

        public static void Bootstrap()
        {
            if (Instance != null) return;
            Instance = new IAPService();
            Instance.Initialize();
        }

        private void Initialize()
        {
            var module = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(module);

            builder.AddProduct(ID_STARLIGHT_100,   ProductType.Consumable);
            builder.AddProduct(ID_STARLIGHT_500,   ProductType.Consumable);
            builder.AddProduct(ID_STARLIGHT_1100,  ProductType.Consumable);
            builder.AddProduct(ID_STARLIGHT_3500,  ProductType.Consumable);
            builder.AddProduct(ID_STARLIGHT_12000, ProductType.Consumable);
            builder.AddProduct(ID_SEASON_PASS,     ProductType.NonConsumable);

            UnityPurchasing.Initialize(this, builder);
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _store = controller;
            _extensions = extensions;
            OnReady?.Invoke();
            Debug.Log("[IAPService] Initialize OK — products: " + (_store?.products?.all?.Length ?? 0));
        }

        public void OnInitializeFailed(InitializationFailureReason error)
            => OnInitializeFailed(error, null);

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.LogError($"[IAPService] Initialize FAILED — {error} ({message})");
        }

        public void InitiatePurchase(string productId)
        {
            if (_store == null) { Debug.LogError("[IAPService] not initialized"); return; }
            var p = _store.products.WithID(productId);
            if (p == null || !p.availableToPurchase)
            {
                Debug.LogError($"[IAPService] product unavailable — {productId}");
                OnPurchaseFailedEvent?.Invoke(productId, "unavailable");
                return;
            }
            _store.InitiatePurchase(p);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            string productId = args?.purchasedProduct?.definition?.id;
            if (string.IsNullOrEmpty(productId)) return PurchaseProcessingResult.Complete;

            // E5 — 영수증 검증 시도 (검증 실패해도 grant 는 일단 적용; 운영 단계에서 엄격화 가능)
            bool validated = ReceiptValidator.Validate(args.purchasedProduct.receipt);
            if (!validated)
                Debug.LogWarning($"[IAPService] receipt validation FAILED — {productId} (grant 진행)");

            ApplyGrant(productId);
            // PIVOT — 한정 상점 IAP 비동기 콜백 (limited_shop_v1.md §6.2)
            LimitedShopService.ConfirmIAPPurchase(productId);
            OnPurchaseSucceededEvent?.Invoke(productId);
            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
            => OnPurchaseFailedEvent?.Invoke(product?.definition?.id, failureReason.ToString());

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
            => OnPurchaseFailedEvent?.Invoke(product?.definition?.id, failureDescription?.reason.ToString());

        private static void ApplyGrant(string productId)
        {
            int starlight = 0;
            switch (productId)
            {
                case ID_STARLIGHT_100:   starlight = 100;   break;
                case ID_STARLIGHT_500:   starlight = 550;   break;  // 500 + 50 보너스
                case ID_STARLIGHT_1100:  starlight = 1220;  break;  // 1100 + 120
                case ID_STARLIGHT_3500:  starlight = 3900;  break;  // 3500 + 400
                case ID_STARLIGHT_12000: starlight = 13500; break;  // 12000 + 1500
                case ID_SEASON_PASS:
                    SeasonPassService.GrantPremium();
                    return;
            }
            if (starlight > 0)
            {
                CurrencyService.GrantStarlight(starlight);
                Debug.Log($"[IAPService] +{starlight}⋆ 적립 ({productId})");
            }
        }

        // ===== 가격 조회 (UI 표기용) =====

        public string GetLocalizedPrice(string productId)
        {
            if (_store == null) return string.Empty;
            var p = _store.products.WithID(productId);
            return p?.metadata?.localizedPriceString ?? string.Empty;
        }
    }
}
#pragma warning restore CS0618
