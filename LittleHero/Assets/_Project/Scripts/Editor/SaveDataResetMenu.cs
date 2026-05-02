using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// SaveData JSON 파일 (Application.persistentDataPath/save.json) 즉시 삭제.
    /// Play 모드 시 새 게임으로 시작 (level=1, 화폐=0, 챕터 잠금, 메타 업그레이드 0).
    ///
    /// 메뉴: Tools > LittleHero > Reset SaveData
    /// </summary>
    public static class SaveDataResetMenu
    {
        [MenuItem("Tools/LittleHero/Reset SaveData (디스크 save.json 삭제)")]
        public static void ResetSaveData()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.json");
            bool existed = File.Exists(path);
            if (existed)
            {
                File.Delete(path);
                Debug.Log("[SaveDataReset] 삭제 완료: " + path);
                EditorUtility.DisplayDialog(
                    "SaveData Reset",
                    "save.json 삭제 완료.\n\n" + path + "\n\nPlay 모드 재시작 시 새 게임으로 시작됩니다.",
                    "OK");
            }
            else
            {
                Debug.Log("[SaveDataReset] 파일 없음 (이미 reset 상태): " + path);
                EditorUtility.DisplayDialog(
                    "SaveData Reset",
                    "save.json 이 이미 없습니다 (새 게임 상태).\n\n" + path,
                    "OK");
            }
        }

        [MenuItem("Tools/LittleHero/Show SaveData Path")]
        public static void ShowSaveDataPath()
        {
            string path = Path.Combine(Application.persistentDataPath, "save.json");
            bool exists = File.Exists(path);
            EditorUtility.DisplayDialog(
                "SaveData Path",
                path + "\n\n" + (exists ? "존재함 (size " + new FileInfo(path).Length + " bytes)" : "없음"),
                "OK");
            // Finder 에서 폴더 열기
            EditorUtility.RevealInFinder(path);
        }

        [MenuItem("Tools/LittleHero/Reset Boss Cooldown (일일·주간 보스 즉시 도전 가능)")]
        public static void ResetBossCooldown()
        {
            var data = SaveSystem.Load();
            if (data == null)
            {
                EditorUtility.DisplayDialog("Boss Cooldown",
                    "SaveData 파일이 없습니다. 먼저 Play 모드 1회 실행해 SaveData 를 생성한 뒤 다시 시도하세요.", "OK");
                return;
            }
            long beforeDaily = data.lastDailyBossClearTime;
            long beforeWeekly = data.lastWeeklyBossClearTime;
            data.lastDailyBossClearTime = 0;
            data.lastWeeklyBossClearTime = 0;
            SaveSystem.Save(data);
            Debug.Log($"[BossCooldownReset] daily {beforeDaily}→0 / weekly {beforeWeekly}→0");
            EditorUtility.DisplayDialog("Boss Cooldown Reset",
                "✅ Reset 완료\n" +
                "  daily: " + beforeDaily + " → 0\n" +
                "  weekly: " + beforeWeekly + " → 0\n\n" +
                "Play 모드 (또는 EndgameMenuPanel 재진입) 시 일일·주간 보스 도전 가능.",
                "OK");
        }

        [MenuItem("Tools/LittleHero/Set Endgame Menu Active (firstClearedChapters 35비트 ON)")]
        public static void EnableEndgameMenu()
        {
            var data = SaveSystem.Load();
            if (data == null) data = new SaveData();
            long before = data.firstClearedChapters;
            data.firstClearedChapters = (1L << 35) - 1;
            SaveSystem.Save(data);
            Debug.Log($"[EndgameEnable] firstClearedChapters {before}→{data.firstClearedChapters}");
            EditorUtility.DisplayDialog("Endgame Menu Active",
                "✅ firstClearedChapters " + before + " → " + data.firstClearedChapters + "\n\n" +
                "Play 모드 시작 시 다락방 우상단 '보스 도전' 메뉴 활성.",
                "OK");
        }
    }
}
