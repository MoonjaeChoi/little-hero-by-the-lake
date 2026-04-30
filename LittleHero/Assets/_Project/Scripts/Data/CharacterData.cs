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

        [Header("언락 조건")]
        public int starFragmentCost;
    }
}
