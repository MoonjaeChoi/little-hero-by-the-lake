using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// EnemyPool + EnemyVisibilityCheck (5013) + EnemyHueVariationHook (5012) 통합.
    /// </summary>
    public static class EnemyRule
    {
        public static void Validate(List<Issue> issues)
        {
            ValidateEnemyPool(issues);
            ValidateEnemyVisibilityCheck(issues);
            ValidateEnemyHueVariationHook(issues);
        }

        private static void ValidateEnemyPool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.EnemyPool");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Warning, "EnemyPool", "EnemyPool 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<EnemyPool>(path);
                if (p == null || p.enemies == null) continue;
                var seenIds = new HashSet<string>();
                for (int i = 0; i < p.enemies.Length; i++)
                {
                    var e = p.enemies[i];
                    if (e == null) { issues.Add(new Issue(Severity.Error, "EnemyPool", $"{path}: enemies[{i}] null")); continue; }
                    if (string.IsNullOrEmpty(e.id)) issues.Add(new Issue(Severity.Error, "EnemyPool", $"{e.name}: id 비어있음 (도감/처치 식별자)"));
                    else if (!seenIds.Add(e.id))    issues.Add(new Issue(Severity.Error, "EnemyPool", $"{e.name}: id 중복 '{e.id}'"));
                    if (e.prefab == null)           issues.Add(new Issue(Severity.Error, "EnemyPool", $"{e.name}: prefab null"));
                }
            }
        }

        // 5013 T04 — EnemyAnimationManager.Update 가 EnemyVisibilityCuller.IsVisible 호출하는지 검사.
        private static void ValidateEnemyVisibilityCheck(List<Issue> issues)
        {
            var path = Path.Combine(Application.dataPath, "_Project/Scripts/Gameplay/Enemies/EnemyAnimationManager.cs");
            if (!File.Exists(path))
            {
                issues.Add(new Issue(Severity.Warning, "5013 visibility", "EnemyAnimationManager.cs 미존재"));
                return;
            }
            var src = File.ReadAllText(path);
            if (!src.Contains("EnemyVisibilityCuller.IsVisible"))
                issues.Add(new Issue(Severity.Warning, "5013 visibility",
                    "EnemyAnimationManager 의 Update 에 EnemyVisibilityCuller.IsVisible 호출 누락 — off-screen culling 미작동"));
            else
                Debug.Log("[ProjectValidator] 5013 visibility OK — EnemyAnimationManager.IsVisible 호출 정상");
        }

        // 5012 T04 — EnemyController.cs 가 EnemyHueVariation.HueShift 호출하는지 검사.
        private static void ValidateEnemyHueVariationHook(List<Issue> issues)
        {
            var path = Path.Combine(Application.dataPath, "_Project/Scripts/Gameplay/Enemies/EnemyController.cs");
            if (!File.Exists(path))
            {
                issues.Add(new Issue(Severity.Warning, "5012 hue", "EnemyController.cs 미존재"));
                return;
            }
            var src = File.ReadAllText(path);
            if (!src.Contains("EnemyHueVariation.HueShift"))
                issues.Add(new Issue(Severity.Warning, "5012 hue",
                    "EnemyController 의 ApplyAppearance 에 EnemyHueVariation.HueShift 호출 누락 — 5008 sprite 시각 다양화 미작동"));
            else
                Debug.Log("[ProjectValidator] 5012 hue OK — EnemyController.HueShift 호출 정상");
        }
    }
}
