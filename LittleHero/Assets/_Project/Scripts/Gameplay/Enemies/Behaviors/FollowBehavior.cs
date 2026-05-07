using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T02 — 기본 추적 behavior (기존 EnemyAI 마이그레이션, D5=A 폐기 후속).
    /// 매 Tick 플레이어 향 이동 — Rigidbody2D linearVelocity = direction × moveSpeed.
    /// </summary>
    public class FollowBehavior : IEnemyBehavior
    {
        private EnemyController _owner;
        private EnemyData _data;
        private Transform _target;
        private Rigidbody2D _rb;

        public void Initialize(EnemyController owner, EnemyData data, Transform target)
        {
            _owner = owner;
            _data = data;
            _target = target;
            _rb = owner.GetComponent<Rigidbody2D>();
        }

        public void OnEnter() { /* no-op */ }
        public void OnExit() { if (_rb != null) _rb.linearVelocity = Vector2.zero; }

        public void Tick(float deltaTime)
        {
            if (_owner == null || _owner.IsDead || _rb == null) return;
            if (_target == null) { _target = PlayerFinder.Get(); if (_target == null) return; }
            Vector2 dir = ((Vector2)_target.position - (Vector2)_owner.transform.position).normalized;
            _rb.linearVelocity = dir * _data.moveSpeed;
        }
    }
}
