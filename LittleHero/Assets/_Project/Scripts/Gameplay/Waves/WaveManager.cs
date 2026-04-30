using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public struct WaveConfig
    {
        public EnemyData[] enemyPool;
        public int spawnCount;
        public float duration;
        public EnemyData miniBoss;  // null이면 미니보스 없음
        public EnemyData boss;      // null이면 보스 없음
    }

    public class WaveManager : MonoBehaviour
    {
        private EnemySpawner _spawner;
        private readonly HashSet<EnemyController> _activeEnemies = new HashSet<EnemyController>();
        private bool _spawnDone;
        private Action _onWaveComplete;
        private Coroutine _spawnCoroutine;

        public int ActiveEnemyCount => _activeEnemies.Count;

        public void Initialize(EnemySpawner spawner)
        {
            _spawner = spawner;
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied     += ClearAll;
            GameEvents.OnChapterCleared += HandleChapterCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied     -= ClearAll;
            GameEvents.OnChapterCleared -= HandleChapterCleared;
        }

        private void HandleChapterCleared(ChapterData _) => ClearAll();

        private void ClearAll()
        {
            if (_spawnCoroutine != null) { StopCoroutine(_spawnCoroutine); _spawnCoroutine = null; }
            foreach (var e in _activeEnemies)
            {
                if (e == null) continue;
                e.OnDied -= HandleEnemyDied;
                if (_spawner != null) _spawner.ReturnToPool(e);
            }
            _activeEnemies.Clear();
            _onWaveComplete = null;
        }

        public void StartWave(WaveConfig config, Action onComplete)
        {
            _onWaveComplete = onComplete;
            _activeEnemies.Clear();
            _spawnDone = false;

            if (_spawnCoroutine != null)
                StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = StartCoroutine(SpawnRoutine(config));
        }

        private IEnumerator SpawnRoutine(WaveConfig config)
        {
            if (config.boss != null)
            {
                SpawnEnemy(config.boss);
            }
            else if (config.miniBoss != null)
            {
                SpawnEnemy(config.miniBoss);
                yield return SpawnRegular(config, config.spawnCount / 2);
            }
            else
            {
                yield return SpawnRegular(config, config.spawnCount);
            }

            _spawnDone = true;
            CheckWaveComplete();
        }

        private IEnumerator SpawnRegular(WaveConfig config, int count)
        {
            if (config.enemyPool == null || config.enemyPool.Length == 0)
                yield break;

            float interval = config.duration / Mathf.Max(1, count);
            for (int i = 0; i < count; i++)
            {
                EnemyData data = config.enemyPool[UnityEngine.Random.Range(0, config.enemyPool.Length)];
                SpawnEnemy(data);
                yield return new WaitForSeconds(interval);
            }
        }

        private const float BossSpawnRadius = 4f;

        private void SpawnEnemy(EnemyData data)
        {
            EnemyController enemy = _spawner.Spawn(data);
            if (enemy == null)
            {
                Debug.LogError($"[WaveManager] SpawnEnemy returned null for '{(data ? data.displayName : "<null>")}' (id='{(data ? data.id : "")}')");
                return;
            }
            if (data.isMiniB || data.isBoss)
                enemy.transform.position = GetBossSpawnPosition();
            _activeEnemies.Add(enemy);
            enemy.OnDied += HandleEnemyDied;
        }

        private static Vector3 GetBossSpawnPosition()
        {
            GameObject player = GameObject.FindWithTag("Player");
            Vector3 origin = player != null ? player.transform.position : Vector3.zero;
            float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            return origin + new Vector3(Mathf.Cos(angle) * BossSpawnRadius, Mathf.Sin(angle) * BossSpawnRadius, 0f);
        }

        private void HandleEnemyDied(EnemyController enemy)
        {
            enemy.OnDied -= HandleEnemyDied;
            _activeEnemies.Remove(enemy);
            _spawner.ReturnToPool(enemy);
            CheckWaveComplete();
        }

        private void CheckWaveComplete()
        {
            if (_spawnDone && _activeEnemies.Count == 0)
                _onWaveComplete?.Invoke();
        }
    }
}
