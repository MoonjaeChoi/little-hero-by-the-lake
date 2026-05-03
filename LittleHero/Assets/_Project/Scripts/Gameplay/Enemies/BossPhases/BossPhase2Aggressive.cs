using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 보스 Phase 2 (HP 50~0%) 강화 패턴 (5001 / Sprint 1.2 / T02).
    /// EnemyData.phase2Pattern 의 IBossAttackPattern 을 BossPatternFactory 로 인스턴스화.
    /// EnemyData.phase2SpeedMultiplier (D2=C SerializeField, 기본 1.3×) 로 attack interval 가속.
    /// 텔레그라프 단축 효과는 IBossAttackPattern 구현체 내부 처리 (현재는 Tick 호출 빈도만 가속).
    /// </summary>
    public sealed class BossPhase2Aggressive : IBossPhase
    {
        private IBossAttackPattern _pattern;
        private float _speedMultiplier = 1.3f;

        public void OnEnter(EnemyController boss)
        {
            if (boss == null || boss.Data == null) return;
            _speedMultiplier = boss.Data.phase2SpeedMultiplier > 0f
                ? boss.Data.phase2SpeedMultiplier
                : 1.3f;
            _pattern = BossPatternFactory.Create(boss.Data.phase2Pattern);
            _pattern?.Initialize(boss, boss.Data);
            Debug.Log($"[BossPhase2.OnEnter] {boss.Data.id} — Aggressive mode (×{_speedMultiplier:F2} speed, pattern={boss.Data.phase2Pattern})");
        }

        public void Tick(EnemyController boss, float deltaTime)
        {
            // 속도 배율 적용 — Tick deltaTime 가속 (1.3× 면 더 자주 attack 사이클 진행)
            _pattern?.Tick(deltaTime * _speedMultiplier);
        }

        public void OnExit(EnemyController boss)
        {
            _pattern?.Cleanup();
            _pattern = null;
        }

        public bool ShouldTransitionNext(EnemyController boss) => false;  // Phase 2 가 끝 (Phase 3 X)
    }
}
