using UnityEngine;

namespace LittleHero
{
    public enum AffixOp { ADD, MUL }

    /// <summary>
    /// PlayerStats 의 어떤 채널에 affix 가 영향을 주는지.
    /// AllStats 는 4 코어 동시 +N (반지 1차 등).
    /// </summary>
    public enum StatTarget
    {
        FinalDamage, AttackDamage, CritDamage, CritChance, AttackSpeed,
        ArmorPenetration, ExtraAttackTargets,
        MaxHealth, Armor, DamageReduction, HpRegen,
        MoveSpeed, Range,
        Strength, Agility, Wisdom, Spirit, AllStats,
        XpGain, DustGain, DropRate, CompanionDuration,
    }

    /// <summary>
    /// B3 — Layer 4 주옵션 풀 (affix_pool_v1 §2 / §7.1).
    /// 각 등급별 [min, max] 수치를 보관. AffixRoller 가 가중 추출 + 균등 굴림.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/AffixDefinition")]
    public class AffixDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public StatTarget target;
        public AffixOp op;

        [Header("등급별 [min, max] (Common~Legendary, 길이 5)")]
        public float[] minByRarity = new float[5];
        public float[] maxByRarity = new float[5];

        [Header("표시")]
        [Tooltip("'%' 또는 '/s' 또는 빈 문자열")]
        public string suffix = "%";
        [Range(0, 3)] public int displayDecimals = 1;
    }
}
