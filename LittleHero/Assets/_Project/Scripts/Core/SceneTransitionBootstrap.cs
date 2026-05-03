using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5004 — FadeOverlay prefab 자동 인스턴스화 (RuntimeInitializeOnLoadMethod).
    /// Resources/UI/FadeOverlay.prefab 1회 로드 + DontDestroyOnLoad (FadeOverlay.Awake 가 처리).
    /// prefab 미존재 시 NoOp fallback (LogWarning + 정상 동작 — fade 없음).
    /// </summary>
    public static class SceneTransitionBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            var prefab = Resources.Load<GameObject>("UI/FadeOverlay");
            if (prefab == null)
            {
                Debug.LogWarning("[SceneTransitionBootstrap] Resources/UI/FadeOverlay.prefab 미존재 — fade 없이 동작 (NoOp default)");
                return;
            }
            Object.Instantiate(prefab);  // FadeOverlay.Awake 가 DontDestroyOnLoad + Default 설정
        }
    }
}
