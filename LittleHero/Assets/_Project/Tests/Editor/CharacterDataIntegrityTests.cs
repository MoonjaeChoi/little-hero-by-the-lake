using NUnit.Framework;
using UnityEditor;

namespace LittleHero.Tests
{
    public class CharacterDataIntegrityTests
    {
        private const string WARRIOR = "Assets/_Project/Data/Characters/Char_HeroWarrior.asset";
        private const string ARCHER  = "Assets/_Project/Data/Characters/Char_HeroArcher.asset";
        private const string MAGE    = "Assets/_Project/Data/Characters/Char_HeroMage.asset";

        private static CharacterData Load(string path)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            Assert.IsNotNull(data, $"{path} 로드 실패");
            return data;
        }

        [Test]
        public void Warrior_HasStr2x_Id_HeroWarrior()
        {
            var c = Load(WARRIOR);
            Assert.AreEqual("hero_warrior", c.id);
            Assert.AreEqual("전사", c.className);
            Assert.AreEqual(2.0f, c.strengthMultiplier, 0.001f);
            Assert.AreEqual(0.5f, c.agilityMultiplier, 0.001f);
            Assert.AreEqual(0.5f, c.wisdomMultiplier, 0.001f);
        }

        [Test]
        public void Archer_HasAgi2x_Id_HeroArcher()
        {
            var c = Load(ARCHER);
            Assert.AreEqual("hero_archer", c.id);
            Assert.AreEqual("궁수", c.className);
            Assert.AreEqual(2.0f, c.agilityMultiplier, 0.001f);
            Assert.AreEqual(0.5f, c.strengthMultiplier, 0.001f);
            Assert.AreEqual(0.5f, c.wisdomMultiplier, 0.001f);
        }

        [Test]
        public void Mage_HasWis2x_Id_HeroMage()
        {
            var c = Load(MAGE);
            Assert.AreEqual("hero_mage", c.id);
            Assert.AreEqual("마법사", c.className);
            Assert.AreEqual(2.0f, c.wisdomMultiplier, 0.001f);
            Assert.AreEqual(0.5f, c.strengthMultiplier, 0.001f);
            Assert.AreEqual(0.5f, c.agilityMultiplier, 0.001f);
        }

        [Test]
        public void AllStarterHeroes_HavePortraitWired()
        {
            Assert.IsNotNull(Load(WARRIOR).portrait, "전사 portrait 미wiring");
            Assert.IsNotNull(Load(ARCHER).portrait,  "궁수 portrait 미wiring");
            Assert.IsNotNull(Load(MAGE).portrait,    "마법사 portrait 미wiring");
        }
    }
}
