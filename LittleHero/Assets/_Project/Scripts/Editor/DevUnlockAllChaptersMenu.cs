#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5036 — 개발/QA 편의 메뉴. 모든 챕터 진입 + 권장 레벨 경고 스킵 토글.
    /// WondrousMapPanel.DevUnlockAllChapters 정적 플래그 + SaveData.firstClearedChapters 비트 set.
    /// </summary>
    public static class DevUnlockAllChaptersMenu
    {
        private const string MENU_UNLOCK = "Tools/LittleHero/Dev — Unlock All Chapters";
        private const string MENU_TOGGLE = "Tools/LittleHero/Dev — Toggle DevUnlockAllChapters Flag";

        [MenuItem(MENU_UNLOCK)]
        public static void UnlockAll()
        {
            // 1. 정적 플래그 활성 (런타임 즉시)
            WondrousMapPanel.DevUnlockAllChapters = true;

            // 2. SaveData.firstClearedChapters 비트 모두 set (Play Mode 영구)
            var data = MetaProgression.Instance != null && Application.isPlaying
                ? MetaProgression.Instance.Data
                : SaveSystem.Load();
            if (data != null)
            {
                long mask = 0L;
                for (int i = 0; i < 50; i++) mask |= (1L << i);  // CH 1~50 모두
                data.firstClearedChapters = mask;
                SaveSystem.Save(data);
                Debug.Log($"[DevUnlockAllChapters] firstClearedChapters = 0x{mask:X} (CH 1~50 모두 unlock) + DevUnlockAllChapters=true");
            }
            else
            {
                Debug.LogWarning("[DevUnlockAllChapters] SaveData null — Play Mode 진입 후 재실행 권장");
            }
        }

        [MenuItem(MENU_TOGGLE)]
        public static void ToggleFlag()
        {
            WondrousMapPanel.DevUnlockAllChapters = !WondrousMapPanel.DevUnlockAllChapters;
            Debug.Log($"[DevUnlockAllChapters] flag = {WondrousMapPanel.DevUnlockAllChapters}");
        }
    }
}
#endif
