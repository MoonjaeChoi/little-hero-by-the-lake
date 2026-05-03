namespace LittleHero
{
    /// <summary>
    /// 보스 Phase 1 (HP 100~50%) 기본 패턴 (5001 / Sprint 1.2 / T02).
    /// EnemyData.bossAttackType 의 IBossAttackPattern 을 BossPatternFactory 로 인스턴스화.
    /// ChapterBoss+ 는 HP &lt;= phase2Threshold 시 Phase 2 전환.
    /// MiniBoss 는 phase2Pattern=None 이라 ShouldTransitionNext 가 항상 false 처럼 동작 (BossPhaseManager 가 차단).
    /// </summary>
    public sealed class BossPhase1Default : IBossPhase
    {
        private IBossAttackPattern _pattern;

        public void OnEnter(EnemyController boss)
        {
            if (boss == null || boss.Data == null) return;
            _pattern = BossPatternFactory.Create(boss.Data.bossAttackType);
            _pattern?.Initialize(boss, boss.Data);
        }

        public void Tick(EnemyController boss, float deltaTime)
        {
            _pattern?.Tick(deltaTime);
        }

        public void OnExit(EnemyController boss)
        {
            _pattern?.Cleanup();
            _pattern = null;
        }

        public bool ShouldTransitionNext(EnemyController boss)
        {
            if (boss == null || boss.Data == null) return false;
            // ChapterBoss 이상만 Phase 2 진입 (D3=A: MiniBoss = Phase 1 만)
            if (boss.Data.bossTier < BossTier.ChapterBoss) return false;
            // phase2Pattern 이 명세된 경우만 전환 (None 이면 보스 attack 미명세 → Phase 1 유지)
            if (boss.Data.phase2Pattern == BossAttackType.None) return false;
            return boss.HealthRatio <= boss.Data.phase2Threshold;
        }
    }
}
