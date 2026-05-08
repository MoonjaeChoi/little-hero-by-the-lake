using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T04 — 동화책 1 페이지의 챕터 카드 (탕탕특공대 stage card 답습).
    /// chapter thumbnail (coverArt) + 잠금/별점/권장레벨/보상 표시.
    /// </summary>
    public class ChapterCardView : MonoBehaviour
    {
        [SerializeField] private Image _thumbnail;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _stateText;       // 잠금/해방/완료
        [SerializeField] private Text _recommendText;
        [SerializeField] private Image _lockedOverlay;
        [SerializeField] private Button _selectButton;

        private ChapterData _chapter;
        public event Action<ChapterData> OnSelected;

        public void Bind(ChapterData chapter, bool unlocked, bool firstCleared, int playerLevel, int recommendedLevel)
        {
            _chapter = chapter;
            if (_thumbnail != null) _thumbnail.sprite = chapter?.coverArt;
            if (_titleText != null) _titleText.text = chapter != null ? $"제 {chapter.chapterNumber}장 — {chapter.displayName}" : "";
            if (_stateText != null) _stateText.text = unlocked ? (firstCleared ? "★ 클리어" : "해방") : "잠금";
            if (_recommendText != null) _recommendText.text = $"권장 Lv.{recommendedLevel}";
            if (_lockedOverlay != null) _lockedOverlay.enabled = !unlocked;
            if (_selectButton != null) _selectButton.interactable = unlocked;
        }

        private void OnEnable()
        {
            if (_selectButton != null) _selectButton.onClick.AddListener(OnSelect);
        }

        private void OnDisable()
        {
            if (_selectButton != null) _selectButton.onClick.RemoveAllListeners();
        }

        private void OnSelect()
        {
            if (_chapter != null) OnSelected?.Invoke(_chapter);
        }
    }
}
