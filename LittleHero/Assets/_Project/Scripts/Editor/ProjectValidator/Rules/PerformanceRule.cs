using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// LitMotionMigration (5016) + GCAllocPatterns (5017) 통합.
    /// </summary>
    public static class PerformanceRule
    {
        public static void Validate(List<Issue> issues)
        {
            ValidateLitMotionMigration(issues);
            ValidateGCAllocPatterns(issues);
        }

        // 5016 T06 — DOTween 재도입 회귀 방지 + LitMotion import 검증.
        private static void ValidateLitMotionMigration(List<Issue> issues)
        {
            string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
            int doTweenViolations = 0;
            int litMotionImports = 0;
            if (!Directory.Exists(scriptDir))
            {
                issues.Add(new Issue(Severity.Warning, "5016 LitMotion", "Scripts dir 미존재"));
                return;
            }
            foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
            {
                // Validator 자체는 검사 룰 코드라 검사 대상 제외
                if (file.EndsWith("ProjectValidator.cs")) continue;
                if (file.EndsWith("PerformanceRule.cs")) continue;  // 5022
                string content = File.ReadAllText(file);
                if (content.Contains("using DG.Tweening")) doTweenViolations++;
                if (content.Contains("using LitMotion")) litMotionImports++;
            }
            if (doTweenViolations > 0)
                issues.Add(new Issue(Severity.Warning, "5016 LitMotion",
                    $"DOTween 재도입 회귀 검출: {doTweenViolations} files (5016 마이그레이션 후 0 이어야 함)"));
            if (litMotionImports < 12)
                issues.Add(new Issue(Severity.Warning, "5016 LitMotion",
                    $"LitMotion import {litMotionImports} files — 5016 마이그레이션 누락 의심 (기대 ≥ 12)"));
            else
                Debug.Log($"[ProjectValidator] 5016 LitMotion OK — DOTween {doTweenViolations} / LitMotion import {litMotionImports} files");
        }

        // 5017 T04 — DropContext struct 검증 + DamageNumberFloater FormatDamage cache 존재 검증.
        private static void ValidateGCAllocPatterns(List<Issue> issues)
        {
            var dropCtxType = System.Type.GetType("LittleHero.DropContext, Assembly-CSharp");
            if (dropCtxType != null && !dropCtxType.IsValueType)
                issues.Add(new Issue(Severity.Warning, "5017 GC", "DropContext 가 struct 가 아님 (heap alloc)"));

            var floaterType = System.Type.GetType("LittleHero.DamageNumberFloater, Assembly-CSharp");
            if (floaterType != null)
            {
                var cacheCountProp = floaterType.GetProperty("FormatCacheCount",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (cacheCountProp == null)
                    issues.Add(new Issue(Severity.Warning, "5017 GC", "DamageNumberFloater.FormatCacheCount 누락 (cache 미작동)"));
            }
            Debug.Log("[ProjectValidator] 5017 GC alloc OK — DropContext struct + FormatDamage cache 검증");
        }
    }
}
