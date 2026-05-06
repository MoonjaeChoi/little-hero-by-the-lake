namespace LittleHero
{
    /// <summary>
    /// 5028 — ★ 단계별 base 곱셈 % (gacha_v1.md §3 결정 2026-05-05).
    /// 데이터포인트: ★0=0% / ★5=30% / ★10=60% / ★15=100% / ★20=200% (cap).
    /// </summary>
    public static class StarforceMultiplierTable
    {
        // 인덱스 N = ⭐N 도달 시 누적 base 곱셈 % (0~MAX_LEVEL)
        private static readonly float[] _baseMulPercent =
        {
            0f, 5f, 10f, 15f, 22f, 30f, 36f, 42f, 48f, 54f, 60f,
            68f, 76f, 84f, 92f, 100f, 120f, 140f, 160f, 180f, 200f,
        };

        public static float GetBaseMultiplierForLevel(int level)
        {
            if (level < 0) return 0f;
            int cap = _baseMulPercent.Length - 1;
            if (level >= cap) return _baseMulPercent[cap] / 100f;
            return _baseMulPercent[level] / 100f;
        }
    }
}
