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
        private const string RULE_PATH = "Assets/_Project/Data/Rules/ChapterVariantRule_Default.asset";

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

        /// <summary>5037.1 / 5037.2 — Rule SO 도입 + CH3~25 일괄 wiring.</summary>
        [MenuItem("Tools/LittleHero/Seed Chapter Variants — Rule SO + CH6~25")]
        public static void SeedRuleAndWire6To25()
        {
            VariantSoFactory.EnsureAllVariants();
            VariantSoFactory.EnsureProjectilePrefabs();
            var rule = EnsureRuleSo(25);  // 5037.2 — 끝값 25 (CH3~25 매트릭스)
            ApplyRule(rule);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ChapterVariantSeeder] Rule SO + CH6~25 wiring 완료. entries={rule.entries.Length}");
        }

        /// <summary>5037.3 — Rule SO 끝값 35 (CH3~35 매트릭스 — 35 챕터 완전 적용).</summary>
        [MenuItem("Tools/LittleHero/Seed Chapter Variants — Rule SO + CH6~35 (전체)")]
        public static void SeedRuleAndWire6To35()
        {
            VariantSoFactory.EnsureAllVariants();
            VariantSoFactory.EnsureProjectilePrefabs();
            var rule = EnsureRuleSo(35);
            ApplyRule(rule);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ChapterVariantSeeder] Rule SO + CH6~35 (전체) wiring 완료. entries={rule.entries.Length}");
        }

        /// <summary>5037.4 — Hardcore CH36~50 cascade (Endgame reuse). 룰 매트릭스 그대로 답습.</summary>
        [MenuItem("Tools/LittleHero/Seed Chapter Variants — Rule SO + Hardcore CH36~50")]
        public static void SeedRuleAndWireHardcore()
        {
            VariantSoFactory.EnsureAllVariants();
            VariantSoFactory.EnsureProjectilePrefabs();
            var rule = EnsureRuleSo(50);  // CH3~50 매트릭스 (Hardcore 포함)
            ApplyRule(rule);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ChapterVariantSeeder] Rule SO + Hardcore CH36~50 wiring 완료. entries={rule.entries.Length}");
        }

        private static ChapterVariantRule EnsureRuleSo(int endChapter = 15)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Rules"))
                AssetDatabase.CreateFolder("Assets/_Project/Data", "Rules");
            var rule = AssetDatabase.LoadAssetAtPath<ChapterVariantRule>(RULE_PATH);
            if (rule == null)
            {
                rule = ScriptableObject.CreateInstance<ChapterVariantRule>();
                AssetDatabase.CreateAsset(rule, RULE_PATH);
            }
            // 5037.1 / 5037.2 — CH3~endChapter 룰 매트릭스 (5037 시범 + cascade 확장)
            var entries = new System.Collections.Generic.List<ChapterVariantRule.Entry>();
            for (int n = 3; n <= endChapter; n++)
            {
                int mod = n % 5;
                if (mod == 3) entries.Add(Make(n, "Variant_Stationary_Sentry", ChapterVariantRule.Slot.MiniBossPool));
                if (mod == 4) entries.Add(Make(n, "Variant_Splitter_Bear",     ChapterVariantRule.Slot.MiniBossPool));
                if (mod == 0) entries.Add(Make(n, "Variant_Bomber_Mine",       ChapterVariantRule.Slot.MiniBossPool));
                if (n >= 10)  entries.Add(Make(n, "Variant_Ranged_Archer",     ChapterVariantRule.Slot.EnemyPool));
                if (n >= 15)  entries.Add(Make(n, "Variant_Charger_Bull",      ChapterVariantRule.Slot.EnemyPool));
            }
            rule.entries = entries.ToArray();
            EditorUtility.SetDirty(rule);
            return rule;
        }

        private static ChapterVariantRule.Entry Make(int chapterNum, string variantId, ChapterVariantRule.Slot slot)
            => new ChapterVariantRule.Entry { chapterNumber = chapterNum, variantId = variantId, slot = slot };

        private static void ApplyRule(ChapterVariantRule rule)
        {
            if (rule == null || rule.entries == null) return;
            for (int i = 0; i < rule.entries.Length; i++)
            {
                var e = rule.entries[i];
                WireSlot(e.chapterNumber, e.variantId, e.slot);
            }
        }

        private static void WireSlot(int chapterNum, string variantId, ChapterVariantRule.Slot slot)
        {
            var ch = LoadChapterByNumber(chapterNum);
            if (ch == null) return;
            string path = $"Assets/_Project/Data/Enemies/Variants/{variantId}.asset";
            var variant = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (variant == null) return;
            if (slot == ChapterVariantRule.Slot.MiniBossPool)
            {
                if (ch.miniBossPool != null && Array.IndexOf(ch.miniBossPool, variant) >= 0) return;
                ArrayUtility.Add(ref ch.miniBossPool, variant);
            }
            else
            {
                if (ch.enemyPool != null && Array.IndexOf(ch.enemyPool, variant) >= 0) return;
                ArrayUtility.Add(ref ch.enemyPool, variant);
            }
            EditorUtility.SetDirty(ch);
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
