using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5023 — SaveSystem.Load + 5021 ResolveSelectedCharacterFromSave 흡수.
    /// </summary>
    public static class MetaProgressionLoader
    {
        public static SaveData Load() => SaveSystem.Load();

        public static CharacterData ResolveSelectedCharacterFromSave(CharacterPool pool, SaveData data)
        {
            if (pool == null)
            {
                Debug.LogWarning("[MetaProgressionLoader] CharacterPool 미wiring — Inspector 확인");
                return null;
            }
            return pool.FindById(data?.selectedCharacterId);
        }
    }
}
