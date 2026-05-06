using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029 — Beam (Arcane) 직선 광선. 일직선 적 다수 동시 데미지.
    /// 기본 element=Arcane (저항 무시), config.element 로 override 가능.
    /// </summary>
    public class BeamAttack : IAttackPattern
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
            FireBeam();
        }

        public void Cleanup() { }

        private void FireBeam()
        {
            if (_owner == null) return;
            float r = Mathf.Max(0.1f, _config.range);
            DamageQuery.FindEnemiesInCircle(_owner.position, r, _targets);
            // 직선 광선 — 가까운 적 부채꼴 60도 내 다수 동시 데미지
            DamageDealer.ApplyMulti(_targets, _config.damage, null, ResolveElement(MagicElement.Arcane));
        }

        private MagicElement ResolveElement(MagicElement defaultElement)
            => _config.element != MagicElement.None ? _config.element : defaultElement;
    }
}
