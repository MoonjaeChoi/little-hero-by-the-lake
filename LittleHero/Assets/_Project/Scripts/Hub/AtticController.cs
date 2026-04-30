using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero
{
    public class AtticController : MonoBehaviour
    {
        [Serializable]
        public class ChapterButton
        {
            public ChapterData chapter;
            public Button button;
        }

        [SerializeField] private Text _starFragmentsText;
        [SerializeField] private string _chapterSceneName = "Chapter";
        [SerializeField] private ChapterButton[] _chapterButtons;

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged += UpdateStarFragments;

            if (_chapterButtons != null)
            {
                for (int i = 0; i < _chapterButtons.Length; i++)
                {
                    var entry = _chapterButtons[i];
                    if (entry?.button == null) continue;
                    var captured = entry.chapter;
                    entry.button.onClick.AddListener(() => HandleStart(captured));
                }
            }
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged -= UpdateStarFragments;

            if (_chapterButtons != null)
                foreach (var entry in _chapterButtons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
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

        private void HandleStart(ChapterData chapter)
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.SelectedChapter = chapter;
            SceneManager.LoadScene(_chapterSceneName);
        }
    }
}
