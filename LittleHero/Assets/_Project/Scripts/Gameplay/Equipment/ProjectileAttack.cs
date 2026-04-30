using UnityEngine;

namespace LittleHero
{
    public class ProjectileAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private ObjectPool<Projectile> _pool;
        private Transform _poolRoot;
        private float _timer;

        // Physics2D 오버랩 버퍼 — 매 호출마다 new 하지 않도록 재사용
        private readonly Collider2D[] _overlapBuffer = new Collider2D[64];

        public void Initialize(AttackPatternConfig config, Transform owner)
        {
            _config = config;
            _owner = owner;

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
            Debug.Log("[ProjectileAttack] 초기화 성공. 풀 생성 완료.");
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < _config.attackInterval) return;

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
            int count = Mathf.Max(1, _config.count);

            for (int i = 0; i < count; i++)
            {
                Projectile proj = _pool.Get();
                proj.transform.position = _owner.position;
                proj.Launch(dir, _config.damage, _config.projectileSpeed, _config.range, ReturnProjectile);
            }
        }

        private void ReturnProjectile(Projectile proj)
        {
            _pool?.Return(proj);
        }

        private EnemyController FindNearestEnemy()
        {
            int count = Physics2D.OverlapCircleNonAlloc(_owner.position, _config.range, _overlapBuffer);
            EnemyController nearest = null;
            float minDist = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                EnemyController e = _overlapBuffer[i].GetComponent<EnemyController>();
                if (e == null || e.IsDead) continue;

                float dist = Vector2.Distance(_owner.position, e.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = e;
                }
            }
            return nearest;
        }
    }
}
