using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T03 — 평소 추적, chargerInterval 마다 텔레그래프 + 빠른 돌진 (3× 속도).
    /// </summary>
    public class ChargerBehavior : IEnemyBehavior
    {
        private const float TELEGRAPH_SECONDS = 0.6f;
        private const float CHARGE_SECONDS = 0.5f;

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _target;
        private Rigidbody2D _rb;
        private float _intervalTimer;
        private float _chargeTimer;
        private Vector2 _chargeDir;
        private enum State { Follow, Telegraph, Charging }
        private State _state;

        public void Initialize(EnemyController owner, EnemyData data, Transform target)
        {
            _owner = owner; _data = data; _target = target;
            _rb = owner.GetComponent<Rigidbody2D>();
            _state = State.Follow;
        }

        public void OnEnter() { _intervalTimer = 0f; _state = State.Follow; }
        public void OnExit() { if (_rb != null) _rb.linearVelocity = Vector2.zero; }

        public void Tick(float deltaTime)
        {
            if (_owner == null || _owner.IsDead || _rb == null) return;
            if (_target == null) { _target = PlayerFinder.Get(); if (_target == null) return; }

            switch (_state)
            {
                case State.Follow:
                    _intervalTimer += deltaTime;
                    Vector2 dir = ((Vector2)_target.position - (Vector2)_owner.transform.position).normalized;
                    _rb.linearVelocity = dir * _data.moveSpeed;
                    if (_intervalTimer >= _data.chargerInterval)
                    {
                        _state = State.Telegraph;
                        _chargeTimer = 0f;
                        _chargeDir = ((Vector2)_target.position - (Vector2)_owner.transform.position).normalized;
                    }
                    break;
                case State.Telegraph:
                    _rb.linearVelocity = Vector2.zero;
                    _chargeTimer += deltaTime;
                    if (_chargeTimer >= TELEGRAPH_SECONDS)
                    {
                        _state = State.Charging;
                        _chargeTimer = 0f;
                    }
                    break;
                case State.Charging:
                    _rb.linearVelocity = _chargeDir * _data.moveSpeed * _data.chargerSpeedMultiplier;
                    _chargeTimer += deltaTime;
                    if (_chargeTimer >= CHARGE_SECONDS)
                    {
                        _state = State.Follow;
                        _intervalTimer = 0f;
                    }
                    break;
            }
        }
    }
}
