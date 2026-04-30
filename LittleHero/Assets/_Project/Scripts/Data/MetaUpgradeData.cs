using UnityEngine;

namespace LittleHero
{
    public enum MetaUpgradeStat { MaxHealth, AttackDamage, StarDustGain }

    [CreateAssetMenu(menuName = "LittleHero/MetaUpgrade")]
    public class MetaUpgradeData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;

        public MetaUpgradeStat targetStat;
        [Tooltip("레벨당 가산값. HP/Damage는 절대값, StarDustGain은 배수(0.1=10%)")]
        public float bonusPerLevel;

        public int maxLevel = 5;
        public int baseCost = 5;
        public int costPerLevel = 3;
    }
}
