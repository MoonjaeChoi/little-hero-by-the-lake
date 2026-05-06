using UnityEngine;

namespace LittleHero
{
    public class TrailAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private ObjectPool<TrailField> _pool;
        private Transform _poolRoot;
        private Vector3 _lastDropPosition;
        private bool _hasDropped;

        private const float TrailLifetime = 2.0f;
        private const float HitInterval = 0.4f;

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;
            if (config.projectilePrefab == null) return;

            // Reuse Projectile prefab visual; ensure TrailField component exists
            _poolRoot = new GameObject("TrailPool").transform;
            TrailField template = CreateTemplate(config.projectilePrefab, _poolRoot);
            if (template == null) return;

            _pool = new ObjectPool<TrailField>(template, _poolRoot, 8);
        }

        public void Tick(float deltaTime)
        {
            if (_pool == null || _owner == null) return;

            float minDist = Mathf.Max(0.4f, _config.range * 0.5f);
            if (!_hasDropped || Vector2.Distance(_lastDropPosition, _owner.position) >= minDist)
            {
                Drop(_owner.position);
                _lastDropPosition = _owner.position;
                _hasDropped = true;
            }
        }

        public void Cleanup()
        {
            if (_poolRoot != null) Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
            _pool = null;
            _hasDropped = false;
        }

        private void Drop(Vector3 pos)
        {
            TrailField field = _pool.Get();
            field.transform.position = pos;
            float r = Mathf.Max(0.1f, _config.range);
            field.transform.localScale = new Vector3(r, r, 1f);
            field.Activate(_config.damage, TrailLifetime, HitInterval, r, ReturnField, _config.element);
        }

        private void ReturnField(TrailField field) { _pool?.Return(field); }

        private static TrailField CreateTemplate(GameObject src, Transform parent)
        {
            GameObject clone = Object.Instantiate(src, parent);
            clone.name = "TrailField_Template";

            Projectile p = clone.GetComponent<Projectile>();
            if (p != null) p.enabled = false;
            Rigidbody2D rb = clone.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
            Collider2D col = clone.GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            TrailField field = clone.GetComponent<TrailField>() ?? clone.AddComponent<TrailField>();
            clone.SetActive(false);
            return field;
        }
    }
}
