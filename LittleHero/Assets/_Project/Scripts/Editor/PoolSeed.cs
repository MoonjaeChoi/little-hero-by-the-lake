#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B5+B6 — SlotAffixPool 12종 + GlobalSubAffixPool 1종 시드.
    /// 메뉴: Tools > LittleHero > Seed Pool SOs (B3+B4 시드 후 실행).
    /// 멱등.
    /// </summary>
    public static class PoolSeed
    {
        private const string BASE_DIR = "Assets/_Project/Data";
        private const string AFFIX_DIR = BASE_DIR + "/Affixes";
        private const string SUB_DIR = BASE_DIR + "/SubAffixes";
        private const string SLOT_POOL_DIR = BASE_DIR + "/SlotPools";
        private const string GLOBAL_POOL_DIR = BASE_DIR + "/GlobalPools";

        [MenuItem("Tools/LittleHero/Seed Pool SOs")]
        public static void Seed()
        {
            EnsureFolder(BASE_DIR, "SlotPools");
            EnsureFolder(BASE_DIR, "GlobalPools");

            // affix_pool §3.1 주 슬롯 7개
            MakeSlotPool(EquipSlot.Hat,      "affix_attack_speed",  "affix_wisdom",        "affix_range");
            MakeSlotPool(EquipSlot.Top,      "affix_extra_targets", "affix_spirit",        "affix_max_hp");
            MakeSlotPool(EquipSlot.Bottom,   "affix_final_dmg",     "affix_spirit",        "affix_armor");
            MakeSlotPool(EquipSlot.Cloak,    "affix_final_dmg",     "affix_dmg_reduction", "affix_agility");
            MakeSlotPool(EquipSlot.Gloves,   "affix_crit_dmg",      "affix_crit_chance",   "affix_agility");
            MakeSlotPool(EquipSlot.Weapon,   "affix_attack_dmg",    "affix_strength",      "affix_armor_pen");
            MakeSlotPool(EquipSlot.Shoes,    "affix_move_speed",    "affix_agility",       "affix_companion_dur");

            // §3.2 장신구 슬롯 5개
            MakeSlotPool(EquipSlot.Earring,  "affix_max_hp",        "affix_spirit",        "affix_hp_regen");
            MakeSlotPool(EquipSlot.Glasses,  "affix_final_dmg",     "affix_wisdom",        "affix_range");
            MakeSlotPool(EquipSlot.Necklace, "affix_attack_dmg",    "affix_all_stats",     "affix_xp_gain");
            MakeSlotPool(EquipSlot.Ring,     "affix_all_stats",     "affix_crit_chance",   "affix_dust_gain");
            MakeSlotPool(EquipSlot.Pet,      "affix_companion_dur", "affix_drop_rate",     "affix_hp_regen");

            // 글로벌 부옵션 풀 1종 — SubAffix 12 모두 묶음
            MakeGlobalPool();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int slotN = AssetDatabase.FindAssets("t:SlotAffixPool", new[] { SLOT_POOL_DIR }).Length;
            int globalN = AssetDatabase.FindAssets("t:GlobalSubAffixPool", new[] { GLOBAL_POOL_DIR }).Length;
            Debug.Log($"[PoolSeed] SlotAffixPool={slotN} GlobalSubAffixPool={globalN}");
        }

        private static void MakeSlotPool(EquipSlot slot, string tier1Id, string tier2Id, string tier3Id)
        {
            string path = $"{SLOT_POOL_DIR}/SlotPool_{slot}.asset";
            var so = AssetDatabase.LoadAssetAtPath<SlotAffixPool>(path) ?? ScriptableObject.CreateInstance<SlotAffixPool>();
            so.slot = slot;
            so.tier1 = LoadAffix(tier1Id);
            so.tier2 = LoadAffix(tier2Id);
            so.tier3 = LoadAffix(tier3Id);
            so.weightTier1 = 0.6f;
            so.weightTier2 = 0.3f;
            so.weightTier3 = 0.1f;
            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
        }

        private static AffixDefinition LoadAffix(string id)
        {
            var a = AssetDatabase.LoadAssetAtPath<AffixDefinition>($"{AFFIX_DIR}/{id}.asset");
            if (a == null) Debug.LogError($"[PoolSeed] AffixDefinition '{id}' 없음 — Tools > LittleHero > Seed Affix SOs 를 먼저 실행");
            return a;
        }

        private static void MakeGlobalPool()
        {
            string path = $"{GLOBAL_POOL_DIR}/GlobalSubAffixPool_Default.asset";
            var so = AssetDatabase.LoadAssetAtPath<GlobalSubAffixPool>(path) ?? ScriptableObject.CreateInstance<GlobalSubAffixPool>();

            string[] subIds = {
                "sub_strength","sub_agility","sub_wisdom","sub_spirit",
                "sub_max_hp","sub_attack_dmg","sub_armor","sub_crit_chance",
                "sub_xp_gain","sub_dust_gain","sub_drop_rate","sub_hp_regen",
            };
            var entries = new SubAffixDefinition[subIds.Length];
            for (int i = 0; i < subIds.Length; i++)
            {
                entries[i] = AssetDatabase.LoadAssetAtPath<SubAffixDefinition>($"{SUB_DIR}/{subIds[i]}.asset");
                if (entries[i] == null) Debug.LogError($"[PoolSeed] SubAffix '{subIds[i]}' 없음");
            }
            so.entries = entries;
            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
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
