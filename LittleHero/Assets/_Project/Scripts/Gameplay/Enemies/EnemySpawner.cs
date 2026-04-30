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

        // C9 — 현재 진행 챕터의 적 스탯 곱셈을 인스펙터·SelectedChapter 어느 경로로든 일관되게 반영.
        private ChapterData _activeChapter;
        public ChapterData ActiveChapter => _activeChapter;
        public void SetActiveChapter(ChapterData chapter) => _activeChapter = chapter;

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
            // C9 — 활성 챕터(인스펙터 또는 SelectedChapter) 의 enemyStatMultiplier 자동 적용.
            //     ActiveChapter 우선, 없으면 SelectedChapter, 둘 다 없으면 1f.
            float multi = _activeChapter != null
                ? _activeChapter.enemyStatMultiplier
                : (MetaProgression.Instance?.SelectedChapter?.enemyStatMultiplier ?? 1f);
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
