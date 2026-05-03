namespace LittleHero
{
    /// <summary>
    /// 보스 분류 (5001 boss_system_v2). 기존 isMiniB/isBoss boolean 을 enum 으로 명료화.
    /// CLAUDE.md §15.4 SO 우선 정합. None=일반 적, MiniBoss=5/10/15 웨이브, ChapterBoss=20 웨이브,
    /// DailyBoss/WeeklyBoss=Endgame 회전.
    /// </summary>
    public enum BossTier
    {
        None = 0,
        MiniBoss = 1,
        ChapterBoss = 2,
        DailyBoss = 3,
        WeeklyBoss = 4,
    }
}
