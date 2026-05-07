using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5034 — ClassBonusApplier 직업별 특수 스탯 적용 검증 (D4=B + D8=B 누적 가드).
    /// </summary>
    public class ClassBonusApplierTests
    {
        // 5037.0 cascade — 정적 _appliedClass 가 Test 간 공유 → 새 GameObject 의 Reset(stats) 가 잘못된 Remove 호출.
        // Test 격리 보장 — 정적 상태만 fresh 로 강제 reset.
        [SetUp]
        public void ResetStaticState()
        {
            var f = typeof(ClassBonusApplier).GetField("_appliedClass",
                BindingFlags.Static | BindingFlags.NonPublic);
            f?.SetValue(null, null);
        }

        [Test]
        public void Warrior_AddsBossDamageBonus()
        {
            var go = new GameObject("TestPlayerStats");
            var stats = go.AddComponent<PlayerStats>();
            ClassBonusApplier.Reset(stats);
            ClassBonusApplier.Apply(stats, CharacterClass.Warrior);
            Assert.AreEqual(ClassBonusApplier.WARRIOR_BOSS_DMG, stats.BossDamageBonus, 0.0001f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Archer_AddsCritChanceBonus()
        {
            var go = new GameObject("TestPlayerStats");
            var stats = go.AddComponent<PlayerStats>();
            ClassBonusApplier.Reset(stats);
            ClassBonusApplier.Apply(stats, CharacterClass.Archer);
            Assert.AreEqual(ClassBonusApplier.ARCHER_CRIT, stats.CritChanceBonus, 0.0001f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Mage_AddsAoeBonus()
        {
            var go = new GameObject("TestPlayerStats");
            var stats = go.AddComponent<PlayerStats>();
            ClassBonusApplier.Reset(stats);
            ClassBonusApplier.Apply(stats, CharacterClass.Mage);
            Assert.AreEqual(ClassBonusApplier.MAGE_AOE, stats.AoeBonus, 0.0001f);
            Object.DestroyImmediate(go);
        }

        [Test]
        public void ClassChange_RemovesPrevious_AppliesNew()
        {
            var go = new GameObject("TestPlayerStats");
            var stats = go.AddComponent<PlayerStats>();
            ClassBonusApplier.Reset(stats);
            ClassBonusApplier.Apply(stats, CharacterClass.Warrior);
            ClassBonusApplier.Apply(stats, CharacterClass.Mage);
            // Warrior 차감 + Mage 적용
            Assert.AreEqual(0f, stats.BossDamageBonus, 0.0001f, "Warrior bonus 차감 안됨");
            Assert.AreEqual(ClassBonusApplier.MAGE_AOE, stats.AoeBonus, 0.0001f, "Mage bonus 적용 안됨");
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DoubleApply_NoAccumulation()
        {
            var go = new GameObject("TestPlayerStats");
            var stats = go.AddComponent<PlayerStats>();
            ClassBonusApplier.Reset(stats);
            ClassBonusApplier.Apply(stats, CharacterClass.Mage);
            ClassBonusApplier.Apply(stats, CharacterClass.Mage);  // 같은 직업 재적용 — 누적 X
            Assert.AreEqual(ClassBonusApplier.MAGE_AOE, stats.AoeBonus, 0.0001f, "누적 발생");
            Object.DestroyImmediate(go);
        }
    }
}
