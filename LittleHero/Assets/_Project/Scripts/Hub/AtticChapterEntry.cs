using System;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 3 — 다락방 챕터 진입 + 5002 D2 pendingChapterClearedId 1회 toast.
    /// AtticController 에서 추출 (§15.1 SRP).
    /// </summary>
    public class AtticChapterEntry : MonoBehaviour
    {
        [Serializable]
        public class ChapterButton
        {
            public ChapterData chapter;
            public Button button;
        }

        [SerializeField] private ChapterButton[] _buttons;
        [SerializeField] private string _chapterSceneName = "Chapter";

        private void OnEnable()
        {
            if (_buttons != null)
            {
                for (int i = 0; i < _buttons.Length; i++)
                {
                    var entry = _buttons[i];
                    if (entry?.button == null) continue;
                    var captured = entry.chapter;
                    entry.button.onClick.AddListener(() => HandleStart(captured));
                }
            }
        }

        private void OnDisable()
        {
            if (_buttons != null)
                foreach (var entry in _buttons)
                    if (entry?.button != null) entry.button.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            HandlePendingChapterCleared();   // 5002 D2 — 챕터 클리어 후 1회 toast
        }

        private void HandleStart(ChapterData chapter)
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.SelectedChapter = chapter;
            SceneTransitionService.Transition(_chapterSceneName);  // 5004 fade transition
        }

        // 5002 D2 — 챕터 클리어 후 다락방 1회 toast (BossEncounterService.CompleteChapterBoss 가 set)
        private void HandlePendingChapterCleared()
        {
            var data = MetaProgression.Instance?.Data;
            if (data == null || string.IsNullOrEmpty(data.pendingChapterClearedId)) return;

            var ch = ResolveChapterById(data.pendingChapterClearedId);
            if (ch != null)
            {
                // StarlightGrantService 이중 raise 안전 (chapterStarlightGranted bit).
                GameEvents.RaiseChapterCleared(ch);
                Debug.Log($"[AtticChapterEntry] 챕터 클리어 1회 toast: {ch.displayName} (5002 D2)");
            }
            data.pendingChapterClearedId = "";
            SaveSystem.Save(data);
        }

        private ChapterData ResolveChapterById(string id)
        {
            if (string.IsNullOrEmpty(id) || _buttons == null) return null;
            foreach (var entry in _buttons)
                if (entry?.chapter != null && entry.chapter.id == id) return entry.chapter;
            return null;
        }
    }
}
