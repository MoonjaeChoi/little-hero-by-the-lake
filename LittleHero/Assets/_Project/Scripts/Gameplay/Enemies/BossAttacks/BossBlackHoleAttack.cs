using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Weekly 보스 공격 — 흡인 + 폭발 (boss_attack_v1.md §3 사이클 3).
    /// 1.5s telegraph → 2s 흡인 (player 매 프레임 boss 방향 끌어당김) → 폭발 (반경 데미지).
    /// </summary>
    public class BossBlackHoleAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 1.5f;
        private const float SUCK_DURATION = 2f;
        private const float SUCK_FORCE = 4f;

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _player;
        private SpriteRenderer _sr;
        private Color _origColor;

        private enum State { Idle, Telegraph, Sucking }
        private State _state = State.Idle;
        private float _stateTimer, _cooldown;
        private GameObject _visual;
        private static Sprite _bhSprite;

        public void Initialize(EnemyController owner, EnemyData data)
        {
            _owner = owner; _data = data;
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
                    if (_cooldown <= 0f) {
                        _state = State.Telegraph; _stateTimer = TELEGRAPH_SEC;
                        VfxPool.SpawnWarning(_owner.transform.position, _data.bossAttackRange, TELEGRAPH_SEC);
                        AudioManager.Instance?.Play(SfxType.BossTelegraph);
                    }
                    break;
                case State.Telegraph:
                    _stateTimer -= dt;
                    if (_sr != null)
                        _sr.color = Color.Lerp(_origColor, new Color(0.4f, 0.1f, 0.5f), Mathf.PingPong((TELEGRAPH_SEC - _stateTimer) * 4f, 1f));
                    if (_stateTimer <= 0f) {
                        _state = State.Sucking; _stateTimer = SUCK_DURATION;
                        SpawnVisual();
                    }
                    break;
                case State.Sucking:
                    _stateTimer -= dt;
                    if (_player != null)
                    {
                        Vector3 toBoss = _owner.transform.position - _player.position;
                        _player.position += toBoss.normalized * SUCK_FORCE * dt;
                    }
                    if (_visual != null) {
                        _visual.transform.position = _owner.transform.position;
                        _visual.transform.Rotate(0f, 0f, 360f * dt);
                        // 흡인 동안 scale pulse
                        float s = _data.bossAttackRange * 2f * (1f + Mathf.Sin(Time.time * 5f) * 0.1f);
                        _visual.transform.localScale = new Vector3(s, s, 1f);
                    }
                    if (_stateTimer <= 0f) Detonate();
                    break;
            }
        }

        private void SpawnVisual()
        {
            if (_bhSprite == null) _bhSprite = Resources.Load<Sprite>("VFX/Boss/Boss_BlackHole");
            _visual = new GameObject("BossBlackHoleVisual");
            _visual.transform.position = _owner.transform.position;
            _visual.transform.localScale = new Vector3(_data.bossAttackRange * 2f, _data.bossAttackRange * 2f, 1f);
            var sr = _visual.AddComponent<SpriteRenderer>();
            sr.sprite = _bhSprite;
            sr.color = new Color(1f, 1f, 1f, 0.85f);
            sr.sortingOrder = 4;
        }

        private void Detonate()
        {
            if (_player != null
                && Vector2.Distance(_owner.transform.position, _player.position) < _data.bossAttackRange)
            {
                var ph = _player.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
            }
            VfxPool.SpawnStarBurst(_owner.transform.position);
            AudioManager.Instance?.Play(SfxType.BossDetonate);
            _state = State.Idle;
            _cooldown = _data.bossAttackCooldown;
            if (_sr != null) _sr.color = _origColor;
            if (_visual != null) { Object.Destroy(_visual); _visual = null; }
        }

        public void Cleanup() {
            if (_sr != null) _sr.color = _origColor;
            if (_visual != null) Object.Destroy(_visual);
        }
    }
}
