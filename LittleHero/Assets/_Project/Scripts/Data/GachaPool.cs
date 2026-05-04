using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// PIVOT Phase C3 — 무기 가챠 풀 (gacha_v1.md §1).
    /// 등급별 EquipmentTemplate 풀 + 소환 레벨별 등급 확률 + 비용.
    /// 출시 시 4 등급 (Common~Epic), 출시 후 + Legendary·Mythic 풀.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/GachaPool")]
    public class GachaPool : ScriptableObject
    {
        public string id;
        public string displayName;

        [Header("등급별 풀 (출시 시 4 등급)")]
        public EquipmentTemplate[] commonPool;
        public EquipmentTemplate[] magicPool;
        public EquipmentTemplate[] rarePool;
        public EquipmentTemplate[] epicPool;

        [Header("등급별 풀 (출시 후 — 소환 레벨 11+)")]
        public EquipmentTemplate[] legendaryPool;
        public EquipmentTemplate[] mythicPool;

        [Header("소환 레벨별 등급 확률 (length = MAX_SUMMON_LEVEL)")]
        public SummonLevelProbability[] probabilities;

        [Header("소환 레벨 누적 임계값 (length = MAX_SUMMON_LEVEL)")]
        [Tooltip("Lv N 도달 누적 소환 횟수. Lv 1=0, Lv 2=100, ..., Lv 10=27000")]
        public int[] summonLevelThresholds;

        [Header("비용")]
        public int starlightCostSingle = 100;
        public int starlightCost10Pull = 900;

        [Header("마법의돌 조각 부수 획득 (등급별)")]
        public int shardsCommon = 1;
        public int shardsMagic = 2;
        public int shardsRare = 5;
        public int shardsEpic = 15;
        public int shardsLegendary = 50;
        public int shardsMythic = 200;

        public const int MAX_SUMMON_LEVEL = 10;
    }

    [System.Serializable]
    public class SummonLevelProbability
    {
        public int level;  // 1~10
        // 0~1 비율, 합 = 1.0
        public float commonRate;
        public float magicRate;
        public float rareRate;
        public float epicRate;
        public float legendaryRate;
        public float mythicRate;
    }
}
