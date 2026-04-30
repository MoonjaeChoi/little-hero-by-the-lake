using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    public class EquipmentManager : MonoBehaviour
    {
        private readonly Dictionary<SlotType, EquipmentData> _equipped
            = new Dictionary<SlotType, EquipmentData>();

        private readonly List<IAttackPattern> _activePatterns
            = new List<IAttackPattern>();

        private PlayerStats _stats;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        private void OnEnable()
        {
            GameEvents.OnPlayerDied     += ClearAllPatterns;
            GameEvents.OnChapterCleared += HandleChapterCleared;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied     -= ClearAllPatterns;
            GameEvents.OnChapterCleared -= HandleChapterCleared;
        }

        private void HandleChapterCleared(ChapterData _) => ClearAllPatterns();

        private void ClearAllPatterns()
        {
            for (int i = 0; i < _activePatterns.Count; i++)
                _activePatterns[i].Cleanup();
            _activePatterns.Clear();
            enabled = false;
        }

        private void Update()
        {
            for (int i = 0; i < _activePatterns.Count; i++)
                _activePatterns[i].Tick(Time.deltaTime);
        }

        public void Equip(EquipmentData data)
        {
            if (_equipped.TryGetValue(data.slot, out EquipmentData current))
                Unequip(current);

            _equipped[data.slot] = data;
            _stats.ApplyEquipmentBonus(data.statBonus);

            if (data.attackPattern != AttackPatternType.None)
                ActivatePattern(data);

            GameEvents.RaiseEquipmentChanged(data.slot, data);
        }

        public void Unequip(EquipmentData data)
        {
            if (!_equipped.ContainsKey(data.slot)) return;

            _equipped.Remove(data.slot);
            _stats.RemoveEquipmentBonus(data.statBonus);
            DeactivatePattern(data);

            GameEvents.RaiseEquipmentChanged(data.slot, null);
        }

        private void ActivatePattern(EquipmentData data)
        {
            IAttackPattern pattern = CreatePattern(data.attackPattern);
            if (pattern == null) return;

            pattern.Initialize(data.attackConfig, transform);
            _activePatterns.Add(pattern);
        }

        private void DeactivatePattern(EquipmentData data)
        {
            // 같은 타입 패턴 제거 (동일 슬롯 교체 시)
            for (int i = _activePatterns.Count - 1; i >= 0; i--)
            {
                if (_activePatterns[i].GetType() == GetPatternType(data.attackPattern))
                {
                    _activePatterns[i].Cleanup();
                    _activePatterns.RemoveAt(i);
                    return;
                }
            }
        }

        private static IAttackPattern CreatePattern(AttackPatternType type)
        {
            return type switch
            {
                AttackPatternType.Projectile => new ProjectileAttack(),
                AttackPatternType.Aura       => new AuraAttack(),
                AttackPatternType.Orbit      => new OrbitAttack(),
                AttackPatternType.Rain       => new RainAttack(),
                AttackPatternType.Trail      => new TrailAttack(),
                AttackPatternType.Pulse      => new PulseAttack(),
                AttackPatternType.Summon     => new SummonAttack(),
                _ => null
            };
        }

        private static System.Type GetPatternType(AttackPatternType type)
        {
            return type switch
            {
                AttackPatternType.Projectile => typeof(ProjectileAttack),
                AttackPatternType.Aura       => typeof(AuraAttack),
                AttackPatternType.Orbit      => typeof(OrbitAttack),
                AttackPatternType.Rain       => typeof(RainAttack),
                AttackPatternType.Trail      => typeof(TrailAttack),
                AttackPatternType.Pulse      => typeof(PulseAttack),
                AttackPatternType.Summon     => typeof(SummonAttack),
                _ => null
            };
        }
    }
}
