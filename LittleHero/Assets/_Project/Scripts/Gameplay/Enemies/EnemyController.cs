using System;
using UnityEngine;

namespace LittleHero
{
    public class EnemyController : MonoBehaviour
    {
        public event Action<EnemyController> OnDied;

        private EnemyData _data;
        private float _currentHealth;
        private float _maxHealth;
        private float _attackDamage;
        private bool _isDead;

        public EnemyData Data => _data;
        public bool IsDead => _isDead;
        public float MaxHealth => _maxHealth;
        public float AttackDamage => _attackDamage;
        public float HealthRatio => _maxHealth > 0f ? _currentHealth / _maxHealth : 0f;

        // A4 — chapter_curve §9.1: HP/공격력에 챕터 곱셈 적용. multiplier=1f면 영향 없음.
        public void Initialize(EnemyData data, float statMultiplier = 1f)
        {
            _data = data;
            float m = Mathf.Max(0.01f, statMultiplier);
            _maxHealth     = data.maxHealth * m;
            _attackDamage  = data.damage    * m;
            _currentHealth = _maxHealth;
            _isDead = false;
            ApplyAppearance(data);
        }

        private void ApplyAppearance(EnemyData data)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = data.tintColor;
            float s = data.scaleMultiplier > 0f ? data.scaleMultiplier : 1f;
            transform.localScale = new Vector3(s, s, 1f);
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
