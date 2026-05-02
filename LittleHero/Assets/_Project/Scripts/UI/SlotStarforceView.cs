using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT D-가챠 3 — 12 슬롯 강화 그리드의 단일 슬롯 뷰.
    /// 슬롯 라벨 + 현재 ⭐ 단계 + 다음 비용 표시. 클릭 시 OnClicked(slot) 발행.
    /// </summary>
    public class SlotStarforceView : MonoBehaviour
    {
        public event Action<EquipSlot> OnClicked;

        [SerializeField] private EquipSlot _slot;
        [SerializeField] private Button _button;
        [SerializeField] private Text _slotLabelText;
        [SerializeField] private Text _starLevelText;       // ⭐N
        [SerializeField] private Text _nextCostText;        // 다음 강화 N 조각

        public EquipSlot Slot => _slot;

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(HandleClick);
            Refresh();
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        public void Refresh()
        {
            if (_slotLabelText != null) _slotLabelText.text = SlotLabel(_slot);

            int lv = StarforceService.GetLevel(_slot);
            bool maxed = lv >= StarforceService.MAX_LEVEL;
            if (_starLevelText != null) _starLevelText.text = $"⭐ {lv}";
            if (_nextCostText != null)
            {
                _nextCostText.text = maxed
                    ? "최대 단계"
                    : $"다음 ◈ {StarforceService.GetCostForLevel(lv)}";
            }
        }

        private void HandleClick() => OnClicked?.Invoke(_slot);

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
            EquipSlot.Pet => "애완동물",
            _ => s.ToString(),
        };
    }
}
