using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "Little Hero/Character Pool")]
    public class CharacterPool : ScriptableObject
    {
        public CharacterData[] characters;

        public CharacterData FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || characters == null) return null;
            for (int i = 0; i < characters.Length; i++)
                if (characters[i] != null && characters[i].id == id) return characters[i];
            return null;
        }
    }
}
