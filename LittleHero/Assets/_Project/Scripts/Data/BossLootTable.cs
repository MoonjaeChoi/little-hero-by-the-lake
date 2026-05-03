using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 보스 처치 보상 테이블 (5001 boss_system_v2 / Sprint 3.1 / T05).
    /// Tier 별 RewardEntry 배열 + 신화 보장 확률.
    /// CLAUDE.md §15.4 SO 우선 — 4 인스턴스 (Mini/Chapter/Daily/Weekly) Inspector 조정 가능.
    /// EnemyData.lootTable wiring (T06) → EnemyController.Die 시 BossLootGrantService.Apply.
    /// </summary>
    [CreateAssetMenu(fileName = "LootTable_", menuName = "LittleHero/BossLootTable")]
    public sealed class BossLootTable : ScriptableObject
    {
        [Tooltip("보스 분류 — 본 LootTable 이 적용될 Tier")]
        public BossTier tier;

        [Tooltip("처치 시 grant 할 보상 배열 (StarDust/RainbowOrb/MagicCrystal/Starlight)")]
        public RewardEntry[] rewards;

        [Range(0f, 1f), Tooltip("신화 발현 보장 확률. WeeklyBoss=1.0, Daily=0.1 등")]
        public float guaranteedMythicChance = 0f;
    }
}
