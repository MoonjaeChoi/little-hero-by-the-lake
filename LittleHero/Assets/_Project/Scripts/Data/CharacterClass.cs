namespace LittleHero
{
    /// <summary>
    /// weapon_system_v2.md §4.1 — 캐릭터 직업 enum (3 직업).
    /// CharacterData.className 한국어 라벨과 매핑 ('전사' → Warrior 등).
    /// </summary>
    public enum CharacterClass
    {
        Warrior = 0,   // 전사 — 검·둔기·도끼·창
        Archer  = 1,   // 궁수 — 활·석궁·단검·부메랑
        Mage    = 2,   // 마법사 — 지팡이·완드·마도서
    }

    /// <summary>
    /// weapon_system_v2.md §4.1 — 무기 카테고리 enum (11종, 메이플 표준).
    /// EquipmentTemplate.category 필드로 무기-직업 시너지 검증.
    /// </summary>
    public enum WeaponCategory
    {
        // 전사 (Warrior)
        Sword = 0,
        Axe = 1,
        Mace = 2,        // 둔기·망치
        Spear = 3,

        // 궁수 (Archer)
        Bow = 4,
        Crossbow = 5,
        Dagger = 6,      // 단검 (도적 → 궁수 통합, Little Hero 3 직업 환경)
        Boomerang = 7,

        // 마법사 (Mage)
        Staff = 8,
        Wand = 9,
        Tome = 10,       // 마도서

        None = 99,       // 비-무기 슬롯 (모자/상의 등) 또는 미지정
    }
}
