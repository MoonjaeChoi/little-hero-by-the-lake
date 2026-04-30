using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class SummonAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private ObjectPool<Projectile> _projectilePool;
        private Transform _projRoot;
        private readonly List<Transform> _companions = new List<Transform>();
        private readonly List<float> _timers = new List<float>();
        private readonly Collider2D[] _overlapBuffer = new Collider2D[64];

        private const float CompanionRadius = 1.4f;
        private const float CompanionLag = 4f;

        public void Initialize(AttackPatternConfig config, Transform owner)
        {
            _config = config;
            _owner = owner;
            if (config.projectilePrefab == null) return;

            Projectile p = config.projectilePrefab.GetComponent<Projectile>();
            if (p == null) return;

            _projRoot = new GameObject("ProjectilePool_Summon").transform;
            _projectilePool = new ObjectPool<Projectile>(p, _projRoot, 12);

            int count = Mathf.Max(1, config.count);
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
                if (_timers[i] < _config.attackInterval) continue;
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
            proj.Launch(dir, _config.damage, _config.projectileSpeed, _config.range, ReturnProjectile);
        }

        private void ReturnProjectile(Projectile p) { _projectilePool?.Return(p); }

        private EnemyController FindNearestEnemy(Vector3 origin)
        {
            int n = Physics2D.OverlapCircleNonAlloc(origin, _config.range, _overlapBuffer);
            EnemyController nearest = null;
            float minDist = float.MaxValue;
            for (int i = 0; i < n; i++)
            {
                EnemyController e = _overlapBuffer[i].GetComponent<EnemyController>();
                if (e == null || e.IsDead) continue;
                float d = Vector2.Distance(origin, e.transform.position);
                if (d < minDist) { minDist = d; nearest = e; }
            }
            return nearest;
        }
    }
}
