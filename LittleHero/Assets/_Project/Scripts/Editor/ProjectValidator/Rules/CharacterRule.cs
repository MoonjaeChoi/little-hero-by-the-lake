using System.Collections.Generic;
using UnityEditor;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// CharacterData 기본 검증 + 5020 StarterHero portrait wiring + 5021 CharacterPool 통합.
    /// </summary>
    public static class CharacterRule
    {
        public static void Validate(List<Issue> issues)
        {
            ValidateCharacters(issues);
            ValidateStarterHeroPortraits(issues);
            ValidateCharacterPool(issues);
        }

        private static void ValidateCharacters(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.CharacterData");
            if (guids.Length == 0) issues.Add(new Issue(Severity.Warning, "CharacterData", "CharacterData 인스턴스가 없음"));
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var c = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (c == null) continue;
                if (string.IsNullOrEmpty(c.id)) issues.Add(new Issue(Severity.Warning, "CharacterData", $"{path}: id 비어있음"));
                if (c.maxHealth <= 0)    issues.Add(new Issue(Severity.Error,   "CharacterData", $"{path}: maxHealth={c.maxHealth}"));
                if (c.moveSpeed <= 0)    issues.Add(new Issue(Severity.Error,   "CharacterData", $"{path}: moveSpeed={c.moveSpeed}"));
                if (c.attackDamage <= 0) issues.Add(new Issue(Severity.Error,   "CharacterData", $"{path}: attackDamage={c.attackDamage}"));
            }
        }

        // 5020 T06 — 시작 주인공 3종 portrait wiring 검사
        private static void ValidateStarterHeroPortraits(List<Issue> issues)
        {
            string[] paths =
            {
                "Assets/_Project/Data/Characters/Char_HeroWarrior.asset",
                "Assets/_Project/Data/Characters/Char_HeroArcher.asset",
                "Assets/_Project/Data/Characters/Char_HeroMage.asset",
            };
            foreach (var path in paths)
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (data == null) issues.Add(new Issue(Severity.Error, "5020 StarterHero", $"{path}: 로드 실패"));
                else if (data.portrait == null) issues.Add(new Issue(Severity.Error, "5020 StarterHero", $"{path}: portrait 미wiring"));
            }
        }

        // 5021 T02 — CharacterPool 인스턴스 + 3 character 매핑 검사
        private static void ValidateCharacterPool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.CharacterPool");
            if (guids.Length == 0)
            {
                issues.Add(new Issue(Severity.Warning, "5021 CharacterPool", "CharacterPool 인스턴스 0건 — Tools/LittleHero/Setup CharacterPool Default 실행"));
                return;
            }
            string[] expected = { "hero_warrior", "hero_archer", "hero_mage" };
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var pool = AssetDatabase.LoadAssetAtPath<CharacterPool>(path);
                if (pool == null) continue;
                if (pool.characters == null || pool.characters.Length != 3)
                {
                    issues.Add(new Issue(Severity.Error, "5021 CharacterPool", $"{path}: characters 길이={(pool.characters?.Length ?? 0)} 기대 3"));
                    continue;
                }
                var ids = new HashSet<string>();
                foreach (var c in pool.characters)
                    if (c != null) ids.Add(c.id);
                foreach (var id in expected)
                    if (!ids.Contains(id)) issues.Add(new Issue(Severity.Error, "5021 CharacterPool", $"{path}: id '{id}' 누락"));
            }
        }
    }
}
