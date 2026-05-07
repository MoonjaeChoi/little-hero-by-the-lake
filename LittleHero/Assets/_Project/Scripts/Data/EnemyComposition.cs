using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T05 — Wave 안 단일 적 그룹 정의 (SO 재사용 ↑).
    /// 1 EnemyComposition = 1 적 종류 + 수량 + spawn 패턴.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/EnemyComposition")]
    public class EnemyComposition : ScriptableObject
    {
        [Tooltip("적 SO — 이 그룹에서 spawn 할 적 종류")]
        public EnemyData enemyData;

        [Tooltip("이 그룹에서 spawn 할 적 마리수 (continuous 시 한 wave 동안 누적)")]
        public int count = 10;

        [Tooltip("spawn 패턴 — Continuous / Burst / Timed")]
        public SpawnPattern pattern = SpawnPattern.Continuous;

        [Tooltip("spawn 간격 (Continuous=마리 간격, Burst=한 번 모두, Timed=매 N초)")]
        public float interval = 0.5f;

        [Tooltip("Burst 시작 지연 (wave 시작 후 N초 후 spawn)")]
        public float startDelay = 0f;
    }

    public enum SpawnPattern
    {
        Continuous = 0,  // 일정 간격 누적 spawn (기존 wave 패턴)
        Burst      = 1,  // 한 번에 모두 spawn
        Timed      = 2,  // 정확한 시점 (startDelay 후)
    }
}
