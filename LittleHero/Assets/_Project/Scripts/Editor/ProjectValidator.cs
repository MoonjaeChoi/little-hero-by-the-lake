using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5022 — orchestrator only (~30L). 11 Rule 클래스 분리 (Editor/ProjectValidator/Rules/*.cs).
    /// Issue/Severity 는 Editor/ProjectValidator/Issue.cs.
    /// </summary>
    public static class ProjectValidator
    {
        [MenuItem("Tools/LittleHero/Validate Project")]
        public static void Run()
        {
            var issues = new List<Issue>();
            // 6 간단 Rule (Sprint 1.2)
            Rules.BuildSettingsRule.Validate(issues);
            Rules.BalanceRule.Validate(issues);
            Rules.ShopRule.Validate(issues);
            Rules.ChapterRule.Validate(issues);
            Rules.CharacterRule.Validate(issues);  // 5020 StarterHero + 5021 CharacterPool 흡수
            Rules.MemoryMetaRule.Validate(issues);
            // 5 복잡 Rule (Sprint 2)
            Rules.EnemyRule.Validate(issues);              // EnemyPool + 5012 Hue + 5013 Visibility
            Rules.SceneRule.Validate(issues);              // ActiveScene + Camera + Cinemachine + Panel + 5004 SceneManager
            Rules.OOPPolicyRule.Validate(issues);          // §15.6 200줄 룰
            Rules.WeaponBossRule.Validate(issues);         // weapon_v2 + 5001 Boss + 5002 Context
            Rules.PerformanceRule.Validate(issues);        // 5016 LitMotion + 5017 GC
            Print(issues);
        }

        private static void Print(List<Issue> issues)
        {
            int errors = 0, warnings = 0;
            foreach (var i in issues)
            {
                if (i.Sev == Severity.Error) { errors++; Debug.LogError($"[Validator] [{i.Category}] {i.Msg}"); }
                else { warnings++; Debug.LogWarning($"[Validator] [{i.Category}] {i.Msg}"); }
            }
            string summary = $"[Validator] 완료. errors={errors}, warnings={warnings}";
            if (errors > 0) Debug.LogError(summary); else Debug.Log(summary);
        }
    }
}
