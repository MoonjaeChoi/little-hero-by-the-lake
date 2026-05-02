using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 옵션 B — 슬롯 클릭 시 보유 인스턴스 list 표시 + 장착 토글.
    /// InventoryPanel.HandleSlotClicked 진입. 같은 슬롯 인스턴스만 필터.
    /// 클릭 시 isEquipped 토글 + EquipmentManager.EquipInstance/UnequipInstance + 즉시 갱신.
    /// </summary>
    public class InventorySelectionPanel : MonoBehaviour
    {
        [SerializeField] private Transform _rowContainer;
        [SerializeField] private GameObject _rowPrefab;       // InventorySelectionRow 컴포넌트 보유 prefab
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _emptyText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _detailButton;        // EquipmentCardPanel 진입 (선택된 인스턴스)
        [SerializeField] private EquipmentCardPanel _cardPanel;
        [SerializeField] private InventoryPanel _inventoryPanel;  // 갱신용
        [SerializeField] private EquipmentTemplate[] _templateLookup;

        private EquipSlot _currentSlot;
        private EquipmentInstance _selected;
        private readonly List<InventorySelectionRow> _rows = new List<InventorySelectionRow>();

        private void OnEnable()
        {
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_detailButton != null) _detailButton.onClick.AddListener(OpenCard);
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_detailButton != null) _detailButton.onClick.RemoveAllListeners();
        }

        public void Show(EquipSlot slot)
        {
            _currentSlot = slot;
            _selected = null;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (_titleText != null) _titleText.text = $"{SlotLabel(slot)} 슬롯 — 보유 장비";
            Refresh();
        }

        public void Close() => gameObject.SetActive(false);

        private void Refresh()
        {
            ClearRows();
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) { ShowEmpty(true); return; }

            var matches = new List<(EquipmentInstance inst, EquipmentTemplate tpl)>();
            for (int i = 0; i < data.equipmentInstances.Length; i++)
            {
                var inst = data.equipmentInstances[i];
                if (inst == null) continue;
                var tpl = FindTemplate(inst.templateId);
                if (tpl == null || tpl.slot != _currentSlot) continue;
                matches.Add((inst, tpl));
            }

            if (matches.Count == 0) { ShowEmpty(true); return; }
            ShowEmpty(false);

            // 등급 내림차순 + 강화 내림차순
            matches.Sort((a, b) =>
            {
                int r = ((int)b.tpl.rarity).CompareTo((int)a.tpl.rarity);
                if (r != 0) return r;
                return b.inst.enhanceLevel.CompareTo(a.inst.enhanceLevel);
            });

            foreach (var m in matches)
            {
                if (_rowPrefab == null || _rowContainer == null) break;
                var go = Instantiate(_rowPrefab, _rowContainer);
                go.SetActive(true);
                var row = go.GetComponent<InventorySelectionRow>();
                if (row == null) row = go.AddComponent<InventorySelectionRow>();
                row.Bind(m.inst, m.tpl, m.inst.isEquipped);
                row.OnEquipClicked = HandleEquipClicked;
                row.OnRowClicked = HandleRowSelected;
                _rows.Add(row);
            }
        }

        private void HandleRowSelected(EquipmentInstance inst)
        {
            _selected = inst;
            // 단일 선택 강조 — 모든 row 의 selected 갱신
            foreach (var r in _rows) r.SetSelected(r.Instance == inst);
        }

        private void HandleEquipClicked(EquipmentInstance inst)
        {
            if (inst == null) return;
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) return;
            var tpl = FindTemplate(inst.templateId);
            if (tpl == null) return;

            bool wasEquipped = inst.isEquipped;
            // 이미 장착중 → 해제
            if (wasEquipped)
            {
                inst.isEquipped = false;
                SaveSystem.Save(data);
                var em = Object.FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);
                if (em != null) em.UnequipInstance(inst);
            }
            else
            {
                // 같은 슬롯의 다른 isEquipped=true 인스턴스 해제
                for (int i = 0; i < data.equipmentInstances.Length; i++)
                {
                    var other = data.equipmentInstances[i];
                    if (other == null || other == inst) continue;
                    if (!other.isEquipped) continue;
                    var ot = FindTemplate(other.templateId);
                    if (ot == null || ot.slot != tpl.slot) continue;
                    other.isEquipped = false;
                    var em2 = Object.FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);
                    if (em2 != null) em2.UnequipInstance(other);
                }
                inst.isEquipped = true;
                SaveSystem.Save(data);
                var em3 = Object.FindFirstObjectByType<EquipmentManager>(FindObjectsInactive.Include);
                if (em3 != null) em3.EquipInstance(inst);
            }

            Refresh();
            if (_inventoryPanel != null) _inventoryPanel.SendMessage("Refresh", SendMessageOptions.DontRequireReceiver);
        }

        private void OpenCard()
        {
            if (_selected == null || _cardPanel == null) return;
            var tpl = FindTemplate(_selected.templateId);
            if (tpl == null) return;
            _cardPanel.Show(_selected, tpl);
        }

        private EquipmentTemplate FindTemplate(string id)
        {
            if (_templateLookup == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _templateLookup.Length; i++)
                if (_templateLookup[i] != null && _templateLookup[i].id == id) return _templateLookup[i];
            return null;
        }

        private void ClearRows()
        {
            for (int i = 0; i < _rows.Count; i++)
                if (_rows[i] != null) Destroy(_rows[i].gameObject);
            _rows.Clear();
        }

        private void ShowEmpty(bool show)
        {
            if (_emptyText != null) _emptyText.gameObject.SetActive(show);
            if (_rowContainer != null) _rowContainer.gameObject.SetActive(!show);
        }

        private static string SlotLabel(EquipSlot s) => s switch
        {
            EquipSlot.Hat      => "모자",
            EquipSlot.Top      => "상의",
            EquipSlot.Bottom   => "하의",
            EquipSlot.Cloak    => "망토",
            EquipSlot.Gloves   => "장갑",
            EquipSlot.Weapon   => "무기",
            EquipSlot.Shoes    => "신발",
            EquipSlot.Earring  => "귀걸이",
            EquipSlot.Glasses  => "안경",
            EquipSlot.Necklace => "목걸이",
            EquipSlot.Ring     => "반지",
            EquipSlot.Pet      => "애완동물",
            _ => s.ToString(),
        };
    }
}
