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

        [Header("5003 — 미니보스 씬 분리 옵션")]
        [Tooltip("None=분리 X (5002 종료 상태) / LastOnly=마지막 미니보스 wave (15)만 분리 / All=5/10/15 모두 분리")]
        public MiniBossSeparationMode miniBossSeparation = MiniBossSeparationMode.None;

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

        // PIVOT v4 (2026-05-03 사용자 결정) — Cinematic Combat (cinematic_combat_v1.md)
        // cap 1,500 → 300. 마리당 ★/XP/드랍 ×5 인상 (합산 보존). Premium 피격 + 데미지 수치.
        [Header("PIVOT v4 — Cinematic Combat (cap 300 + Premium 피격)")]
        public float enemyHpMul        = 0.3f;   // 적 HP × 0.3 (식별성, 그대로)
        public float enemyAtkMul       = 0.3f;   // 적 ATK × 0.3 (그대로)
        public float enemyScaleMul     = 0.5f;   // sprite scale × 0.5 (cap 적어 큰 적 OK)
        public float enemyStarDustMul  = 0.5f;   // 마리당 ×5 인상 (1500/300 = 5배)
        public float enemyXpMul        = 0.5f;   // 마리당 ×5
        public float spawnIntervalMul  = 0.15f;  // spawn 간격 × 0.15 (cap 비례)
        public float enemyDropChanceMul = 0.5f;  // 마리당 ×5 (드랍 합산 보존)
    }
}
