using NUnit.Framework;

namespace LittleHero.Tests
{
    /// <summary>
    /// CLAUDE.md §15.9 — StarforceService 곡선 테스트.
    /// 사용자 결정 2026-05-05 — 메이플 본판 답습 + 파괴 폐기 + ★20 cap.
    /// 곡선: ★0~14 본판 (95→30) / ★14→15 30% / ★15→16 25% / ★16→17 20% / ★17→18 15% / ★18→19 10% / ★19→20 5%.
    /// </summary>
    public class StarforceServiceTests
    {
        [Test]
        public void GetBaseMultiplierForLevel_Star0_Returns0()
        {
            Assert.AreEqual(0f, StarforceService.GetBaseMultiplierForLevel(0), 0.001f);
        }

        [Test]
        public void GetBaseMultiplierForLevel_Star15_Returns100Percent()
        {
            // ⭐15 = +100%
            Assert.AreEqual(1f, StarforceService.GetBaseMultiplierForLevel(15), 0.001f);
        }

        [Test]
        public void GetBaseMultiplierForLevel_Star20_Returns200Percent()
        {
            // ⭐20 = +200% (곡선 마지막, cap)
            Assert.AreEqual(2f, StarforceService.GetBaseMultiplierForLevel(20), 0.001f);
        }

        [Test]
        public void GetBaseMultiplierForLevel_BeyondCap_StaysAt200Percent()
        {
            // ★20 cap — 그 이상 시도해도 200% 동결
            Assert.AreEqual(2f, StarforceService.GetBaseMultiplierForLevel(21), 0.001f);
            Assert.AreEqual(2f, StarforceService.GetBaseMultiplierForLevel(50), 0.001f);
        }

        [Test]
        public void GetSuccessRateForLevel_Star0_Returns95Percent()
        {
            Assert.AreEqual(0.95f, StarforceService.GetSuccessRateForLevel(0), 0.001f);
        }

        [Test]
        public void GetSuccessRateForLevel_Star10_Returns50Percent()
        {
            // 사용자 결정 — ★10→11 = 50% (PC 본판 답습)
            Assert.AreEqual(0.50f, StarforceService.GetSuccessRateForLevel(10), 0.001f);
        }

        [Test]
        public void GetSuccessRateForLevel_UserDefinedCurve_Star14To19()
        {
            // 사용자 결정 — ★14~19 사용자 정의 곡선
            Assert.AreEqual(0.30f, StarforceService.GetSuccessRateForLevel(14), 0.001f, "★14→15 = 30%");
            Assert.AreEqual(0.25f, StarforceService.GetSuccessRateForLevel(15), 0.001f, "★15→16 = 25%");
            Assert.AreEqual(0.20f, StarforceService.GetSuccessRateForLevel(16), 0.001f, "★16→17 = 20%");
            Assert.AreEqual(0.15f, StarforceService.GetSuccessRateForLevel(17), 0.001f, "★17→18 = 15%");
            Assert.AreEqual(0.10f, StarforceService.GetSuccessRateForLevel(18), 0.001f, "★18→19 = 10%");
            Assert.AreEqual(0.05f, StarforceService.GetSuccessRateForLevel(19), 0.001f, "★19→20 = 5%");
        }

        [Test]
        public void GetSuccessRateForLevel_AtCap_Returns0()
        {
            // ★20 cap — 다음 시도 X (성공률 0)
            Assert.AreEqual(0f, StarforceService.GetSuccessRateForLevel(20), 0.001f);
            Assert.AreEqual(0f, StarforceService.GetSuccessRateForLevel(50), 0.001f);
        }

        [Test]
        public void GetCostForLevel_Star19_Returns24K()
        {
            // ★19→20 = 24,000 조각 (마지막 강화 비용)
            Assert.AreEqual(24000, StarforceService.GetCostForLevel(19));
        }

        [Test]
        public void GetCostForLevel_AtCap_Returns0()
        {
            // ★20 cap — 비용 X (강화 불가)
            Assert.AreEqual(0, StarforceService.GetCostForLevel(20));
            Assert.AreEqual(0, StarforceService.GetCostForLevel(50));
        }

        [Test]
        public void IsMaxLevel_DependsOnCurrentLevel()
        {
            // ★20 cap — 슬롯 단계 따라 결정 (실제 호출 시 SaveData 의존)
            // 정적 메서드 의미: GetLevel(slot) >= MAX_LEVEL.
            Assert.AreEqual(20, StarforceService.MAX_LEVEL);
        }
    }
}
