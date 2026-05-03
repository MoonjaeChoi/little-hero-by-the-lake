using NUnit.Framework;

namespace LittleHero.Tests
{
    /// <summary>
    /// CLAUDE.md §15.9 — DamageDealer.GetPenalty 다중 타깃 보정 테스트.
    /// damage_system_v2.md §5.2 메이플 풍 보정 곡선 (1.0 / 0.8 / 0.7 / 0.6 floor).
    /// </summary>
    public class DamageDealerTests
    {
        [Test]
        public void GetPenalty_FirstTarget_Returns100Percent()
        {
            Assert.AreEqual(1f, DamageDealer.GetPenalty(0));
        }

        [Test]
        public void GetPenalty_SecondTarget_Returns80Percent()
        {
            Assert.AreEqual(0.8f, DamageDealer.GetPenalty(1));
        }

        [Test]
        public void GetPenalty_ThirdTarget_Returns70Percent()
        {
            Assert.AreEqual(0.7f, DamageDealer.GetPenalty(2));
        }

        [Test]
        public void GetPenalty_FourthTarget_FloorAt60()
        {
            Assert.AreEqual(0.6f, DamageDealer.GetPenalty(3));
        }

        [Test]
        public void GetPenalty_BeyondCurve_StaysAtFloor60()
        {
            // 5+ 타깃은 60% floor 동결 (메이플 26% 완화)
            for (int i = 4; i < 20; i++)
                Assert.AreEqual(0.6f, DamageDealer.GetPenalty(i),
                    "타깃 " + i + " 60% floor 유지");
        }

        [Test]
        public void GetPenalty_NegativeIndex_Returns100Percent()
        {
            // 안전 기본값
            Assert.AreEqual(1f, DamageDealer.GetPenalty(-1));
        }

        [Test]
        public void Penalty_DecreasesMonotonic()
        {
            // 1.0 ≥ 0.8 ≥ 0.7 ≥ 0.6 단조 감소
            Assert.GreaterOrEqual(DamageDealer.GetPenalty(0), DamageDealer.GetPenalty(1));
            Assert.GreaterOrEqual(DamageDealer.GetPenalty(1), DamageDealer.GetPenalty(2));
            Assert.GreaterOrEqual(DamageDealer.GetPenalty(2), DamageDealer.GetPenalty(3));
        }
    }
}
