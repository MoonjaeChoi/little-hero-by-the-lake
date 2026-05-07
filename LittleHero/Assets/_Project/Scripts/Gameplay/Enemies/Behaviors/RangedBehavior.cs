using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T03 — 거리 유지 + 주기적 발사체. 너무 가까우면 후퇴, 너무 멀면 접근.
    /// </summary>
    public class RangedBehavior : IEnemyBehavior
    {
        private EnemyController _owner;
        private EnemyData _data;
        private Transform _target;
        private Rigidbody2D _rb;
        private float _fireTimer;

        public void Initialize(EnemyController owner, EnemyData data, Transform target)
        {
            _owner = owner; _data = data; _target = target;
            _rb = owner.GetComponent<Rigidbody2D>();
        }

        public void OnEnter() { _fireTimer = 0f; }
        public void OnExit() { if (_rb != null) _rb.linearVelocity = Vector2.zero; }

        public void Tick(float deltaTime)
        {
            if (_owner == null || _owner.IsDead || _rb == null) return;
            if (_target == null) { _target = PlayerFinder.Get(); if (_target == null) return; }

            Vector2 toTarget = (Vector2)_target.position - (Vector2)_owner.transform.position;
            float dist = toTarget.magnitude;
            Vector2 dir = toTarget.normalized;

            // 거리 유지 — keepDistance 보다 가까우면 후퇴, 멀면 접근
            if (dist < _data.rangedKeepDistance - 0.5f)
                _rb.linearVelocity = -dir * _data.moveSpeed * 0.7f;
            else if (dist > _data.rangedKeepDistance + 0.5f)
                _rb.linearVelocity = dir * _data.moveSpeed * 0.7f;
            else
                _rb.linearVelocity = Vector2.zero;

            // 주기적 발사
            if (_data.projectilePrefab == null) return;
            _fireTimer += deltaTime;
            if (_fireTimer < _data.rangedFireInterval) return;
            _fireTimer = 0f;

            var go = Object.Instantiate(_data.projectilePrefab, _owner.transform.position, Quaternion.identity);
            var prb = go.GetComponent<Rigidbody2D>();
            if (prb != null) prb.linearVelocity = dir * 4f;
            Object.Destroy(go, 3f);
        }
    }
}
