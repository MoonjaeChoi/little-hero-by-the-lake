using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// WeaponSystem (weapon_v2) + BossSystem (5001) + BossEncounterContextAccess (5002) 통합.
    /// </summary>
    public static class WeaponBossRule
    {
        public static void Validate(List<Issue> issues)
        {
            ValidateWeaponSystem(issues);
            ValidateBossSystem(issues);
            ValidateBossEncounterContextAccess(issues);
        }

        // weapon_system_v2.md — 모든 무기 SO requiredClass + category 검증
        private static void ValidateWeaponSystem(List<Issue> issues)
        {
            var guids = AssetDatabase.FindAssets("t:EquipmentTemplate", new[] { "Assets/_Project/Data/Templates/Weapons" });
            int violations = 0;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var t = AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(p);
                if (t == null) continue;
                if (t.category == WeaponCategory.None)
                {
                    issues.Add(new Issue(Severity.Warning, "Weapon", $"weapon_v2 — {t.id} category=None (Weapon SO 인데 카테고리 미지정)"));
                    violations++;
                }
                if (!WeaponClassBonus.IsClassMatch(t.requiredClass, t.category))
                {
                    issues.Add(new Issue(Severity.Warning, "Weapon", $"weapon_v2 — {t.id}: {t.requiredClass} + {t.category} 매칭 X (시너지 발동 X)"));
                    violations++;
                }
            }
            if (violations == 0)
                issues.Add(new Issue(Severity.Warning, "Weapon", $"weapon_v2 — {guids.Length} 무기 SO 모두 통과"));
        }

        // 5001 boss_system_v2 — 모든 보스 SO BossTier wiring 검증
        private static void ValidateBossSystem(List<Issue> issues)
        {
            var guids = AssetDatabase.FindAssets("t:EnemyData");
            int tierViolations = 0, lootViolations = 0, attackViolations = 0;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var d = AssetDatabase.LoadAssetAtPath<EnemyData>(p);
                if (d == null) continue;

                if ((d.isMiniB || d.isBoss) && d.bossTier == BossTier.None)
                {
                    issues.Add(new Issue(Severity.Warning, "Boss", $"5001 — {d.name}: isMiniB/isBoss=true 인데 bossTier=None (마이그레이션 누락)"));
                    tierViolations++;
                }
                if (d.bossTier >= BossTier.ChapterBoss && d.lootTable == null)
                {
                    issues.Add(new Issue(Severity.Warning, "Boss", $"5001 — {d.name}: bossTier={d.bossTier} 인데 lootTable=null (보상 미적용)"));
                    lootViolations++;
                }
                if (d.bossTier != BossTier.None && d.bossAttackType == BossAttackType.None)
                {
                    issues.Add(new Issue(Severity.Warning, "Boss", $"5001 — {d.name}: bossTier={d.bossTier} 인데 bossAttackType=None (공격 패턴 미명세)"));
                    attackViolations++;
                }
            }
            int total = tierViolations + lootViolations + attackViolations;
            if (total == 0)
                issues.Add(new Issue(Severity.Warning, "Boss", $"5001 boss_system_v2 — {guids.Length} EnemyData 모두 BossTier 룰 통과"));
        }

        // 5002 T09 — BossEncounterContext 외부 직접 R/W 검사 (§15.7 DI)
        private static void ValidateBossEncounterContextAccess(List<Issue> issues)
        {
            var allowedFiles = new HashSet<string>
            {
                "BossEncounterLauncher.cs",
                "BossEncounterRunner.cs",
                "BossEncounterContext.cs",
                "BossEncounterService.cs",
                "AtticController.cs",
                "ProjectValidator.cs",
                "WeaponBossRule.cs",  // 5022: 룰 정의 파일 자체
            };
            string scriptDir = Path.Combine(UnityEngine.Application.dataPath, "_Project/Scripts");
            if (!Directory.Exists(scriptDir)) return;

            int violations = 0;
            foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
            {
                string filename = Path.GetFileName(file);
                if (allowedFiles.Contains(filename)) continue;
                string content = File.ReadAllText(file);
                if (content.Contains("BossEncounterContext.Set(")
                    || content.Contains("BossEncounterContext.PendingBoss =")
                    || content.Contains("BossEncounterContext.PendingChapter =")
                    || content.Contains("BossEncounterContext.PendingMode =")
                    || content.Contains("BossEncounterContext.PendingReturnScene ="))
                {
                    issues.Add(new Issue(Severity.Warning, "5002 §15.7", $"BossEncounterContext 외부 직접 R/W: {filename} — Launcher 위임 권장"));
                    violations++;
                }
            }
            if (violations == 0)
                UnityEngine.Debug.Log("[ProjectValidator] 5002 §15.7 OK — BossEncounterContext 외부 직접 R/W 0건");
        }
    }
}
