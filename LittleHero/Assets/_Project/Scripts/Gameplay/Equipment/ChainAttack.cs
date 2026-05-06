using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029 — Chain (Lightning) 사슬 — 첫 적 → 인접 N 적 자동 점프.
    /// ElementResolver.ApplyElementEffect 가 인접 1 추가 (Lightning 50%) 자동 적용.
    /// 본 pattern 은 첫 타깃만 단일 타격 — element effect 가 사슬 처리.
    /// </summary>
    public class ChainAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private float _timer;

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
            FireChain();
        }

        public void Cleanup() { }

        private void FireChain()
        {
            if (_owner == null) return;
            var nearest = DamageQuery.FindNearest(_owner.position, _config.range);
            if (nearest == null || nearest.IsDead) return;
            DamageDealer.ApplySingle(nearest, _config.damage, null, ResolveElement(MagicElement.Lightning));
            // 추가 사슬 점프 (config.chainCount 활용)
            for (int i = 0; i < Mathf.Min(_config.chainCount, 3); i++)
            {
                var next = DamageQuery.FindNearest(nearest.transform.position, _config.chainRange);
                if (next == null || next.IsDead || next == nearest) break;
                DamageDealer.ApplySingle(next, _config.damage * 0.7f, null, ResolveElement(MagicElement.Lightning));
                nearest = next;
            }
        }

        private MagicElement ResolveElement(MagicElement defaultElement)
            => _config.element != MagicElement.None ? _config.element : defaultElement;
    }
}
