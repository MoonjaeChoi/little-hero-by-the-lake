using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// A3 — 3 데미지 버킷 + 방어 관통.
    /// 최종 데미지 = base
    ///             × (1 + AdditiveDamageBonus)        ← 버킷 1 (합산)
    ///             × (1 + FinalDamageBonus)            ← 버킷 2 (메이플 키우기 핵심)
    ///             × (1 + CritChance × (CritDamage-1)) ← 버킷 3 (평균) 또는 RNG 분기
    ///   ÷ (1 + EffectiveArmor / 100)
    /// EffectiveArmor = enemy.armor × (1 - ArmorPenetration)
    ///
    /// 호출자는 두 모드 중 선택:
    ///   - <see cref="CalculateExpectedDamage"/>: UI/시뮬용 평균 (크리 기댓값)
    ///   - <see cref="RollDamage"/>: 런타임 1회 공격 (크리 RNG)
    ///
    /// (참조: docs/little_hero/stats_v1.md §6, §1 데미지 버킷)
    /// </summary>
    public static class DamageCalculator
    {
        // 평균 데미지 — 크리티컬을 확률 가중 기댓값으로 처리. UI/시뮬용.
        public static float CalculateExpectedDamage(PlayerStats stats, EnemyData enemy, float baseOverride = -1f)
        {
            if (stats == null) return 0f;
            float baseDmg = baseOverride >= 0f ? baseOverride : stats.AttackDamage;
            float b1 = 1f + stats.AdditiveDamageBonus;
            float b2 = 1f + stats.FinalDamageBonus;
            float critContribution = Mathf.Max(0f, stats.CritDamage - 1f);
            float b3 = 1f + Mathf.Clamp01(stats.CritChance) * critContribution;
            float pre = baseDmg * b1 * b2 * b3;
            return ApplyArmor(pre, stats, enemy);
        }

        // 실제 1회 공격 — 크리티컬 RNG 분기. isCrit out 파라미터로 연출 트리거 가능.
        public static float RollDamage(PlayerStats stats, EnemyData enemy, out bool isCrit, float baseOverride = -1f)
        {
            isCrit = false;
            if (stats == null) return 0f;
            float baseDmg = baseOverride >= 0f ? baseOverride : stats.AttackDamage;
            float b1 = 1f + stats.AdditiveDamageBonus;
            float b2 = 1f + stats.FinalDamageBonus;
            isCrit = Random.value < Mathf.Clamp01(stats.CritChance);
            float b3 = isCrit ? Mathf.Max(1f, stats.CritDamage) : 1f;
            float pre = baseDmg * b1 * b2 * b3;
            return ApplyArmor(pre, stats, enemy);
        }

        // out 파라미터 없이 단순 호출용
        public static float RollDamage(PlayerStats stats, EnemyData enemy, float baseOverride = -1f)
            => RollDamage(stats, enemy, out _, baseOverride);

        private static float ApplyArmor(float predmg, PlayerStats stats, EnemyData enemy)
        {
            if (predmg <= 0f) return 0f;
            float armor = enemy != null ? enemy.armor : 0f;
            float pen = stats != null ? Mathf.Clamp01(stats.ArmorPenetration) : 0f;
            float effective = Mathf.Max(0f, armor * (1f - pen));
            return predmg / (1f + effective / 100f);
        }
    }
}
