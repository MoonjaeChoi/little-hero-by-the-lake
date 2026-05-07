using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T03 — 위치 고정 (Stationary) + 주기적 발사체 (D2=B 사용자 결정).
    /// 사용자 요청 핵심 — 미니 적 중 위치 고정. rangedFireInterval 마다 plane projectilePrefab spawn.
    /// </summary>
    public class StationaryBehavior : IEnemyBehavior
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
            _fireTimer = 0f;
        }

        public void OnEnter() { if (_rb != null) _rb.linearVelocity = Vector2.zero; }
        public void OnExit() { /* no-op */ }

        public void Tick(float deltaTime)
        {
            if (_owner == null || _owner.IsDead) return;
            if (_rb != null) _rb.linearVelocity = Vector2.zero;  // 위치 고정

            if (_data.projectilePrefab == null) return;
            if (_target == null) { _target = PlayerFinder.Get(); if (_target == null) return; }

            _fireTimer += deltaTime;
            if (_fireTimer < _data.rangedFireInterval) return;
            _fireTimer = 0f;
            FireProjectile();
        }

        private void FireProjectile()
        {
            Vector2 dir = ((Vector2)_target.position - (Vector2)_owner.transform.position).normalized;
            var go = Object.Instantiate(_data.projectilePrefab, _owner.transform.position, Quaternion.identity);
            // 5037.1 — EnemyProjectile.Launch 위임 (player 데미지 + lifetime 통합)
            var ep = go.GetComponent<EnemyProjectile>();
            if (ep != null) ep.Launch(dir, _data.damage);
            else
            {
                // legacy fallback (5035) — Rigidbody2D 직접 + 3s destroy
                var rb = go.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = dir * 4f;
                Object.Destroy(go, 3f);
            }
        }
    }
}
