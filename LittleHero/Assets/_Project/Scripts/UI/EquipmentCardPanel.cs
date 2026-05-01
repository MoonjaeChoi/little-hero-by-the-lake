using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// D5 — 장비 카드 6단 풀스크린 (ui_v1 §9 + item_drop §7).
    /// 1. 정체성(아이콘 + 이름 + 강화 레벨) / 2. 베이스 스탯 / 3. 강화 영역(EnhanceModalPanel 연결)
    /// 4. 주옵션 3 (보라) / 5. 부옵션 3 (청록) / 6. 신화 영역.
    /// 잠금 토글·리롤 버튼은 별도 row prefab(EquipmentCardRow) 가 처리.
    /// </summary>
    public class EquipmentCardPanel : MonoBehaviour
    {
        [Header("1. 정체성")]
        [SerializeField] private Image _itemIcon;
        [SerializeField] private Text _itemNameText;
        [SerializeField] private Text _enhanceBadgeText;       // "+12"
        [SerializeField] private Text _historyText;            // "총 강화 17회 / 천장 0 / 신화 0"

        [Header("2. 베이스 스탯")]
        [SerializeField] private Text _baseStatsText;          // multi-line summary

        [Header("3. 강화 진입 버튼")]
        [SerializeField] private Button _openEnhanceButton;
        [SerializeField] private EnhanceModalPanel _enhanceModal;
        [SerializeField] private BalanceConfig _balanceConfig;

        [Header("4. 주옵션 (보라 배경)")]
        [SerializeField] private RectTransform _mainAffixRowRoot;
        [SerializeField] private GameObject _mainAffixRowPrefab; // EquipmentCardRow
        [SerializeField] private Button _rerollMainButton;
        [SerializeField] private Text _rerollMainCostText;

        [Header("5. 부옵션 (청록 배경)")]
        [SerializeField] private RectTransform _subAffixRowRoot;
        [SerializeField] private GameObject _subAffixRowPrefab;
        [SerializeField] private Button _rerollSubButton;
        [SerializeField] private Text _rerollSubCostText;

        [Header("6. 신화 영역")]
        [SerializeField] private GameObject _mythicRoot;
        [SerializeField] private Text _mythicNameText;
        [SerializeField] private Text _mythicFlavorText;

        [Header("리롤 풀 의존성")]
        [SerializeField] private SlotAffixPool[] _slotPools;
        [SerializeField] private GlobalSubAffixPool _globalSubPool;
        [SerializeField] private MythicDefinition[] _mythicLookup;

        [Header("닫기")]
        [SerializeField] private Button _closeButton;

        private EquipmentInstance _instance;
        private EquipmentTemplate _template;
        private readonly List<EquipmentCardRow> _mainRows = new List<EquipmentCardRow>();
        private readonly List<EquipmentCardRow> _subRows  = new List<EquipmentCardRow>();

        public void Show(EquipmentInstance instance, EquipmentTemplate template)
        {
            _instance = instance;
            _template = template;
            // F8: 카드 진입 시 약속 무료 리롤 토큰 재충전 (인스턴스별 세션 1회)
            MythicEffectRuntime.ResetPromiseFreeReroll();
            gameObject.SetActive(true);
            Refresh();
        }

        private void OnEnable()
        {
            if (_openEnhanceButton != null) _openEnhanceButton.onClick.AddListener(OpenEnhance);
            if (_rerollMainButton != null) _rerollMainButton.onClick.AddListener(RerollMain);
            if (_rerollSubButton != null) _rerollSubButton.onClick.AddListener(RerollSub);
            if (_closeButton != null) _closeButton.onClick.AddListener(Close);
            GameEvents.OnAffixRolled += OnAffixRolled;
            GameEvents.OnStarDustChanged += _ => Refresh();
        }

        private void OnDisable()
        {
            if (_openEnhanceButton != null) _openEnhanceButton.onClick.RemoveAllListeners();
            if (_rerollMainButton != null) _rerollMainButton.onClick.RemoveAllListeners();
            if (_rerollSubButton != null) _rerollSubButton.onClick.RemoveAllListeners();
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            GameEvents.OnAffixRolled -= OnAffixRolled;
        }

        private void OnAffixRolled(EquipmentInstance i)
        {
            if (i == _instance) Refresh();
        }

        private void OpenEnhance()
        {
            if (_enhanceModal == null || _instance == null || _balanceConfig == null) return;
            _enhanceModal.Show(_instance, _template, _balanceConfig, Refresh);
        }

        private void RerollMain()
        {
            if (_instance == null || _template == null) return;
            var pool = FindSlotPool(_template.slot);
            if (pool == null) return;
            int cost = AffixRoller.CalcRerollCostMain(_instance, _balanceConfig);
            // F8: mythic_dad_promise 발현 시 첫 리롤 1회 무료 (equipment §3.7)
            bool freePromise = MythicEffectRuntime.TryConsumePromiseFreeReroll();
            if (!AffixRoller.CanRerollMain(_instance)) return;
            if (!freePromise && !TrySpendDust(cost)) return;
            AffixRoller.RerollMain(_instance, pool, _template.rarity, null);
            Refresh();
        }

        private void RerollSub()
        {
            if (_instance == null || _template == null || _globalSubPool == null) return;
            int cost = AffixRoller.CalcRerollCostSub(_instance, _balanceConfig);
            bool freePromise = MythicEffectRuntime.TryConsumePromiseFreeReroll();
            if (!AffixRoller.CanRerollSub(_instance)) return;
            if (!freePromise && !TrySpendDust(cost)) return;
            AffixRoller.RerollSub(_instance, _globalSubPool, _template.rarity, null);
            Refresh();
        }

        private static bool TrySpendDust(int cost)
            => GameManager.Instance != null && GameManager.Instance.SpendStarDust(cost);

        private SlotAffixPool FindSlotPool(EquipSlot slot)
        {
            if (_slotPools == null) return null;
            for (int i = 0; i < _slotPools.Length; i++)
                if (_slotPools[i] != null && _slotPools[i].slot == slot) return _slotPools[i];
            return null;
        }

        private void Refresh()
        {
            if (_instance == null || _template == null) { gameObject.SetActive(false); return; }

            // 1. 정체성
            if (_itemIcon != null && _template.icon != null) _itemIcon.sprite = _template.icon;
            if (_itemNameText != null) _itemNameText.text = _template.displayName;
            if (_enhanceBadgeText != null) _enhanceBadgeText.text = $"+{_instance.enhanceLevel}";
            var h = _instance.history ?? new InstanceHistory();
            if (_historyText != null)
                _historyText.text = $"총 강화 {h.totalEnhanceAttempts}회 / 천장 {h.pityTriggeredCount} / 신화 {h.mythicAttempts}";

            // 2. 베이스 스탯
            if (_baseStatsText != null)
                _baseStatsText.text = FormatBaseStats(_template.baseStats);

            // 4. 주옵션
            BindAffixRows(_mainRows, _mainAffixRowRoot, _mainAffixRowPrefab, _instance.mainAffixes, isSub: false);
            // 5. 부옵션
            BindAffixRows(_subRows, _subAffixRowRoot, _subAffixRowPrefab, _instance.subAffixes, isSub: true);

            int rerollMainCost = AffixRoller.CalcRerollCostMain(_instance, _balanceConfig);
            int rerollSubCost = AffixRoller.CalcRerollCostSub(_instance, _balanceConfig);
            if (_rerollMainCostText != null) _rerollMainCostText.text = $"리롤 ★ {rerollMainCost}";
            if (_rerollSubCostText != null)  _rerollSubCostText.text  = $"리롤 ★ {rerollSubCost}";
            if (_rerollMainButton != null)
                _rerollMainButton.interactable = AffixRoller.CanRerollMain(_instance) && (GameManager.Instance?.StarDust ?? 0) >= rerollMainCost;
            if (_rerollSubButton != null)
                _rerollSubButton.interactable  = AffixRoller.CanRerollSub(_instance)  && (GameManager.Instance?.StarDust ?? 0) >= rerollSubCost;

            // 6. 신화
            string mythicId = _instance.mythicId;
            bool hasMythic = !string.IsNullOrEmpty(mythicId);
            if (_mythicRoot != null) _mythicRoot.SetActive(hasMythic);
            if (hasMythic)
            {
                var def = FindMythic(mythicId);
                if (_mythicNameText != null) _mythicNameText.text = def != null ? def.displayName : mythicId;
                if (_mythicFlavorText != null) _mythicFlavorText.text = def != null ? def.flavorText : "아빠의 마법";
            }
        }

        private static string FormatBaseStats(StatModifiers s)
        {
            if (s == null) return "";
            var lines = new List<string>(6);
            if (s.maxHealthBonus > 0)    lines.Add($"체력 +{s.maxHealthBonus:F0}");
            if (s.attackDamageBonus > 0) lines.Add($"공격력 +{s.attackDamageBonus:F1}");
            if (s.attackSpeedBonus > 0)  lines.Add($"공격 속도 +{s.attackSpeedBonus * 100f:F1}%");
            if (s.speedBonus > 0)        lines.Add($"이동 속도 +{s.speedBonus:F2}");
            if (s.rangeBonus > 0)        lines.Add($"사거리 +{s.rangeBonus:F1}");
            if (s.xpGainBonus > 0)       lines.Add($"XP +{s.xpGainBonus * 100f:F0}%");
            return lines.Count == 0 ? "(베이스 스탯 없음)" : string.Join("\n", lines);
        }

        private MythicDefinition FindMythic(string id)
        {
            if (_mythicLookup == null) return null;
            for (int i = 0; i < _mythicLookup.Length; i++)
                if (_mythicLookup[i] != null && _mythicLookup[i].id == id) return _mythicLookup[i];
            return null;
        }

        private void BindAffixRows(List<EquipmentCardRow> rows, RectTransform root, GameObject prefab,
                                    AffixLine[] lines, bool isSub)
        {
            if (root == null || prefab == null || lines == null) return;
            // pool 단순화: 부족하면 추가 생성, 남으면 비활성
            while (rows.Count < lines.Length)
            {
                var go = Instantiate(prefab, root);
                rows.Add(go.GetComponent<EquipmentCardRow>());
            }
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i] == null) continue;
                if (i < lines.Length)
                {
                    rows[i].gameObject.SetActive(true);
                    rows[i].Bind(lines[i], isSub, OnRowChanged);
                }
                else rows[i].gameObject.SetActive(false);
            }
        }

        private void OnRowChanged() => Refresh();

        public void Close()
        {
            _instance = null;
            _template = null;
            gameObject.SetActive(false);
        }
    }
}
