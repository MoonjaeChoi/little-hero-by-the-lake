using System.Collections;
using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 검은 fade overlay (DontDestroyOnLoad Canvas). ISceneTransition 구현.
    /// Resources/UI/FadeOverlay.prefab 으로 RuntimeInitializeOnLoadMethod 자동 로드 (Bootstrap).
    /// 5016 T05 — IEnumerator while-loop + new Color/frame alloc → LMotion (1 alloc).
    /// ISceneTransition 시그니처 유지 — ToYieldInstruction() 으로 Coroutine 호환.
    /// WithScheduler(UpdateIgnoreTimeScale) — Time.timeScale=0 환경에서도 동작.
    /// </summary>
    public class FadeOverlay : MonoBehaviour, ISceneTransition
    {
        [SerializeField] private Image _blackImage;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneTransitionService.Default = this;
            if (_blackImage != null)
                _blackImage.color = new Color(0, 0, 0, 0);
        }

        public IEnumerator FadeOut(float duration)
        {
            if (_blackImage == null) yield break;
            // Ensure starting alpha = 0 (transparent) regardless of prior state.
            var c0 = _blackImage.color; c0.a = 0f; _blackImage.color = c0;
            yield return LMotion.Create(0f, 1f, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(_blackImage, (a, img) => { var c = img.color; c.a = a; img.color = c; })
                .ToYieldInstruction();
            _blackImage.color = Color.black;
        }

        public IEnumerator FadeIn(float duration)
        {
            if (_blackImage == null) yield break;
            // Ensure starting alpha = 1 (black) regardless of prior state.
            var c0 = _blackImage.color; c0.a = 1f; _blackImage.color = c0;
            yield return LMotion.Create(1f, 0f, duration)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .Bind(_blackImage, (a, img) => { var c = img.color; c.a = a; img.color = c; })
                .ToYieldInstruction();
            _blackImage.color = new Color(0, 0, 0, 0);
        }
    }
}
