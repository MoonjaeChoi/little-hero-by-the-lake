using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT D-가챠 3 — 12 슬롯 강화 그리드 (gacha_v1.md §3.1).
    ///
    /// 12 SlotStarforceView 자식이 슬롯별 ⭐ 단계·다음 비용·확률 표시.
    /// 클릭 → StarforceEnhanceModal 진입.
    /// </summary>
    public class SlotStarforcePanel : MonoBehaviour
    {
        [Header("12 슬롯 (Hat~Pet 순서, 길이 12)")]
        [SerializeField] private SlotStarforceView[] _slots;

        [Header("연결")]
        [SerializeField] private StarforceEnhanceModal _enhanceModal;
        [SerializeField] private Text _shardsText;          // 마법의돌 조각 잔량
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            transform.SetAsLastSibling();
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            BindSlots();
            GameEvents.OnStarforceShardsChanged += HandleShardsChanged;
            GameEvents.OnSlotStarforceChanged   += HandleSlotChanged;
            HandleShardsChanged(StarforceService.Shards);
            Refresh();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_slots != null)
                foreach (var s in _slots)
                    if (s != null) s.OnClicked -= HandleSlotClicked;
            GameEvents.OnStarforceShardsChanged -= HandleShardsChanged;
            GameEvents.OnSlotStarforceChanged   -= HandleSlotChanged;
        }

        private void BindSlots()
        {
            if (_slots == null) return;
            for (int i = 0; i < _slots.Length; i++)
            {
                if (_slots[i] == null) continue;
                _slots[i].OnClicked -= HandleSlotClicked;
                _slots[i].OnClicked += HandleSlotClicked;
            }
        }

        private void Refresh()
        {
            if (_slots == null) return;
            for (int i = 0; i < _slots.Length; i++)
                if (_slots[i] != null) _slots[i].Refresh();
        }

        private void HandleSlotClicked(EquipSlot slot)
        {
            if (_enhanceModal != null) _enhanceModal.Show(slot);
        }

        private void HandleShardsChanged(int amount)
        {
            if (_shardsText != null) _shardsText.text = $"마법의돌 조각 ◈ {amount}";
            Refresh();
        }

        private void HandleSlotChanged(int slotIdx, int newLevel)
        {
            Refresh();
        }

        public void Close() => gameObject.SetActive(false);
    }
}
