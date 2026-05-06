using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 — IAttackPattern 활성/해제 + Tick. EquipmentManager 가 instance 보유 (D1=A POCO).
    /// </summary>
    public class EquipmentPatternActivator
    {
        private readonly Transform _owner;
        private readonly PlayerStats _stats;
        private readonly List<IAttackPattern> _active = new List<IAttackPattern>();
        private readonly Dictionary<string, IAttackPattern> _instancePatterns = new Dictionary<string, IAttackPattern>();

        public EquipmentPatternActivator(Transform owner, PlayerStats stats)
        {
            _owner = owner; _stats = stats;
        }

        public void Tick(float deltaTime)
        {
            for (int i = 0; i < _active.Count; i++)
                _active[i].Tick(deltaTime);
        }

        public void ClearAll()
        {
            for (int i = 0; i < _active.Count; i++)
                _active[i].Cleanup();
            _active.Clear();
            _instancePatterns.Clear();
        }

        // EquipmentData (Data 레이어) 활성화
        public void Activate(EquipmentData data)
        {
            var pattern = EquipmentPatternFactory.Create(data.attackPattern);
            if (pattern == null) return;
            pattern.Initialize(data.attackConfig, _owner, _stats);
            _active.Add(pattern);
        }

        // EquipmentData 해제 — 같은 패턴 타입 제거
        public void Deactivate(EquipmentData data)
        {
            var targetType = EquipmentPatternFactory.GetType(data.attackPattern);
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].GetType() == targetType)
                {
                    _active[i].Cleanup();
                    _active.RemoveAt(i);
                    return;
                }
            }
        }

        // EquipmentInstance (가챠 무기) 활성화
        public bool ActivateInstance(EquipmentInstance inst, EquipmentTemplate template)
        {
            if (_instancePatterns.ContainsKey(inst.instanceId)) return false;
            if (template.attackPattern == AttackPatternType.None || template.attackConfig == null) return false;

            var pattern = EquipmentPatternFactory.Create(template.attackPattern);
            if (pattern == null) return false;
            pattern.Initialize(template.attackConfig, _owner, _stats);
            _active.Add(pattern);
            _instancePatterns[inst.instanceId] = pattern;
            return true;
        }

        // EquipmentInstance 해제
        public void DeactivateInstance(EquipmentInstance inst)
        {
            if (_instancePatterns.TryGetValue(inst.instanceId, out var pattern))
            {
                pattern.Cleanup();
                _active.Remove(pattern);
                _instancePatterns.Remove(inst.instanceId);
            }
        }

        public bool IsInstanceActive(string instanceId) => _instancePatterns.ContainsKey(instanceId);
    }
}
