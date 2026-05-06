using System.Collections.Generic;

namespace LittleHero
{
    /// <summary>
    /// 5028 facade — 한정 상점 (limited_shop_v1.md §6).
    /// 위임: TriggerService / OfferQuery / RewardGranter. TryPurchase 핵심 로직 응집.
    /// </summary>
    public static class LimitedShopService
    {
        private static LimitedShopOffer[] _catalog;
        private static GachaPool _gachaPool;
        private static bool _initialized;
        private static string _pendingIapOfferId;

        public static bool IsInitialized => _initialized;

        public static void Initialize(LimitedShopOffer[] catalog, GachaPool gachaPool = null)
        {
            _catalog = catalog ?? new LimitedShopOffer[0];
            _gachaPool = gachaPool;
            _initialized = true;
            EnsureFirstLaunchRecorded();
        }

        public static void Reset()
        {
            _catalog = null;
            _initialized = false;
        }

        // ===== 트리거 hook (위임) =====
        public static void HandleSummonLevelUp(int newLevel)        { if (_initialized) LimitedShopTriggerService.HandleSummonLevelUp(_catalog, newLevel); }
        public static void HandleChapterFirstClear(int chapterN)    { if (_initialized) LimitedShopTriggerService.HandleChapterFirstClear(_catalog, chapterN); }
        public static void HandleNewUser()                          { if (_initialized) LimitedShopTriggerService.HandleNewUser(_catalog); }
        public static void HandleDailyBossClear()                   { if (_initialized) LimitedShopTriggerService.HandleDailyBossClear(_catalog); }
        public static void HandleWeeklyBossClear()                  { if (_initialized) LimitedShopTriggerService.HandleWeeklyBossClear(_catalog); }
        public static void HandleCharacterFirstSelect(string id)    { if (_initialized) LimitedShopTriggerService.HandleCharacterFirstSelect(_catalog, id); }

        // ===== 조회 (위임) =====
        public static List<LimitedShopOffer> GetActiveOffers()      => _initialized ? LimitedShopOfferQuery.GetActiveOffers(_catalog) : new List<LimitedShopOffer>();
        public static bool IsOfferAvailable(LimitedShopOffer offer) => LimitedShopOfferQuery.IsOfferAvailable(offer);
        public static bool IsOfferPurchased(LimitedShopOffer offer) => LimitedShopOfferQuery.IsOfferPurchased(offer);
        public static long GetExpirationUnix(LimitedShopOffer offer) => LimitedShopOfferQuery.GetExpirationUnix(offer);
        public static long GetSecondsRemaining(LimitedShopOffer offer) => LimitedShopOfferQuery.GetSecondsRemaining(offer);

        // ===== 구매 (응집 핵심 로직) =====

        public enum PurchaseResult { Success, AlreadyPurchased, NotTriggered, Expired, InsufficientStarlight, IAPRequired, IAPInitFailed }

        public static PurchaseResult TryPurchase(LimitedShopOffer offer)
        {
            if (offer == null) return PurchaseResult.NotTriggered;
            if (IsOfferPurchased(offer)) return PurchaseResult.AlreadyPurchased;
            if (!IsOfferAvailable(offer))
            {
                var data0 = MetaProgression.Instance?.Data;
                var trig0 = data0 != null ? LimitedShopOfferQuery.FindTrigger(data0, offer.id) : null;
                return trig0 == null ? PurchaseResult.NotTriggered : PurchaseResult.Expired;
            }

            // 별빛 분기
            if (offer.starlightPrice > 0 && string.IsNullOrEmpty(offer.iapProductId))
            {
                if (CurrencyService.Starlight < offer.starlightPrice) return PurchaseResult.InsufficientStarlight;
                if (!CurrencyService.SpendStarlight(offer.starlightPrice)) return PurchaseResult.InsufficientStarlight;
                LimitedShopRewardGranter.ApplyRewards(offer, _gachaPool);
                LimitedShopRewardGranter.MarkPurchased(offer);
                return PurchaseResult.Success;
            }

            // IAP 분기
            if (!string.IsNullOrEmpty(offer.iapProductId))
            {
                _pendingIapOfferId = offer.id;
                if (IAPService.Instance == null) return PurchaseResult.IAPInitFailed;
                IAPService.Instance.InitiatePurchase(offer.iapProductId);
                return PurchaseResult.IAPRequired;
            }
            return PurchaseResult.NotTriggered;
        }

        public static void ConfirmIAPPurchase(string productId)
        {
            if (_catalog == null) return;
            for (int i = 0; i < _catalog.Length; i++)
            {
                var offer = _catalog[i];
                if (offer == null || offer.iapProductId != productId) continue;
                if (IsOfferPurchased(offer)) continue;
                LimitedShopRewardGranter.ApplyRewards(offer, _gachaPool);
                LimitedShopRewardGranter.MarkPurchased(offer);
                _pendingIapOfferId = null;
                return;
            }
        }

        // ===== 내부 =====
        private static void EnsureFirstLaunchRecorded()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            if (data.firstLaunchUnix > 0) return;
            data.firstLaunchUnix = LimitedShopOfferQuery.NowUnix();
            SaveSystem.Save(data);
            HandleNewUser();
        }
    }
}
