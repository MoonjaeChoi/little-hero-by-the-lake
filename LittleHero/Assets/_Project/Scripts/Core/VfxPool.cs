using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v3 Phase 2 — VFX 오브젝트 풀 (game_pivot_v2.md GC critical 해결).
    ///
    /// EnemyController.Die 매 처치 시 Instantiate(StarBurstFx) 폐기 →
    /// VfxPool.SpawnStarBurst(pos) 가 풀에서 재사용. 10k 처치 시 Instantiate 99% 감소.
    ///
    /// VFX 오브젝트는 SimpleFx (lifetime 자동 비활성) 가 0.6s 후 SetActive(false).
    /// VfxPool 이 다음 호출 시 비활성 오브젝트 재발견 + reuse.
    /// </summary>
    public static class VfxPool
    {
        private const string STAR_BURST_PATH = "VFX/StarBurstFx";
        private const int INITIAL_SIZE = 32;

        private static GameObject _starBurstPrefab;
        private static readonly List<GameObject> _starBurstPool = new List<GameObject>(INITIAL_SIZE);
        private static Transform _root;

        public static void SpawnStarBurst(Vector3 position)
        {
            EnsurePrefab();
            EnsureRoot();
            var go = AcquireStarBurst();
            if (go == null) return;
            go.transform.position = position;
            go.transform.rotation = Quaternion.identity;
            go.SetActive(true);
        }

        private static GameObject AcquireStarBurst()
        {
            // 풀에서 비활성 오브젝트 검색 (SimpleFx 가 0.6s 후 SetActive(false))
            for (int i = 0; i < _starBurstPool.Count; i++)
            {
                if (_starBurstPool[i] != null && !_starBurstPool[i].activeSelf)
                    return _starBurstPool[i];
            }
            // 모두 활성 — 신규 Instantiate
            if (_starBurstPrefab == null) return null;
            var go = Object.Instantiate(_starBurstPrefab, _root);
            _starBurstPool.Add(go);
            return go;
        }

        private static void EnsurePrefab()
        {
            if (_starBurstPrefab != null) return;
            _starBurstPrefab = Resources.Load<GameObject>(STAR_BURST_PATH);
            if (_starBurstPrefab == null)
                Debug.LogWarning("[VfxPool] " + STAR_BURST_PATH + " 미존재 — VFX 풀링 스킵");
        }

        private static void EnsureRoot()
        {
            if (_root != null) return;
            var go = new GameObject("[VfxPool]");
            Object.DontDestroyOnLoad(go);
            _root = go.transform;
        }
    }
}
