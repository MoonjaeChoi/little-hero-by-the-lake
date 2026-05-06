using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029 — element 저항 곱 + element effect 적용.
    /// 효과 (PRD §3.4 D1=A 수치):
    ///   Fire: 추가 데미지 30% (DOT 단순화 — 즉시 적용)
    ///   Ice: 추가 데미지 20% + 둔화 placeholder (5029.1 후속)
    ///   Lightning: 인접 적 1 추가 데미지 50% (사슬)
    ///   Arcane: 저항 무시 (1.0× 강제)
    /// </summary>
    public static class ElementResolver
    {
        private const float FIRE_DOT_MUL = 0.30f;
        private const float ICE_BONUS_MUL = 0.20f;
        private const float LIGHTNING_CHAIN_MUL = 0.50f;
        private const float LIGHTNING_CHAIN_RANGE = 3f;

        /// <summary>저항 곱 — Arcane 은 1.0 강제 (무시).</summary>
        public static float ResolveMultiplier(EnemyController enemy, MagicElement element)
        {
            if (element == MagicElement.None) return 1f;
            if (element == MagicElement.Arcane) return 1f;  // 저항 무시
            if (enemy == null || enemy.Data == null) return 1f;
            var res = enemy.Data.elementResistance;
            if (res == null || res.Length != 4) return 1f;
            int idx = (int)element - 1;  // Fire=1 → idx=0
            if (idx < 0 || idx >= 4) return 1f;
            return res[idx];
        }

        /// <summary>element effect 적용 — DamageDealer 가 호출. baseDamage 는 EnhanceDamage 후 값.</summary>
        public static void ApplyElementEffect(EnemyController target, MagicElement element, float baseDamage)
        {
            if (target == null || target.IsDead) return;
            if (element == MagicElement.None) return;

            switch (element)
            {
                case MagicElement.Fire:
                    DamageSystem.DealDamage(target, baseDamage * FIRE_DOT_MUL, null);
                    break;
                case MagicElement.Ice:
                    DamageSystem.DealDamage(target, baseDamage * ICE_BONUS_MUL, null);
                    // 둔화 visual placeholder — 5029.1 후속
                    break;
                case MagicElement.Lightning:
                    ApplyChainLightning(target, baseDamage);
                    break;
                case MagicElement.Arcane:
                    // 저항 무시는 ResolveMultiplier 에서 처리, 여기는 noop
                    break;
            }
        }

        private static void ApplyChainLightning(EnemyController source, float baseDamage)
        {
            var nearest = DamageQuery.FindNearest(source.transform.position, LIGHTNING_CHAIN_RANGE);
            if (nearest != null && !nearest.IsDead && nearest != source)
                DamageSystem.DealDamage(nearest, baseDamage * LIGHTNING_CHAIN_MUL, null);
        }
    }
}
