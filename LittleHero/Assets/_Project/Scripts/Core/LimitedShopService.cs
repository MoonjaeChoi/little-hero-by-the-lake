using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT — 한정 상점 (핫딜) 정적 facade. limited_shop_v1.md §6.
    /// 트리거 hook (소환 레벨/챕터 첫 클리어/신규 유저/보스/캐릭터 선택) +
    /// 활성 핫딜 조회 + 구매 흐름 (별빛 분기 / IAP 분기) + 만료 검사.
    ///
    /// 메이플 키우기 핫딜 패턴 차용 — 메이플 100% 캐시 → Little Hero 30% 무료/70% 캐시 혼합.
    /// </summary>
    public static class LimitedShopService
    {
        private static LimitedShopOffer[] _catalog;
        private static GachaPool _gachaPool;
        private static bool _initialized;

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

        // ===== 트리거 hook =====

        public static void HandleSummonLevelUp(int newLevel)
            => TriggerByCondition(LimitedTriggerType.SummonLevelReached, newLevel);

        public static void HandleChapterFirstClear(int chapterN)
            => TriggerByCondition(LimitedTriggerType.ChapterFirstCleared, chapterN);

        public static void HandleNewUser()
            => TriggerByCondition(LimitedTriggerType.NewUser, 0);

        public static void HandleDailyBossClear()
            => TriggerByCondition(LimitedTriggerType.DailyBossFirstClear, 0);

        public static void HandleWeeklyBossClear()
            => TriggerByCondition(LimitedTriggerType.WeeklyBossFirstClear, 0);

        public static void HandleCharacterFirstSelect(string characterId)
            => TriggerByCondition(LimitedTriggerType.CharacterFirstSelect, 0);

        private static void TriggerByCondition(LimitedTriggerType type, int value)
        {
            if (!_initialized) return;
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;

            for (int i = 0; i < _catalog.Length; i++)
            {
                var offer = _catalog[i];
                if (offer == null) continue;
                if (offer.triggerType != type) continue;
                // SummonLevel/Chapter 는 triggerValue 정확 일치, 그 외는 무시
                bool valueMatch =
                    type == LimitedTriggerType.SummonLevelReached ? offer.triggerValue == value :
                    type == LimitedTriggerType.ChapterFirstCleared ? offer.triggerValue == value :
                    true;
                if (!valueMatch) continue;

                if (FindTrigger(data, offer.id) != null) continue;  // 이미 트리거됨

                AppendTrigger(data, new LimitedOfferTrigger {
                    offerId = offer.id,
                    triggeredUnix = NowUnix(),
                });
                SaveSystem.Save(data);
                GameEvents.RaiseLimitedOfferActivated(offer);
                Debug.Log($"[LimitedShop] 활성: {offer.id} ({offer.displayName})");
            }
        }

        // ===== 조회 =====

        /// <summary>현재 활성 (트리거됨 + 미만료 + 미구매) 핫딜 목록.</summary>
        public static List<LimitedShopOffer> GetActiveOffers()
        {
            var list = new List<LimitedShopOffer>();
            if (!_initialized || _catalog == null) return list;
            for (int i = 0; i < _catalog.Length; i++)
            {
                var offer = _catalog[i];
                if (offer != null && IsOfferAvailable(offer)) list.Add(offer);
            }
            return list;
        }

        public static bool IsOfferAvailable(LimitedShopOffer offer)
        {
            if (offer == null) return false;
            var data = MetaProgression.Instance?.Data;
            if (data == null) return false;
            if (IsOfferPurchased(offer)) return false;
            var trig = FindTrigger(data, offer.id);
            if (trig == null || trig.triggeredUnix <= 0) return false;
            if (offer.durationDays <= 0) return true;  // 영구
            long expires = trig.triggeredUnix + offer.durationDays * 86400L;
            return NowUnix() < expires;
        }

        public static bool IsOfferPurchased(LimitedShopOffer offer)
        {
            if (offer == null) return false;
            var data = MetaProgression.Instance?.Data;
            if (data?.purchasedLimitedOfferIds == null) return false;
            for (int i = 0; i < data.purchasedLimitedOfferIds.Length; i++)
                if (data.purchasedLimitedOfferIds[i] == offer.id) return true;
            return false;
        }

        public static long GetExpirationUnix(LimitedShopOffer offer)
        {
            if (offer == null || offer.durationDays <= 0) return 0;
            var data = MetaProgression.Instance?.Data;
            var trig = data != null ? FindTrigger(data, offer.id) : null;
            if (trig == null || trig.triggeredUnix <= 0) return 0;
            return trig.triggeredUnix + offer.durationDays * 86400L;
        }

        public static long GetSecondsRemaining(LimitedShopOffer offer)
        {
            long exp = GetExpirationUnix(offer);
            if (exp <= 0) return long.MaxValue;
            return Math.Max(0, exp - NowUnix());
        }

        // ===== 구매 =====

        public enum PurchaseResult
        {
            Success, AlreadyPurchased, NotTriggered, Expired,
            InsufficientStarlight, IAPRequired, IAPInitFailed,
        }

        public static PurchaseResult TryPurchase(LimitedShopOffer offer)
        {
            if (offer == null) return PurchaseResult.NotTriggered;
            if (IsOfferPurchased(offer)) return PurchaseResult.AlreadyPurchased;
            if (!IsOfferAvailable(offer))
            {
                var data0 = MetaProgression.Instance?.Data;
                var trig0 = data0 != null ? FindTrigger(data0, offer.id) : null;
                if (trig0 == null) return PurchaseResult.NotTriggered;
                return PurchaseResult.Expired;
            }

            // 별빛 분기
            if (offer.starlightPrice > 0 && string.IsNullOrEmpty(offer.iapProductId))
            {
                if (CurrencyService.Starlight < offer.starlightPrice) return PurchaseResult.InsufficientStarlight;
                if (!CurrencyService.SpendStarlight(offer.starlightPrice)) return PurchaseResult.InsufficientStarlight;
                ApplyRewards(offer);
                MarkPurchased(offer);
                return PurchaseResult.Success;
            }

            // IAP 분기 — 비동기 결과는 IAPService 콜백에서 ApplyRewards/MarkPurchased
            // MVP: IAPService.IsInitialized 검사 + InitiatePurchase 호출
            // 실제 운영: PendingIAPOfferId 캐시 + 구매 성공 콜백에서 ConfirmIAPPurchase 호출
            if (!string.IsNullOrEmpty(offer.iapProductId))
            {
                _pendingIapOfferId = offer.id;
                if (IAPService.Instance == null) return PurchaseResult.IAPInitFailed;
                IAPService.Instance.InitiatePurchase(offer.iapProductId);
                return PurchaseResult.IAPRequired;
            }

            return PurchaseResult.NotTriggered;
        }

        // IAPService.OnPurchaseSucceededEvent 가 LimitedShopService.ConfirmIAPPurchase(productId) 호출
        private static string _pendingIapOfferId;

        public static void ConfirmIAPPurchase(string productId)
        {
            if (_catalog == null) return;
            for (int i = 0; i < _catalog.Length; i++)
            {
                var offer = _catalog[i];
                if (offer == null || offer.iapProductId != productId) continue;
                if (IsOfferPurchased(offer)) continue;  // 멱등
                ApplyRewards(offer);
                MarkPurchased(offer);
                _pendingIapOfferId = null;
                return;
            }
        }

        private static void ApplyRewards(LimitedShopOffer offer)
        {
            if (offer.rewards == null) return;
            for (int i = 0; i < offer.rewards.Length; i++)
            {
                var r = offer.rewards[i];
                if (r == null) continue;
                switch (r.type)
                {
                    case LimitedRewardType.GuaranteedWeapon:
                        // GachaService 의 신규 helper — equipmentTemplateId 매칭 EquipmentInstance 생성
                        GrantSpecificWeapon(r.equipmentTemplateId);
                        break;
                    case LimitedRewardType.GuaranteedRarityPool:
                        GrantWeaponByRarity(r.rarity);
                        break;
                    case LimitedRewardType.StarforceShards:
                        StarforceService.GrantShards(r.amount);
                        break;
                    case LimitedRewardType.StarlightBundle:
                        CurrencyService.GrantStarlight(r.amount);
                        break;
                    case LimitedRewardType.SummonTickets:
                        GachaService.GrantTickets(r.amount);
                        break;
                    case LimitedRewardType.MagicCrystal:
                        CurrencyService.GrantMagicCrystal(r.amount);
                        break;
                    case LimitedRewardType.RainbowOrb:
                        CurrencyService.GrantStarFragment(r.amount);
                        break;
                }
            }
        }

        private static void GrantSpecificWeapon(string templateId)
        {
            if (_gachaPool == null || string.IsNullOrEmpty(templateId))
            {
                Debug.LogWarning("[LimitedShop] GrantSpecificWeapon: GachaPool 미설정 또는 templateId 빈 값");
                return;
            }
            GachaService.GrantSpecificWeapon(_gachaPool, templateId);
        }

        private static void GrantWeaponByRarity(Rarity rarity)
        {
            if (_gachaPool == null)
            {
                Debug.LogWarning("[LimitedShop] GrantWeaponByRarity: GachaPool 미설정");
                return;
            }
            GachaService.GrantWeaponByRarity(_gachaPool, rarity);
        }

        private static void MarkPurchased(LimitedShopOffer offer)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            int len = data.purchasedLimitedOfferIds?.Length ?? 0;
            var arr = new string[len + 1];
            for (int i = 0; i < len; i++) arr[i] = data.purchasedLimitedOfferIds[i];
            arr[len] = offer.id;
            data.purchasedLimitedOfferIds = arr;
            SaveSystem.Save(data);
            GameEvents.RaiseLimitedOfferPurchased(offer);
        }

        // ===== 내부 =====

        private static void EnsureFirstLaunchRecorded()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            if (data.firstLaunchUnix > 0) return;
            data.firstLaunchUnix = NowUnix();
            SaveSystem.Save(data);
            HandleNewUser();
        }

        private static LimitedOfferTrigger FindTrigger(SaveData data, string offerId)
        {
            if (data?.limitedOfferTriggers == null || string.IsNullOrEmpty(offerId)) return null;
            for (int i = 0; i < data.limitedOfferTriggers.Length; i++)
                if (data.limitedOfferTriggers[i] != null && data.limitedOfferTriggers[i].offerId == offerId)
                    return data.limitedOfferTriggers[i];
            return null;
        }

        private static void AppendTrigger(SaveData data, LimitedOfferTrigger trig)
        {
            int len = data.limitedOfferTriggers?.Length ?? 0;
            var arr = new LimitedOfferTrigger[len + 1];
            for (int i = 0; i < len; i++) arr[i] = data.limitedOfferTriggers[i];
            arr[len] = trig;
            data.limitedOfferTriggers = arr;
        }

        private static long NowUnix()
            => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
