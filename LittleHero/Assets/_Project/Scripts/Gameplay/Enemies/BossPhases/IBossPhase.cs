namespace LittleHero
{
    /// <summary>
    /// 보스 페이즈 State 패턴 (5001 boss_system_v2 / Sprint 1.2 / T02).
    /// CLAUDE.md §15.1 ISP — 4 메서드 작은 인터페이스.
    /// 구현체: BossPhase1Default / BossPhase2Aggressive (ChapterBoss+ 만 Phase 2).
    /// </summary>
    public interface IBossPhase
    {
        /// <summary>Phase 진입 — attack pattern 인스턴스화 + 초기화.</summary>
        void OnEnter(EnemyController boss);

        /// <summary>매 프레임 호출 (EnemyController.UpdateAnim 위임).</summary>
        void Tick(EnemyController boss, float deltaTime);

        /// <summary>Phase 종료 — pattern Cleanup.</summary>
        void OnExit(EnemyController boss);

        /// <summary>다음 Phase 전환 조건 검사 (BossPhaseManager 가 매 프레임 호출).</summary>
        bool ShouldTransitionNext(EnemyController boss);
    }
}
