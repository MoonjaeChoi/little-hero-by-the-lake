using System.Collections.Generic;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v4 — 다중 타깃 데미지 적용 정적 헬퍼 (damage_system_v2.md §5.2).
    /// 메이플 키우기 풍 다중 타깃 보정 (Little Hero 완화: 60% cap).
    /// </summary>
    public static class DamageDealer
    {
        // 메이플 26% 보정 → Little Hero 완화 (active gameplay)
        // 1차 100% / 2차 80% / 3차 70% / 4차+ 60% (cap)
        private static readonly float[] _multiTargetPenalty = { 1f, 0.8f, 0.7f, 0.6f };
        private const float MULTI_PENALTY_FLOOR = 0.6f;

        /// <summary>다중 타깃 데미지 — 거리 정렬된 list 가정. PIVOT v4 OnHit 자동.</summary>
        public static void ApplyMulti(List<EnemyController> targets, float baseDamage)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null || targets[i].IsDead) continue;
                float penalty = i < _multiTargetPenalty.Length ? _multiTargetPenalty[i] : MULTI_PENALTY_FLOOR;
                DamageSystem.DealDamage(targets[i], baseDamage * penalty);
            }
        }

        /// <summary>단일 타깃 — 100% 데미지.</summary>
        public static void ApplySingle(EnemyController target, float damage)
        {
            if (target == null || target.IsDead) return;
            DamageSystem.DealDamage(target, damage);
        }

        /// <summary>다중 타깃 보정 비율 미리보기 (UI / 시뮬용).</summary>
        public static float GetPenalty(int targetIndex)
        {
            if (targetIndex < 0) return 1f;
            return targetIndex < _multiTargetPenalty.Length ? _multiTargetPenalty[targetIndex] : MULTI_PENALTY_FLOOR;
        }
    }
}
