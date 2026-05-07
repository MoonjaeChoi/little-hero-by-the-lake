using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5033.0g — modal panel UI 결함 일괄 fix.
    /// 1. 자체 Canvas + overrideSorting + sortingOrder=100 (부모 panel raycast 가로채기 회피)
    /// 2. GraphicRaycaster 자동 부착
    /// 3. 비-Selectable Graphic 의 raycastTarget=false (Image/Text 가 click 가로채는 결함 회피)
    /// Awake 1회 자동 적용. 모든 modal panel GameObject 에 부착.
    /// </summary>
    public class ModalPanelGuard : MonoBehaviour
    {
        [SerializeField] private int _sortingOrder = 100;

        private void Awake()
        {
            var canvas = GetComponent<Canvas>();
            if (canvas == null) canvas = gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = _sortingOrder;
            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            foreach (var g in GetComponentsInChildren<Graphic>(true))
            {
                if (!g.raycastTarget) continue;
                if (g.GetComponent<Selectable>() != null) continue;
                if (g.GetComponentInParent<Selectable>() != null) continue;
                g.raycastTarget = false;
            }
        }
    }
}
