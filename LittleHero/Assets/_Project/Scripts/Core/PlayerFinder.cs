using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// CLAUDE.md §15.3 GC 최소화 — GameObject.Find 다회 호출 폐기.
    /// 정적 cache + 씬 전환 시 자동 invalidate.
    /// </summary>
    public static class PlayerFinder
    {
        private static Transform _cached;
        private static bool _sceneHookInstalled;

        public static Transform Get()
        {
            EnsureSceneHook();
            if (_cached != null) return _cached;
            var go = GameObject.FindWithTag("Player");
            if (go != null) _cached = go.transform;
            return _cached;
        }

        public static void Invalidate() => _cached = null;

        private static void EnsureSceneHook()
        {
            if (_sceneHookInstalled) return;
            SceneManager.sceneLoaded += (_, __) => Invalidate();
            SceneManager.sceneUnloaded += _ => Invalidate();
            _sceneHookInstalled = true;
        }
    }
}
