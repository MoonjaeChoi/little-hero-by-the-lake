using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/Character")]
    public class CharacterData : ScriptableObject
    {
        public string id;
        public string displayName;
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

        [Header("코어 스탯 비중 (stats_v1 §3.3)")]
        public float strengthMultiplier = 1f;
        public float agilityMultiplier = 1f;
        public float wisdomMultiplier = 1f;
        public float spiritMultiplier = 1f;

        [Header("언락 조건")]
        public int starFragmentCost;
    }
}
