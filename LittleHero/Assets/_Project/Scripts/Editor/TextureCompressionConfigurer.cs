using UnityEditor;
using UnityEngine;

namespace LittleHero.Editor
{
    /// <summary>
    /// 5018 — Texture Compression Policy 일괄 적용.
    /// Generated_v2 의 786 PNG 모두 DefaultPlatform Compressed + Crunched + iOS Override ASTC 6x6 적용.
    /// 목표: Mac 894 MB → ~140 MB (-84%, TestFlight 진입 가능).
    /// </summary>
    public static class TextureCompressionConfigurer
    {
        private static readonly string[] TARGET_FOLDERS =
        {
            "Assets/_Project/Art/Generated_v2",
        };

        [MenuItem("Tools/LittleHero/5018 Apply Texture Compression Policy")]
        public static void ApplyPolicy()
        {
            int processed = 0;
            int skipped = 0;

            try
            {
                AssetDatabase.StartAssetEditing();
                var guids = AssetDatabase.FindAssets("t:Texture2D", TARGET_FOLDERS);
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer == null) { skipped++; continue; }

                    if (ApplyToImporter(importer)) processed++;
                    else skipped++;

                    importer.SaveAndReimport();
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
                AssetDatabase.SaveAssets();
            }

            Debug.Log($"[5018 TextureCompressionConfigurer] Processed: {processed} / Skipped: {skipped}");
        }

        public static bool ApplyToImporter(TextureImporter importer)
        {
            if (importer == null) return false;

            // 1) DefaultPlatform — Compressed + Crunched (Editor + 비명시 platform 모두)
            var def = importer.GetDefaultPlatformTextureSettings();
            def.maxTextureSize = 2048;
            def.textureCompression = TextureImporterCompression.Compressed;
            def.crunchedCompression = true;
            def.compressionQuality = 50;
            importer.SetPlatformTextureSettings(def);

            // 2) iOS Override — ASTC 6x6 + Crunched (App Store 임계 150 MB 회복)
            var ios = importer.GetPlatformTextureSettings("iPhone");
            ios.overridden = true;
            ios.maxTextureSize = 2048;
            ios.format = TextureImporterFormat.ASTC_6x6;
            ios.textureCompression = TextureImporterCompression.Compressed;
            ios.crunchedCompression = true;
            ios.compressionQuality = 50;
            importer.SetPlatformTextureSettings(ios);

            // 3) Android Override — ETC2 / ASTC + Crunched (선택, 후속 플랫폼)
            var android = importer.GetPlatformTextureSettings("Android");
            android.overridden = true;
            android.maxTextureSize = 2048;
            android.format = TextureImporterFormat.ASTC_6x6;
            android.textureCompression = TextureImporterCompression.Compressed;
            android.crunchedCompression = true;
            android.compressionQuality = 50;
            importer.SetPlatformTextureSettings(android);

            return true;
        }
    }
}
