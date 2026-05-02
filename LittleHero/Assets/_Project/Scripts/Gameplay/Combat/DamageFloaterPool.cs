using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 데미지 부동 텍스트 정적 풀 (VfxPool 패턴 답습).
    /// 풀 크기 50 — cap 300 × 동시 피격 ~10% × buffer 1.6.
    /// cinematic_combat_v1.md §4.5
    /// </summary>
    public static class DamageFloaterPool
    {
        private const int INITIAL_SIZE = 50;
        private static readonly List<DamageNumberFloater> _pool = new List<DamageNumberFloater>(INITIAL_SIZE);
        private static Transform _root;

        public static void Spawn(Vector3 worldPos, int damage, DamageType type)
        {
            EnsureRoot();
            var floater = Acquire();
            if (floater == null) return;
            floater.gameObject.SetActive(true);
            floater.Initialize(damage, type, worldPos);
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
