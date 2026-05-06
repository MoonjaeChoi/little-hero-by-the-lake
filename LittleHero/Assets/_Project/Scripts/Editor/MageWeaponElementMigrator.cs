#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5029 — 마법사 무기 9개 element 일괄 매핑.
    /// 분포 (옵션 B 채택): Fire 2 / Ice 2 / Lightning 2 / Arcane 3.
    /// </summary>
    public static class MageWeaponElementMigrator
    {
        private static readonly Dictionary<string, MagicElement> ElementMap = new()
        {
            // Fire (2)
            { "weapon_firebottle",        MagicElement.Fire },
            { "weapon_meteor_rod",        MagicElement.Fire },
            // Ice (2)
            { "weapon_frost_rune",        MagicElement.Ice },
            { "mage_glacier_rune",        MagicElement.Ice },
            // Lightning (2 — 옵션 B 신규 weapon_thunder_wand 포함)
            { "weapon_annihilation_rain", MagicElement.Lightning },
            { "weapon_thunder_wand",      MagicElement.Lightning },
            // Arcane (3)
            { "weapon_rune_staff",        MagicElement.Arcane },
            { "weapon_spirit_pet",        MagicElement.Arcane },
            { "weapon_time_spirit",       MagicElement.Arcane },
        };

        [MenuItem("Tools/LittleHero/Migrate Mage Weapon Elements")]
        public static void Run()
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.EquipmentTemplate", new[] { "Assets/_Project/Data/Templates/Weapons" });
            int migrated = 0, skipped = 0, notInMap = 0;
            var counts = new Dictionary<MagicElement, int>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var t = AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(path);
                if (t == null) continue;

                if (t.requiredClass != CharacterClass.Mage) { skipped++; continue; }

                if (!ElementMap.TryGetValue(t.id, out var element))
                {
                    Debug.LogWarning($"[MageWeaponElementMigrator] '{t.id}' ElementMap 미존재");
                    notInMap++;
                    continue;
                }
                if (t.attackConfig != null)
                {
                    t.attackConfig.element = element;
                    EditorUtility.SetDirty(t);
                    migrated++;
                    counts[element] = counts.GetValueOrDefault(element, 0) + 1;
                }
            }
            AssetDatabase.SaveAssets();

            string breakdown = $"Fire:{counts.GetValueOrDefault(MagicElement.Fire, 0)} / Ice:{counts.GetValueOrDefault(MagicElement.Ice, 0)} / Lightning:{counts.GetValueOrDefault(MagicElement.Lightning, 0)} / Arcane:{counts.GetValueOrDefault(MagicElement.Arcane, 0)}";
            Debug.Log($"[MageWeaponElementMigrator] migrated={migrated} skipped={skipped} notInMap={notInMap} ({breakdown})");
            EditorUtility.DisplayDialog("Mage Weapon Element Migration",
                $"갱신: {migrated} 무기\n비-마법사: {skipped}\nMap 미존재: {notInMap}\n\n{breakdown}", "OK");
        }
    }
}
#endif
