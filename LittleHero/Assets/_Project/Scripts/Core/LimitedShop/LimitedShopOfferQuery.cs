using System;
using System.Collections.Generic;

namespace LittleHero
{
    /// <summary>
    /// 5028 — 핫딜 활성/구매 여부 조회 + 만료 계산.
    /// </summary>
    public static class LimitedShopOfferQuery
    {
        public static List<LimitedShopOffer> GetActiveOffers(LimitedShopOffer[] catalog)
        {
            var list = new List<LimitedShopOffer>();
            if (catalog == null) return list;
            for (int i = 0; i < catalog.Length; i++)
                if (catalog[i] != null && IsOfferAvailable(catalog[i])) list.Add(catalog[i]);
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
            if (offer.durationDays <= 0) return true;
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

        public static LimitedOfferTrigger FindTrigger(SaveData data, string offerId)
        {
            if (data?.limitedOfferTriggers == null || string.IsNullOrEmpty(offerId)) return null;
            for (int i = 0; i < data.limitedOfferTriggers.Length; i++)
                if (data.limitedOfferTriggers[i] != null && data.limitedOfferTriggers[i].offerId == offerId)
                    return data.limitedOfferTriggers[i];
            return null;
        }

        public static void AppendTrigger(SaveData data, LimitedOfferTrigger trig)
        {
            int len = data.limitedOfferTriggers?.Length ?? 0;
            var arr = new LimitedOfferTrigger[len + 1];
            for (int i = 0; i < len; i++) arr[i] = data.limitedOfferTriggers[i];
            arr[len] = trig;
            data.limitedOfferTriggers = arr;
        }

        public static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}
