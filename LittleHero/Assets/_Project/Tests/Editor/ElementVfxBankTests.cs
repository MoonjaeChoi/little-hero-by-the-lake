using System.Reflection;
using LittleHero;
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    /// <summary>5033 — ElementVfxBank Resources Load + None 가드 + Spawn 안전성 검증.</summary>
    public class ElementVfxBankTests
    {
        [SetUp]
        public void Reset()
        {
            // _initialized 리셋 — 다른 테스트 영향 격리 (lazy load 보장)
            var field = typeof(ElementVfxBank).GetField("_initialized",
                BindingFlags.NonPublic | BindingFlags.Static);
            field?.SetValue(null, false);
        }

        [Test]
        public void GetPrefab_Fire_NotNull()
        {
            var prefab = ElementVfxBank.GetPrefab(MagicElement.Fire);
            Assert.IsNotNull(prefab, "Resources/VFX/Element/Fire.prefab 가 로드되지 않았습니다 (T01 누락 확인).");
        }

        [Test]
        public void GetPrefab_None_ReturnsNull()
        {
            var prefab = ElementVfxBank.GetPrefab(MagicElement.None);
            Assert.IsNull(prefab);
        }

        [Test]
        public void Spawn_None_DoesNotThrow()
        {
            // Spawn(None) 은 early return — exception 미발생
            Assert.DoesNotThrow(() => ElementVfxBank.Spawn(MagicElement.None, Vector3.zero));
        }
    }
}
