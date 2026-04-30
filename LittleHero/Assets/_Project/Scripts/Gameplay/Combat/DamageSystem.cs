namespace LittleHero
{
    public static class DamageSystem
    {
        public static void DealDamage(EnemyController enemy, float damage)
        {
            if (enemy == null || enemy.IsDead) return;
            enemy.TakeDamage(damage);
        }

        public static void DealDamageToPlayer(PlayerHealth player, float damage)
        {
            if (player == null || player.IsDead) return;
            player.TakeDamage(damage);
        }
    }
}
