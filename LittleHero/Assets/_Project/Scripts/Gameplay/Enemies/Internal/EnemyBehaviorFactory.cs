namespace LittleHero
{
    /// <summary>
    /// 5035 — EnemyBehaviorType → IEnemyBehavior 인스턴스 생성 (Factory, 5001 BossPatternFactory 답습).
    /// switch 패턴 (§15.1 OCP — 신규 behavior 추가 시 case 1 줄만).
    /// </summary>
    public static class EnemyBehaviorFactory
    {
        public static IEnemyBehavior Create(EnemyBehaviorType type) => type switch
        {
            EnemyBehaviorType.Follow     => new FollowBehavior(),
            EnemyBehaviorType.Stationary => new StationaryBehavior(),
            EnemyBehaviorType.Charger    => new ChargerBehavior(),
            EnemyBehaviorType.Splitter   => new FollowBehavior(),  // 추적 + 사망 분열은 IDeathReaction
            EnemyBehaviorType.Ranged     => new RangedBehavior(),
            EnemyBehaviorType.Bomber     => new BomberBehavior(),
            _                             => new FollowBehavior(),
        };
    }
}
