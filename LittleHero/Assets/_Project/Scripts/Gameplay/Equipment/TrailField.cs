using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v4 / Damage v2 — 충돌 폐기 (damage_system_v2.md §6).
    /// OnTriggerStay 폐기, Update 매 프레임 tick 시 OverlapCircle + ApplyMulti.
    /// </summary>
    public class TrailField : MonoBehaviour
    {
        private float _damage;
        private float _lifetime;
        private float _hitInterval;
        private float _radius;
        private float _elapsed;
        private System.Action<TrailField> _onExpire;
        private static readonly List<EnemyController> _scratch = new List<EnemyController>(64);

        public void Activate(float damage, float lifetime, float hitInterval, float radius, System.Action<TrailField> onExpire)
        {
            _damage = damage;
            _lifetime = lifetime;
            _hitInterval = hitInterval;
            _radius = radius;
            _elapsed = 0f;
            _onExpire = onExpire;
        }

        private void Update()
        {
            float prev = _elapsed;
            _elapsed += Time.deltaTime;
            // tick 경계 (interval 마다 1회)
            int prevTick = Mathf.FloorToInt(prev / _hitInterval);
            int currTick = Mathf.FloorToInt(_elapsed / _hitInterval);
            if (currTick > prevTick)
            {
                DamageQuery.FindEnemiesInCircle(transform.position, _radius, _scratch);
                DamageDealer.ApplyMulti(_scratch, _damage);
            }
            if (_elapsed >= _lifetime)
            {
                _onExpire?.Invoke(this);
            }
        }
    }
}
