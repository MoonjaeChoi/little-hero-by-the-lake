using LittleHero;
using LittleHero.EditorTools;
using NUnit.Framework;

namespace LittleHero.Tests.Editor
{
    /// <summary>5026 — BossAttackTypeMigrator 매핑 검증.</summary>
    public class BossAttackTypeMigratorTests
    {
        [Test]
        public void ResolveByName_MetaArchetype_ReturnsSummon()
        {
            Assert.AreEqual(BossAttackType.Summon, BossAttackTypeMigrator.ResolveByName("Enemy_Meta_Boss"));
            Assert.AreEqual(BossAttackType.Summon, BossAttackTypeMigrator.ResolveByName("Enemy_Meta_MiniA"));
        }

        [Test]
        public void ResolveByName_V1AnimalForms_ReturnsCharge()
        {
            // D1=B 시각 정합 — 동물형
            Assert.AreEqual(BossAttackType.Charge, BossAttackTypeMigrator.ResolveByName("Enemy_Bear"));
            Assert.AreEqual(BossAttackType.Charge, BossAttackTypeMigrator.ResolveByName("Enemy_BigWolf"));
            Assert.AreEqual(BossAttackType.Charge, BossAttackTypeMigrator.ResolveByName("Enemy_Rabbit"));
        }

        [Test]
        public void ResolveByName_V1CardForms_ReturnsPulse()
        {
            // D1=B 시각 정합 — 카드 (Civ)
            Assert.AreEqual(BossAttackType.Pulse, BossAttackTypeMigrator.ResolveByName("Enemy_QueenHearts"));
            Assert.AreEqual(BossAttackType.Pulse, BossAttackTypeMigrator.ResolveByName("Enemy_CardSoldier"));
        }

        [Test]
        public void ResolveByChapterId_CH06_ReturnsVine()
        {
            // CH 6~14 → Nature → Vine
            Assert.AreEqual(BossAttackType.Vine, BossAttackTypeMigrator.ResolveByChapterId("ch_06_xxx"));
            Assert.AreEqual(BossAttackType.Vine, BossAttackTypeMigrator.ResolveByChapterId("CH14_Boss"));
        }

        [Test]
        public void ResolveByChapterId_HardcoreCH36_ReturnsPulse()
        {
            // Hardcore CH36 → CH21 (Civ) → Pulse
            Assert.AreEqual(BossAttackType.Pulse, BossAttackTypeMigrator.ResolveByChapterId("CH36_Boss"));
            // Hardcore CH50 → CH35 (Fantasy) → Spread
            Assert.AreEqual(BossAttackType.Spread, BossAttackTypeMigrator.ResolveByChapterId("CH50_Boss"));
        }
    }
}
