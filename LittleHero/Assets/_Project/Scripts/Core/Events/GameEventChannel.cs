using System;
using UnityEngine;
using UnityEngine.Events;

namespace LittleHero
{
    /// <summary>
    /// Ryan Hipple 패턴 — ScriptableObject 기반 Event Channel.
    /// architecture_analysis_v1 §B — 디자이너 친화 + 정적 GameEvents 와 호환.
    ///
    /// 사용법:
    ///   1. 신규 SO 인스턴스 생성 (Asset > Create > LittleHero/Events/...)
    ///   2. Inspector 에서 GameEventListener 컴포넌트 의 channel 필드 wiring
    ///   3. UnityEvent 응답 (코드 X)
    ///
    /// 정적 GameEvents.Raise* 와 병행 — 정적 EventBus 는 게임플레이 핵심,
    /// SO Channel 은 UI/디자이너 wiring 영역.
    /// </summary>
    public abstract class GameEventChannel<T> : ScriptableObject
    {
        public event Action<T> OnRaised;

        public void Raise(T payload)
        {
            OnRaised?.Invoke(payload);
        }

        // Inspector wiring 용 UnityEvent 호환 wrapper
        public void Subscribe(Action<T> handler) { OnRaised += handler; }
        public void Unsubscribe(Action<T> handler) { OnRaised -= handler; }
    }

    /// <summary>void payload (단순 알림). Inspector wiring 용.</summary>
    [CreateAssetMenu(menuName = "LittleHero/Events/Void Channel")]
    public class VoidEventChannel : ScriptableObject
    {
        public event Action OnRaised;
        public UnityEvent unityEvent;  // Inspector 시각 wiring (선택)

        public void Raise()
        {
            OnRaised?.Invoke();
            unityEvent?.Invoke();
        }
    }

    /// <summary>int payload (별가루·XP 등).</summary>
    [CreateAssetMenu(menuName = "LittleHero/Events/Int Channel")]
    public class IntEventChannel : GameEventChannel<int> { }

    /// <summary>EnemyData payload (적 처치).</summary>
    [CreateAssetMenu(menuName = "LittleHero/Events/Enemy Channel")]
    public class EnemyEventChannel : GameEventChannel<EnemyData> { }

    /// <summary>ChapterData payload (챕터 클리어).</summary>
    [CreateAssetMenu(menuName = "LittleHero/Events/Chapter Channel")]
    public class ChapterEventChannel : GameEventChannel<ChapterData> { }
}
