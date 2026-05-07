using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5035 — 적 사망 시 반응 Strategy interface.
    /// EnemyController.Die 시점 EnemyData.deathReactionType 기반 EnemyDeathReactionFactory.Create.
    /// 3 구현체: None (기본) / Splitter (분열, 곰형) / Explode (Bomber 폭발).
    /// </summary>
    public interface IDeathReaction
    {
        /// <summary>적 사망 시 호출. deathPosition = transform.position 캐싱.</summary>
        void OnDeath(EnemyController owner, Vector3 deathPosition);
    }
}
