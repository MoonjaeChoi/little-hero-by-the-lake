using UnityEngine;

namespace LittleHero
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyController))]
    public class EnemyAI : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private EnemyController _controller;
        private Transform _target;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _controller = GetComponent<EnemyController>();
        }

        private void OnEnable()
        {
            FindPlayer();
        }

        private void FixedUpdate()
        {
            if (_controller.IsDead) return;
            if (_target == null) FindPlayer();
            Chase();
        }

        private void Chase()
        {
            if (_target == null) return;

            Vector2 direction = ((Vector2)_target.position - (Vector2)transform.position).normalized;
            _rb.linearVelocity = direction * _controller.Data.moveSpeed;
        }

        private void FindPlayer()
        {
            // CLAUDE.md §15.3 GC 최소화 — PlayerFinder 정적 cache
            _target = PlayerFinder.Get();
        }
    }
}
