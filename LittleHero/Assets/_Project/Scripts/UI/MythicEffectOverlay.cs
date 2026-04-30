using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D11 — 신화 발현 무지개 연출 오버레이 (equipment_v1 §3.7 + ui_v1 §13).
    /// MythicMileageService.TryRoll 성공 시 호출. 풀스크린 무지개 fade-in/out + 추억 한 장면.
    /// 비폭력 톤 — "찰랑~" 사운드 + 무지개 휘감음, 폭발/번쩍임 ❌.
    /// </summary>
    public class MythicEffectOverlay : MonoBehaviour
    {
        [SerializeField] private Image _rainbowImage;        // 풀스크린 stretch
        [SerializeField] private Text _mythicNameText;
        [SerializeField] private Text _mythicFlavorText;
        [SerializeField] private float _fadeInSeconds = 0.4f;
        [SerializeField] private float _holdSeconds = 1.4f;
        [SerializeField] private float _fadeOutSeconds = 0.6f;

        private Coroutine _routine;

        public void Play(MythicDefinition mythic)
        {
            if (mythic == null) return;
            if (_mythicNameText != null) _mythicNameText.text = $"아빠의 마법 — {mythic.displayName}";
            if (_mythicFlavorText != null) _mythicFlavorText.text = string.IsNullOrEmpty(mythic.flavorText) ? "아빠의 마법이 깨어났어요" : mythic.flavorText;
            gameObject.SetActive(true);
            if (_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(PlayRoutine());
        }

        private IEnumerator PlayRoutine()
        {
            float t = 0f;
            if (_rainbowImage != null)
            {
                Color c = _rainbowImage.color; c.a = 0f; _rainbowImage.color = c;
                while (t < _fadeInSeconds)
                {
                    t += Time.unscaledDeltaTime;
                    c.a = Mathf.Lerp(0f, 0.85f, t / _fadeInSeconds);
                    _rainbowImage.color = c;
                    yield return null;
                }
                c.a = 0.85f; _rainbowImage.color = c;
            }
            yield return new WaitForSecondsRealtime(_holdSeconds);
            if (_rainbowImage != null)
            {
                Color c = _rainbowImage.color;
                t = 0f;
                while (t < _fadeOutSeconds)
                {
                    t += Time.unscaledDeltaTime;
                    c.a = Mathf.Lerp(0.85f, 0f, t / _fadeOutSeconds);
                    _rainbowImage.color = c;
                    yield return null;
                }
            }
            gameObject.SetActive(false);
            _routine = null;
        }
    }
}
