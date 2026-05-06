using UnityEngine;

namespace LittleHero
{
    public class ProjectileAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private ObjectPool<Projectile> _pool;
        private Transform _poolRoot;
        private float _timer;

        // Physics2D 오버랩 버퍼 — 매 호출마다 new 하지 않도록 재사용
        // 5032 — _overlapBuffer 폐기 (DamageQuery.FindNearest 위임, GC 0 통합)

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;

            if (config.projectilePrefab == null)
            {
                Debug.LogError("[ProjectileAttack] projectilePrefab 이 null 입니다. EquipmentData의 Attack Config → Projectile Prefab 을 설정하세요.");
                return;
            }

            Projectile prefabComp = config.projectilePrefab.GetComponent<Projectile>();
            if (prefabComp == null)
            {
                Debug.LogError("[ProjectileAttack] Projectile 컴포넌트가 프리팹에 없습니다.");
                return;
            }

            _poolRoot = new GameObject("ProjectilePool_Projectile").transform;
            _pool = new ObjectPool<Projectile>(prefabComp, _poolRoot, 20);
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < AttackPatternUtil.EffectiveInterval(_config.attackInterval, _stats)) return;

            _timer = 0f;
            Fire();
        }

        public void Cleanup()
        {
            if (_poolRoot != null) Object.Destroy(_poolRoot.gameObject);
            _poolRoot = null;
            _pool = null;
        }

        private void Fire()
        {
            if (_pool == null) return;

            EnemyController target = FindNearestEnemy();
            if (target == null) return;

            Vector2 dir = ((Vector2)target.transform.position - (Vector2)_owner.position).normalized;
            int count = AttackPatternUtil.EffectiveCount(_config.count, _stats);

            for (int i = 0; i < count; i++)
            {
                Projectile proj = _pool.Get();
                proj.transform.position = _owner.position;
                proj.Launch(dir, _config.damage, _config.projectileSpeed, _config.range, ReturnProjectile, _config.element);
            }
        }

        private void ReturnProjectile(Projectile proj)
        {
            _pool?.Return(proj);
        }

        // 5032 — DamageQuery.FindNearest 위임 (v6 OverlapCircle + ContactFilter2D 정적 + EnemyMask)
        private EnemyController FindNearestEnemy()
            => DamageQuery.FindNearest(_owner.position, _config.range);
    }
}
