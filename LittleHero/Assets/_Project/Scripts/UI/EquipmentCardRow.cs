using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D5 — 장비 카드의 각 affix 라인 row (ui_v1 §9 + affix_pool §8.1).
    /// 라인별 잠금 토글 + 텍스트(채널·값) 표시. 부옵션은 청록 배경(외부에서 색 설정).
    /// 잠금 토글은 무료 — 다음 리롤 비용에만 영향 (lockMultiplier).
    /// </summary>
    public class EquipmentCardRow : MonoBehaviour
    {
        [SerializeField] private Text _label;
        [SerializeField] private Toggle _lockToggle;
        [SerializeField] private Image _background;
        [SerializeField] private Color _mainColor = new Color(0.66f, 0.42f, 0.83f, 0.32f);
        [SerializeField] private Color _subColor  = new Color(0.36f, 0.70f, 0.83f, 0.32f);

        private AffixLine _line;
        private Action _onChanged;

        public void Bind(AffixLine line, bool isSub, Action onChanged)
        {
            _line = line;
            _onChanged = onChanged;
            if (_background != null) _background.color = isSub ? _subColor : _mainColor;
            if (_label != null) _label.text = Format(line);
            if (_lockToggle != null)
            {
                _lockToggle.SetIsOnWithoutNotify(line != null && line.locked);
                _lockToggle.onValueChanged.RemoveAllListeners();
                _lockToggle.onValueChanged.AddListener(OnLockChanged);
            }
        }

        private void OnLockChanged(bool isOn)
        {
            if (_line == null) return;
            _line.locked = isOn;
            _onChanged?.Invoke();
        }

        private static string Format(AffixLine line)
        {
            if (line == null) return "(빈 라인)";
            string display = line.affixId;
            string sign = line.value >= 0 ? "+" : "";
            // op == "MUL" → 퍼센트 표기, ADD → 절대값 (어린이 친화: "곱셈" 어휘 미노출)
            return line.opName == "MUL"
                ? $"{display} {sign}{line.value:F1}%"
                : $"{display} {sign}{line.value:F1}";
        }
    }
}
