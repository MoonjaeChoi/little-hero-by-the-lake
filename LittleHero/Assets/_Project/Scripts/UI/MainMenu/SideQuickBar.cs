using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero.MainMenu
{
    /// <summary>
    /// 5040 T05 — 사이드 quick FOMO bar (탕탕특공대 답습).
    /// DailyMission / SeasonPass / LimitedShop 알림 dot + click event.
    /// </summary>
    public class SideQuickBar : MonoBehaviour
    {
        [Serializable]
        public class QuickEntry
        {
            public string id;          // "daily" / "season" / "limited"
            public Button button;
            public Image notifDot;     // 알림 점
        }

        [SerializeField] private QuickEntry[] _entries;

        public event Action<string> OnQuickSelected;

        private void OnEnable()
        {
            if (_entries == null) return;
            for (int i = 0; i < _entries.Length; i++)
            {
                var entry = _entries[i];
                if (entry?.button == null) continue;
                var captured = entry.id;
                entry.button.onClick.AddListener(() => OnQuickSelected?.Invoke(captured));
            }
        }

        private void OnDisable()
        {
            if (_entries == null) return;
            foreach (var e in _entries)
                if (e?.button != null) e.button.onClick.RemoveAllListeners();
        }

        public void SetNotificationDot(string id, bool active)
        {
            if (_entries == null) return;
            foreach (var e in _entries)
                if (e?.id == id && e.notifDot != null) e.notifDot.enabled = active;
        }
    }
}
