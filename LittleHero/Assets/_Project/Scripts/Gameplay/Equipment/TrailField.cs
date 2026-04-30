using UnityEngine;

namespace LittleHero
{
    public class TrailField : MonoBehaviour
    {
        private float _damage;
        private float _lifetime;
        private float _hitInterval;
        private float _elapsed;
        private System.Action<TrailField> _onExpire;

        public void Activate(float damage, float lifetime, float hitInterval, System.Action<TrailField> onExpire)
        {
            _damage = damage;
            _lifetime = lifetime;
            _hitInterval = hitInterval;
            _elapsed = 0f;
            _onExpire = onExpire;
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifetime)
            {
                _onExpire?.Invoke(this);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (Mathf.FloorToInt(_elapsed / _hitInterval) ==
                Mathf.FloorToInt((_elapsed - Time.deltaTime) / _hitInterval)) return;

            EnemyController e = other.GetComponent<EnemyController>();
            if (e == null || e.IsDead) return;
            DamageSystem.DealDamage(e, _damage);
        }
    }
}
