using System.Collections.Generic;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    public static class ShopRule
    {
        private const string DefaultPoolPath = "Assets/_Project/Data/Shop/ShopPool_Default.asset";

        public static void Validate(List<Issue> issues)
        {
            var pool = AssetDatabase.LoadAssetAtPath<ShopPool>(DefaultPoolPath);
            if (pool == null) { issues.Add(new Issue(Severity.Error, "ShopPool", $"{DefaultPoolPath} 없음")); return; }
            if (pool.items == null || pool.items.Length == 0)
                issues.Add(new Issue(Severity.Error, "ShopPool", "items 비어있음"));
            for (int i = 0; pool.items != null && i < pool.items.Length; i++)
            {
                var it = pool.items[i];
                if (it == null) { issues.Add(new Issue(Severity.Error, "ShopPool", $"items[{i}] null")); continue; }
                if (string.IsNullOrEmpty(it.displayName)) issues.Add(new Issue(Severity.Warning, "ShopPool", $"{it.name}: displayName 비어있음"));
                if (it.icon == null) issues.Add(new Issue(Severity.Warning, "ShopPool", $"{it.name}: icon 미설정"));
                if (it.attackPattern != AttackPatternType.None && it.attackConfig.projectilePrefab == null)
                    issues.Add(new Issue(Severity.Error, "ShopPool", $"{it.name}: pattern={it.attackPattern}인데 projectilePrefab null"));
            }
        }
    }
}
