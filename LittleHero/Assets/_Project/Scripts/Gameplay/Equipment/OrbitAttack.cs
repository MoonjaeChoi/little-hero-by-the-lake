using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class OrbitAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private float _angle;
        private readonly List<Transform> _orbs = new List<Transform>();

        public void Initialize(AttackPatternConfig config, Transform owner)
        {
            _config = config;
            _owner = owner;
            if (config.projectilePrefab == null)
            {
                Debug.LogError("[OrbitAttack] projectilePrefab 이 null 입니다.");
                return;
            }

            int count = Mathf.Max(1, config.count);
            for (int i = 0; i < count; i++)
            {
                GameObject go = Object.Instantiate(config.projectilePrefab, owner);
                go.transform.localPosition = Vector3.zero;

                Projectile p = go.GetComponent<Projectile>();
                if (p != null) p.enabled = false;
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.bodyType = RigidbodyType2D.Kinematic;
                }
                OrbitOrb orb = go.GetComponent<OrbitOrb>() ?? go.AddComponent<OrbitOrb>();
                orb.Configure(_config.damage);

                _orbs.Add(go.transform);
            }
        }

        public void Tick(float deltaTime)
        {
            if (_orbs.Count == 0 || _owner == null) return;
            _angle += _config.projectileSpeed * deltaTime;
            float radius = Mathf.Max(0.1f, _config.range);
            int n = _orbs.Count;
            for (int i = 0; i < n; i++)
            {
                float a = _angle + i * Mathf.PI * 2f / n;
                _orbs[i].localPosition = new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
            }
        }

        public void Cleanup()
        {
            for (int i = 0; i < _orbs.Count; i++)
                if (_orbs[i] != null) Object.Destroy(_orbs[i].gameObject);
            _orbs.Clear();
        }
    }
}
