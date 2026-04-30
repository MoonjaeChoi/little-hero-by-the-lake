using UnityEngine;

namespace LittleHero
{
    public class PlayerDamageReceiver : MonoBehaviour
    {
        [SerializeField] private float _damageCooldown = 1f;

        private PlayerHealth _health;
        private float _lastDamageTime;

        private void Awake()
        {
            _health = GetComponent<PlayerHealth>();
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (Time.time - _lastDamageTime < _damageCooldown) return;
            if (_health == null || _health.IsDead) return;

            EnemyController enemy = collision.gameObject.GetComponent<EnemyController>();
            if (enemy == null || enemy.IsDead) return;

            // A4 — 챕터 곱셈이 적용된 공격력 사용 (enemy.Data.damage는 SO 베이스값이라 사용 금지)
            DamageSystem.DealDamageToPlayer(_health, enemy.AttackDamage);
            _lastDamageTime = Time.time;
        }
    }
}
