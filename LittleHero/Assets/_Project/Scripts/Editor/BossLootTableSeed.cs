#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5001 boss_system_v2 — BossLootTable 4 인스턴스 시드 (Sprint 3.1 / T05).
    /// 메뉴: Tools > LittleHero > Seed Boss Loot Tables (4).
    /// PRD §3.5 / decisions.md C5 명세 그대로.
    /// </summary>
    public static class BossLootTableSeed
    {
        private const string DIR = "Assets/_Project/Data/BossLoot";

        [MenuItem("Tools/LittleHero/Seed Boss Loot Tables (4)")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project/Data", "BossLoot");

            // PRD §3.5 — Tier 별 보상 + mythicChance
            MakeTable("LootTable_MiniBoss",     BossTier.MiniBoss,    0f,
                Reward(CurrencyType.StarDust,    50),
                Reward(CurrencyType.RainbowOrb,  1));

            MakeTable("LootTable_ChapterBoss",  BossTier.ChapterBoss, 0.05f,
                Reward(CurrencyType.StarDust,    200),
                Reward(CurrencyType.RainbowOrb,  1),
                Reward(CurrencyType.MagicCrystal, 1));

            MakeTable("LootTable_DailyBoss",    BossTier.DailyBoss,   0.1f,
                Reward(CurrencyType.StarDust,    100),
                Reward(CurrencyType.Starlight,   5));

            MakeTable("LootTable_WeeklyBoss",   BossTier.WeeklyBoss,  1.0f,
                Reward(CurrencyType.StarDust,    500),
                Reward(CurrencyType.Starlight,   20));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[BossLootTableSeed] 완료 — 4 인스턴스 생성 (Mini/Chapter/Daily/Weekly).");
        }

        private static void MakeTable(string id, BossTier tier, float mythicChance, params RewardEntry[] rewards)
        {
            var path = $"{DIR}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<BossLootTable>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<BossLootTable>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.tier = tier;
            so.rewards = rewards;
            so.guaranteedMythicChance = mythicChance;
            EditorUtility.SetDirty(so);
        }

        private static RewardEntry Reward(CurrencyType currency, int amount)
        {
            return new RewardEntry { currency = currency, amount = amount };
        }

        private static void EnsureFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder($"{parent}/{name}"))
            {
                AssetDatabase.CreateFolder(parent, name);
            }
        }
    }
}
#endif
