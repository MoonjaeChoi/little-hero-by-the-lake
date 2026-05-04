using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5009.4 후속 — Pack Preview 비정상 (outline 손실 + size 부족) fix.
    /// EnemyAtlas TextureSettings + PlatformSettings 일괄 설정:
    ///   - maxTextureSize 2048 → 8192 (251 PNG 수용)
    ///   - textureCompression Normal → Uncompressed (RGB 손실 0)
    ///   - crunchedCompression off
    /// </summary>
    public static class EnemyAtlasConfigurer
    {
        private const string ATLAS_PATH = "Assets/_Project/Art/Atlases/EnemyAtlas.spriteatlas";

        [MenuItem("Tools/LittleHero/Configure EnemyAtlas High Quality")]
        public static void Configure()
        {
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(ATLAS_PATH);
            if (atlas == null)
            {
                Debug.LogError($"[EnemyAtlasConfigurer] EnemyAtlas 미존재: {ATLAS_PATH}");
                return;
            }

            // 1. TextureSettings (default)
            var ts = atlas.GetTextureSettings();
            ts.anisoLevel = 1;
            ts.filterMode = FilterMode.Bilinear;
            ts.generateMipMaps = false;
            ts.readable = false;
            ts.sRGB = true;
            atlas.SetTextureSettings(ts);

            // 2. Default + iOS + Android + Standalone platform settings
            var platforms = new[] { "DefaultTexturePlatform", "iPhone", "Android", "Standalone" };
            foreach (var platform in platforms)
            {
                var ps = atlas.GetPlatformSettings(platform);
                ps.maxTextureSize = 8192;
                ps.textureCompression = TextureImporterCompression.Uncompressed;
                ps.crunchedCompression = false;
                ps.format = TextureImporterFormat.Automatic;
                if (platform != "DefaultTexturePlatform") ps.overridden = true;
                atlas.SetPlatformSettings(ps);
            }

            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[EnemyAtlasConfigurer] ✅ EnemyAtlas 설정 갱신: maxSize=8192, compression=Uncompressed, crunch=off (Default + iOS + Android + Standalone). 다음: Tools/LittleHero/Rebuild Sprite Atlases 재실행 권장.");
        }
    }
}
