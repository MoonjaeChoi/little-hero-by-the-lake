using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT — 한정 상점 패널 (limited_shop_v1.md §7.1).
    /// 활성 핫딜 그리드 + 카운트다운. 트리거 hook 자동 갱신.
    /// </summary>
    public class LimitedShopPanel : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private LimitedShopOffer[] _catalog;
        [SerializeField] private GachaPool _gachaPool;       // GuaranteedRarityPool/Weapon grant 용

        [Header("UI")]
        [SerializeField] private Transform _itemContainer;   // GridLayoutGroup 자식
        [SerializeField] private LimitedShopItemView _itemPrefab;
        [SerializeField] private Text _emptyText;            // 활성 핫딜 없을 때
        [SerializeField] private Button _closeButton;

        private readonly List<LimitedShopItemView> _spawned = new List<LimitedShopItemView>();

        private void Awake()
        {
            if (!LimitedShopService.IsInitialized)
                LimitedShopService.Initialize(_catalog, _gachaPool);
        }

        private void OnEnable()
        {
            transform.SetAsLastSibling();
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            GameEvents.OnLimitedOfferActivated += HandleChanged;
            GameEvents.OnLimitedOfferPurchased += HandleChanged;
            GameEvents.OnLimitedOfferExpired   += HandleChanged;
            Refresh();
        }

        private void OnDisable()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            GameEvents.OnLimitedOfferActivated -= HandleChanged;
            GameEvents.OnLimitedOfferPurchased -= HandleChanged;
            GameEvents.OnLimitedOfferExpired   -= HandleChanged;
        }

        private void HandleChanged(LimitedShopOffer _) => Refresh();

        private void Refresh()
        {
            // 기존 자식 청소
            for (int i = _spawned.Count - 1; i >= 0; i--)
                if (_spawned[i] != null) Destroy(_spawned[i].gameObject);
            _spawned.Clear();

            var active = LimitedShopService.GetActiveOffers();
            if (_emptyText != null) _emptyText.gameObject.SetActive(active.Count == 0);

            if (_itemPrefab == null || _itemContainer == null) return;
            for (int i = 0; i < active.Count; i++)
            {
                var v = Instantiate(_itemPrefab, _itemContainer);
                v.Bind(active[i]);
                _spawned.Add(v);
            }
        }

        public void Close() => gameObject.SetActive(false);
    }
}
