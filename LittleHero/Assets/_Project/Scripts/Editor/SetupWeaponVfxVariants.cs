#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5033.0i — Hovl Magic Effects FREE 8 prefab 의 variant 를
    /// Resources/VFX/Weapon/ 에 생성. WeaponVfxBank.GetPrefab 가 Resources.Load 로 사용.
    /// </summary>
    public static class SetupWeaponVfxVariants
    {
        private const string MENU_PATH = "Tools/LittleHero/Setup Weapon VFX Variants";
        private const string OUTPUT_DIR = "Assets/_Project/Resources/VFX/Weapon";

        private static readonly (string category, string sourcePath)[] _variants = new[]
        {
            // 전사
            ("Sword",     "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Slash effects/Charge slash blue.prefab"),
            ("Axe",       "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Slash effects/Charge slash red.prefab"),
            ("Mace",      "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Slash effects/Stone slash.prefab"),
            ("Spear",     "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Slash effects/Charge slash purple.prefab"),
            // 궁수
            ("Bow",       "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Sparks/Sparks flashing yellow.prefab"),
            ("Crossbow",  "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Sparks/Sparks flashing white.prefab"),
            ("Dagger",    "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Hits and explosions/Star hit.prefab"),
            ("Boomerang", "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Sparks/Sparks flashing green.prefab"),
        };

        [MenuItem(MENU_PATH)]
        public static void Setup()
        {
            if (!Directory.Exists(OUTPUT_DIR))
                Directory.CreateDirectory(OUTPUT_DIR);

            int created = 0;
            int skipped = 0;
            foreach (var (cat, sourcePath) in _variants)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (source == null)
                {
                    Debug.LogWarning($"[SetupWeaponVfxVariants] 원본 prefab 누락: {sourcePath}");
                    skipped++;
                    continue;
                }

                var variantPath = $"{OUTPUT_DIR}/{cat}.prefab";
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
                instance.name = cat;
                PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
                Object.DestroyImmediate(instance);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SetupWeaponVfxVariants] {created} prefab variant 생성 완료 → {OUTPUT_DIR} (skipped {skipped})");
        }
    }
}
#endif
