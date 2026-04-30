using System;
using UnityEngine;

namespace LittleHero
{
    // 프리팹 요구사항: Rigidbody2D + CircleCollider2D (IsTrigger = true)
    public class Projectile : MonoBehaviour
    {
        private float _damage;
        private float _speed;
        private float _maxRange;
        private Vector2 _startPosition;
        private Rigidbody2D _rb;
        private Action<Projectile> _onReturn;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null)
            {
                _rb.gravityScale = 0f;
                _rb.freezeRotation = true;
            }
        }

        public void Launch(Vector2 direction, float damage, float speed, float range, Action<Projectile> onReturn)
        {
            _damage = damage;
            _speed = speed;
            _maxRange = range;
            _startPosition = transform.position;
            _onReturn = onReturn;
            _rb.linearVelocity = direction.normalized * _speed;
        }

        private void FixedUpdate()
        {
            if (Vector2.Distance(_startPosition, (Vector2)transform.position) >= _maxRange)
                ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy == null) return;

            DamageSystem.DealDamage(enemy, _damage);
            ReturnToPool();
        }

        private void ReturnToPool()
        {
            _rb.linearVelocity = Vector2.zero;
            _onReturn?.Invoke(this);
        }
    }
}
