using UnityEngine;

namespace LittleHero
{
    public enum EnhanceOutcome
    {
        Success,
        Failure,
        AtMaxLevel,
        InsufficientDust,
        InsufficientCrystal,
        InvalidInstance,
    }

    /// <summary>
    /// 강화 시도 결과. UI 연출(별 흩어짐 / 천장 게이지 / 카운트업)에 사용.
    /// </summary>
    public struct EnhanceResult
    {
        public EnhanceOutcome outcome;
        public int newLevel;          // 시도 후 enhanceLevel
        public int dustCost;          // 차감된 별가루 (0 = 미차감)
        public int crystalCost;       // 차감된 마법 결정
        public bool wasPityTriggered; // 하드 천장으로 100% 보장
        public float rolledChance;    // 굴린 성공률(0~1) — 결정론 단계는 1f
        public bool IsSuccess => outcome == EnhanceOutcome.Success;
    }

    /// <summary>
    /// C3 — 강화 +0~+25 결정론·천장 시스템 (equipment_v1 §3.3 / §3.4 + economy §4.1·§7.3).
    ///
    /// +0~+15 결정론 (100%).
    /// +16~+25 RNG: p_base(level) = 0.50 × 0.85^(level - 16)
    ///   소프트 천장: failCount ≥ 5 부터 +5%/회 ((failCount - 4) × 0.05)
    ///   하드 천장: failCount ≥ 20 → 100% 보장 ("아빠가 도와주는 중")
    /// 비용: BalanceConfig.enhanceCostByLevel[level] ★ + enhanceCrystalByLevel[level] ◇.
    ///   비용은 시도 시점에 차감, 실패해도 환불 없음.
    /// </summary>
    public static class EnhancementService
    {
        public const int MAX_ENHANCE_LEVEL = 25;
        public const int DETERMINISTIC_LEVEL_CAP = 16;   // [0, 16) = 결정론
        public const int SOFT_PITY_START_FAILS = 5;
        public const float SOFT_PITY_BONUS_PER_FAIL = 0.05f;
        public const int HARD_PITY_FAILS = 20;
        public const float P_BASE = 0.50f;
        public const float P_DECAY = 0.85f;

        private static readonly System.Random _defaultRng = new System.Random();

        /// <summary>
        /// 단일 강화 시도. EquipmentInstance·BalanceConfig·(선택) RNG 주입.
        /// </summary>
        public static EnhanceResult TryEnhance(EquipmentInstance instance, BalanceConfig config, System.Random rng = null)
        {
            if (instance == null || config == null)
                return new EnhanceResult { outcome = EnhanceOutcome.InvalidInstance };

            int level = instance.enhanceLevel;
            if (level >= MAX_ENHANCE_LEVEL)
                return new EnhanceResult { outcome = EnhanceOutcome.AtMaxLevel, newLevel = level };

            int dustCost = GetDustCost(level, config);
            int crystalCost = GetCrystalCost(level, config);

            // Pre-flight check (refund 회피)
            var gm = GameManager.Instance;
            if (gm == null || gm.StarDust < dustCost)
                return new EnhanceResult { outcome = EnhanceOutcome.InsufficientDust, newLevel = level, dustCost = dustCost, crystalCost = crystalCost };
            if (crystalCost > 0 && CurrencyService.MagicCrystals < crystalCost)
                return new EnhanceResult { outcome = EnhanceOutcome.InsufficientCrystal, newLevel = level, dustCost = dustCost, crystalCost = crystalCost };

            // 차감
            gm.SpendStarDust(dustCost);
            if (crystalCost > 0) CurrencyService.SpendMagicCrystal(crystalCost);

            // 성공 판정
            rng ??= _defaultRng;
            float p = CalcSuccessRate(level, instance.failCount);
            bool pityTriggered = (level >= DETERMINISTIC_LEVEL_CAP && instance.failCount >= HARD_PITY_FAILS);
            bool success = (level < DETERMINISTIC_LEVEL_CAP) || pityTriggered || rng.NextDouble() < p;

            instance.history ??= new InstanceHistory();
            instance.history.totalEnhanceAttempts++;
            if (pityTriggered) instance.history.pityTriggeredCount++;

            if (success)
            {
                instance.enhanceLevel++;
                instance.failCount = 0;
                return new EnhanceResult
                {
                    outcome = EnhanceOutcome.Success,
                    newLevel = instance.enhanceLevel,
                    dustCost = dustCost,
                    crystalCost = crystalCost,
                    wasPityTriggered = pityTriggered,
                    rolledChance = p,
                };
            }
            else
            {
                instance.failCount++;
                return new EnhanceResult
                {
                    outcome = EnhanceOutcome.Failure,
                    newLevel = instance.enhanceLevel,
                    dustCost = dustCost,
                    crystalCost = crystalCost,
                    wasPityTriggered = false,
                    rolledChance = p,
                };
            }
        }

        /// <summary>
        /// 다음 시도 성공률(0~1). 결정론 단계는 1f, RNG 단계는 base × decay + soft pity, 하드 천장은 1f.
        /// UI(천장 게이지·예상 확률 표기)에서 호출.
        /// </summary>
        public static float CalcSuccessRate(int level, int failCount)
        {
            if (level < DETERMINISTIC_LEVEL_CAP) return 1f;
            if (failCount >= HARD_PITY_FAILS) return 1f;
            float p = P_BASE * Mathf.Pow(P_DECAY, level - DETERMINISTIC_LEVEL_CAP);
            if (failCount >= SOFT_PITY_START_FAILS)
                p += SOFT_PITY_BONUS_PER_FAIL * (failCount - (SOFT_PITY_START_FAILS - 1));
            return Mathf.Clamp01(p);
        }

        public static int GetDustCost(int level, BalanceConfig config)
        {
            if (config?.enhanceCostByLevel == null) return 0;
            if (level < 0 || level >= config.enhanceCostByLevel.Length) return 0;
            return config.enhanceCostByLevel[level];
        }

        public static int GetCrystalCost(int level, BalanceConfig config)
        {
            if (config?.enhanceCrystalByLevel == null) return 0;
            if (level < 0 || level >= config.enhanceCrystalByLevel.Length) return 0;
            return config.enhanceCrystalByLevel[level];
        }
    }
}
