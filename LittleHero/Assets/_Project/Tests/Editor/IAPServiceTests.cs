using LittleHero;
using NUnit.Framework;

namespace LittleHero.Tests.Editor
{
    /// <summary>5031 — D2=B Mock IIAPGateway 활용 단위 Test.</summary>
    public class IAPServiceTests
    {
        [SetUp]
        public void Setup()
        {
            // Bootstrap with Mock — Singleton 재초기화
            var mock = new MockIAPGateway();
            IAPService.BootstrapForTest(mock);
        }

        [Test]
        public void ProductIds_AllSixDefined()
        {
            Assert.AreEqual("com.littlehero.starlight_100",   IAPService.ID_STARLIGHT_100);
            Assert.AreEqual("com.littlehero.starlight_500",   IAPService.ID_STARLIGHT_500);
            Assert.AreEqual("com.littlehero.starlight_1100",  IAPService.ID_STARLIGHT_1100);
            Assert.AreEqual("com.littlehero.starlight_3500",  IAPService.ID_STARLIGHT_3500);
            Assert.AreEqual("com.littlehero.starlight_12000", IAPService.ID_STARLIGHT_12000);
            Assert.AreEqual("com.littlehero.season_pass",     IAPService.ID_SEASON_PASS);
        }

        [Test]
        public void Bootstrap_WithMock_GatewayReady()
        {
            // SetUp 에서 Mock Bootstrap 후 Mock.InitializeAsync(products) 즉시 IsReady=true
            Assert.IsTrue(IAPService.Instance.IsInitialized);
        }

        [Test]
        public void InitiatePurchase_CallsGatewayWithProductId()
        {
            var mock = new MockIAPGateway();
            IAPService.BootstrapForTest(mock);
            IAPService.Instance.InitiatePurchase(IAPService.ID_STARLIGHT_500);
            Assert.AreEqual(IAPService.ID_STARLIGHT_500, mock.LastPurchaseRequest);
            Assert.AreEqual(1, mock.PurchaseCallCount);
        }

        [Test]
        public void ProductDefinitions_HasSixProducts()
        {
            var defs = IAPProductCatalog.GetProductDefinitions();
            Assert.AreEqual(6, defs.Count);
        }

        [Test]
        public void GetLocalizedPrice_FromMock_ReturnsConfiguredPrice()
        {
            var mock = new MockIAPGateway();
            IAPService.BootstrapForTest(mock);
            mock.SetLocalizedPrice(IAPService.ID_STARLIGHT_100, "₩1,200");
            Assert.AreEqual("₩1,200", IAPService.Instance.GetLocalizedPrice(IAPService.ID_STARLIGHT_100));
        }

        [Test]
        public void SimulatePurchaseSuccess_FiresEvent()
        {
            var mock = new MockIAPGateway();
            IAPService.BootstrapForTest(mock);
            string firedId = null;
            IAPService.Instance.OnPurchaseSucceededEvent += id => firedId = id;
            mock.SimulatePurchaseSuccess(IAPService.ID_STARLIGHT_100, receipt: "");
            Assert.AreEqual(IAPService.ID_STARLIGHT_100, firedId);
        }
    }
}
