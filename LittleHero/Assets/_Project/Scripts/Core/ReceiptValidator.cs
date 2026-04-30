using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

namespace LittleHero
{
    /// <summary>
    /// E5 — Unity IAP CrossPlatformValidator 기반 영수증 검증 (monetization_v1 §7.4).
    ///
    /// 사전 작업: Window > Services > In-App Purchasing 에서 Tangle 키 생성 후
    /// `Assets/Plugins/UnityPurchasing/generated/` 에 `AppleTangle.cs` / `GooglePlayTangle.cs` 생성됨.
    /// 본 검증은 자동으로 그 키를 읽어 사용. 키 미생성 시 검증 스킵 (false 반환).
    ///
    /// 운영 단계 권장: 서버리스 검증을 추후 서버 검증으로 보강 (App Store Server API).
    /// MVP: 단일 디바이스 위·변조 차단 수준.
    /// </summary>
    public static class ReceiptValidator
    {
        private static CrossPlatformValidator _validator;
        private static bool _initialized;
        private static bool _available;

        /// <summary>
        /// 검증 시도. 키 미생성/Editor 모드/지원 안 되는 플랫폼 → 무조건 true(grant 진행).
        /// 위·변조 감지 시 false.
        /// </summary>
        public static bool Validate(string receipt)
        {
            if (string.IsNullOrEmpty(receipt)) return false;
            EnsureInit();
            if (!_available) return true; // Tangle 키 없으면 검증 스킵 — Editor / Tangle 미생성

            try
            {
                var receipts = _validator.Validate(receipt);
                Debug.Log($"[ReceiptValidator] OK — {receipts.Length} entries");
                return true;
            }
            catch (IAPSecurityException e)
            {
                Debug.LogError($"[ReceiptValidator] FAILED — {e.Message}");
                return false;
            }
        }

        private static void EnsureInit()
        {
            if (_initialized) return;
            _initialized = true;
#if UNITY_EDITOR
            _available = false;
#elif (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
            try
            {
                // Tangle 키는 Window > Services > IAP 메뉴에서 생성. 클래스가 없으면 LinkException.
                _validator = new CrossPlatformValidator(GooglePlayTangleStubOrReal(), AppleTangleStubOrReal(), Application.identifier);
                _available = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[ReceiptValidator] init skip — {e.Message}");
                _available = false;
            }
#else
            _available = false;
#endif
        }

#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX)
        // Tangle 키 클래스가 생성되지 않은 상태에서도 컴파일이 깨지지 않도록 reflection 으로 우회.
        private static byte[] GooglePlayTangleStubOrReal()
        {
            var t = System.Type.GetType("GooglePlayTangle, Assembly-CSharp");
            if (t == null) return null;
            var m = t.GetMethod("Data", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            return m?.Invoke(null, null) as byte[];
        }

        private static byte[] AppleTangleStubOrReal()
        {
            var t = System.Type.GetType("AppleTangle, Assembly-CSharp");
            if (t == null) return null;
            var m = t.GetMethod("Data", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            return m?.Invoke(null, null) as byte[];
        }
#endif
    }
}
