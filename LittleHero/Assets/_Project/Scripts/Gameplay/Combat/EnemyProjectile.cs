using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5037.1 — 적이 player 에게 발사하는 발사체 (StationaryBehavior / RangedBehavior).
    /// player 측 Projectile.cs 와 분리 — 충돌 대상이 PlayerHealth, 정지 거리 검사.
    /// Rigidbody2D linearVelocity 자체 이동 + 매 프레임 Player 거리 검사 (HIT_RADIUS) + 데미지.
    /// </summary>
    public class EnemyProjectile : MonoBehaviour
    {
        private const float HIT_RADIUS = 0.5f;
        private const float DEFAULT_SPEED = 4f;
        private const float DEFAULT_LIFETIME = 4f;

        private float _damage;
        private float _lifeTimer;
        private bool _hit;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null) { _rb.gravityScale = 0f; _rb.freezeRotation = true; }
        }

        public void Launch(Vector2 direction, float damage, float speed = DEFAULT_SPEED)
        {
            _damage = damage;
            _hit = false;
            _lifeTimer = 0f;
            if (_rb != null) _rb.linearVelocity = direction.normalized * speed;
        }

        private void Update()
        {
            if (_hit) return;
            _lifeTimer += Time.deltaTime;
            if (_lifeTimer >= DEFAULT_LIFETIME) { Object.Destroy(gameObject); return; }

            var player = PlayerFinder.Get();
            if (player == null) return;
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist <= HIT_RADIUS)
            {
                var health = player.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(_damage);
                _hit = true;
                Object.Destroy(gameObject);
            }
        }
    }
}
