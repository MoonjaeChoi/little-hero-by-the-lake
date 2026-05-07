namespace LittleHero
{
    /// <summary>
    /// 5035 — EnemyDeathReactionType → IDeathReaction 인스턴스 생성 (Factory).
    /// </summary>
    public static class EnemyDeathReactionFactory
    {
        private static readonly NoneReaction _none = new NoneReaction();

        public static IDeathReaction Create(EnemyDeathReactionType type) => type switch
        {
            EnemyDeathReactionType.Splitter => new SplitterReaction(),
            EnemyDeathReactionType.Explode  => new ExplodeReaction(),
            _                                => _none,
        };
    }

    /// <summary>5035 — 기본 (no-op).</summary>
    public class NoneReaction : IDeathReaction
    {
        public void OnDeath(EnemyController owner, UnityEngine.Vector3 deathPosition) { }
    }
}
