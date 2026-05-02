namespace LittleHero
{
    /// <summary>
    /// 보스 공격 패턴 인터페이스 (boss_attack_v1.md §2.1).
    /// 각 패턴은 EnemyController.Initialize 시점에 인스턴스화.
    /// </summary>
    public interface IBossAttackPattern
    {
        void Initialize(EnemyController owner, EnemyData data);
        void Tick(float deltaTime);
        void Cleanup();
    }

    public enum BossAttackType
    {
        None,
        Charge,      // Animal — 1.5s telegraph → 직선 돌진
        Pulse,       // Civ — 광역 폭발
        Spread,      // Fantasy — 부채꼴 탄막 (사이클 2)
        Summon,      // Meta — 작은 적 소환 (사이클 2)
        Vine,        // Nature — 식물 함정 (사이클 2)
        Tornado,     // Daily — 회전 영역 (사이클 3)
        BlackHole,   // Weekly — 흡인 + 폭발 (사이클 3)
    }
}
