using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D3 — 신기한 지도의 챕터 1개 노드. 잠금/해방/첫 클리어 3 상태 표시.
    /// </summary>
    public class WondrousMapNode : MonoBehaviour
    {
        public event Action<ChapterData> OnClicked;

        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _lockOverlay;
        [SerializeField] private Image _starBadge;       // 첫 클리어 ⭐
        [SerializeField] private Text _chapterLabelText; // "1", "12" 등
        [SerializeField] private Text _nameText;         // 챕터 이름

        private ChapterData _chapter;

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        public void Bind(ChapterData chapter, int chapterNum, bool unlocked, bool firstCleared, int playerLevel)
        {
            _chapter = chapter;
            if (_chapterLabelText != null) _chapterLabelText.text = chapterNum.ToString();
            if (_nameText != null) _nameText.text = chapter != null ? chapter.displayName : "?";
            if (_icon != null && chapter != null && chapter.coverArt != null)
            {
                _icon.sprite = chapter.coverArt;
                _icon.enabled = true;
            }
            if (_lockOverlay != null) _lockOverlay.enabled = !unlocked;
            if (_starBadge != null) _starBadge.enabled = firstCleared;
            if (_button != null) _button.interactable = unlocked;
        }

        private void HandleClick() => OnClicked?.Invoke(_chapter);
    }
}
