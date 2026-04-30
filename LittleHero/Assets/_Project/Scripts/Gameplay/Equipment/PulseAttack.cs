using UnityEngine;

namespace LittleHero
{
    public class PulseAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private float _timer;
        private ObjectPool<PulseVisual> _visualPool;
        private Transform _poolRoot;

        private const float PulseDuration = 0.35f;
        private readonly Collider2D[] _overlapBuffer = new Collider2D[64];

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;
            if (config.projectilePrefab != null)
            {
                _poolRoot = new GameObject("PulsePool").transform;
                PulseVisual template = CreateTemplate(config.projectilePrefab, _poolRoot);
                _visualPool = new ObjectPool<PulseVisual>(template, _poolRoot, 4);
            }
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < AttackPatternUtil.EffectiveInterval(_config.attackInterval, _stats)) return;
            _timer = 0f;
            Pulse();
        }

        public void Cleanup()
        {
            if (_poolRoot != null) Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
            _visualPool = null;
        }

        private void Pulse()
        {
            if (_owner == null) return;
            float r = Mathf.Max(0.1f, _config.range);

            int count = Physics2D.OverlapCircleNonAlloc(_owner.position, r, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                EnemyController e = _overlapBuffer[i].GetComponent<EnemyController>();
                if (e == null || e.IsDead) continue;
                DamageSystem.DealDamage(e, _config.damage);
            }

            if (_visualPool != null)
            {
                PulseVisual v = _visualPool.Get();
                v.transform.position = _owner.position;
                v.Play(r * 2f, PulseDuration, ReturnVisual);
            }
        }

        private void ReturnVisual(PulseVisual v) { _visualPool?.Return(v); }

        private static PulseVisual CreateTemplate(GameObject src, Transform parent)
        {
            GameObject clone = Object.Instantiate(src, parent);
            clone.name = "PulseVisual_Template";
            Projectile p = clone.GetComponent<Projectile>();
            if (p != null) p.enabled = false;
            Rigidbody2D rb = clone.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
            Collider2D col = clone.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
            PulseVisual v = clone.GetComponent<PulseVisual>() ?? clone.AddComponent<PulseVisual>();
            clone.SetActive(false);
            return v;
        }
    }
}
