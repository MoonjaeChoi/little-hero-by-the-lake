using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/Enemy")]
    public class EnemyData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public GameObject prefab;

        [Header("스탯")]
        public float maxHealth;
        public float moveSpeed;
        public float damage;
        public float xpDrop;
        public float starDustDrop;

        [Header("외형")]
        public Color tintColor = Color.white;
        public float scaleMultiplier = 1f;

        [Header("분류")]
        public bool isMiniB;
        public bool isBoss;
    }
}
