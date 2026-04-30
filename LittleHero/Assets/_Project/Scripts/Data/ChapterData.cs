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

        [Tooltip("1~35. PlayerProgress 의 XP 모디파이어·챕터 클리어 보너스에 사용. B7가 35개 SO 일괄 생성 시 채움. 기본 0 = 영향 없음")]
        public int chapterNumber = 0;

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
