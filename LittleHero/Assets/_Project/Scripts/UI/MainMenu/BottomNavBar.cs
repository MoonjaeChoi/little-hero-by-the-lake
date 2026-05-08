using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 — 탕탕특공대 답습 5-tab BottomNav. Default = Adventure (★).
    /// MainMenuController 가 OnTabSelected 구독 → ContentPanel SetActive 분기.
    /// </summary>
    public class BottomNavBar : MonoBehaviour
    {
        [Serializable]
        public class TabButton
        {
            public TabId id;
            public Button button;
            public Image highlight;  // active 시각 강조
        }

        [SerializeField] private TabButton[] _buttons;
        [SerializeField] private TabId _defaultTab = TabId.Adventure;

        public TabId SelectedTab { get; private set; }
        public event Action<TabId> OnTabSelected;

        private void OnEnable()
        {
            if (_buttons == null) return;
            for (int i = 0; i < _buttons.Length; i++)
            {
                var entry = _buttons[i];
                if (entry?.button == null) continue;
                var captured = entry.id;
                entry.button.onClick.AddListener(() => Select(captured));
            }
            Select(_defaultTab);
        }

        private void OnDisable()
        {
            if (_buttons == null) return;
            foreach (var entry in _buttons)
                if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
        }

        public void Select(TabId id)
        {
            SelectedTab = id;
            // active 시각 강조
            if (_buttons != null)
            {
                foreach (var entry in _buttons)
                {
                    if (entry?.highlight == null) continue;
                    entry.highlight.enabled = (entry.id == id);
                }
            }
            OnTabSelected?.Invoke(id);
        }
    }
}
