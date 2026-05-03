using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 검은 fade overlay (DontDestroyOnLoad Canvas). ISceneTransition 구현.
    /// Resources/UI/FadeOverlay.prefab 으로 RuntimeInitializeOnLoadMethod 자동 로드 (Bootstrap).
    /// Time.unscaledDeltaTime 사용 — Time.timeScale=0 일시정지 중에도 fade 동작.
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
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _blackImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / duration));
                yield return null;
            }
            _blackImage.color = Color.black;
        }

        public IEnumerator FadeIn(float duration)
        {
            if (_blackImage == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _blackImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(t / duration));
                yield return null;
            }
            _blackImage.color = new Color(0, 0, 0, 0);
        }
    }
}
