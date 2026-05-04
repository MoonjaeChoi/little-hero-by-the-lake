using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 데미지 부동 텍스트 정적 풀 (VfxPool 패턴 답습).
    /// 5007 T10 D5=B — INITIAL_SIZE 50 → 300 (cap 300 환경 GC 0 보장).
    /// CinematicEffectsConfig.floaterPoolSize 또는 RuntimeInitializeOnLoadMethod 자동 prewarm.
    /// cinematic_combat_v1.md §4.5
    /// </summary>
    public static class DamageFloaterPool
    {
        private const int INITIAL_SIZE = 300;  // 5007 T10 — 50 → 300 (D5=B)
        private static readonly List<DamageNumberFloater> _pool = new List<DamageNumberFloater>(INITIAL_SIZE);
        private static Transform _root;
        private static int _peakActiveCount;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            // 5007 T10 — Resources/Combat/CinematicEffectsConfig.floaterPoolSize 우선 (없으면 default 300)
            var cfg = CinematicEffectsConfigLoader.Get();
            int target = cfg != null && cfg.floaterPoolSize > 0 ? cfg.floaterPoolSize : INITIAL_SIZE;
            Prewarm(target);
        }

        /// <summary>5007 T10 — N개 사전 할당 (GC 폭증 방지).</summary>
        public static void Prewarm(int count)
        {
            EnsureRoot();
            while (_pool.Count < count)
            {
                var go = new GameObject("DamageFloater");
                go.transform.SetParent(_root);
                var floater = go.AddComponent<DamageNumberFloater>();
                floater.EnsureSetup();
                go.SetActive(false);
                _pool.Add(floater);
            }
        }

        public static void Spawn(Vector3 worldPos, int damage, DamageType type, float sizeMul = 1f)
        {
            EnsureRoot();
            var floater = Acquire();
            if (floater == null) return;
            floater.gameObject.SetActive(true);
            floater.Initialize(damage, type, worldPos, sizeMul);

            // 5007 T10 — peak monitoring (80% 초과 시 warning)
            int active = 0;
            for (int i = 0; i < _pool.Count; i++)
                if (_pool[i] != null && _pool[i].gameObject.activeSelf) active++;
            if (active > _peakActiveCount) _peakActiveCount = active;
            if (active > _pool.Count * 0.8f)
                Debug.LogWarning($"[DamageFloaterPool] active={active}/{_pool.Count} (80% 초과 — Prewarm 증대 권장)");
        }

        public static void Return(DamageNumberFloater floater)
        {
            if (floater == null) return;
            floater.gameObject.SetActive(false);
        }

        private static DamageNumberFloater Acquire()
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                if (_pool[i] != null && !_pool[i].gameObject.activeSelf) return _pool[i];
            }
            // 신규 생성
            var go = new GameObject("DamageFloater");
            go.transform.SetParent(_root);
            var floater = go.AddComponent<DamageNumberFloater>();
            floater.EnsureSetup();
            go.SetActive(false);
            _pool.Add(floater);
            return floater;
        }

        private static void EnsureRoot()
        {
            if (_root != null) return;
            var go = new GameObject("[DamageFloaterPool]");
            Object.DontDestroyOnLoad(go);
            _root = go.transform;
        }
    }
}
