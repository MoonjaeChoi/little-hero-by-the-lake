using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT Phase C5 — 슬롯 강화 (마법의돌) 정적 서비스 (gacha_v1.md §3).
    ///
    /// 메이플 키우기 패턴: **장비**가 아닌 **슬롯** 강화. 장비 교체해도 ⭐ 단계 그대로 유지.
    /// 12 슬롯 독립 (모자 ⭐15 / 무기 ⭐20 등 가능).
    /// 안전 강화 — 하강·파괴 X. 실패 시 비용만 차감, 단계 그대로 (재도전).
    ///
    /// 효과: 장비 baseStats × (1 + GetBaseMultiplier(slot)).
    /// 강화 단계 0~20 → 베이스 +0% ~ +200%.
    ///
    /// 곡선 (사용자 결정 2026-05-05) — 메이플 본판 답습 + 파괴 폐기 + ★20 cap:
    ///   ★0→1 95% / ... / ★14→15 30% / ★15→16 25% / ★16→17 20% / ★17→18 15% / ★18→19 10% / ★19→20 5%.
    ///   ★0→★20 누적 평균 시도 ~73회 (PC 본판 ~283회 대비 -74%).
    ///   ★20 = MAX (cap, IsMaxLevel = true).
    /// </summary>
    public static class StarforceService
    {
        // 사용자 결정 2026-05-05 — ★0~20 cap (stats_v2.5 무한 강화 폐기).
        public const int MAX_LEVEL = 20;
        public const int CURVE_LAST = 20;          // 정의된 곡선 마지막 단계 (= MAX_LEVEL)

        // 인덱스 N = ⭐N → ⭐N+1 강화 (성공 시 ⭐N+1 도달).

        // base 곱셈 % (단계 N 도달 시 적용 — 누적 효과)
        // 데이터포인트: ⭐0=0% / ⭐5=30% / ⭐10=60% / ⭐15=100% / ⭐20=200% (cap)
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
            /* ⭐16 */ 120f,
            /* ⭐17 */ 140f,
            /* ⭐18 */ 160f,
            /* ⭐19 */ 180f,
            /* ⭐20 */ 200f,  // cap
        };

        // 단계별 비용 (마법의돌 조각). 인덱스 N = ⭐N → ⭐N+1 비용. 길이 20 (⭐19→⭐20 까지).
        // 데이터포인트: ⭐0→1=100 / ⭐5→6=500 / ⭐10→11=2000 / ⭐15→16=8000 / ⭐19→20=24000
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
        };

        // 단계별 성공 확률 (0~1). 인덱스 N = ⭐N → ⭐N+1.
        // 사용자 결정 2026-05-05 — 메이플 본판 답습 (★0~14) + 사용자 정의 (★15~20):
        //   ★0~14 본판: 95/90/85/85/80/75/70/65/60/55/50/45/40/35/30
        //   ★14→15 = 30% / ★15→16 = 25% / ★16→17 = 20% / ★17→18 = 15% / ★18→19 = 10% / ★19→20 = 5%
        //   ★20 = cap (다음 시도 X). 누적 평균 시도 ~73회.
        private static readonly float[] _successRate = {
            /* ⭐0→1   */ 0.95f,
            /* ⭐1→2   */ 0.90f,
            /* ⭐2→3   */ 0.85f,
            /* ⭐3→4   */ 0.85f,
            /* ⭐4→5   */ 0.80f,
            /* ⭐5→6   */ 0.75f,
            /* ⭐6→7   */ 0.70f,
            /* ⭐7→8   */ 0.65f,
            /* ⭐8→9   */ 0.60f,
            /* ⭐9→10  */ 0.55f,
            /* ⭐10→11 */ 0.50f,
            /* ⭐11→12 */ 0.45f,
            /* ⭐12→13 */ 0.40f,
            /* ⭐13→14 */ 0.35f,
            /* ⭐14→15 */ 0.30f,
            /* ⭐15→16 */ 0.25f,
            /* ⭐16→17 */ 0.20f,
            /* ⭐17→18 */ 0.15f,
            /* ⭐18→19 */ 0.10f,
            /* ⭐19→20 */ 0.05f,
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
            if (level >= MAX_LEVEL) return _baseMulPercent[MAX_LEVEL] / 100f;
            return _baseMulPercent[level] / 100f;
        }

        /// <summary>현재 단계에서 다음 강화 시도 비용.</summary>
        public static int GetCost(EquipSlot slot) => GetCostForLevel(GetLevel(slot));

        public static int GetCostForLevel(int currentLevel)
        {
            if (currentLevel < 0) return 0;
            if (currentLevel >= MAX_LEVEL) return 0;  // ★20 cap — 비용 X (강화 불가)
            return _shardCost[currentLevel];
        }

        /// <summary>현재 단계에서 다음 강화 시도 성공 확률 (0~1). ★20 cap 시 0f.</summary>
        public static float GetSuccessRate(EquipSlot slot) => GetSuccessRateForLevel(GetLevel(slot));

        public static float GetSuccessRateForLevel(int currentLevel)
        {
            if (currentLevel < 0) return 0f;
            if (currentLevel >= MAX_LEVEL) return 0f;  // ★20 cap — 다음 시도 X
            return _successRate[currentLevel];
        }

        public static bool IsMaxLevel(EquipSlot slot) => GetLevel(slot) >= MAX_LEVEL;

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
            // ★20 cap — 더 이상 강화 X (AtMaxLevel 반환)
            if (currentLevel >= MAX_LEVEL)
            {
                return new StarforceResult
                {
                    outcome = StarforceOutcome.AtMaxLevel,
                    slot = slot,
                    previousLevel = currentLevel,
                    newLevel = currentLevel,
                };
            }
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
