using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace LittleHero
{
    /// <summary>
    /// 5033.0 — InputSystemUIInputModule 의 actionsAsset 미wiring 시 Awake 에서 default 자동 할당.
    /// Editor 모드에선 wiring 영구 어려움 (DefaultInputActions 는 코드 생성 SO) → Awake 시 자동 보장.
    /// EventSystem GameObject 에 부착.
    /// </summary>
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class EnsureInputActions : MonoBehaviour
    {
        private void Awake()
        {
            var im = GetComponent<InputSystemUIInputModule>();
            if (im == null) return;
            if (im.actionsAsset != null) return;
            im.AssignDefaultActions();
            Debug.Log("[EnsureInputActions] DefaultInputActions auto-assigned");
        }
    }
}
