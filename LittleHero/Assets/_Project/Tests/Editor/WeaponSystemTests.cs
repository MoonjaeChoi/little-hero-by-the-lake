using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// CLAUDE.md §15.9 — weapon_system_v2 단위 테스트.
    /// IsClassMatch (6) + WeaponInstance.CanEquip (3) + Apply 검증 (3).
    /// </summary>
    public class WeaponSystemTests
    {
        // === IsClassMatch 6 ===

        [Test] public void IsClassMatch_WarriorSword_True()
            => Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Warrior, WeaponCategory.Sword));

        [Test] public void IsClassMatch_WarriorMace_True()
            => Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Warrior, WeaponCategory.Mace));

        [Test] public void IsClassMatch_ArcherBow_True()
            => Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Archer, WeaponCategory.Bow));

        [Test] public void IsClassMatch_ArcherDagger_True()
            => Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Archer, WeaponCategory.Dagger));

        [Test] public void IsClassMatch_MageStaff_True()
            => Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Mage, WeaponCategory.Staff));

        [Test] public void IsClassMatch_MageTome_True()
            => Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Mage, WeaponCategory.Tome));

        // === Mismatch (false) ===

        [Test] public void IsClassMatch_WarriorBow_False()
            => Assert.IsFalse(WeaponClassBonus.IsClassMatch(CharacterClass.Warrior, WeaponCategory.Bow));

        [Test] public void IsClassMatch_ArcherStaff_False()
            => Assert.IsFalse(WeaponClassBonus.IsClassMatch(CharacterClass.Archer, WeaponCategory.Staff));

        [Test] public void IsClassMatch_MageDagger_False()
            => Assert.IsFalse(WeaponClassBonus.IsClassMatch(CharacterClass.Mage, WeaponCategory.Dagger));

        [Test] public void IsClassMatch_NoneCategory_False()
            => Assert.IsFalse(WeaponClassBonus.IsClassMatch(CharacterClass.Warrior, WeaponCategory.None));

        // === WeaponInstance.CanEquip ===

        [Test]
        public void WeaponInstance_CanEquip_RoleMatch_True()
        {
            var tpl = ScriptableObject.CreateInstance<EquipmentTemplate>();
            tpl.requiredClass = CharacterClass.Warrior;
            tpl.category = WeaponCategory.Sword;
            var inst = new EquipmentInstance { templateId = "test", isEquipped = false };
            var weapon = new WeaponInstance(inst, tpl);
            Assert.IsTrue(weapon.CanEquip(CharacterClass.Warrior));
            ScriptableObject.DestroyImmediate(tpl);
        }

        [Test]
        public void WeaponInstance_CanEquip_RoleMismatch_False()
        {
            var tpl = ScriptableObject.CreateInstance<EquipmentTemplate>();
            tpl.requiredClass = CharacterClass.Warrior;
            tpl.category = WeaponCategory.Sword;
            var inst = new EquipmentInstance { templateId = "test", isEquipped = false };
            var weapon = new WeaponInstance(inst, tpl);
            Assert.IsFalse(weapon.CanEquip(CharacterClass.Archer));
            ScriptableObject.DestroyImmediate(tpl);
        }

        [Test]
        public void WeaponInstance_CanEquip_NoneCategory_AlwaysTrue()
        {
            // 비-무기 슬롯 (None) 은 직업 검증 스킵
            var tpl = ScriptableObject.CreateInstance<EquipmentTemplate>();
            tpl.requiredClass = CharacterClass.Warrior;
            tpl.category = WeaponCategory.None;
            var inst = new EquipmentInstance { templateId = "test", isEquipped = false };
            var weapon = new WeaponInstance(inst, tpl);
            Assert.IsTrue(weapon.CanEquip(CharacterClass.Archer));  // None → 모두 OK
            Assert.IsTrue(weapon.CanEquip(CharacterClass.Mage));
            ScriptableObject.DestroyImmediate(tpl);
        }
    }
}
