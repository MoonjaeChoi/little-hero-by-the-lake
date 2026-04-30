using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// E4 — 시즌 패스 (monetization_v1 §3.3). 출시 후 3개월차 도입 검토.
    ///
    /// 무료 선 + 프리미엄 선(3,300원/시즌). 진행도(seasonPoints)는 endgame·챕터 활동으로 적립.
    /// 매 시즌(3개월) 시작 시 시즌 ID 갱신·진행도 0 리셋. 무료/프리미엄 보상 비트마스크로 1회 수령.
    ///
    /// 보상 정의:
    ///  - 100 pt: 별빛 ⋆ 30 (free) / ⋆ 80 (premium)
    ///  - 250 pt: 무지개 구슬 ✦ 10 (free) / ✦ 30 (premium)
    ///  - 500 pt: 마법 결정 ◇ 5 (free) / ◇ 25 (premium)
    ///  - 1000 pt: 별빛 ⋆ 70 (free) / ⋆ 320 (premium)
    /// 합계: free 100⋆ + 50⋆ = 100⋆ / 30 ✦ / 5 ◇   /   premium 500⋆ / 200 ✦ / 150 ◇ + 코스메틱
    ///
    /// MVP 외 (Phase 2 후속): 코스메틱·칭호 보상.
    /// </summary>
    public static class SeasonPassService
    {
        // 단계별 점수 임계값 + 보상.
        private static readonly int[] PointThresholds = { 100, 250, 500, 1000 };
        private static readonly RewardSpec[] FreeRewards =
        {
            new RewardSpec { starlight = 30 },
            new RewardSpec { starFragment = 10 },
            new RewardSpec { magicCrystal = 5 },
            new RewardSpec { starlight = 70 },
        };
        private static readonly RewardSpec[] PremiumRewards =
        {
            new RewardSpec { starlight = 80 },
            new RewardSpec { starFragment = 30 },
            new RewardSpec { magicCrystal = 25 },
            new RewardSpec { starlight = 320 },
        };

        public struct RewardSpec
        {
            public int starlight;       // ⋆
            public int starFragment;    // ✦
            public int magicCrystal;    // ◇
        }

        public static int CurrentSeasonId
            => MetaProgression.Instance?.Data?.seasonId ?? 0;

        public static int Points
            => MetaProgression.Instance?.Data?.seasonPoints ?? 0;

        public static bool IsPremium
            => MetaProgression.Instance?.Data?.seasonPassPremium ?? false;

        public static int TierCount => PointThresholds.Length;
        public static int GetThreshold(int tierIndex) => PointThresholds[Mathf.Clamp(tierIndex, 0, TierCount - 1)];
        public static RewardSpec GetFreeReward(int tierIndex) => FreeRewards[Mathf.Clamp(tierIndex, 0, TierCount - 1)];
        public static RewardSpec GetPremiumReward(int tierIndex) => PremiumRewards[Mathf.Clamp(tierIndex, 0, TierCount - 1)];

        public static void AddPoints(int amount)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || amount <= 0) return;
            data.seasonPoints += amount;
            SaveSystem.Save(data);
        }

        public static bool IsTierReached(int tierIndex)
            => Points >= GetThreshold(tierIndex);

        public static bool TryClaimFree(int tierIndex)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return false;
            if (!IsTierReached(tierIndex)) return false;
            int bit = 1 << tierIndex;
            if ((data.seasonRewardClaimedFreeMask & bit) != 0) return false;

            ApplyReward(GetFreeReward(tierIndex));
            data.seasonRewardClaimedFreeMask |= bit;
            SaveSystem.Save(data);
            return true;
        }

        public static bool TryClaimPremium(int tierIndex)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return false;
            if (!IsPremium) return false;
            if (!IsTierReached(tierIndex)) return false;
            int bit = 1 << tierIndex;
            if ((data.seasonRewardClaimedPremiumMask & bit) != 0) return false;

            ApplyReward(GetPremiumReward(tierIndex));
            data.seasonRewardClaimedPremiumMask |= bit;
            SaveSystem.Save(data);
            return true;
        }

        public static void GrantPremium()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            if (data.seasonPassPremium) return;
            data.seasonPassPremium = true;
            SaveSystem.Save(data);
            Debug.Log($"[SeasonPass] 시즌 {data.seasonId} 프리미엄 활성화");
        }

        /// <summary>새 시즌 시작 — 시즌 ID 증가 + 진행도/보상 비트 리셋. 시즌 패스 프리미엄은 시즌별로 별도 구매.</summary>
        public static void RotateSeason(int newSeasonId)
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            if (data.seasonId == newSeasonId) return;
            data.seasonId = newSeasonId;
            data.seasonPoints = 0;
            data.seasonPassPremium = false;
            data.seasonRewardClaimedFreeMask = 0;
            data.seasonRewardClaimedPremiumMask = 0;
            SaveSystem.Save(data);
        }

        private static void ApplyReward(RewardSpec r)
        {
            if (r.starlight > 0)    CurrencyService.GrantStarlight(r.starlight);
            if (r.starFragment > 0) CurrencyService.GrantStarFragment(r.starFragment);
            if (r.magicCrystal > 0) CurrencyService.GrantMagicCrystal(r.magicCrystal);
        }
    }
}
