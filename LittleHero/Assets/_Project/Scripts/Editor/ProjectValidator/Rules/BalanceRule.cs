using System.Collections.Generic;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    public static class BalanceRule
    {
        public static void Validate(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.BalanceConfig");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Error, "BalanceConfig", "BalanceConfig 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var b = AssetDatabase.LoadAssetAtPath<BalanceConfig>(path);
                if (b == null) continue;
                if (b.priceByRarity == null || b.priceByRarity.Length != 5)
                    issues.Add(new Issue(Severity.Error, "BalanceConfig", $"{path}: priceByRarity 길이가 5가 아님"));
                if (b.shopItemCount <= 0)
                    issues.Add(new Issue(Severity.Warning, "BalanceConfig", $"{path}: shopItemCount={b.shopItemCount}"));
            }
        }
    }
}
