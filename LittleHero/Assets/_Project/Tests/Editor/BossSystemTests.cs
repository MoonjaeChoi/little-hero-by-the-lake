using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5001 boss_system_v2 — BossPatternFactory + IBossPhase + BossLootTable 단위 테스트.
    /// CLAUDE.md §15.9 정책 + Sprint 6 / T09.
    /// </summary>
    public class BossSystemTests
    {
        // === BossPatternFactory 7 패턴 ===

        [Test]
        public void BossPatternFactory_Create_Charge_ReturnsChargePattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.Charge);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossChargeAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_Pulse_ReturnsPulsePattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.Pulse);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossPulseAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_Spread_ReturnsSpreadPattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.Spread);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossSpreadAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_Summon_ReturnsSummonPattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.Summon);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossSummonAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_Vine_ReturnsVinePattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.Vine);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossVineAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_Tornado_ReturnsTornadoPattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.Tornado);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossTornadoAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_BlackHole_ReturnsBlackHolePattern()
        {
            var p = BossPatternFactory.Create(BossAttackType.BlackHole);
            Assert.IsNotNull(p);
            Assert.IsInstanceOf<BossBlackHoleAttack>(p);
        }

        [Test]
        public void BossPatternFactory_Create_None_ReturnsNull()
        {
            var p = BossPatternFactory.Create(BossAttackType.None);
            Assert.IsNull(p);
        }

        // === BossLootTable 4 인스턴스 ===

        [Test]
        public void BossLootTable_Tier_MatchesAllInstances()
        {
            var guids = AssetDatabase.FindAssets("t:BossLootTable");
            Assert.AreEqual(4, guids.Length, "BossLootTable 4 인스턴스 (Mini/Chapter/Daily/Weekly)");

            bool foundMini = false, foundChapter = false, foundDaily = false, foundWeekly = false;
            foreach (var g in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(g);
                var lt = AssetDatabase.LoadAssetAtPath<BossLootTable>(p);
                Assert.IsNotNull(lt);
                Assert.AreNotEqual(BossTier.None, lt.tier, $"{p} tier=None 금지");
                Assert.GreaterOrEqual(lt.guaranteedMythicChance, 0f);
                Assert.LessOrEqual(lt.guaranteedMythicChance, 1f);

                switch (lt.tier)
                {
                    case BossTier.MiniBoss: foundMini = true; break;
                    case BossTier.ChapterBoss: foundChapter = true; break;
                    case BossTier.DailyBoss: foundDaily = true; break;
                    case BossTier.WeeklyBoss: foundWeekly = true; break;
                }
            }
            Assert.IsTrue(foundMini && foundChapter && foundDaily && foundWeekly,
                "Tier 4종 모두 존재 (Mini/Chapter/Daily/Weekly)");
        }

        // === BossPhase State 패턴 (10번째) ===

        [Test]
        public void BossPhase2_DefaultSpeedMultiplier_Is1_3()
        {
            // BossPhase2Aggressive 의 기본 _speedMultiplier 가 1.3 인지 검증 (D2=C 결정).
            // OnEnter 호출 없이 default 값 검증 — 인스턴스 생성 후 reflection.
            var phase = new BossPhase2Aggressive();
            var fld = typeof(BossPhase2Aggressive).GetField("_speedMultiplier",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.IsNotNull(fld, "BossPhase2Aggressive 에 _speedMultiplier 필드 존재 (D2=C)");
            float val = (float)fld.GetValue(phase);
            Assert.AreEqual(1.3f, val, 0.001f, "기본 _speedMultiplier = 1.3 (PRD 원안)");
        }
    }
}
