using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Animal 보스 공격 — 1.5s telegraph → 0.5s 직선 돌진 (boss_attack_v1.md §3 사이클 1).
    /// telegraph 동안 sprite 적색 flash + scale ×1.2.
    /// 돌진 시 player 방향 직선 이동, 충돌 시 PlayerHealth.Damage.
    /// </summary>
    public class BossChargeAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 1.5f;
        private const float CHARGE_SEC = 0.5f;
        private const float CHARGE_SPEED = 12f;

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _player;
        private SpriteRenderer _sr;

        private enum State { Idle, Telegraph, Charging }
        private State _state = State.Idle;
        private float _stateTimer;
        private float _cooldown;
        private Vector2 _chargeDirection;
        private Color _origColor;
        private Vector3 _origScale;

        public void Initialize(EnemyController owner, EnemyData data)
        {
            _owner = owner;
            _data = data;
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
            _sr = owner.GetComponentInChildren<SpriteRenderer>();
            if (_sr != null) _origColor = _sr.color;
            _origScale = owner.transform.localScale;
            _cooldown = Random.Range(0f, data.bossAttackCooldown);  // 무작위 phase
        }

        public void Tick(float dt)
        {
            if (_owner == null || _owner.IsDead) return;

            switch (_state)
            {
                case State.Idle:
                    _cooldown -= dt;
                    if (_cooldown <= 0f) BeginTelegraph();
                    break;
                case State.Telegraph:
                    _stateTimer -= dt;
                    // sprite 적색 lerp + scale wobble
                    if (_sr != null)
                    {
                        float t = 1f - (_stateTimer / TELEGRAPH_SEC);
                        _sr.color = Color.Lerp(_origColor, Color.red, Mathf.PingPong(t * 6f, 1f));
                    }
                    _owner.transform.localScale = _origScale * (1f + Mathf.PingPong(Time.time * 4f, 0.2f));
                    if (_stateTimer <= 0f) BeginCharge();
                    break;
                case State.Charging:
                    _stateTimer -= dt;
                    _owner.transform.position += (Vector3)(_chargeDirection * CHARGE_SPEED * dt);
                    // 충돌 감지 (반경 0.5)
                    if (_player != null && Vector2.Distance(_owner.transform.position, _player.position) < 0.6f)
                        HitPlayer();
                    if (_stateTimer <= 0f) EndCharge();
                    break;
            }
        }

        private void BeginTelegraph()
        {
            _state = State.Telegraph;
            _stateTimer = TELEGRAPH_SEC;
            AudioManager.Instance?.Play(SfxType.BossTelegraph);
        }

        private void BeginCharge()
        {
            _state = State.Charging;
            _stateTimer = CHARGE_SEC;
            if (_sr != null) _sr.color = _origColor;
            _owner.transform.localScale = _origScale;
            AudioManager.Instance?.Play(SfxType.BossCharge);
            // 돌진 방향 = player 방향 (telegraph 시작 시점이 아닌 charge 시작 시점)
            if (_player != null)
                _chargeDirection = ((Vector2)(_player.position - _owner.transform.position)).normalized;
            else
                _chargeDirection = Vector2.right;
        }

        private void EndCharge()
        {
            _state = State.Idle;
            _cooldown = _data.bossAttackCooldown;
        }

        private void HitPlayer()
        {
            var ph = _player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
            EndCharge();  // 1회 충돌 후 종료
        }

        public void Cleanup()
        {
            if (_sr != null) _sr.color = _origColor;
            if (_owner != null) _owner.transform.localScale = _origScale;
        }
    }
}
