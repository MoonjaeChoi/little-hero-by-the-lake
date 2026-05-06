using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5029 — element 별 VFX placeholder. 현재 VfxPool 정적 메서드 활용 (SpawnHitStar/StarBurst).
    /// 5029.2 후속 — 무료 에셋 또는 자체 합성 element 별 VFX prefab 정착.
    /// </summary>
    public static class ElementVfxBank
    {
        public static Color GetColor(MagicElement element) => element switch
        {
            MagicElement.Fire      => new Color(1.0f, 0.4f, 0.1f),  // 빨강·주황
            MagicElement.Ice       => new Color(0.4f, 0.8f, 1.0f),  // 시안
            MagicElement.Lightning => new Color(1.0f, 0.95f, 0.3f), // 노랑
            MagicElement.Arcane    => new Color(0.8f, 0.4f, 1.0f),  // 보라
            _                      => Color.white,
        };

        /// <summary>element placeholder VFX — 5007 SpawnHitStar 활용 (색조는 element 별).</summary>
        public static void Spawn(MagicElement element, Vector3 position)
        {
            if (element == MagicElement.None) return;
            // placeholder — 5029.2 후속 에서 element prefab + VfxPool.Register 정착
            VfxPool.SpawnStarBurst(position);
        }
    }
}
