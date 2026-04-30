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
            _health.Initialize(_stats);

            if (character != null)
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = character.tintColor;
            }
        }

        private void OnDestroy()
        {
            _moveAction.Disable();
            _moveAction.Dispose();
        }

        private void FixedUpdate()
        {
            Vector2 input = _joystickActive ? _joystickInput : _moveAction.ReadValue<Vector2>();
            Move(input);
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
