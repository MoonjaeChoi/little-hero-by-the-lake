using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT 2026-05-02 — 검기 발사 패턴 (탕탕특공대 카타나 / 빛을 쫓는자 차용).
    /// 전방 부채꼴 영역 (range × spreadAngle) 의 적 max maxTargets 마리에 동시 데미지.
    /// Damage v2 — DamageQuery + 부채꼴 필터 + 거리 정렬 + ApplyMulti (메이플 풍 다중 보정).
    /// </summary>
    public class SlashAttack : IAttackPattern
    {
        private AttackPatternConfig _config;
        private Transform _owner;
        private PlayerStats _stats;
        private float _timer;
        private static readonly List<EnemyController> _scratch = new List<EnemyController>(32);
        private static readonly List<EnemyController> _filtered = new List<EnemyController>(32);

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

            DamageQuery.FindEnemiesInCircle(_owner.position, r, _scratch);
            _filtered.Clear();
            for (int i = 0; i < _scratch.Count; i++)
            {
                var e = _scratch[i];
                if (_config.spreadAngle > 0f)
                {
                    Vector2 toEnemy = (e.transform.position - _owner.position);
                    Vector2 forward = Vector2.right;
                    float angle = Vector2.Angle(forward, toEnemy);
                    if (angle > _config.spreadAngle * 0.5f) continue;
                }
                _filtered.Add(e);
            }
            // 거리 정렬 + maxTargets 자르기
            Vector3 ownerPos = _owner.position;
            _filtered.Sort((a, b) =>
            {
                float sa = ((Vector2)a.transform.position - (Vector2)ownerPos).sqrMagnitude;
                float sb = ((Vector2)b.transform.position - (Vector2)ownerPos).sqrMagnitude;
                return sa.CompareTo(sb);
            });
            if (_filtered.Count > maxTargets) _filtered.RemoveRange(maxTargets, _filtered.Count - maxTargets);
            DamageDealer.ApplyMulti(_filtered, _config.damage, null, _config.element);
        }
    }
}
