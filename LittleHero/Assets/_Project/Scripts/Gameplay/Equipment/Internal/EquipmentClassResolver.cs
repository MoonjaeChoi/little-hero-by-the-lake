namespace LittleHero
{
    /// <summary>
    /// 5028 — CharacterData.className 한국어 → CharacterClass enum.
    /// weapon_v2 직업 분류 정합 (warrior_v2.md).
    /// </summary>
    public static class EquipmentClassResolver
    {
        public static CharacterClass ResolvePlayerClass()
        {
            var ch = MetaProgressionCharacter.SelectedCharacter;
            if (ch == null || string.IsNullOrEmpty(ch.className)) return CharacterClass.Warrior;
            switch (ch.className)
            {
                case "전사": return CharacterClass.Warrior;
                case "궁수": return CharacterClass.Archer;
                case "마법사": return CharacterClass.Mage;
                default: return CharacterClass.Warrior;
            }
        }
    }
}
