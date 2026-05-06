namespace LittleHero
{
    /// <summary>
    /// 5028 — ⭐N → ⭐N+1 성공 확률 (0~1). 메이플 본판 ★0~14 + 사용자 정의 ★15~20.
    /// 누적 평균 시도 ~73회 (PC 본판 ~283회 대비 -74%).
    /// </summary>
    public static class StarforceProbability
    {
        private static readonly float[] _successRate =
        {
            0.95f, 0.90f, 0.85f, 0.85f, 0.80f, 0.75f, 0.70f, 0.65f, 0.60f, 0.55f,
            0.50f, 0.45f, 0.40f, 0.35f, 0.30f, 0.25f, 0.20f, 0.15f, 0.10f, 0.05f,
        };

        public static float GetSuccessRateForLevel(int currentLevel)
        {
            if (currentLevel < 0) return 0f;
            if (currentLevel >= _successRate.Length) return 0f;  // ★20 cap
            return _successRate[currentLevel];
        }
    }
}
