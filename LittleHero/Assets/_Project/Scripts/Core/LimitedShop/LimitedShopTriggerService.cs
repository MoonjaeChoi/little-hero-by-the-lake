using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 — 한정 상점 트리거 hook (소환레벨/챕터/신규/보스/캐릭터).
    /// </summary>
    public static class LimitedShopTriggerService
    {
        public static void HandleSummonLevelUp(LimitedShopOffer[] catalog, int newLevel)
            => TriggerByCondition(catalog, LimitedTriggerType.SummonLevelReached, newLevel);

        public static void HandleChapterFirstClear(LimitedShopOffer[] catalog, int chapterN)
            => TriggerByCondition(catalog, LimitedTriggerType.ChapterFirstCleared, chapterN);

        public static void HandleNewUser(LimitedShopOffer[] catalog)
            => TriggerByCondition(catalog, LimitedTriggerType.NewUser, 0);

        public static void HandleDailyBossClear(LimitedShopOffer[] catalog)
            => TriggerByCondition(catalog, LimitedTriggerType.DailyBossFirstClear, 0);

        public static void HandleWeeklyBossClear(LimitedShopOffer[] catalog)
            => TriggerByCondition(catalog, LimitedTriggerType.WeeklyBossFirstClear, 0);

        public static void HandleCharacterFirstSelect(LimitedShopOffer[] catalog, string characterId)
            => TriggerByCondition(catalog, LimitedTriggerType.CharacterFirstSelect, 0);

        private static void TriggerByCondition(LimitedShopOffer[] catalog, LimitedTriggerType type, int value)
        {
            if (catalog == null) return;
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;

            for (int i = 0; i < catalog.Length; i++)
            {
                var offer = catalog[i];
                if (offer == null) continue;
                if (offer.triggerType != type) continue;

                bool valueMatch =
                    type == LimitedTriggerType.SummonLevelReached ? offer.triggerValue == value :
                    type == LimitedTriggerType.ChapterFirstCleared ? offer.triggerValue == value :
                    true;
                if (!valueMatch) continue;
                if (LimitedShopOfferQuery.FindTrigger(data, offer.id) != null) continue;

                LimitedShopOfferQuery.AppendTrigger(data, new LimitedOfferTrigger
                {
                    offerId = offer.id,
                    triggeredUnix = LimitedShopOfferQuery.NowUnix(),
                });
                SaveSystem.Save(data);
                GameEvents.RaiseLimitedOfferActivated(offer);
                Debug.Log($"[LimitedShop] 활성: {offer.id} ({offer.displayName})");
            }
        }
    }
}
