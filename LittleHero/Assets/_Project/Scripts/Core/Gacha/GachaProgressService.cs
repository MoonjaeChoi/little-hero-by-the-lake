using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 가챠 누적 카운트 → 소환 레벨 진행 + 등급 확률.
    /// 5027 — GachaService 분리.
    /// </summary>
    public static class GachaProgressService
    {
        /// <summary>현재 누적 카운트로 도달한 소환 레벨(1~MAX) 계산.</summary>
        public static int CalcSummonLevel(GachaPool pool, int totalCount)
        {
            if (pool == null || pool.summonLevelThresholds == null || pool.summonLevelThresholds.Length == 0)
                return 1;
            int level = 1;
            for (int i = 0; i < pool.summonLevelThresholds.Length; i++)
            {
                if (totalCount >= pool.summonLevelThresholds[i])
                    level = i + 1;
                else break;
            }
            return Mathf.Clamp(level, 1, GachaPool.MAX_SUMMON_LEVEL);
        }

        public static SummonLevelProbability GetProbability(GachaPool pool, int summonLevel)
        {
            if (pool == null || pool.probabilities == null || pool.probabilities.Length == 0)
                return null;
            int idx = Mathf.Clamp(summonLevel, 1, pool.probabilities.Length) - 1;
            return pool.probabilities[idx];
        }

        public static int GetNextLevelThreshold(GachaPool pool, int currentLevel)
        {
            if (pool == null || pool.summonLevelThresholds == null) return -1;
            if (currentLevel >= pool.summonLevelThresholds.Length) return -1;
            return pool.summonLevelThresholds[currentLevel];
        }
    }
}
