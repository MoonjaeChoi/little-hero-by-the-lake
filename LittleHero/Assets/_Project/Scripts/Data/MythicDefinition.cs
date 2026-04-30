using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// B10 — 신화 옵션 "아빠의 마법" (equipment_v1 §3.7).
    /// 추억 4종 모두 발견 후 활성화 (MetaProgression.AllMemoriesDiscovered).
    /// 잭팟 ARPG Part 2 마일리지 공식으로 발현 — base_rate 0.005 + mileage/200.
    /// 발현 효과는 EffectResolver(C4) 가 EquipmentInstance.mythicId 로 조회 후 적용.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/MythicDefinition")]
    public class MythicDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string flavorText;
        public Sprite icon;

        [Header("발동 효과 — 자유 텍스트로 선언, 코드에서 id 매칭")]
        [Tooltip("Effect 적용은 EffectResolver(C4) 가 본 SO 의 id 로 분기 처리")]
        public string effectDescription;

        [Header("슬롯 제한 (비어있으면 모든 슬롯 가능)")]
        public EquipSlot[] slotRestriction;
    }
}
