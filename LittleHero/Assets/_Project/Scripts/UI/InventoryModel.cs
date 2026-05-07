using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// architecture_analysis_v1 §3.3.A — InventorySelectionPanel MVP 분리 (Model).
    /// 데이터 + 비즈니스 로직만. UI 갱신은 Panel(View) 책임.
    /// </summary>
    public struct InventoryMatch
    {
        public EquipmentInstance instance;
        public EquipmentTemplate template;
    }

    public class InventoryModel
    {
        private EquipmentTemplate[] _templateLookup;

        public InventoryModel(EquipmentTemplate[] templates)
        {
            _templateLookup = templates;
        }

        public void SetTemplates(EquipmentTemplate[] templates)
        {
            _templateLookup = templates;
        }

        /// <summary>슬롯 매칭 인스턴스 list 수집 + 정렬 (장착중 우선 → 등급 → 강화).</summary>
        public List<InventoryMatch> CollectMatches(EquipSlot slot)
        {
            var matches = new List<InventoryMatch>();
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) return matches;
            for (int i = 0; i < data.equipmentInstances.Length; i++)
            {
                var inst = data.equipmentInstances[i];
                if (inst == null) continue;
                var tpl = FindTemplate(inst.templateId);
                if (tpl == null || tpl.slot != slot) continue;
                matches.Add(new InventoryMatch { instance = inst, template = tpl });
            }
            matches.Sort((a, b) =>
            {
                int e = (b.instance.IsEquippedByCurrentClass() ? 1 : 0) - (a.instance.IsEquippedByCurrentClass() ? 1 : 0);
                if (e != 0) return e;
                int r = ((int)b.template.rarity).CompareTo((int)a.template.rarity);
                if (r != 0) return r;
                return b.instance.enhanceLevel.CompareTo(a.instance.enhanceLevel);
            });
            return matches;
        }

        /// <summary>5033.2 — 직업별 장착 토글. equippedByClass == 현재 직업 인 같은 슬롯 다른 instance 자동 해제.</summary>
        public bool ToggleEquip(EquipmentInstance inst, EquipmentTemplate tpl)
        {
            if (inst == null || tpl == null) return false;
            var data = MetaProgression.Instance?.Data;
            if (data?.equipmentInstances == null) return false;
            var em = Object.FindAnyObjectByType<EquipmentManager>(FindObjectsInactive.Include);
            var playerClass = EquipmentClassResolver.ResolvePlayerClass();

            // 직업 mismatch — 장착 거부 (UI 가드)
            if (tpl.category != WeaponCategory.None && tpl.requiredClass != playerClass) return false;

            if (inst.EquippedByClass == playerClass)
            {
                // 이미 현재 직업으로 장착 중 → unequip
                if (em != null) em.UnequipInstance(inst);
            }
            else
            {
                // 같은 슬롯의 현재 직업 장착 다른 instance 해제
                for (int i = 0; i < data.equipmentInstances.Length; i++)
                {
                    var other = data.equipmentInstances[i];
                    if (other == null || other == inst || other.EquippedByClass != playerClass) continue;
                    var ot = FindTemplate(other.templateId);
                    if (ot == null || ot.slot != tpl.slot) continue;
                    if (em != null) em.UnequipInstance(other);
                }
                if (em != null) em.EquipInstance(inst);
            }
            SaveSystem.Save(data);
            return true;
        }

        public EquipmentTemplate FindTemplate(string id)
        {
            if (_templateLookup == null || string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < _templateLookup.Length; i++)
                if (_templateLookup[i] != null && _templateLookup[i].id == id) return _templateLookup[i];
            return null;
        }
    }
}
