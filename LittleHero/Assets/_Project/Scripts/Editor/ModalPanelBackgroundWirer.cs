#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5038.7 — 모든 modal panel 의 Background Image 에 Kenney button_rectangle_border (Grey) 9-slice 일괄 적용.
    /// Canvas 안의 *Panel GameObject 검색 → Image 컴포넌트 갱신 (sprite + Sliced + color).
    /// 4 scene (Attic / Chapter / InfiniteTower / BossEncounter) 순회.
    /// </summary>
    public static class ModalPanelBackgroundWirer
    {
        private const string SPRITE_PATH = "Assets/_ThirdParty/Kenney/UI-Pack/PNG/Grey/Default/button_rectangle_border.png";
        private static readonly string[] SCENES = {
            "Assets/_Project/Scenes/Attic.unity",
            "Assets/_Project/Scenes/Chapter.unity",
            "Assets/_Project/Scenes/InfiniteTower.unity",
            "Assets/_Project/Scenes/BossEncounter.unity",
        };

        [MenuItem("Tools/LittleHero/Wire Modal Panel Backgrounds — Kenney 9-Slice")]
        public static void WireBackgrounds()
        {
            // 1. sprite Importer 9-slice 설정 (border 16px 기본)
            var sprite = EnsureSlicedSprite();
            if (sprite == null) { Debug.LogError($"[ModalPanelBackgroundWirer] sprite 부재: {SPRITE_PATH}"); return; }

            // 2. 4 scene 순회 + panel BG 갱신
            int totalPanels = 0, scenesProcessed = 0;
            foreach (var scenePath in SCENES)
            {
                if (!System.IO.File.Exists(scenePath)) continue;
                var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int panels = WireSceneBackgrounds(scene, sprite);
                totalPanels += panels;
                EditorSceneManager.SaveScene(scene);
                scenesProcessed++;
                Debug.Log($"[ModalPanelBackgroundWirer] {scene.name} — {panels} panel 갱신");
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ModalPanelBackgroundWirer] 완료 — {scenesProcessed} scene / 총 {totalPanels} panel BG 적용.");
        }

        private static Sprite EnsureSlicedSprite()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(SPRITE_PATH);
            if (importer == null) return null;
            // Sprite Single + 9-slice border (16px 기본)
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = new Vector4(16, 16, 16, 16);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(SPRITE_PATH);
        }

        private static int WireSceneBackgrounds(Scene scene, Sprite sprite)
        {
            int count = 0;
            foreach (var root in scene.GetRootGameObjects())
            {
                var panels = new List<GameObject>();
                CollectPanels(root.transform, panels);
                foreach (var panel in panels)
                {
                    var image = panel.GetComponent<Image>();
                    if (image == null) continue;
                    image.sprite = sprite;
                    image.type = Image.Type.Sliced;
                    image.color = new Color(1f, 1f, 1f, 0.95f);
                    EditorUtility.SetDirty(panel);
                    count++;
                }
            }
            return count;
        }

        private static void CollectPanels(Transform t, List<GameObject> result)
        {
            // Panel GameObject — name contains "Panel" + active (CharacterPanel, GachaPanel 등 30+)
            if (t.name.Contains("Panel") && t.gameObject.activeInHierarchy == false)  // inactive modal
                result.Add(t.gameObject);
            else if (t.name.Contains("Modal"))
                result.Add(t.gameObject);
            for (int i = 0; i < t.childCount; i++)
                CollectPanels(t.GetChild(i), result);
        }
    }
}
#endif
