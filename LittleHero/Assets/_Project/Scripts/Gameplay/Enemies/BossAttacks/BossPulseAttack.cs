using LitMotion;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Civ 보스 공격 — 0.8s telegraph → 광역 폭발 (boss_attack_v1.md §3 사이클 1).
    /// telegraph 동안 sprite 청색 flash. 폭발 시 OverlapCircle (data.bossAttackRange) Player 데미지.
    /// </summary>
    public class BossPulseAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 0.8f;

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _player;
        private SpriteRenderer _sr;
        private Color _origColor;

        private enum State { Idle, Telegraph }
        private State _state = State.Idle;
        private float _stateTimer;
        private float _cooldown;

        public void Initialize(EnemyController owner, EnemyData data)
        {
            _owner = owner;
            _data = data;
            var p = GameObject.FindWithTag("Player");
            if (p != null) _player = p.transform;
            _sr = owner.GetComponentInChildren<SpriteRenderer>();
            if (_sr != null) _origColor = _sr.color;
            _cooldown = Random.Range(0f, data.bossAttackCooldown);
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
                    if (_sr != null)
                    {
                        float t = 1f - (_stateTimer / TELEGRAPH_SEC);
                        _sr.color = Color.Lerp(_origColor, new Color(0.4f, 0.7f, 1f), Mathf.PingPong(t * 6f, 1f));
                    }
                    if (_stateTimer <= 0f) Detonate();
                    break;
            }
        }

        private void BeginTelegraph()
        {
            _state = State.Telegraph;
            _stateTimer = TELEGRAPH_SEC;
            VfxPool.SpawnWarning(_owner.transform.position, _data.bossAttackRange, TELEGRAPH_SEC);
            AudioManager.Instance?.Play(SfxType.BossTelegraph);
        }

        private void Detonate()
        {
            AudioManager.Instance?.Play(SfxType.BossDetonate);
            // OverlapCircle — Player 만 검사
            if (_player != null)
            {
                float dist = Vector2.Distance(_owner.transform.position, _player.position);
                if (dist < _data.bossAttackRange)
                {
                    var ph = _player.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
                }
            }
            // VFX (기존 별 흩어짐 재사용)
            VfxPool.SpawnStarBurst(_owner.transform.position);
            // 색상 복원
            if (_sr != null) _sr.color = _origColor;
            _state = State.Idle;
            _cooldown = _data.bossAttackCooldown;
        }

        public void Cleanup()
        {
            if (_sr != null) _sr.color = _origColor;
        }
    }
}
