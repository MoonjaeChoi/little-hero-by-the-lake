using NUnit.Framework;
using System;
using UnityEngine;

namespace LittleHero.Tests
{
    /// <summary>
    /// 5024 T04 — ItemDropService 정적 service Test.
    /// D2=B — TestUtils 로 RNG 결정성 보장 (UnityEngine.Random 와 별도 — System.Random 사용).
    /// 5017 회귀 — DropContext struct 검증.
    /// </summary>
    public class ItemDropServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            ItemDropService.Reset();   // 각 Test 독립 시작
            TestUtils.InitDeterministic(12345);
        }

        [TearDown]
        public void TearDown()
        {
            ItemDropService.Reset();
            TestUtils.RestoreRandom();
        }

        [Test]
        public void IsInitialized_AfterReset_False()
        {
            Assert.IsFalse(ItemDropService.IsInitialized);
        }

        [Test]
        public void TryDrop_Uninitialized_ReturnsNull()
        {
            var ctx = new DropContext();
            var result = ItemDropService.TryDrop(ctx);
            Assert.IsNull(result, "초기화 미호출 시 null + 경고 (Editor.LogWarning)");
            // LogAssert.Expect 로 경고 확인 가능 (선택)
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, "[ItemDropService] Initialize 미호출 — 드랍 무시");
        }

        [Test]
        public void Initialize_NullMythics_NoThrow()
        {
            // mythics 파라미터 default null 허용
            Assert.DoesNotThrow(() =>
                ItemDropService.Initialize(null, null, null, null));
            Assert.IsTrue(ItemDropService.IsInitialized);
        }

        [Test]
        public void Reset_AfterInitialize_SetsUninitialized()
        {
            ItemDropService.Initialize(null, null, null, null);
            Assert.IsTrue(ItemDropService.IsInitialized);
            ItemDropService.Reset();
            Assert.IsFalse(ItemDropService.IsInitialized);
        }

        [Test]
        public void DropContext_IsValueType_5017Regression()
        {
            // 5017 T02 — DropContext class → struct (heap alloc 폐기)
            var t = typeof(DropContext);
            Assert.IsTrue(t.IsValueType, "DropContext 는 struct (5017 GC critical 정합)");
        }

        [Test]
        public void TryDrop_ForcedWeaponSlot_ReturnsNull()
        {
            // PIVOT C6 — 무기 슬롯은 가챠 한정 (drop 풀 제외)
            ItemDropService.Initialize(new SlotAffixPool[0], null, new EquipmentTemplate[0], null);
            var ctx = new DropContext { forcedSlot = EquipSlot.Weapon };
            var result = ItemDropService.TryDrop(ctx);
            Assert.IsNull(result, "forcedSlot=Weapon 은 drop 거부 (가챠 한정)");
        }

        [Test]
        public void TryDrop_EmptyTemplates_ReturnsNull()
        {
            ItemDropService.Initialize(new SlotAffixPool[0], null, new EquipmentTemplate[0], null);
            var ctx = new DropContext();
            // 매칭 템플릿 0 → null
            var result = ItemDropService.TryDrop(ctx, new System.Random(42));
            Assert.IsNull(result);
        }

        [Test]
        public void Initialize_DataPreserved_AcrossMultipleCalls()
        {
            // 첫 init 후 재 init 시 가장 최근 값 사용
            ItemDropService.Initialize(null, null, null, null);
            bool firstInit = ItemDropService.IsInitialized;
            ItemDropService.Initialize(new SlotAffixPool[0], null, null, null);
            bool secondInit = ItemDropService.IsInitialized;
            Assert.IsTrue(firstInit && secondInit, "재 init 후에도 IsInitialized=true");
        }
    }
}
