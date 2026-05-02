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

        // 옵션 Z 사이클 1 — 가챠 무기 인스턴스 추적 (가챠 무기 사이클 dispose 용)
        private readonly Dictionary<string, IAttackPattern> _instancePatterns
            = new Dictionary<string, IAttackPattern>();

        private PlayerStats _stats;

        // 가챠 무기 템플릿 lookup (Resources 자동 로드)
        private static Dictionary<string, EquipmentTemplate> _templateCache;

        private void Awake()
        {
            _stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            // 옵션 Z — 게임 시작 시 SaveData 의 isEquipped 인스턴스 자동 장착
            SyncEquippedInstances();
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
            _instancePatterns.Clear();
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

        // ===== 옵션 Z 사이클 1 — 가챠 무기 인스턴스 장착 =====

        /// <summary>SaveData.equipmentInstances 의 isEquipped=true 인스턴스 모두 자동 장착.</summary>
        public void SyncEquippedInstances()
        {
            EnsureTemplateCache();
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.equipmentInstances == null) return;

            int activated = 0;
            foreach (var inst in data.equipmentInstances)
            {
                if (inst == null || !inst.isEquipped) continue;
                if (string.IsNullOrEmpty(inst.templateId)) continue;
                if (_instancePatterns.ContainsKey(inst.instanceId)) continue;

                EquipmentTemplate template;
                if (!_templateCache.TryGetValue(inst.templateId, out template) || template == null) continue;

                // 1. baseStats 적용 (StatModifiers — PlayerStats 합산)
                if (template.baseStats != null) _stats.ApplyEquipmentBonus(template.baseStats);

                // 2. attackPattern 활성화 (None 제외)
                if (template.attackPattern != AttackPatternType.None && template.attackConfig != null)
                {
                    var pattern = CreatePattern(template.attackPattern);
                    if (pattern != null)
                    {
                        pattern.Initialize(template.attackConfig, transform, _stats);
                        _activePatterns.Add(pattern);
                        _instancePatterns[inst.instanceId] = pattern;
                        activated++;
                    }
                }
            }
            if (activated > 0) Debug.Log($"[EquipmentManager] 가챠 무기 {activated}종 활성화");
        }

        /// <summary>인스턴스 단일 장착 (InventoryPanel 등에서 호출).</summary>
        public void EquipInstance(EquipmentInstance inst)
        {
            if (inst == null || string.IsNullOrEmpty(inst.templateId)) return;
            EnsureTemplateCache();
            EquipmentTemplate template;
            if (!_templateCache.TryGetValue(inst.templateId, out template) || template == null) return;

            if (_instancePatterns.ContainsKey(inst.instanceId)) return;
            if (template.baseStats != null) _stats.ApplyEquipmentBonus(template.baseStats);

            if (template.attackPattern != AttackPatternType.None && template.attackConfig != null)
            {
                var pattern = CreatePattern(template.attackPattern);
                if (pattern != null)
                {
                    pattern.Initialize(template.attackConfig, transform, _stats);
                    _activePatterns.Add(pattern);
                    _instancePatterns[inst.instanceId] = pattern;
                }
            }
        }

        /// <summary>인스턴스 단일 해제.</summary>
        public void UnequipInstance(EquipmentInstance inst)
        {
            if (inst == null) return;
            EnsureTemplateCache();
            EquipmentTemplate template = null;
            if (!string.IsNullOrEmpty(inst.templateId))
                _templateCache.TryGetValue(inst.templateId, out template);

            if (template?.baseStats != null) _stats.RemoveEquipmentBonus(template.baseStats);

            if (_instancePatterns.TryGetValue(inst.instanceId, out var pattern))
            {
                pattern.Cleanup();
                _activePatterns.Remove(pattern);
                _instancePatterns.Remove(inst.instanceId);
            }
        }

        private static void EnsureTemplateCache()
        {
            if (_templateCache != null) return;
            _templateCache = new Dictionary<string, EquipmentTemplate>();
            // Editor / Build 에서 모두 작동하도록 Resources 또는 직접 로드 — 본 캐시는
            // GachaPool.allTemplates 또는 AssetDatabase 검색을 통해 채울 수도 있음.
            // 현재는 GachaService.Initialize 시 호출 가능하도록 public Register 도 제공.
            var all = Resources.LoadAll<EquipmentTemplate>("");
            foreach (var t in all)
                if (t != null && !string.IsNullOrEmpty(t.id) && !_templateCache.ContainsKey(t.id))
                    _templateCache[t.id] = t;
        }

        public static void RegisterTemplates(IEnumerable<EquipmentTemplate> templates)
        {
            if (_templateCache == null) _templateCache = new Dictionary<string, EquipmentTemplate>();
            foreach (var t in templates)
                if (t != null && !string.IsNullOrEmpty(t.id)) _templateCache[t.id] = t;
        }

        private void ActivatePattern(EquipmentData data)
        {
            IAttackPattern pattern = CreatePattern(data.attackPattern);
            if (pattern == null) return;

            pattern.Initialize(data.attackConfig, transform, _stats);
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
                AttackPatternType.Slash      => new SlashAttack(),
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
                AttackPatternType.Slash      => typeof(SlashAttack),
                _ => null
            };
        }
    }
}
