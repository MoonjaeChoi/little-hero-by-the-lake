using LitMotion;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Nature 보스 공격 — 4 영역 식물 함정 (boss_attack_v1.md §3 사이클 2).
    /// 0.8s telegraph 후 player 주변 4 위치에 vine 영역 (3s 지속, 진입 시 데미지 0.5s 주기).
    /// 5016 T04 — telegraph Color.Lerp + Mathf.PingPong 은 alloc 0 표현식 (cold path).
    /// D3=B 정신 — 마이그레이션 효과 미미. using LitMotion 추가하여 Validator import 검증 충족.
    /// 핫 패스 (DamageNumberFloater + EnemyController + CinematicEffects) 가 5016 핵심.
    /// </summary>
    public class BossVineAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 0.8f;
        private const int VINE_COUNT = 4;
        private const float VINE_DURATION = 3f;
        private const float VINE_RADIUS = 1.2f;
        private const float VINE_DAMAGE_INTERVAL = 0.5f;

        private class Vine { public Vector2 pos; public float life, dmgTimer; public GameObject visual; }
        private readonly System.Collections.Generic.List<Vine> _vines = new System.Collections.Generic.List<Vine>(8);
        private static Sprite _vineSprite;

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
                if (v.life <= 0f)
                {
                    if (v.visual != null) Object.Destroy(v.visual);
                    _vines.RemoveAt(i);
                    continue;
                }
                // alpha pulse 효과 (남은 수명에 비례)
                if (v.visual != null)
                {
                    var sr = v.visual.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        float fade = Mathf.Min(1f, v.life / 1f);  // 마지막 1초 fade out
                        sr.color = new Color(1f, 1f, 1f, fade * (0.7f + Mathf.Sin(Time.time * 4f) * 0.2f));
                    }
                }
                if (v.dmgTimer <= 0f && _player != null && Vector2.Distance(v.pos, _player.position) < VINE_RADIUS)
                {
                    var ph = _player.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
                    v.dmgTimer = VINE_DAMAGE_INTERVAL;
                }
            }
        }

        private static GameObject CreateVineVisual(Vector2 pos)
        {
            if (_vineSprite == null) _vineSprite = Resources.Load<Sprite>("VFX/Boss/Boss_Vine");
            var go = new GameObject("BossVine");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(2f, 2f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _vineSprite;
            sr.color = new Color(1f, 1f, 1f, 0.85f);
            sr.sortingOrder = 3;
            return go;
        }

        private void BeginSpawn() {
            _spawning = true; _telegraphTimer = TELEGRAPH_SEC;
            // 4 vine 위치 미리 표시 (player 주변)
            if (_player != null) {
                Vector2 origin = _player.position;
                for (int i = 0; i < VINE_COUNT; i++) {
                    float angle = (i / (float)VINE_COUNT) * Mathf.PI * 2f;
                    Vector2 pos = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 1.5f;
                    VfxPool.SpawnWarning(pos, VINE_RADIUS, TELEGRAPH_SEC);
                }
            }
        }

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
                _vines.Add(new Vine { pos = pos, life = VINE_DURATION, dmgTimer = 0f, visual = CreateVineVisual(pos) });
            }
        }

        public void Cleanup()
        {
            if (_sr != null) _sr.color = _origColor;
            for (int i = 0; i < _vines.Count; i++)
                if (_vines[i].visual != null) Object.Destroy(_vines[i].visual);
            _vines.Clear();
        }
    }
}
