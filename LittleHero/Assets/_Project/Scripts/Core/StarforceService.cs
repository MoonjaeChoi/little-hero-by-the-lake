using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT Phase C5 — 슬롯 강화 (스타포스) 정적 서비스 (gacha_v1.md §3).
    ///
    /// 메이플 키우기 패턴: **장비**가 아닌 **슬롯** 강화. 장비 교체해도 ⭐ 단계 그대로 유지.
    /// 12 슬롯 독립 (모자 ⭐15 / 무기 ⭐20 등 가능).
    /// 안전 강화 — 하강·파괴 X. 실패 시 비용만 차감, 단계 그대로 (재도전).
    ///
    /// 효과: 장비 baseStats × (1 + GetBaseMultiplier(slot)).
    /// 강화 단계 0~25 → 베이스 +0% ~ +200%.
    /// </summary>
    public static class StarforceService
    {
        // stats_v2.5 — 무한 강화 (사용자 결정 2026-05-03). +25+ 는 곡선 외삽.
        public const int MAX_LEVEL = int.MaxValue;
        public const int CURVE_LAST = 25;          // 정의된 곡선 마지막 단계
        public const float MUL_GROWTH_OVER = 0.05f;  // ⭐25+ 단계마다 +5% baseMul (200% → 205% → ...)
        public const float COST_GROWTH_OVER = 1.4f;  // ⭐25+ 비용 1.4^N 증가
        public const float RATE_FLOOR = 0.30f;       // ⭐25+ 성공률 30% 동결

        // gacha_v1.md §3.2 데이터포인트 + 보간으로 채운 25 길이 곡선.
        // 인덱스 N = ⭐N → ⭐N+1 강화 (성공 시 ⭐N+1 도달).

        // base 곱셈 % (단계 N 도달 시 적용 — 누적 효과)
        // 데이터포인트: ⭐0=0% / ⭐5=30% / ⭐10=60% / ⭐15=100% / ⭐20=160% / ⭐25=200%
        private static readonly float[] _baseMulPercent = {
            /* ⭐0  */ 0f,
            /* ⭐1  */ 5f,
            /* ⭐2  */ 10f,
            /* ⭐3  */ 15f,
            /* ⭐4  */ 22f,
            /* ⭐5  */ 30f,
            /* ⭐6  */ 36f,
            /* ⭐7  */ 42f,
            /* ⭐8  */ 48f,
            /* ⭐9  */ 54f,
            /* ⭐10 */ 60f,
            /* ⭐11 */ 68f,
            /* ⭐12 */ 76f,
            /* ⭐13 */ 84f,
            /* ⭐14 */ 92f,
            /* ⭐15 */ 100f,
            /* ⭐16 */ 112f,
            /* ⭐17 */ 124f,
            /* ⭐18 */ 136f,
            /* ⭐19 */ 148f,
            /* ⭐20 */ 160f,
            /* ⭐21 */ 168f,
            /* ⭐22 */ 176f,
            /* ⭐23 */ 184f,
            /* ⭐24 */ 192f,
            /* ⭐25 */ 200f,
        };

        // 단계별 비용 (스타포스 조각). 인덱스 N = ⭐N → ⭐N+1 비용. 길이 25 (⭐24→⭐25 까지).
        // 데이터포인트: ⭐0→1=100 / ⭐5→6=500 / ⭐10→11=2000 / ⭐15→16=8000 / ⭐20→21=30000 / ⭐24→25=100000
        private static readonly int[] _shardCost = {
            /* ⭐0→1   */    100,
            /* ⭐1→2   */    150,
            /* ⭐2→3   */    220,
            /* ⭐3→4   */    320,
            /* ⭐4→5   */    400,
            /* ⭐5→6   */    500,
            /* ⭐6→7   */    700,
            /* ⭐7→8   */   1000,
            /* ⭐8→9   */   1300,
            /* ⭐9→10  */   1600,
            /* ⭐10→11 */   2000,
            /* ⭐11→12 */   2700,
            /* ⭐12→13 */   3700,
            /* ⭐13→14 */   5000,
            /* ⭐14→15 */   6500,
            /* ⭐15→16 */   8000,
            /* ⭐16→17 */  10500,
            /* ⭐17→18 */  14000,
            /* ⭐18→19 */  18500,
            /* ⭐19→20 */  24000,
            /* ⭐20→21 */  30000,
            /* ⭐21→22 */  42000,
            /* ⭐22→23 */  56000,
            /* ⭐23→24 */  75000,
            /* ⭐24→25 */ 100000,
        };

        // 단계별 성공 확률 (0~1). 인덱스 N = ⭐N → ⭐N+1.
        // ⭐0~10 = 100% / ⭐11~15 = 95% / ⭐16~20 = 75% / ⭐21~24 = 50% / ⭐25 = 30%
        private static readonly float[] _successRate = {
            /* ⭐0→1   */ 1.00f, 1.00f, 1.00f, 1.00f, 1.00f,
            /* ⭐5→6   */ 1.00f, 1.00f, 1.00f, 1.00f, 1.00f,
            /* ⭐10→11 */ 0.95f, 0.95f, 0.95f, 0.95f, 0.95f,
            /* ⭐15→16 */ 0.75f, 0.75f, 0.75f, 0.75f, 0.75f,
            /* ⭐20→21 */ 0.50f, 0.50f, 0.50f, 0.50f,
            /* ⭐24→25 */ 0.30f,
        };

        private static readonly System.Random _defaultRng = new System.Random();

        // ===== 조회 =====

        /// <summary>슬롯의 현재 ⭐ 단계 (0~무한). 미초기화 SaveData 시 0.</summary>
        public static int GetLevel(EquipSlot slot)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.slotStarforceLevels == null) return 0;
            int idx = (int)slot;
            if (idx < 0 || idx >= data.slotStarforceLevels.Length) return 0;
            return Mathf.Max(0, data.slotStarforceLevels[idx]);
        }

        /// <summary>슬롯에 적용되는 base 곱셈 비율 (⭐25 +200%, ⭐25+ 단계마다 +5% 추가 무한).</summary>
        public static float GetBaseMultiplier(EquipSlot slot) => GetBaseMultiplierForLevel(GetLevel(slot));

        public static float GetBaseMultiplierForLevel(int level)
        {
            if (level < 0) return 0f;
            if (level <= CURVE_LAST) return _baseMulPercent[level] / 100f;
            // stats_v2.5 — 무한 ⭐25+ 매 단계 +5%
            return (_baseMulPercent[CURVE_LAST] + (level - CURVE_LAST) * MUL_GROWTH_OVER * 100f) / 100f;
        }

        /// <summary>현재 단계에서 다음 강화 시도 비용.</summary>
        public static int GetCost(EquipSlot slot) => GetCostForLevel(GetLevel(slot));

        public static int GetCostForLevel(int currentLevel)
        {
            if (currentLevel < 0) return 0;
            if (currentLevel < _shardCost.Length) return _shardCost[currentLevel];
            // stats_v2.5 — 무한 (last × 1.4^over)
            int over = currentLevel - _shardCost.Length + 1;
            return Mathf.RoundToInt(_shardCost[_shardCost.Length - 1] * Mathf.Pow(COST_GROWTH_OVER, over));
        }

        /// <summary>현재 단계에서 다음 강화 시도 성공 확률 (0~1).</summary>
        public static float GetSuccessRate(EquipSlot slot) => GetSuccessRateForLevel(GetLevel(slot));

        public static float GetSuccessRateForLevel(int currentLevel)
        {
            if (currentLevel < 0) return 0f;
            if (currentLevel < _successRate.Length) return _successRate[currentLevel];
            return RATE_FLOOR;  // ⭐25+ 무한 동결
        }

        public static bool IsMaxLevel(EquipSlot slot) => false;  // 무한 (cap 없음)

        // ===== 강화 =====

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

        /// <summary>
        /// 슬롯 1단계 강화 시도. 비용은 시도 시점에 차감 (실패해도 환불 X).
        /// 안전 강화 — 실패해도 단계 하강·장비 파괴 없음.
        /// </summary>
        public static StarforceResult TryEnhance(EquipSlot slot, System.Random rng = null)
        {
            rng = rng ?? _defaultRng;
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.slotStarforceLevels == null || data.slotStarforceLevels.Length < 12)
            {
                return new StarforceResult { outcome = StarforceOutcome.InvalidSaveData, slot = slot };
            }

            int idx = (int)slot;
            int currentLevel = Mathf.Max(0, data.slotStarforceLevels[idx]);
            // stats_v2.5 — 무한 강화 (cap X), AtMaxLevel 폐기
            int cost = GetCostForLevel(currentLevel);
            if (data.starforceShards < cost)
            {
                return new StarforceResult
                {
                    outcome = StarforceOutcome.InsufficientShards,
                    slot = slot,
                    previousLevel = currentLevel,
                    newLevel = currentLevel,
                    shardCost = cost,
                };
            }

            // 비용 차감
            data.starforceShards -= cost;
            GameEvents.RaiseStarforceShardsChanged(data.starforceShards);

            // 확률 굴림
            float rate = GetSuccessRateForLevel(currentLevel);
            float roll = (float)rng.NextDouble();
            bool success = roll < rate;

            int newLevel = success ? currentLevel + 1 : currentLevel;
            data.slotStarforceLevels[idx] = newLevel;

            if (success)
                GameEvents.RaiseSlotStarforceChanged(idx, newLevel);

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
