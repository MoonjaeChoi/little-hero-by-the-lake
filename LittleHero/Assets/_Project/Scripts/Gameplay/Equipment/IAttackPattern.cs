using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 자동 공격 패턴 (CLAUDE.md §6).
    /// C8 — `PlayerStats` 인자 추가. 패턴은 본 stats 의 `AttackSpeed`(공격 간격 단축)·
    /// `ExtraAttackTargets`(투사체/타겟 +N)·`AttackDamage` 등을 EffectiveInterval/EffectiveCount 으로
    /// 반영해 적용한다(stats_v1 §6 + ui_v1 §9). stats 는 nullable — 미지정 시 베이스 동작.
    /// </summary>
    public interface IAttackPattern
    {
        void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats);
        void Tick(float deltaTime);
        void Cleanup();
    }

    /// <summary>
    /// 패턴이 자주 쓰는 stats-아웨어 헬퍼. 정적 메서드 — IAttackPattern 구현체에서 호출.
    /// </summary>
    public static class AttackPatternUtil
    {
        /// <summary>baseInterval / max(0.1, AttackSpeed) — AttackSpeed 1.0 = 베이스.</summary>
        public static float EffectiveInterval(float baseInterval, PlayerStats stats)
            => baseInterval / Mathf.Max(0.1f, stats != null ? stats.AttackSpeed : 1f);

        /// <summary>max(1, baseCount) + ExtraAttackTargets.</summary>
        public static int EffectiveCount(int baseCount, PlayerStats stats)
            => Mathf.Max(1, baseCount) + (stats != null ? stats.ExtraAttackTargets : 0);
    }
}
