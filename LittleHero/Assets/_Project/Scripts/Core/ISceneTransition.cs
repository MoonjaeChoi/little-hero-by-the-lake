using System.Collections;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 씬 전환 fade 인터페이스 (§15.7 DI). FadeOverlay 가 기본 구현.
    /// </summary>
    public interface ISceneTransition
    {
        IEnumerator FadeOut(float duration);
        IEnumerator FadeIn(float duration);
    }
}
