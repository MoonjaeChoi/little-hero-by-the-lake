using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// B1 — 장비 마스터 (equipment_v1 §4.1).
    /// 운영 중 변하지 않는 데이터. EquipmentInstance(B2) 가 본 SO 의 id 로 참조.
    /// 기존 EquipmentData(16종, 레거시 7슬롯 SlotType)는 그대로 유지 — 본 클래스는 신규 12슬롯·6줄 affix 시스템 전용.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/EquipmentTemplate")]
    public class EquipmentTemplate : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public EquipSlot slot;
        public Rarity rarity;

        [Header("베이스 스탯 (강화·옵션 무시한 순수 마스터 값)")]
        public StatModifiers baseStats;

        [Header("자동 공격 (선택)")]
        public AttackPatternType attackPattern;
        public AttackPatternConfig attackConfig;

        [Header("강화·옵션 사양")]
        [Tooltip("강화 +0 ~ +N (equipment §3.3, 기본 25)")]
        public int maxEnhanceLevel = 25;
        [Tooltip("주옵션 라인 수 (affix_pool §1, 기본 3)")]
        public int mainAffixLineCount = 3;
        [Tooltip("부옵션 라인 수 (item_drop §1, 기본 3)")]
        public int subAffixLineCount = 3;

        // 세트(setId)는 폐기 (README §1 결정).
    }
}
