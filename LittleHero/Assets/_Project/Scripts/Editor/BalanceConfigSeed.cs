#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// B9 — BalanceConfig.enhanceCostByLevel / enhanceCrystalByLevel 곡선을 시드.
    /// 메뉴: Tools > LittleHero > Seed BalanceConfig Curves.
    /// 멱등 — 기존 BalanceConfig SO 의 해당 필드만 덮어씀, 나머지(baseHp 등)는 보존.
    /// </summary>
    public static class BalanceConfigSeed
    {
        [MenuItem("Tools/LittleHero/Seed BalanceConfig Curves")]
        public static void Seed()
        {
            var guids = AssetDatabase.FindAssets("t:BalanceConfig");
            if (guids.Length == 0) { Debug.LogError("[BalanceConfigSeed] BalanceConfig SO 미발견"); return; }

            // economy §4.1 별가루 곡선 (+0→+1 ~ +24→+25)
            int[] enhance = {
                5, 10, 15, 20, 25,
                30, 40, 55, 70, 90,
                110, 140, 170, 200, 250,
                350, 500, 700, 1000, 1200,
                1500, 1900, 2400, 3000, 4000,
            };

            // economy §7.3 마법 결정 곡선 — +15→+16 부터
            int[] crystal = {
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                3, 4, 6, 8, 12,
                15, 20, 25, 32, 40,
            };

            int updated = 0;
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var so = AssetDatabase.LoadAssetAtPath<BalanceConfig>(path);
                if (so == null) continue;
                so.enhanceCostByLevel = (int[])enhance.Clone();
                so.enhanceCrystalByLevel = (int[])crystal.Clone();
                EditorUtility.SetDirty(so);
                updated++;
                Debug.Log($"[BalanceConfigSeed] 갱신: {path}");
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[BalanceConfigSeed] {updated} 개 갱신 완료");
        }
    }
}
#endif
