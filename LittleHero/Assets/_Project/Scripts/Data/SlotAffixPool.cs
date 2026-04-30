using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// B5 — 슬롯별 주옵션 풀 (affix_pool §3 / §7.2).
    /// 가중치 6:3:1 로 1차/2차/3차 affix 추출. AffixRoller(C1) 가 사용.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/SlotAffixPool")]
    public class SlotAffixPool : ScriptableObject
    {
        public EquipSlot slot;
        public AffixDefinition tier1;
        public AffixDefinition tier2;
        public AffixDefinition tier3;

        [Header("가중치 (합 = 1.0)")]
        [Range(0f, 1f)] public float weightTier1 = 0.6f;
        [Range(0f, 1f)] public float weightTier2 = 0.3f;
        [Range(0f, 1f)] public float weightTier3 = 0.1f;
    }
}
