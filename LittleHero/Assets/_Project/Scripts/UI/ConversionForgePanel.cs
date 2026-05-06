using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D7 — 변환 모루 UI (economy_v1 §11.3).
    /// 별가루 ★ → 마법 결정 ◇ (1,000:1) / 무지개 구슬 ✦ → 마법 결정 ◇ (5:1).
    /// 사용자가 수량 버튼 (×1, ×5) 을 누르면 ConversionForge 가 실제 차감 + 적립.
    /// </summary>
    public class ConversionForgePanel : MonoBehaviour
    {
        [Header("화폐 표시")]
        [SerializeField] private Text _starDustText;        // ★
        [SerializeField] private Text _starFragmentText;    // ✦
        [SerializeField] private Text _magicCrystalText;    // ◇

        [Header("별가루 → 마법 결정 (1,000 ★ : 1 ◇)")]
        [SerializeField] private Button _dustX1Button;
        [SerializeField] private Button _dustX5Button;
        [SerializeField] private Text _dustResultText;

        [Header("무지개 구슬 → 마법 결정 (5 ✦ : 1 ◇)")]
        [SerializeField] private Button _orbX1Button;
        [SerializeField] private Button _orbX5Button;
        [SerializeField] private Text _orbResultText;

        [Header("닫기")]
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            if (_dustX1Button != null) _dustX1Button.onClick.AddListener(() => OnConvertDust(1));
            if (_dustX5Button != null) _dustX5Button.onClick.AddListener(() => OnConvertDust(5));
            if (_orbX1Button != null) _orbX1Button.onClick.AddListener(() => OnConvertOrb(1));
            if (_orbX5Button != null) _orbX5Button.onClick.AddListener(() => OnConvertOrb(5));
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);

            GameEvents.OnStarDustChanged       += _ => Refresh();
            GameEvents.OnMagicCrystalChanged   += _ => Refresh();
            if (MetaProgression.Instance != null)
                MetaProgression.OnStarFragmentsChanged += _ => Refresh();

            Refresh();
        }

        private void OnDisable()
        {
            if (_dustX1Button != null) _dustX1Button.onClick.RemoveAllListeners();
            if (_dustX5Button != null) _dustX5Button.onClick.RemoveAllListeners();
            if (_orbX1Button != null) _orbX1Button.onClick.RemoveAllListeners();
            if (_orbX5Button != null) _orbX5Button.onClick.RemoveAllListeners();
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }

        private void OnConvertDust(int crystalCount)
        {
            if (ConversionForge.ConvertDustToCrystal(crystalCount))
                Refresh();
        }

        private void OnConvertOrb(int crystalCount)
        {
            if (ConversionForge.ConvertOrbToCrystal(crystalCount))
                Refresh();
        }

        private void Refresh()
        {
            int dust = GameManager.Instance?.StarDust ?? 0;
            int orb = CurrencyService.StarFragments;
            int crystal = CurrencyService.MagicCrystals;

            if (_starDustText != null)     _starDustText.text     = $"★ {dust:N0}";
            if (_starFragmentText != null) _starFragmentText.text = $"✦ {orb}";
            if (_magicCrystalText != null) _magicCrystalText.text = $"◇ {crystal}";

            // 버튼 활성/비활성
            if (_dustX1Button != null) _dustX1Button.interactable = dust >= ConversionForge.CalcDustCost(1);
            if (_dustX5Button != null) _dustX5Button.interactable = dust >= ConversionForge.CalcDustCost(5);
            if (_orbX1Button != null)  _orbX1Button.interactable  = orb >= ConversionForge.CalcOrbCost(1);
            if (_orbX5Button != null)  _orbX5Button.interactable  = orb >= ConversionForge.CalcOrbCost(5);

            if (_dustResultText != null) _dustResultText.text = $"{ConversionForge.DUST_PER_CRYSTAL:N0} ★ → 1 ◇";
            if (_orbResultText != null)  _orbResultText.text  = $"{ConversionForge.ORB_PER_CRYSTAL} ✦ → 1 ◇";
        }

        public void Close() => gameObject.SetActive(false);
    }
}
