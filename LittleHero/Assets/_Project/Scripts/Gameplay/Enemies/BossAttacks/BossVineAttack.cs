using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Nature 보스 공격 — 4 영역 식물 함정 (boss_attack_v1.md §3 사이클 2).
    /// 0.8s telegraph 후 player 주변 4 위치에 vine 영역 (3s 지속, 진입 시 데미지 0.5s 주기).
    /// </summary>
    public class BossVineAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 0.8f;
        private const int VINE_COUNT = 4;
        private const float VINE_DURATION = 3f;
        private const float VINE_RADIUS = 1.2f;
        private const float VINE_DAMAGE_INTERVAL = 0.5f;

        private struct Vine { public Vector2 pos; public float life, dmgTimer; }
        private readonly System.Collections.Generic.List<Vine> _vines = new System.Collections.Generic.List<Vine>(8);

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _player;
        private SpriteRenderer _sr;
        private Color _origColor;
        private float _cooldown;
        private float _telegraphTimer;
        private bool _spawning;

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

            if (_spawning)
            {
                _telegraphTimer -= dt;
                if (_sr != null)
                    _sr.color = Color.Lerp(_origColor, new Color(0.3f, 0.8f, 0.3f), Mathf.PingPong((TELEGRAPH_SEC - _telegraphTimer) * 6f, 1f));
                if (_telegraphTimer <= 0f) PlaceVines();
            }
            else
            {
                _cooldown -= dt;
                if (_cooldown <= 0f) BeginSpawn();
            }

            // vine update
            for (int i = _vines.Count - 1; i >= 0; i--)
            {
                var v = _vines[i];
                v.life -= dt;
                v.dmgTimer -= dt;
                if (v.life <= 0f) { _vines.RemoveAt(i); continue; }
                if (v.dmgTimer <= 0f && _player != null && Vector2.Distance(v.pos, _player.position) < VINE_RADIUS)
                {
                    var ph = _player.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
                    v.dmgTimer = VINE_DAMAGE_INTERVAL;
                }
                _vines[i] = v;
            }
        }

        private void BeginSpawn() { _spawning = true; _telegraphTimer = TELEGRAPH_SEC; }

        private void PlaceVines()
        {
            _spawning = false;
            _cooldown = _data.bossAttackCooldown;
            if (_sr != null) _sr.color = _origColor;
            if (_player == null) return;
            Vector2 origin = _player.position;
            for (int i = 0; i < VINE_COUNT; i++)
            {
                float angle = Random.Range(0f, Mathf.PI * 2f);
                float dist = Random.Range(1f, 2f);
                Vector2 pos = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;
                _vines.Add(new Vine { pos = pos, life = VINE_DURATION, dmgTimer = 0f });
            }
        }

        public void Cleanup()
        {
            if (_sr != null) _sr.color = _origColor;
            _vines.Clear();
        }
    }
}
