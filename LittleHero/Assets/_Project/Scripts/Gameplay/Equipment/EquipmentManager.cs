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
            // weapon_v2 후속 — 직업 변경 시 무기 자동 unequip
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

        // weapon_v2 후속 — 직업 변경 시 잘못된 직업 무기 자동 해제
        private void HandleClassChanged(CharacterClass newClass)
        {
            EnsureTemplateCache();
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) return;
            int unequipped = 0;
            foreach (var inst in data.equipmentInstances)
            {
                if (inst == null || !inst.isEquipped) continue;
                if (string.IsNullOrEmpty(inst.templateId)) continue;
                if (!_templateCache.TryGetValue(inst.templateId, out var tpl) || tpl == null) continue;
                if (tpl.category == WeaponCategory.None) continue;  // 비-무기 슬롯 스킵
                if (tpl.requiredClass == newClass) continue;  // 매칭 OK

                inst.isEquipped = false;
                UnequipInstance(inst);
                unequipped++;
            }
            if (unequipped > 0)
            {
                SaveSystem.Save(data);
                Debug.Log($"[EquipmentManager] 캐릭터 직업 변경 ({newClass}) — {unequipped}개 무기 자동 해제");
            }
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

        /// <summary>
        /// SaveData.equipmentInstances 의 isEquipped=true 인스턴스 모두 자동 장착.
        /// stats_v2 §11 — EffectResolver 호출 + ApplyResolvedBonus 통합 (affix MUL% 자동 반영).
        /// </summary>
        public void SyncEquippedInstances()
        {
            EnsureTemplateCache();
            var data = MetaProgression.Instance?.Data;
            if (data == null || data.equipmentInstances == null) return;

            // 장착 인스턴스만 모음 + weapon_v2 직업 검증
            var playerClass = ResolvePlayerClass();
            var equippedList = new List<EquipmentInstance>();
            foreach (var inst in data.equipmentInstances)
                if (inst != null && inst.isEquipped) equippedList.Add(inst);

            int activated = 0;
            foreach (var inst in equippedList)
            {
                if (string.IsNullOrEmpty(inst.templateId)) continue;
                if (_instancePatterns.ContainsKey(inst.instanceId)) continue;

                EquipmentTemplate template;
                if (!_templateCache.TryGetValue(inst.templateId, out template) || template == null) continue;

                // weapon_v2 — 직업 검증 (잘못된 직업 무기 거부)
                if (template.category != WeaponCategory.None && template.requiredClass != playerClass)
                {
                    Debug.LogWarning($"[EquipmentManager] {template.id} 는 {template.requiredClass} 전용 (현재 {playerClass}) — 활성화 거부");
                    continue;
                }

                // attackPattern 활성화 (None 제외) — baseStats 가산은 EffectResolver 통합 (아래)
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

            // weapon_v2 시너지 — 본인 직업 무기 카테고리 보너스
            foreach (var inst in equippedList)
            {
                if (string.IsNullOrEmpty(inst.templateId)) continue;
                if (!_templateCache.TryGetValue(inst.templateId, out var tpl) || tpl == null) continue;
                if (tpl.category == WeaponCategory.None) continue;
                if (tpl.requiredClass != playerClass) continue;
                WeaponClassBonus.Apply(_stats, playerClass, tpl.category);
            }

            // stats_v2 — EffectResolver 일괄 + PlayerStats.ApplyResolvedBonus
            // baseStats × StarforceMul + affix MUL% / FinalDmg% / CritChance% / ArmorIgnore% 등 반영
            var resolved = EffectResolver.Resolve(equippedList, _templateCache);
            if (resolved != null && _stats != null)
            {
                // baseStats 채널 (MaxHealth/Speed/AtkDmg/AtkSpd/Range/XpGain) 적용
                var statMod = new StatModifiers
                {
                    maxHealthBonus    = resolved.MaxHealthAdd,
                    speedBonus        = resolved.MoveSpeedAdd,
                    attackDamageBonus = resolved.AttackDamageAdd,
                    attackSpeedBonus  = 0f,  // EffectResolver 가 이미 % 처리, 별도 가산 X
                    rangeBonus        = resolved.RangeAdd,
                    xpGainBonus       = 0f,
                };
                _stats.ApplyEquipmentBonus(statMod);
                // affix MUL% / 신규 채널 적용
                _stats.ApplyResolvedBonus(resolved);
            }
            if (activated > 0) Debug.Log($"[EquipmentManager] 가챠 무기 {activated}종 활성화 + EffectResolver 통합");

            // 5007 T06 — DamageDealer 의 정적 cache 갱신 (IAttackPattern 변경 X)
            DamageDealer.SetActiveWeaponCategory(GetActiveWeaponCategory());
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

            // 5007 T06 — 무기 장착 시 DamageDealer cache 갱신
            DamageDealer.SetActiveWeaponCategory(GetActiveWeaponCategory());
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

            // 5007 T06 — 무기 해제 시 DamageDealer cache 갱신
            DamageDealer.SetActiveWeaponCategory(GetActiveWeaponCategory());
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

        // weapon_v2 — CharacterData.className 한국어 → CharacterClass enum
        public static CharacterClass ResolvePlayerClass()
        {
            var ch = MetaProgression.Instance?.SelectedCharacter;
            if (ch == null || string.IsNullOrEmpty(ch.className)) return CharacterClass.Warrior;
            switch (ch.className)
            {
                case "전사": return CharacterClass.Warrior;
                case "궁수": return CharacterClass.Archer;
                case "마법사": return CharacterClass.Mage;
                default: return CharacterClass.Warrior;
            }
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

        // CLAUDE.md §15.1 SOLID-O — Factory 추출 (EquipmentPatternFactory)
        private static IAttackPattern CreatePattern(AttackPatternType type) => EquipmentPatternFactory.Create(type);
        private static System.Type GetPatternType(AttackPatternType type) => EquipmentPatternFactory.GetType(type);

        /// <summary>5007 — 현재 장착된 주 무기 (EquipSlot.Weapon) 의 카테고리. 미장착 시 null.</summary>
        public WeaponCategory? GetActiveWeaponCategory()
        {
            if (MetaProgression.Instance?.Data == null) return null;
            foreach (var inst in MetaProgression.Instance.Data.equipmentInstances)
            {
                if (inst == null || !inst.isEquipped || string.IsNullOrEmpty(inst.templateId)) continue;
                if (!_templateCache.TryGetValue(inst.templateId, out var tpl) || tpl == null) continue;
                if (tpl.slot != EquipSlot.Weapon) continue;
                if (tpl.category == WeaponCategory.None) continue;
                return tpl.category;
            }
            return null;
        }
    }
}
