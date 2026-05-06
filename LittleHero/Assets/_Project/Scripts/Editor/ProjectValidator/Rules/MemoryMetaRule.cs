using System.Collections.Generic;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    public static class MemoryMetaRule
    {
        public static void Validate(List<Issue> issues)
        {
            ValidateMemoryPool(issues);
            ValidateMetaUpgradePool(issues);
        }

        private static void ValidateMemoryPool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.MemoryPool");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Warning, "MemoryPool", "MemoryPool 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<MemoryPool>(path);
                if (p == null || p.memories == null) continue;
                var seenIds = new HashSet<string>();
                for (int i = 0; i < p.memories.Length; i++)
                {
                    var m = p.memories[i];
                    if (m == null) { issues.Add(new Issue(Severity.Error, "MemoryPool", $"{path}: memories[{i}] null")); continue; }
                    if (string.IsNullOrEmpty(m.id)) issues.Add(new Issue(Severity.Error, "MemoryPool", $"{m.name}: id 비어있음 (저장 키)"));
                    else if (!seenIds.Add(m.id))    issues.Add(new Issue(Severity.Error, "MemoryPool", $"{m.name}: id 중복 '{m.id}'"));
                    if (string.IsNullOrEmpty(m.chapterId)) issues.Add(new Issue(Severity.Warning, "MemoryPool", $"{m.name}: chapterId 비어있음 (어떤 챕터에서도 spawn 안됨)"));
                    if (m.spawnChance <= 0f) issues.Add(new Issue(Severity.Warning, "MemoryPool", $"{m.name}: spawnChance={m.spawnChance}"));
                }
            }
        }

        private static void ValidateMetaUpgradePool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.MetaUpgradePool");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Warning, "MetaUpgradePool", "MetaUpgradePool 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<MetaUpgradePool>(path);
                if (p == null || p.upgrades == null) continue;
                var seenIds = new HashSet<string>();
                for (int i = 0; i < p.upgrades.Length; i++)
                {
                    var u = p.upgrades[i];
                    if (u == null) { issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{path}: upgrades[{i}] null")); continue; }
                    if (string.IsNullOrEmpty(u.id)) issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{u.name}: id 비어있음"));
                    else if (!seenIds.Add(u.id))    issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{u.name}: id 중복 '{u.id}'"));
                    if (u.maxLevel <= 0) issues.Add(new Issue(Severity.Warning, "MetaUpgradePool", $"{u.name}: maxLevel={u.maxLevel}"));
                }
            }
        }
    }
}
