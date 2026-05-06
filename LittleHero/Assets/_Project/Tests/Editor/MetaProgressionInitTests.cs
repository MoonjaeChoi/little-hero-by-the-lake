using NUnit.Framework;
using UnityEditor;

namespace LittleHero.Tests
{
    public class MetaProgressionInitTests
    {
        private const string POOL_PATH = "Assets/_Project/Data/Characters/CharacterPool_Default.asset";

        private CharacterPool LoadPool()
        {
            var pool = AssetDatabase.LoadAssetAtPath<CharacterPool>(POOL_PATH);
            Assert.IsNotNull(pool, $"{POOL_PATH} 로드 실패 — Tools/LittleHero/Setup CharacterPool Default 실행 필요");
            return pool;
        }

        [Test]
        public void ResolveCharacter_EmptyId_RemainsNull()
        {
            var pool = LoadPool();
            Assert.IsNull(pool.FindById(""), "빈 id → null");
            Assert.IsNull(pool.FindById(null), "null id → null");
        }

        [Test]
        public void ResolveCharacter_HeroWarrior_ResolvesToWarriorSO()
        {
            var pool = LoadPool();
            var c = pool.FindById("hero_warrior");
            Assert.IsNotNull(c);
            Assert.AreEqual("hero_warrior", c.id);
            Assert.AreEqual("전사", c.className);
        }

        [Test]
        public void ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior()
        {
            var migrated = SaveDataMigration.MigrateCharacterId("default_boy");
            Assert.AreEqual("hero_warrior", migrated);
            var pool = LoadPool();
            var c = pool.FindById(migrated);
            Assert.IsNotNull(c);
            Assert.AreEqual("hero_warrior", c.id);
        }
    }
}
