namespace LittleHero
{
    /// <summary>
    /// F4·F5 — Endgame 보스 씬 진입 시 정적 컨텍스트.
    /// EndgameMenuPanel 가 LoadScene("BossEncounter") 직전에 Set 호출,
    /// BossEncounterRunner 가 Start 에서 읽어 보스 spawn + 처치 hook.
    /// 처치 후 결과는 LastMythicResult 로 전달 → 다락방 복귀 후 AtticController 가 Overlay 호출.
    /// </summary>
    public static class BossEncounterContext
    {
        public enum Mode { Daily, Weekly, ChapterBoss, MiniBoss }   // 5002 ChapterBoss / 5003 MiniBoss

        public static EnemyData PendingBoss;
        public static EndgameModeData PendingMode;
        public static Mode PendingType;
        public static MythicDefinition[] PendingMythicPool;

        // 5002 T03 — ChapterBoss 한정
        public static ChapterData PendingChapter;
        public static string PendingReturnScene;

        // 처치 결과 (Weekly 한정 — 다락방 복귀 후 신화 오버레이 1회 표시)
        public static EquipmentInstance LastMythicGranted;
        // Pool 에서 lookup 한 MythicDefinition (AtticController 에서 Overlay.Play 직접 호출용)
        public static MythicDefinition LastMythicDefinition;

        public static void Set(Mode type, EnemyData boss, EndgameModeData mode, MythicDefinition[] mythicPool)
        {
            PendingType = type;
            PendingBoss = boss;
            PendingMode = mode;
            PendingMythicPool = mythicPool;
        }

        public static void Clear()
        {
            PendingBoss = null;
            PendingMode = null;
            PendingMythicPool = null;
            PendingChapter = null;        // 5002 T03
            PendingReturnScene = null;    // 5002 T03
        }

        public static bool HasPending => PendingBoss != null
            && (PendingMode != null || PendingType == Mode.ChapterBoss);
    }
}
