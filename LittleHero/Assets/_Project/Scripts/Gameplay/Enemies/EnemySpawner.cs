using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private float _spawnRadius = 10f;
        [SerializeField] private int _poolSizePerType = 20;

        private readonly Dictionary<string, ObjectPool<EnemyController>> _pools
            = new Dictionary<string, ObjectPool<EnemyController>>();

        private Transform _poolRoot;

        private void Awake()
        {
            _poolRoot = new GameObject("EnemyPool").transform;
            _poolRoot.SetParent(transform);
        }

        public void RegisterEnemy(EnemyData data)
        {
            if (_pools.ContainsKey(data.id)) return;
            if (data.prefab == null) return;

            EnemyController prefabController = data.prefab.GetComponent<EnemyController>();
            if (prefabController == null) return;

            var pool = new ObjectPool<EnemyController>(prefabController, _poolRoot, _poolSizePerType);
            _pools[data.id] = pool;
        }

        public EnemyController Spawn(EnemyData data)
        {
            if (!_pools.ContainsKey(data.id))
                RegisterEnemy(data);

            if (!_pools.TryGetValue(data.id, out var pool)) return null;

            EnemyController enemy = pool.Get();
            enemy.transform.position = GetSpawnPosition();
            // A4 — 현재 챕터의 enemyStatMultiplier 자동 적용 (HP/공격력)
            float multi = MetaProgression.Instance?.SelectedChapter?.enemyStatMultiplier ?? 1f;
            enemy.Initialize(data, multi);
            return enemy;
        }

        public void ReturnToPool(EnemyController enemy)
        {
            if (_pools.TryGetValue(enemy.Data.id, out var pool))
                pool.Return(enemy);
        }

        private Vector2 GetSpawnPosition()
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector3 origin = player != null ? player.transform.position : transform.position;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            return new Vector2(
                origin.x + Mathf.Cos(angle) * _spawnRadius,
                origin.y + Mathf.Sin(angle) * _spawnRadius
            );
        }
    }
}
