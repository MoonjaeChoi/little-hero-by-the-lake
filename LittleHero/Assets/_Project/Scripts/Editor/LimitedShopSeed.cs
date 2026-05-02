#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// PIVOT — 한정 상점 SO 시드 (limited_shop_v1.md §3).
    /// 출시 시 12 핫딜: 무료 4 + 캐시 8.
    /// 메뉴: Tools > LittleHero > Seed Limited Shop. 멱등.
    /// </summary>
    public static class LimitedShopSeed
    {
        private const string DIR = "Assets/_Project/Data/LimitedShop";

        [MenuItem("Tools/LittleHero/Seed Limited Shop")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project/Data", "LimitedShop");

            // === 무료 핫딜 (별빛 ⋆) — limited_shop_v1.md §3.1 ===
            MakeOffer(
                id: "free_summon_lv2",
                displayName: "소환 기념 — 희귀 무기",
                description: "소환 레벨 ⭐2 도달 축하 패키지. 희귀 등급 무기 1개 확정!",
                trigger: LimitedTriggerType.SummonLevelReached, triggerValue: 2,
                duration: 14, starlight: 200, iap: "",
                rewards: new[] { Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Rare) });

            MakeOffer(
                id: "free_chapter_05",
                displayName: "5챕터 클리어 보너스",
                description: "5챕터 첫 클리어! 조각 1,000 + 소환권 5장.",
                trigger: LimitedTriggerType.ChapterFirstCleared, triggerValue: 5,
                duration: 7, starlight: 100, iap: "",
                rewards: new[] {
                    Reward(LimitedRewardType.StarforceShards, amount: 1000),
                    Reward(LimitedRewardType.SummonTickets, amount: 5),
                });

            MakeOffer(
                id: "free_chapter_15",
                displayName: "15챕터 클리어 보너스",
                description: "15챕터 첫 클리어! 조각 5,000 + 소환권 10장.",
                trigger: LimitedTriggerType.ChapterFirstCleared, triggerValue: 15,
                duration: 7, starlight: 500, iap: "",
                rewards: new[] {
                    Reward(LimitedRewardType.StarforceShards, amount: 5000),
                    Reward(LimitedRewardType.SummonTickets, amount: 10),
                });

            MakeOffer(
                id: "free_chapter_25",
                displayName: "25챕터 클리어 보너스",
                description: "25챕터 첫 클리어! 조각 15,000 + 영웅 무기 1개 확정.",
                trigger: LimitedTriggerType.ChapterFirstCleared, triggerValue: 25,
                duration: 7, starlight: 1000, iap: "",
                rewards: new[] {
                    Reward(LimitedRewardType.StarforceShards, amount: 15000),
                    Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Epic),
                });

            // === 캐시 핫딜 (Unity IAP) — limited_shop_v1.md §3.2 ===
            MakeOffer(
                id: "cash_newuser_starter",
                displayName: "새내기 입문 패키지",
                description: "게임 시작 7일 한정! 별빛 1,000 + 조각 1,000 + 소환권 5장.",
                trigger: LimitedTriggerType.NewUser, triggerValue: 0,
                duration: 7, starlight: 0, iap: "LIMITED_NEWUSER_STARTER", discount: "-50%",
                rewards: new[] {
                    Reward(LimitedRewardType.StarlightBundle, amount: 1000),
                    Reward(LimitedRewardType.StarforceShards, amount: 1000),
                    Reward(LimitedRewardType.SummonTickets, amount: 5),
                });

            MakeOffer(
                id: "cash_summon_lv3_rare",
                displayName: "소환 ⭐3 — 희귀 확정",
                description: "희귀 등급 무기 1개 확정 + 조각 3,000.",
                trigger: LimitedTriggerType.SummonLevelReached, triggerValue: 3,
                duration: 14, starlight: 0, iap: "LIMITED_SUMMON_LV3_RARE", discount: "-40%",
                rewards: new[] {
                    Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Rare),
                    Reward(LimitedRewardType.StarforceShards, amount: 3000),
                });

            MakeOffer(
                id: "cash_summon_lv5_epic",
                displayName: "소환 ⭐5 — 영웅 확정",
                description: "영웅 등급 무기 1개 확정 + 조각 10,000 + 별빛 1,100.",
                trigger: LimitedTriggerType.SummonLevelReached, triggerValue: 5,
                duration: 14, starlight: 0, iap: "LIMITED_SUMMON_LV5_EPIC", discount: "-45%",
                rewards: new[] {
                    Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Epic),
                    Reward(LimitedRewardType.StarforceShards, amount: 10000),
                    Reward(LimitedRewardType.StarlightBundle, amount: 1100),
                });

            MakeOffer(
                id: "cash_summon_lv7_legendary",
                displayName: "소환 ⭐7 — 전설 확정",
                description: "출시 후 — 전설 등급 무기 1개 확정 + 조각 30,000.",
                trigger: LimitedTriggerType.SummonLevelReached, triggerValue: 7,
                duration: 14, starlight: 0, iap: "LIMITED_SUMMON_LV7_LEGENDARY", discount: "-50%",
                rewards: new[] {
                    Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Legendary),
                    Reward(LimitedRewardType.StarforceShards, amount: 30000),
                });

            MakeOffer(
                id: "cash_chapter_10",
                displayName: "10챕터 보너스",
                description: "조각 5,000 + 별빛 500 + 소환권 10장.",
                trigger: LimitedTriggerType.ChapterFirstCleared, triggerValue: 10,
                duration: 7, starlight: 0, iap: "LIMITED_CHAPTER_10", discount: "-35%",
                rewards: new[] {
                    Reward(LimitedRewardType.StarforceShards, amount: 5000),
                    Reward(LimitedRewardType.StarlightBundle, amount: 500),
                    Reward(LimitedRewardType.SummonTickets, amount: 10),
                });

            MakeOffer(
                id: "cash_chapter_20",
                displayName: "20챕터 보너스",
                description: "희귀 등급 무기 1개 확정 + 조각 15,000.",
                trigger: LimitedTriggerType.ChapterFirstCleared, triggerValue: 20,
                duration: 7, starlight: 0, iap: "LIMITED_CHAPTER_20", discount: "-40%",
                rewards: new[] {
                    Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Rare),
                    Reward(LimitedRewardType.StarforceShards, amount: 15000),
                });

            MakeOffer(
                id: "cash_chapter_30",
                displayName: "30챕터 보너스",
                description: "영웅 등급 무기 1개 확정 + 조각 50,000 + 별빛 3,500.",
                trigger: LimitedTriggerType.ChapterFirstCleared, triggerValue: 30,
                duration: 7, starlight: 0, iap: "LIMITED_CHAPTER_30", discount: "-45%",
                rewards: new[] {
                    Reward(LimitedRewardType.GuaranteedRarityPool, rarity: Rarity.Epic),
                    Reward(LimitedRewardType.StarforceShards, amount: 50000),
                    Reward(LimitedRewardType.StarlightBundle, amount: 3500),
                });

            MakeOffer(
                id: "cash_character_starter",
                displayName: "캐릭터 입문 패키지",
                description: "캐릭터 선택 환영 — 별빛 500 + 조각 500.",
                trigger: LimitedTriggerType.CharacterFirstSelect, triggerValue: 0,
                duration: 14, starlight: 0, iap: "LIMITED_CHARACTER_STARTER", discount: "-30%",
                rewards: new[] {
                    Reward(LimitedRewardType.StarlightBundle, amount: 500),
                    Reward(LimitedRewardType.StarforceShards, amount: 500),
                });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            int n = AssetDatabase.FindAssets("t:LimitedShopOffer", new[] { DIR }).Length;
            Debug.Log("[LimitedShopSeed] " + n + "개 핫딜 SO 시드 완료");
        }

        private static void MakeOffer(
            string id, string displayName, string description,
            LimitedTriggerType trigger, int triggerValue, int duration,
            int starlight, string iap, string discount = "",
            LimitedRewardEntry[] rewards = null)
        {
            string path = DIR + "/" + id + ".asset";
            var so = AssetDatabase.LoadAssetAtPath<LimitedShopOffer>(path)
                     ?? ScriptableObject.CreateInstance<LimitedShopOffer>();
            so.id = id;
            so.displayName = displayName;
            so.description = description;
            so.triggerType = trigger;
            so.triggerValue = triggerValue;
            so.durationDays = duration;
            so.starlightPrice = starlight;
            so.iapProductId = iap;
            so.discountLabel = discount;
            so.rewards = rewards ?? new LimitedRewardEntry[0];
            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
        }

        private static LimitedRewardEntry Reward(
            LimitedRewardType type, int amount = 0,
            string equipmentTemplateId = "", Rarity rarity = Rarity.Common)
        {
            return new LimitedRewardEntry {
                type = type, amount = amount,
                equipmentTemplateId = equipmentTemplateId, rarity = rarity,
            };
        }

        private static void EnsureFolder(string parent, string name)
        {
            string full = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
