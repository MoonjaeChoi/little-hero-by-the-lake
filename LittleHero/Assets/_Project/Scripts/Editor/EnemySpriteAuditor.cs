#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5008 — Enemy sprite alpha audit. AssetImporter readable 임시 복원 패턴 (D1=C):
    /// audit 시 isReadable=true 일시 적용 → GetPixels32 → finally 복원. 임포트 설정 변동 0.
    /// V2 archetype 폴백 검증 (D2=C): EnemyData.icon 참조가 손상 sprite 인지 검사 →
    /// fallback OK 시 chapter override 손상은 5008.1 anchor / 깨짐 시 Sprint 5.5 활성화.
    /// </summary>
    public static class EnemySpriteAuditor
    {
        private const string ENEMY_ROOT = "Assets/_Project/Art/Generated_v2/Enemies";
        private const string ENEMY_DATA_DIR = "Assets/_Project/Data/Enemies";
        private const string REPORT_PATH = "docs/little_hero/enemy_sprite_audit_report.md";
        private const float DAMAGE_LO = 0.05f;  // 5% 미만 = 손상 (전부 불투명)
        private const float DAMAGE_HI = 0.95f;  // 95% 초과 = 손상 (전부 투명)
        private const float HEALTHY_LO = 0.30f; // 30~70% = 건강 sticker (캐릭터 + 여백)
        private const float HEALTHY_HI = 0.70f;

        [MenuItem("Tools/LittleHero/Audit Enemy Sprite Quality")]
        public static void AuditAll()
        {
            var entries = new List<Entry>();
            foreach (var path in EnumeratePngPaths(ENEMY_ROOT)) entries.Add(AuditOne(path));

            var damaged = entries.FindAll(e => e.ratio < DAMAGE_LO || e.ratio > DAMAGE_HI);
            var fallbackOk = ValidateV2Fallback(damaged);
            WriteReport(entries, damaged, fallbackOk);

            Debug.Log($"[EnemySpriteAuditor] PNG={entries.Count} damaged={damaged.Count} v2_fallback={(fallbackOk ? "OK" : "BROKEN")} → {REPORT_PATH}");
        }

        private static Entry AuditOne(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            bool wasReadable = importer != null && importer.isReadable;
            try
            {
                if (importer != null && !wasReadable)
                {
                    importer.isReadable = true;
                    importer.SaveAndReimport();
                }
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                float ratio = tex != null ? ComputeTransparentRatio(tex.GetPixels32()) : -1f;
                return new Entry { path = path, ratio = ratio, category = ResolveCategory(path) };
            }
            finally
            {
                if (importer != null && !wasReadable)
                {
                    importer.isReadable = false;
                    importer.SaveAndReimport();
                }
            }
        }

        private static float ComputeTransparentRatio(Color32[] pixels)
        {
            if (pixels == null || pixels.Length == 0) return 0f;
            int t = 0;
            for (int i = 0; i < pixels.Length; i++) if (pixels[i].a < 16) t++;
            return (float)t / pixels.Length;
        }

        private static string ResolveCategory(string path)
        {
            if (path.Contains("/Chapters/")) return "ChapterOverride";
            if (path.Contains("/V1/")) return "V1";
            string n = Path.GetFileNameWithoutExtension(path);
            if (n.StartsWith("Enemy_Meta_")) return "Meta";
            if (n.StartsWith("Enemy_Nature_")) return "Nature";
            if (n.StartsWith("Enemy_Animal_")) return "Animal";
            if (n.StartsWith("Enemy_Civ_")) return "Civ";
            if (n.StartsWith("Enemy_Fantasy_")) return "Fantasy";
            if (n.StartsWith("DailyBoss_")) return "DailyBoss";
            if (n.StartsWith("WeeklyBoss_")) return "WeeklyBoss";
            return "Other";
        }

        private static bool ValidateV2Fallback(List<Entry> damaged)
        {
            // D2=C: EnemyData.icon 참조 중 손상 sprite 가 있으면 fallback 깨짐.
            // 현재 PhaseFContentSeed 는 icon 자동 wiring 안 함 → 대부분 null. fallback OK 로 보고.
            var damagedPaths = new HashSet<string>();
            foreach (var d in damaged) damagedPaths.Add(d.path);
            foreach (var guid in AssetDatabase.FindAssets("t:EnemyData", new[] { ENEMY_DATA_DIR }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (data == null || data.icon == null) continue;
                var iconPath = AssetDatabase.GetAssetPath(data.icon);
                if (damagedPaths.Contains(iconPath)) return false;
            }
            return true;
        }

        private static IEnumerable<string> EnumeratePngPaths(string root)
        {
            var dir = Path.Combine(Application.dataPath, root.Substring("Assets/".Length));
            if (!Directory.Exists(dir)) yield break;
            foreach (var f in Directory.GetFiles(dir, "*.png", SearchOption.AllDirectories))
                yield return "Assets/" + f.Substring(Application.dataPath.Length + 1).Replace('\\', '/');
        }

        private static void WriteReport(List<Entry> entries, List<Entry> damaged, bool fallbackOk)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# Enemy Sprite Audit Report");
            sb.AppendLine();
            sb.AppendLine($"> Generated by EnemySpriteAuditor (5008). Total PNG: {entries.Count}. Damaged: {damaged.Count}. V2 fallback: **{(fallbackOk ? "OK" : "BROKEN")}**");
            sb.AppendLine($"> Healthy sticker range: {HEALTHY_LO:P0}~{HEALTHY_HI:P0} transparent pixels. Damage threshold: <{DAMAGE_LO:P0} or >{DAMAGE_HI:P0}.");
            sb.AppendLine();
            sb.AppendLine("## Category Summary");
            sb.AppendLine();
            sb.AppendLine("| Category | Total | Healthy (30~70%) | Damaged |");
            sb.AppendLine("|---|---:|---:|---:|");
            var byCat = new Dictionary<string, (int total, int healthy, int damaged)>();
            foreach (var e in entries)
            {
                if (!byCat.ContainsKey(e.category)) byCat[e.category] = (0, 0, 0);
                var (tot, hea, dmg) = byCat[e.category];
                tot++;
                bool isDamaged = e.ratio < DAMAGE_LO || e.ratio > DAMAGE_HI;
                bool isHealthy = e.ratio >= HEALTHY_LO && e.ratio <= HEALTHY_HI;
                if (isDamaged) dmg++;
                if (isHealthy) hea++;
                byCat[e.category] = (tot, hea, dmg);
            }
            foreach (var kv in byCat)
                sb.AppendLine($"| {kv.Key} | {kv.Value.total} | {kv.Value.healthy} | {kv.Value.damaged} |");
            sb.AppendLine();
            sb.AppendLine("## Damaged PNG List");
            sb.AppendLine();
            if (damaged.Count == 0)
            {
                sb.AppendLine("(none)");
            }
            else
            {
                foreach (var d in damaged)
                    sb.AppendLine($"- `{d.path}` — transparent {d.ratio:P1} ({d.category})");
            }
            sb.AppendLine();
            sb.AppendLine("## V2 Archetype Fallback Decision (D2=C)");
            sb.AppendLine();
            if (fallbackOk)
            {
                sb.AppendLine("- **fallback OK** — 손상 sprite 가 EnemyData.icon 으로 참조되지 않음.");
                sb.AppendLine("- 결정: Sprint 5.5 미활성화. 손상 chapter override 는 5008.1 anchor 우선 처리.");
            }
            else
            {
                sb.AppendLine("- **fallback BROKEN** — 손상 sprite 가 EnemyData.icon 으로 참조됨.");
                sb.AppendLine("- 결정: Sprint 5.5 활성화 권장. 손상 chapter override 즉시 재생성 필요.");
            }
            File.WriteAllText(REPORT_PATH, sb.ToString());
            AssetDatabase.Refresh();
        }

        private struct Entry
        {
            public string path;
            public float ratio;
            public string category;
        }
    }
}
#endif
