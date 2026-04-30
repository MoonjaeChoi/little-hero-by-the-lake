using UnityEngine;

namespace LittleHero
{
    public class PlayerHealth : MonoBehaviour
    {
        private PlayerStats _stats;

        public float CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }
        public float HealthRatio => CurrentHealth / _stats.MaxHealth;

        public void Initialize(PlayerStats stats)
        {
            _stats = stats;
            CurrentHealth = stats.MaxHealth;
            IsDead = false;
            GameEvents.RaisePlayerHealthChanged(Mathf.RoundToInt(CurrentHealth));
        }

        public void TakeDamage(float damage)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            GameEvents.RaisePlayerHealthChanged(Mathf.RoundToInt(CurrentHealth));

            if (CurrentHealth <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (IsDead) return;

            CurrentHealth = Mathf.Min(_stats.MaxHealth, CurrentHealth + amount);
            GameEvents.RaisePlayerHealthChanged(Mathf.RoundToInt(CurrentHealth));
        }

        private void Die()
        {
            IsDead = true;
            GameEvents.RaisePlayerHealthChanged(0);
            GameEvents.RaisePlayerDied();
        }
    }
}
