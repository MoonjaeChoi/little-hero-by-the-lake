using System.Collections.Generic;

namespace LittleHero
{
    public static class SaveDataMigration
    {
        // 5020 D3-B: 키즈 → 직업 정합 character id
        private static readonly Dictionary<string, string> CharacterIdMap = new()
        {
            { "default_boy",   "hero_warrior" },
            { "brave_girl",    "hero_archer" },
            { "quiet_friend",  "hero_mage" },
        };

        public static string MigrateCharacterId(string oldId)
        {
            if (string.IsNullOrEmpty(oldId)) return oldId;
            return CharacterIdMap.TryGetValue(oldId, out var newId) ? newId : oldId;
        }

        public static SaveData Apply(SaveData data)
        {
            if (data == null) return data;

            data.selectedCharacterId = MigrateCharacterId(data.selectedCharacterId);

            if (data.unlockedCharacterIds != null)
            {
                for (int i = 0; i < data.unlockedCharacterIds.Length; i++)
                    data.unlockedCharacterIds[i] = MigrateCharacterId(data.unlockedCharacterIds[i]);
            }

            return data;
        }
    }
}
