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
    }
}
