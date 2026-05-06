using NUnit.Framework;
using System.Reflection;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5024 T02 — Equipment 영역 Test (EquipmentManager + WeaponClassBonus + Affix).
    /// EquipmentManager 는 MonoBehaviour 의존성 다수 (PlayerStats / SaveData 등) 로
    /// EditMode 한계 — 정적 영역 (WeaponClassBonus) + reflection 검증 위주.
    /// </summary>
    public class EquipmentManagerTests
    {
        // === WeaponClassBonus 정적 검증 (5001 weapon_v2 cascade) ===

        [Test]
        public void WeaponClassBonus_Warrior_Sword_Matches()
        {
            Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Warrior, WeaponCategory.Sword));
        }

        [Test]
        public void WeaponClassBonus_Warrior_Bow_DoesNotMatch()
        {
            Assert.IsFalse(WeaponClassBonus.IsClassMatch(CharacterClass.Warrior, WeaponCategory.Bow));
        }

        [Test]
        public void WeaponClassBonus_Archer_Bow_Matches()
        {
            Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Archer, WeaponCategory.Bow));
        }

        [Test]
        public void WeaponClassBonus_Mage_Staff_Matches()
        {
            Assert.IsTrue(WeaponClassBonus.IsClassMatch(CharacterClass.Mage, WeaponCategory.Staff));
        }

        // === EquipmentManager Type 존재 + 메서드 시그니처 (reflection) ===

        [Test]
        public void EquipmentManager_Type_Exists()
        {
            var t = System.Type.GetType("LittleHero.EquipmentManager, Assembly-CSharp");
            Assert.IsNotNull(t, "LittleHero.EquipmentManager 타입 누락");
        }

        [Test]
        public void EquipmentManager_HasRegisterTemplates_Method()
        {
            var t = System.Type.GetType("LittleHero.EquipmentManager, Assembly-CSharp");
            var method = t?.GetMethod("RegisterTemplates", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(method, "EquipmentManager.RegisterTemplates static 메서드 누락 (옵션 Z 사이클 1)");
        }

        // === Affix 영역 — EffectResolver 정적 검증 ===

        [Test]
        public void EffectResolver_Type_Exists()
        {
            var t = System.Type.GetType("LittleHero.EffectResolver, Assembly-CSharp");
            Assert.IsNotNull(t, "LittleHero.EffectResolver 타입 누락 (Affix → PlayerStats 변환)");
        }
    }
}
