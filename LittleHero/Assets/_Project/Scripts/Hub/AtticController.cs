using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    public class AtticController : MonoBehaviour
    {
        [SerializeField] private Text _starFragmentsText;
        [SerializeField] private Button _startChapterButton;
        [SerializeField] private string _chapterSceneName = "Chapter";

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged += UpdateStarFragments;
            if (_startChapterButton != null)
                _startChapterButton.onClick.AddListener(HandleStart);
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged -= UpdateStarFragments;
            if (_startChapterButton != null)
                _startChapterButton.onClick.RemoveListener(HandleStart);
        }

        private void Start()
        {
            Time.timeScale = 1f;
            int frags = MetaProgression.Instance != null ? MetaProgression.Instance.StarFragments : 0;
            UpdateStarFragments(frags);
        }

        private void UpdateStarFragments(int amount)
        {
            if (_starFragmentsText != null)
                _starFragmentsText.text = $"별 조각 ✦ {amount}";
        }

        private void HandleStart()
        {
            SceneManager.LoadScene(_chapterSceneName);
        }
    }
}
