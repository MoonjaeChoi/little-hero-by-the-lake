using NUnit.Framework;
using LitMotion;

namespace LittleHero.Tests.Editor
{
    public class LMotionExtensionsSmokeTest
    {
        [Test]
        public void Sprint1_LMotion_Create_Bind_ProducesActiveHandle()
        {
            float result = 0f;
            var handle = LMotion.Create(0f, 1f, 0.1f).Bind(v => result = v);
            Assert.IsTrue(handle.IsActive(), "MotionHandle should be active after Create+Bind");
            handle.Cancel();
        }

        [Test]
        public void Sprint1_LMotion_Cancel_DeactivatesHandle()
        {
            var handle = LMotion.Create(0f, 1f, 1f).Bind(_ => { });
            Assert.IsTrue(handle.IsActive());
            handle.Cancel();
            Assert.IsFalse(handle.IsActive());
        }
    }
}
