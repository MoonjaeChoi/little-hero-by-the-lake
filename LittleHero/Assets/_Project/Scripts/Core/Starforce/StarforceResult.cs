namespace LittleHero
{
    /// <summary>5028 — Starforce 강화 시도 결과 DTO.</summary>
    public enum StarforceOutcome
    {
        Success,
        Failure,
        AtMaxLevel,
        InsufficientShards,
        InvalidSaveData,
    }

    public class StarforceResult
    {
        public StarforceOutcome outcome;
        public EquipSlot slot;
        public int previousLevel;
        public int newLevel;
        public int shardCost;
        public float rolledChance;
    }
}
