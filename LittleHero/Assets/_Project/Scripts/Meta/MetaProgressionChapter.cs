namespace LittleHero
{
    /// <summary>
    /// 5023 — 챕터 선택 책임 분리. SelectedChapter prop.
    /// Note: WondrousMapPanel / ChapterRunner / MemorySpawner / EnemyController 가 호출.
    /// </summary>
    public static class MetaProgressionChapter
    {
        public static ChapterData SelectedChapter { get; set; }
    }
}
