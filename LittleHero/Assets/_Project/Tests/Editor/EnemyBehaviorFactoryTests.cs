using NUnit.Framework;

namespace LittleHero.Tests.Editor
{
    /// <summary>5035 — EnemyBehaviorFactory + EnemyDeathReactionFactory 통합 검증.</summary>
    public class EnemyBehaviorFactoryTests
    {
        [Test]
        public void EnemyBehaviorFactory_AllTypes_NotNull()
        {
            foreach (EnemyBehaviorType t in System.Enum.GetValues(typeof(EnemyBehaviorType)))
            {
                var b = EnemyBehaviorFactory.Create(t);
                Assert.IsNotNull(b, $"EnemyBehaviorFactory.Create({t}) returned null");
            }
        }

        [Test]
        public void EnemyDeathReactionFactory_AllTypes_NotNull()
        {
            foreach (EnemyDeathReactionType t in System.Enum.GetValues(typeof(EnemyDeathReactionType)))
            {
                var r = EnemyDeathReactionFactory.Create(t);
                Assert.IsNotNull(r, $"EnemyDeathReactionFactory.Create({t}) returned null");
            }
        }

        [Test]
        public void Factory_Follow_ReturnsFollowBehavior()
        {
            var b = EnemyBehaviorFactory.Create(EnemyBehaviorType.Follow);
            Assert.IsInstanceOf<FollowBehavior>(b);
        }

        [Test]
        public void Factory_Stationary_ReturnsStationaryBehavior()
        {
            var b = EnemyBehaviorFactory.Create(EnemyBehaviorType.Stationary);
            Assert.IsInstanceOf<StationaryBehavior>(b);
        }

        [Test]
        public void Factory_Charger_ReturnsChargerBehavior()
        {
            var b = EnemyBehaviorFactory.Create(EnemyBehaviorType.Charger);
            Assert.IsInstanceOf<ChargerBehavior>(b);
        }

        [Test]
        public void Factory_Ranged_ReturnsRangedBehavior()
        {
            var b = EnemyBehaviorFactory.Create(EnemyBehaviorType.Ranged);
            Assert.IsInstanceOf<RangedBehavior>(b);
        }

        [Test]
        public void Factory_Bomber_ReturnsBomberBehavior()
        {
            var b = EnemyBehaviorFactory.Create(EnemyBehaviorType.Bomber);
            Assert.IsInstanceOf<BomberBehavior>(b);
        }

        [Test]
        public void Factory_Splitter_ReturnsFollowBehavior()
        {
            // Splitter 는 추적 + IDeathReaction 분리
            var b = EnemyBehaviorFactory.Create(EnemyBehaviorType.Splitter);
            Assert.IsInstanceOf<FollowBehavior>(b);
        }

        [Test]
        public void DeathReaction_None_ReturnsNoneReaction()
        {
            var r = EnemyDeathReactionFactory.Create(EnemyDeathReactionType.None);
            Assert.IsInstanceOf<NoneReaction>(r);
        }

        [Test]
        public void DeathReaction_Splitter_ReturnsSplitterReaction()
        {
            var r = EnemyDeathReactionFactory.Create(EnemyDeathReactionType.Splitter);
            Assert.IsInstanceOf<SplitterReaction>(r);
        }

        [Test]
        public void DeathReaction_Explode_ReturnsExplodeReaction()
        {
            var r = EnemyDeathReactionFactory.Create(EnemyDeathReactionType.Explode);
            Assert.IsInstanceOf<ExplodeReaction>(r);
        }

        [Test]
        public void NoneReaction_OnDeath_DoesNotThrow()
        {
            var r = new NoneReaction();
            Assert.DoesNotThrow(() => r.OnDeath(null, UnityEngine.Vector3.zero));
        }
    }
}
