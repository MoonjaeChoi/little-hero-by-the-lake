using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class AuraAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private float _timer;
        private GameObject _visual;

        // Damage v2 — DamageQuery 통일 (LayerMask Enemy)
        private readonly List<EnemyController> _targets = new List<EnemyController>(64);

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;

            if (config.projectilePrefab != null)
            {
                _visual = Object.Instantiate(config.projectilePrefab, owner);
                _visual.transform.localPosition = Vector3.zero;
                Projectile p = _visual.GetComponent<Projectile>();
                if (p != null) p.enabled = false;
                Rigidbody2D rb = _visual.GetComponent<Rigidbody2D>();
                if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
                Collider2D col = _visual.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                float r = Mathf.Max(0.1f, _config.range);
                _visual.transform.localScale = new Vector3(r, r, 1f);
            }
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < AttackPatternUtil.EffectiveInterval(_config.attackInterval, _stats)) return;
            _timer = 0f;
            DealAuraDamage();
        }

        public void Cleanup()
        {
            if (_visual != null) Object.Destroy(_visual);
            _visual = null;
        }

        private void DealAuraDamage()
        {
            if (_owner == null) return;
            // Damage v2 — Aura 는 sustained DPS, 모든 적 100% 데미지 (penalty X)
            DamageQuery.FindEnemiesInCircle(_owner.position, _config.range, _targets);
            for (int i = 0; i < _targets.Count; i++)
            {
                DamageDealer.ApplySingle(_targets[i], _config.damage, null, _config.element);
            }
        }
    }
}
