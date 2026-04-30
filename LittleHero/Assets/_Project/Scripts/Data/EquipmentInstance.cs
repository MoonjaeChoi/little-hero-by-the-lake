using System;

namespace LittleHero
{
    /// <summary>
    /// B2 — 굴림 결과 인스턴스 (equipment_v1 §4.2 + item_drop §5.1).
    /// 6줄 affix(주 3 + 부 3) + 강화 레벨 + 천장 카운터 + 신화 옵션.
    /// SaveData.equipmentInstances 에 배열 직렬화.
    /// </summary>
    [Serializable]
    public class EquipmentInstance
    {
        public string instanceId;        // GUID
        public string templateId;        // EquipmentTemplate.id 참조
        public int enhanceLevel;         // 0~25
        public int failCount;            // 소프트/하드 천장 카운터 (equipment §3.4)

        public AffixLine[] mainAffixes;  // 길이 3 (affix_pool §1)
        public AffixLine[] subAffixes;   // 길이 3 (item_drop §1)

        public string mythicId;          // null/공백 = 미발현 (equipment §3.7)

        public bool isLocked;            // 장착 보호 (실수로 분해 방지)
        public bool isEquipped;          // 현재 장착 중
        public InstanceHistory history = new InstanceHistory();
    }

    [Serializable]
    public class AffixLine
    {
        public string affixId;           // AffixDefinition.id 또는 SubAffixDefinition.id
        public string targetName;        // StatTarget.ToString() 캐시
        public string opName;            // "ADD" | "MUL"
        public float value;
        public bool locked;              // 라인 잠금 (리롤 보호, affix_pool §4.2)
    }

    [Serializable]
    public class InstanceHistory
    {
        public int totalEnhanceAttempts;
        public int pityTriggeredCount;
        public int mythicAttempts;
        public int mileage;              // 신화 발현 누적 마일리지 (equipment §3.7)
    }
}
