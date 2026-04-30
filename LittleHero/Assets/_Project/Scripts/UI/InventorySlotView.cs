using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D4 — 인벤토리 슬롯 1개 뷰. 자물쇠/빈 슬롯/장착 아이콘 3 상태.
    /// 클릭 시 OnClicked(slot) 발행 (해방·장착 시에만).
    /// </summary>
    public class InventorySlotView : MonoBehaviour
    {
        public event Action<EquipSlot> OnClicked;

        [SerializeField] private EquipSlot _slot;
        [SerializeField] private Button _button;
        [SerializeField] private Image _icon;
        [SerializeField] private Image _lockOverlay;
        [SerializeField] private Text _enhanceBadgeText;
        [SerializeField] private Text _slotLabelText;

        public EquipSlot Slot => _slot;

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        public void Bind(EquipSlot slot, EquipmentInstance instance, EquipmentTemplate template, bool unlocked)
        {
            _slot = slot;
            if (_lockOverlay != null) _lockOverlay.enabled = !unlocked;
            if (_button != null) _button.interactable = unlocked;
            if (_slotLabelText != null) _slotLabelText.text = SlotLabel(slot);

            if (instance != null && template != null && unlocked)
            {
                if (_icon != null)
                {
                    _icon.sprite = template.icon;
                    _icon.enabled = template.icon != null;
                }
                if (_enhanceBadgeText != null)
                    _enhanceBadgeText.text = instance.enhanceLevel > 0 ? $"+{instance.enhanceLevel}" : "";
            }
            else
            {
                if (_icon != null) _icon.enabled = false;
                if (_enhanceBadgeText != null) _enhanceBadgeText.text = "";
            }
        }

        private void HandleClick()
        {
            OnClicked?.Invoke(_slot);
        }

        private static string SlotLabel(EquipSlot s) => s switch
        {
            EquipSlot.Hat => "모자",
            EquipSlot.Top => "상의",
            EquipSlot.Bottom => "하의",
            EquipSlot.Cloak => "망토",
            EquipSlot.Gloves => "장갑",
            EquipSlot.Weapon => "무기",
            EquipSlot.Shoes => "신발",
            EquipSlot.Earring => "귀걸이",
            EquipSlot.Glasses => "안경",
            EquipSlot.Necklace => "목걸이",
            EquipSlot.Ring => "반지",
            EquipSlot.Pet => "펫",
            _ => s.ToString(),
        };
    }
}
