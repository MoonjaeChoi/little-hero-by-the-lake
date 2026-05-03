using NUnit.Framework;

namespace LittleHero.Tests
{
    /// <summary>
    /// CLAUDE.md §15.9 — StarforceService 곡선 테스트.
    /// gacha_v1.md §3 + stats_v2.5 무한 강화 (cap 폐기, ⭐25+ 외삽).
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
        public void GetBaseMultiplierForLevel_Star25_Returns200Percent()
        {
            // ⭐25 = +200% (곡선 마지막)
            Assert.AreEqual(2f, StarforceService.GetBaseMultiplierForLevel(25), 0.001f);
        }

        [Test]
        public void GetBaseMultiplierForLevel_BeyondCurve_GrowsLinearly()
        {
            // stats_v2.5 — ⭐25+ 매 단계 +5% (200% → 205% → 210% ...)
            float at25 = StarforceService.GetBaseMultiplierForLevel(25);
            float at26 = StarforceService.GetBaseMultiplierForLevel(26);
            float at50 = StarforceService.GetBaseMultiplierForLevel(50);

            Assert.AreEqual(at25 + StarforceService.MUL_GROWTH_OVER, at26, 0.001f);
            // ⭐50 = 200% + 25 × 5% = 325%
            Assert.AreEqual(2f + 25 * StarforceService.MUL_GROWTH_OVER, at50, 0.001f);
        }

        [Test]
        public void GetSuccessRateForLevel_Star10_Returns100Percent()
        {
            // ⭐0~10 = 100%
            for (int i = 0; i <= 10; i++)
                Assert.AreEqual(1f, StarforceService.GetSuccessRateForLevel(i), 0.001f,
                    "⭐" + i + " 는 100%");
        }

        [Test]
        public void GetSuccessRateForLevel_Star24_Returns30Percent()
        {
            // ⭐24→⭐25 = 30%
            Assert.AreEqual(0.30f, StarforceService.GetSuccessRateForLevel(24), 0.001f);
        }

        [Test]
        public void GetSuccessRateForLevel_BeyondCurve_FloorAt30()
        {
            // stats_v2.5 — ⭐25+ 무한 30% 동결
            Assert.AreEqual(StarforceService.RATE_FLOOR,
                StarforceService.GetSuccessRateForLevel(50), 0.001f);
            Assert.AreEqual(StarforceService.RATE_FLOOR,
                StarforceService.GetSuccessRateForLevel(100), 0.001f);
        }

        [Test]
        public void GetCostForLevel_Star24_Returns100K()
        {
            // ⭐24→⭐25 = 100,000 조각
            Assert.AreEqual(100000, StarforceService.GetCostForLevel(24));
        }

        [Test]
        public void GetCostForLevel_BeyondCurve_GrowsExponentially()
        {
            // stats_v2.5 — ⭐25+ 비용 last × 1.4^over
            int last = StarforceService.GetCostForLevel(24);  // 100000
            int at25 = StarforceService.GetCostForLevel(25);  // 100000 × 1.4
            int at26 = StarforceService.GetCostForLevel(26);  // 100000 × 1.4²

            Assert.Greater(at25, last);
            Assert.Greater(at26, at25);
            // ⭐25→⭐26 = 100K × 1.4 = 140K
            Assert.AreEqual(140000, at25);
        }

        [Test]
        public void IsMaxLevel_AlwaysFalse()
        {
            // stats_v2.5 — 무한 (cap X)
            Assert.IsFalse(StarforceService.IsMaxLevel(EquipSlot.Hat));
            Assert.IsFalse(StarforceService.IsMaxLevel(EquipSlot.Weapon));
        }
    }
}
