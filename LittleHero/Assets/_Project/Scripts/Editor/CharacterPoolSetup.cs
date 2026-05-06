#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    public static class CharacterPoolSetup
    {
        private const string PoolPath = "Assets/_Project/Data/Characters/CharacterPool_Default.asset";

        [MenuItem("Tools/LittleHero/Setup CharacterPool Default")]
        public static void Setup()
        {
            var existing = AssetDatabase.LoadAssetAtPath<CharacterPool>(PoolPath);
            if (existing != null)
            {
                Debug.Log($"[CharacterPoolSetup] {PoolPath} 이미 존재");
                return;
            }

            var warrior = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Characters/Char_HeroWarrior.asset");
            var archer  = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Characters/Char_HeroArcher.asset");
            var mage    = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Characters/Char_HeroMage.asset");
            if (warrior == null || archer == null || mage == null)
            {
                Debug.LogError("[CharacterPoolSetup] 3 CharacterData SO 중 하나 이상 로드 실패");
                return;
            }

            var pool = ScriptableObject.CreateInstance<CharacterPool>();
            pool.characters = new[] { warrior, archer, mage };
            AssetDatabase.CreateAsset(pool, PoolPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CharacterPoolSetup] {PoolPath} 생성 완료 (3 character)");
        }
    }
}
#endif
