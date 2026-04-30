using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    public class ChapterClearedPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Button _returnHubButton;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _rewardText;
        [SerializeField] private string _hubSceneName = "Attic";

        private void OnEnable()
        {
            GameEvents.OnChapterCleared += HandleChapterCleared;
            if (_returnHubButton != null) _returnHubButton.onClick.AddListener(HandleReturnHub);
        }

        private void OnDisable()
        {
            GameEvents.OnChapterCleared -= HandleChapterCleared;
            if (_returnHubButton != null) _returnHubButton.onClick.RemoveListener(HandleReturnHub);
        }

        private void Start()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void HandleChapterCleared(ChapterData data)
        {
            if (_titleText != null)
                _titleText.text = data != null ? $"챕터 클리어!\n{data.displayName}" : "챕터 클리어!";
            if (_rewardText != null && data != null)
                _rewardText.text = $"별 조각 ✦ +{data.starFragmentReward}";
            if (_panelRoot != null) _panelRoot.SetActive(true);
        }

        private void HandleReturnHub()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_hubSceneName);
        }
    }
}
