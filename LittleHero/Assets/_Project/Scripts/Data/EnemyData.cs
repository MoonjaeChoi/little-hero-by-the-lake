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

        [Tooltip("방어력 — 0=무방어. DamageCalculator 가 dmg/(1 + armor×(1-관통)/100). 일몹 0, 보스만 ~30~80 권장 (stats_v1 §5)")]
        public float armor = 0f;

        [Header("외형")]
        public Color tintColor = Color.white;
        public float scaleMultiplier = 1f;

        [Header("분류")]
        public bool isMiniB;
        public bool isBoss;
    }
}
