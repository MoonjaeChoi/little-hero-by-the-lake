using System;
using UnityEngine;

namespace LittleHero
{
    public class EnemyController : MonoBehaviour
    {
        public event Action<EnemyController> OnDied;

        private EnemyData _data;
        private float _currentHealth;
        private bool _isDead;

        public EnemyData Data => _data;
        public bool IsDead => _isDead;
        public float HealthRatio => _currentHealth / _data.maxHealth;

        public void Initialize(EnemyData data)
        {
            _data = data;
            _currentHealth = data.maxHealth;
            _isDead = false;
        }

        public void TakeDamage(float damage)
        {
            if (_isDead) return;

            _currentHealth = Mathf.Max(0f, _currentHealth - damage);

            if (_currentHealth <= 0f)
                Die();
        }

        private void Die()
        {
            _isDead = true;
            GameEvents.RaiseEnemyKilled(_data);
            OnDied?.Invoke(this);

            // 풀에서 반환되지 않은 경우 안전 비활성화
            if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }
    }
}
