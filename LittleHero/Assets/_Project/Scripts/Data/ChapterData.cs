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

        [Tooltip("적 HP/공격력에 곱하는 챕터 곡선 (chapter_curve §9.1). 미리 계산해 SO에 저장 — 보통 Mathf.Pow(1.10, chapterNumber-1). 기본 1f = 영향 없음. B7가 35개 SO에 채움")]
        public float enemyStatMultiplier = 1f;

        [Header("웨이브 설정")]
        public int totalWaves = 20;
        public float waveDuration = 45f;
        public EnemyData[] enemyPool;
        public EnemyData[] miniBossPool;
        public EnemyData bossEnemy;

        [Header("5038 step 1 — Wave 단위 데이터 (runtime 미사용, step 3 통합 예정)")]
        [Tooltip("waves[N-1] 가 set 이어도 step 1 단계에서는 ChapterRunner 가 사용 안 함 — Seeder 시드만 (step 2). step 3 cascade 시 BuildFromComposition 분기 추가.")]
        public WaveComposition[] waves;

        [Header("보상")]
        public int starFragmentReward;
    }
}
