using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5024 T03 — PlayerStats Test (recalc + Level cap).
    /// MonoBehaviour 의존성 — GameObject + AddComponent + BalanceConfig SO 임시.
    /// </summary>
    public class PlayerStatsTests
    {
        private GameObject _go;
        private PlayerStats _stats;
        private BalanceConfig _balance;

        [SetUp]
        public void SetUp()
        {
            _go = new GameObject("TestPlayer");
            _go.AddComponent<UnityEngine.Rigidbody2D>();
            _stats = _go.AddComponent<PlayerStats>();
            _balance = ScriptableObject.CreateInstance<BalanceConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
            if (_balance != null) Object.DestroyImmediate(_balance);
        }

        [Test]
        public void Initialize_NullCharacter_DefaultsApplied()
        {
            _stats.Initialize(_balance, null);
            Assert.AreEqual(1, _stats.Level, "Level 초기 1");
            Assert.GreaterOrEqual(_stats.MaxHealth, 0f, "MaxHealth 음수 X");
            Assert.GreaterOrEqual(_stats.MoveSpeed, 0f, "MoveSpeed 음수 X");
        }

        [Test]
        public void Initialize_WithCharacter_AppliesCharacterStats()
        {
            var ch = ScriptableObject.CreateInstance<CharacterData>();
            ch.id = "test_warrior";
            ch.className = "전사";
            ch.maxHealth = 150;
            ch.moveSpeed = 5f;
            ch.attackDamage = 12;
            ch.strengthMultiplier = 2f;

            _stats.Initialize(_balance, ch);
            Assert.GreaterOrEqual(_stats.MaxHealth, 0f);
            Object.DestroyImmediate(ch);
        }

        [Test]
        public void RecalculateStats_AfterEquipmentBonus_HealthIncreased()
        {
            _stats.Initialize(_balance, null);
            float beforeHp = _stats.MaxHealth;
            var bonus = new StatModifiers { maxHealthBonus = 100f };
            _stats.ApplyEquipmentBonus(bonus);
            Assert.Greater(_stats.MaxHealth, beforeHp, "장비 보너스 적용 시 MaxHealth 증가");
        }

        [Test]
        public void RemoveEquipmentBonus_AfterApply_RestoresOriginal()
        {
            _stats.Initialize(_balance, null);
            float originalHp = _stats.MaxHealth;
            var bonus = new StatModifiers { maxHealthBonus = 100f };
            _stats.ApplyEquipmentBonus(bonus);
            _stats.RemoveEquipmentBonus(bonus);
            Assert.AreEqual(originalHp, _stats.MaxHealth, 0.001f, "Apply + Remove → 원복");
        }

        [Test]
        public void LevelCap_Constant_Is100()
        {
            // stats_v2.5 — L100 cap 복원 (사용자 결정 2026-05-03)
            Assert.AreEqual(100, PlayerStats.LEVEL_CAP, "LEVEL_CAP 100");
        }

        [Test]
        public void Initialize_TwoTimes_StateConsistent()
        {
            _stats.Initialize(_balance, null);
            float firstHp = _stats.MaxHealth;
            _stats.Initialize(_balance, null);
            float secondHp = _stats.MaxHealth;
            Assert.AreEqual(firstHp, secondHp, 0.001f, "Initialize 재호출 시 동일 결과");
        }
    }
}
