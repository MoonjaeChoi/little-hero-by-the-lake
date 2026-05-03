namespace LittleHero
{
    /// <summary>
    /// 5002 — 보스 씬 진입 인터페이스 (§15.1 DIP, §15.7 DI).
    /// 기본 구현 = BossEncounterLauncher.Default. 테스트 시 mock 주입.
    /// </summary>
    public interface IBossEncounterLauncher
    {
        void Launch(BossEncounterRequest request);
    }
}
