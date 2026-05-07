using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5034 — BalanceConfig.recommended* 매트릭스 검증.
    /// </summary>
    public class BalanceMatrixTests
    {
        private const string BALANCE_PATH = "Assets/_Project/Data/BalanceConfig.asset";

        [Test]
        public void BalanceConfig_RecommendedArrays_Length35()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<BalanceConfig>(BALANCE_PATH);
            Assert.IsNotNull(cfg, "BalanceConfig.asset 미존재");
            // 길이 35 보장 (Tools/LittleHero/Seed BalanceConfig Recommended Matrix 실행 후)
            if (cfg.recommendedLevelByChapter != null && cfg.recommendedLevelByChapter.Length > 0)
                Assert.AreEqual(35, cfg.recommendedLevelByChapter.Length);
            if (cfg.recommendedWeaponEnhanceByChapter != null && cfg.recommendedWeaponEnhanceByChapter.Length > 0)
                Assert.AreEqual(35, cfg.recommendedWeaponEnhanceByChapter.Length);
            if (cfg.recommendedSlotStarforceByChapter != null && cfg.recommendedSlotStarforceByChapter.Length > 0)
                Assert.AreEqual(35, cfg.recommendedSlotStarforceByChapter.Length);
        }

        [Test]
        public void RecommendedLevel_Monotonic_NonDecreasing()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<BalanceConfig>(BALANCE_PATH);
            if (cfg?.recommendedLevelByChapter == null || cfg.recommendedLevelByChapter.Length < 35) return;
            for (int i = 1; i < 35; i++)
                Assert.GreaterOrEqual(cfg.recommendedLevelByChapter[i], cfg.recommendedLevelByChapter[i - 1],
                    $"recommendedLevel[{i}] < [{i - 1}] (non-monotonic)");
        }

        [Test]
        public void RecommendedSlotStarforce_Cap20()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<BalanceConfig>(BALANCE_PATH);
            if (cfg?.recommendedSlotStarforceByChapter == null || cfg.recommendedSlotStarforceByChapter.Length < 35) return;
            for (int i = 0; i < 35; i++)
                Assert.LessOrEqual(cfg.recommendedSlotStarforceByChapter[i], StarforceService.MAX_LEVEL,
                    $"recommendedSlotStarforce[{i}] > MAX_LEVEL ({StarforceService.MAX_LEVEL})");
        }
    }
}
