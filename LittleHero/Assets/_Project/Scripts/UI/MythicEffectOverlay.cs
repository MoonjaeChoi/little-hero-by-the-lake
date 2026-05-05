using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D11 — 신화 발현 무지개 연출 오버레이 (equipment_v1 §3.7 + ui_v1 §13).
    /// MythicMileageService.TryRoll 성공 시 호출. 풀스크린 무지개 fade-in/out + 추억 한 장면.
    /// 비폭력 톤 — "찰랑~" 사운드 + 무지개 휘감음, 폭발/번쩍임 ❌.
    /// 5016 T05 — IEnumerator + while-loop → LSequence 3단 (fadeIn → hold → fadeOut). state machine alloc 0.
    /// </summary>
    public class MythicEffectOverlay : MonoBehaviour
    {
        [SerializeField] private Image _rainbowImage;        // 풀스크린 stretch
        [SerializeField] private Text _mythicNameText;
        [SerializeField] private Text _mythicFlavorText;
        [SerializeField] private float _fadeInSeconds = 0.4f;
        [SerializeField] private float _holdSeconds = 1.4f;
        [SerializeField] private float _fadeOutSeconds = 0.6f;

        private const float PEAK_ALPHA = 0.85f;
        private MotionHandle _sequence;

        public void Play(MythicDefinition mythic)
        {
            if (mythic == null) return;
            if (_mythicNameText != null) _mythicNameText.text = $"아빠의 마법 — {mythic.displayName}";
            if (_mythicFlavorText != null) _mythicFlavorText.text = string.IsNullOrEmpty(mythic.flavorText) ? "아빠의 마법이 깨어났어요" : mythic.flavorText;
            gameObject.SetActive(true);
            if (_sequence.IsActive()) _sequence.Cancel();
            if (_rainbowImage == null)
            {
                gameObject.SetActive(false);
                return;
            }
            // 시작 alpha = 0 보장
            var c = _rainbowImage.color; c.a = 0f; _rainbowImage.color = c;

            _sequence = LSequence.Create()
                .Append(LMotion.Create(0f, PEAK_ALPHA, _fadeInSeconds)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(_rainbowImage, (a, img) => { var col = img.color; col.a = a; img.color = col; }))
                .AppendInterval(_holdSeconds)
                .Append(LMotion.Create(PEAK_ALPHA, 0f, _fadeOutSeconds)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .Bind(_rainbowImage, (a, img) => { var col = img.color; col.a = a; img.color = col; }))
                .Run(b => b.WithOnComplete(() => gameObject.SetActive(false)));
        }
    }
}
