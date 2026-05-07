#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5034 — BalanceConfig 권장 진행 매트릭스 시드 (D1=A 균형 곡선).
    /// 챕터 N → 권장 Lv N+9 / 무기 +N×0.7 / 슬롯 ⭐ N×0.6 (반올림).
    /// </summary>
    public static class SeedBalanceMatrix
    {
        private const string MENU_PATH = "Tools/LittleHero/Seed BalanceConfig Recommended Matrix";
        private const string ASSET_PATH = "Assets/_Project/Data/BalanceConfig.asset";

        [MenuItem(MENU_PATH)]
        public static void Seed()
        {
            var cfg = AssetDatabase.LoadAssetAtPath<BalanceConfig>(ASSET_PATH);
            if (cfg == null) { Debug.LogError($"[SeedBalanceMatrix] {ASSET_PATH} 미존재"); return; }

            int chapters = 35;
            cfg.recommendedLevelByChapter = new int[chapters];
            cfg.recommendedWeaponEnhanceByChapter = new int[chapters];
            cfg.recommendedSlotStarforceByChapter = new int[chapters];

            for (int n = 1; n <= chapters; n++)
            {
                int idx = n - 1;
                cfg.recommendedLevelByChapter[idx]          = Mathf.Clamp(n + 9, 1, 50);            // L10~L44 → cap 50
                cfg.recommendedWeaponEnhanceByChapter[idx]  = Mathf.Clamp(Mathf.RoundToInt(n * 0.7f), 0, 25);
                cfg.recommendedSlotStarforceByChapter[idx]  = Mathf.Clamp(Mathf.RoundToInt(n * 0.6f), 0, 20);
            }

            EditorUtility.SetDirty(cfg);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SeedBalanceMatrix] 35 챕터 매트릭스 시드 완료 — Lv[0]={cfg.recommendedLevelByChapter[0]} Lv[34]={cfg.recommendedLevelByChapter[34]} ⭐[34]={cfg.recommendedSlotStarforceByChapter[34]}");
        }
    }
}
#endif
