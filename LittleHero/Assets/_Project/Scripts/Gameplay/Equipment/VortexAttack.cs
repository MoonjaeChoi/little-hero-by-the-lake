using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029 — Vortex (Ice) 소용돌이 — 광역 흡인 + 빙결 placeholder.
    /// ElementResolver Ice 효과 (둔화 visual) 자동 적용.
    /// 빙결 visual 은 5029.1 후속 (placeholder).
    /// </summary>
    public class VortexAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private float _timer;
        private readonly List<EnemyController> _targets = new List<EnemyController>(32);

        public void Initialize(AttackPatternConfig config, Transform owner, PlayerStats stats)
        {
            _config = config;
            _owner = owner;
            _stats = stats;
        }

        public void Tick(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer < AttackPatternUtil.EffectiveInterval(_config.attackInterval, _stats)) return;
            _timer = 0f;
            CreateVortex();
        }

        public void Cleanup() { }

        private void CreateVortex()
        {
            if (_owner == null) return;
            float r = Mathf.Max(0.1f, _config.range);
            DamageQuery.FindNearestN(_owner.position, r, 24, _targets);
            DamageDealer.ApplyMulti(_targets, _config.damage, null, ResolveElement(MagicElement.Ice));
        }

        private MagicElement ResolveElement(MagicElement defaultElement)
            => _config.element != MagicElement.None ? _config.element : defaultElement;
    }
}
