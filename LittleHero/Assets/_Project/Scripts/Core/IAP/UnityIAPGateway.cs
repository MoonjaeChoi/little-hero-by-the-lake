using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Purchasing;

namespace LittleHero
{
    /// <summary>
    /// 5031 — Unity IAP v5 (StoreController) wrapper. IIAPGateway 구현.
    /// PendingOrder → productId + receipt 변환, Mock 친화 인터페이스 노출.
    /// </summary>
    public class UnityIAPGateway : IIAPGateway
    {
        private StoreController _store;
        private bool _ready;

        public bool IsReady => _ready;

        public event Action OnReady;
        public event Action<string, string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public async Task InitializeAsync(List<ProductDefinition> products)
        {
            _store = UnityIAPServices.StoreController();
            _store.OnPurchasePending += HandlePending;
            _store.OnPurchaseFailed  += HandleFailed;
            _store.OnProductsFetched += HandleProductsFetched;
            // v5 권장 콜백 (5031.1)
            _store.OnStoreDisconnected += desc => Debug.LogError($"[UnityIAPGateway] disconnected — {desc?.Reason}");
            _store.OnProductsFetchFailed += desc => Debug.LogError($"[UnityIAPGateway] FetchProducts FAILED");

            try
            {
                await _store.Connect();
                _store.FetchProducts(products);
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityIAPGateway] Connect/FetchProducts FAILED — {e.Message}");
            }
        }

        public void PurchaseProduct(string productId)
        {
            if (_store == null) { Debug.LogError("[UnityIAPGateway] not initialized"); return; }
            _store.PurchaseProduct(productId);
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_store == null) return string.Empty;
            var p = _store.GetProductById(productId);
            return p?.metadata?.localizedPriceString ?? string.Empty;
        }

        private void HandlePending(PendingOrder order)
        {
            string productId = ExtractProductId(order);
            string receipt = order?.Info?.Receipt ?? string.Empty;
            OnPurchaseSucceeded?.Invoke(productId, receipt);
            _store?.ConfirmPurchase(order);
        }

        private void HandleFailed(FailedOrder order)
        {
            string productId = ExtractProductId(order);
            string reason = order?.FailureReason.ToString() ?? "Unknown";
            OnPurchaseFailed?.Invoke(productId, reason);
        }

        private void HandleProductsFetched(List<Product> products)
        {
            _ready = true;
            OnReady?.Invoke();
            Debug.Log($"[UnityIAPGateway] FetchProducts OK — {products?.Count ?? 0} products");
        }

        private static string ExtractProductId(Order order)
        {
            var items = order?.CartOrdered?.Items();
            if (items == null || items.Count == 0) return null;
            return items[0].Product?.definition?.id;
        }
    }
}
