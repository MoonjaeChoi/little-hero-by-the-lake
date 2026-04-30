#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B3+B4 — affix_pool §2 / item_drop §3.1 의 35종 SO 인스턴스를 일괄 시드.
    /// 메뉴: Tools > LittleHero > Seed Affix SOs.
    /// 멱등(이미 존재하면 덮어씀). 신규 affix 추가 시 본 파일에 한 줄만 추가.
    /// </summary>
    public static class AffixSeed
    {
        private const string BASE_DIR = "Assets/_Project/Data";
        private const string AFFIX_DIR = BASE_DIR + "/Affixes";
        private const string SUB_DIR = BASE_DIR + "/SubAffixes";

        [MenuItem("Tools/LittleHero/Seed Affix SOs")]
        public static void Seed()
        {
            EnsureFolder("Assets/_Project", "Data");
            EnsureFolder(BASE_DIR, "Affixes");
            EnsureFolder(BASE_DIR, "SubAffixes");

            // === Layer 4 주옵션 23종 (affix_pool §2) ===

            // §2.1 데미지 채널
            MakeAffix("affix_final_dmg",     "최종 데미지 +%",    StatTarget.FinalDamage,       AffixOp.MUL, new[]{1f,3f,5f,8f,12f},  new[]{3f,5f,8f,12f,18f}, "%", 1);
            MakeAffix("affix_attack_dmg",    "공격력 +%",         StatTarget.AttackDamage,      AffixOp.MUL, new[]{2f,4f,7f,12f,18f}, new[]{4f,7f,12f,18f,26f}, "%", 1);
            MakeAffix("affix_crit_dmg",      "크리티컬 피해 +%",  StatTarget.CritDamage,        AffixOp.ADD, new[]{4f,8f,14f,22f,32f},new[]{8f,14f,22f,32f,46f}, "%", 1);
            MakeAffix("affix_crit_chance",   "크리티컬 확률 +%",  StatTarget.CritChance,        AffixOp.ADD, new[]{0.5f,1.5f,3f,5f,7f},new[]{1.5f,3f,5f,7f,10f}, "%", 1);
            MakeAffix("affix_attack_speed",  "공격 속도 +%",      StatTarget.AttackSpeed,       AffixOp.MUL, new[]{2f,4f,6f,10f,14f}, new[]{4f,6f,10f,14f,20f}, "%", 1);
            MakeAffix("affix_armor_pen",     "방어 관통력 +%",    StatTarget.ArmorPenetration,  AffixOp.MUL, new[]{2f,4f,7f,12f,18f}, new[]{4f,7f,12f,18f,26f}, "%", 1);
            MakeAffix("affix_extra_targets", "공격 대상 수 +N",   StatTarget.ExtraAttackTargets,AffixOp.ADD, new[]{1f,1f,1f,2f,3f},   new[]{1f,1f,1f,2f,3f}, "", 0);

            // §2.2 방어·생존
            MakeAffix("affix_max_hp",        "최대 HP +%",        StatTarget.MaxHealth,         AffixOp.MUL, new[]{2f,4f,7f,11f,16f}, new[]{4f,7f,11f,16f,24f}, "%", 1);
            MakeAffix("affix_armor",         "방어력 +N",         StatTarget.Armor,             AffixOp.ADD, new[]{3f,7f,13f,22f,33f},new[]{7f,13f,22f,33f,50f}, "", 0);
            MakeAffix("affix_dmg_reduction", "데미지 감소 +%",    StatTarget.DamageReduction,   AffixOp.ADD, new[]{1f,2f,4f,6f,9f},   new[]{2f,4f,6f,9f,14f}, "%", 1);
            MakeAffix("affix_hp_regen",      "자원 회복 +/s",     StatTarget.HpRegen,           AffixOp.ADD, new[]{0.1f,0.2f,0.4f,0.7f,1.0f},new[]{0.2f,0.4f,0.7f,1.0f,1.5f}, "/s", 1);

            // §2.3 이동·사거리
            MakeAffix("affix_move_speed",    "이동 속도 +",       StatTarget.MoveSpeed,         AffixOp.ADD, new[]{0.05f,0.15f,0.25f,0.40f,0.60f}, new[]{0.15f,0.25f,0.40f,0.60f,0.90f}, "", 2);
            MakeAffix("affix_range",         "사거리 +",          StatTarget.Range,             AffixOp.ADD, new[]{0.1f,0.3f,0.5f,0.8f,1.2f}, new[]{0.3f,0.5f,0.8f,1.2f,1.8f}, "", 1);

            // §2.4 코어 스탯
            MakeAffix("affix_strength",      "힘 +N",             StatTarget.Strength,          AffixOp.ADD, new[]{1f,2f,4f,7f,12f}, new[]{1f,2f,4f,7f,12f}, "", 0);
            MakeAffix("affix_agility",       "빠름 +N",           StatTarget.Agility,           AffixOp.ADD, new[]{1f,2f,4f,7f,12f}, new[]{1f,2f,4f,7f,12f}, "", 0);
            MakeAffix("affix_wisdom",        "슬기 +N",           StatTarget.Wisdom,            AffixOp.ADD, new[]{1f,2f,4f,7f,12f}, new[]{1f,2f,4f,7f,12f}, "", 0);
            MakeAffix("affix_spirit",        "마음 +N",           StatTarget.Spirit,            AffixOp.ADD, new[]{1f,2f,4f,7f,12f}, new[]{1f,2f,4f,7f,12f}, "", 0);
            MakeAffix("affix_all_stats",     "모든 스탯 +N",      StatTarget.AllStats,          AffixOp.ADD, new[]{1f,1f,2f,3f,5f}, new[]{1f,1f,2f,3f,5f}, "", 0);

            // §2.5 메타·드랍
            MakeAffix("affix_xp_gain",       "xp 획득 +%",        StatTarget.XpGain,            AffixOp.MUL, new[]{2f,4f,7f,10f,14f}, new[]{4f,7f,10f,14f,20f}, "%", 1);
            MakeAffix("affix_dust_gain",     "별가루 획득 +%",    StatTarget.DustGain,          AffixOp.MUL, new[]{4f,8f,14f,22f,32f},new[]{8f,14f,22f,32f,46f}, "%", 1);
            MakeAffix("affix_drop_rate",     "드랍률 +%",         StatTarget.DropRate,          AffixOp.MUL, new[]{4f,8f,14f,22f,32f},new[]{8f,14f,22f,32f,46f}, "%", 1);
            MakeAffix("affix_companion_dur", "동행 지속시간 +%",  StatTarget.CompanionDuration, AffixOp.MUL, new[]{4f,8f,14f,22f,32f},new[]{8f,14f,22f,32f,46f}, "%", 1);

            // === 부옵션 12종 (item_drop §3.1) — 가중치는 §3.2 ===
            MakeSub("sub_strength",    "힘 +N",            StatTarget.Strength,        AffixOp.ADD, new[]{1f,1f,2f,3f,5f}, new[]{1f,1f,2f,3f,5f}, 0.15f, "", 0);
            MakeSub("sub_agility",     "빠름 +N",          StatTarget.Agility,         AffixOp.ADD, new[]{1f,1f,2f,3f,5f}, new[]{1f,1f,2f,3f,5f}, 0.15f, "", 0);
            MakeSub("sub_wisdom",      "슬기 +N",          StatTarget.Wisdom,          AffixOp.ADD, new[]{1f,1f,2f,3f,5f}, new[]{1f,1f,2f,3f,5f}, 0.15f, "", 0);
            MakeSub("sub_spirit",      "마음 +N",          StatTarget.Spirit,          AffixOp.ADD, new[]{1f,1f,2f,3f,5f}, new[]{1f,1f,2f,3f,5f}, 0.15f, "", 0);
            MakeSub("sub_max_hp",      "최대 HP +%",       StatTarget.MaxHealth,       AffixOp.MUL, new[]{1f,2f,3f,5f,7f}, new[]{2f,3f,5f,7f,10f}, 0.10f, "%", 1);
            MakeSub("sub_attack_dmg",  "공격력 +%",        StatTarget.AttackDamage,    AffixOp.MUL, new[]{1f,2f,3f,5f,8f}, new[]{2f,3f,5f,8f,12f}, 0.08f, "%", 1);
            MakeSub("sub_armor",       "방어력 +N",        StatTarget.Armor,           AffixOp.ADD, new[]{1f,3f,6f,10f,15f},new[]{3f,6f,10f,15f,22f}, 0.05f, "", 0);
            MakeSub("sub_crit_chance", "크리티컬 확률 +%", StatTarget.CritChance,      AffixOp.ADD, new[]{0.2f,0.5f,1.0f,1.5f,2.5f},new[]{0.5f,1.0f,1.5f,2.5f,4.0f}, 0.05f, "%", 1);
            MakeSub("sub_xp_gain",     "xp 획득 +%",       StatTarget.XpGain,          AffixOp.MUL, new[]{1f,2f,3f,5f,7f}, new[]{2f,3f,5f,7f,10f}, 0.03f, "%", 1);
            MakeSub("sub_dust_gain",   "별가루 획득 +%",   StatTarget.DustGain,        AffixOp.MUL, new[]{2f,4f,6f,10f,14f},new[]{4f,6f,10f,14f,20f}, 0.04f, "%", 1);
            MakeSub("sub_drop_rate",   "드랍률 +%",        StatTarget.DropRate,        AffixOp.MUL, new[]{2f,4f,6f,10f,14f},new[]{4f,6f,10f,14f,20f}, 0.03f, "%", 1);
            MakeSub("sub_hp_regen",    "자원 회복 +/s",    StatTarget.HpRegen,         AffixOp.ADD, new[]{0.05f,0.1f,0.2f,0.3f,0.5f},new[]{0.1f,0.2f,0.3f,0.5f,0.8f}, 0.02f, "/s", 2);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            int aN = AssetDatabase.FindAssets("t:AffixDefinition", new[] { AFFIX_DIR }).Length;
            int sN = AssetDatabase.FindAssets("t:SubAffixDefinition", new[] { SUB_DIR }).Length;
            Debug.Log($"[AffixSeed] Affix={aN} SubAffix={sN}");
        }

        private static void MakeAffix(string id, string display, StatTarget target, AffixOp op,
            float[] mins, float[] maxs, string suffix, int dec)
        {
            string path = $"{AFFIX_DIR}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<AffixDefinition>(path) ?? ScriptableObject.CreateInstance<AffixDefinition>();
            so.id = id;
            so.displayName = display;
            so.target = target;
            so.op = op;
            so.minByRarity = mins;
            so.maxByRarity = maxs;
            so.suffix = suffix;
            so.displayDecimals = dec;
            if (!AssetDatabase.Contains(so)) AssetDatabase.CreateAsset(so, path);
            EditorUtility.SetDirty(so);
        }

        private static void MakeSub(string id, string display, StatTarget target, AffixOp op,
            float[] mins, float[] maxs, float weight, string suffix, int dec)
        {
            string path = $"{SUB_DIR}/{id}.asset";
            var so = AssetDatabase.LoadAssetAtPath<SubAffixDefinition>(path) ?? ScriptableObject.CreateInstance<SubAffixDefinition>();
            so.id = id;
            so.displayName = display;
            so.target = target;
            so.op = op;
            so.minByRarity = mins;
            so.maxByRarity = maxs;
            so.globalWeight = weight;
            so.suffix = suffix;
            so.displayDecimals = dec;
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
