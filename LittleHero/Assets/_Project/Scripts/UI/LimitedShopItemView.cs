using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT — 한정 상점 카드 1개 (limited_shop_v1.md §7.2).
    /// 카운트다운 (Update 폴링) + 구매 버튼 + 잔여 시간 < 1시간 빨간 경고.
    /// </summary>
    public class LimitedShopItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _titleText;
        [SerializeField] private Text _descriptionText;
        [SerializeField] private Text _rewardSummaryText;
        [SerializeField] private Text _priceText;
        [SerializeField] private Text _discountText;
        [SerializeField] private Text _remainingText;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Color _warningColor = new Color(1f, 0.4f, 0.4f);
        [SerializeField] private Color _normalColor = new Color(0.8f, 0.85f, 1f);

        private LimitedShopOffer _offer;

        public void Bind(LimitedShopOffer offer)
        {
            _offer = offer;
            if (offer == null) return;

            if (_icon != null && offer.icon != null) { _icon.sprite = offer.icon; _icon.enabled = true; }
            else if (_icon != null) _icon.enabled = false;

            if (_titleText != null) _titleText.text = offer.displayName;
            if (_descriptionText != null) _descriptionText.text = offer.description;
            if (_rewardSummaryText != null) _rewardSummaryText.text = SummarizeRewards(offer);
            if (_priceText != null)
            {
                _priceText.text = offer.starlightPrice > 0
                    ? "별빛 ⋆ " + offer.starlightPrice
                    : "캐시 결제";
            }
            if (_discountText != null)
            {
                _discountText.text = offer.discountLabel;
                _discountText.gameObject.SetActive(!string.IsNullOrEmpty(offer.discountLabel));
            }
            if (_purchaseButton != null)
            {
                _purchaseButton.onClick.RemoveAllListeners();
                _purchaseButton.onClick.AddListener(HandlePurchase);
            }
            UpdateRemaining();
        }

        private void Update()
        {
            if (_offer != null) UpdateRemaining();
        }

        private void UpdateRemaining()
        {
            if (_remainingText == null) return;
            long sec = LimitedShopService.GetSecondsRemaining(_offer);
            if (sec == long.MaxValue) { _remainingText.text = "영구"; _remainingText.color = _normalColor; return; }
            _remainingText.text = "남은 시간 " + FormatRemaining(sec);
            _remainingText.color = sec < 3600 ? _warningColor : _normalColor;
        }

        private void HandlePurchase()
        {
            if (_offer == null) return;
            var result = LimitedShopService.TryPurchase(_offer);
            switch (result)
            {
                case LimitedShopService.PurchaseResult.Success:
                    Debug.Log("[LimitedShop] 구매 성공: " + _offer.id);
                    break;
                case LimitedShopService.PurchaseResult.IAPRequired:
                    Debug.Log("[LimitedShop] IAP 진행 중: " + _offer.iapProductId);
                    break;
                case LimitedShopService.PurchaseResult.InsufficientStarlight:
                    Debug.Log("[LimitedShop] 별빛 부족");
                    break;
                default:
                    Debug.Log("[LimitedShop] 구매 실패: " + result);
                    break;
            }
        }

        private static string SummarizeRewards(LimitedShopOffer offer)
        {
            if (offer.rewards == null || offer.rewards.Length == 0) return "";
            var sb = new StringBuilder();
            for (int i = 0; i < offer.rewards.Length; i++)
            {
                var r = offer.rewards[i];
                if (r == null) continue;
                if (i > 0) sb.Append(" · ");
                switch (r.type)
                {
                    case LimitedRewardType.GuaranteedWeapon:    sb.Append("[" + r.equipmentTemplateId + " 확정]"); break;
                    case LimitedRewardType.GuaranteedRarityPool: sb.Append("[" + RarityKorean(r.rarity) + " 무기 확정]"); break;
                    case LimitedRewardType.StarforceShards:     sb.Append("◈ " + r.amount); break;
                    case LimitedRewardType.StarlightBundle:     sb.Append("⋆ " + r.amount); break;
                    case LimitedRewardType.SummonTickets:       sb.Append("소환권 ×" + r.amount); break;
                    case LimitedRewardType.MagicCrystal:        sb.Append("◇ " + r.amount); break;
                    case LimitedRewardType.RainbowOrb:          sb.Append("✦ " + r.amount); break;
                }
            }
            return sb.ToString();
        }

        private static string FormatRemaining(long sec)
        {
            long d = sec / 86400;
            long h = (sec % 86400) / 3600;
            long m = (sec % 3600) / 60;
            if (d > 0) return d + "일 " + h + "시간";
            if (h > 0) return h + "시간 " + m + "분";
            return m + "분";
        }

        private static string RarityKorean(Rarity r)
        {
            switch (r)
            {
                case Rarity.Common:    return "일반";
                case Rarity.Magic:     return "고급";
                case Rarity.Rare:      return "희귀";
                case Rarity.Epic:      return "영웅";
                case Rarity.Legendary: return "전설";
                case Rarity.Mythic:    return "신화";
                default: return r.ToString();
            }
        }
    }
}
