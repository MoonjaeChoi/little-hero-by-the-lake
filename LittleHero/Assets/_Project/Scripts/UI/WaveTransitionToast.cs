using LitMotion;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v2 fix — 웨이브 전환 시각 피드백.
    /// 상점 페이즈 폐기 후 5초 휴식 동안 "Wave N 완료! 다음 5초..." 토스트 + 카운트다운.
    /// OnWaveCleared 이벤트 구독 → 5초 표시 후 자동 비활성.
    /// 5016 T05 — IEnumerator + while-loop → LMotion (state machine alloc 0).
    /// WithScheduler(UpdateIgnoreTimeScale) — 사용자 baseline 정합 (HitPause/timeScale 영향 X).
    ///
    /// 사용자 baseline (2026-05-05) — 가독성 강화:
    ///   - LMotion UpdateIgnoreTimeScale (HitPause 누적 시 timeScale 영향 회피)
    ///   - 큰 카운트다운 텍스트 (5/4/3/2/1)
    ///   - Progress dots ●●○○○ 시각화
    ///   - (선택) progress bar SerializeField — Inspector wiring 시 동작
    /// </summary>
    public class WaveTransitionToast : MonoBehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private Text _countdownText;
        [SerializeField] private Image _progressBar;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _restSeconds = 5f;

        private MotionHandle _countdownHandle;
        private int _activeClearedWave;
        private int _activeTotalWaves;

        private void Awake()
        {
            if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void OnEnable()
        {
            GameEvents.OnWaveCleared += HandleWaveCleared;
            GameEvents.OnPlayerDied += Hide;
            GameEvents.OnChapterCleared += HideOnChapter;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveCleared -= HandleWaveCleared;
            GameEvents.OnPlayerDied -= Hide;
            GameEvents.OnChapterCleared -= HideOnChapter;
        }

        private void HandleWaveCleared(int clearedWave)
        {
            // 챕터 마지막 웨이브 = ChapterCleared 이벤트로 별도 처리. 여기는 다음 웨이브 진행 시만.
            int total = MetaProgressionChapter.SelectedChapter?.totalWaves ?? 20;
            if (clearedWave >= total) return;

            if (_countdownHandle.IsActive()) _countdownHandle.Cancel();
            _activeClearedWave = clearedWave;
            _activeTotalWaves = total;
            SetVisible(true);

            // remaining 5 → 0, UpdateIgnoreTimeScale (사용자 baseline 정합)
            _countdownHandle = LMotion.Create(_restSeconds, 0f, _restSeconds)
                .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                .WithOnComplete(() =>
                {
                    SetVisible(false);
                })
                .Bind(this, (remaining, self) => self.UpdateCountdownUI(remaining));
        }

        private void UpdateCountdownUI(float remaining)
        {
            int seconds = Mathf.CeilToInt(remaining);
            int nextWave = _activeClearedWave + 1;
            if (_messageText != null)
            {
                int filled = Mathf.Clamp(_restSeconds > 0f
                    ? Mathf.CeilToInt((1f - remaining / _restSeconds) * _restSeconds)
                    : 0, 0, Mathf.RoundToInt(_restSeconds));
                string dots = BuildProgressDots(filled, Mathf.RoundToInt(_restSeconds));
                _messageText.text = $"<size=36>웨이브 {_activeClearedWave} / {_activeTotalWaves} 완료!</size>\n{dots}\n<size=28>다음: 웨이브 {nextWave}</size>";
            }
            if (_countdownText != null)
            {
                _countdownText.text = seconds.ToString();
            }
            if (_progressBar != null)
            {
                _progressBar.fillAmount = 1f - (remaining / _restSeconds);
            }
        }

        // 5 dots ●●●○○ 형태 — filled 만큼 ● 나머지 ○
        private static string BuildProgressDots(int filled, int total)
        {
            if (total <= 0) return "";
            var sb = new System.Text.StringBuilder(total * 2);
            for (int i = 0; i < total; i++)
                sb.Append(i < filled ? "●" : "○");
            return sb.ToString();
        }

        private void HideOnChapter(ChapterData _) => Hide();

        private void Hide()
        {
            if (_countdownHandle.IsActive()) _countdownHandle.Cancel();
            SetVisible(false);
        }

        private void SetVisible(bool on)
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = on ? 1f : 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
            else
            {
                gameObject.SetActive(on);
            }
        }
    }
}
