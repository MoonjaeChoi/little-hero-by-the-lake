using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5029 — 무기 카드 + 가챠 결과 element 아이콘 표시.
    /// element=None 시 자동 숨김 (전사/궁수 호환).
    /// </summary>
    public class ElementIcon : MonoBehaviour
    {
        [SerializeField] private Image _iconImage;
        [SerializeField] private GameObject _container;

        public void Apply(MagicElement element)
        {
            bool show = element != MagicElement.None;
            if (_container != null) _container.SetActive(show);
            if (!show) return;

            if (_iconImage != null)
                _iconImage.color = ElementVfxBank.GetColor(element);
        }
    }
}
