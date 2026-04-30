using UnityEngine;

namespace LittleHero
{
    [CreateAssetMenu(menuName = "LittleHero/Chapter")]
    public class ChapterData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite coverArt;

        [Header("웨이브 설정")]
        public int totalWaves = 20;
        public float waveDuration = 45f;
        public EnemyData[] enemyPool;
        public EnemyData[] miniBossPool;
        public EnemyData bossEnemy;

        [Header("보상")]
        public int starFragmentReward;
    }
}
