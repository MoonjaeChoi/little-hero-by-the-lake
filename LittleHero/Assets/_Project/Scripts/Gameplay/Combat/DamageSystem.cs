namespace LittleHero
{
    /// <summary>
    /// 데미지 적용 진입점. F8 — 신화 효과 곱셈 자동 반영.
    /// 호출자는 베이스 데미지만 전달, 신화 보정은 본 클래스에서 일괄 처리.
    /// </summary>
    public static class DamageSystem
    {
        public static void DealDamage(EnemyController enemy, float damage)
        {
            if (enemy == null || enemy.IsDead) return;
            float final = damage * MythicEffectRuntime.CalcOutgoingDamageMultiplier(enemy);
            enemy.TakeDamage(final);
        }

        public static void DealDamageToPlayer(PlayerHealth player, float damage)
        {
            if (player == null || player.IsDead) return;
            float final = damage * MythicEffectRuntime.CalcIncomingDamageMultiplier(player);
            player.TakeDamage(final);
        }
    }
}
