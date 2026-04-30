using UnityEngine;

namespace LittleHero
{
    public class PlayerStats : MonoBehaviour
    {
        private BalanceConfig _config;
        private StatModifiers _equipmentBonus = new StatModifiers();

        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Range { get; private set; }
        public float XpGainMultiplier { get; private set; }

        public void Initialize(BalanceConfig config)
        {
            _config = config;
            RecalculateStats();
        }

        public void ApplyEquipmentBonus(StatModifiers bonus)
        {
            _equipmentBonus.maxHealthBonus += bonus.maxHealthBonus;
            _equipmentBonus.speedBonus += bonus.speedBonus;
            _equipmentBonus.attackDamageBonus += bonus.attackDamageBonus;
            _equipmentBonus.attackSpeedBonus += bonus.attackSpeedBonus;
            _equipmentBonus.rangeBonus += bonus.rangeBonus;
            _equipmentBonus.xpGainBonus += bonus.xpGainBonus;
            RecalculateStats();
        }

        public void RemoveEquipmentBonus(StatModifiers bonus)
        {
            _equipmentBonus.maxHealthBonus -= bonus.maxHealthBonus;
            _equipmentBonus.speedBonus -= bonus.speedBonus;
            _equipmentBonus.attackDamageBonus -= bonus.attackDamageBonus;
            _equipmentBonus.attackSpeedBonus -= bonus.attackSpeedBonus;
            _equipmentBonus.rangeBonus -= bonus.rangeBonus;
            _equipmentBonus.xpGainBonus -= bonus.xpGainBonus;
            RecalculateStats();
        }

        private void RecalculateStats()
        {
            // Initialize() 이전에 장비가 장착되면 config가 없으므로 대기
            if (_config == null) return;

            MaxHealth = _config.baseMaxHealth + _equipmentBonus.maxHealthBonus;
            MoveSpeed = _config.baseMoveSpeed + _equipmentBonus.speedBonus;
            AttackDamage = _config.baseAttackDamage + _equipmentBonus.attackDamageBonus;
            AttackSpeed = _config.baseAttackSpeed + _equipmentBonus.attackSpeedBonus;
            Range = _config.baseRange + _equipmentBonus.rangeBonus;
            XpGainMultiplier = 1f + _equipmentBonus.xpGainBonus;
        }
    }
}
