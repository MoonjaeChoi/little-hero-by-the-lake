using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5007 T07 D3=A — CinematicEffectsConfig 정적 cache (Resources.Load 1회 + 모든 적 공유).
    /// 137 prefab SerializeField wiring 부담 X. RuntimeInitializeOnLoadMethod 자동 부트스트랩.
    /// SO 위치: Assets/_Project/Resources/Combat/CinematicEffectsConfig.asset
    /// </summary>
    public static class CinematicEffectsConfigLoader
    {
        private static CinematicEffectsConfig _cache;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            _cache = Resources.Load<CinematicEffectsConfig>("Combat/CinematicEffectsConfig");
            if (_cache == null)
                Debug.LogWarning("[CinematicEffectsConfigLoader] Resources/Combat/CinematicEffectsConfig 누락 — 기본값 사용");
        }

        public static CinematicEffectsConfig Get()
        {
            if (_cache == null) Bootstrap();
            return _cache;
        }
    }
}
