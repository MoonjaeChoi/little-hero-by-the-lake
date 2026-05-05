using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;

namespace LittleHero.Tests.Editor
{
    public class LMotionExtensionsTests
    {
        [Test]
        public void DOFadeAlpha_SpriteRenderer_ReturnsActiveHandle()
        {
            var go = new GameObject("test");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.color = Color.white;

            var handle = sr.DOFadeAlpha(0f, 0.5f);
            Assert.IsTrue(handle.IsActive(), "Handle should be active immediately after Bind");
            handle.Cancel();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DOFadeAlpha_Image_ReturnsActiveHandle()
        {
            var go = new GameObject("test");
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = Color.white;

            var handle = img.DOFadeAlpha(0f, 0.5f);
            Assert.IsTrue(handle.IsActive());
            handle.Cancel();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DOFade_CanvasGroup_ReturnsActiveHandle()
        {
            var go = new GameObject("test");
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;

            var handle = cg.DOFade(0f, 0.5f);
            Assert.IsTrue(handle.IsActive());
            handle.Cancel();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DOPunchScale_Transform_ReturnsActiveHandle()
        {
            var go = new GameObject("test");
            var handle = go.transform.DOPunchScale(0.5f, 0.3f);
            Assert.IsTrue(handle.IsActive());
            handle.Cancel();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DOMove_Transform_ReturnsActiveHandle()
        {
            var go = new GameObject("test");
            var handle = go.transform.DOMove(new Vector3(1f, 2f, 3f), 0.5f);
            Assert.IsTrue(handle.IsActive());
            handle.Cancel();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void DOFillAmount_Image_ReturnsActiveHandle()
        {
            var go = new GameObject("test");
            go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.fillAmount = 0f;

            var handle = img.DOFillAmount(1f, 0.5f);
            Assert.IsTrue(handle.IsActive());
            handle.Cancel();
            Object.DestroyImmediate(go);
        }

        [Test]
        public void Type_LMotionExtensions_IsStaticClass()
        {
            var t = typeof(LMotionExtensions);
            Assert.IsTrue(t.IsAbstract && t.IsSealed, "LMotionExtensions must be a static class");
            Assert.IsTrue(t.GetMethod("DOFadeAlpha", new[] { typeof(SpriteRenderer), typeof(float), typeof(float) }) != null);
            Assert.IsTrue(t.GetMethod("DOFadeAlpha", new[] { typeof(Image), typeof(float), typeof(float) }) != null);
            Assert.IsTrue(t.GetMethod("DOFade", new[] { typeof(CanvasGroup), typeof(float), typeof(float) }) != null);
            Assert.IsTrue(t.GetMethod("DOPunchScale") != null);
            Assert.IsTrue(t.GetMethod("DOMove") != null);
            Assert.IsTrue(t.GetMethod("DOFillAmount") != null);
        }
    }
}
