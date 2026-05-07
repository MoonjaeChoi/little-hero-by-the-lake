using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D4 — 인벤토리 12 슬롯 풀스크린 (ui_v1 §8 + equipment §3.7).
    /// 좌측: 캐릭터 풀바디 + 4 코어 스탯 미리보기 (선택).
    /// 우측: 12 슬롯 그리드 (주 7 + 장신구 5). 장신구는 챕터 보스 해방 전까지 자물쇠.
    /// 각 슬롯 클릭 시 EquipmentCardPanel.Show 로 6단 카드 진입.
    /// </summary>
    public class InventoryPanel : MonoBehaviour
    {
        [Header("12 슬롯 (Hat~Pet 순서, 길이 12)")]
        [SerializeField] private InventorySlotView[] _slots;

        [Header("연결")]
        [SerializeField] private EquipmentCardPanel _cardPanel;
        [SerializeField] private InventorySelectionPanel _selectionPanel;  // 옵션 B
        [SerializeField] private EquipmentTemplate[] _templateLookup;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            // 5033.0 — Awake 1회 등록 (OnEnable 미트리거 시나리오 회피, panel default active 시 listener 누락 방지)
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
        }

        private void OnEnable()
        {
            BindSlots();
            Refresh();
        }

        private void OnDisable()
        {
            if (_slots != null)
                foreach (var s in _slots)
                    if (s != null) s.OnClicked -= HandleSlotClicked;
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
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

        public void Refresh()
        {
            var data = MetaProgression.Instance?.Data;
            EquipmentInstance[] instances = data?.equipmentInstances ?? new EquipmentInstance[0];
            // 5033.2 — 슬롯 → 현재 직업으로 장착된 인스턴스 매핑 (equippedByClass 기반)
            var playerClass = EquipmentClassResolver.ResolvePlayerClass();
            var equipped = new Dictionary<EquipSlot, EquipmentInstance>();
            for (int i = 0; i < instances.Length; i++)
            {
                var inst = instances[i];
                if (inst == null || inst.EquippedByClass != playerClass) continue;
                var template = FindTemplate(inst.templateId);
                if (template == null) continue;
                equipped[template.slot] = inst;
            }

            if (_slots == null) return;
            for (int i = 0; i < _slots.Length; i++)
            {
                var view = _slots[i];
                if (view == null) continue;
                EquipSlot slot = view.Slot;
                bool unlocked = IsSlotUnlocked(slot);
                EquipmentInstance inst;
                EquipmentTemplate tpl = null;
                if (equipped.TryGetValue(slot, out inst))
                    tpl = FindTemplate(inst.templateId);
                view.Bind(slot, inst, tpl, unlocked);
            }
        }

        private void HandleSlotClicked(EquipSlot slot)
        {
            Debug.Log($"[InventoryPanel] Slot clicked: {slot} (selectionPanel={_selectionPanel != null})");
            // 옵션 B — 보유 인스턴스 list 진입 (장착/해제 가능)
            if (_selectionPanel != null)
            {
                _selectionPanel.Show(slot);
                return;
            }
            // 폴백 — SelectionPanel 미wiring 시 EquipmentCardPanel 직접
            if (_cardPanel == null) return;
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) return;
            for (int i = 0; i < data.equipmentInstances.Length; i++)
            {
                var inst = data.equipmentInstances[i];
                if (inst == null || !inst.IsEquippedByCurrentClass()) continue;
                var tpl = FindTemplate(inst.templateId);
                if (tpl != null && tpl.slot == slot)
                {
                    _cardPanel.Show(inst, tpl);
                    return;
                }
            }
        }


        private static bool IsSlotUnlocked(EquipSlot slot)
        {
            // 주 슬롯 7개는 항상 해방.
            switch (slot)
            {
                case EquipSlot.Hat: case EquipSlot.Top: case EquipSlot.Bottom:
                case EquipSlot.Cloak: case EquipSlot.Gloves: case EquipSlot.Weapon:
                case EquipSlot.Shoes: return true;
            }
            // 장신구 5: 챕터 6~10 첫 클리어 비트로 판정 (chapter_curve §3.7).
            // SaveData.firstClearedChapters long 의 비트 N-1 = 챕터 N 첫 클리어.
            long mask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0L;
            int requiredChapter = slot switch
            {
                EquipSlot.Earring  => 6,
                EquipSlot.Glasses  => 7,
                EquipSlot.Necklace => 8,
                EquipSlot.Ring     => 9,
                EquipSlot.Pet      => 10,
                _ => 0,
            };
            if (requiredChapter == 0) return false;
            return (mask & (1L << (requiredChapter - 1))) != 0;
        }

        private EquipmentTemplate FindTemplate(string id)
        {
            if (_templateLookup == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _templateLookup.Length; i++)
                if (_templateLookup[i] != null && _templateLookup[i].id == id) return _templateLookup[i];
            return null;
        }

        public void Close() => gameObject.SetActive(false);
    }
}
