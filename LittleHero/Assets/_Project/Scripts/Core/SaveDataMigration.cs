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

            // 5033.2 — equipmentInstances.isEquipped → equippedByClass 자동 변환
            // 마이그레이션 정책: isEquipped=true + equippedByClassInt=-1 → 현재 SelectedCharacter 의 class 로 추정
            if (data.equipmentInstances != null && data.equipmentInstances.Length > 0)
            {
                int migrated = 0;
                CharacterClass? selectedClass = null;
                if (!string.IsNullOrEmpty(data.selectedCharacterId))
                {
                    if (data.selectedCharacterId.Contains("warrior")) selectedClass = CharacterClass.Warrior;
                    else if (data.selectedCharacterId.Contains("archer"))  selectedClass = CharacterClass.Archer;
                    else if (data.selectedCharacterId.Contains("mage"))    selectedClass = CharacterClass.Mage;
                }
                foreach (var inst in data.equipmentInstances)
                {
                    if (inst == null) continue;
                    if (inst.equippedByClassInt >= 0) continue;  // 이미 마이그레이션됨
                    if (!inst.isEquipped) continue;              // 미장착 → 변환 X
                    if (selectedClass.HasValue)
                    {
                        inst.equippedByClassInt = (int)selectedClass.Value;
                        migrated++;
                    }
                }
                if (migrated > 0)
                    UnityEngine.Debug.Log($"[SaveDataMigration] 5033.2 — {migrated} EquipmentInstance.isEquipped → equippedByClass={selectedClass} 변환");
            }

            return data;
        }
    }
}
