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
            // 5035 T06 — EnemyController 의 IEnemyBehavior (FollowBehavior 답습) 이 Tick 처리.
            // EnemyAI 폐기 (D5=A) — prefab 호환 위해 컴포넌트 보존만, 자가 비활성.
            _rb = GetComponent<Rigidbody2D>();
            if (_rb != null) { _rb.gravityScale = 0f; _rb.freezeRotation = true; }
            enabled = false;
        }

        private void FixedUpdate() { /* 5035 — 폐기 (EnemyController.FixedUpdate 가 처리) */ }

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
