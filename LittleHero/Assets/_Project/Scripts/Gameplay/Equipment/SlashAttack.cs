using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT 2026-05-02 — 검기 발사 패턴 (탕탕특공대 카타나 / 빛을 쫓는자 차용).
    /// 전방 부채꼴 영역 (range × spreadAngle) 의 적 max maxTargets 마리에 동시 데미지.
    /// 효과 변형: lifestealRatio (흡혈) / dotDamage / pierceCount (관통은 부채꼴 모양상 자동 처리).
    /// </summary>
    public class SlashAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private float _timer;
        private static readonly Collider2D[] _overlapBuffer = new Collider2D[32];

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
            Slash();
        }

        public void Cleanup() { /* SlashAttack 는 별도 풀 미사용 */ }

        private void Slash()
        {
            if (_owner == null) return;
            float r = Mathf.Max(0.1f, _config.range);
            int maxTargets = _config.maxTargets > 0 ? _config.maxTargets : 1;
            int hitCount = 0;

            int count = Physics2D.OverlapCircleNonAlloc(_owner.position, r, _overlapBuffer);
            for (int i = 0; i < count && hitCount < maxTargets; i++)
            {
                EnemyController e = _overlapBuffer[i].GetComponent<EnemyController>();
                if (e == null || e.IsDead) continue;

                // 부채꼴 (spreadAngle=0 → 전 방향 / spreadAngle>0 → 전방 부채꼴)
                if (_config.spreadAngle > 0f)
                {
                    Vector2 toEnemy = (e.transform.position - _owner.position);
                    Vector2 forward = Vector2.right;  // 단순화 — 향후 _stats 또는 Rigidbody 의 facing 으로
                    float angle = Vector2.Angle(forward, toEnemy);
                    if (angle > _config.spreadAngle * 0.5f) continue;
                }

                DamageSystem.DealDamage(e, _config.damage);
                hitCount++;

                // 흡혈 — PlayerHealth.Heal 미구현이라 보류 (Phase C5 활성화)
                // if (_config.lifestealRatio > 0f) {
                //     var health = _owner.GetComponent<PlayerHealth>();
                //     if (health != null) health.Heal(_config.damage * _config.lifestealRatio);
                // }
            }
        }
    }
}
