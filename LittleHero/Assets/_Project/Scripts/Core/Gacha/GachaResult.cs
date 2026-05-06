namespace LittleHero
{
    /// <summary>
    /// 단일 가챠 결과 (gacha_v1.md §1).
    /// 10연차 의 경우 results[i].isRarePlusGuaranteed 가 true 면 보장 발동.
    /// </summary>
    public class GachaResult
    {
        public EquipmentInstance instance;
        public Rarity rarity;
        public string templateId;
        public int starforceShards;
        public bool isRarePlusGuaranteed;
        public bool causedSummonLevelUp;
        public int newSummonLevel;
    }

    public enum GachaCurrency { Starlight, SummonTicket }
}
