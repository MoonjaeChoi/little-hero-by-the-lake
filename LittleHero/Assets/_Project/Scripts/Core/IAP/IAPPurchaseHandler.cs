using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleHero
{
    /// <summary>
    /// 5031 — v5 OnPurchasePending / OnPurchaseFailed 핸들러.
    /// IAPService facade 가 StoreController 이벤트를 본 핸들러로 라우팅.
    /// </summary>
    public class IAPPurchaseHandler
    {
        private readonly StoreController _store;
        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public IAPPurchaseHandler(StoreController store)
        {
            _store = store;
        }

        public void HandlePending(PendingOrder order)
        {
            string productId = ExtractProductId(order);
            if (string.IsNullOrEmpty(productId))
            {
                Debug.LogWarning("[IAPPurchaseHandler] PendingOrder productId null");
                return;
            }

            // E5 — Receipt 검증 (5031 D1=A: Sprint 2 시점 v5 호환 검증)
            // v5 PendingOrder.Info 의 receipt 접근 미확정 — Sprint 2 또는 5031.1
            // 임시: 검증 생략, ApplyGrant 직접 진행
            IAPProductCatalog.ApplyGrant(productId);
            LimitedShopService.ConfirmIAPPurchase(productId);
            OnPurchaseSucceeded?.Invoke(productId);

            // v5 — 구매 확정 (필수)
            _store?.ConfirmPurchase(order);
        }

        public void HandleFailed(FailedOrder order)
        {
            string productId = ExtractProductId(order);
            string reason = order?.FailureReason.ToString() ?? "Unknown";
            Debug.LogError($"[IAPPurchaseHandler] FAILED — {productId} ({reason})");
            OnPurchaseFailed?.Invoke(productId, reason);
        }

        private static string ExtractProductId(Order order)
        {
            var items = order?.CartOrdered?.Items();
            if (items == null || items.Count == 0) return null;
            return items[0].Product?.definition?.id;
        }
    }
}
