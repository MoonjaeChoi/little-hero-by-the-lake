using UnityEngine;

namespace LittleHero
{
    public enum SlotType { Head, Top, Hand, Foot, Acc1, Acc2, Bag }
    public enum Rarity { Common, Uncommon, Rare, Epic, Legendary }
    public enum AttackPatternType { None, Projectile, Aura, Orbit, Rain, Trail, Pulse, Summon }

    [System.Serializable]
    public class StatModifiers
    {
        public float maxHealthBonus;
        public float speedBonus;
        public float attackDamageBonus;
        public float attackSpeedBonus;
        public float rangeBonus;
        public float xpGainBonus;
    }

    [System.Serializable]
    public class AttackPatternConfig
    {
        public float damage;
        public float attackInterval;
        public float range;
        public float projectileSpeed;
        public int count;
        public GameObject projectilePrefab;
    }

    [CreateAssetMenu(menuName = "LittleHero/Equipment")]
    public class EquipmentData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public SlotType slot;
        public Rarity rarity;

        [Header("스탯 보너스")]
        public StatModifiers statBonus;

        [Header("자동 공격 (선택)")]
        public AttackPatternType attackPattern;
        public AttackPatternConfig attackConfig;

        [Header("세트 효과 (선택)")]
        public string setId;
    }
}
