using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT D-가챠 1 — 무기 소환 패널 (gacha_v1.md §5.1).
    ///
    /// 단판·10연차 버튼 + 화폐 표시(별빛 ⋆ / 무기 소환권) + 소환 레벨 게이지 + 등급 분포.
    /// 결과 연출은 GachaResultPanel 로 위임.
    /// </summary>
    public class GachaPanel : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GachaPool _pool;
        [SerializeField] private SlotAffixPool[] _slotPools;
        [SerializeField] private GlobalSubAffixPool _globalPool;

        [Header("연결")]
        [SerializeField] private GachaResultPanel _resultPanel;
        [SerializeField] private Button _closeButton;

        [Header("화폐 표시")]
        [SerializeField] private Text _starlightText;       // 별빛 ⋆
        [SerializeField] private Text _ticketText;          // 무기 소환권

        [Header("소환 레벨")]
        [SerializeField] private Text _summonLevelText;     // 소환 레벨 ⭐ N
        [SerializeField] private Slider _summonLevelGauge;
        [SerializeField] private Text _summonLevelProgressText;  // 누적 / 다음 임계

        [Header("등급 분포")]
        [SerializeField] private Text _rarityDistributionText;

        [Header("굴림 버튼")]
        [SerializeField] private Button _singleButton;
        [SerializeField] private Text _singleCostText;
        [SerializeField] private Button _multiButton;       // 10연차
        [SerializeField] private Text _multiCostText;

        [Header("화폐 모드")]
        [SerializeField] private GachaCurrency _currency = GachaCurrency.Starlight;

        private void Awake()
        {
            // 부트스트랩 — 미초기화 시 자동 호출
            if (!GachaService.IsInitialized)
                GachaService.Initialize(_slotPools, _globalPool);
        }

        private void OnEnable()
        {
            if (_singleButton != null) _singleButton.onClick.AddListener(OnSingleClicked);
            if (_multiButton  != null) _multiButton.onClick.AddListener(OnMultiClicked);
            if (_closeButton  != null) _closeButton.onClick.AddListener(Close);

            GameEvents.OnStarlightChanged              += HandleStarlightChanged;
            GameEvents.OnWeaponSummonTicketsChanged    += HandleTicketsChanged;
            GameEvents.OnSummonLevelUp                 += HandleSummonLevelUp;

            Refresh();
        }

        private void OnDisable()
        {
            if (_singleButton != null) _singleButton.onClick.RemoveAllListeners();
            if (_multiButton  != null) _multiButton.onClick.RemoveAllListeners();
            if (_closeButton  != null) _closeButton.onClick.RemoveAllListeners();

            GameEvents.OnStarlightChanged              -= HandleStarlightChanged;
            GameEvents.OnWeaponSummonTicketsChanged    -= HandleTicketsChanged;
            GameEvents.OnSummonLevelUp                 -= HandleSummonLevelUp;
        }

        private void OnSingleClicked()
        {
            if (_pool == null) return;
            if (!GachaService.CanRollSingle(_pool, _currency))
            {
                ShowInsufficientWarning();
                return;
            }
            var result = GachaService.RollSingle(_pool, _currency);
            if (result != null && _resultPanel != null)
                _resultPanel.ShowSingle(result);
            Refresh();
        }

        private void OnMultiClicked()
        {
            if (_pool == null) return;
            if (!GachaService.CanRoll10x(_pool, _currency))
            {
                ShowInsufficientWarning();
                return;
            }
            var results = GachaService.Roll10x(_pool, _currency);
            if (results != null && _resultPanel != null)
                _resultPanel.Show10x(results);
            Refresh();
        }

        private void Refresh()
        {
            HandleStarlightChanged(CurrencyService.Starlight);
            HandleTicketsChanged(GachaService.GetTickets());
            UpdateSummonLevel();
            UpdateRarityDistribution();
            UpdateButtonState();
        }

        private void UpdateSummonLevel()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || _pool == null) return;
            int level = data.summonLevel;
            int total = data.totalGachaCount;
            int next  = GachaService.GetNextLevelThreshold(_pool, level);

            if (_summonLevelText != null)
                _summonLevelText.text = $"소환 레벨 ⭐ {level}";

            if (_summonLevelGauge != null)
            {
                if (next < 0)
                {
                    _summonLevelGauge.value = 1f;
                }
                else
                {
                    int prev = level >= 1 && level - 1 < _pool.summonLevelThresholds.Length
                        ? _pool.summonLevelThresholds[level - 1] : 0;
                    int span = Mathf.Max(1, next - prev);
                    _summonLevelGauge.value = Mathf.Clamp01((float)(total - prev) / span);
                }
            }

            if (_summonLevelProgressText != null)
            {
                _summonLevelProgressText.text = next < 0
                    ? $"{total} / 최대 단계"
                    : $"{total} / {next}";
            }
        }

        private void UpdateRarityDistribution()
        {
            if (_rarityDistributionText == null || _pool == null) return;
            var data = MetaProgression.Instance?.Data;
            int level = data != null ? data.summonLevel : 1;
            var prob = GachaService.GetProbability(_pool, level);
            if (prob == null) { _rarityDistributionText.text = ""; return; }

            _rarityDistributionText.text =
                $"일반 {prob.commonRate * 100:F1}%   "
              + $"고급 {prob.magicRate * 100:F1}%   "
              + $"희귀 {prob.rareRate * 100:F1}%   "
              + $"영웅 {prob.epicRate * 100:F1}%";
        }

        private void UpdateButtonState()
        {
            if (_pool == null) return;
            if (_singleCostText != null)
                _singleCostText.text = _currency == GachaCurrency.Starlight
                    ? $"단판 ⋆ {_pool.starlightCostSingle}"
                    : "단판 (소환권 ×1)";
            if (_multiCostText != null)
                _multiCostText.text = _currency == GachaCurrency.Starlight
                    ? $"10연차 ⋆ {_pool.starlightCost10Pull} (희귀+ 보장)"
                    : "10연차 (소환권 ×10, 희귀+ 보장)";

            if (_singleButton != null) _singleButton.interactable = GachaService.CanRollSingle(_pool, _currency);
            if (_multiButton  != null) _multiButton.interactable  = GachaService.CanRoll10x(_pool, _currency);
        }

        private void HandleStarlightChanged(int amount)
        {
            if (_starlightText != null) _starlightText.text = $"별빛 ⋆ {amount}";
            UpdateButtonState();
        }

        private void HandleTicketsChanged(int amount)
        {
            if (_ticketText != null) _ticketText.text = $"소환권 ×{amount}";
            UpdateButtonState();
        }

        private void HandleSummonLevelUp(int newLevel)
        {
            UpdateSummonLevel();
            UpdateRarityDistribution();
        }

        private void ShowInsufficientWarning()
        {
            // 단순화 — 콘솔 로그만. 후속: 토스트 알림 패널.
            Debug.Log("[GachaPanel] 화폐 부족 — 굴림 거부");
        }

        public void Close() => gameObject.SetActive(false);
    }
}
