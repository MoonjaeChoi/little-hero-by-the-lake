using UnityEngine;

namespace LittleHero
{
    public class OrbitOrb : MonoBehaviour
    {
        private float _damage;

        public void Configure(float damage)
        {
            _damage = damage;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            EnemyController e = other.GetComponent<EnemyController>();
            if (e == null || e.IsDead) return;
            DamageSystem.DealDamage(e, _damage);
        }
    }
}
