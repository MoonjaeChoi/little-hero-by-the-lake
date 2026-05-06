using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5023 — Currency + Character service 단위 테스트.
    /// Note: MetaProgression.Instance 가 SaveSystem.Save 호출하므로 임시 SaveData 보호 필요.
    /// </summary>
    public class MetaProgressionServicesTests
    {
        private MetaProgression _instance;
        private SaveData _backup;

        [SetUp]
        public void SetUp()
        {
            // 임시 MetaProgression GameObject + SaveData 백업
            var go = new GameObject("TestMetaProgression");
            _instance = go.AddComponent<MetaProgression>();
            // Awake 자동 호출 시 Save 트리거 — 백업 후 복원
            _backup = SaveSystem.Load();
        }

        [TearDown]
        public void TearDown()
        {
            if (_instance != null) Object.DestroyImmediate(_instance.gameObject);
            // 원본 SaveData 복원
            if (_backup != null) SaveSystem.Save(_backup);
        }

        [Test]
        public void Currency_AddStarFragments_DataUpdated()
        {
            int before = MetaProgression.Instance.Data.starFragments;
            MetaProgressionCurrency.AddStarFragments(50);
            Assert.AreEqual(before + 50, MetaProgression.Instance.Data.starFragments);
        }

        [Test]
        public void Currency_SpendStarFragments_DeductsCorrectly()
        {
            MetaProgression.Instance.Data.starFragments = 100;
            bool ok = MetaProgressionCurrency.SpendStarFragments(30);
            Assert.IsTrue(ok);
            Assert.AreEqual(70, MetaProgression.Instance.Data.starFragments);
        }

        [Test]
        public void Currency_AddStarlight_DataUpdated()
        {
            int before = MetaProgression.Instance.Data.starlight;
            MetaProgressionCurrency.AddStarlight(20);
            Assert.AreEqual(before + 20, MetaProgression.Instance.Data.starlight);
        }

        [Test]
        public void Currency_AddMagicCrystals_DataUpdated()
        {
            int before = MetaProgression.Instance.Data.magicCrystals;
            MetaProgressionCurrency.AddMagicCrystals(5);
            Assert.AreEqual(before + 5, MetaProgression.Instance.Data.magicCrystals);
        }

        [Test]
        public void Character_ResolveCharacterClass_Warrior()
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            data.className = "전사";
            Assert.AreEqual(CharacterClass.Warrior, MetaProgressionCharacter.ResolveCharacterClass(data));
            Object.DestroyImmediate(data);
        }

        [Test]
        public void Character_ResolveCharacterClass_NullDefaults()
        {
            Assert.AreEqual(CharacterClass.Warrior, MetaProgressionCharacter.ResolveCharacterClass(null));
        }
    }
}
