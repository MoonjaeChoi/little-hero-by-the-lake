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

        [Serializable]
        public class CharacterButton
        {
            public CharacterData character;
            public Button button;
            public Image highlight;   // 선택된 캐릭터 강조용 (선택사항)
        }

        [SerializeField] private Text _starFragmentsText;
        [SerializeField] private Text _selectedCharacterText;
        [SerializeField] private string _chapterSceneName = "Chapter";
        [SerializeField] private ChapterButton[] _chapterButtons;
        [SerializeField] private CharacterButton[] _characterButtons;
        [SerializeField] private CharacterData _defaultCharacter;

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
            {
                MetaProgression.Instance.OnStarFragmentsChanged += UpdateStarFragments;
                MetaProgression.Instance.OnCharacterChanged     += UpdateSelectedCharacter;
            }

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

            if (_characterButtons != null)
            {
                for (int i = 0; i < _characterButtons.Length; i++)
                {
                    var entry = _characterButtons[i];
                    if (entry?.button == null) continue;
                    var captured = entry.character;
                    entry.button.onClick.AddListener(() => HandleSelectCharacter(captured));
                }
            }
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
            {
                MetaProgression.Instance.OnStarFragmentsChanged -= UpdateStarFragments;
                MetaProgression.Instance.OnCharacterChanged     -= UpdateSelectedCharacter;
            }

            if (_chapterButtons != null)
                foreach (var entry in _chapterButtons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
            if (_characterButtons != null)
                foreach (var entry in _characterButtons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            Time.timeScale = 1f;
            int frags = MetaProgression.Instance != null ? MetaProgression.Instance.StarFragments : 0;
            UpdateStarFragments(frags);

            // 저장된 캐릭터 ID 복원, 없으면 기본 캐릭터 자동 선택
            if (MetaProgression.Instance != null)
            {
                var saved = MetaProgression.Instance.SelectedCharacter;
                if (saved == null)
                {
                    string id = MetaProgression.Instance.Data?.selectedCharacterId;
                    saved = ResolveCharacterById(id) ?? _defaultCharacter;
                    if (saved != null) MetaProgression.Instance.SelectCharacter(saved);
                }
                UpdateSelectedCharacter(saved);
            }
        }

        private CharacterData ResolveCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id) || _characterButtons == null) return null;
            foreach (var entry in _characterButtons)
                if (entry?.character != null && entry.character.id == id) return entry.character;
            return null;
        }

        private void HandleSelectCharacter(CharacterData character)
        {
            if (MetaProgression.Instance == null || character == null) return;
            MetaProgression.Instance.SelectCharacter(character);
        }

        private void UpdateSelectedCharacter(CharacterData character)
        {
            if (_selectedCharacterText != null)
                _selectedCharacterText.text = character != null ? $"선택: {character.displayName}" : "선택: -";

            if (_characterButtons != null)
                foreach (var entry in _characterButtons)
                    if (entry?.highlight != null)
                        entry.highlight.enabled = (entry.character == character);
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
