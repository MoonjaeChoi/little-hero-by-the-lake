using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T03 — 다락방 책상 위 캐릭터 portrait 표시 (탕탕특공대 CenterStage 답습).
    /// MetaProgression.OnCharacterChanged 구독.
    /// </summary>
    public class CharacterStageView : MonoBehaviour
    {
        [SerializeField] private Image _portraitImage;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _classText;

        private void OnEnable()
        {
            MetaProgression.OnCharacterChanged += OnCharacterChanged;
            Refresh();
        }

        private void OnDisable()
        {
            MetaProgression.OnCharacterChanged -= OnCharacterChanged;
        }

        public void Refresh()
        {
            var ch = MetaProgressionCharacter.SelectedCharacter;
            ApplyCharacter(ch);
        }

        private void OnCharacterChanged(CharacterData ch) => ApplyCharacter(ch);

        private void ApplyCharacter(CharacterData ch)
        {
            if (ch == null) return;
            if (_portraitImage != null && ch.portrait != null) _portraitImage.sprite = ch.portrait;
            if (_nameText != null) _nameText.text = ch.displayName;
            if (_classText != null) _classText.text = ch.className;
        }
    }
}
