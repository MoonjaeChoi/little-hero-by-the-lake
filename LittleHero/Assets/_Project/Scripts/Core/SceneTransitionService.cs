using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 모든 씬 전환 통합 service (§15.1 SRP, §15.7 DI).
    /// Default property 로 mock 주입 가능. 5002 BossEncounterLauncher 패턴 답습.
    /// </summary>
    public static class SceneTransitionService
    {
        public static ISceneTransition Default { get; set; } = new NoOpTransition();
        public const float DEFAULT_FADE_DURATION = 0.3f;

        public static void Transition(string sceneName, float fadeDuration = DEFAULT_FADE_DURATION)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            var runner = TransitionCoroutineRunner.GetOrCreate();
            runner.StartCoroutine(TransitionRoutine(sceneName, fadeDuration));
        }

        private static IEnumerator TransitionRoutine(string sceneName, float fadeDuration)
        {
            Time.timeScale = 1f;
            yield return Default.FadeOut(fadeDuration);
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null) while (!op.isDone) yield return null;
            yield return Default.FadeIn(fadeDuration);
        }

        private class NoOpTransition : ISceneTransition
        {
            public IEnumerator FadeOut(float d) { yield break; }
            public IEnumerator FadeIn(float d) { yield break; }
        }
    }

    internal class TransitionCoroutineRunner : MonoBehaviour
    {
        private static TransitionCoroutineRunner _instance;
        public static TransitionCoroutineRunner GetOrCreate()
        {
            if (_instance == null)
            {
                var go = new GameObject("[SceneTransitionRunner]");
                _instance = go.AddComponent<TransitionCoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}
