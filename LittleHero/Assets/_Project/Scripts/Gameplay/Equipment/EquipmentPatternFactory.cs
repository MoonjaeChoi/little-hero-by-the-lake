using System;

namespace LittleHero
{
    /// <summary>
    /// CLAUDE.md §15.1 SOLID-O — EquipmentManager 의 CreatePattern/GetPatternType switch 추출.
    /// 신규 패턴 추가 시 본 클래스만 수정 (EquipmentManager 변경 X — Open-Closed 준수).
    /// </summary>
    public static class EquipmentPatternFactory
    {
        public static IAttackPattern Create(AttackPatternType type)
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
                _ => null,
            };
        }

        public static Type GetType(AttackPatternType type)
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
                _ => null,
            };
        }
    }
}
