using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class ShopSlotView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Button _buyButton;

        private int _index;
        private Action<int> _onBuy;

        private void Awake()
        {
            if (_buyButton != null)
                _buyButton.onClick.AddListener(HandleClick);
        }

        private void OnDestroy()
        {
            if (_buyButton != null)
                _buyButton.onClick.RemoveListener(HandleClick);
        }

        public void Bind(int index, ShopController.ShopItem item, Action<int> onBuy)
        {
            _index = index;
            _onBuy = onBuy;

            if (_icon != null)
            {
                _icon.sprite = item.data != null ? item.data.icon : null;
                _icon.enabled = _icon.sprite != null;
            }

            if (_nameText != null)
                _nameText.text = item.sold
                    ? "구매완료"
                    : (item.data != null ? item.data.displayName : "(빈 슬롯)");

            if (_priceText != null)
                _priceText.text = $"★ {item.price}";

            if (_buyButton != null)
                _buyButton.interactable = !item.sold && item.data != null;
        }

        private void HandleClick() => _onBuy?.Invoke(_index);
    }
}
