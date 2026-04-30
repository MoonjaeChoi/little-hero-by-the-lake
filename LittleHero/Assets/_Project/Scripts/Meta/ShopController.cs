using System;
using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class ShopController : MonoBehaviour
    {
        [Serializable]
        public struct ShopItem
        {
            public EquipmentData data;
            public int price;
            public bool sold;
        }

        [SerializeField] private ShopPool _pool;
        [SerializeField] private EquipmentManager _equipmentManager;

        private readonly List<ShopItem> _currentItems = new List<ShopItem>();
        private int _refreshCount;

        public event Action OnItemsRefreshed;
        public event Action<int> OnSlotChanged;

        public IReadOnlyList<ShopItem> CurrentItems => _currentItems;
        public int CurrentRefreshCost => ComputeRefreshCost();

        private void OnEnable()  { GameEvents.OnShopOpened += HandleShopOpened; }
        private void OnDisable() { GameEvents.OnShopOpened -= HandleShopOpened; }

        private void HandleShopOpened()
        {
            _refreshCount = 0;
            Roll();
        }

        public bool TryRefresh()
        {
            int cost = ComputeRefreshCost();
            if (!GameManager.Instance.SpendStarDust(cost)) return false;
            _refreshCount++;
            Roll();
            return true;
        }

        public bool TryBuy(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _currentItems.Count) return false;
            ShopItem item = _currentItems[slotIndex];
            if (item.sold || item.data == null) return false;
            if (!GameManager.Instance.SpendStarDust(item.price)) return false;

            _equipmentManager.Equip(item.data);
            item.sold = true;
            _currentItems[slotIndex] = item;
            OnSlotChanged?.Invoke(slotIndex);
            return true;
        }

        private void Roll()
        {
            _currentItems.Clear();
            int count = GameManager.Instance.Balance.shopItemCount;
            if (_pool == null || _pool.items == null || _pool.items.Length == 0)
            {
                OnItemsRefreshed?.Invoke();
                return;
            }
            for (int i = 0; i < count; i++)
            {
                EquipmentData data = _pool.items[UnityEngine.Random.Range(0, _pool.items.Length)];
                _currentItems.Add(new ShopItem { data = data, price = GetPrice(data.rarity), sold = false });
            }
            OnItemsRefreshed?.Invoke();
        }

        private int GetPrice(Rarity rarity)
        {
            int[] table = GameManager.Instance.Balance.priceByRarity;
            int idx = (int)rarity;
            return (table != null && idx < table.Length) ? table[idx] : 5;
        }

        private int ComputeRefreshCost()
        {
            BalanceConfig b = GameManager.Instance.Balance;
            return Mathf.RoundToInt(b.shopRefreshBaseCost * Mathf.Pow(b.shopRefreshCostMultiplier, _refreshCount));
        }
    }
}
