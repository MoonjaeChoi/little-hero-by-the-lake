using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// B4 — 부옵션 글로벌 풀 (item_drop_v1 §3.1).
    /// 모든 슬롯이 같은 풀을 공유 — 슬롯 정체성은 주옵션이 책임지므로 부옵션은 광범위·균등.
    /// 수치는 주옵션 대비 30~50% 수준. 부옵션 6줄 합산 ≈ 주옵션 3줄 가치.
    /// 가중치(globalWeight)는 GlobalSubAffixPool 에서 합산 사용 (item_drop §3.2).
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/SubAffixDefinition")]
    public class SubAffixDefinition : ScriptableObject
    {
        public string id;
        public string displayName;
        public StatTarget target;
        public AffixOp op;

        [Header("등급별 [min, max] (Common~Legendary)")]
        public float[] minByRarity = new float[5];
        public float[] maxByRarity = new float[5];

        [Header("글로벌 풀 가중치 (item_drop §3.2, 합 = 1.0)")]
        [Range(0f, 1f)] public float globalWeight = 0.05f;

        [Header("표시")]
        public string suffix = "%";
        [Range(0, 3)] public int displayDecimals = 1;
    }
}
