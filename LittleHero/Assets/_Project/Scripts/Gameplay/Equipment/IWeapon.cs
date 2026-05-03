namespace LittleHero
{
    /// <summary>
    /// weapon_system_v2.md §4.1.D — IWeapon 추상 인터페이스 (SOLID-I).
    /// 무기 = EquipmentInstance + EquipmentTemplate 결합.
    /// </summary>
    public interface IWeapon
    {
        EquipmentInstance Instance { get; }
        EquipmentTemplate Template { get; }
        CharacterClass RequiredClass { get; }
        WeaponCategory Category { get; }
        bool CanEquip(CharacterClass playerClass);
    }

    /// <summary>
    /// IWeapon 표준 구현체. EquipmentInstance + Template 결합 wrapper.
    /// </summary>
    public sealed class WeaponInstance : IWeapon
    {
        public EquipmentInstance Instance { get; }
        public EquipmentTemplate Template { get; }
        public CharacterClass RequiredClass => Template != null ? Template.requiredClass : CharacterClass.Warrior;
        public WeaponCategory Category => Template != null ? Template.category : WeaponCategory.None;

        public WeaponInstance(EquipmentInstance instance, EquipmentTemplate template)
        {
            Instance = instance;
            Template = template;
        }

        public bool CanEquip(CharacterClass playerClass)
        {
            if (Template == null) return false;
            // 비-무기 슬롯 (None) 은 직업 검증 X
            if (Template.category == WeaponCategory.None) return true;
            return Template.requiredClass == playerClass;
        }
    }
}
