using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 데미지 적용 진입점. F8 — 신화 효과 곱셈 자동 반영.
    /// PIVOT v4 — 자체 crit roll + EnemyController.OnHit 호출 (cinematic_combat_v1.md §6).
    /// 호출자는 베이스 데미지만 전달, 신화 보정·crit 은 본 클래스에서 일괄 처리.
    /// </summary>
    public static class DamageSystem
    {
        private static PlayerStats _statsCache;

        public static void DealDamage(EnemyController enemy, float damage, WeaponCategory? weaponCategory = null)
        {
            if (enemy == null || enemy.IsDead) return;
            bool isCrit = RollCrit();
            float critMul = isCrit ? GetCritDamageMul() : 1f;
            float final = damage * MythicEffectRuntime.CalcOutgoingDamageMultiplier(enemy) * critMul;

            // stats_v2 §10 — armor reduction (메이플 공식: dmg × (1 - def/(def+1000)))
            var s = GetStats();
            if (s != null && enemy.Data != null && enemy.Data.armor > 0)
            {
                float defense = enemy.Data.armor * (1f - s.ArmorIgnore);
                if (defense > 0) final *= 1f - defense / (defense + 1000f);
            }

            // stats_v2 — Lifesteal (cap 50%)
            if (s != null && s.LifestealRatio > 0)
            {
                var health = Object.FindFirstObjectByType<PlayerHealth>(FindObjectsInactive.Include);
                if (health != null) health.Heal(final * Mathf.Min(s.LifestealRatio, 0.5f));
            }

            // 5007 D2=B — avgDamage DamageSystem 에서 1회 query → OnHit 인자 전달
            // (EnemyController 의 PlayerStats 의존성 0)
            float avgPlayerDamage = s != null ? s.AttackDamage : 0f;

            // PIVOT v4 — OnHit 먼저 (적 1발 처치 시 SetActive(false) 후 coroutine 실패 방지).
            enemy.OnHit(final, isCrit, weaponCategory, avgPlayerDamage);
            enemy.TakeDamage(final);
        }

        public static void DealDamageToPlayer(PlayerHealth player, float damage)
        {
            if (player == null || player.IsDead) return;
            float final = damage * MythicEffectRuntime.CalcIncomingDamageMultiplier(player);
            player.TakeDamage(final);
        }

        private static PlayerStats GetStats()
        {
            if (_statsCache != null) return _statsCache;
            _statsCache = Object.FindFirstObjectByType<PlayerStats>(FindObjectsInactive.Include);
            return _statsCache;
        }

        private static bool RollCrit()
        {
            var s = GetStats();
            float chance = s != null ? s.CritChance : 0.05f;
            return Random.value < chance;
        }

        private static float GetCritDamageMul()
        {
            var s = GetStats();
            return s != null && s.CritDamage > 1f ? s.CritDamage : 1.5f;
        }
    }
}
