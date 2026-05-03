using System.Globalization;

namespace LittleHero
{
    /// <summary>
    /// inventory_ui_v2.md §3 — StatTarget enum 한국어 라벨 + AffixLine 포맷 헬퍼.
    /// </summary>
    public static class StatTargetLabel
    {
        public static string Korean(string targetName)
        {
            switch (targetName)
            {
                case "FinalDamage":        return "최종 데미지";
                case "AttackDamage":       return "공격력";
                case "CritDamage":         return "치명타 피해";
                case "CritChance":         return "치명타 확률";
                case "AttackSpeed":        return "공격속도";
                case "ArmorPenetration":   return "방어 관통";
                case "ExtraAttackTargets": return "다중 타깃";
                case "MaxHealth":          return "최대 체력";
                case "Armor":              return "방어력";
                case "DamageReduction":    return "피해 감소";
                case "HpRegen":            return "체력 재생";
                case "MoveSpeed":          return "이동속도";
                case "Range":              return "사거리";
                case "Strength":           return "힘";
                case "Agility":            return "민첩";    // stats_v2 (빠름 → 민첩)
                case "Wisdom":             return "지혜";    // stats_v2 (슬기 → 지혜)
                // Spirit 폐기 (Wisdom 흡수)
                case "AllStats":           return "올스탯";
                case "XpGain":             return "경험치 획득";
                case "DustGain":           return "별가루 획득";
                case "DropRate":           return "드랍률";
                case "CompanionDuration":  return "동료 지속";
                // stats_v2 신규 9
                case "EvasionRate":        return "회피";
                case "StatusResist":       return "상태이상 저항";
                case "ArmorIgnore":        return "방어 무시";
                case "ChainTargets":       return "연쇄 타깃";
                case "LifestealRatio":     return "흡혈";
                case "CompanionDamage":    return "동료 데미지";
                case "MagicDamage":        return "마법 데미지";
                case "MagicCritChance":    return "마법 치명타 확률";
                case "MagicCritDamage":    return "마법 치명타 피해";
                default:                   return targetName;
            }
        }

        public static string Format(AffixLine line)
        {
            if (line == null || string.IsNullOrEmpty(line.targetName)) return "";
            string label = Korean(line.targetName);
            string suffix = line.opName == "MUL" ? "%" : "";
            string sign = line.value >= 0 ? "+" : "";
            string val = line.value.ToString("0.##", CultureInfo.InvariantCulture);
            string lockMark = line.locked ? "🔒 " : "▸ ";
            return $"{lockMark}{label} {sign}{val}{suffix}";
        }
    }
}
