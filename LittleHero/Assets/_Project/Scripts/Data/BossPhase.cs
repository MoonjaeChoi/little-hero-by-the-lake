namespace LittleHero
{
    /// <summary>
    /// 보스 페이즈 (5001 boss_system_v2). State 패턴 진입점.
    /// Phase1=HP 100~50%, Phase2=HP 50~0% (ChapterBoss+ 만, MiniBoss 는 Phase 1 만 — D3=A).
    /// </summary>
    public enum BossPhase
    {
        Phase1 = 0,
        Phase2 = 1,
    }
}
