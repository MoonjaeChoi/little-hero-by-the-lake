using UnityEngine;

namespace LittleHero
{
    public enum SlotType { Head, Top, Hand, Foot, Acc1, Acc2, Bag }
    // PIVOT 2026-05-02: 6 등급 (메이플 키우기 정합). Uncommon = Magic 으로 재명명.
    public enum Rarity { Common, Magic, Rare, Epic, Legendary, Mythic }
    public enum AttackPatternType { None, Projectile, Aura, Orbit, Rain, Trail, Pulse, Summon, Slash }

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

        // PIVOT 2026-05-02: 효과 변형 8종 (gacha_v1.md §효과 변형)
        [Header("효과 변형 (선택)")]
        [Tooltip("관통 — 추가 적 N마리 통과")]
        public int pierceCount;
        [Tooltip("연쇄 — 감전 점프 적 수")]
        public int chainCount;
        [Tooltip("연쇄 점프 사거리")]
        public float chainRange;
        [Tooltip("둔화 — 0~1 비율 (0.2 = -20% 이속)")]
        public float slowAmount;
        public float slowDuration;
        [Tooltip("감전 스턴 시간 (초)")]
        public float stunDuration;
        [Tooltip("지속 데미지 (초당)")]
        public float dotDamage;
        public float dotDuration;
        [Tooltip("흡혈 — 0~1 비율 (0.05 = 5% 회복)")]
        public float lifestealRatio;
        [Tooltip("적중 시 절대 회복량")]
        public float healOnHit;
        [Tooltip("동시 다수 타격 — Heavy/Slash 전용 (한 번에 N마리 동시 데미지)")]
        public int maxTargets;

        [Header("패턴 변형 (선택)")]
        [Tooltip("Spread fan 각도 (도). 0 = 단발")]
        public float spreadAngle;
        [Tooltip("Orbit 회귀 — true 면 부메랑 (탕탕특공대)")]
        public bool isBoomerang;
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
