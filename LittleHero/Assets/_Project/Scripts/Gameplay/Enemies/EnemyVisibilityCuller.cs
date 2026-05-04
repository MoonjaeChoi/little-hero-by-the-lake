using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// 5013 — Off-screen 적 visibility 판정 정적 헬퍼.
    /// PIVOT v4 §7 cap 300 환경 60fps 마진 강화. PlayerFinder 패턴 답습 (Camera cache).
    /// SceneManager.sceneLoaded → InvalidateCache 자동 hook (Unity 표준 API).
    /// </summary>
    public static class EnemyVisibilityCuller
    {
        private const float DEFAULT_MARGIN = 1.5f;
        private static Camera _cachedCam;

        public static bool IsVisible(Vector3 worldPos, float margin = DEFAULT_MARGIN)
        {
            var cam = GetCamera();
            if (cam == null) return true;
            var vp = cam.WorldToViewportPoint(worldPos);
            float lo = (1f - margin) * 0.5f;
            float hi = 1f - lo;
            return vp.z > 0 && vp.x >= lo && vp.x <= hi && vp.y >= lo && vp.y <= hi;
        }

        public static void InvalidateCache() => _cachedCam = null;

        private static Camera GetCamera()
        {
            if (_cachedCam == null) _cachedCam = Camera.main;
            return _cachedCam;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => InvalidateCache();
    }
}
