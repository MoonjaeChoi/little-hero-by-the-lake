namespace LittleHero
{
    /// <summary>
    /// 5034 — 직업별 특수 스탯 시너지 (D4=B 메이플 정합).
    /// PlayerStats Add/Remove 메서드 활용 (D8=B 정합).
    /// 직업 변경 시 Remove 후 Apply 재호출.
    /// </summary>
    public static class ClassBonusApplier
    {
        // 5034 D4=B 메이플 정합 수치
        public const float WARRIOR_BOSS_DMG = 0.10f;  // 보스 데미지 +10%
        public const float ARCHER_CRIT      = 0.10f;  // 크리 확률 +10%
        public const float MAGE_AOE         = 0.15f;  // AOE +15%

        /// <summary>현재 직업의 특수 스탯 보너스 적용.</summary>
        public static void Apply(PlayerStats stats, CharacterClass cls)
        {
            if (stats == null) return;
            switch (cls)
            {
                case CharacterClass.Warrior: stats.AddBossDamageBonus(WARRIOR_BOSS_DMG); break;
                case CharacterClass.Archer:  stats.AddCritChanceBonus(ARCHER_CRIT); break;
                case CharacterClass.Mage:    stats.AddAoeBonus(MAGE_AOE); break;
            }
        }

        /// <summary>직업 변경 시 이전 직업 보너스 차감.</summary>
        public static void Remove(PlayerStats stats, CharacterClass cls)
        {
            if (stats == null) return;
            switch (cls)
            {
                case CharacterClass.Warrior: stats.RemoveBossDamageBonus(WARRIOR_BOSS_DMG); break;
                case CharacterClass.Archer:  stats.RemoveCritChanceBonus(ARCHER_CRIT); break;
                case CharacterClass.Mage:    stats.RemoveAoeBonus(MAGE_AOE); break;
            }
        }
    }
}
