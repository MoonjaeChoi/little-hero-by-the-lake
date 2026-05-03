using NUnit.Framework;
using System;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5005 attic_decomposition — AtticController 책임 분리 6 신규 컴포넌트 정적 검증.
    /// CLAUDE.md §15.1 SRP + Phase 4 Contract C4.
    /// </summary>
    public class AtticDecompositionTests
    {
        [Test]
        public void AtticCurrencyView_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticCurrencyView, Assembly-CSharp"));
        }

        [Test]
        public void AtticCharacterSelector_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticCharacterSelector, Assembly-CSharp"));
        }

        [Test]
        public void AtticChapterEntry_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticChapterEntry, Assembly-CSharp"));
        }

        [Test]
        public void AtticEndgameMenuView_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticEndgameMenuView, Assembly-CSharp"));
        }

        [Test]
        public void AtticPanelToggleManager_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticPanelToggleManager, Assembly-CSharp"));
        }

        [Test]
        public void AtticReturnHandler_Type_Exists()
        {
            Assert.NotNull(Type.GetType("LittleHero.AtticReturnHandler, Assembly-CSharp"));
        }
    }
}
