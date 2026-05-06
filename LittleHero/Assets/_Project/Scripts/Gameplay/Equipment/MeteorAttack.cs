using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029 — Meteor (Fire) 운석 낙하 — 광역 폭발 (Rain 변형, 단발 + 큰 폭발).
    /// ElementResolver Fire 효과 (DOT +30%) 자동 적용.
    /// </summary>
    public class MeteorAttack : IAttackPattern
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
            DropMeteor();
        }

        public void Cleanup() { }

        private void DropMeteor()
        {
            if (_owner == null) return;
            // 운석 낙하 — 가장 가까운 적 위치에 광역 폭발
            var primary = DamageQuery.FindNearest(_owner.position, _config.range * 1.5f);
            Vector2 impactPos = primary != null ? (Vector2)primary.transform.position : (Vector2)_owner.position;
            float blastRadius = Mathf.Max(0.5f, _config.range * 0.6f);
            DamageQuery.FindNearestN(impactPos, blastRadius, 32, _targets);
            DamageDealer.ApplyMulti(_targets, _config.damage * 1.5f, null, ResolveElement(MagicElement.Fire));
        }

        private MagicElement ResolveElement(MagicElement defaultElement)
            => _config.element != MagicElement.None ? _config.element : defaultElement;
    }
}
