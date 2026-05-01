using UnityEngine;
using UnityEngine.InputSystem;

namespace LittleHero
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerStats))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private PlayerStats _stats;
        private PlayerHealth _health;
        private InputAction _moveAction;
        private Vector2 _joystickInput;
        private bool _joystickActive;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.freezeRotation = true;
            _stats = GetComponent<PlayerStats>();
            _health = GetComponent<PlayerHealth>();

            _moveAction = new InputAction("Move", InputActionType.Value);
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up",    "<Keyboard>/w")
                .With("Down",  "<Keyboard>/s")
                .With("Left",  "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")
                .With("Up",    "<Keyboard>/upArrow")
                .With("Down",  "<Keyboard>/downArrow")
                .With("Left",  "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            _moveAction.Enable();
        }

        private void Start()
        {
            CharacterData character = MetaProgression.Instance != null ? MetaProgression.Instance.SelectedCharacter : null;
            _stats.Initialize(GameManager.Instance.Balance, character);
            // A2 — PlayerProgress 의 SaveData Level 을 stats 에 절대 동기화
            PlayerProgress.Instance?.SyncStatsToCurrentLevel(_stats);
            _health.Initialize(_stats);

            if (character != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = character.tintColor;
                    // Phase G2 — character.portrait 가 있으면 sprite 동적 변경 (placeholder Circle → 실 캐릭터 idle)
                    if (character.portrait != null)
                    {
                        sr.sprite = character.portrait;
                        // 1024×768 PNG 가 너무 크지 않게 캐릭터별 통일 scale (게임 내 균형)
                        transform.localScale = Vector3.one * 0.6f;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            _moveAction.Disable();
            _moveAction.Dispose();
        }

        private SpriteRenderer _sr;
        private float _bobBaseY;

        private void FixedUpdate()
        {
            Vector2 input = _joystickActive ? _joystickInput : _moveAction.ReadValue<Vector2>();
            Move(input);
        }

        private void Update()
        {
            // Phase G2 P10: 단순 애니메이션 — 좌우 반전 + 작은 bob
            if (_sr == null) _sr = GetComponent<SpriteRenderer>();
            if (_sr == null) return;
            float vx = _rb.linearVelocity.x;
            if (Mathf.Abs(vx) > 0.05f)
                _sr.flipX = vx < 0f;  // 좌측 이동 시 sprite 좌우 반전
            // bob: 이동 중에만 살짝 위아래
            bool moving = _rb.linearVelocity.sqrMagnitude > 0.01f;
            if (_bobBaseY == 0f) _bobBaseY = transform.localPosition.y;
            float bob = moving ? Mathf.Sin(Time.time * 8f) * 0.05f : 0f;
            var p = transform.localPosition;
            p.y = _bobBaseY + bob;
            transform.localPosition = p;
        }

        // 가상 조이스틱 UI에서 호출
        public void SetJoystickInput(Vector2 input)
        {
            _joystickInput = input;
            _joystickActive = input != Vector2.zero;
        }

        private void Move(Vector2 input)
        {
            if (_health.IsDead)
            {
                _rb.linearVelocity = Vector2.zero;
                return;
            }
            _rb.linearVelocity = input * _stats.MoveSpeed;
        }
    }
}
