using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// inventory_ui_v2.md §5.2 — 그리드 단일 카드 (320×320).
    /// 큰 썸네일 + 이름 + 강화 + 등급 + 장착 라벨 + 등급별 테두리.
    /// </summary>
    public class InventoryGridCard : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _enhanceText;
        [SerializeField] private Text _rarityText;
        [SerializeField] private GameObject _equippedBadge;
        [SerializeField] private Image _highlightBg;
        [SerializeField] private Image _border;
        [SerializeField] private Button _button;

        public EquipmentInstance Instance { get; private set; }
        public EquipmentTemplate Template { get; private set; }
        public Action<EquipmentInstance, EquipmentTemplate> OnClicked;

        private static readonly Color[] _rarityColors = {
            new Color(0.85f, 0.85f, 0.85f),  // Common
            new Color(0.40f, 0.68f, 1.00f),  // Magic
            new Color(0.30f, 0.85f, 0.40f),  // Rare
            new Color(0.70f, 0.40f, 0.95f),  // Epic
            new Color(1.00f, 0.84f, 0.00f),  // Legendary
            new Color(1.00f, 0.42f, 0.71f),  // Mythic
        };

        private static readonly string[] _rarityLabels = {
            "일반", "고급", "희귀", "영웅", "전설", "신화"
        };

        private void OnEnable()
        {
            if (_button != null) _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (_button != null) _button.onClick.RemoveAllListeners();
        }

        public void Bind(EquipmentInstance inst, EquipmentTemplate tpl, bool isEquipped)
        {
            Instance = inst;
            Template = tpl;
            if (_icon != null) { _icon.sprite = tpl?.icon; _icon.preserveAspect = true; _icon.enabled = tpl?.icon != null; }
            if (_nameText != null) _nameText.text = tpl?.displayName ?? "";
            if (_enhanceText != null) _enhanceText.text = inst != null && inst.enhanceLevel > 0 ? $"+{inst.enhanceLevel}" : "";
            int rarityIdx = tpl != null ? Mathf.Clamp((int)tpl.rarity, 0, _rarityColors.Length - 1) : 0;
            Color rcol = _rarityColors[rarityIdx];
            // weapon_v2 — 잘못된 직업 무기 회색 + 라벨 (rarity 라벨 자리에 직업 표시)
            if (tpl != null && tpl.category != WeaponCategory.None)
            {
                var playerClass = EquipmentManager.ResolvePlayerClass();
                bool wrongClass = tpl.requiredClass != playerClass;
                if (_rarityText != null)
                {
                    if (wrongClass)
                    {
                        _rarityText.text = ClassLabel(tpl.requiredClass) + " 전용";
                        _rarityText.color = ClassColor(tpl.requiredClass);
                    }
                    else
                    {
                        _rarityText.text = _rarityLabels[rarityIdx];
                        _rarityText.color = rcol;
                    }
                }
                if (_icon != null) _icon.color = wrongClass ? new Color(0.5f, 0.5f, 0.5f, 0.5f) : Color.white;
            }
            else
            {
                if (_rarityText != null) { _rarityText.text = _rarityLabels[rarityIdx]; _rarityText.color = rcol; }
                if (_icon != null) _icon.color = Color.white;
            }
            if (_border != null) _border.color = rcol;
            if (_equippedBadge != null) _equippedBadge.SetActive(isEquipped);
            SetSelected(false);
        }

        private static readonly Color[] _classColors = {
            new Color(0.85f, 0.20f, 0.20f),  // Warrior 빨강
            new Color(0.30f, 0.85f, 0.40f),  // Archer 녹
            new Color(0.65f, 0.42f, 0.95f),  // Mage 보라
        };
        private static string ClassLabel(CharacterClass c) => c switch {
            CharacterClass.Warrior => "전사",
            CharacterClass.Archer => "궁수",
            CharacterClass.Mage => "마법사",
            _ => "",
        };
        private static Color ClassColor(CharacterClass c) {
            int i = (int)c;
            if (i < 0 || i >= _classColors.Length) return Color.white;
            return _classColors[i];
        }

        public void SetSelected(bool selected)
        {
            if (_highlightBg == null) return;
            int rarityIdx = Template != null ? Mathf.Clamp((int)Template.rarity, 0, _rarityColors.Length - 1) : 0;
            var c = _rarityColors[rarityIdx];
            c.a = selected ? 0.35f : 0f;
            _highlightBg.color = c;
        }

        private void HandleClick() => OnClicked?.Invoke(Instance, Template);
    }
}
