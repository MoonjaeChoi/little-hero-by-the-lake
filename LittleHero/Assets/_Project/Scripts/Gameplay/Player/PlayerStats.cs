using UnityEngine;

namespace LittleHero
{
    public class PlayerStats : MonoBehaviour
    {
        private BalanceConfig _config;
        private CharacterData _character;
        private StatModifiers _equipmentBonus = new StatModifiers();

        public float MaxHealth { get; private set; }
        public float MoveSpeed { get; private set; }
        public float AttackDamage { get; private set; }
        public float AttackSpeed { get; private set; }
        public float Range { get; private set; }
        public float XpGainMultiplier { get; private set; }

        public void Initialize(BalanceConfig config, CharacterData character = null)
        {
            _config = config;
            _character = character;
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

            // 캐릭터 선택 시 그 스탯을 base로, 미선택 시 BalanceConfig 기본값
            float baseHp  = _character != null ? _character.maxHealth     : _config.baseMaxHealth;
            float baseSpd = _character != null ? _character.moveSpeed     : _config.baseMoveSpeed;
            float baseDmg = _character != null ? _character.attackDamage  : _config.baseAttackDamage;
            float baseAs  = _character != null ? _character.attackSpeed   : _config.baseAttackSpeed;
            float baseR   = _character != null ? _character.range         : _config.baseRange;

            float metaHp  = MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.MaxHealth) : 0f;
            float metaDmg = MetaProgression.Instance != null ? MetaProgression.Instance.GetMetaBonus(MetaUpgradeStat.AttackDamage) : 0f;

            MaxHealth = baseHp  + metaHp  + _equipmentBonus.maxHealthBonus;
            MoveSpeed = baseSpd + _equipmentBonus.speedBonus;
            AttackDamage = baseDmg + metaDmg + _equipmentBonus.attackDamageBonus;
            AttackSpeed = baseAs  + _equipmentBonus.attackSpeedBonus;
            Range = baseR + _equipmentBonus.rangeBonus;
            XpGainMultiplier = 1f + _equipmentBonus.xpGainBonus;
        }
    }
}
