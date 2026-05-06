using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 facade — Equip/Unequip + 위임 (Activator/Syncer/Registry/ClassResolver).
    /// MonoBehaviour Awake/Update 보존, 호출처 시그니처 보존.
    /// </summary>
    public class EquipmentManager : MonoBehaviour
    {
        private readonly Dictionary<SlotType, EquipmentData> _equipped = new();
        private PlayerStats _stats;
        private EquipmentPatternActivator _activator;
        private EquipmentInstanceSyncer _syncer;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
            _activator = new EquipmentPatternActivator(transform, _stats);
            _syncer = new EquipmentInstanceSyncer(_activator, _stats);
        }

        private void Start() => _syncer.SyncEquippedInstances();

        private void OnEnable()
        {
            GameEvents.OnPlayerDied     += ClearAllPatterns;
            GameEvents.OnChapterCleared += HandleChapterCleared;
            if (MetaProgression.Instance != null)
                MetaProgression.OnCharacterClassChanged += HandleClassChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied     -= ClearAllPatterns;
            GameEvents.OnChapterCleared -= HandleChapterCleared;
            if (MetaProgression.Instance != null)
                MetaProgression.OnCharacterClassChanged -= HandleClassChanged;
        }

        private void Update()
        {
            if (_activator == null) return;
            _activator.Tick(Time.deltaTime);
        }

        private void HandleClassChanged(CharacterClass newClass) => _syncer.HandleClassChanged(newClass);
        private void HandleChapterCleared(ChapterData _) => ClearAllPatterns();

        private void ClearAllPatterns()
        {
            _activator.ClearAll();
            enabled = false;
        }

        public void Equip(EquipmentData data)
        {
            if (_equipped.TryGetValue(data.slot, out EquipmentData current))
                Unequip(current);

            _equipped[data.slot] = data;
            _stats.ApplyEquipmentBonus(data.statBonus);

            if (data.attackPattern != AttackPatternType.None)
                _activator.Activate(data);

            GameEvents.RaiseEquipmentChanged(data.slot, data);
        }

        public void Unequip(EquipmentData data)
        {
            if (!_equipped.ContainsKey(data.slot)) return;
            _equipped.Remove(data.slot);
            _stats.RemoveEquipmentBonus(data.statBonus);
            _activator.Deactivate(data);
            GameEvents.RaiseEquipmentChanged(data.slot, null);
        }

        public void SyncEquippedInstances() => _syncer.SyncEquippedInstances();
        public void EquipInstance(EquipmentInstance inst) => _syncer.EquipInstance(inst);
        public void UnequipInstance(EquipmentInstance inst) => _syncer.UnequipInstance(inst);

        // 정적 위임 (호출처 호환)
        public static CharacterClass ResolvePlayerClass() => EquipmentClassResolver.ResolvePlayerClass();
        public static void RegisterTemplates(IEnumerable<EquipmentTemplate> templates) => EquipmentTemplateRegistry.RegisterTemplates(templates);

        public WeaponCategory? GetActiveWeaponCategory() => EquipmentInstanceSyncer.GetActiveWeaponCategory(MetaProgression.Instance?.Data);
    }
}
