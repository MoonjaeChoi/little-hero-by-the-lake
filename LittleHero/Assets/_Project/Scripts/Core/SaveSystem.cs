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

        // ✦ B2 — 장비 인스턴스 영구 저장 (equipment §4.2)
        public EquipmentInstance[] equipmentInstances = new EquipmentInstance[0];

        // monetization §7.1 — 별빛으로 영구 잠금/마일리지 산 결과 (인스턴스ID 기반)
        public string[] permanentlyLockedInstanceIds = new string[0];
        public MythicMileageEntry[] mythicMileageBonus = new MythicMileageEntry[0];

        // ✦ C6 — Endgame 진행 상태 (endgame_v1 §8.2 + §8.3)
        public long lastDailyBossClearTime;                          // Unix 초. 0 = 미클리어
        public long lastWeeklyBossClearTime;                         // Unix 초
        public int infiniteModeMaxDepth;                             // 무한 모드 최고 깊이
        public bool[] hardcoreChaptersCleared = new bool[15];        // CH 36~50 (인덱스 0~14)
    }

    [System.Serializable]
    public class MythicMileageEntry
    {
        public string instanceId;
        public int bonus;
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
