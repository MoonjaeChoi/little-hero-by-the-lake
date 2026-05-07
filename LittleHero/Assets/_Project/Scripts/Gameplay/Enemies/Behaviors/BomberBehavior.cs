using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T03 — 추적 + 가까이 가면 텔레그래프 후 자폭 (Explode).
    /// 사망 처리는 IDeathReaction (ExplodeReaction) 분리.
    /// </summary>
    public class BomberBehavior : IEnemyBehavior
    {
        private EnemyController _owner;
        private EnemyData _data;
        private Transform _target;
        private Rigidbody2D _rb;
        private float _telegraphTimer;
        private bool _telegraphing;

        public void Initialize(EnemyController owner, EnemyData data, Transform target)
        {
            _owner = owner; _data = data; _target = target;
            _rb = owner.GetComponent<Rigidbody2D>();
        }

        public void OnEnter() { _telegraphing = false; _telegraphTimer = 0f; }
        public void OnExit() { if (_rb != null) _rb.linearVelocity = Vector2.zero; }

        public void Tick(float deltaTime)
        {
            if (_owner == null || _owner.IsDead || _rb == null) return;
            if (_target == null) { _target = PlayerFinder.Get(); if (_target == null) return; }

            float dist = Vector2.Distance(_target.position, _owner.transform.position);

            if (!_telegraphing)
            {
                // 추적 (FollowBehavior 답습)
                Vector2 dir = ((Vector2)_target.position - (Vector2)_owner.transform.position).normalized;
                _rb.linearVelocity = dir * _data.moveSpeed;

                // bomberRadius 진입 시 텔레그래프 시작
                if (dist <= _data.bomberRadius)
                {
                    _telegraphing = true;
                    _telegraphTimer = 0f;
                    _rb.linearVelocity = Vector2.zero;
                    // 5033 VfxPool.SpawnWarning 시각 (warning circle)
                    VfxPool.SpawnWarning(_owner.transform.position, _data.bomberRadius, _data.bomberTelegraphSeconds);
                }
            }
            else
            {
                _telegraphTimer += deltaTime;
                if (_telegraphTimer >= _data.bomberTelegraphSeconds)
                {
                    // 자폭 — TakeDamage 로 Die 트리거 → ExplodeReaction
                    _owner.TakeDamage(_owner.MaxHealth + 1f);
                }
            }
        }
    }
}
