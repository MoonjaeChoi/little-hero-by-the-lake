using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// A5 — 4 화폐 통합 진입점 (정적 facade).
    /// ★ 별가루(런 내, GameManager) / ✦ 무지개 구슬(영구, MetaProgression) /
    /// ◇ 마법 결정(영구, MetaProgression) / ⋆ 별빛(영구·캐시, MetaProgression).
    /// 모든 grant/spend 는 본 클래스를 거치며, 보상 계산식도 여기 모인다.
    /// (참조: economy_v1 §9.3, monetization_v1 §7.3)
    /// </summary>
    public static class CurrencyService
    {
        // === Grant ===

        public static void GrantStarDust(int amount)
        {
            if (amount <= 0) return;
            GameManager.Instance?.AddStarDust(amount);
        }

        public static void GrantStarFragment(int amount)
        {
            if (amount <= 0) return;
            MetaProgression.Instance?.AddStarFragments(amount);
        }

        public static void GrantMagicCrystal(int amount)
        {
            if (amount <= 0) return;
            MetaProgression.Instance?.AddMagicCrystals(amount);
        }

        public static void GrantStarlight(int amount)
        {
            if (amount <= 0) return;
            MetaProgression.Instance?.AddStarlight(amount);
        }

        // === Spend ===

        public static bool SpendStarFragment(int amount)
            => MetaProgression.Instance != null && MetaProgressionCurrency.SpendStarFragments(amount);

        public static bool SpendMagicCrystal(int amount)
            => MetaProgression.Instance != null && MetaProgressionCurrency.SpendMagicCrystals(amount);

        public static bool SpendStarlight(int amount)
            => MetaProgression.Instance != null && MetaProgressionCurrency.SpendStarlight(amount);

        // === Query ===

        public static int StarFragments => MetaProgression.Instance?.StarFragments ?? 0;
        public static int MagicCrystals => MetaProgression.Instance?.MagicCrystals ?? 0;
        public static int Starlight     => MetaProgression.Instance?.Starlight     ?? 0;

        // === 보상 계산 (economy §9.3 + monetization §7.3) ===

        // 챕터 N 클리어 보상: 무지개 구슬
        // 첫 클리어 = 5 × 1.10^(N-1) (반올림), 재클리어 = ×0.25
        public static int CalcChapterStarFragmentReward(int chapterN, bool isFirst)
        {
            int baseReward = Mathf.RoundToInt(5f * Mathf.Pow(1.10f, chapterN - 1));
            return isFirst ? baseReward : Mathf.RoundToInt(baseReward * 0.25f);
        }

        // 챕터 N 클리어 보상: 마법 결정
        // 첫 클리어 = 1 + N/3 (반올림), 재클리어 = ×0.4
        public static int CalcChapterMagicCrystalReward(int chapterN, bool isFirst)
        {
            int baseReward = Mathf.RoundToInt(1f + chapterN / 3f);
            return isFirst ? baseReward : Mathf.RoundToInt(baseReward * 0.4f);
        }

        // 챕터 N 첫 클리어 보상: 별빛 (1회만)
        public static int CalcChapterStarlightReward(int chapterN) => chapterN * 2;

        // 별빛 무료 출처 (monetization §3.1)
        public static int CalcDailyLoginStarlight() => 5;
        public static int CalcMemoryStarlight()     => 10;
    }
}
