using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// inventory_ui_v2.md — 페이징 (9 카드/페이지) + 썸네일 + 옵션 6라인 표시.
    /// 슬롯 클릭 시 진입. 카드 클릭 → 하단 상세 갱신. 장착/해제 버튼.
    /// </summary>
    public class InventorySelectionPanel : MonoBehaviour
    {
        [Header("그리드")]
        [SerializeField] private Transform _gridContainer;     // GridLayoutGroup, 9 카드 자식
        [SerializeField] private InventoryGridCard[] _cards;   // 9개, 재사용

        [Header("페이징")]
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Text _pageText;               // "1 / 6"

        [Header("상세 + 액션")]
        [SerializeField] private InventoryDetailView _detailView;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _unequipButton;
        [SerializeField] private Text _equipButtonText;        // 토글 텍스트

        [Header("기본")]
        [SerializeField] private Text _titleText;
        [SerializeField] private GameObject _emptyText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private EquipmentCardPanel _cardPanel; // 옵션 리롤 진입 (선택)
        [SerializeField] private Button _detailButton;         // "옵션 보기" → cardPanel
        [SerializeField] private InventoryPanel _inventoryPanel;
        [SerializeField] private EquipmentTemplate[] _templateLookup;

        private const int PAGE_SIZE = 9;
        private EquipSlot _currentSlot;
        private int _currentPage;
        // architecture_analysis_v1 §A — Model 분리. View 는 데이터 collect/equip 직접 책임 X
        private InventoryModel _model;
        private List<InventoryMatch> _matches = new List<InventoryMatch>();
        private EquipmentInstance _selected;
        private EquipmentTemplate _selectedTpl;

        private void Awake()
        {
            // 5033.0 — Awake 1회 등록 (OnEnable 미트리거 시나리오 회피).
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            if (_prevButton != null) _prevButton.onClick.AddListener(() => GoPage(-1));
            if (_nextButton != null) _nextButton.onClick.AddListener(() => GoPage(+1));
            if (_equipButton != null) _equipButton.onClick.AddListener(ToggleEquip);
            if (_detailButton != null) _detailButton.onClick.AddListener(OpenCardPanel);
            BindCards();

            // 5033.0 — 자체 Canvas + sortingOrder 우선화 (부모 InventoryPanel 의 X 보다 raycast 우선).
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            if (GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 5033.0 — 비-Selectable Graphic 의 raycastTarget=false 자동 적용
            // (Detail/Text/Image 가 click 가로채는 결함 회피. Play Mode 마다 보장)
            foreach (var g in GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
            {
                if (!g.raycastTarget) continue;
                if (g.GetComponent<UnityEngine.UI.Selectable>() != null) continue;
                if (g.GetComponentInParent<UnityEngine.UI.Selectable>() != null) continue;
                g.raycastTarget = false;
            }
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_prevButton != null) _prevButton.onClick.RemoveAllListeners();
            if (_nextButton != null) _nextButton.onClick.RemoveAllListeners();
            if (_equipButton != null) _equipButton.onClick.RemoveAllListeners();
            if (_detailButton != null) _detailButton.onClick.RemoveAllListeners();
            if (_cards != null)
                foreach (var c in _cards)
                    if (c != null) c.OnClicked = null;
        }

        private void BindCards()
        {
            if (_cards == null) return;
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null) continue;
                _cards[i].OnClicked = HandleCardClicked;
            }
        }

        public void Show(EquipSlot slot)
        {
            _currentSlot = slot;
            _currentPage = 0;
            _selected = null;
            _selectedTpl = null;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
            if (_titleText != null) _titleText.text = $"{SlotLabel(slot)} — 보유 장비";
            CollectMatches();
            DrawPage(0);
            if (_detailView != null) _detailView.Clear();
            UpdateActionButtons();
            Debug.Log($"[InventorySelectionPanel] Show {slot} — matches={_matches.Count}");
        }

        public void Close() => gameObject.SetActive(false);

        // ===== 매칭 & 그리드 =====

        private void CollectMatches()
        {
            // architecture_analysis_v1 §A — Model 위임 (CollectMatches + 정렬)
            if (_model == null) _model = new InventoryModel(_templateLookup);
            else _model.SetTemplates(_templateLookup);
            _matches = _model.CollectMatches(_currentSlot);
        }

        private void DrawPage(int page)
        {
            int total = TotalPages();
            bool empty = _matches.Count == 0;
            if (_emptyText != null) _emptyText.SetActive(empty);
            if (_gridContainer != null) _gridContainer.gameObject.SetActive(!empty);

            if (_cards == null) return;
            int start = page * PAGE_SIZE;
            for (int i = 0; i < _cards.Length; i++)
            {
                if (_cards[i] == null) continue;
                int idx = start + i;
                if (idx < _matches.Count)
                {
                    _cards[i].gameObject.SetActive(true);
                    var m = _matches[idx];
                    _cards[i].Bind(m.instance, m.template, m.instance.isEquipped);
                    _cards[i].SetSelected(m.instance == _selected);
                }
                else _cards[i].gameObject.SetActive(false);
            }
            UpdatePageText(total);
            UpdateNavButtons(total);
        }

        private void GoPage(int delta)
        {
            int total = TotalPages();
            int newPage = Mathf.Clamp(_currentPage + delta, 0, Mathf.Max(0, total - 1));
            if (newPage == _currentPage) return;
            _currentPage = newPage;
            DrawPage(_currentPage);
        }

        private int TotalPages() => Mathf.Max(1, (_matches.Count + PAGE_SIZE - 1) / PAGE_SIZE);

        private void UpdatePageText(int total)
        {
            if (_pageText != null)
                _pageText.text = _matches.Count == 0 ? "0 / 0" : $"{_currentPage + 1} / {total}";
        }

        private void UpdateNavButtons(int total)
        {
            if (_prevButton != null) _prevButton.interactable = _currentPage > 0;
            if (_nextButton != null) _nextButton.interactable = _currentPage < total - 1;
        }

        // ===== 카드 선택 + 상세 =====

        private void HandleCardClicked(EquipmentInstance inst, EquipmentTemplate tpl)
        {
            _selected = inst;
            _selectedTpl = tpl;
            if (_cards != null)
                foreach (var c in _cards) if (c != null) c.SetSelected(c.Instance == inst);
            if (_detailView != null) _detailView.Show(inst, tpl);
            UpdateActionButtons();
        }

        private void UpdateActionButtons()
        {
            bool hasSelection = _selected != null;
            if (_equipButton != null)
            {
                _equipButton.interactable = hasSelection;
                if (_equipButtonText != null)
                    _equipButtonText.text = hasSelection && _selected.isEquipped ? "해제" : "장착";
            }
            if (_unequipButton != null)
                _unequipButton.gameObject.SetActive(false);  // 단일 토글 버튼만 사용
            if (_detailButton != null)
                _detailButton.interactable = hasSelection;
        }

        private void ToggleEquip()
        {
            // architecture_analysis_v1 §A — Model 위임 (장착 비즈니스 로직)
            if (_model == null || _selected == null || _selectedTpl == null) return;
            _model.ToggleEquip(_selected, _selectedTpl);
            CollectMatches();
            DrawPage(_currentPage);
            UpdateActionButtons();
            if (_inventoryPanel != null) _inventoryPanel.Refresh();
        }

        private void OpenCardPanel()
        {
            if (_selected == null || _selectedTpl == null || _cardPanel == null) return;
            _cardPanel.Show(_selected, _selectedTpl);
        }

        private EquipmentTemplate FindTemplate(string id)
        {
            if (_templateLookup == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _templateLookup.Length; i++)
                if (_templateLookup[i] != null && _templateLookup[i].id == id) return _templateLookup[i];
            return null;
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
