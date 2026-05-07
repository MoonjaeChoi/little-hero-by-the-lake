using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// 5037 — Variant EnemyData SO 일관성 검사. D1=A 단일 클래스 50L 통합.
    /// - Splitter behaviorType 면 splitterChildData != null
    /// - Stationary/Ranged behaviorType 면 projectilePrefab != null (5037.1 cascade 보강 명시)
    /// </summary>
    public static class EnemyVariantConsistencyRule
    {
        public static void Validate(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:EnemyData");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var so = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (so == null) continue;

                if (so.behaviorType == EnemyBehaviorType.Splitter && so.splitterChildData == null)
                    issues.Add(new Issue(Severity.Warning, "EnemyVariant",
                        $"[{so.name}] Splitter behavior 인데 splitterChildData=null"));

                if ((so.behaviorType == EnemyBehaviorType.Stationary || so.behaviorType == EnemyBehaviorType.Ranged)
                    && so.projectilePrefab == null)
                    issues.Add(new Issue(Severity.Warning, "EnemyVariant",
                        $"[{so.name}] {so.behaviorType} behavior 인데 projectilePrefab=null (5037.1 cascade 보강 예정)"));
            }
        }
    }
}
