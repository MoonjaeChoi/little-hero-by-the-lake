using System.Collections.Generic;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    public static class ChapterRule
    {
        public static void Validate(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.ChapterData");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Error, "ChapterData", "ChapterData 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
                if (ch == null) continue;
                if (ch.totalWaves < 1) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: totalWaves={ch.totalWaves}"));
                if (ch.enemyPool == null || ch.enemyPool.Length == 0)
                    issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: enemyPool 비어있음"));
                else
                    for (int i = 0; i < ch.enemyPool.Length; i++)
                        if (ch.enemyPool[i] == null) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: enemyPool[{i}] null"));
                        else if (ch.enemyPool[i].prefab == null) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: {ch.enemyPool[i].name} prefab null"));
                if (ch.bossEnemy == null) issues.Add(new Issue(Severity.Warning, "ChapterData", $"{path}: bossEnemy null (마지막 웨이브가 일반 spawn으로 처리됨)"));
                else if (ch.bossEnemy.prefab == null) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: boss prefab null"));
            }
        }
    }
}
