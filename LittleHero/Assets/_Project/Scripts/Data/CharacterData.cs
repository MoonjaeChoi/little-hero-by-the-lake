using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/Character")]
    public class CharacterData : ScriptableObject
    {
        public string id;
        public string displayName;
        public string className;       // stats_v2 — '전사' / '궁수' / '마법사'
        [TextArea] public string description;
        public Sprite portrait;
        public GameObject prefab;

        [Header("기본 스탯")]
        public float maxHealth;
        public float moveSpeed;
        public float attackDamage;
        public float attackSpeed;
        public float range;

        [Header("외형")]
        public Color tintColor = Color.white;

        [Header("코어 스탯 비중 (stats_v2 §6 — 3 코어 직업 차별화)")]
        public float strengthMultiplier = 1f;
        public float agilityMultiplier = 1f;
        public float wisdomMultiplier = 1f;
        // spiritMultiplier 폐기 (Spirit 코어 제거)

        [Header("언락 조건")]
        public int starFragmentCost;
    }
}
