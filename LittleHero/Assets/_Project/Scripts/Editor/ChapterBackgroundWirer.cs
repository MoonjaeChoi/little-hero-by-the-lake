#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5038.5 — 35 챕터 ChapterData.coverArt 에 Kenney Sample.png 자동 wiring.
    /// 카테고리별 매트릭스: CH1~10 Background-Elements / CH11~20 Pirate-Pack / CH21~30 City-Kit-Suburban / CH31~35 Space-Kit.
    /// V5 sticker BG 답습 폐기 — Kenney CC0 Sample 로 즉시 시각 차별화 (사용자 Inspector 후속 정밀 조정).
    /// </summary>
    public static class ChapterBackgroundWirer
    {
        private const string PACKS = "Assets/_ThirdParty/Kenney";
        private const string BG_ELEMENTS  = PACKS + "/Background-Elements/Sample.png";
        private const string PIRATE       = PACKS + "/Pirate-Pack/Sample.png";
        private const string CITY         = PACKS + "/City-Kit-Suburban/Sample.png";
        private const string SPACE        = PACKS + "/Space-Kit/Sample.png";

        [MenuItem("Tools/LittleHero/Wire Chapter Backgrounds — Kenney")]
        public static void WireBackgrounds()
        {
            int wired = 0, missing = 0;
            for (int n = 1; n <= 35; n++)
            {
                string spritePath = PickSpritePath(n);
                if (WireOne(n, spritePath)) wired++;
                else missing++;
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ChapterBackgroundWirer] CH1~35 BG wiring 완료 — wired={wired}, missing={missing}.");
        }

        private static string PickSpritePath(int chapterNum)
        {
            if (chapterNum <= 10) return BG_ELEMENTS;
            if (chapterNum <= 20) return PIRATE;
            if (chapterNum <= 30) return CITY;
            return SPACE;  // 31~35
        }

        private static bool WireOne(int chapterNum, string spritePath)
        {
            var ch = ChapterVariantSeeder.LoadChapterByNumber(chapterNum);
            if (ch == null) { Debug.LogWarning($"[ChapterBackgroundWirer] CH{chapterNum} ChapterData 부재"); return false; }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (sprite == null)
            {
                // PNG 가 Sprite Type 아니면 Importer 자동 갱신
                var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.spritePixelsPerUnit = 100f;
                    importer.SaveAndReimport();
                    sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                }
            }
            if (sprite == null) { Debug.LogWarning($"[ChapterBackgroundWirer] {spritePath} sprite 부재"); return false; }
            ch.coverArt = sprite;
            EditorUtility.SetDirty(ch);
            return true;
        }
    }
}
#endif
