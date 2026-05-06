using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT v4 / Damage v2 — 충돌 폐기 (damage_system_v2.md §6).
    /// OnTriggerEnter2D 폐기, _interval 마다 OverlapCircle 0.4f + ApplySingle.
    /// orb 이 회전하면서 _interval 마다 가장 가까운 적 1마리 데미지 (orbit 회전 속도 정합).
    /// </summary>
    public class OrbitOrb : MonoBehaviour
    {
        private const float HIT_RADIUS = 0.4f;
        private const float HIT_INTERVAL = 0.3f;

        private float _damage;
        private float _timer;
        private MagicElement _element;

        public void Configure(float damage, MagicElement element = MagicElement.None)
        {
            _damage = damage;
            _element = element;
            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < HIT_INTERVAL) return;
            _timer = 0f;
            var enemy = DamageQuery.FindNearest(transform.position, HIT_RADIUS);
            if (enemy != null) DamageDealer.ApplySingle(enemy, _damage, null, _element);
        }
    }
}
