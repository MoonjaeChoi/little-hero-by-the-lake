using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// Fantasy 보스 공격 — 5발 부채꼴 탄막 (boss_attack_v1.md §3 사이클 2).
    /// 0.5s telegraph 후 player 방향 ±30도 부채꼴 5발 발사.
    /// projectile = 단순 OverlapCircle 데미지 영역 (지속 1.5s, 속도 8).
    /// </summary>
    public class BossSpreadAttack : IBossAttackPattern
    {
        private const float TELEGRAPH_SEC = 0.5f;
        private const int SHOT_COUNT = 5;
        private const float SPREAD_DEG = 30f;
        private const float PROJ_SPEED = 8f;
        private const float PROJ_LIFETIME = 1.5f;
        private const float PROJ_RADIUS = 0.4f;

        private EnemyController _owner;
        private EnemyData _data;
        private Transform _player;
        private SpriteRenderer _sr;
        private Color _origColor;
        private float _cooldown;
        private float _telegraphTimer;
        private bool _firing;

        // 활성 탄 추적 + 시각 GameObject
        private class Projectile { public Vector2 pos, dir; public float life; public bool hit; public GameObject visual; }
        private readonly System.Collections.Generic.List<Projectile> _projectiles = new System.Collections.Generic.List<Projectile>(32);
        private static Sprite _projectileSprite;

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

            if (_firing)
            {
                _telegraphTimer -= dt;
                if (_sr != null)
                    _sr.color = Color.Lerp(_origColor, new Color(0.85f, 0.4f, 1f), Mathf.PingPong((TELEGRAPH_SEC - _telegraphTimer) * 6f, 1f));
                if (_telegraphTimer <= 0f) FireSpread();
            }
            else
            {
                _cooldown -= dt;
                if (_cooldown <= 0f) BeginFire();
            }

            // projectile update
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var p = _projectiles[i];
                p.pos += p.dir * PROJ_SPEED * dt;
                p.life -= dt;
                if (p.visual != null) p.visual.transform.position = p.pos;
                if (!p.hit && _player != null && Vector2.Distance(p.pos, _player.position) < PROJ_RADIUS)
                {
                    var ph = _player.GetComponent<PlayerHealth>();
                    if (ph != null) ph.TakeDamage(_data.bossAttackDamage);
                    p.hit = true;
                }
                if (p.life <= 0f)
                {
                    if (p.visual != null) Object.Destroy(p.visual);
                    _projectiles.RemoveAt(i);
                }
            }
        }

        private static GameObject CreateProjectileVisual(Vector2 pos)
        {
            if (_projectileSprite == null)
            {
                // 기존 별 스프라이트 재활용 — 없으면 흰색 점
                var t = Resources.Load<Sprite>("VFX/Boss/Boss_BlackHole");
                _projectileSprite = t;
            }
            var go = new GameObject("BossProjectile");
            go.transform.position = pos;
            go.transform.localScale = new Vector3(0.4f, 0.4f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _projectileSprite;
            sr.color = new Color(0.85f, 0.4f, 1f, 0.9f);
            sr.sortingOrder = 5;
            return go;
        }

        private void BeginFire()
        {
            _firing = true;
            _telegraphTimer = TELEGRAPH_SEC;
        }

        private void FireSpread()
        {
            _firing = false;
            _cooldown = _data.bossAttackCooldown;
            if (_sr != null) _sr.color = _origColor;
            if (_player == null) return;

            Vector2 toPlayer = ((Vector2)(_player.position - _owner.transform.position)).normalized;
            float baseAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            float halfSpread = SPREAD_DEG;
            float step = (SHOT_COUNT > 1) ? (halfSpread * 2f / (SHOT_COUNT - 1)) : 0f;
            for (int i = 0; i < SHOT_COUNT; i++)
            {
                float a = (baseAngle - halfSpread + step * i) * Mathf.Deg2Rad;
                Vector2 pos0 = _owner.transform.position;
                _projectiles.Add(new Projectile
                {
                    pos = pos0,
                    dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a)),
                    life = PROJ_LIFETIME,
                    hit = false,
                    visual = CreateProjectileVisual(pos0),
                });
            }
        }

        public void Cleanup()
        {
            if (_sr != null) _sr.color = _origColor;
            for (int i = 0; i < _projectiles.Count; i++)
                if (_projectiles[i].visual != null) Object.Destroy(_projectiles[i].visual);
            _projectiles.Clear();
        }
    }
}
