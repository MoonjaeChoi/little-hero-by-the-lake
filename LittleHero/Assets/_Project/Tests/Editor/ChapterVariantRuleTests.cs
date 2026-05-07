using NUnit.Framework;
using UnityEditor;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5037.1 — ChapterVariantRule SO + CH6~15 cascade wiring 검증 (NUnit 4).
    /// </summary>
    public class ChapterVariantRuleTests
    {
        private const string RULE_PATH = "Assets/_Project/Data/Rules/ChapterVariantRule_Default.asset";

        private static ChapterVariantRule LoadRule()
            => AssetDatabase.LoadAssetAtPath<ChapterVariantRule>(RULE_PATH);

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

        private static bool MiniBossContains(ChapterData ch, string variantId)
        {
            if (ch == null || ch.miniBossPool == null) return false;
            for (int i = 0; i < ch.miniBossPool.Length; i++)
                if (ch.miniBossPool[i] != null && ch.miniBossPool[i].id == variantId) return true;
            return false;
        }

        private static bool EnemyPoolContains(ChapterData ch, string variantId)
        {
            if (ch == null || ch.enemyPool == null) return false;
            for (int i = 0; i < ch.enemyPool.Length; i++)
                if (ch.enemyPool[i] != null && ch.enemyPool[i].id == variantId) return true;
            return false;
        }

        [Test]
        public void RuleSO_Has_Entries()
        {
            var rule = LoadRule();
            Assert.IsNotNull(rule, "ChapterVariantRule_Default SO 부재 (메뉴 실행 후)");
            Assert.IsNotNull(rule.entries);
            Assert.GreaterOrEqual(rule.entries.Length, 16, "entries 수 부족 (CH3~15 매트릭스)");
        }

        [Test]
        public void Chapter10_Has_Bomber_And_Ranged()
        {
            var ch = LoadChapter(10);
            Assert.IsNotNull(ch);
            Assert.IsTrue(MiniBossContains(ch, "Variant_Bomber_Mine"), "CH10 miniBossPool Bomber 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Ranged_Archer"), "CH10 enemyPool Ranged 부재");
        }

        [Test]
        public void Chapter15_Has_Bomber_Ranged_Charger()
        {
            var ch = LoadChapter(15);
            Assert.IsNotNull(ch);
            Assert.IsTrue(MiniBossContains(ch, "Variant_Bomber_Mine"), "CH15 miniBossPool Bomber 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Ranged_Archer"), "CH15 enemyPool Ranged 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Charger_Bull"), "CH15 enemyPool Charger 부재");
        }

        [Test]
        public void Stationary_Variant_Has_ProjectilePrefab()
        {
            var so = AssetDatabase.LoadAssetAtPath<EnemyData>(
                "Assets/_Project/Data/Enemies/Variants/Variant_Stationary_Sentry.asset");
            Assert.IsNotNull(so);
            Assert.IsNotNull(so.projectilePrefab, "Stationary projectilePrefab 미wiring (5037.1 cascade)");
        }

        // 5037.2 — CH16~25 cascade 검증
        [Test]
        public void Chapter18_Has_Stationary_Ranged_Charger()
        {
            var ch = LoadChapter(18);
            Assert.IsNotNull(ch);
            Assert.IsTrue(MiniBossContains(ch, "Variant_Stationary_Sentry"), "CH18 miniBossPool Stationary 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Ranged_Archer"), "CH18 enemyPool Ranged 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Charger_Bull"), "CH18 enemyPool Charger 부재");
        }

        [Test]
        public void Chapter25_Has_Bomber_Ranged_Charger()
        {
            var ch = LoadChapter(25);
            Assert.IsNotNull(ch);
            Assert.IsTrue(MiniBossContains(ch, "Variant_Bomber_Mine"), "CH25 miniBossPool Bomber 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Ranged_Archer"), "CH25 enemyPool Ranged 부재");
            Assert.IsTrue(EnemyPoolContains(ch, "Variant_Charger_Bull"), "CH25 enemyPool Charger 부재");
        }

        [Test]
        public void RuleSO_Has_42_Entries_After_Cascade()
        {
            var rule = LoadRule();
            Assert.IsNotNull(rule);
            Assert.GreaterOrEqual(rule.entries.Length, 42, "5037.2 cascade 후 entries < 42");
        }
    }
}
