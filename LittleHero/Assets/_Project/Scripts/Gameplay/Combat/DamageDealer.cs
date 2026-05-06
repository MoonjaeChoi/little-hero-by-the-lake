using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v4 — 다중 타깃 데미지 적용 정적 헬퍼 (damage_system_v2.md §5.2).
    /// 메이플 키우기 풍 다중 타깃 보정 (Little Hero 완화: 60% cap).
    /// 옵션 Z 사이클 2 — EnhanceDamage 헬퍼로 PlayerStats.AttackDamage / FinalDamage 통합.
    /// </summary>
    public static class DamageDealer
    {
        // 메이플 26% 보정 → Little Hero 완화 (active gameplay)
        // 1차 100% / 2차 80% / 3차 70% / 4차+ 60% (cap)
        private static readonly float[] _multiTargetPenalty = { 1f, 0.8f, 0.7f, 0.6f };
        private const float MULTI_PENALTY_FLOOR = 0.6f;

        // 옵션 Z 사이클 2 — PlayerStats 캐시 (DamageSystem 패턴 답습)
        private static PlayerStats _statsCache;

        // 5007 — 현재 장착 무기 카테고리 cache (EquipmentManager 가 무기 변경 시 SetActiveWeaponCategory 호출)
        private static WeaponCategory? _activeWeaponCategory;

        /// <summary>5007 T06 — EquipmentManager 가 무기 장착·해제 시 호출 (cache 갱신).</summary>
        public static void SetActiveWeaponCategory(WeaponCategory? category)
        {
            _activeWeaponCategory = category;
        }

        private static PlayerStats GetStats()
        {
            if (_statsCache != null) return _statsCache;
            _statsCache = Object.FindAnyObjectByType<PlayerStats>(FindObjectsInactive.Include);
            return _statsCache;
        }

        /// <summary>
        /// 옵션 Z — 패턴 베이스 데미지 + PlayerStats.AttackDamage 합산 + FinalDamage% 곱셈.
        /// 메이플 키우기 표준: STR+1 = ATK+1, FinalDamage% = 곱셈 채널.
        /// 강화/affix/마법의돌 → PlayerStats.AttackDamage 갱신 → 모든 패턴 자동 반영.
        /// </summary>
        public static float EnhanceDamage(float baseDamage)
        {
            var s = GetStats();
            if (s == null) return baseDamage;
            // 메이플 풍 — base + STR-derived 합산 (이미 PlayerStats.AttackDamage 가 baseConfig + bonus 합산)
            float dmg = baseDamage + s.AttackDamage;
            // FinalDamage% (망토·하의·안경) + AdditiveDamageBonus% (옵션 라인) 합산 곱셈
            float mul = 1f + (s.FinalDamageBonus + s.AdditiveDamageBonus) / 100f;
            return dmg * mul;
        }

        /// <summary>다중 타깃 데미지 — 거리 정렬된 list 가정. 5029 — element 매개변수 default None (전사/궁수 호환).</summary>
        public static void ApplyMulti(List<EnemyController> targets, float baseDamage, WeaponCategory? weaponCategory = null, MagicElement element = MagicElement.None)
        {
            var wc = weaponCategory ?? _activeWeaponCategory;
            float effective = EnhanceDamage(baseDamage);
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i] == null || targets[i].IsDead) continue;
                float penalty = i < _multiTargetPenalty.Length ? _multiTargetPenalty[i] : MULTI_PENALTY_FLOOR;
                float elemMul = ElementResolver.ResolveMultiplier(targets[i], element);
                float finalDmg = effective * penalty * elemMul;
                DamageSystem.DealDamage(targets[i], finalDmg, wc);
                ElementResolver.ApplyElementEffect(targets[i], element, finalDmg);
            }
        }

        /// <summary>단일 타깃 — 100% 데미지. 5029 — element 매개변수.</summary>
        public static void ApplySingle(EnemyController target, float damage, WeaponCategory? weaponCategory = null, MagicElement element = MagicElement.None)
        {
            if (target == null || target.IsDead) return;
            var wc = weaponCategory ?? _activeWeaponCategory;
            float elemMul = ElementResolver.ResolveMultiplier(target, element);
            float finalDmg = EnhanceDamage(damage) * elemMul;
            DamageSystem.DealDamage(target, finalDmg, wc);
            ElementResolver.ApplyElementEffect(target, element, finalDmg);
        }

        /// <summary>다중 타깃 보정 비율 미리보기 (UI / 시뮬용).</summary>
        public static float GetPenalty(int targetIndex)
        {
            if (targetIndex < 0) return 1f;
            return targetIndex < _multiTargetPenalty.Length ? _multiTargetPenalty[targetIndex] : MULTI_PENALTY_FLOOR;
        }
    }
}
