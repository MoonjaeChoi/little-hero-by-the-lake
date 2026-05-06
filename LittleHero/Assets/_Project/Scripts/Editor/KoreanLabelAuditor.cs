#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using LittleHero;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5025 D1=B — SO displayName 한국어 검수 + whitelist 패턴 자동 skip.
    /// 보고서: docs/little_hero/korean_label_audit_report.md.
    /// </summary>
    public static class KoreanLabelAuditor
    {
        // 의도된 영문 패턴 — Auditor 가 자동 skip (D1=B whitelist)
        private static readonly Regex[] WhitelistPatterns =
        {
            new Regex(@"^Lv\.?\s?\d+$"),                                // "Lv. 50"
            new Regex(@"\b(DPS|HP|MP|XP|UI|VFX|SFX|BGM)\b"),           // 게임 어휘 약어
            new Regex(@"^(hero_|enemy_|boss_|chapter_|affix_|equip_)"), // ID format
            new Regex(@"^[\★✦◇⋆]+$"),                                  // 화폐 단위
            new Regex(@"^\d+(\.\d+)?[%xX]?$"),                          // 숫자
            new Regex(@"\+N\b"),                                        // "+N" placeholder (예 "민첩 +N")
            new Regex(@"\+?/s\b"),                                      // "+/s" 시간 단위 (예 "회복 +/s")
        };

        private const string ReportRel = "../../docs/little_hero/korean_label_audit_report.md";

        [MenuItem("Tools/LittleHero/Audit Korean Labels")]
        public static void Run()
        {
            var report = new StringBuilder();
            report.AppendLine("# Korean Label Audit Report");
            report.AppendLine($"\n생성: {System.DateTime.Now:yyyy-MM-dd HH:mm}\n");
            report.AppendLine("> D1=B — whitelist 패턴 (Lv\\., DPS, ID format, 화폐 단위) 자동 skip\n");

            int totalChecked = 0, englishFound = 0;
            CheckCategory<ChapterData>(report, "ChapterData", "displayName", ref totalChecked, ref englishFound, c => c.displayName);
            CheckCategory<EquipmentTemplate>(report, "EquipmentTemplate", "displayName", ref totalChecked, ref englishFound, t => t.displayName);
            CheckCategory<AffixDefinition>(report, "AffixDefinition", "displayName", ref totalChecked, ref englishFound, a => a.displayName);
            CheckCategory<MythicDefinition>(report, "MythicDefinition", "displayName", ref totalChecked, ref englishFound, m => m.displayName);
            CheckCategory<EndgameModeData>(report, "EndgameModeData", "displayName", ref totalChecked, ref englishFound, e => e.displayName);
            CheckCategory<MetaUpgradeData>(report, "MetaUpgradeData", "displayName", ref totalChecked, ref englishFound, u => u.displayName);
            CheckCategory<MemoryData>(report, "MemoryData", "displayName", ref totalChecked, ref englishFound, m => m.displayName);
            CheckCategory<CharacterData>(report, "CharacterData", "displayName", ref totalChecked, ref englishFound, c => c.displayName);

            report.AppendLine($"\n## 종합");
            report.AppendLine($"- 검사: {totalChecked} SO");
            report.AppendLine($"- 영문 잔존: {englishFound} 건 (whitelist 제외)");

            string path = Path.GetFullPath(Path.Combine(Application.dataPath, ReportRel));
            File.WriteAllText(path, report.ToString());
            Debug.Log($"[KoreanLabelAuditor] 검사 {totalChecked} SO / 영문 잔존 {englishFound} → {path}");
            EditorUtility.DisplayDialog("Korean Label Audit", $"검사: {totalChecked} SO\n영문 잔존: {englishFound}\n\n보고서: {path}", "OK");
        }

        private static void CheckCategory<T>(StringBuilder report, string typeName, string fieldName,
                                              ref int total, ref int englishCount,
                                              System.Func<T, string> getter) where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeName}");
            if (guids.Length == 0) return;
            report.AppendLine($"\n## {typeName} ({guids.Length} 건)");
            int catEnglish = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset == null) continue;
                total++;
                string text = getter(asset);
                if (string.IsNullOrEmpty(text)) continue;
                if (IsWhitelisted(text)) continue;
                if (HasEnglish(text))
                {
                    report.AppendLine($"- `{path}`: \"{text}\"");
                    catEnglish++;
                }
            }
            if (catEnglish == 0) report.AppendLine("  (영문 잔존 0)");
            englishCount += catEnglish;
        }

        private static bool IsWhitelisted(string text)
        {
            foreach (var pattern in WhitelistPatterns)
                if (pattern.IsMatch(text)) return true;
            return false;
        }

        private static bool HasEnglish(string text) => Regex.IsMatch(text, @"[a-zA-Z]");
    }
}
#endif
