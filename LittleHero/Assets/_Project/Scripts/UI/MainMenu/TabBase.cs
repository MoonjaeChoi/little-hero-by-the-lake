using UnityEngine;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 — 5-tab BottomNav 의 Tab 추상 base.
    /// MainMenuController 가 SwitchTab 시 OnEnter/OnExit 호출.
    /// </summary>
    public abstract class TabBase : MonoBehaviour
    {
        public TabId Id { get; protected set; }

        public virtual void OnEnter() { gameObject.SetActive(true); }
        public virtual void OnExit()  { gameObject.SetActive(false); }
    }

    /// <summary>5040 — 5 tab 종류. Adventure 가 default.</summary>
    public enum TabId
    {
        Hero      = 0,
        Equipment = 1,
        Adventure = 2,  // default
        Shop      = 3,
        Record    = 4,
    }
}
