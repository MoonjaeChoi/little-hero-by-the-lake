using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5002 — 챕터 BG 적용 정적 헬퍼 (D1 Rebuttal — §15.1 SRP + DRY).
    /// ChapterRunner / BossEncounterRunner 공유. Tiled BG 패턴 단일 진실원.
    /// </summary>
    public static class BackgroundApplier
    {
        public static void Apply(SpriteRenderer renderer, ChapterData chapter)
        {
            if (renderer == null || chapter == null || chapter.coverArt == null) return;
            renderer.sprite = chapter.coverArt;
            renderer.sortingOrder = -100;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(300f, 300f);
            renderer.transform.position = Vector3.zero;
            renderer.color = Color.white;
        }
    }
}
