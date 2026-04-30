using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class BestiarySlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _descText;

        public void Bind(EnemyData data, bool defeated)
        {
            if (_icon != null)
            {
                if (defeated && data.icon != null) _icon.sprite = data.icon;
                _icon.color = defeated ? data.tintColor : new Color(0.25f, 0.25f, 0.30f, 1f);
                _icon.enabled = true;
            }
            if (_nameText != null)
                _nameText.text = defeated ? data.displayName : "???";
            if (_descText != null)
                _descText.text = defeated ? FormatStats(data) : "(아직 처치하지 않음)";
        }

        private static string FormatStats(EnemyData d)
        {
            string tag = d.isBoss ? "[보스] " : (d.isMiniB ? "[미니보스] " : "");
            return $"{tag}HP {d.maxHealth:0} / 공격 {d.damage:0}";
        }
    }
}
