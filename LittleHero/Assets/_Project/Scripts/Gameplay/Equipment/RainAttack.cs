using UnityEngine;

namespace LittleHero
{
    public class RainAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private ObjectPool<Projectile> _pool;
        private Transform _poolRoot;
        private float _timer;

        private const float DropHeight = 6f;

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;
            if (config.projectilePrefab == null) return;

            Projectile p = config.projectilePrefab.GetComponent<Projectile>();
            if (p == null) return;

            _poolRoot = new GameObject("ProjectilePool_Rain").transform;
            _pool = new ObjectPool<Projectile>(p, _poolRoot, 16);
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < AttackPatternUtil.EffectiveInterval(_config.attackInterval, _stats)) return;
            _timer = 0f;
            DropAll();
        }

        public void Cleanup()
        {
            if (_poolRoot != null) Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
            _pool = null;
        }

        private void DropAll()
        {
            if (_pool == null || _owner == null) return;
            int count = AttackPatternUtil.EffectiveCount(_config.count, _stats);
            float r = Mathf.Max(0.1f, _config.range);
            float speed = Mathf.Max(0.1f, _config.projectileSpeed);

            for (int i = 0; i < count; i++)
            {
                Vector2 offset = Random.insideUnitCircle * r;
                Vector3 spawn = _owner.position + new Vector3(offset.x, DropHeight, 0f);
                Projectile proj = _pool.Get();
                proj.transform.position = spawn;
                proj.Launch(Vector2.down, _config.damage, speed, DropHeight + 1f, ReturnProjectile);
            }
        }

        private void ReturnProjectile(Projectile proj) { _pool?.Return(proj); }
    }
}
