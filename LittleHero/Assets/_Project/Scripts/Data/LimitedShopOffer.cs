using System;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT — 한정 상점 핫딜 SO (limited_shop_v1.md §5).
    /// 메이플 키우기 핫딜 패턴 차용.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/LimitedShopOffer")]
    public class LimitedShopOffer : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        [Header("트리거")]
        public LimitedTriggerType triggerType;
        [Tooltip("SummonLevelReached/ChapterFirstCleared 시 N. 그 외 0")]
        public int triggerValue;

        [Tooltip("트리거 시각 +N일 후 만료. 0 = 영구 (조건 도달 후 항상 활성)")]
        public int durationDays = 7;

        [Header("가격 (둘 중 하나)")]
        [Tooltip("별빛 ⋆ 가격. 0 = 캐시 핫딜")]
        public int starlightPrice;
        [Tooltip("Unity IAP productId. 빈 값 = 별빛 핫딜")]
        public string iapProductId;
        [Tooltip("캐시 한정 — 정상가 대비 할인율 표시용 (예: -40%)")]
        public string discountLabel;

        [Header("보상")]
        public LimitedRewardEntry[] rewards;
    }

    public enum LimitedTriggerType
    {
        SummonLevelReached,
        ChapterFirstCleared,
        NewUser,
        DailyBossFirstClear,
        WeeklyBossFirstClear,
        CharacterFirstSelect,
    }

    [Serializable]
    public class LimitedRewardEntry
    {
        public LimitedRewardType type;
        public int amount;
        public string equipmentTemplateId;  // GuaranteedWeapon 시
        public Rarity rarity;                // GuaranteedRarityPool 시
    }

    public enum LimitedRewardType
    {
        GuaranteedWeapon,        // 특정 EquipmentTemplate 1개 확정
        GuaranteedRarityPool,    // 등급 풀에서 1개 확정 (가챠와 동일 풀)
        StarforceShards,
        StarlightBundle,
        SummonTickets,
        MagicCrystal,
        RainbowOrb,
    }

    [Serializable]
    public class LimitedOfferTrigger
    {
        public string offerId;
        public long triggeredUnix;  // 0 = 미트리거
    }
}
