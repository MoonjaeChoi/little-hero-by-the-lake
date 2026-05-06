namespace LittleHero
{
    /// <summary>
    /// 5028 — ⭐N → ⭐N+1 강화 비용 (마법의돌 조각).
    /// 데이터포인트: ★0→1=100 / ★5→6=500 / ★10→11=2000 / ★15→16=8000 / ★19→20=24000.
    /// </summary>
    public static class StarforceCostTable
    {
        // 인덱스 N = ⭐N → ⭐N+1 비용. 길이 20 (⭐19→⭐20 까지).
        private static readonly int[] _shardCost =
        {
            100, 150, 220, 320, 400, 500, 700, 1000, 1300, 1600,
            2000, 2700, 3700, 5000, 6500, 8000, 10500, 14000, 18500, 24000,
        };

        public static int GetCostForLevel(int currentLevel)
        {
            if (currentLevel < 0) return 0;
            if (currentLevel >= _shardCost.Length) return 0;  // ★20 cap — 비용 X
            return _shardCost[currentLevel];
        }
    }
}
