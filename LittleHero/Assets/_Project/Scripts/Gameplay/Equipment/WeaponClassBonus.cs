using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// weapon_system_v2.md §4.2.F — 직업+카테고리 시너지 보너스 (메이플 키우기 풍).
    /// 전사 + Sword/Axe/Mace/Spear → Damage% +10%
    /// 궁수 + Bow/Crossbow/Dagger/Boomerang → AttackSpeed +5%
    /// 마법사 + Staff/Wand/Tome → MagicDamage +10%
    /// </summary>
    public static class WeaponClassBonus
    {
        public const float WARRIOR_DAMAGE_BONUS = 10f;     // %
        public const float ARCHER_AS_BONUS_MUL = 1.05f;
        public const float MAGE_MAGIC_DMG_BONUS = 0.10f;   // 0.0~1.0

        /// <summary>직업 + 카테고리 매칭 검증 (시너지 발동 여부).</summary>
        public static bool IsClassMatch(CharacterClass cls, WeaponCategory cat)
        {
            switch (cls)
            {
                case CharacterClass.Warrior:
                    return cat == WeaponCategory.Sword || cat == WeaponCategory.Axe
                        || cat == WeaponCategory.Mace || cat == WeaponCategory.Spear;
                case CharacterClass.Archer:
                    return cat == WeaponCategory.Bow || cat == WeaponCategory.Crossbow
                        || cat == WeaponCategory.Dagger || cat == WeaponCategory.Boomerang;
                case CharacterClass.Mage:
                    return cat == WeaponCategory.Staff || cat == WeaponCategory.Wand
                        || cat == WeaponCategory.Tome;
                default: return false;
            }
        }

        /// <summary>PlayerStats 직접 갱신 (EquipmentManager.SyncEquippedInstances 끝부분 호출).</summary>
        public static void Apply(PlayerStats stats, CharacterClass cls, WeaponCategory cat)
        {
            if (stats == null || !IsClassMatch(cls, cat)) return;
            switch (cls)
            {
                case CharacterClass.Warrior:
                    stats.AddAdditiveDamageBonus(WARRIOR_DAMAGE_BONUS);
                    break;
                case CharacterClass.Archer:
                    stats.MultiplyAttackSpeed(ARCHER_AS_BONUS_MUL);
                    break;
                case CharacterClass.Mage:
                    stats.AddMagicDamageMul(MAGE_MAGIC_DMG_BONUS);
                    break;
            }
        }
    }
}
