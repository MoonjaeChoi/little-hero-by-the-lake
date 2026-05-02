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

        public static void DealDamage(EnemyController enemy, float damage)
        {
            if (enemy == null || enemy.IsDead) return;
            bool isCrit = RollCrit();
            float critMul = isCrit ? GetCritDamageMul() : 1f;
            float final = damage * MythicEffectRuntime.CalcOutgoingDamageMultiplier(enemy) * critMul;
            // PIVOT v4 — OnHit 먼저 (적 1발 처치 시 SetActive(false) 후 coroutine 시작 실패 방지).
            // floater/audio/shake 는 enemy 비활성과 무관, punch/flash coroutine 만 적이 alive 일 때 동작.
            enemy.OnHit(final, isCrit);
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
