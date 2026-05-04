using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    public class EnemyHueVariationTests
    {
        [Test]
        public void HueShift_Red_Plus50Percent_ReturnsCyan()
        {
            // red(h=0, s=1, v=1) + 0.5 → cyan(h=0.5)
            var result = EnemyHueVariation.HueShift(Color.red, 0.5f);
            Assert.Greater(result.b, 0.5f, "blue 채널 강화");
            Assert.Greater(result.g, 0.5f, "green 채널 강화");
            Assert.Less(result.r, 0.5f, "red 채널 감소");
        }

        [Test]
        public void HueShift_White_NoChange()
        {
            // white saturation=0 → hue 무관 (white 무변화)
            var result = EnemyHueVariation.HueShift(Color.white, 0.3f);
            Assert.AreEqual(1f, result.r, 0.001f);
            Assert.AreEqual(1f, result.g, 0.001f);
            Assert.AreEqual(1f, result.b, 0.001f);
        }

        [Test]
        public void HueShift_ZeroOffset_Identity()
        {
            var input = new Color(0.7f, 0.3f, 0.5f, 1f);
            var result = EnemyHueVariation.HueShift(input, 0f);
            Assert.AreEqual(input.r, result.r, 0.001f);
            Assert.AreEqual(input.g, result.g, 0.001f);
            Assert.AreEqual(input.b, result.b, 0.001f);
        }

        [Test]
        public void VariationIndex_Wave1_Spawn0_Returns0()
        {
            // (1-1)*7 + 0 = 0 % 5 = 0
            Assert.AreEqual(0, EnemyHueVariation.VariationIndex(1, 0));
        }

        [Test]
        public void VariationIndex_Wave1_Spawn7_Returns2()
        {
            // (1-1)*7 + 7 = 7 % 5 = 2
            Assert.AreEqual(2, EnemyHueVariation.VariationIndex(1, 7));
        }

        [Test]
        public void OFFSETS_HasFive_NoDuplicate()
        {
            var s = new HashSet<float>();
            foreach (var f in EnemyHueVariation.OFFSETS) s.Add(f);
            Assert.AreEqual(5, EnemyHueVariation.VARIATION_COUNT);
            Assert.AreEqual(5, s.Count, "5 unique offsets");
            Assert.AreEqual(5, EnemyHueVariation.OFFSETS.Length);
        }
    }
}
