using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 장비·affix·신화 효과 해석 결과 (equipment_v1 §4.3).
    /// PlayerStats 가 본 구조체를 받아 최종 스탯을 합산.
    /// ADD = 절대값 가산, MUL = 퍼센트(%) 합산 (10 = +10%, 1.10 배수).
    /// </summary>
    public class ResolvedEquipmentBonus
    {
        // 3 코어 (Spirit 폐기, stats_v2)
        public int StrengthAdd;
        public int AgilityAdd;
        public int WisdomAdd;

        // 전투 베이스 (ADD 절대값 + MUL %)
        public float MaxHealthAdd;
        public float MaxHealthMulPercent;     // affix_max_hp / sub_max_hp
        public float AttackDamageAdd;
        public float AttackDamageMulPercent;  // affix_attack_dmg / sub_attack_dmg
        public float AttackSpeedMulPercent;
        public float MoveSpeedAdd;
        public float RangeAdd;

        // 메이플 키우기 채널·데미지 버킷 (stats_v1 §6)
        public float FinalDamageBonusPercent;     // 버킷 2 (망토·하의·안경)
        public int   ExtraAttackTargets;
        public float ArmorPenetrationPercent;     // %
        public int   ArmorAdd;
        public float DamageReductionPercent;      // %
        public float HpRegenAdd;                  // /s
        public float CritChancePercent;           // %
        public float CritDamageAddPercent;        // % (베이스 1.5 별도)
        public float CompanionDurationBonusPercent;
        public float AdditiveDamageBonusPercent;  // 버킷 1

        // 메타 채널 (시뮬·런 외부 효과)
        public float XpGainPercent;
        public float DustGainPercent;
        public float DropRatePercent;

        // 발현된 신화 옵션 ID 목록 (조건 효과는 게임플레이 측에서 처리 — DamageSystem 등이 본 리스트 조회)
        public readonly List<string> ActiveMythicIds = new List<string>();
    }

    /// <summary>
    /// C4 — 장비 인스턴스 + 신화 옵션 통합 stats 해석 (equipment_v1 §4.3).
    /// 잭팟 ARPG Part 4 의 리졸버 알고리즘 + Little Hero 의 세트 폐기 결정 반영.
    ///
    /// 입력: 장착 EquipmentInstance 리스트 + 템플릿/신화 lookup.
    /// 처리: 각 인스턴스에 대해 baseStats → 주옵션 3 → 부옵션 3 → 신화 ID 수집.
    /// 출력: <see cref="ResolvedEquipmentBonus"/> — PlayerStats 가 본 결과를 base/level/core 와 합산.
    ///
    /// 세트 효과 단계는 생략 (README §1, equipment §3.6 결정).
    /// 신화 옵션 효과는 대부분 조건부(HP&lt;30% 등)이므로 ActiveMythicIds 만 수집,
    /// 실제 발동은 DamageSystem / PlayerHealth 등 런타임 측에서 처리.
    /// </summary>
    public static class EffectResolver
    {
        public static ResolvedEquipmentBonus Resolve(
            IReadOnlyList<EquipmentInstance> equipped,
            IReadOnlyDictionary<string, EquipmentTemplate> templates = null,
            IReadOnlyDictionary<string, MythicDefinition> mythics = null)
        {
            var bonus = new ResolvedEquipmentBonus();
            if (equipped == null) return bonus;

            for (int i = 0; i < equipped.Count; i++)
            {
                var inst = equipped[i];
                if (inst == null) continue;

                // 1. 템플릿 베이스 스탯 — StatModifiers 의 6 필드는 모두 ADD.
                //    C5: 슬롯 ⭐ 단계만큼 base 에 % 곱셈 (gacha_v1.md §3.3 — 장비 교체해도 ⭐ 유지).
                EquipmentTemplate template = null;
                if (templates != null && !string.IsNullOrEmpty(inst.templateId))
                    templates.TryGetValue(inst.templateId, out template);
                if (template != null && template.baseStats != null)
                {
                    float starforceMul = 1f + StarforceService.GetBaseMultiplier(template.slot);
                    AddBaseStats(bonus, template.baseStats, starforceMul);
                }

                // 2. 주옵션 3 + 부옵션 3
                ApplyAffixArray(bonus, inst.mainAffixes);
                ApplyAffixArray(bonus, inst.subAffixes);

                // 3. 신화 ID 수집 (효과 적용은 게임플레이 측)
                if (!string.IsNullOrEmpty(inst.mythicId))
                    bonus.ActiveMythicIds.Add(inst.mythicId);
            }

            return bonus;
        }

        private static void AddBaseStats(ResolvedEquipmentBonus bonus, StatModifiers s, float mul = 1f)
        {
            bonus.MaxHealthAdd     += s.maxHealthBonus     * mul;
            bonus.MoveSpeedAdd     += s.speedBonus         * mul;
            bonus.AttackDamageAdd  += s.attackDamageBonus  * mul;
            // attackSpeedBonus 은 SO 마스터값이라 ADD 로 취급 (베이스 attackSpeed 와 합산).
            bonus.AttackSpeedMulPercent += s.attackSpeedBonus * 100f * mul; // 0.1 → +10%
            bonus.RangeAdd         += s.rangeBonus         * mul;
            bonus.XpGainPercent    += s.xpGainBonus * 100f * mul;            // 0.2 → +20%
        }

        private static void ApplyAffixArray(ResolvedEquipmentBonus bonus, AffixLine[] lines)
        {
            if (lines == null) return;
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line == null) continue;
                ApplyAffixLine(bonus, line);
            }
        }

        private static void ApplyAffixLine(ResolvedEquipmentBonus bonus, AffixLine line)
        {
            if (!System.Enum.TryParse<StatTarget>(line.targetName, out var target))
            {
                Debug.LogWarning($"[EffectResolver] 알 수 없는 StatTarget: '{line.targetName}'");
                return;
            }

            // op 는 표시용 — 본 구조체 필드명이 이미 ADD/MUL 을 구분하므로
            // target 별로 정해진 채널에 그대로 누적.
            switch (target)
            {
                case StatTarget.FinalDamage:        bonus.FinalDamageBonusPercent     += line.value; break;
                case StatTarget.AttackDamage:       bonus.AttackDamageMulPercent      += line.value; break;
                case StatTarget.CritDamage:         bonus.CritDamageAddPercent        += line.value; break;
                case StatTarget.CritChance:         bonus.CritChancePercent           += line.value; break;
                case StatTarget.AttackSpeed:        bonus.AttackSpeedMulPercent       += line.value; break;
                case StatTarget.ArmorPenetration:   bonus.ArmorPenetrationPercent     += line.value; break;
                case StatTarget.ExtraAttackTargets: bonus.ExtraAttackTargets          += Mathf.RoundToInt(line.value); break;
                case StatTarget.MaxHealth:          bonus.MaxHealthMulPercent         += line.value; break;
                case StatTarget.Armor:              bonus.ArmorAdd                    += Mathf.RoundToInt(line.value); break;
                case StatTarget.DamageReduction:    bonus.DamageReductionPercent      += line.value; break;
                case StatTarget.HpRegen:            bonus.HpRegenAdd                  += line.value; break;
                case StatTarget.MoveSpeed:          bonus.MoveSpeedAdd                += line.value; break;
                case StatTarget.Range:              bonus.RangeAdd                    += line.value; break;

                case StatTarget.Strength: bonus.StrengthAdd += Mathf.RoundToInt(line.value); break;
                case StatTarget.Agility:  bonus.AgilityAdd  += Mathf.RoundToInt(line.value); break;
                case StatTarget.Wisdom:   bonus.WisdomAdd   += Mathf.RoundToInt(line.value); break;
                // Spirit 폐기 — Wisdom 으로 흡수
                case StatTarget.AllStats:
                {
                    int v = Mathf.RoundToInt(line.value);
                    bonus.StrengthAdd += v; bonus.AgilityAdd += v; bonus.WisdomAdd += v;
                    // SpiritAdd 제거 (Spirit 코어 폐기)
                    break;
                }

                case StatTarget.XpGain:            bonus.XpGainPercent              += line.value; break;
                case StatTarget.DustGain:          bonus.DustGainPercent            += line.value; break;
                case StatTarget.DropRate:          bonus.DropRatePercent            += line.value; break;
                case StatTarget.CompanionDuration: bonus.CompanionDurationBonusPercent += line.value; break;
            }
        }

        /// <summary>
        /// 신화 ID 의 조건 효과를 게임플레이 측에서 빠르게 조회하기 위한 헬퍼.
        /// 신화 효과의 실제 발동(HP&lt;30% 등)은 호출자 책임.
        /// </summary>
        public static bool HasMythic(ResolvedEquipmentBonus bonus, string mythicId)
        {
            if (bonus == null || string.IsNullOrEmpty(mythicId)) return false;
            return bonus.ActiveMythicIds.Contains(mythicId);
        }
    }
}
