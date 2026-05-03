#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5001 boss_system_v2 — 51 Boss SO 자동 마이그레이션 (Sprint 2 / T04).
    /// isMiniB / isBoss boolean + path → BossTier enum 매핑.
    /// Phase 2 Pattern 매핑 (D3=A, ChapterBoss+ 만): Charge→Charge / Pulse→Vine /
    /// Spread→Spread / Summon→Tornado / Vine→Pulse / Tornado→Tornado / BlackHole→BlackHole.
    /// MiniBoss = phase2Pattern None (Phase 2 X, D3=A PRD 그대로).
    /// 메뉴: Tools > LittleHero > Migrate Boss Tier (5001).
    /// WeaponMigrationSeed 패턴 답습.
    /// </summary>
    public static class BossTierMigrationSeed
    {
        [MenuItem("Tools/LittleHero/Migrate Boss Tier (5001)")]
        public static void Migrate()
        {
            var guids = AssetDatabase.FindAssets("t:EnemyData");
            int total = 0, mini = 0, chapter = 0, daily = 0, weekly = 0, none = 0, phase2Wired = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var data = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
                if (data == null) continue;
                total++;

                var oldTier = data.bossTier;
                var oldPhase2 = data.phase2Pattern;

                // 1) BossTier 매핑
                if (data.isMiniB)
                {
                    data.bossTier = BossTier.MiniBoss;
                    mini++;
                }
                else if (data.isBoss)
                {
                    if (path.Contains("DailyBoss"))
                    {
                        data.bossTier = BossTier.DailyBoss;
                        daily++;
                    }
                    else if (path.Contains("WeeklyBoss"))
                    {
                        data.bossTier = BossTier.WeeklyBoss;
                        weekly++;
                    }
                    else
                    {
                        data.bossTier = BossTier.ChapterBoss;
                        chapter++;
                    }
                }
                else
                {
                    data.bossTier = BossTier.None;
                    none++;
                }

                // 2) Phase 2 Pattern 매핑 (D3=A, ChapterBoss+ 만)
                if (data.bossTier >= BossTier.ChapterBoss)
                {
                    data.phase2Pattern = MapPhase2(data.bossAttackType);
                    if (data.phase2Pattern != BossAttackType.None) phase2Wired++;
                }
                else
                {
                    // MiniBoss / None — Phase 2 X
                    data.phase2Pattern = BossAttackType.None;
                }

                if (data.bossTier != oldTier || data.phase2Pattern != oldPhase2)
                {
                    EditorUtility.SetDirty(data);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(
                $"[BossTierMigration] 완료 — 총 {total} EnemyData 처리. " +
                $"None={none} / MiniBoss={mini} / ChapterBoss={chapter} / DailyBoss={daily} / WeeklyBoss={weekly} / Phase2 wired={phase2Wired}"
            );
        }

        // PRD §5.2 Phase 2 Pattern 매핑 (사이클 강화)
        private static BossAttackType MapPhase2(BossAttackType phase1)
        {
            switch (phase1)
            {
                case BossAttackType.Charge:    return BossAttackType.Charge;     // 사이클 1, 변동 X
                case BossAttackType.Pulse:     return BossAttackType.Vine;       // Civ → Nature 결합
                case BossAttackType.Spread:    return BossAttackType.Spread;     // Fantasy, 변동 X
                case BossAttackType.Summon:    return BossAttackType.Tornado;    // Meta → 복합
                case BossAttackType.Vine:      return BossAttackType.Pulse;      // Nature → Civ
                case BossAttackType.Tornado:   return BossAttackType.Tornado;    // Daily, 변동 X
                case BossAttackType.BlackHole: return BossAttackType.BlackHole;  // Weekly, 변동 X
                default:                       return BossAttackType.None;
            }
        }
    }
}
#endif
