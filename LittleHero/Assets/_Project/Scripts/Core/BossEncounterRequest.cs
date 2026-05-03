namespace LittleHero
{
    /// <summary>
    /// 5002 — BossEncounterLauncher 진입 DTO (§15.7 DI).
    /// 모드별 필드 nullable — Daily/Weekly: endgameMode/mythicPool, ChapterBoss: chapter.
    /// </summary>
    public struct BossEncounterRequest
    {
        public BossEncounterContext.Mode mode;
        public EnemyData boss;
        public EndgameModeData endgameMode;       // Daily/Weekly 만 (null 가능)
        public ChapterData chapter;                // ChapterBoss 만 (null 가능)
        public MythicDefinition[] mythicPool;      // Weekly 만 (null 가능)
        public string returnSceneName;             // 기본 "Attic"
    }
}
