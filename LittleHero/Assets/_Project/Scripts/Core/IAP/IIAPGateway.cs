using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Purchasing;

namespace LittleHero
{
    /// <summary>
    /// 5031 — IAP 추상화 (D2=B Mock 친화).
    /// PendingOrder/FailedOrder 노출 X — productId + receipt + reason 단순 인터페이스.
    /// UnityIAPGateway (v5 wrapper) + MockIAPGateway (Test) 두 구현.
    /// </summary>
    public interface IIAPGateway
    {
        bool IsReady { get; }

        Task InitializeAsync(List<ProductDefinition> products);
        void PurchaseProduct(string productId);
        string GetLocalizedPrice(string productId);

        event Action OnReady;
        event Action<string, string> OnPurchaseSucceeded;  // (productId, receipt)
        event Action<string, string> OnPurchaseFailed;     // (productId, reason)
    }
}
