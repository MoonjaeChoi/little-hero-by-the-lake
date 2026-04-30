namespace LittleHero
{
    /// <summary>
    /// B5 — 12 슬롯 시스템 (equipment_v1 §4 + affix_pool §3).
    /// 기존 SlotType(Head/Top/Hand/Foot/Acc1/Acc2/Bag) 은 EquipmentData(레거시 16종)에서만 사용.
    /// 신규 EquipmentTemplate / SlotAffixPool 등은 본 enum 채택.
    /// 순서: 주 7 (Hat~Shoes) + 장신구 5 (Earring~Pet, 메이플 키우기 매칭).
    /// </summary>
    public enum EquipSlot
    {
        Hat,        // 모자
        Top,        // 상의
        Bottom,     // 하의
        Cloak,      // 망토
        Gloves,     // 장갑
        Weapon,     // 무기
        Shoes,      // 신발
        Earring,    // 귀걸이
        Glasses,    // 안경
        Necklace,   // 목걸이
        Ring,       // 반지
        Pet,        // 펫
    }
}
