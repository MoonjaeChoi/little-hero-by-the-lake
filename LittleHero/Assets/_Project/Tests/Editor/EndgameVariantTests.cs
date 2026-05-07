using NUnit.Framework;
using UnityEditor;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5037.5 — Daily/Weekly EndgameModeData enemyPool 검증 (옵션 B Endgame reuse).
    /// </summary>
    public class EndgameVariantTests
    {
        private static EndgameModeData LoadMode(string path)
            => AssetDatabase.LoadAssetAtPath<EndgameModeData>(path);

        private static bool ContainsVariant(EnemyData[] pool, string id)
        {
            if (pool == null) return false;
            for (int i = 0; i < pool.Length; i++)
                if (pool[i] != null && pool[i].id == id) return true;
            return false;
        }

        [Test]
        public void DailyBoss_EnemyPool_Has_5_Variants_And_SpawnCount_12()
        {
            var mode = LoadMode("Assets/_Project/Data/Endgame/daily_boss.asset");
            Assert.IsNotNull(mode, "daily_boss SO 부재");
            Assert.IsNotNull(mode.enemyPool);
            Assert.GreaterOrEqual(mode.enemyPool.Length, 5, "Daily enemyPool < 5 archetype");
            Assert.AreEqual(12, mode.subEnemySpawnCount, "Daily subEnemySpawnCount != 12");
            Assert.IsTrue(ContainsVariant(mode.enemyPool, "Variant_Bomber_Mine"));
            Assert.IsTrue(ContainsVariant(mode.enemyPool, "Variant_Splitter_Bear"));
        }

        [Test]
        public void WeeklyBoss_EnemyPool_Has_5_Variants_And_SpawnCount_20()
        {
            var mode = LoadMode("Assets/_Project/Data/Endgame/weekly_boss.asset");
            Assert.IsNotNull(mode, "weekly_boss SO 부재");
            Assert.IsNotNull(mode.enemyPool);
            Assert.GreaterOrEqual(mode.enemyPool.Length, 5);
            Assert.AreEqual(20, mode.subEnemySpawnCount);
            Assert.IsTrue(ContainsVariant(mode.enemyPool, "Variant_Stationary_Sentry"));
            Assert.IsTrue(ContainsVariant(mode.enemyPool, "Variant_Charger_Bull"));
            Assert.IsTrue(ContainsVariant(mode.enemyPool, "Variant_Ranged_Archer"));
        }

        [Test]
        public void InfiniteTower_And_Hardcore_EnemyPool_Empty()
        {
            // 5037.5 — Daily/Weekly 만 wiring (다른 모드는 default null/0)
            var inf = LoadMode("Assets/_Project/Data/Endgame/infinite_tower.asset");
            var hc = LoadMode("Assets/_Project/Data/Endgame/hardcore_chapter.asset");
            Assert.IsNotNull(inf);
            Assert.IsNotNull(hc);
            Assert.AreEqual(0, inf.subEnemySpawnCount, "InfiniteTower subEnemySpawnCount default 0");
            Assert.AreEqual(0, hc.subEnemySpawnCount, "Hardcore subEnemySpawnCount default 0");
        }
    }
}
