#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5033 — Hovl Magic Effects FREE 4 prefab 의 prefab variant 를
    /// Resources/VFX/Element/ 에 생성. ElementVfxBank.GetPrefab 가 Resources.Load 로 사용.
    /// </summary>
    public static class SetupElementVfxVariants
    {
        private const string MENU_PATH = "Tools/LittleHero/Setup Element VFX Variants";
        private const string OUTPUT_DIR = "Assets/_Project/Resources/VFX/Element";

        private static readonly (string element, string sourcePath)[] _variants = new[]
        {
            ("Fire",      "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/AoE effects/Red energy explosion.prefab"),
            ("Ice",       "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/AoE effects/Snow AOE.prefab"),
            ("Lightning", "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Hits and explosions/Electro hit.prefab"),
            ("Arcane",    "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Magic circles/Magic circle.prefab"),
        };

        [MenuItem(MENU_PATH)]
        public static void Setup()
        {
            if (!Directory.Exists(OUTPUT_DIR))
                Directory.CreateDirectory(OUTPUT_DIR);

            int created = 0;
            int skipped = 0;
            foreach (var (element, sourcePath) in _variants)
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
                if (source == null)
                {
                    Debug.LogWarning($"[SetupElementVfxVariants] 원본 prefab 누락: {sourcePath}");
                    skipped++;
                    continue;
                }

                var variantPath = $"{OUTPUT_DIR}/{element}.prefab";
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
                instance.name = element;
                PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
                Object.DestroyImmediate(instance);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[SetupElementVfxVariants] {created} prefab variant 생성 완료 → {OUTPUT_DIR} (skipped {skipped})");
        }
    }
}
#endif
