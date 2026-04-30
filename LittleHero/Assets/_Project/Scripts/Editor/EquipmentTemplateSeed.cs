#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B1 — 기존 EquipmentData 16종 → EquipmentTemplate 마이그레이션 (equipment_v1 §4.4 표).
    /// 메뉴: Tools > LittleHero > Migrate Equipment Templates.
    /// 멱등 — 기존 Template 가 있으면 덮어쓰기.
    /// 기존 EquipmentData 16종은 그대로 유지(레거시 시스템 사용 중).
    /// </summary>
    public static class EquipmentTemplateSeed
    {
        private const string TEMPLATE_DIR = "Assets/_Project/Data/Templates";
        private const string LEGACY_DIR = "Assets/_Project/Data/Equipment";

        // §4.4 마이그레이션 표
        private struct Mapping
        {
            public string id;
            public EquipSlot slot;
            public Rarity rarity;
            public Mapping(string id, EquipSlot s, Rarity r) { this.id = id; slot = s; rarity = r; }
        }

        private static readonly Mapping[] MAP = {
            new Mapping("Equip_StarWand",        EquipSlot.Weapon,   Rarity.Common),
            new Mapping("Equip_MeteorRod",       EquipSlot.Weapon,   Rarity.Rare),
            new Mapping("Equip_HaloLight",       EquipSlot.Hat,      Rarity.Common),
            new Mapping("Equip_StarlightHalo",   EquipSlot.Hat,      Rarity.Rare),
            new Mapping("Equip_BraveHat",        EquipSlot.Hat,      Rarity.Uncommon),
            new Mapping("Equip_PetalCloak",      EquipSlot.Cloak,    Rarity.Common),
            new Mapping("Equip_DragonflyWings",  EquipSlot.Cloak,    Rarity.Uncommon),
            new Mapping("Equip_HeroOutfit",      EquipSlot.Top,      Rarity.Epic),
            new Mapping("Equip_RunShoes",        EquipSlot.Shoes,    Rarity.Common),
            new Mapping("Equip_RainBoots",       EquipSlot.Shoes,    Rarity.Rare),
            new Mapping("Equip_SparkleTrail",    EquipSlot.Shoes,    Rarity.Uncommon),
            new Mapping("Equip_FairyFriend",     EquipSlot.Pet,      Rarity.Uncommon),
            new Mapping("Equip_SummonBuddy",     EquipSlot.Pet,      Rarity.Rare),
            new Mapping("Equip_DancingStarDust", EquipSlot.Necklace, Rarity.Rare),
            new Mapping("Equip_MagicBag",        EquipSlot.Necklace, Rarity.Common),
            new Mapping("Equip_PulseBag",        EquipSlot.Necklace, Rarity.Uncommon),
        };

        [MenuItem("Tools/LittleHero/Migrate Equipment Templates")]
        public static void Migrate()
        {
            EnsureFolder("Assets/_Project/Data", "Templates");

            int migrated = 0, missing = 0;
            foreach (var m in MAP)
            {
                var legacy = FindLegacy(m.id);
                if (legacy == null) { Debug.LogWarning($"[EquipmentTemplateSeed] 레거시 EquipmentData 미발견: {m.id}"); missing++; continue; }

                string path = $"{TEMPLATE_DIR}/{m.id}.asset";
                var t = AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(path) ?? ScriptableObject.CreateInstance<EquipmentTemplate>();
                t.id = m.id;
                t.displayName = legacy.displayName;
                t.description = legacy.description;
                t.icon = legacy.icon;
                t.slot = m.slot;
                t.rarity = m.rarity;
                t.baseStats = legacy.statBonus;
                t.attackPattern = legacy.attackPattern;
                t.attackConfig = legacy.attackConfig;
                t.maxEnhanceLevel = 25;
                t.mainAffixLineCount = 3;
                t.subAffixLineCount = 3;
                if (!AssetDatabase.Contains(t)) AssetDatabase.CreateAsset(t, path);
                EditorUtility.SetDirty(t);
                migrated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EquipmentTemplateSeed] migrated={migrated} missing={missing} (총 16 매핑)");
        }

        private static EquipmentData FindLegacy(string id)
        {
            // 정확한 경로 우선
            var direct = AssetDatabase.LoadAssetAtPath<EquipmentData>($"{LEGACY_DIR}/{id}.asset");
            if (direct != null) return direct;

            // 폴더 내 어디에 있어도 ID 매칭
            string[] guids = AssetDatabase.FindAssets("t:EquipmentData");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var so = AssetDatabase.LoadAssetAtPath<EquipmentData>(path);
                if (so != null && so.id == id) return so;
            }
            return null;
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
