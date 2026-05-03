using UnityEngine;
using UnityEngine.Events;

namespace LittleHero
{
    /// <summary>
    /// Ryan Hipple 패턴 — VoidEventChannel 구독 + UnityEvent 발행.
    /// Inspector wiring 으로 코드 없이 이벤트 → UI 응답 가능.
    /// </summary>
    public class GameEventListener : MonoBehaviour
    {
        [SerializeField] private VoidEventChannel _channel;
        [SerializeField] private UnityEvent _response;

        private void OnEnable()
        {
            if (_channel != null) _channel.OnRaised += HandleRaised;
        }

        private void OnDisable()
        {
            if (_channel != null) _channel.OnRaised -= HandleRaised;
        }

        private void HandleRaised() => _response?.Invoke();
    }
}
