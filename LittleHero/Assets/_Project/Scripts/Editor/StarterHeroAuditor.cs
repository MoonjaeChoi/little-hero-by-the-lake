#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    public static class StarterHeroAuditor
    {
        private static readonly string[] HeroPaths =
        {
            "Assets/_Project/Data/Characters/Char_HeroWarrior.asset",
            "Assets/_Project/Data/Characters/Char_HeroArcher.asset",
            "Assets/_Project/Data/Characters/Char_HeroMage.asset",
        };

        private const int EXPECT_W = 768;
        private const int EXPECT_H = 1024;
        private const float TR_LO = 0.05f;
        private const float TR_HI = 0.50f;

        [MenuItem("Tools/LittleHero/Audit Starter Hero Sprite Quality")]
        public static void Run()
        {
            int pass = 0, fail = 0;
            foreach (var path in HeroPaths)
            {
                if (AuditOne(path)) pass++; else fail++;
            }
            string msg = $"PASS: {pass}/3\nFAIL: {fail}/3";
            Debug.Log($"[StarterHeroAuditor] {msg}");
            EditorUtility.DisplayDialog("Starter Hero Audit", msg, "OK");
        }

        private static bool AuditOne(string assetPath)
        {
            var data = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
            if (data == null) { Debug.LogError($"[Auditor] {assetPath} 로드 실패"); return false; }
            if (data.portrait == null) { Debug.LogError($"[Auditor] {data.id}: portrait 미wiring"); return false; }
            if (!ValidateId(data, assetPath)) return false;
            if (!ValidateClassName(data)) return false;
            if (!ValidatePng(data, assetPath)) return false;
            Debug.Log($"[Auditor] PASS {data.id} ({data.displayName})");
            return true;
        }

        private static bool ValidateId(CharacterData data, string assetPath)
        {
            string fname = Path.GetFileNameWithoutExtension(assetPath);
            string expected = fname.Replace("Char_Hero", "hero_").ToLower();
            if (data.id != expected) { Debug.LogError($"[Auditor] {assetPath}: id={data.id} 기대 {expected}"); return false; }
            return true;
        }

        private static bool ValidateClassName(CharacterData data)
        {
            if (data.id == "hero_warrior" && data.className != "전사") { Debug.LogError($"[Auditor] {data.id}: className={data.className} 기대 전사"); return false; }
            if (data.id == "hero_archer"  && data.className != "궁수") { Debug.LogError($"[Auditor] {data.id}: className={data.className} 기대 궁수"); return false; }
            if (data.id == "hero_mage"    && data.className != "마법사") { Debug.LogError($"[Auditor] {data.id}: className={data.className} 기대 마법사"); return false; }
            return true;
        }

        private static bool ValidatePng(CharacterData data, string assetPath)
        {
            string pngPath = AssetDatabase.GetAssetPath(data.portrait);
            var importer = (TextureImporter)AssetImporter.GetAtPath(pngPath);
            if (importer == null) { Debug.LogError($"[Auditor] {pngPath}: importer null"); return false; }
            if (importer.spriteImportMode != SpriteImportMode.Single) Debug.LogWarning($"[Auditor] {pngPath}: spriteImportMode != Single");
            if (Mathf.Abs(importer.spritePivot.x - 0.5f) > 0.01f || Mathf.Abs(importer.spritePivot.y - 0.1f) > 0.05f)
                Debug.LogWarning($"[Auditor] {pngPath}: pivot {importer.spritePivot} 기대 (0.5, 0.1)");
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(pngPath);
            if (tex == null) { Debug.LogError($"[Auditor] {pngPath}: Texture 로드 실패"); return false; }
            if (tex.width != EXPECT_W || tex.height != EXPECT_H)
                Debug.LogWarning($"[Auditor] {pngPath}: {tex.width}x{tex.height} 기대 {EXPECT_W}x{EXPECT_H}");
            return true;
        }
    }
}
#endif
