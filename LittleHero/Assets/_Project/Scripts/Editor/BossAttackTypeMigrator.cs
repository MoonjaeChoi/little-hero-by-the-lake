#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LittleHero;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5026 — 137 보스 EnemyData 중 96개의 bossAttackType=None 잔존 일괄 갱신.
    /// 카테고리 매핑: 5008/5019 정착 (Meta/Nature/Animal/Civ/Fantasy → Summon/Vine/Charge/Pulse/Spread).
    /// </summary>
    public static class BossAttackTypeMigrator
    {
        // 5026 D2=A — 정적 const Dictionary (디자이너 후속 SO 화 후속 검토)
        private static readonly Dictionary<string, BossAttackType> CategoryMap = new()
        {
            { "Meta",    BossAttackType.Summon },
            { "Nature",  BossAttackType.Vine   },
            { "Animal",  BossAttackType.Charge },
            { "Civ",     BossAttackType.Pulse  },
            { "Fantasy", BossAttackType.Spread },
        };

        // 5026 D1=B — V1 11종 시각 정합 매핑
        private static readonly Dictionary<string, BossAttackType> V1NameMap = new()
        {
            { "Enemy_Bear",         BossAttackType.Charge },
            { "Enemy_BigWolf",      BossAttackType.Charge },
            { "Enemy_Rabbit",       BossAttackType.Charge },
            { "Enemy_BeanGiant",    BossAttackType.Vine   },
            { "Enemy_CloudGiant",   BossAttackType.Vine   },
            { "Enemy_Pumpkin",      BossAttackType.Vine   },
            { "Enemy_QueenHearts",  BossAttackType.Pulse  },
            { "Enemy_CardSoldier",  BossAttackType.Pulse  },
            { "Enemy_StarCloud",    BossAttackType.Summon },
            { "Enemy_Firefly",      BossAttackType.Summon },
            { "Enemy_Basic",        BossAttackType.Summon },
        };

        private static readonly Regex V2ArchetypeRegex = new(@"Enemy_(Meta|Nature|Animal|Civ|Fantasy)_");
        private static readonly Regex ChapterRegex    = new(@"(?:^|/)CH(\d{1,2})_");

        public static BossAttackType ResolveByName(string assetName)
        {
            if (V1NameMap.TryGetValue(assetName, out var v1)) return v1;
            var m = V2ArchetypeRegex.Match(assetName);
            if (m.Success && CategoryMap.TryGetValue(m.Groups[1].Value, out var v2)) return v2;
            return BossAttackType.None;
        }

        public static BossAttackType ResolveByChapterId(string chapterId)
        {
            // ch_06_xxx → 6, CH06_MiniA → 6
            var m = Regex.Match(chapterId ?? "", @"(\d{1,2})");
            if (!m.Success) return BossAttackType.None;
            int n = int.Parse(m.Groups[1].Value);
            // Hardcore CH36~50 → CH21~35 카테고리 (Civ/Fantasy)
            if (n >= 36) n = ((n - 36) % 35) + 21;
            string cat = n <= 5  ? "Meta"
                       : n <= 14 ? "Nature"
                       : n <= 20 ? "Animal"
                       : n <= 27 ? "Civ"
                       : "Fantasy";
            return CategoryMap.TryGetValue(cat, out var t) ? t : BossAttackType.None;
        }

        [MenuItem("Tools/LittleHero/Migrate Boss Attack Type")]
        public static void Run()
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.EnemyData");
            int migrated = 0, skipped = 0, fallback = 0;
            var counts = new Dictionary<BossAttackType, int>();
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var d = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (d == null) continue;
                if (d.bossTier == BossTier.None) continue;
                if (d.bossAttackType != BossAttackType.None) { skipped++; continue; }

                var byName = ResolveByName(d.name);
                var byChap = byName != BossAttackType.None ? byName : ResolveByChapterId(path);
                if (byChap == BossAttackType.None) { byChap = BossAttackType.Charge; fallback++; }

                d.bossAttackType = byChap;
                EditorUtility.SetDirty(d);
                migrated++;
                counts[byChap] = counts.GetValueOrDefault(byChap, 0) + 1;
            }
            AssetDatabase.SaveAssets();

            string breakdown = string.Join(" / ", new[] {
                $"Charge:{counts.GetValueOrDefault(BossAttackType.Charge, 0)}",
                $"Pulse:{counts.GetValueOrDefault(BossAttackType.Pulse, 0)}",
                $"Spread:{counts.GetValueOrDefault(BossAttackType.Spread, 0)}",
                $"Summon:{counts.GetValueOrDefault(BossAttackType.Summon, 0)}",
                $"Vine:{counts.GetValueOrDefault(BossAttackType.Vine, 0)}",
            });
            Debug.Log($"[BossAttackTypeMigrator] migrated={migrated} skipped={skipped} fallback={fallback} ({breakdown})");
            EditorUtility.DisplayDialog("Boss Attack Type Migration",
                $"갱신: {migrated} SO\n건너뜀: {skipped} (이미 wiring)\nFallback Charge: {fallback}\n\n{breakdown}", "OK");
        }
    }
}
#endif
