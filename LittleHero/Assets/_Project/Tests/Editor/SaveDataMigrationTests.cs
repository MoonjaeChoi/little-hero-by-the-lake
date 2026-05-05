using NUnit.Framework;

namespace LittleHero.Tests
{
    public class SaveDataMigrationTests
    {
        [Test]
        public void MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior()
        {
            Assert.AreEqual("hero_warrior", SaveDataMigration.MigrateCharacterId("default_boy"));
        }

        [Test]
        public void MigrateCharacterId_BraveGirl_ReturnsHeroArcher()
        {
            Assert.AreEqual("hero_archer", SaveDataMigration.MigrateCharacterId("brave_girl"));
        }

        [Test]
        public void MigrateCharacterId_QuietFriend_ReturnsHeroMage()
        {
            Assert.AreEqual("hero_mage", SaveDataMigration.MigrateCharacterId("quiet_friend"));
        }

        [Test]
        public void MigrateCharacterId_AlreadyNew_ReturnsSame()
        {
            Assert.AreEqual("hero_warrior", SaveDataMigration.MigrateCharacterId("hero_warrior"));
        }

        [Test]
        public void MigrateUnlockedCharacterIds_ArrayMapping_Works()
        {
            var data = new SaveData
            {
                selectedCharacterId = "default_boy",
                unlockedCharacterIds = new[] { "default_boy", "brave_girl" },
            };
            SaveDataMigration.Apply(data);
            Assert.AreEqual("hero_warrior", data.selectedCharacterId);
            CollectionAssert.AreEqual(new[] { "hero_warrior", "hero_archer" }, data.unlockedCharacterIds);
        }
    }
}
