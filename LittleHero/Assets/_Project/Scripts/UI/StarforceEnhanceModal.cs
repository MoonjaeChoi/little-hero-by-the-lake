using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// PIVOT D-가챠 4 — 슬롯 강화 모달 (gacha_v1.md §5.2).
    ///
    /// 현재 슬롯의 ⭐ 단계 + 다음 단계 효과 + 비용 + 성공 확률 + 강화 버튼.
    /// 안전 강화 — 실패 시 비용만 차감, 단계 그대로.
    /// </summary>
    public class StarforceEnhanceModal : MonoBehaviour
    {
        [Header("표시")]
        [SerializeField] private Text _titleText;          // 슬롯 이름
        [SerializeField] private Text _currentLevelText;   // ⭐ N
        [SerializeField] private Text _nextEffectText;     // base +N% 강화
        [SerializeField] private Text _costText;           // 비용 ◈ N
        [SerializeField] private Text _successRateText;    // 성공 확률 N%
        [SerializeField] private Text _resultText;         // 결과 (성공/실패/잔액 부족)

        [Header("버튼")]
        [SerializeField] private Button _enhanceButton;
        [SerializeField] private Button _closeButton;

        private EquipSlot _slot;

        private void OnEnable()
        {
            if (_enhanceButton != null) _enhanceButton.onClick.AddListener(HandleEnhance);
            if (_closeButton   != null) _closeButton.onClick.AddListener(Close);
            // 5033.3 — Shards/SlotLevel 외부 변경 시 자동 Refresh (메뉴 충전 / 가챠 결과 / 다른 슬롯 강화)
            GameEvents.OnStarforceShardsChanged += HandleShardsChanged;
            GameEvents.OnSlotStarforceChanged   += HandleSlotChanged;
        }

        private void OnDisable()
        {
            if (_enhanceButton != null) _enhanceButton.onClick.RemoveAllListeners();
            if (_closeButton   != null) _closeButton.onClick.RemoveAllListeners();
            GameEvents.OnStarforceShardsChanged -= HandleShardsChanged;
            GameEvents.OnSlotStarforceChanged   -= HandleSlotChanged;
        }

        private void HandleShardsChanged(int amount) => Refresh();
        private void HandleSlotChanged(int slotIdx, int newLevel) => Refresh();

        public void Show(EquipSlot slot)
        {
            _slot = slot;
            gameObject.SetActive(true);
            transform.SetAsLastSibling();  // 강화 모달은 SlotStarforcePanel 위에 떠야 함
            if (_resultText != null) _resultText.text = "";
            Refresh();
        }

        private void Refresh()
        {
            int lv = StarforceService.GetLevel(_slot);
            bool maxed = lv >= StarforceService.MAX_LEVEL;
            int cost = StarforceService.GetCostForLevel(lv);
            float rate = StarforceService.GetSuccessRateForLevel(lv);
            float nextMul = StarforceService.GetBaseMultiplierForLevel(lv + 1);

            if (_titleText != null) _titleText.text = $"슬롯 강화 — {SlotLabel(_slot)}";
            // 5034 — 권장 ⭐ 표시 (BalanceConfig 기반, 현재 챕터)
            int recommended = GetRecommendedStarforce();
            if (_currentLevelText != null)
            {
                _currentLevelText.text = (recommended > 0 && lv < recommended)
                    ? $"⭐ {lv} / {StarforceService.MAX_LEVEL} <color=#aaa>(권장 ⭐{recommended})</color>"
                    : $"⭐ {lv} / {StarforceService.MAX_LEVEL}";
            }
            if (_nextEffectText != null)
            {
                _nextEffectText.text = maxed
                    ? "최대 단계 도달"
                    : $"강화 후: 베이스 +{Mathf.RoundToInt(nextMul * 100f)}%";
            }
            if (_costText != null) _costText.text = maxed ? "" : $"비용: ◈ {cost}";
            if (_successRateText != null) _successRateText.text = maxed ? "" : $"성공 확률: {Mathf.RoundToInt(rate * 100f)}%";

            if (_enhanceButton != null)
            {
                bool canEnhance = !maxed && StarforceService.Shards >= cost;
                // 5033.3 진단 로그 (검증 후 제거)
                Debug.Log($"[StarforceEnhanceModal.Refresh] slot={_slot} lv={lv} cost={cost} Shards={StarforceService.Shards} maxed={maxed} canEnhance={canEnhance}");
                _enhanceButton.interactable = canEnhance;
            }
        }

        private void HandleEnhance()
        {
            var result = StarforceService.TryEnhance(_slot);
            if (result == null) return;

            if (_resultText != null)
            {
                _resultText.text = result.outcome switch
                {
                    StarforceOutcome.Success           => $"✦ 성공! ⭐ {result.previousLevel} → ⭐ {result.newLevel}",
                    StarforceOutcome.Failure           => $"실패… 단계 그대로 ⭐ {result.previousLevel} (재도전 가능)",
                    StarforceOutcome.AtMaxLevel        => "이미 최대 단계입니다",
                    StarforceOutcome.InsufficientShards => $"조각 부족 — 필요: ◈ {result.shardCost}",
                    StarforceOutcome.InvalidSaveData   => "저장 데이터 오류",
                    _ => "",
                };
            }
            Refresh();
        }

        public void Close() => gameObject.SetActive(false);

        // 5034 — BalanceConfig.recommendedSlotStarforceByChapter[챕터-1] 조회
        private int GetRecommendedStarforce()
        {
            var cfg = GameManager.Instance?.Balance;
            if (cfg == null || cfg.recommendedSlotStarforceByChapter == null) return 0;
            int chapter = MetaProgressionChapter.SelectedChapter?.chapterNumber ?? 1;
            int idx = Mathf.Clamp(chapter - 1, 0, cfg.recommendedSlotStarforceByChapter.Length - 1);
            return cfg.recommendedSlotStarforceByChapter[idx];
        }

        private static string SlotLabel(EquipSlot s) => s switch
        {
            EquipSlot.Hat => "모자",
            EquipSlot.Top => "상의",
            EquipSlot.Bottom => "하의",
            EquipSlot.Cloak => "망토",
            EquipSlot.Gloves => "장갑",
            EquipSlot.Weapon => "무기",
            EquipSlot.Shoes => "신발",
            EquipSlot.Earring => "귀걸이",
            EquipSlot.Glasses => "안경",
            EquipSlot.Necklace => "목걸이",
            EquipSlot.Ring => "반지",
            EquipSlot.Pet => "애완동물",
            _ => s.ToString(),
        };
    }
}
