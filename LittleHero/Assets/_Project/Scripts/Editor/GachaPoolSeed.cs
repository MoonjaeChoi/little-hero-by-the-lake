#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// PIVOT C3 — weapon_gacha_v1 GachaPool SO 시드 (gacha_v1.md §1).
    ///
    /// 18 EquipmentTemplate (Templates/Weapons/) 을 등급별로 자동 분류.
    /// 10 소환 레벨별 등급 확률 + 누적 임계값(0/100/300/700/1500/3000/5500/9500/16000/27000).
    /// 비용: 별빛 ⋆ 100/단판, 900/10연차.
    ///
    /// 메뉴: Tools > LittleHero > Seed Gacha Pool. 멱등.
    /// </summary>
    public static class GachaPoolSeed
    {
        private const string BASE_DIR = "Assets/_Project/Data";
        private const string GACHA_DIR = BASE_DIR + "/Gacha";
        private const string TEMPLATE_DIR = BASE_DIR + "/Templates/Weapons";

        [MenuItem("Tools/LittleHero/Seed Gacha Pool")]
        public static void Seed()
        {
            EnsureFolder(BASE_DIR, "Gacha");

            string path = $"{GACHA_DIR}/weapon_gacha_v1.asset";
            var so = AssetDatabase.LoadAssetAtPath<GachaPool>(path)
                     ?? ScriptableObject.CreateInstance<GachaPool>();

            so.id = "weapon_gacha_v1";
            so.displayName = "무기 소환";

            // 18 무기 자동 등급 분류
            var byRarity = new Dictionary<Rarity, List<EquipmentTemplate>>
            {
                { Rarity.Common, new List<EquipmentTemplate>() },
                { Rarity.Magic, new List<EquipmentTemplate>() },
                { Rarity.Rare, new List<EquipmentTemplate>() },
                { Rarity.Epic, new List<EquipmentTemplate>() },
                { Rarity.Legendary, new List<EquipmentTemplate>() },
                { Rarity.Mythic, new List<EquipmentTemplate>() },
            };

            string[] guids = AssetDatabase.FindAssets("t:EquipmentTemplate", new[] { TEMPLATE_DIR });
            for (int i = 0; i < guids.Length; i++)
            {
                string p = AssetDatabase.GUIDToAssetPath(guids[i]);
                var t = AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(p);
                if (t == null || t.slot != EquipSlot.Weapon) continue;
                if (byRarity.TryGetValue(t.rarity, out var list)) list.Add(t);
            }

            so.commonPool    = byRarity[Rarity.Common].ToArray();
            so.magicPool     = byRarity[Rarity.Magic].ToArray();
            so.rarePool      = byRarity[Rarity.Rare].ToArray();
            so.epicPool      = byRarity[Rarity.Epic].ToArray();
            so.legendaryPool = byRarity[Rarity.Legendary].ToArray();
            so.mythicPool    = byRarity[Rarity.Mythic].ToArray();

            // 누적 임계값 (gacha_v1.md §1)
            so.summonLevelThresholds = new[]
            {
                0, 100, 300, 700, 1500, 3000, 5500, 9500, 16000, 27000
            };

            // 10 레벨 확률 곡선 (Lv1=초급 → Lv10=상위 4 등급 균등)
            // gacha_v1.md §1: Lv1 (C70/M25/R4.5/E0.5) → Lv10 (C25/M30/R30/E15) 보간
            so.probabilities = new[]
            {
                MakeProb(1,  0.700f, 0.250f, 0.045f, 0.005f, 0f, 0f),
                MakeProb(2,  0.650f, 0.275f, 0.060f, 0.015f, 0f, 0f),
                MakeProb(3,  0.600f, 0.295f, 0.080f, 0.025f, 0f, 0f),
                MakeProb(4,  0.550f, 0.305f, 0.105f, 0.040f, 0f, 0f),
                MakeProb(5,  0.500f, 0.305f, 0.135f, 0.060f, 0f, 0f),
                MakeProb(6,  0.450f, 0.310f, 0.160f, 0.080f, 0f, 0f),
                MakeProb(7,  0.400f, 0.315f, 0.190f, 0.095f, 0f, 0f),
                MakeProb(8,  0.350f, 0.315f, 0.220f, 0.115f, 0f, 0f),
                MakeProb(9,  0.300f, 0.310f, 0.260f, 0.130f, 0f, 0f),
                MakeProb(10, 0.250f, 0.300f, 0.300f, 0.150f, 0f, 0f),
            };

            // 비용 (gacha_v1.md §1)
            so.starlightCostSingle  = 100;
            so.starlightCost10Pull  = 900;

            // 스타포스 조각 부수 획득 (gacha_v1.md §3)
            so.shardsCommon    = 1;
            so.shardsMagic     = 2;
            so.shardsRare      = 5;
            so.shardsEpic      = 15;
            so.shardsLegendary = 50;
            so.shardsMythic    = 200;

            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[GachaPoolSeed] weapon_gacha_v1 — "
                + $"Common={so.commonPool.Length} Magic={so.magicPool.Length} "
                + $"Rare={so.rarePool.Length} Epic={so.epicPool.Length} "
                + $"Legendary={so.legendaryPool.Length} Mythic={so.mythicPool.Length}");
        }

        private static SummonLevelProbability MakeProb(
            int level, float c, float m, float r, float e, float l, float my)
        {
            return new SummonLevelProbability
            {
                level = level,
                commonRate = c,
                magicRate = m,
                rareRate = r,
                epicRate = e,
                legendaryRate = l,
                mythicRate = my,
            };
        }

        private static void EnsureFolder(string parent, string name)
        {
            string full = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
