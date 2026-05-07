#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5037.5 — Daily/Weekly EndgameModeData 의 enemyPool 에 5 Variant archetype 추가.
    /// 보스 전투 중 sub-enemy mix spawn — Endgame reuse 입증 옵션 B.
    /// </summary>
    public static class EndgameVariantSeeder
    {
        [MenuItem("Tools/LittleHero/Seed Endgame Variants — Daily/Weekly enemyPool")]
        public static void SeedEndgameEnemyPools()
        {
            // 5037.1 답습 — Variant SO 보장
            VariantSoFactory.EnsureAllVariants();
            VariantSoFactory.EnsureProjectilePrefabs();

            int wired = 0;
            wired += WireEndgame("Assets/_Project/Data/Endgame/daily_boss.asset",  spawnCount: 12) ? 1 : 0;
            wired += WireEndgame("Assets/_Project/Data/Endgame/weekly_boss.asset", spawnCount: 20) ? 1 : 0;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[EndgameVariantSeeder] Daily/Weekly enemyPool wiring 완료 — {wired}/2.");
        }

        private static bool WireEndgame(string path, int spawnCount)
        {
            var mode = AssetDatabase.LoadAssetAtPath<EndgameModeData>(path);
            if (mode == null) { Debug.LogWarning($"[EndgameVariantSeeder] {path} 부재"); return false; }

            // 5 Variant archetype 모두 추가 (Stationary/Charger/Splitter/Ranged/Bomber)
            var variants = new[] {
                LoadVariant("Variant_Stationary_Sentry"),
                LoadVariant("Variant_Charger_Bull"),
                LoadVariant("Variant_Splitter_Bear"),
                LoadVariant("Variant_Ranged_Archer"),
                LoadVariant("Variant_Bomber_Mine"),
            };
            mode.enemyPool = variants;
            mode.subEnemySpawnCount = spawnCount;
            EditorUtility.SetDirty(mode);
            return true;
        }

        private static EnemyData LoadVariant(string id)
            => AssetDatabase.LoadAssetAtPath<EnemyData>($"Assets/_Project/Data/Enemies/Variants/{id}.asset");
    }
}
#endif
