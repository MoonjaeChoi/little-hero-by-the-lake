using NUnit.Framework;
using UnityEditor;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5038 — ChapterData.waves[] 인프라 검증 (NUnit 2).
    /// Step 1 (필드) + Step 3 대안 (BuildFromComposition 단순화). Seeder + 시드 SO 는 5038.1 cascade 보류.
    /// </summary>
    public class ChapterWavesTests
    {
        private static ChapterData LoadChapter(int n)
        {
            string[] guids = AssetDatabase.FindAssets("t:ChapterData");
            for (int i = 0; i < guids.Length; i++)
            {
                var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (ch != null && ch.chapterNumber == n) return ch;
            }
            return null;
        }

        [Test]
        public void ChapterData_Has_Waves_Field()
        {
            var ch = LoadChapter(1);
            Assert.IsNotNull(ch);
            Assert.IsNotNull(ch.GetType().GetField("waves"), "ChapterData.waves[] 필드 부재 (Step 1)");
        }

        [Test]
        public void All_Chapters_Waves_Null_Or_Empty_By_Default()
        {
            // Step 1+3 — 시드 도구 없이 모든 챕터 waves[] 미설정 → enemyPool fallback 보장
            string[] guids = AssetDatabase.FindAssets("t:ChapterData");
            int chapters = 0, fallback = 0;
            for (int i = 0; i < guids.Length; i++)
            {
                var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(AssetDatabase.GUIDToAssetPath(guids[i]));
                if (ch == null) continue;
                chapters++;
                if (ch.waves == null || ch.waves.Length == 0) fallback++;
            }
            Assert.AreEqual(chapters, fallback, $"전체 {chapters} 챕터 중 fallback {fallback} — 5038.1 시드 전 모두 fallback 기대");
        }
    }
}
