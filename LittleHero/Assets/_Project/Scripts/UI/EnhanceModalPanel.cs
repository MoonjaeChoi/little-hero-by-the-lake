using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D6 — 강화 모달 (+0~+25, ui_v1 §9.3 + economy §10.3).
    /// 현재 강화 레벨·다음 단계 비용(★/◇)·다음 시도 성공률·천장 게이지 표시.
    /// 강화 버튼 클릭 시 EnhancementService.TryEnhance 호출 → 결과 텍스트.
    /// </summary>
    public class EnhanceModalPanel : MonoBehaviour
    {
        [Header("아이템 정보")]
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _enhanceLevelText;     // "+12 → +13"

        [Header("비용·확률·천장")]
        [SerializeField] private Text _dustCostText;         // "별가루 ★ 250"
        [SerializeField] private Text _crystalCostText;      // "마법 결정 ◇ 3" (+15 미만 시 비활성)
        [SerializeField] private GameObject _crystalCostRow; // ◇ 표시 행 (선택, 비결정론 단계에서만 활성)
        [SerializeField] private Text _successRateText;      // "성공률 50%"
        [SerializeField] private Slider _pityGauge;          // failCount / 20
        [SerializeField] private Text _pityGaugeText;        // "5 / 20"
        [SerializeField] private Text _resultText;           // 마지막 시도 결과

        [Header("버튼")]
        [SerializeField] private Button _enhanceButton;
        [SerializeField] private Button _closeButton;

        private EquipmentInstance _instance;
        private EquipmentTemplate _template;
        private BalanceConfig _config;
        private Action _onChanged;

        public void Show(EquipmentInstance instance, EquipmentTemplate template, BalanceConfig config, Action onChanged = null)
        {
            _instance = instance;
            _template = template;
            _config = config;
            _onChanged = onChanged;
            gameObject.SetActive(true);
            Refresh();
        }

        private void OnEnable()
        {
            if (_enhanceButton != null) _enhanceButton.onClick.AddListener(OnEnhance);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            GameEvents.OnStarDustChanged     += _ => Refresh();
            GameEvents.OnMagicCrystalChanged += _ => Refresh();
        }

        private void OnDisable()
        {
            if (_enhanceButton != null) _enhanceButton.onClick.RemoveAllListeners();
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
        }

        private void OnEnhance()
        {
            if (_instance == null || _config == null) return;
            var result = EnhancementService.TryEnhance(_instance, _config);
            DescribeResult(result);
            _onChanged?.Invoke();
            Refresh();
        }

        private void DescribeResult(EnhanceResult result)
        {
            if (_resultText == null) return;
            _resultText.text = result.outcome switch
            {
                EnhanceOutcome.Success            => result.wasPityTriggered
                                                       ? $"+{result.newLevel} 성공! (아빠가 도와주셨어요)"
                                                       : $"+{result.newLevel} 성공!",
                EnhanceOutcome.Failure            => "별가루가 흩어졌어요…",
                EnhanceOutcome.AtMaxLevel         => "더 이상 강해질 수 없어요",
                EnhanceOutcome.InsufficientDust   => "별가루가 부족해요",
                EnhanceOutcome.InsufficientCrystal=> "마법 결정이 부족해요",
                EnhanceOutcome.InvalidInstance    => "장비를 불러올 수 없어요",
                _ => string.Empty,
            };
        }

        private void Refresh()
        {
            if (_instance == null || _config == null) { gameObject.SetActive(false); return; }

            int level = _instance.enhanceLevel;
            int max = _template != null ? _template.maxEnhanceLevel : EnhancementService.MAX_ENHANCE_LEVEL;
            int dust = EnhancementService.GetDustCost(level, _config);
            int crystal = EnhancementService.GetCrystalCost(level, _config);
            float p = EnhancementService.CalcSuccessRate(level, _instance.failCount);

            if (_itemNameText != null)
                _itemNameText.text = _template != null ? _template.displayName : "(템플릿 없음)";
            if (_enhanceLevelText != null)
                _enhanceLevelText.text = level >= max ? $"+{level} (MAX)" : $"+{level} → +{level + 1}";
            if (_dustCostText != null)
                _dustCostText.text = $"별가루 ★ {dust:N0}";
            if (_crystalCostText != null)
                _crystalCostText.text = $"마법 결정 ◇ {crystal}";
            if (_crystalCostRow != null)
                _crystalCostRow.SetActive(crystal > 0);
            if (_successRateText != null)
                _successRateText.text = level < EnhancementService.DETERMINISTIC_LEVEL_CAP
                    ? "성공률 100% (확정)"
                    : $"성공률 {p * 100f:F1}%";
            if (_pityGauge != null)
                _pityGauge.value = Mathf.Clamp01((float)_instance.failCount / EnhancementService.HARD_PITY_FAILS);
            if (_pityGaugeText != null)
                _pityGaugeText.text = $"확정까지 {_instance.failCount} / {EnhancementService.HARD_PITY_FAILS}";

            bool maxed = level >= max;
            if (_enhanceButton != null) _enhanceButton.interactable = !maxed;
        }

        public void Close()
        {
            _instance = null;
            _template = null;
            _onChanged = null;
            gameObject.SetActive(false);
        }
    }
}
