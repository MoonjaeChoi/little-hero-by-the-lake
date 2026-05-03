using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 보스 페이즈 전환 관리자 (5001 / Sprint 4 / T07).
    /// EnemyController.UpdateAnim 가 매 프레임 Update(this, deltaTime) 호출.
    /// CLAUDE.md §15.7 정적 헬퍼 (Singleton 직접 참조 X).
    /// CreateInitial — bossTier == None 면 null (일반 적 phase 시스템 X).
    /// </summary>
    public static class BossPhaseManager
    {
        public static IBossPhase CreateInitial(EnemyController boss)
        {
            if (boss == null || boss.Data == null) return null;
            if (boss.Data.bossTier == BossTier.None) return null;
            return new BossPhase1Default();
        }

        public static void Update(EnemyController boss, float deltaTime)
        {
            if (boss == null || boss.CurrentPhase == null) return;
            boss.CurrentPhase.Tick(boss, deltaTime);
            if (boss.CurrentPhase.ShouldTransitionNext(boss))
            {
                TransitionToNext(boss);
            }
        }

        private static void TransitionToNext(EnemyController boss)
        {
            var oldPhase = boss.CurrentPhase;
            if (oldPhase == null) return;
            oldPhase.OnExit(boss);

            // Phase 1 → Phase 2 만 지원 (Phase 3 X)
            IBossPhase next = oldPhase is BossPhase1Default ? new BossPhase2Aggressive() : null;
            boss.SetCurrentPhase(next);
            next?.OnEnter(boss);

            string id = boss.Data?.id ?? "(unknown)";
            float hp = boss.HealthRatio;
            Debug.Log($"[Boss] {id} Phase 1 → Phase 2 (HP {hp:P0})");
        }
    }
}
