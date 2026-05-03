using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// 5002 — 정적 진입점 (§15.8 Service Locator). Default property 로 mock 주입 가능.
    /// 내부적으로 BossEncounterContext 를 채우고 LoadScene("BossEncounter") 호출.
    /// </summary>
    public static class BossEncounterLauncher
    {
        public static IBossEncounterLauncher Default { get; set; } = new DefaultLauncher();

        public static void Launch(BossEncounterRequest req) => Default.Launch(req);

        private class DefaultLauncher : IBossEncounterLauncher
        {
            public void Launch(BossEncounterRequest req)
            {
                BossEncounterContext.Set(req.mode, req.boss, req.endgameMode, req.mythicPool);
                // T03 에서 Mode.ChapterBoss / PendingChapter / PendingReturnScene 활성화
                // BossEncounterContext.PendingChapter = req.chapter;
                // BossEncounterContext.PendingReturnScene = req.returnSceneName;
                Time.timeScale = 1f;
                SceneManager.LoadScene("BossEncounter");
            }
        }
    }
}
