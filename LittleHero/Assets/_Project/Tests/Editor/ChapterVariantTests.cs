using System;
using NUnit.Framework;
using UnityEditor;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5037 — Variant SO + CH3/4/5 시범 wiring 검증 (NUnit 6).
    /// </summary>
    public class ChapterVariantTests
    {
        private const string VARIANTS_PATH = "Assets/_Project/Data/Enemies/Variants";

        private static EnemyData LoadVariant(string id)
        {
            return AssetDatabase.LoadAssetAtPath<EnemyData>($"{VARIANTS_PATH}/{id}.asset");
        }

        private static ChapterData LoadChapterByNumber(int chapterNum)
        {
            string[] guids = AssetDatabase.FindAssets("t:ChapterData");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
                if (ch != null && ch.chapterNumber == chapterNum) return ch;
            }
            return null;
        }

        // === Variant SO 검증 ===

        [Test]
        public void Variant_Stationary_Has_BehaviorType_Stationary()
        {
            var so = LoadVariant("Variant_Stationary_Sentry");
            Assert.IsNotNull(so, "Variant_Stationary_Sentry SO 부재");
            Assert.AreEqual(EnemyBehaviorType.Stationary, so.behaviorType);
            Assert.AreEqual(0f, so.moveSpeed, "Stationary 는 moveSpeed=0 권장");
        }

        [Test]
        public void Variant_Splitter_Has_ChildData_Configured()
        {
            var so = LoadVariant("Variant_Splitter_Bear");
            Assert.IsNotNull(so, "Variant_Splitter_Bear SO 부재");
            Assert.AreEqual(EnemyBehaviorType.Splitter, so.behaviorType);
            Assert.IsNotNull(so.splitterChildData, "Splitter 는 splitterChildData 필수");
            Assert.AreEqual(3, so.splitterChildCount);
        }

        [Test]
        public void Variant_Bomber_Has_ExplodeReaction()
        {
            var so = LoadVariant("Variant_Bomber_Mine");
            Assert.IsNotNull(so, "Variant_Bomber_Mine SO 부재");
            Assert.AreEqual(EnemyBehaviorType.Bomber, so.behaviorType);
            Assert.AreEqual(EnemyDeathReactionType.Explode, so.deathReactionType);
            Assert.AreEqual(1.5f, so.bomberRadius, 0.001f);
        }

        // === Chapter wiring 검증 (D2=C chapterNumber 스캔) ===

        [Test]
        public void Chapter03_MiniBossPool_Contains_Stationary()
        {
            var ch = LoadChapterByNumber(3);
            Assert.IsNotNull(ch, "chapterNumber=3 ChapterData 부재");
            var variant = LoadVariant("Variant_Stationary_Sentry");
            Assert.IsNotNull(ch.miniBossPool, "miniBossPool=null");
            Assert.IsTrue(Array.IndexOf(ch.miniBossPool, variant) >= 0,
                "CH3 miniBossPool 에 Variant_Stationary_Sentry 부재");
        }

        [Test]
        public void Chapter04_MiniBossPool_Contains_Splitter()
        {
            var ch = LoadChapterByNumber(4);
            Assert.IsNotNull(ch, "chapterNumber=4 ChapterData 부재");
            var variant = LoadVariant("Variant_Splitter_Bear");
            Assert.IsTrue(Array.IndexOf(ch.miniBossPool, variant) >= 0,
                "CH4 miniBossPool 에 Variant_Splitter_Bear 부재");
        }

        [Test]
        public void Chapter05_MiniBossPool_Contains_Bomber()
        {
            var ch = LoadChapterByNumber(5);
            Assert.IsNotNull(ch, "chapterNumber=5 ChapterData 부재");
            var variant = LoadVariant("Variant_Bomber_Mine");
            Assert.IsTrue(Array.IndexOf(ch.miniBossPool, variant) >= 0,
                "CH5 miniBossPool 에 Variant_Bomber_Mine 부재");
        }
    }
}
