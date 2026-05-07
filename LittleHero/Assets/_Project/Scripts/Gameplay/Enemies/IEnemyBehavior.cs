using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 — 적 이동/공격 패턴 Strategy interface (5001 IBossPhase 답습).
    /// EnemyController 가 EnemyData.behaviorType 기반 EnemyBehaviorFactory.Create.
    /// 6 구현체: Follow / Stationary / Charger / Splitter / Ranged / Bomber.
    /// </summary>
    public interface IEnemyBehavior
    {
        /// <summary>EnemyController.Initialize 시점 호출. target = PlayerFinder.Get() 결과.</summary>
        void Initialize(EnemyController owner, EnemyData data, Transform target);

        /// <summary>매 FixedUpdate 호출 (이동 / 공격).</summary>
        void Tick(float deltaTime);

        /// <summary>활성 시 1회 (Pool 재사용 시 매번).</summary>
        void OnEnter();

        /// <summary>비활성 시 1회 (Pool 반환 시).</summary>
        void OnExit();
    }
}
