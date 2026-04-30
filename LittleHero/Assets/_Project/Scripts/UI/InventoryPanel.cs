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
        [SerializeField] private EquipmentTemplate[] _templateLookup;
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            BindSlots();
            Refresh();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_slots != null)
                foreach (var s in _slots)
                    if (s != null) s.OnClicked -= HandleSlotClicked;
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
            var data = MetaProgression.Instance?.Data;
            EquipmentInstance[] instances = data?.equipmentInstances ?? new EquipmentInstance[0];
            // 슬롯 → 장착된 인스턴스 매핑 (isEquipped=true 인 첫 인스턴스)
            var equipped = new Dictionary<EquipSlot, EquipmentInstance>();
            for (int i = 0; i < instances.Length; i++)
            {
                var inst = instances[i];
                if (inst == null || !inst.isEquipped) continue;
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
            if (_cardPanel == null) return;
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) return;
            for (int i = 0; i < data.equipmentInstances.Length; i++)
            {
                var inst = data.equipmentInstances[i];
                if (inst == null || !inst.isEquipped) continue;
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
            // SaveData.firstClearedChapters int 의 비트 N-1 = 챕터 N 첫 클리어.
            int mask = MetaProgression.Instance?.Data?.firstClearedChapters ?? 0;
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
            return (mask & (1 << (requiredChapter - 1))) != 0;
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
