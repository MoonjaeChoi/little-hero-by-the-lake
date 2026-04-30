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

        private readonly Collider2D[] _overlapBuffer = new Collider2D[64];

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
            int count = Physics2D.OverlapCircleNonAlloc(_owner.position, _config.range, _overlapBuffer);
            for (int i = 0; i < count; i++)
            {
                EnemyController e = _overlapBuffer[i].GetComponent<EnemyController>();
                if (e == null || e.IsDead) continue;
                DamageSystem.DealDamage(e, _config.damage);
            }
        }
    }
}
