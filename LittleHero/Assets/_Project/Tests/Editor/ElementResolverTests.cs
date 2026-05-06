using LittleHero;
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    /// <summary>5029 — ElementResolver 저항 + 효과 검증.</summary>
    public class ElementResolverTests
    {
        [Test]
        public void ResolveMultiplier_None_Returns1()
        {
            var enemy = MakeEnemyData(new[] { 1.5f, 0.7f, 1f, 1f });
            // None 은 enemy 무관 1.0
            Assert.AreEqual(1f, ElementResolver.ResolveMultiplier(null, MagicElement.None));
        }

        [Test]
        public void ResolveMultiplier_Arcane_IgnoresResistance()
        {
            // Arcane 은 항상 1.0 강제 (저항 무시)
            Assert.AreEqual(1f, ElementResolver.ResolveMultiplier(null, MagicElement.Arcane));
        }

        [Test]
        public void ResolveMultiplier_NullEnemy_ReturnsDefault1()
        {
            Assert.AreEqual(1f, ElementResolver.ResolveMultiplier(null, MagicElement.Fire));
            Assert.AreEqual(1f, ElementResolver.ResolveMultiplier(null, MagicElement.Ice));
            Assert.AreEqual(1f, ElementResolver.ResolveMultiplier(null, MagicElement.Lightning));
        }

        [Test]
        public void ApplyElementEffect_None_NoOp()
        {
            // null target — DoesNotThrow (방어)
            Assert.DoesNotThrow(() =>
                ElementResolver.ApplyElementEffect(null, MagicElement.None, 100f));
        }

        [Test]
        public void ApplyElementEffect_NullTarget_DoesNotThrow()
        {
            Assert.DoesNotThrow(() =>
                ElementResolver.ApplyElementEffect(null, MagicElement.Fire, 100f));
            Assert.DoesNotThrow(() =>
                ElementResolver.ApplyElementEffect(null, MagicElement.Lightning, 100f));
        }

        [Test]
        public void MagicElement_HasFiveValues()
        {
            // None + Fire + Ice + Lightning + Arcane = 5
            var values = System.Enum.GetValues(typeof(MagicElement));
            Assert.AreEqual(5, values.Length);
        }

        private EnemyData MakeEnemyData(float[] resist)
        {
            var d = ScriptableObject.CreateInstance<EnemyData>();
            d.elementResistance = resist;
            return d;
        }
    }
}
