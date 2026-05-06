using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LittleHero.EditorTools.Rules
{
    /// <summary>
    /// 활성 Scene + SceneManagerDirectAccess (5004) + Camera/Cinemachine + Panel wiring 통합.
    /// </summary>
    public static class SceneRule
    {
        private const string ChapterScenePath = "Assets/_Project/Scenes/Chapter.unity";
        private const string AtticScenePath   = "Assets/_Project/Scenes/Attic.unity";

        public static void Validate(List<Issue> issues)
        {
            ValidateActiveScene(issues);
            ValidateSceneManagerDirectAccess(issues);
        }

        private static void ValidateActiveScene(List<Issue> issues)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ChapterScenePath)
            {
                issues.Add(new Issue(Severity.Warning, "Scene", $"활성 씬={scene.path} (Chapter.unity가 아니므로 Chapter 검증 일부 생략)"));
                if (scene.path != AtticScenePath) return;
            }

            if (scene.path == ChapterScenePath) ValidateChapterScene(issues);
            else if (scene.path == AtticScenePath) ValidateAtticScene(issues);
        }

        private static void ValidateChapterScene(List<Issue> issues)
        {
            ValidateMainCamera(issues);
            ValidateCinemachine(issues);
            ValidatePanelDefaults(issues);
            ValidatorHelpers.ValidateRequired<ShopController>(issues, "ShopController");
            ValidatorHelpers.ValidateRequired<MetaProgression>(issues, "MetaProgression");
            ValidatorHelpers.ValidateRequired<ChapterRunner>(issues, "ChapterRunner");
            ValidatorHelpers.ValidateRequired<HUDController>(issues, "HUDController");
            ValidateShopPanelWiring(issues);
            ValidateGameOverPanelWiring(issues);
            ValidateChapterClearedPanelWiring(issues);
        }

        private static void ValidateAtticScene(List<Issue> issues)
        {
            ValidatorHelpers.ValidateRequired<MetaProgression>(issues, "MetaProgression");
            ValidatorHelpers.ValidateRequired<AtticController>(issues, "AtticController");

            // 5005 attic_decomposition_v1 분리 후 — AtticCharacterSelector._buttons + WondrousMapPanel 검증 (5021 D1=B)
            var selector = Object.FindAnyObjectByType<AtticCharacterSelector>();
            if (selector == null) issues.Add(new Issue(Severity.Error, "Attic", "AtticCharacterSelector 미존재 (5005 분리)"));
            else
            {
                var ssel = new SerializedObject(selector);
                var sarr = ssel.FindProperty("_buttons");
                if (sarr == null || !sarr.isArray) issues.Add(new Issue(Severity.Error, "Attic", "AtticCharacterSelector._buttons 배열 미존재"));
                else if (sarr.arraySize == 0) issues.Add(new Issue(Severity.Error, "Attic", "AtticCharacterSelector._buttons 비어있음 (5020.2 회귀)"));
                else
                {
                    for (int i = 0; i < sarr.arraySize; i++)
                    {
                        var elem = sarr.GetArrayElementAtIndex(i);
                        var ch = elem.FindPropertyRelative("character");
                        var bt = elem.FindPropertyRelative("button");
                        if (ch == null || ch.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, "Attic", $"AtticCharacterSelector._buttons[{i}].character 미연결"));
                        if (bt == null || bt.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, "Attic", $"AtticCharacterSelector._buttons[{i}].button 미연결"));
                    }
                }
            }

            var mapPanel = Object.FindAnyObjectByType<WondrousMapPanel>();
            if (mapPanel == null) issues.Add(new Issue(Severity.Warning, "Attic", "WondrousMapPanel 미존재 (production 챕터 진입 path)"));
        }

        private static void ValidateMainCamera(List<Issue> issues)
        {
            var cam = Camera.main;
            if (cam == null) { issues.Add(new Issue(Severity.Error, "Camera", "MainCamera 태그 카메라 없음")); return; }
            if (!cam.orthographic) issues.Add(new Issue(Severity.Warning, "Camera", "orthographic=false (2D 게임에서 perspective는 비표준)"));
            if (cam.orthographicSize < 0.1f) issues.Add(new Issue(Severity.Error, "Camera", $"orthographicSize={cam.orthographicSize}"));
            if (cam.nearClipPlane > 1f || cam.nearClipPlane < 0.01f)
                issues.Add(new Issue(Severity.Warning, "Camera", $"nearClipPlane={cam.nearClipPlane}"));
            if (cam.farClipPlane < 1f)
                issues.Add(new Issue(Severity.Error, "Camera", $"farClipPlane={cam.farClipPlane} — 거의 아무것도 렌더되지 않음"));
            if (cam.backgroundColor.a < 0.5f && cam.clearFlags == CameraClearFlags.SolidColor)
                issues.Add(new Issue(Severity.Error, "Camera", $"backgroundColor.a={cam.backgroundColor.a} (SolidColor + alpha<0.5는 화면 흰색 가능)"));
        }

        private static void ValidateCinemachine(List<Issue> issues)
        {
            var cmType = System.Type.GetType("Unity.Cinemachine.CinemachineCamera, Unity.Cinemachine");
            if (cmType == null) return;
            Object[] cms = Object.FindObjectsByType(cmType, FindObjectsInactive.Exclude);
            if (cms.Length == 0) { issues.Add(new Issue(Severity.Warning, "Cinemachine", "CinemachineCamera 없음")); return; }

            foreach (var cm in cms)
            {
                var so = new SerializedObject(cm);
                var lensProp = so.FindProperty("Lens");
                if (lensProp != null)
                {
                    var nearProp = lensProp.FindPropertyRelative("NearClipPlane");
                    var farProp  = lensProp.FindPropertyRelative("FarClipPlane");
                    if (nearProp != null && nearProp.floatValue < 0.1f)
                        issues.Add(new Issue(Severity.Error, "Cinemachine", $"{cm.name}: Lens.NearClipPlane={nearProp.floatValue} (Main Camera에 전파되어 화면 흰색 유발)"));
                    if (farProp != null && farProp.floatValue < 1f)
                        issues.Add(new Issue(Severity.Error, "Cinemachine", $"{cm.name}: Lens.FarClipPlane={farProp.floatValue} (렌더 범위 거의 0)"));
                }
            }
        }

        private static void ValidatePanelDefaults(List<Issue> issues)
        {
            string[] panelPaths = { "Canvas/ShopPanel/PanelRoot", "Canvas/GameOverPanel/PanelRoot", "Canvas/ChapterClearedPanel/PanelRoot" };
            foreach (var p in panelPaths)
            {
                var go = GameObject.Find(p);
                if (go == null) issues.Add(new Issue(Severity.Error, "Panel", $"{p} 없음"));
                else if (go.activeSelf) issues.Add(new Issue(Severity.Warning, "Panel", $"{p} default 활성 — 시작 시 화면 가림 가능"));
            }
        }

        private static void ValidateShopPanelWiring(List<Issue> issues)
        {
            var sp = Object.FindAnyObjectByType<ShopPanel>();
            if (sp == null) { issues.Add(new Issue(Severity.Error, "ShopPanel", "씬에 ShopPanel 없음")); return; }
            var so = new SerializedObject(sp);
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_shopController");
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_panelRoot");
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_slotsParent");
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_slotPrefab");
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_refreshButton");
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_startButton");
            ValidatorHelpers.CheckRef(issues, "ShopPanel", so, "_refreshCostText");
        }

        private static void ValidateGameOverPanelWiring(List<Issue> issues)
        {
            var go = Object.FindAnyObjectByType<GameOverPanel>();
            if (go == null) { issues.Add(new Issue(Severity.Error, "GameOverPanel", "씬에 GameOverPanel 없음")); return; }
            var so = new SerializedObject(go);
            ValidatorHelpers.CheckRef(issues, "GameOverPanel", so, "_panelRoot");
            ValidatorHelpers.CheckRef(issues, "GameOverPanel", so, "_restartButton");
        }

        private static void ValidateChapterClearedPanelWiring(List<Issue> issues)
        {
            var cc = Object.FindAnyObjectByType<ChapterClearedPanel>();
            if (cc == null) { issues.Add(new Issue(Severity.Error, "ChapterClearedPanel", "씬에 ChapterClearedPanel 없음")); return; }
            var so = new SerializedObject(cc);
            ValidatorHelpers.CheckRef(issues, "ChapterClearedPanel", so, "_panelRoot");
            ValidatorHelpers.CheckRef(issues, "ChapterClearedPanel", so, "_returnHubButton");
            ValidatorHelpers.CheckRef(issues, "ChapterClearedPanel", so, "_titleText");
            ValidatorHelpers.CheckRef(issues, "ChapterClearedPanel", so, "_rewardText");
        }

        // 5004 T06 — SceneManager.LoadScene 외부 직접 호출 검사 (§15.7 DI).
        private static void ValidateSceneManagerDirectAccess(List<Issue> issues)
        {
            var allowedFiles = new HashSet<string>
            {
                "SceneTransitionService.cs",
                "ProjectValidator.cs",
                "SceneRule.cs",  // 5022: 룰 정의 파일 자체
            };
            string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
            if (!Directory.Exists(scriptDir)) return;

            int violations = 0;
            foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
            {
                string filename = Path.GetFileName(file);
                if (allowedFiles.Contains(filename)) continue;
                string content = File.ReadAllText(file);
                if (content.Contains("SceneManager.LoadScene("))
                {
                    issues.Add(new Issue(Severity.Warning, "5004 §15.7",
                        $"SceneManager.LoadScene 외부 직접 호출: {filename} — SceneTransitionService.Transition 권장"));
                    violations++;
                }
            }
            if (violations == 0)
                Debug.Log("[ProjectValidator] 5004 §15.7 OK — SceneManager.LoadScene 외부 직접 호출 0건");
        }
    }
}
