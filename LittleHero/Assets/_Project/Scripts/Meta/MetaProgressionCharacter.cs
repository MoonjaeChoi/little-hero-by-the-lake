namespace LittleHero
{
    /// <summary>
    /// 5023 — Character 선택 책임 분리. SelectedCharacter / SelectCharacter / ResolveCharacterClass.
    /// Event 발행은 정적 (D3=B) — 구독자 호환성 보존.
    /// </summary>
    public static class MetaProgressionCharacter
    {
        public static CharacterData SelectedCharacter { get; private set; }

        // 5023 T02 — Awake 시 Loader 결과를 Save 호출 + event 발행 없이 초기화 (init only)
        public static void InitFromSave(CharacterData character)
        {
            SelectedCharacter = character;
        }

        public static void SelectCharacter(CharacterData character)
        {
            // weapon_v2 — 직업 변경 감지 (이전 → 신규)
            var prevClass = ResolveCharacterClass(SelectedCharacter);
            var newClass = ResolveCharacterClass(character);

            SelectedCharacter = character;
            if (MetaProgression.Instance?.Data != null)
            {
                MetaProgression.Instance.Data.selectedCharacterId = character != null ? character.id : null;
                SaveSystem.Save(MetaProgression.Instance.Data);
            }
            MetaProgression.OnCharacterChanged?.Invoke(character);
            if (character != null) LimitedShopService.HandleCharacterFirstSelect(character.id);

            // 직업 enum 변경 시만 발행 (같은 직업 내 캐릭터 교체 시 X)
            if (prevClass != newClass) MetaProgression.OnCharacterClassChanged?.Invoke(newClass);
        }

        public static CharacterClass ResolveCharacterClass(CharacterData ch)
        {
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
