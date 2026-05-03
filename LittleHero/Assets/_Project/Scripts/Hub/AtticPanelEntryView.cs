using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5006 Sprint 2 — 다락방 진입 버튼 → panel 활성화 (AtticController 잔여 3 제외).
    /// 5005 패턴 답습 (CharacterButton/ChapterButton — Serializable 내부 클래스 + onClick captured local).
    /// §15.1 SRP — 진입 버튼 list + onClick → panel.SetActive(true) 만.
    /// §15.7 DI — 모든 reference SerializeField (Singleton 직접 참조 X).
    /// 5005 PanelToggleManager 가 자동 비활 처리 (한 번에 하나만 활성).
    /// </summary>
    public class AtticPanelEntryView : MonoBehaviour
    {
        [Serializable]
        public class PanelEntry
        {
            public string id;          // 식별자 (diary/bestiary/char_info/...)
            public Button button;       // 진입 버튼
            public GameObject panelRoot;// 활성화할 panel GameObject
        }

        [SerializeField] private PanelEntry[] _entries;

        private void OnEnable()
        {
            if (_entries == null) return;
            for (int i = 0; i < _entries.Length; i++)
            {
                var e = _entries[i];
                if (e?.button == null || e.panelRoot == null) continue;
                var captured = e.panelRoot;
                e.button.onClick.AddListener(() => captured.SetActive(true));
            }
        }

        private void OnDisable()
        {
            if (_entries == null) return;
            foreach (var e in _entries)
                if (e?.button != null) e.button.onClick.RemoveAllListeners();
        }
    }
}
