using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5002 boss_scene_separation_v1 — Launcher / Context / BackgroundApplier / CompleteChapterBoss 단위 테스트.
    /// CLAUDE.md §15.9 + Sprint 7 / T09.
    /// </summary>
    public class BossSceneSeparationTests
    {
        // === Launcher 인프라 ===

        [Test]
        public void Launcher_Default_NotNull()
        {
            Assert.NotNull(BossEncounterLauncher.Default);
        }

        [Test]
        public void Mode_Enum_Has_ChapterBoss()
        {
            var values = System.Enum.GetValues(typeof(BossEncounterContext.Mode));
            CollectionAssert.Contains(values, BossEncounterContext.Mode.ChapterBoss);
        }

        // === Launcher Mock 주입 ===

        [Test]
        public void Launcher_Launch_FillsContext_ChapterBoss()
        {
            BossEncounterContext.Clear();
            var prevDefault = BossEncounterLauncher.Default;
            var mockLauncher = new MockLauncher();
            BossEncounterLauncher.Default = mockLauncher;

            var boss = ScriptableObject.CreateInstance<EnemyData>();
            boss.id = "test_boss";
            var chapter = ScriptableObject.CreateInstance<ChapterData>();
            chapter.id = "ch1";

            BossEncounterLauncher.Launch(new BossEncounterRequest
            {
                mode = BossEncounterContext.Mode.ChapterBoss,
                boss = boss,
                chapter = chapter,
                returnSceneName = "Attic",
            });

            Assert.AreEqual(1, mockLauncher.LaunchCount);
            Assert.AreEqual(BossEncounterContext.Mode.ChapterBoss, mockLauncher.LastRequest.mode);
            Assert.AreEqual("test_boss", mockLauncher.LastRequest.boss.id);
            Assert.AreEqual("ch1", mockLauncher.LastRequest.chapter.id);
            Assert.AreEqual("Attic", mockLauncher.LastRequest.returnSceneName);

            BossEncounterLauncher.Default = prevDefault;
            Object.DestroyImmediate(boss);
            Object.DestroyImmediate(chapter);
        }

        [Test]
        public void Launcher_Launch_FillsContext_Daily()
        {
            BossEncounterContext.Clear();
            var prevDefault = BossEncounterLauncher.Default;
            var mockLauncher = new MockLauncher();
            BossEncounterLauncher.Default = mockLauncher;

            var boss = ScriptableObject.CreateInstance<EnemyData>();
            var endgameMode = ScriptableObject.CreateInstance<EndgameModeData>();

            BossEncounterLauncher.Launch(new BossEncounterRequest
            {
                mode = BossEncounterContext.Mode.Daily,
                boss = boss,
                endgameMode = endgameMode,
                returnSceneName = "Attic",
            });

            Assert.AreEqual(BossEncounterContext.Mode.Daily, mockLauncher.LastRequest.mode);
            Assert.AreSame(endgameMode, mockLauncher.LastRequest.endgameMode);

            BossEncounterLauncher.Default = prevDefault;
            Object.DestroyImmediate(boss);
            Object.DestroyImmediate(endgameMode);
        }

        // === CompleteChapterBoss 안전성 ===

        [Test]
        public void CompleteChapterBoss_Null_NoOp()
        {
            Assert.DoesNotThrow(() => BossEncounterService.CompleteChapterBoss(null, null));
        }

        // === Context Clear ===

        [Test]
        public void Context_Clear_ResetsAllFields()
        {
            BossEncounterContext.PendingBoss = ScriptableObject.CreateInstance<EnemyData>();
            BossEncounterContext.PendingMode = ScriptableObject.CreateInstance<EndgameModeData>();
            BossEncounterContext.PendingChapter = ScriptableObject.CreateInstance<ChapterData>();
            BossEncounterContext.PendingReturnScene = "Attic";

            var boss = BossEncounterContext.PendingBoss;
            var mode = BossEncounterContext.PendingMode;
            var chapter = BossEncounterContext.PendingChapter;

            BossEncounterContext.Clear();

            Assert.IsNull(BossEncounterContext.PendingBoss);
            Assert.IsNull(BossEncounterContext.PendingMode);
            Assert.IsNull(BossEncounterContext.PendingChapter);
            Assert.IsNull(BossEncounterContext.PendingReturnScene);

            Object.DestroyImmediate(boss);
            Object.DestroyImmediate(mode);
            Object.DestroyImmediate(chapter);
        }

        // === BackgroundApplier ===

        [Test]
        public void BackgroundApplier_Null_NoOp()
        {
            Assert.DoesNotThrow(() => BackgroundApplier.Apply(null, null));
            var ch = ScriptableObject.CreateInstance<ChapterData>();
            Assert.DoesNotThrow(() => BackgroundApplier.Apply(null, ch));
            Object.DestroyImmediate(ch);
        }

        // === Reflection 검증 ===

        [Test]
        public void BossEncounterRequest_Has_AllFields()
        {
            var t = typeof(BossEncounterRequest);
            Assert.NotNull(t.GetField("mode"));
            Assert.NotNull(t.GetField("boss"));
            Assert.NotNull(t.GetField("endgameMode"));
            Assert.NotNull(t.GetField("chapter"));
            Assert.NotNull(t.GetField("mythicPool"));
            Assert.NotNull(t.GetField("returnSceneName"));
        }

        // === Mock Helper ===

        private class MockLauncher : IBossEncounterLauncher
        {
            public int LaunchCount;
            public BossEncounterRequest LastRequest;
            public void Launch(BossEncounterRequest req)
            {
                LaunchCount++;
                LastRequest = req;
            }
        }
    }
}
