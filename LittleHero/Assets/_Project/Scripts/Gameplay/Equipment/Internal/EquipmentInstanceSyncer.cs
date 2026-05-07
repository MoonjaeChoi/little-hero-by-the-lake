using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5028 — SaveData.equipmentInstances → 활성화 + EffectResolver 통합.
    /// EquipmentManager facade 가 instance 보유 (D1=A POCO + Activator reference).
    /// </summary>
    public class EquipmentInstanceSyncer
    {
        private readonly EquipmentPatternActivator _activator;
        private readonly PlayerStats _stats;

        public EquipmentInstanceSyncer(EquipmentPatternActivator activator, PlayerStats stats)
        {
            _activator = activator; _stats = stats;
        }

        public void SyncEquippedInstances()
        {
            EquipmentTemplateRegistry.EnsureCache();
            var data = MetaProgression.Instance?.Data;
            if (data == null) return;
            if (data.equipmentInstances == null) data.equipmentInstances = new EquipmentInstance[0];

            // 5033.0f — TestEquip 폐기 후속: starter weapon 자동 생성 (CharacterData.starterWeaponTemplateId 기반)
            EnsureStarterWeapon(data);

            var playerClass = EquipmentClassResolver.ResolvePlayerClass();
            // 5033.2 — equippedByClass == playerClass 인 instance 만 활성. 다른 직업은 보존 (비활성).
            var equippedList = new List<EquipmentInstance>();
            foreach (var inst in data.equipmentInstances)
                if (inst != null && inst.EquippedByClass == playerClass) equippedList.Add(inst);

            int activated = 0;
            foreach (var inst in equippedList)
            {
                if (string.IsNullOrEmpty(inst.templateId)) continue;
                if (_activator.IsInstanceActive(inst.instanceId)) continue;
                if (!EquipmentTemplateRegistry.TryGet(inst.templateId, out var template) || template == null) continue;

                if (template.category != WeaponCategory.None && template.requiredClass != playerClass)
                {
                    // 5033.2 — equippedByClass 와 requiredClass 불일치 (사용자 설정 결함, ToggleEquip 가드 후 발생 가능성 낮음)
                    Debug.LogWarning($"[EquipmentInstanceSyncer] {template.id} 는 {template.requiredClass} 전용 (현재 {playerClass}) — 활성화 거부");
                    continue;
                }
                if (_activator.ActivateInstance(inst, template)) activated++;
            }

            // weapon_v2 시너지
            foreach (var inst in equippedList)
            {
                if (string.IsNullOrEmpty(inst.templateId)) continue;
                if (!EquipmentTemplateRegistry.TryGet(inst.templateId, out var tpl) || tpl == null) continue;
                if (tpl.category == WeaponCategory.None) continue;
                if (tpl.requiredClass != playerClass) continue;
                WeaponClassBonus.Apply(_stats, playerClass, tpl.category);
            }

            // EffectResolver 일괄 + ApplyResolvedBonus
            var resolved = EffectResolver.Resolve(equippedList, EquipmentTemplateRegistry.GetCache());
            if (resolved != null && _stats != null)
            {
                var statMod = new StatModifiers
                {
                    maxHealthBonus    = resolved.MaxHealthAdd,
                    speedBonus        = resolved.MoveSpeedAdd,
                    attackDamageBonus = resolved.AttackDamageAdd,
                    attackSpeedBonus  = 0f,
                    rangeBonus        = resolved.RangeAdd,
                    xpGainBonus       = 0f,
                };
                _stats.ApplyEquipmentBonus(statMod);
                _stats.ApplyResolvedBonus(resolved);
            }
            if (activated > 0) Debug.Log($"[EquipmentInstanceSyncer] 가챠 무기 {activated}종 활성화 + EffectResolver 통합");

            DamageDealer.SetActiveWeaponCategory(GetActiveWeaponCategory(data));

            // 5034 T03.5 — 직업별 특수 스탯 시너지 (D4=B). 누적 가드 (ClassBonusApplier 내부).
            ClassBonusApplier.Apply(_stats, playerClass);
        }

        public void EquipInstance(EquipmentInstance inst)
        {
            if (inst == null || string.IsNullOrEmpty(inst.templateId)) return;
            if (!EquipmentTemplateRegistry.TryGet(inst.templateId, out var template) || template == null) return;
            // 5033.2 — equippedByClass 갱신 (현재 직업으로 장착)
            inst.EquippedByClass = EquipmentClassResolver.ResolvePlayerClass();
            if (_activator.IsInstanceActive(inst.instanceId)) return;
            if (template.baseStats != null) _stats.ApplyEquipmentBonus(template.baseStats);
            _activator.ActivateInstance(inst, template);
            DamageDealer.SetActiveWeaponCategory(GetActiveWeaponCategory(MetaProgression.Instance?.Data));
        }

        public void UnequipInstance(EquipmentInstance inst)
        {
            if (inst == null) return;
            // 5033.2 — equippedByClass 해제
            inst.EquippedByClass = null;
            EquipmentTemplate template = null;
            if (!string.IsNullOrEmpty(inst.templateId))
                EquipmentTemplateRegistry.TryGet(inst.templateId, out template);
            if (template?.baseStats != null) _stats.RemoveEquipmentBonus(template.baseStats);
            _activator.DeactivateInstance(inst);
            DamageDealer.SetActiveWeaponCategory(GetActiveWeaponCategory(MetaProgression.Instance?.Data));
        }

        // 5007 — 현재 장착된 주 무기 카테고리 (5033.2 — equippedByClass 기반)
        public static WeaponCategory? GetActiveWeaponCategory(SaveData data)
        {
            if (data?.equipmentInstances == null) return null;
            var playerClass = EquipmentClassResolver.ResolvePlayerClass();
            foreach (var inst in data.equipmentInstances)
            {
                if (inst == null || inst.EquippedByClass != playerClass || string.IsNullOrEmpty(inst.templateId)) continue;
                if (!EquipmentTemplateRegistry.TryGet(inst.templateId, out var tpl) || tpl == null) continue;
                if (tpl.slot != EquipSlot.Weapon) continue;
                if (tpl.category == WeaponCategory.None) continue;
                return tpl.category;
            }
            return null;
        }

        // 5033.0f — Starter weapon 자동 생성 (직업 일치 Weapon 슬롯 비어있을 때 1회)
        // 5033.2 — equippedByClass 기반: 현재 직업으로 Weapon 슬롯 장착 instance 있으면 skip.
        //          mismatch 인스턴스는 보존 (다른 직업에서 장착된 상태). 직업 변경 시 자동 활성화.
        private void EnsureStarterWeapon(SaveData data)
        {
            var playerClass = EquipmentClassResolver.ResolvePlayerClass();

            // 현재 직업의 Weapon 슬롯 장착 instance 있으면 skip
            foreach (var inst in data.equipmentInstances)
            {
                if (inst == null || inst.EquippedByClass != playerClass || string.IsNullOrEmpty(inst.templateId)) continue;
                if (!EquipmentTemplateRegistry.TryGet(inst.templateId, out var tpl) || tpl == null) continue;
                if (tpl.slot == EquipSlot.Weapon) return;
            }

            var character = MetaProgressionCharacter.SelectedCharacter;
            if (character == null || string.IsNullOrEmpty(character.starterWeaponTemplateId)) return;

            if (!EquipmentTemplateRegistry.TryGet(character.starterWeaponTemplateId, out var starterTpl) || starterTpl == null)
            {
                Debug.LogWarning($"[EquipmentInstanceSyncer] starter template '{character.starterWeaponTemplateId}' 미등록");
                return;
            }

            var starter = new EquipmentInstance
            {
                instanceId = "starter_" + character.starterWeaponTemplateId + "_" + System.Guid.NewGuid().ToString("N").Substring(0, 8),
                templateId = character.starterWeaponTemplateId,
                enhanceLevel = 0,
                isEquipped = true,  // backwards compat (마이그레이션 후 폐기)
                equippedByClassInt = (int)playerClass,
            };

            var list = new List<EquipmentInstance>(data.equipmentInstances) { starter };
            data.equipmentInstances = list.ToArray();
            SaveSystem.Save(data);
            Debug.Log($"[EquipmentInstanceSyncer] starter weapon 자동 생성: {starter.templateId} (class={playerClass}, instanceId={starter.instanceId})");
        }

        public void HandleClassChanged(CharacterClass newClass)
        {
            // 5033.2 — equippedByClass 기반: 직업 변경 시 unequip 강제 폐기.
            // 이전 직업의 장착 instance 보존 (equippedByClass 유지) → 복귀 시 자동 활성.
            // _activator 의 _active 만 새 직업으로 재구성.
            EquipmentTemplateRegistry.EnsureCache();
            _activator.ClearAll();
            SyncEquippedInstances();
            Debug.Log($"[EquipmentInstanceSyncer] 캐릭터 직업 변경 ({newClass}) — Sync 재실행 (이전 직업 장착 보존)");
        }
    }
}
