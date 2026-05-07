using System;
using UnityEngine;

namespace LittleHero
{
    public enum ModeType { ChapterRepeat, HardcoreChapter, DailyBoss, WeeklyBoss, InfiniteTower }

    public enum CurrencyType { StarDust, RainbowOrb, MagicCrystal, Starlight, Xp }

    [Serializable]
    public class RewardEntry
    {
        public CurrencyType currency;
        public int amount;
        [Tooltip("챕터 깊이/플레이어 챕터 진행에 따라 스케일 (0=고정)")]
        public int chapterScaleMul = 0;
    }

    /// <summary>
    /// B8 — Endgame 모드 데이터 (endgame_v1 §8.1).
    /// 4종 SO: daily_boss / weekly_boss / infinite_tower / hardcore_chapter.
    /// SaveData.lastDailyBossClearTime 등과 결합해 EndgameResetService(C6) 가 가용성 판정.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/EndgameModeData")]
    public class EndgameModeData : ScriptableObject
    {
        public string id;
        public string displayName;
        public ModeType type;

        [Tooltip("daily=86400, weekly=604800, infinite=0, hardcore=0")]
        public int cooldownSeconds;

        [Header("보상")]
        public RewardEntry[] firstClearRewards;
        public RewardEntry[] repeatRewards;

        [Header("보스 풀 (DailyBoss/WeeklyBoss 전용)")]
        public EnemyData[] bossPool;

        // 5037.5 — 보스 전투 중 함께 spawn 되는 sub-enemy 풀 (Variant archetype 활용).
        [Header("Sub-enemy 풀 (5037.5 — 보스 전투 중 mix spawn)")]
        public EnemyData[] enemyPool;
        [Tooltip("보스 전투 동안 spawn 할 sub-enemy 총 마리수 (0=mix 비활성)")]
        public int subEnemySpawnCount = 0;

        [Header("무한 모드 깊이 → 보상 스케일 곡선 (InfiniteTower 전용)")]
        public AnimationCurve depthScaling;
    }
}
