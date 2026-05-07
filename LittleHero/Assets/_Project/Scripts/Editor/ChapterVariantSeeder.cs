#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5037 — Variant SO 6종 일괄 생성/갱신 + CH3/4/5 시범 wiring 진입점.
    /// chapterNumber 스캔 (D2=C) — 파일명 무관 35 챕터 reuse.
    /// 본체 ~50L. VariantSoFactory 정적 헬퍼 분리 (§15.1 SRP).
    /// </summary>
    public static class ChapterVariantSeeder
    {
        [MenuItem("Tools/LittleHero/Seed Chapter Enemy Variants")]
        public static void SeedAndWire()
        {
            VariantSoFactory.EnsureAllVariants();
            int wired = 0;
            wired += WireChapter(3, "Variant_Stationary_Sentry") ? 1 : 0;
            wired += WireChapter(4, "Variant_Splitter_Bear")     ? 1 : 0;
            wired += WireChapter(5, "Variant_Bomber_Mine")       ? 1 : 0;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ChapterVariantSeeder] 6 Variant SO 갱신 + {wired}/3 챕터 wiring 완료 (CH3/4/5).");
        }

        /// <summary>D2=C — chapterNumber 스캔. 35 챕터 일괄 reuse 헬퍼.</summary>
        public static ChapterData LoadChapterByNumber(int chapterNum)
        {
            string[] guids = AssetDatabase.FindAssets("t:ChapterData");
            for (int i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
                if (ch != null && ch.chapterNumber == chapterNum) return ch;
            }
            return null;
        }

        private static bool WireChapter(int chapterNum, string variantId)
        {
            var ch = LoadChapterByNumber(chapterNum);
            if (ch == null) { Debug.LogWarning($"[ChapterVariantSeeder] CH{chapterNum} ChapterData not found."); return false; }
            string path = $"Assets/_Project/Data/Enemies/Variants/{variantId}.asset";
            var variant = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (variant == null) { Debug.LogWarning($"[ChapterVariantSeeder] Variant '{variantId}' not found at {path}."); return false; }
            if (ch.miniBossPool != null && Array.IndexOf(ch.miniBossPool, variant) >= 0) return true;  // 멱등
            ArrayUtility.Add(ref ch.miniBossPool, variant);
            EditorUtility.SetDirty(ch);
            return true;
        }
    }
}
#endif
