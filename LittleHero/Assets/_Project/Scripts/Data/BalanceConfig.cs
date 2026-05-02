using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/BalanceConfig")]
    public class BalanceConfig : ScriptableObject
    {
        [Header("플레이어 기본 스탯")]
        public float baseMaxHealth = 100f;
        public float baseMoveSpeed = 5f;
        public float baseAttackDamage = 10f;
        public float baseAttackSpeed = 1f;
        public float baseRange = 5f;

        [Header("레벨업")]
        public int[] xpThresholds;

        [Header("상점")]
        public int shopItemCount = 6;
        public int shopRefreshBaseCost = 2;
        public float shopRefreshCostMultiplier = 1.5f;

        [Tooltip("희귀도별 가격: Common, Magic, Rare, Epic, Legendary, Mythic")]
        public int[] priceByRarity = { 3, 6, 12, 25, 50 };

        [Header("웨이브 스케일링")]
        public float waveScalingFactor = 0.1f;
        public int miniBossWaveInterval = 5;

        // ===== B9 — economy_v1 §9.1 =====
        [Header("강화 비용 (B9, economy §4.1 / §7.3)")]
        [Tooltip("길이 25. enhanceCostByLevel[i] = +i → +(i+1) 단계의 별가루 ★ 비용")]
        public int[] enhanceCostByLevel;
        [Tooltip("길이 25. +15→+16 부터 양수 (마법 결정 ◇)")]
        public int[] enhanceCrystalByLevel;

        [Header("리롤 (B9, affix_pool §4.2 / item_drop §6)")]
        public int rerollCostMain = 30;
        public int rerollCostSub = 15;
        [Tooltip("잠금당 비용 배수. 0.5 = +50%/잠금")]
        public float lockMultiplier = 0.5f;

        [Header("무지개 구슬 ✦ 상점 (B9, item_drop §4.1)")]
        public int accessoryShopCommon = 5;
        public int accessoryShopMagic = 12;
        public int accessoryShopRare = 30;
        public int accessoryShopEpic = 80;
        public int subAffixUpgradeCost = 10;

        [Header("드랍 곡선 (B9, economy §3 / §7)")]
        [Tooltip("별가루 챕터 곱셈 베이스 — 챕터 N 시 ×base^(N-1)")]
        public float starDustChapterMultiplierBase = 1.08f;
        public float bossCrystalBaseDrop = 1f;
        public float bossCrystalChapterPerStep = 3f;

        // PIVOT v3 — 작은 적 무리 패러다임 (Vampire Survivors / Maplestory Idle)
        // 적 1/10 + 출현 ×10 = 합산 동일 + 시각·게임플레이 idle 톤 정합
        // game_pivot_v2.md PIVOT v3 결정 (2026-05-02 사용자 추천 안 채택)
        [Header("PIVOT v3 글로벌 곱셈 (적 1/10 / 출현 ×10)")]
        public float enemyHpMul        = 0.1f;   // 적 HP × 0.1 (보스 제외)
        public float enemyAtkMul       = 0.1f;   // 적 ATK × 0.1 (보스 제외)
        public float enemyScaleMul     = 0.1f;   // sprite scale × 0.1
        public float enemyStarDustMul  = 0.1f;   // 적 별가루 드롭 × 0.1
        public float enemyXpMul        = 0.1f;   // 적 XP 드롭 × 0.1
        public float spawnIntervalMul  = 0.1f;   // spawn 간격 × 0.1 = 10× 빈도
        public float enemyDropChanceMul = 0.1f;  // Enemy 드랍 확률 × 0.1 (마리수 ×10 으로 합산 동일)
    }
}
