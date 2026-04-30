using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// B6 — 부옵션 글로벌 공용 풀 (item_drop §3.2).
    /// 모든 슬롯이 공유. 가중치는 각 SubAffixDefinition.globalWeight 사용.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/GlobalSubAffixPool")]
    public class GlobalSubAffixPool : ScriptableObject
    {
        public SubAffixDefinition[] entries;
    }
}
