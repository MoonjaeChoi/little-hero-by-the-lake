using System.IO;
using UnityEngine;

namespace LittleHero
{
    [System.Serializable]
    public class MetaUpgradeEntry
    {
        public string id;
        public int level;
    }

    [System.Serializable]
    public class SaveData
    {
        public int starFragments;
        public string selectedCharacterId;
        public string[] unlockedCharacterIds = new string[0];
        public string[] unlockedEquipmentIds = new string[0];
        public string[] discoveredMemoryIds = new string[0];
        public int[] clearedChapterWaves = new int[0];
        public MetaUpgradeEntry[] metaUpgrades = new MetaUpgradeEntry[0];
    }

    public static class SaveSystem
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData Load()
        {
            if (!File.Exists(SavePath))
                return new SaveData();
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        public static void Delete()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        public static bool HasSave() => File.Exists(SavePath);
    }
}
