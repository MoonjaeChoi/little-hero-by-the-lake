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

        // PIVOT v3.1 — 동시 적 cap 1,000 → 1,500 (출현 ×20 정합, +50%).
        private const int MAX_CONCURRENT_ENEMIES = 1500;

        private IEnumerator SpawnRegular(WaveConfig config, int count)
        {
            if (config.enemyPool == null || config.enemyPool.Length == 0)
                yield break;

            // PIVOT v3 — spawn ×10 (game_pivot_v2.md PIVOT v3). interval × 0.1 = ×10 빈도.
            var balance = GameManager.Instance != null ? GameManager.Instance.Balance : null;
            float spawnMul = balance != null ? Mathf.Max(0.01f, balance.spawnIntervalMul) : 1f;
            float interval = (config.duration / Mathf.Max(1, count)) * spawnMul;
            // 총 spawn 수 = count / spawnIntervalMul (1/10 → ×10)
            int totalCount = Mathf.RoundToInt(count / Mathf.Max(0.01f, spawnMul));
            for (int i = 0; i < totalCount; i++)
            {
                // PIVOT v3 Phase 2 — 동시 적 1,000 cap. 도달 시 0.5초 대기 후 재시도.
                while (_activeEnemies.Count >= MAX_CONCURRENT_ENEMIES)
                    yield return new WaitForSeconds(0.5f);

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
            // 보스 체력바 — isBoss 만 (미니보스는 제외)
            if (data.isBoss) GameEvents.RaiseBossSpawned(enemy);
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
            // 보스 체력바 — isBoss 사망 시 비활성화
            if (enemy.Data != null && enemy.Data.isBoss) GameEvents.RaiseBossDespawned();
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
