using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    /// <summary>
    /// 5017 T04 — DamageNumberFloater FormatDamage cache 검증.
    /// FormatDamage 는 private static — Reflection 으로 호출.
    /// </summary>
    public class DamageNumberFloaterFormatTests
    {
        private static MethodInfo _formatMethod;

        [OneTimeSetUp]
        public void Setup()
        {
            _formatMethod = typeof(DamageNumberFloater).GetMethod(
                "FormatDamage",
                BindingFlags.NonPublic | BindingFlags.Static);
        }

        private static string Format(int damage, DamageType type)
            => (string)_formatMethod.Invoke(null, new object[] { damage, type });

        [Test]
        public void FormatDamage_BelowThousand_NoSuffix()
        {
            Assert.AreEqual("100", Format(100, DamageType.Normal));
            Assert.AreEqual("999", Format(999, DamageType.Normal));
        }

        [Test]
        public void FormatDamage_OverMillion_MSuffix()
        {
            string result = Format(1_500_000, DamageType.Normal);
            Assert.IsTrue(result.EndsWith("M"), $"Expected 'M' suffix, got: {result}");
        }

        [Test]
        public void FormatDamage_Cache_RepeatedCallReturnsSameInstance()
        {
            // 첫 호출 — cache miss + insert
            string s1 = Format(12345, DamageType.Critical);
            // 두 번째 호출 — cache hit, 동일 string instance 반환
            string s2 = Format(12345, DamageType.Critical);
            // string interning 으로 동일 값은 같은 reference 일 수 있음 — ReferenceEquals 검증
            Assert.IsTrue(ReferenceEquals(s1, s2),
                "Cache hit 시 동일 string instance 반환 (alloc 0)");
        }

        [Test]
        public void Type_DamageNumberFloater_FormatDamageIsStatic()
        {
            Assert.IsNotNull(_formatMethod, "FormatDamage 메서드 존재");
            Assert.IsTrue(_formatMethod.IsStatic, "FormatDamage 는 정적");
            Assert.IsTrue(_formatMethod.IsPrivate, "FormatDamage 는 private");
        }
    }
}
