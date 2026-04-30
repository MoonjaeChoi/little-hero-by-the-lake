using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// C5 — 변환 모루 (economy_v1 §11.4).
    /// 잉여 화폐 → 마법 결정 ◇ 환전. 다락방 메뉴에서만 호출.
    /// 환율: 별가루 1,000 ★ : 1 ◇ / 무지개 구슬 5 ✦ : 1 ◇.
    /// 역방향(◇ → ★ / ◇ → ✦) 미지원 — 마법 결정은 영구 가치 자원.
    /// </summary>
    public static class ConversionForge
    {
        public const int DUST_PER_CRYSTAL = 1000;
        public const int ORB_PER_CRYSTAL = 5;

        /// <summary>
        /// 별가루(런 내) → 마법 결정. 부족·런 외부면 false.
        /// </summary>
        public static bool ConvertDustToCrystal(int crystalCount)
        {
            if (crystalCount <= 0) return false;
            int cost = crystalCount * DUST_PER_CRYSTAL;
            var gm = GameManager.Instance;
            if (gm == null || gm.StarDust < cost) return false;
            if (!gm.SpendStarDust(cost)) return false;
            CurrencyService.GrantMagicCrystal(crystalCount);
            return true;
        }

        /// <summary>
        /// 무지개 구슬(영구) → 마법 결정. 부족 시 false.
        /// </summary>
        public static bool ConvertOrbToCrystal(int crystalCount)
        {
            if (crystalCount <= 0) return false;
            int cost = crystalCount * ORB_PER_CRYSTAL;
            if (CurrencyService.StarFragments < cost) return false;
            if (!CurrencyService.SpendStarFragment(cost)) return false;
            CurrencyService.GrantMagicCrystal(crystalCount);
            return true;
        }

        /// <summary>
        /// 시뮬용 — 미리 변환 시 차감 비용 계산.
        /// </summary>
        public static int CalcDustCost(int crystalCount) => Mathf.Max(0, crystalCount) * DUST_PER_CRYSTAL;
        public static int CalcOrbCost(int crystalCount) => Mathf.Max(0, crystalCount) * ORB_PER_CRYSTAL;
    }
}
