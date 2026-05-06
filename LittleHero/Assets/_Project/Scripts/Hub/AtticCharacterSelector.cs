using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 2 — 다락방 캐릭터 선택 (3 캐릭터 버튼 + 강제 첫 선택 prompt + 저장 복원).
    /// AtticController 에서 추출 (§15.1 SRP). MetaProgression.OnCharacterChanged 구독.
    /// </summary>
    public class AtticCharacterSelector : MonoBehaviour
    {
        [Serializable]
        public class CharacterButton
        {
            public CharacterData character;
            public Button button;
            public Image highlight;
        }

        [SerializeField] private Text _selectedCharacterText;
        [SerializeField] private CharacterButton[] _buttons;
        [SerializeField] private CharacterData _defaultCharacter;

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.OnCharacterChanged += UpdateSelectedCharacter;

            if (_buttons != null)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    var entry = _buttons[i];
                    if (entry?.button == null) continue;
                    var captured = entry.character;
                    entry.button.onClick.AddListener(() => HandleSelectCharacter(captured));
                }
            }
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.OnCharacterChanged -= UpdateSelectedCharacter;
            if (_buttons != null)
                foreach (var entry in _buttons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            if (MetaProgression.Instance == null) return;
            var saved = MetaProgressionCharacter.SelectedCharacter;
            if (saved == null)
            {
                string id = MetaProgression.Instance.Data?.selectedCharacterId;
                saved = ResolveCharacterById(id);
                if (saved == null)
                {
                    ShowFirstStartCharacterPrompt();
                    return;
                }
                MetaProgressionCharacter.SelectCharacter(saved);
            }
            UpdateSelectedCharacter(saved);
        }

        private void ShowFirstStartCharacterPrompt()
        {
            if (_selectedCharacterText != null)
                _selectedCharacterText.text = "영웅을 선택하세요!";
            Debug.Log("[AtticCharacterSelector] 첫 시작 — 캐릭터 선택 강제");
        }

        private CharacterData ResolveCharacterById(string id)
        {
            if (string.IsNullOrEmpty(id) || _buttons == null) return null;
            foreach (var entry in _buttons)
                if (entry?.character != null && entry.character.id == id) return entry.character;
            return null;
        }

        private void HandleSelectCharacter(CharacterData character)
        {
            if (MetaProgression.Instance == null || character == null) return;
            MetaProgressionCharacter.SelectCharacter(character);
        }

        private void UpdateSelectedCharacter(CharacterData character)
        {
            if (_selectedCharacterText != null)
                _selectedCharacterText.text = character != null ? $"선택: {character.displayName}" : "선택: -";

            if (_buttons != null)
                foreach (var entry in _buttons)
                    if (entry?.highlight != null)
                        entry.highlight.enabled = (entry.character == character);
        }
    }
}
