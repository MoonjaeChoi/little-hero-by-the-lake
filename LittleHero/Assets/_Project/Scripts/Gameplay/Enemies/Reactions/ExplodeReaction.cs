using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 T04 — Bomber 사망 시 폭발. bomberRadius (D4=B 1.5u) 내 플레이어 + 다른 적 데미지.
    /// PG-12 톤 — 만화풍 별 흩어짐 (VfxPool.SpawnStarBurst).
    /// </summary>
    public class ExplodeReaction : IDeathReaction
    {
        private static readonly List<EnemyController> _scratch = new List<EnemyController>(32);

        public void OnDeath(EnemyController owner, Vector3 deathPosition)
        {
            if (owner == null || owner.Data == null) return;
            var data = owner.Data;
            float radius = data.bomberRadius;
            float damage = data.bomberDamage;

            // 1. 플레이어 데미지 — PlayerHealth 가 반경 내라면
            var player = PlayerFinder.Get();
            if (player != null)
            {
                float distToPlayer = Vector2.Distance(player.position, deathPosition);
                if (distToPlayer <= radius)
                {
                    var hp = player.GetComponent<PlayerHealth>();
                    if (hp != null) hp.TakeDamage(damage);
                }
            }

            // 2. 다른 적 데미지 (chain explosion 가능 — 5035.1 backlog)
            DamageQuery.FindEnemiesInCircle(deathPosition, radius, _scratch);
            for (int i = 0; i < _scratch.Count; i++)
            {
                if (_scratch[i] == owner) continue;
                _scratch[i].TakeDamage(damage * 0.5f);  // 적 끼리는 50%
            }

            // 3. 시각 효과 — 별 흩어짐 (PG-12 만화풍)
            VfxPool.SpawnStarBurst(deathPosition);
        }
    }
}
