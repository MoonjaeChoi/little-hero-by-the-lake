using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class MetaUpgradeRow : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _bonusText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _costText;
        [SerializeField] private Button _upgradeButton;

        private MetaUpgradeData _data;

        private void Awake()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.AddListener(HandleUpgrade);
        }

        private void OnDestroy()
        {
            if (_upgradeButton != null)
                _upgradeButton.onClick.RemoveListener(HandleUpgrade);
        }

        public void Bind(MetaUpgradeData data)
        {
            _data = data;
            if (_icon != null && data.icon != null) { _icon.sprite = data.icon; _icon.enabled = true; }
            else if (_icon != null) _icon.enabled = data.icon != null;
            if (_nameText != null) _nameText.text = data.displayName;
            Refresh();
        }

        public void Refresh()
        {
            if (_data == null || MetaProgression.Instance == null) return;

            int level = MetaProgression.Instance.GetUpgradeLevel(_data.id);
            bool maxed = level >= _data.maxLevel;

            if (_levelText != null) _levelText.text = $"Lv {level} / {_data.maxLevel}";
            if (_bonusText != null) _bonusText.text = FormatBonus(_data, level);

            if (maxed)
            {
                if (_costText != null) _costText.text = "최대";
                if (_upgradeButton != null) _upgradeButton.interactable = false;
                return;
            }

            int cost = MetaProgression.Instance.GetUpgradeCost(_data);
            if (_costText != null) _costText.text = $"강화 (✦ {cost})";
            if (_upgradeButton != null) _upgradeButton.interactable = MetaProgression.Instance.CanUpgrade(_data);
        }

        private static string FormatBonus(MetaUpgradeData u, int level)
        {
            float total = u.bonusPerLevel * level;
            switch (u.targetStat)
            {
                case MetaUpgradeStat.MaxHealth:    return $"+{total:0} 체력";
                case MetaUpgradeStat.AttackDamage: return $"+{total:0} 공격력";
                case MetaUpgradeStat.StarDustGain: return $"+{total * 100f:0}% 별가루";
            }
            return "";
        }

        private void HandleUpgrade()
        {
            if (_data == null || MetaProgression.Instance == null) return;
            MetaProgression.Instance.TryUpgrade(_data);
        }
    }
}
