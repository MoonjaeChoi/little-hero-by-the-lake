using System;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v4 / Damage v2 — 충돌 폐기 + DamageQuery.FindNearest (damage_system_v2.md §6).
    /// Rigidbody2D 보존 (자체 이동), OnTriggerEnter2D 폐기.
    /// 매 프레임 OverlapCircle 0.4f 반경 검사 → 적 발견 시 ApplySingle + Pool 반환.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        private const float HIT_RADIUS = 0.4f;

        private float _damage;
        private float _speed;
        private float _maxRange;
        private Vector2 _startPosition;
        private Rigidbody2D _rb;
        private Action<Projectile> _onReturn;
        private bool _hit;

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
            _hit = false;
            if (_rb != null) _rb.linearVelocity = direction.normalized * _speed;
        }

        private void Update()
        {
            if (_hit) return;
            // Damage v2 — Layer Enemy 검사 (Collider 충돌 폐기)
            var enemy = DamageQuery.FindNearest(transform.position, HIT_RADIUS);
            if (enemy != null)
            {
                DamageDealer.ApplySingle(enemy, _damage);
                _hit = true;
                ReturnToPool();
                return;
            }
            // Range 종료
            if (Vector2.Distance(_startPosition, (Vector2)transform.position) >= _maxRange)
                ReturnToPool();
        }

        private void ReturnToPool()
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            _onReturn?.Invoke(this);
        }
    }
}
