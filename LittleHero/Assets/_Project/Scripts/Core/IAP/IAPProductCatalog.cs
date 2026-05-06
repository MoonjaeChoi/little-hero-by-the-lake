using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleHero
{
    /// <summary>
    /// 5031 — IAP product 카탈로그 + ApplyGrant 매핑.
    /// monetization_v1.md §7.4 — 5 별빛 패키지 + 1 시즌 패스.
    /// </summary>
    public static class IAPProductCatalog
    {
        // Product IDs
        public const string ID_STARLIGHT_100   = "com.littlehero.starlight_100";
        public const string ID_STARLIGHT_500   = "com.littlehero.starlight_500";
        public const string ID_STARLIGHT_1100  = "com.littlehero.starlight_1100";
        public const string ID_STARLIGHT_3500  = "com.littlehero.starlight_3500";
        public const string ID_STARLIGHT_12000 = "com.littlehero.starlight_12000";
        public const string ID_SEASON_PASS     = "com.littlehero.season_pass";

        public static List<ProductDefinition> GetProductDefinitions() => new()
        {
            new ProductDefinition(ID_STARLIGHT_100,   ProductType.Consumable),
            new ProductDefinition(ID_STARLIGHT_500,   ProductType.Consumable),
            new ProductDefinition(ID_STARLIGHT_1100,  ProductType.Consumable),
            new ProductDefinition(ID_STARLIGHT_3500,  ProductType.Consumable),
            new ProductDefinition(ID_STARLIGHT_12000, ProductType.Consumable),
            new ProductDefinition(ID_SEASON_PASS,     ProductType.NonConsumable),
        };

        public static void ApplyGrant(string productId)
        {
            int starlight = 0;
            switch (productId)
            {
                case ID_STARLIGHT_100:   starlight = 100;   break;
                case ID_STARLIGHT_500:   starlight = 550;   break;
                case ID_STARLIGHT_1100:  starlight = 1220;  break;
                case ID_STARLIGHT_3500:  starlight = 3900;  break;
                case ID_STARLIGHT_12000: starlight = 13500; break;
                case ID_SEASON_PASS:
                    SeasonPassService.GrantPremium();
                    return;
            }
            if (starlight > 0)
            {
                CurrencyService.GrantStarlight(starlight);
                Debug.Log($"[IAPProductCatalog] +{starlight}⋆ 적립 ({productId})");
            }
        }
    }
}
