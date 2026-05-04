using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero.Tests.Editor
{
    public class EnemyVisibilityCullerTests
    {
        [SetUp]
        public void Setup()
        {
            EnemyVisibilityCuller.InvalidateCache();
        }

        [Test]
        public void IsVisible_NoCamera_DefaultsTrue()
        {
            // Camera.main null 시 안전 fallback true
            // Editor mode 에서 Camera.main 이 없을 가능성 — fallback 검증
            EnemyVisibilityCuller.InvalidateCache();
            bool result = EnemyVisibilityCuller.IsVisible(Vector3.zero);
            // Camera.main null 또는 origin visible → true
            Assert.IsTrue(result);
        }

        [Test]
        public void IsVisible_FarOffscreen_False_WhenCameraExists()
        {
            // Camera.main 이 있으면 매우 먼 위치는 false (Camera.main 없으면 fallback true)
            if (Camera.main == null) Assert.Pass("Camera.main null in Editor — fallback true (covered by other test)");
            EnemyVisibilityCuller.InvalidateCache();
            bool result = EnemyVisibilityCuller.IsVisible(new Vector3(10000, 10000, 0), margin: 1.0f);
            Assert.IsFalse(result);
        }

        [Test]
        public void InvalidateCache_NextCallReinit()
        {
            // _cachedCam private static field — Reflection 으로 null 검증
            EnemyVisibilityCuller.IsVisible(Vector3.zero);
            EnemyVisibilityCuller.InvalidateCache();
            var field = typeof(EnemyVisibilityCuller).GetField("_cachedCam",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "_cachedCam field 존재 확인");
            Assert.IsNull(field.GetValue(null), "InvalidateCache 후 _cachedCam == null");
        }

        [Test]
        public void IsVisible_DefaultMargin_15x()
        {
            // DEFAULT_MARGIN const 1.5f 검증 (Reflection)
            var field = typeof(EnemyVisibilityCuller).GetField("DEFAULT_MARGIN",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(field, "DEFAULT_MARGIN const 존재");
            Assert.AreEqual(1.5f, (float)field.GetValue(null), 0.001f, "D2=A margin 1.5x");
        }

        [Test]
        public void Type_Exists_StaticClass()
        {
            // 정적 클래스 + 정적 메서드 검증
            var t = System.Type.GetType("LittleHero.EnemyVisibilityCuller, Assembly-CSharp");
            Assert.IsNotNull(t, "EnemyVisibilityCuller Type 존재");
            Assert.IsTrue(t.IsAbstract && t.IsSealed, "정적 클래스 (abstract+sealed)");
            var m = t.GetMethod("IsVisible", BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(m, "IsVisible 정적 public 메서드 존재");
            Assert.AreEqual(typeof(bool), m.ReturnType, "IsVisible 리턴 bool");
        }

        [Test]
        public void SceneManager_SceneLoaded_HookExists()
        {
            // Rebuttal §3.3 — RuntimeInitializeOnLoadMethod 자동 hook 검증
            // Init() private static 메서드 존재 + RuntimeInitializeOnLoadMethod 어트리뷰트 검증
            var initMethod = typeof(EnemyVisibilityCuller).GetMethod("Init",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(initMethod, "Init private static 메서드 존재");
            var attrs = initMethod.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
            Assert.AreEqual(1, attrs.Length, "RuntimeInitializeOnLoadMethod 어트리뷰트 존재");
        }
    }
}
