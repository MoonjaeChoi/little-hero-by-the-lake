using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// inventory_ui_v2.md §5.3 — 선택된 카드의 옵션 상세 표시 (하단 580 영역).
    /// 큰 아이콘 + 이름 + 강화·등급 + 주옵션 3 + 부옵션 3 + 신화 (있을 때).
    /// </summary>
    public class InventoryDetailView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _enhanceRarityText;
        [SerializeField] private Transform _mainAffixContainer;
        [SerializeField] private Transform _subAffixContainer;
        [SerializeField] private GameObject _affixRowPrefab;  // Text 컴포넌트 보유
        [SerializeField] private Text _mythicLabel;
        [SerializeField] private GameObject _emptyHint;       // "카드를 선택하세요"

        private static readonly Color _mainColor = new Color(0.65f, 0.42f, 0.79f);  // 보라
        private static readonly Color _subColor  = new Color(0.31f, 0.76f, 0.84f);  // 청록
        private static readonly Color _mythicColor = new Color(1f, 0.84f, 0f);      // 골드

        private static readonly string[] _rarityLabels = {
            "일반", "고급", "희귀", "영웅", "전설", "신화"
        };

        public void Show(EquipmentInstance inst, EquipmentTemplate tpl)
        {
            if (inst == null || tpl == null) { Clear(); return; }
            if (_emptyHint != null) _emptyHint.SetActive(false);
            if (_icon != null) { _icon.sprite = tpl.icon; _icon.preserveAspect = true; _icon.enabled = tpl.icon != null; }
            if (_nameText != null) _nameText.text = tpl.displayName;

            int rarityIdx = Mathf.Clamp((int)tpl.rarity, 0, _rarityLabels.Length - 1);
            string lvl = inst.enhanceLevel > 0 ? $"+{inst.enhanceLevel}  " : "";
            if (_enhanceRarityText != null) _enhanceRarityText.text = $"{lvl}{_rarityLabels[rarityIdx]}";

            ClearContainer(_mainAffixContainer);
            ClearContainer(_subAffixContainer);
            FillAffixes(_mainAffixContainer, inst.mainAffixes, _mainColor);
            FillAffixes(_subAffixContainer, inst.subAffixes, _subColor);

            if (_mythicLabel != null)
            {
                bool hasMythic = !string.IsNullOrEmpty(inst.mythicId);
                _mythicLabel.gameObject.SetActive(hasMythic);
                if (hasMythic) { _mythicLabel.text = $"✨ 신화: {inst.mythicId}"; _mythicLabel.color = _mythicColor; }
            }
        }

        public void Clear()
        {
            if (_emptyHint != null) _emptyHint.SetActive(true);
            if (_icon != null) _icon.enabled = false;
            if (_nameText != null) _nameText.text = "";
            if (_enhanceRarityText != null) _enhanceRarityText.text = "";
            ClearContainer(_mainAffixContainer);
            ClearContainer(_subAffixContainer);
            if (_mythicLabel != null) _mythicLabel.gameObject.SetActive(false);
        }

        private void FillAffixes(Transform container, AffixLine[] lines, Color color)
        {
            if (container == null || _affixRowPrefab == null || lines == null) return;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i] == null) continue;
                var go = Instantiate(_affixRowPrefab, container);
                go.SetActive(true);
                var txt = go.GetComponent<Text>() ?? go.GetComponentInChildren<Text>();
                if (txt != null)
                {
                    txt.text = StatTargetLabel.Format(lines[i]);
                    txt.color = color;
                }
            }
        }

        private static void ClearContainer(Transform container)
        {
            if (container == null) return;
            for (int i = container.childCount - 1; i >= 0; i--)
                Destroy(container.GetChild(i).gameObject);
        }
    }
}
