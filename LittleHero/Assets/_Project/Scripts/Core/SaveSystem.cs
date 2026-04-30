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
        public int starFragments;                                    // 무지개 구슬 ✦ (영구)
        public string selectedCharacterId;
        public string[] unlockedCharacterIds = new string[0];
        public string[] unlockedEquipmentIds = new string[0];
        public string[] discoveredMemoryIds = new string[0];
        public string[] defeatedEnemyIds = new string[0];
        public int[] clearedChapterWaves = new int[0];
        public MetaUpgradeEntry[] metaUpgrades = new MetaUpgradeEntry[0];

        // ✦ A6 — 신규 화폐·진행도 (economy_v1 §9.2 + character_progression §7.4 + monetization §7.1)
        public int magicCrystals;                                    // 마법 결정 ◇ (영구, +16 이상 강화 전용)
        public int starlight;                                        // 별빛 ⋆ (영구·캐시)
        public int level = 1;                                        // 캐릭터 레벨 (L100 캡)
        public int xp;                                               // 누적 XP (현 레벨 내 진행도는 BalanceConfig.xpThresholds로 환산)
        public int firstClearedChapters;                             // 35 챕터 첫 클리어 비트마스크 (별빛 1회 보상용)
        // permanentlyLockedAffixes / mythicMileageBonus 는 B2 EquipmentInstance 직렬화와 함께 도입.
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
