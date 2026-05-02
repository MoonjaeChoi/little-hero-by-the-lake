using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// InventorySelectionPanel 의 단일 row. 무기 1개 표시 + 장착 토글.
    /// </summary>
    public class InventorySelectionRow : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _enhanceText;
        [SerializeField] private Text _rarityText;
        [SerializeField] private Text _equippedText;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Text _equipButtonText;
        [SerializeField] private Image _highlightBg;
        [SerializeField] private Button _rowButton;

        public EquipmentInstance Instance { get; private set; }
        public Action<EquipmentInstance> OnEquipClicked;
        public Action<EquipmentInstance> OnRowClicked;

        private static readonly Color[] _rarityColors = {
            new Color(0.8f, 0.8f, 0.8f),    // Common 흰
            new Color(0.4f, 0.7f, 1f),      // Magic 청
            new Color(0.3f, 0.85f, 0.4f),   // Rare 녹
            new Color(0.7f, 0.4f, 0.95f),   // Epic 보
            new Color(1f, 0.84f, 0f),       // Legendary 골드
            new Color(1f, 0.42f, 0.7f),     // Mythic 분홍
        };

        private void OnEnable()
        {
            if (_equipButton != null) _equipButton.onClick.AddListener(HandleEquipClicked);
            if (_rowButton != null) _rowButton.onClick.AddListener(HandleRowClicked);
        }

        private void OnDisable()
        {
            if (_equipButton != null) _equipButton.onClick.RemoveAllListeners();
            if (_rowButton != null) _rowButton.onClick.RemoveAllListeners();
        }

        public void Bind(EquipmentInstance inst, EquipmentTemplate tpl, bool isEquipped)
        {
            Instance = inst;
            if (_icon != null && tpl != null) _icon.sprite = tpl.icon;
            if (_nameText != null && tpl != null) _nameText.text = tpl.displayName;
            if (_enhanceText != null) _enhanceText.text = inst.enhanceLevel > 0 ? $"+{inst.enhanceLevel}" : "";
            if (_rarityText != null && tpl != null)
            {
                _rarityText.text = RarityLabel(tpl.rarity);
                _rarityText.color = _rarityColors[(int)tpl.rarity];
            }
            if (_equippedText != null) _equippedText.gameObject.SetActive(isEquipped);
            if (_equipButtonText != null) _equipButtonText.text = isEquipped ? "해제" : "장착";
        }

        public void SetSelected(bool selected)
        {
            if (_highlightBg != null)
            {
                var c = _highlightBg.color;
                c.a = selected ? 0.4f : 0f;
                _highlightBg.color = c;
            }
        }

        private void HandleEquipClicked() => OnEquipClicked?.Invoke(Instance);
        private void HandleRowClicked() => OnRowClicked?.Invoke(Instance);

        private static string RarityLabel(Rarity r) => r switch
        {
            Rarity.Common    => "일반",
            Rarity.Magic     => "고급",
            Rarity.Rare      => "희귀",
            Rarity.Epic      => "영웅",
            Rarity.Legendary => "전설",
            Rarity.Mythic    => "신화",
            _ => r.ToString(),
        };
    }
}
