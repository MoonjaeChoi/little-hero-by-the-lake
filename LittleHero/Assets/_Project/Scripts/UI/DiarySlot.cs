using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    public class DiarySlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _descText;

        public void Bind(MemoryData data, bool discovered)
        {
            if (_icon != null)
            {
                _icon.sprite = discovered ? data.image : null;
                _icon.color = discovered ? Color.white : new Color(0.3f, 0.3f, 0.35f, 1f);
                _icon.enabled = true;
            }
            if (_nameText != null)
                _nameText.text = discovered ? data.displayName : "???";
            if (_descText != null)
                _descText.text = discovered ? data.description : "(아직 발견하지 않음)";
        }
    }
}
