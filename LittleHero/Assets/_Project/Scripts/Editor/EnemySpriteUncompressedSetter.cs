using System.IO;
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5009.3 후속 — SpriteAtlas pack 시 "compressed format" 경고 해결.
    /// Generated_v2/Enemies 폴더 (Chapters/V1/archetype) 의 모든 PNG TextureImporter 를
    /// textureCompression=Uncompressed 로 설정 → SpriteAtlas pack 시 source pixel 손실 0.
    /// 252 PNG 일괄 처리. 메뉴 한 번 클릭 후 SpriteAtlas Pack 재실행 권장.
    /// </summary>
    public static class EnemySpriteUncompressedSetter
    {
        private const string ENEMY_ROOT = "Assets/_Project/Art/Generated_v2/Enemies";

        [MenuItem("Tools/LittleHero/Set Enemy Sprites Uncompressed")]
        public static void SetUncompressed()
        {
            var fullRoot = Path.Combine(Application.dataPath, "_Project/Art/Generated_v2/Enemies");
            if (!Directory.Exists(fullRoot))
            {
                Debug.LogError($"[UncompressedSetter] 폴더 미존재: {fullRoot}");
                return;
            }

            int total = 0, changed = 0, skipped = 0;
            var pngs = Directory.GetFiles(fullRoot, "*.png", SearchOption.AllDirectories);
            foreach (var full in pngs)
            {
                total++;
                var rel = "Assets" + full.Substring(Application.dataPath.Length).Replace('\\', '/');
                var importer = AssetImporter.GetAtPath(rel) as TextureImporter;
                if (importer == null) { skipped++; continue; }
                if (importer.textureCompression == TextureImporterCompression.Uncompressed)
                {
                    skipped++;
                    continue;
                }
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
                changed++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[UncompressedSetter] ✅ 총 {total} PNG / 변경 {changed} / 스킵 {skipped} (이미 Uncompressed). 다음: Tools/LittleHero/Rebuild Sprite Atlases 재실행 권장.");
        }
    }
}
