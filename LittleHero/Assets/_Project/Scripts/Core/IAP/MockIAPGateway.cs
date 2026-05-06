using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace LittleHero
{
    /// <summary>
    /// 5031 — Test 전용 in-memory IIAPGateway 구현 (D2=B Mock).
    /// SimulatePurchaseSuccess / SimulatePurchaseFailure 로 단위 Test.
    /// </summary>
    public class MockIAPGateway : IIAPGateway
    {
        private readonly Dictionary<string, string> _prices = new();
        private bool _ready;

        public bool IsReady => _ready;

        public event Action OnReady;
        public event Action<string, string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public string LastPurchaseRequest { get; private set; }
        public int PurchaseCallCount { get; private set; }

        public Task InitializeAsync(List<ProductDefinition> products)
        {
            if (products != null)
                foreach (var p in products)
                    if (!string.IsNullOrEmpty(p.id)) _prices[p.id] = "$0.99";
            _ready = true;
            OnReady?.Invoke();
            return Task.CompletedTask;
        }

        public void PurchaseProduct(string productId)
        {
            LastPurchaseRequest = productId;
            PurchaseCallCount++;
        }

        public string GetLocalizedPrice(string productId)
            => _prices.TryGetValue(productId, out var p) ? p : string.Empty;

        // ===== Test 전용 시뮬 =====

        public void SimulatePurchaseSuccess(string productId, string receipt = "")
            => OnPurchaseSucceeded?.Invoke(productId, receipt);

        public void SimulatePurchaseFailure(string productId, string reason)
            => OnPurchaseFailed?.Invoke(productId, reason);

        public void SetLocalizedPrice(string productId, string price)
            => _prices[productId] = price;
    }
}
