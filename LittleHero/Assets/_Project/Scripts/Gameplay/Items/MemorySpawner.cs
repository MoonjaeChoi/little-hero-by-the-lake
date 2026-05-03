using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class MemorySpawner : MonoBehaviour
    {
        [SerializeField] private MemoryPickup _pickupPrefab;
        [SerializeField] private float _spawnRadius = 5f;
        [SerializeField] private int _initialPoolSize = 4;

        private ObjectPool<MemoryPickup> _pool;
        private Transform _poolRoot;
        private readonly HashSet<string> _spawnedThisRun = new HashSet<string>();

        private void Awake()
        {
            if (_pickupPrefab == null) return;
            _poolRoot = new GameObject("MemoryPickupPool").transform;
            _pool = new ObjectPool<MemoryPickup>(_pickupPrefab, _poolRoot, _initialPoolSize);
        }

        private void OnEnable()
        {
            GameEvents.OnWaveCleared += HandleWaveCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveCleared -= HandleWaveCleared;
        }

        private void OnDestroy()
        {
            if (_poolRoot != null) Destroy(_poolRoot.gameObject);
        }

        // 웨이브 클리어 시점에 같은 챕터 메모리 중 미발견 항목을 확률 롤로 spawn
        private void HandleWaveCleared(int wave)
        {
            if (_pool == null) return;
            if (MetaProgression.Instance == null || MetaProgression.Instance.MemoryPool == null) return;
            if (MetaProgression.Instance.SelectedChapter == null) return;

            string chapterId = MetaProgression.Instance.SelectedChapter.id;
            var memories = MetaProgression.Instance.MemoryPool.memories;
            if (memories == null) return;

            for (int i = 0; i < memories.Length; i++)
            {
                MemoryData m = memories[i];
                if (m == null) continue;
                if (m.chapterId != chapterId) continue;
                if (MetaProgression.Instance.IsMemoryDiscovered(m.id)) continue;
                if (_spawnedThisRun.Contains(m.id)) continue;
                if (Random.value > m.spawnChance) continue;

                Spawn(m);
                _spawnedThisRun.Add(m.id);
            }
        }

        private void Spawn(MemoryData data)
        {
            var pt = PlayerFinder.Get();
            Vector3 origin = pt != null ? pt.position : transform.position;
            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector3 pos = origin + new Vector3(Mathf.Cos(angle) * _spawnRadius, Mathf.Sin(angle) * _spawnRadius, 0f);

            MemoryPickup pickup = _pool.Get();
            pickup.transform.position = pos;
            pickup.Initialize(data);
        }
    }
}
