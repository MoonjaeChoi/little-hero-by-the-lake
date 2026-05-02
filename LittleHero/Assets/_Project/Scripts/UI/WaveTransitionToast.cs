using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v2 fix — 웨이브 전환 시각 피드백.
    /// 상점 페이즈 폐기 후 5초 휴식 동안 "Wave N 완료! 다음 5초..." 토스트 + 카운트다운.
    /// OnWaveCleared 이벤트 구독 → 5초 표시 후 자동 비활성.
    /// </summary>
    public class WaveTransitionToast : MonoBehaviour
    {
        [SerializeField] private Text _messageText;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _restSeconds = 5f;

        private Coroutine _activeRoutine;

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
            int total = MetaProgression.Instance?.SelectedChapter?.totalWaves ?? 20;
            if (clearedWave >= total) return;

            if (_activeRoutine != null) StopCoroutine(_activeRoutine);
            _activeRoutine = StartCoroutine(ShowCountdown(clearedWave, total));
        }

        private IEnumerator ShowCountdown(int clearedWave, int totalWaves)
        {
            SetVisible(true);
            float remaining = _restSeconds;
            while (remaining > 0f)
            {
                if (_messageText != null)
                {
                    int seconds = Mathf.CeilToInt(remaining);
                    _messageText.text = "웨이브 " + clearedWave + " / " + totalWaves + " 완료!\n다음 웨이브까지 " + seconds + "초";
                }
                yield return null;
                remaining -= Time.deltaTime;
            }
            SetVisible(false);
            _activeRoutine = null;
        }

        private void HideOnChapter(ChapterData _) => Hide();

        private void Hide()
        {
            if (_activeRoutine != null) { StopCoroutine(_activeRoutine); _activeRoutine = null; }
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
