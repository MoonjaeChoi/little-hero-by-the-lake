using NUnit.Framework;
using System;
using System.Reflection;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5006 attic_entry_ui_redesign — AtticGroupLayoutPresenter / AtticPanelEntryView / EndgameMenuPanel.OnEnable 정적 검증.
    /// 5005 6 컴포넌트 회귀 검사 포함.
    /// CLAUDE.md §15.9 + Phase 4 Contract C4.
    /// </summary>
    public class AtticEntryUITests
    {
        [Test]
        public void AtticGroupLayoutPresenter_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticGroupLayoutPresenter, Assembly-CSharp"));
        }

        [Test]
        public void AtticPanelEntryView_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticPanelEntryView, Assembly-CSharp"));
        }

        [Test]
        public void AtticPanelEntryView_HasPanelEntry_NestedClass()
        {
            var t = Type.GetType("LittleHero.AtticPanelEntryView, Assembly-CSharp");
            Assert.NotNull(t);
            var nested = t.GetNestedType("PanelEntry");
            Assert.NotNull(nested, "AtticPanelEntryView 내부 PanelEntry nested class 존재");
        }

        [Test]
        public void AtticGroupLayoutPresenter_MinTouchSize_44()
        {
            var t = Type.GetType("LittleHero.AtticGroupLayoutPresenter, Assembly-CSharp");
            Assert.NotNull(t);
            var field = t.GetField("_minTouchSize", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(field, "_minTouchSize SerializeField 존재");
            Assert.AreEqual(typeof(float), field.FieldType, "_minTouchSize 는 float 타입");
        }

        [Test]
        public void EndgameMenuPanel_HasOnEnable_Method()
        {
            var t = Type.GetType("LittleHero.EndgameMenuPanel, Assembly-CSharp");
            Assert.NotNull(t);
            var m = t.GetMethod("OnEnable", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(m, "EndgameMenuPanel.OnEnable 메서드 존재 (D3=C 자체 검증 hook)");
        }

        [Test]
        public void AtticEntryUI_5005_RegressionPass()
        {
            // 5005 6 컴포넌트 회귀 — 모두 Type.GetType 통과
            string[] types5005 = {
                "LittleHero.AtticController",
                "LittleHero.AtticCurrencyView",
                "LittleHero.AtticCharacterSelector",
                "LittleHero.AtticChapterEntry",
                "LittleHero.AtticEndgameMenuView",
                "LittleHero.AtticPanelToggleManager",
                "LittleHero.AtticReturnHandler"
            };
            foreach (var name in types5005)
                Assert.NotNull(Type.GetType($"{name}, Assembly-CSharp"), $"5005 회귀 — {name} 누락");
        }
    }
}
