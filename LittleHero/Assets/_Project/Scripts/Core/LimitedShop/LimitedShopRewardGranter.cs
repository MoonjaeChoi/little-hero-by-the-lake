using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 — 핫딜 보상 부여 + MarkPurchased.
    /// </summary>
    public static class LimitedShopRewardGranter
    {
        public static void ApplyRewards(LimitedShopOffer offer, GachaPool gachaPool)
        {
            if (offer.rewards == null) return;
            for (int i = 0; i < offer.rewards.Length; i++)
            {
                var r = offer.rewards[i];
                if (r == null) continue;
                switch (r.type)
                {
                    case LimitedRewardType.GuaranteedWeapon:    GrantSpecificWeapon(gachaPool, r.equipmentTemplateId); break;
                    case LimitedRewardType.GuaranteedRarityPool:GrantWeaponByRarity(gachaPool, r.rarity); break;
                    case LimitedRewardType.StarforceShards:     StarforceService.GrantShards(r.amount); break;
                    case LimitedRewardType.StarlightBundle:     CurrencyService.GrantStarlight(r.amount); break;
                    case LimitedRewardType.SummonTickets:       GachaService.GrantTickets(r.amount); break;
                    case LimitedRewardType.MagicCrystal:        CurrencyService.GrantMagicCrystal(r.amount); break;
                    case LimitedRewardType.RainbowOrb:          CurrencyService.GrantStarFragment(r.amount); break;
                }
            }
        }

        public static void MarkPurchased(LimitedShopOffer offer)
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

        private static void GrantSpecificWeapon(GachaPool gachaPool, string templateId)
        {
            if (gachaPool == null || string.IsNullOrEmpty(templateId))
            {
                Debug.LogWarning("[LimitedShopRewardGranter] GrantSpecificWeapon: GachaPool 미설정 또는 templateId 빈 값");
                return;
            }
            GachaService.GrantSpecificWeapon(gachaPool, templateId);
        }

        private static void GrantWeaponByRarity(GachaPool gachaPool, Rarity rarity)
        {
            if (gachaPool == null)
            {
                Debug.LogWarning("[LimitedShopRewardGranter] GrantWeaponByRarity: GachaPool 미설정");
                return;
            }
            GachaService.GrantWeaponByRarity(gachaPool, rarity);
        }
    }
}
