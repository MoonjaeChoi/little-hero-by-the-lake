using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Daily 보스 공격 — 회전 영역 (boss_attack_v1.md §3 사이클 3).
    /// 1s telegraph → boss 주변 5s 회전 영역 (반경 = bossAttackRange).
    /// 영역 진입 시 0.5s 주기 데미지.
    /// </summary>
    public class BossTornadoAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 1f;
        private const float TORNADO_DURATION = 5f;
        private const float DMG_INTERVAL = 0.5f;

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _player;
        private SpriteRenderer _sr;
        private Color _origColor;

        private enum State { Idle, Telegraph, Active }
        private State _state = State.Idle;
        private float _stateTimer, _cooldown, _dmgTimer;
        private GameObject _visual;
        private static Sprite _tornadoSprite;

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
                    }
                    break;
                case State.Telegraph:
                    _stateTimer -= dt;
                    if (_sr != null)
                        _sr.color = Color.Lerp(_origColor, new Color(0.7f, 0.85f, 1f), Mathf.PingPong((TELEGRAPH_SEC - _stateTimer) * 6f, 1f));
                    if (_stateTimer <= 0f) {
                        _state = State.Active; _stateTimer = TORNADO_DURATION; _dmgTimer = 0f;
                        SpawnVisual();
                        AudioManager.Instance?.Play(SfxType.BossTornado);
                    }
                    break;
                case State.Active:
                    _stateTimer -= dt;
                    _dmgTimer -= dt;
                    // 보스 회전 시각 (transform.rotation.z spin)
                    _owner.transform.Rotate(0f, 0f, 360f * dt);
                    if (_visual != null) {
                        _visual.transform.position = _owner.transform.position;
                        _visual.transform.Rotate(0f, 0f, 720f * dt);  // 회오리 빠르게 회전
                    }
                    if (_dmgTimer <= 0f && _player != null
                        && Vector2.Distance(_owner.transform.position, _player.position) < _data.bossAttackRange)
                    {
                        var ph = _player.GetComponent<PlayerHealth>();
                        if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
                        _dmgTimer = DMG_INTERVAL;
                    }
                    if (_stateTimer <= 0f)
                    {
                        _state = State.Idle;
                        _cooldown = _data.bossAttackCooldown;
                        _owner.transform.rotation = Quaternion.identity;
                        if (_sr != null) _sr.color = _origColor;
                        if (_visual != null) { Object.Destroy(_visual); _visual = null; }
                    }
                    break;
            }
        }

        private void SpawnVisual()
        {
            if (_tornadoSprite == null) _tornadoSprite = Resources.Load<Sprite>("VFX/Boss/Boss_Tornado");
            _visual = new GameObject("BossTornadoVisual");
            _visual.transform.position = _owner.transform.position;
            _visual.transform.localScale = new Vector3(_data.bossAttackRange * 2f, _data.bossAttackRange * 2f, 1f);
            var sr = _visual.AddComponent<SpriteRenderer>();
            sr.sprite = _tornadoSprite;
            sr.color = new Color(1f, 1f, 1f, 0.7f);
            sr.sortingOrder = 4;
        }

        public void Cleanup()
        {
            if (_sr != null) _sr.color = _origColor;
            if (_owner != null) _owner.transform.rotation = Quaternion.identity;
            if (_visual != null) Object.Destroy(_visual);
        }
    }
}
