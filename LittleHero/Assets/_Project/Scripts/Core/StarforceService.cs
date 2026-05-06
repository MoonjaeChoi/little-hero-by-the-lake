using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 facade — 슬롯 강화 (마법의돌, gacha_v1.md §3).
    /// 위임: MultiplierTable / CostTable / Probability + Result DTO.
    /// TryEnhance 핵심 응집 (D2=A).
    /// </summary>
    public static class StarforceService
    {
        public const int MAX_LEVEL = 20;
        public const int CURVE_LAST = 20;

        private static readonly System.Random _defaultRng = new System.Random();

        // ===== 조회 (Multiplier/Cost/Probability table 위임) =====

        public static int GetLevel(EquipSlot slot)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.slotStarforceLevels == null) return 0;
            int idx = (int)slot;
            if (idx < 0 || idx >= data.slotStarforceLevels.Length) return 0;
            return Mathf.Max(0, data.slotStarforceLevels[idx]);
        }

        public static float GetBaseMultiplier(EquipSlot slot) => StarforceMultiplierTable.GetBaseMultiplierForLevel(GetLevel(slot));
        public static float GetBaseMultiplierForLevel(int level) => StarforceMultiplierTable.GetBaseMultiplierForLevel(level);
        public static int GetCost(EquipSlot slot) => StarforceCostTable.GetCostForLevel(GetLevel(slot));
        public static int GetCostForLevel(int currentLevel) => StarforceCostTable.GetCostForLevel(currentLevel);
        public static float GetSuccessRate(EquipSlot slot) => StarforceProbability.GetSuccessRateForLevel(GetLevel(slot));
        public static float GetSuccessRateForLevel(int currentLevel) => StarforceProbability.GetSuccessRateForLevel(currentLevel);
        public static bool IsMaxLevel(EquipSlot slot) => GetLevel(slot) >= MAX_LEVEL;

        // ===== 강화 (응집 D2=A) =====

        /// <summary>
        /// 슬롯 1단계 강화 시도. 비용은 시도 시점에 차감 (실패 환불 X).
        /// 안전 강화 — 실패해도 단계 하강·장비 파괴 없음.
        /// </summary>
        public static StarforceResult TryEnhance(EquipSlot slot, System.Random rng = null)
        {
            rng = rng ?? _defaultRng;
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.slotStarforceLevels == null || data.slotStarforceLevels.Length < 12)
                return new StarforceResult { outcome = StarforceOutcome.InvalidSaveData, slot = slot };

            int idx = (int)slot;
            int currentLevel = Mathf.Max(0, data.slotStarforceLevels[idx]);

            if (currentLevel >= MAX_LEVEL)
                return new StarforceResult { outcome = StarforceOutcome.AtMaxLevel, slot = slot, previousLevel = currentLevel, newLevel = currentLevel };

            int cost = StarforceCostTable.GetCostForLevel(currentLevel);
            if (data.starforceShards < cost)
                return new StarforceResult { outcome = StarforceOutcome.InsufficientShards, slot = slot, previousLevel = currentLevel, newLevel = currentLevel, shardCost = cost };

            data.starforceShards -= cost;
            GameEvents.RaiseStarforceShardsChanged(data.starforceShards);

            float rate = StarforceProbability.GetSuccessRateForLevel(currentLevel);
            float roll = (float)rng.NextDouble();
            bool success = roll < rate;

            int newLevel = success ? currentLevel + 1 : currentLevel;
            data.slotStarforceLevels[idx] = newLevel;
            if (success) GameEvents.RaiseSlotStarforceChanged(idx, newLevel);
            SaveSystem.Save(data);

            return new StarforceResult
            {
                outcome = success ? StarforceOutcome.Success : StarforceOutcome.Failure,
                slot = slot,
                previousLevel = currentLevel,
                newLevel = newLevel,
                shardCost = cost,
                rolledChance = rate,
            };
        }

        // ===== 조각 재화 =====

        public static int Shards => MetaProgression.Instance?.Data?.starforceShards ?? 0;

        public static void GrantShards(int amount)
        {
            if (amount <= 0) return;
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            data.starforceShards += amount;
            GameEvents.RaiseStarforceShardsChanged(data.starforceShards);
            SaveSystem.Save(data);
        }
    }
}
