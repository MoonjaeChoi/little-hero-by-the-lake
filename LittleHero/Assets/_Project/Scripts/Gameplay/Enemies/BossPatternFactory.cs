namespace LittleHero
{
    /// <summary>
    /// 보스 공격 패턴 Factory (5001 boss_system_v2 / Sprint 1.3).
    /// EnemyController.CreateBossAttack switch 추출 — 신규 패턴 추가 시 본 클래스 1 곳만 수정.
    /// CLAUDE.md §15.1 OCP + §15.8 Factory 패턴 (EquipmentPatternFactory 패턴 답습).
    /// </summary>
    public static class BossPatternFactory
    {
        public static IBossAttackPattern Create(BossAttackType type)
        {
            switch (type)
            {
                case BossAttackType.Charge:    return new BossChargeAttack();
                case BossAttackType.Pulse:     return new BossPulseAttack();
                case BossAttackType.Spread:    return new BossSpreadAttack();
                case BossAttackType.Summon:    return new BossSummonAttack();
                case BossAttackType.Vine:      return new BossVineAttack();
                case BossAttackType.Tornado:   return new BossTornadoAttack();
                case BossAttackType.BlackHole: return new BossBlackHoleAttack();
                default:                       return null;
            }
        }
    }
}
