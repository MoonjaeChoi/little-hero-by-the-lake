using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// CLAUDE.md §15.9 — EnhancementService 단위 테스트.
    /// 정책: 핵심 정적 service 테스트 (회귀 방지).
    /// </summary>
    public class EnhancementServiceTests
    {
        [Test]
        public void CalcSuccessRate_DeterministicLevels_Returns1f()
        {
            for (int level = 0; level < EnhancementService.DETERMINISTIC_LEVEL_CAP; level++)
            {
                Assert.AreEqual(1f, EnhancementService.CalcSuccessRate(level, 0),
                    "결정론 단계 +" + level + " 는 100% 성공");
            }
        }

        [Test]
        public void CalcSuccessRate_FirstRngLevel_Returns0_5()
        {
            // +15 → +16 첫 RNG 단계 (P_BASE × 0.85^0 = 0.50)
            float p = EnhancementService.CalcSuccessRate(EnhancementService.DETERMINISTIC_LEVEL_CAP, 0);
            Assert.AreEqual(0.50f, p, 0.001f);
        }

        [Test]
        public void CalcSuccessRate_HardPity_Returns1f()
        {
            // failCount ≥ 20 → 100% 보장 ('아빠가 도와주는 중')
            float p = EnhancementService.CalcSuccessRate(20, EnhancementService.HARD_PITY_FAILS);
            Assert.AreEqual(1f, p);
        }

        [Test]
        public void CalcSuccessRate_BeyondCurve_FloorAt30()
        {
            // stats_v2.5 — +25+ 무한 강화 P_FLOOR 30% 동결
            float p = EnhancementService.CalcSuccessRate(50, 0);
            Assert.AreEqual(EnhancementService.P_FLOOR, p, 0.001f);
        }

        [Test]
        public void CalcSuccessRate_SoftPity_BoostsRate()
        {
            // failCount=10 (5 초과) → +5%/회 누적
            float baseRate = EnhancementService.CalcSuccessRate(20, 0);
            float boostedRate = EnhancementService.CalcSuccessRate(20, 10);
            Assert.Greater(boostedRate, baseRate);
        }

        [Test]
        public void GetDustCost_BeyondCurve_GrowsExponentially()
        {
            var config = ScriptableObject.CreateInstance<BalanceConfig>();
            config.enhanceCostByLevel = new[] { 100, 200, 500 };
            // +0~+2 는 곡선 / +3+ 는 last × 1.5^over
            Assert.AreEqual(100, EnhancementService.GetDustCost(0, config));
            Assert.AreEqual(500, EnhancementService.GetDustCost(2, config));
            // +3 = 500 × 1.5^1 = 750
            Assert.AreEqual(750, EnhancementService.GetDustCost(3, config));
            // +4 = 500 × 1.5^2 = 1125
            Assert.AreEqual(1125, EnhancementService.GetDustCost(4, config));
            ScriptableObject.DestroyImmediate(config);
        }
    }
}
