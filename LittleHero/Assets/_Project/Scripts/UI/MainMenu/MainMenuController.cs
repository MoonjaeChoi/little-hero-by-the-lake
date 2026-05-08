using System.Collections.Generic;
using UnityEngine;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T06a — MainMenu 최상위 컨트롤러. BottomNavBar 구독 + 11 panel SetActive 분기.
    /// 5 tab content GameObject 직접 SerializeField wiring (D2=A SetActive 패턴).
    /// AtticController 와 협업 (또는 흡수).
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private BottomNavBar _bottomNav;

        [Header("5 Tab Content GameObject (SetActive 패턴, D2=A)")]
        [SerializeField] private GameObject _heroContent;
        [SerializeField] private GameObject _equipmentContent;
        [SerializeField] private GameObject _adventureContent;  // ChapterBookCarousel + MainCTAButton
        [SerializeField] private GameObject _shopContent;
        [SerializeField] private GameObject _recordContent;

        private void OnEnable()
        {
            if (_bottomNav != null) _bottomNav.OnTabSelected += SwitchTab;
        }

        private void OnDisable()
        {
            if (_bottomNav != null) _bottomNav.OnTabSelected -= SwitchTab;
        }

        public void SwitchTab(TabId id)
        {
            // SetActive 패턴 (D2=A) — 5 GameObject 인메모리, 빠른 전환
            if (_heroContent != null)      _heroContent.SetActive(id == TabId.Hero);
            if (_equipmentContent != null) _equipmentContent.SetActive(id == TabId.Equipment);
            if (_adventureContent != null) _adventureContent.SetActive(id == TabId.Adventure);
            if (_shopContent != null)      _shopContent.SetActive(id == TabId.Shop);
            if (_recordContent != null)    _recordContent.SetActive(id == TabId.Record);
        }
    }
}
