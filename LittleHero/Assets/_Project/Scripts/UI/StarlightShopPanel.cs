using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D9 — 별빛 상점 (monetization §8). E1+E2 의존 해제 후 Phase E 사이클에서 추가.
    /// 두 영역:
    ///  1. **별빛 패키지 구매** (실금 → ⋆) — IAPService.InitiatePurchase
    ///  2. **별빛 사용처** (⋆ 차감 → 마법 결정 ◇ / 강화 보장권 / 영구 잠금 등)
    ///
    /// MVP: 별빛 → 마법 결정 직구매(5⋆/22⋆/40⋆) + 패키지 5종 표시.
    /// 강화 보장권·영구 잠금 등은 인스턴스 컨텍스트 필요 — D5 카드 카드에서 직접 호출 (후속 wiring).
    /// </summary>
    public class StarlightShopPanel : MonoBehaviour
    {
        [Header("화폐 표시")]
        [SerializeField] private Text _starlightText;       // ⋆
        [SerializeField] private Text _magicCrystalText;    // ◇

        [Header("별빛 → 마법 결정 (monetization §4.1)")]
        [SerializeField] private Button _crystal1Button;    // 5 ⋆
        [SerializeField] private Button _crystal5Button;    // 22 ⋆
        [SerializeField] private Button _crystal10Button;   // 40 ⋆
        [SerializeField] private Text _crystal1PriceText;
        [SerializeField] private Text _crystal5PriceText;
        [SerializeField] private Text _crystal10PriceText;

        [Header("별빛 패키지 (실금)")]
        [SerializeField] private Button _pkg100Button;
        [SerializeField] private Button _pkg500Button;
        [SerializeField] private Button _pkg1100Button;
        [SerializeField] private Button _pkg3500Button;
        [SerializeField] private Button _pkg12000Button;
        [SerializeField] private Text _pkg100PriceText;
        [SerializeField] private Text _pkg500PriceText;
        [SerializeField] private Text _pkg1100PriceText;
        [SerializeField] private Text _pkg3500PriceText;
        [SerializeField] private Text _pkg12000PriceText;

        [Header("닫기")]
        [SerializeField] private Button _closeButton;

        private const int CRYSTAL_1_COST  = 5;
        private const int CRYSTAL_5_COST  = 22;
        private const int CRYSTAL_10_COST = 40;

        private void OnEnable()
        {
            // 별빛 → 마법 결정
            if (_crystal1Button != null)  _crystal1Button.onClick.AddListener(() => BuyCrystal(1, CRYSTAL_1_COST));
            if (_crystal5Button != null)  _crystal5Button.onClick.AddListener(() => BuyCrystal(5, CRYSTAL_5_COST));
            if (_crystal10Button != null) _crystal10Button.onClick.AddListener(() => BuyCrystal(10, CRYSTAL_10_COST));

            // 별빛 패키지
            if (_pkg100Button != null)   _pkg100Button.onClick.AddListener(() => BuyPackage(IAPService.ID_STARLIGHT_100));
            if (_pkg500Button != null)   _pkg500Button.onClick.AddListener(() => BuyPackage(IAPService.ID_STARLIGHT_500));
            if (_pkg1100Button != null)  _pkg1100Button.onClick.AddListener(() => BuyPackage(IAPService.ID_STARLIGHT_1100));
            if (_pkg3500Button != null)  _pkg3500Button.onClick.AddListener(() => BuyPackage(IAPService.ID_STARLIGHT_3500));
            if (_pkg12000Button != null) _pkg12000Button.onClick.AddListener(() => BuyPackage(IAPService.ID_STARLIGHT_12000));

            if (_closeButton != null) _closeButton.onClick.AddListener(Close);

            GameEvents.OnStarlightChanged    += _ => Refresh();
            GameEvents.OnMagicCrystalChanged += _ => Refresh();
            Refresh();
        }

        private void OnDisable()
        {
            if (_crystal1Button != null) _crystal1Button.onClick.RemoveAllListeners();
            if (_crystal5Button != null) _crystal5Button.onClick.RemoveAllListeners();
            if (_crystal10Button != null) _crystal10Button.onClick.RemoveAllListeners();
            if (_pkg100Button != null)   _pkg100Button.onClick.RemoveAllListeners();
            if (_pkg500Button != null)   _pkg500Button.onClick.RemoveAllListeners();
            if (_pkg1100Button != null)  _pkg1100Button.onClick.RemoveAllListeners();
            if (_pkg3500Button != null)  _pkg3500Button.onClick.RemoveAllListeners();
            if (_pkg12000Button != null) _pkg12000Button.onClick.RemoveAllListeners();
            if (_closeButton != null)    _closeButton.onClick.RemoveAllListeners();
        }

        private static void BuyCrystal(int amount, int starlightCost)
        {
            if (CurrencyService.Starlight < starlightCost) return;
            if (!CurrencyService.SpendStarlight(starlightCost)) return;
            CurrencyService.GrantMagicCrystal(amount);
            Debug.Log($"[StarlightShop] -{starlightCost}⋆ → +{amount}◇");
        }

        private static void BuyPackage(string productId)
        {
            if (IAPService.Instance == null || !IAPService.Instance.IsInitialized)
            {
                Debug.LogError("[StarlightShop] IAP 미초기화");
                return;
            }
            IAPService.Instance.InitiatePurchase(productId);
        }

        private void Refresh()
        {
            int starlight = CurrencyService.Starlight;
            int crystal = CurrencyService.MagicCrystals;

            if (_starlightText != null)    _starlightText.text    = $"⋆ {starlight}";
            if (_magicCrystalText != null) _magicCrystalText.text = $"◇ {crystal}";

            // 별빛 → 마법 결정 비용·활성화
            SetPriceAndState(_crystal1PriceText, _crystal1Button,  $"마법 결정 ◇ 1 — ⋆ {CRYSTAL_1_COST}",  starlight >= CRYSTAL_1_COST);
            SetPriceAndState(_crystal5PriceText, _crystal5Button,  $"마법 결정 ◇ 5 — ⋆ {CRYSTAL_5_COST}",  starlight >= CRYSTAL_5_COST);
            SetPriceAndState(_crystal10PriceText, _crystal10Button,$"마법 결정 ◇ 10 — ⋆ {CRYSTAL_10_COST}", starlight >= CRYSTAL_10_COST);

            // 패키지 가격 — IAP 에서 로컬라이즈된 가격 (없으면 기본 한국 원화)
            SetPackagePrice(_pkg100PriceText,   IAPService.ID_STARLIGHT_100,   "100⋆ — 1,200원");
            SetPackagePrice(_pkg500PriceText,   IAPService.ID_STARLIGHT_500,   "500+50⋆ — 5,500원");
            SetPackagePrice(_pkg1100PriceText,  IAPService.ID_STARLIGHT_1100,  "1,100+120⋆ — 11,000원");
            SetPackagePrice(_pkg3500PriceText,  IAPService.ID_STARLIGHT_3500,  "3,500+400⋆ — 33,000원");
            SetPackagePrice(_pkg12000PriceText, IAPService.ID_STARLIGHT_12000, "12,000+1,500⋆ — 110,000원");
        }

        private static void SetPriceAndState(Text text, Button button, string label, bool affordable)
        {
            if (text != null) text.text = label;
            if (button != null) button.interactable = affordable;
        }

        private static void SetPackagePrice(Text text, string productId, string fallback)
        {
            if (text == null) return;
            string localized = IAPService.Instance?.GetLocalizedPrice(productId);
            text.text = !string.IsNullOrEmpty(localized) ? localized : fallback;
        }

        public void Close() => gameObject.SetActive(false);
    }
}
