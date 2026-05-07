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
        public void Chapter01_Waves_Has_19_Or_20_Entries()
        {
            // 5038.1 — Seeder 실행 후 검증. Seeder 미실행 시 skip.
            var ch = LoadChapter(1);
            Assert.IsNotNull(ch);
            if (ch.waves == null || ch.waves.Length == 0) Assert.Ignore("CH1 미시드 — Tools/LittleHero/Seed Wave Compositions — CH1 시범 실행 필요");
            Assert.GreaterOrEqual(ch.waves.Length, 19, "CH1 waves 길이 < 19");
        }

        [Test]
        public void Chapter01_Wave5_Is_Elite()
        {
            var ch = LoadChapter(1);
            Assert.IsNotNull(ch);
            if (ch.waves == null || ch.waves.Length < 5) Assert.Ignore("CH1 미시드");
            var w5 = ch.waves[4];
            Assert.IsNotNull(w5, "wave 5 WaveComposition null");
            Assert.AreEqual(WaveType.Elite, w5.waveType, "CH1 wave 5 가 Elite 아님");
        }

        [Test]
        public void Chapter02_Waves_Null_Fallback()
        {
            // 5038.1 시범은 CH1 만이라 CH2 는 fallback 기대
            var ch2 = LoadChapter(2);
            Assert.IsNotNull(ch2);
            bool fallback = ch2.waves == null || ch2.waves.Length == 0;
            Assert.IsTrue(fallback, "CH2 waves 가 set 됨 — 5038.1 시범은 CH1 만이라 fallback 기대");
        }
    }
}
