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

            DamageSystem.DealDamageToPlayer(_health, enemy.Data.damage);
            _lastDamageTime = Time.time;
        }
    }
}
