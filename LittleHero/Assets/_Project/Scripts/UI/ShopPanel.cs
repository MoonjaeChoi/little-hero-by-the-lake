using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class ShopPanel : MonoBehaviour
    {
        [SerializeField] private ShopController _shopController;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private Transform _slotsParent;
        [SerializeField] private GameObject _slotPrefab;
        [SerializeField] private Button _refreshButton;
        [SerializeField] private Button _startButton;
        [SerializeField] private Text _refreshCostText;

        private readonly List<ShopSlotView> _slotViews = new List<ShopSlotView>();
        private Action<int> _buyHandler;

        private void Awake()
        {
            _buyHandler = TryBuy;
        }

        private void OnEnable()
        {
            GameEvents.OnShopOpened    += HandleShopOpened;
            GameEvents.OnShopClosed    += HandleShopClosed;
            GameEvents.OnStarDustChanged += UpdateRefreshButton;

            if (_shopController != null)
            {
                _shopController.OnItemsRefreshed += Rebuild;
                _shopController.OnSlotChanged    += UpdateSlot;
            }
            if (_refreshButton != null) _refreshButton.onClick.AddListener(HandleRefresh);
            if (_startButton   != null) _startButton.onClick.AddListener(HandleStart);
        }

        private void OnDisable()
        {
            GameEvents.OnShopOpened    -= HandleShopOpened;
            GameEvents.OnShopClosed    -= HandleShopClosed;
            GameEvents.OnStarDustChanged -= UpdateRefreshButton;

            if (_shopController != null)
            {
                _shopController.OnItemsRefreshed -= Rebuild;
                _shopController.OnSlotChanged    -= UpdateSlot;
            }
            if (_refreshButton != null) _refreshButton.onClick.RemoveListener(HandleRefresh);
            if (_startButton   != null) _startButton.onClick.RemoveListener(HandleStart);
        }

        private void Start()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void HandleShopOpened()
        {
            if (_panelRoot != null) _panelRoot.SetActive(true);
        }

        private void HandleShopClosed()
        {
            if (_panelRoot != null) _panelRoot.SetActive(false);
        }

        private void HandleRefresh() { _shopController?.TryRefresh(); }
        private void HandleStart()   { GameManager.Instance.CloseShop(); }
        private void TryBuy(int idx) { _shopController?.TryBuy(idx); }

        private void Rebuild()
        {
            if (_shopController == null || _slotPrefab == null || _slotsParent == null) return;

            IReadOnlyList<ShopController.ShopItem> items = _shopController.CurrentItems;
            while (_slotViews.Count < items.Count)
            {
                GameObject go = Instantiate(_slotPrefab, _slotsParent);
                _slotViews.Add(go.GetComponent<ShopSlotView>());
            }
            for (int i = 0; i < _slotViews.Count; i++)
            {
                bool active = i < items.Count;
                _slotViews[i].gameObject.SetActive(active);
                if (active) _slotViews[i].Bind(i, items[i], _buyHandler);
            }
            UpdateRefreshButton(GameManager.Instance.StarDust);
        }

        private void UpdateSlot(int index)
        {
            if (_shopController == null || index < 0 || index >= _slotViews.Count) return;
            _slotViews[index].Bind(index, _shopController.CurrentItems[index], _buyHandler);
        }

        private void UpdateRefreshButton(int starDust)
        {
            int cost = _shopController != null ? _shopController.CurrentRefreshCost : 0;
            if (_refreshCostText != null) _refreshCostText.text = $"새로고침 (별가루 ★ {cost})";
            if (_refreshButton   != null) _refreshButton.interactable = starDust >= cost;
        }
    }
}
