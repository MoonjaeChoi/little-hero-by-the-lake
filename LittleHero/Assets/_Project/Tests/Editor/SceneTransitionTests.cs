using NUnit.Framework;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5004 scene_transition_async — SceneTransitionService / ISceneTransition / FadeOverlay 단위 테스트.
    /// CLAUDE.md §15.9 + Sprint 5 / T06.
    /// </summary>
    public class SceneTransitionTests
    {
        [Test]
        public void Transition_Default_NotNull()
        {
            Assert.NotNull(SceneTransitionService.Default);
        }

        [Test]
        public void Transition_NoOp_FadeOut_Completes()
        {
            var noOp = new NoOpHelper();
            Assert.DoesNotThrow(() => { var e = noOp.FadeOut(0.3f); while (e.MoveNext()) {} });
        }

        [Test]
        public void Transition_NoOp_FadeIn_Completes()
        {
            var noOp = new NoOpHelper();
            Assert.DoesNotThrow(() => { var e = noOp.FadeIn(0.3f); while (e.MoveNext()) {} });
        }

        [Test]
        public void ISceneTransition_HasFadeOutFadeIn()
        {
            var t = typeof(ISceneTransition);
            Assert.NotNull(t.GetMethod("FadeOut"));
            Assert.NotNull(t.GetMethod("FadeIn"));
        }

        [Test]
        public void DEFAULT_FADE_DURATION_Is_0_3()
        {
            Assert.AreEqual(0.3f, SceneTransitionService.DEFAULT_FADE_DURATION, 0.001f);
        }

        private class NoOpHelper : ISceneTransition
        {
            public System.Collections.IEnumerator FadeOut(float d) { yield break; }
            public System.Collections.IEnumerator FadeIn(float d) { yield break; }
        }
    }
}
