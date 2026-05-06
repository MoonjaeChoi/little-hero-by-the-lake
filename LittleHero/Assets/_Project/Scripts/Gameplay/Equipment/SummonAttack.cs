using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class SummonAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private ObjectPool<Projectile> _projectilePool;
        private Transform _projRoot;
        private readonly List<Transform> _companions = new List<Transform>();
        private readonly List<float> _timers = new List<float>();
        // Damage v2 — DamageQuery.FindNearest 로 통일 (LayerMask Enemy)

        private const float CompanionRadius = 1.4f;
        private const float CompanionLag = 4f;

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;
            if (config.projectilePrefab == null) return;

            Projectile p = config.projectilePrefab.GetComponent<Projectile>();
            if (p == null) return;

            _projRoot = new GameObject("ProjectilePool_Summon").transform;
            _projectilePool = new ObjectPool<Projectile>(p, _projRoot, 12);

            int count = AttackPatternUtil.EffectiveCount(config.count, _stats);
            for (int i = 0; i < count; i++)
            {
                GameObject clone = Object.Instantiate(config.projectilePrefab);
                clone.name = $"Companion_{i}";

                Projectile pp = clone.GetComponent<Projectile>();
                if (pp != null) pp.enabled = false;
                Rigidbody2D rb = clone.GetComponent<Rigidbody2D>();
                if (rb != null) { rb.linearVelocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }
                Collider2D col = clone.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;

                float a = i * Mathf.PI * 2f / count;
                clone.transform.position = owner.position + new Vector3(Mathf.Cos(a) * CompanionRadius, Mathf.Sin(a) * CompanionRadius, 0f);
                clone.transform.localScale *= 0.7f;

                _companions.Add(clone.transform);
                _timers.Add(0f);
            }
        }

        public void Tick(float deltaTime)
        {
            if (_owner == null) return;

            for (int i = 0; i < _companions.Count; i++)
            {
                Transform c = _companions[i];
                if (c == null) continue;

                float a = i * Mathf.PI * 2f / _companions.Count;
                Vector3 desired = _owner.position + new Vector3(Mathf.Cos(a) * CompanionRadius, Mathf.Sin(a) * CompanionRadius, 0f);
                c.position = Vector3.Lerp(c.position, desired, CompanionLag * deltaTime);

                _timers[i] += deltaTime;
                if (_timers[i] < AttackPatternUtil.EffectiveInterval(_config.attackInterval, _stats)) continue;
                _timers[i] = 0f;
                FireFrom(c.position);
            }
        }

        public void Cleanup()
        {
            for (int i = 0; i < _companions.Count; i++)
                if (_companions[i] != null) Object.Destroy(_companions[i].gameObject);
            _companions.Clear();
            _timers.Clear();
            if (_projRoot != null) Object.Destroy(_projRoot.gameObject);
            _projRoot = null;
            _projectilePool = null;
        }

        private void FireFrom(Vector3 origin)
        {
            EnemyController target = FindNearestEnemy(origin);
            if (target == null || _projectilePool == null) return;

            Vector2 dir = ((Vector2)target.transform.position - (Vector2)origin).normalized;
            Projectile proj = _projectilePool.Get();
            proj.transform.position = origin;
            proj.Launch(dir, _config.damage, _config.projectileSpeed, _config.range, ReturnProjectile, _config.element);
        }

        private void ReturnProjectile(Projectile p) { _projectilePool?.Return(p); }

        private EnemyController FindNearestEnemy(Vector3 origin)
        {
            return DamageQuery.FindNearest(origin, _config.range);
        }
    }
}
