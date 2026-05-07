using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5037.1 — 챕터별 적 변형 배치 룰 데이터화 (5037 D3=A 정적 → SO 우선 §15.4).
    /// chapterNumber → 어떤 archetype 변형 SO 가 어느 wave 에 추가될지 매트릭스.
    /// 1 인스턴스 (ChapterVariantRule_Default) 로 전체 35 챕터 룰 관리.
    /// </summary>
    [CreateAssetMenu(menuName = "LittleHero/ChapterVariantRule")]
    public class ChapterVariantRule : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            [Tooltip("챕터 번호 (1~35)")]
            public int chapterNumber;
            [Tooltip("variant SO id (예 Variant_Stationary_Sentry)")]
            public string variantId;
            [Tooltip("배치 위치 — MiniBossPool / EnemyPool")]
            public Slot slot;
        }

        public enum Slot
        {
            MiniBossPool = 0,
            EnemyPool    = 1,
        }

        [Tooltip("챕터별 archetype 배치 룰 — 빈 배열이면 정적 fallback")]
        public Entry[] entries;
    }
}
