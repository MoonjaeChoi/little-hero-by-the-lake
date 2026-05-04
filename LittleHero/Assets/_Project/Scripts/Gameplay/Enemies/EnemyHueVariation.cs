using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5012 — Enemy sprite 5단계 hue 변형 정적 헬퍼.
    /// 5008 LayerDiffuse RGBA sprite 30 archetype 시각 다양화 (30 → 150).
    /// SpriteRenderer.color multiplicative tint (D1=B). HSV 변환 GC 0.
    /// Boss 예외: variationIndex=0 강제 (EnemyController 가 처리).
    /// </summary>
    public static class EnemyHueVariation
    {
        public static readonly float[] OFFSETS = { -0.20f, -0.10f, 0.0f, +0.10f, +0.20f };
        public const int VARIATION_COUNT = 5;
        private const int SPREAD_PRIME = 7;

        public static Color HueShift(Color rgb, float hueOffset)
        {
            Color.RGBToHSV(rgb, out float h, out float s, out float v);
            h = (h + hueOffset + 1f) % 1f;
            var shifted = Color.HSVToRGB(h, s, v);
            shifted.a = rgb.a;
            return shifted;
        }

        public static int VariationIndex(int waveNumber, int spawnOrder)
        {
            int idx = ((waveNumber - 1) * SPREAD_PRIME + spawnOrder) % VARIATION_COUNT;
            return idx < 0 ? idx + VARIATION_COUNT : idx;
        }
    }
}
