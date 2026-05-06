using System.Collections.Generic;
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5023 — Awake bootstrap 책임 분리. IAP / ItemDrop / StarlightGrant 초기화.
    /// </summary>
    public static class MetaProgressionBootstrapper
    {
        public static void Run()
        {
            // E3 — 별빛 무료 출처 자동 부트스트랩 (일일 로그인 + 이벤트 구독)
            StarlightGrantService.Bootstrap();
            // E1+E2 — IAP 백그라운드 초기화
            try { IAPService.Bootstrap(); }
            catch (System.Exception e) { Debug.LogWarning($"[MetaProgressionBootstrapper] IAP skip: {e.Message}"); }
            // PIVOT v2 — ItemDropService 부트스트랩
            try { BootstrapItemDropService(); }
            catch (System.Exception e) { Debug.LogWarning($"[MetaProgressionBootstrapper] ItemDrop skip: {e.Message}"); }
        }

        private static void BootstrapItemDropService()
        {
            if (ItemDropService.IsInitialized) return;
#if UNITY_EDITOR
            var allGuids = UnityEditor.AssetDatabase.FindAssets("t:EquipmentTemplate", new[] { "Assets/_Project/Data/Templates" });
            var templates = new List<EquipmentTemplate>();
            var allTemplates = new List<EquipmentTemplate>();
            foreach (var g in allGuids)
            {
                var p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                var t = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(p);
                if (t == null) continue;
                allTemplates.Add(t);
                if (p.Contains("/Weapons/")) continue;  // 가챠 한정 풀 제외
                templates.Add(t);
            }
            EquipmentManager.RegisterTemplates(allTemplates);

            var slotPoolGuids = UnityEditor.AssetDatabase.FindAssets("t:SlotAffixPool", new[] { "Assets/_Project/Data/SlotPools" });
            var slotPools = new List<SlotAffixPool>();
            foreach (var g in slotPoolGuids)
                slotPools.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<SlotAffixPool>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)));
            var globalPool = UnityEditor.AssetDatabase.LoadAssetAtPath<GlobalSubAffixPool>("Assets/_Project/Data/GlobalPools/GlobalSubAffixPool_Default.asset");
            var balance = UnityEditor.AssetDatabase.LoadAssetAtPath<BalanceConfig>("Assets/_Project/Data/BalanceConfig.asset");

            if (templates.Count == 0 || slotPools.Count == 0 || globalPool == null)
            {
                Debug.LogWarning("[MetaProgressionBootstrapper] ItemDrop assets 부족 — 부트스트랩 스킵");
                return;
            }
            ItemDropService.Initialize(slotPools.ToArray(), globalPool, templates.ToArray(), balance);
            Debug.Log($"[MetaProgressionBootstrapper] ItemDrop bootstrap: templates={templates.Count} slotPools={slotPools.Count}");
#else
            Debug.LogWarning("[MetaProgressionBootstrapper] ItemDrop bootstrap — 빌드 모드: Inspector SerializeField 후속 필요");
#endif
        }
    }
}
