using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D12 — 권장 레벨 미달 경고 (ui_v1 §7.2).
    /// "추천 레벨이 안 됐어요. 그래도 도전할까요?" — 도전 / 닫기 2 버튼.
    /// </summary>
    public class RecommendedLevelWarningModal : MonoBehaviour
    {
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;

        private Action _onConfirm;

        public void Show(ChapterData chapter, int playerLevel, int recommendedLevel, Action onConfirm)
        {
            _onConfirm = onConfirm;
            gameObject.SetActive(true);
            if (_titleText != null)
                _titleText.text = $"제 {chapter.chapterNumber}장 — {chapter.displayName}";
            if (_descText != null)
                _descText.text =
                    $"권장 레벨 {recommendedLevel}, 현재 레벨 {playerLevel}.\n" +
                    "조금 어려울 수 있어요. 그래도 도전할까요?";

            if (_confirmButton != null)
            {
                _confirmButton.onClick.RemoveAllListeners();
                _confirmButton.onClick.AddListener(OnConfirm);
            }
            if (_cancelButton != null)
            {
                _cancelButton.onClick.RemoveAllListeners();
                _cancelButton.onClick.AddListener(OnCancel);
            }
        }

        private void OnConfirm()
        {
            var cb = _onConfirm;
            _onConfirm = null;
            gameObject.SetActive(false);
            cb?.Invoke();
        }

        private void OnCancel()
        {
            _onConfirm = null;
            gameObject.SetActive(false);
        }
    }
}
