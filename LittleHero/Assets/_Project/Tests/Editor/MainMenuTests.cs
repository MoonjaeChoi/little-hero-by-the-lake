using System.Reflection;
using NUnit.Framework;
using LittleHero.MainMenu;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5040 T08 — MainMenu UI 컴포넌트 reflection 검증 (NUnit 6).
    /// scene wiring 은 사용자 측 검증 (C6 Play 시나리오).
    /// </summary>
    public class MainMenuTests
    {
        [Test]
        public void TabBase_Has_OnEnter_OnExit_Methods()
        {
            var t = typeof(TabBase);
            Assert.IsNotNull(t.GetMethod("OnEnter"), "TabBase.OnEnter 부재");
            Assert.IsNotNull(t.GetMethod("OnExit"), "TabBase.OnExit 부재");
        }

        [Test]
        public void TabId_Has_5_Values_Including_Adventure()
        {
            var values = System.Enum.GetValues(typeof(TabId));
            Assert.AreEqual(5, values.Length, "TabId enum 5 값 (Hero/Equipment/Adventure/Shop/Record)");
            Assert.IsTrue(System.Enum.IsDefined(typeof(TabId), "Adventure"));
        }

        [Test]
        public void BottomNavBar_Has_OnTabSelected_Event()
        {
            var t = typeof(BottomNavBar);
            Assert.IsNotNull(t.GetEvent("OnTabSelected"), "BottomNavBar.OnTabSelected event 부재");
            Assert.IsNotNull(t.GetProperty("SelectedTab"), "BottomNavBar.SelectedTab Property 부재");
        }

        [Test]
        public void ChapterBookCarousel_Has_Refresh_OnChapterSelected()
        {
            var t = typeof(ChapterBookCarousel);
            Assert.IsNotNull(t.GetMethod("Refresh"), "ChapterBookCarousel.Refresh 부재");
            Assert.IsNotNull(t.GetEvent("OnChapterSelected"), "OnChapterSelected event 부재");
        }

        [Test]
        public void ChapterCardView_Has_Bind_Method()
        {
            var t = typeof(ChapterCardView);
            var bind = t.GetMethod("Bind");
            Assert.IsNotNull(bind, "ChapterCardView.Bind 부재");
            Assert.AreEqual(5, bind.GetParameters().Length, "Bind(chapter, unlocked, firstCleared, playerLevel, recommended)");
        }

        [Test]
        public void MainMenuController_Has_SwitchTab_Method()
        {
            var t = typeof(MainMenuController);
            Assert.IsNotNull(t.GetMethod("SwitchTab"), "MainMenuController.SwitchTab 부재");
        }
    }
}
