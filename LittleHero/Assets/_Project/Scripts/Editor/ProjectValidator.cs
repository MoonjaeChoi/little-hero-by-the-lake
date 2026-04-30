using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero.EditorTools
{
    public static class ProjectValidator
    {
        private const string ChapterScenePath = "Assets/_Project/Scenes/Chapter.unity";
        private const string AtticScenePath   = "Assets/_Project/Scenes/Attic.unity";
        private const string DefaultPoolPath  = "Assets/_Project/Data/Shop/ShopPool_Default.asset";

        private enum Severity { Error, Warning }

        private struct Issue
        {
            public Severity Sev; public string Category; public string Msg;
            public Issue(Severity s, string c, string m) { Sev = s; Category = c; Msg = m; }
        }

        [MenuItem("Tools/LittleHero/Validate Project")]
        public static void Run()
        {
            var issues = new List<Issue>();
            ValidateBuildSettings(issues);
            ValidateBalance(issues);
            ValidateShopPool(issues);
            ValidateChapters(issues);
            ValidateCharacters(issues);
            ValidateMemoryPool(issues);
            ValidateActiveScene(issues);
            Print(issues);
        }

        private static void ValidateBuildSettings(List<Issue> issues)
        {
            var scenes = EditorBuildSettings.scenes;
            bool hasAttic = false, hasChapter = false;
            foreach (var s in scenes)
            {
                if (!s.enabled) continue;
                if (s.path == AtticScenePath)  hasAttic = true;
                if (s.path == ChapterScenePath) hasChapter = true;
            }
            if (!hasAttic)   issues.Add(new Issue(Severity.Error, "BuildSettings", $"Attic 씬({AtticScenePath})이 활성 빌드 씬에 없음"));
            if (!hasChapter) issues.Add(new Issue(Severity.Error, "BuildSettings", $"Chapter 씬({ChapterScenePath})이 활성 빌드 씬에 없음"));
        }

        private static void ValidateBalance(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.BalanceConfig");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Error, "BalanceConfig", "BalanceConfig 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var b = AssetDatabase.LoadAssetAtPath<BalanceConfig>(path);
                if (b == null) continue;
                if (b.priceByRarity == null || b.priceByRarity.Length != 5)
                    issues.Add(new Issue(Severity.Error, "BalanceConfig", $"{path}: priceByRarity 길이가 5가 아님"));
                if (b.shopItemCount <= 0)
                    issues.Add(new Issue(Severity.Warning, "BalanceConfig", $"{path}: shopItemCount={b.shopItemCount}"));
            }
        }

        private static void ValidateShopPool(List<Issue> issues)
        {
            var pool = AssetDatabase.LoadAssetAtPath<ShopPool>(DefaultPoolPath);
            if (pool == null) { issues.Add(new Issue(Severity.Error, "ShopPool", $"{DefaultPoolPath} 없음")); return; }
            if (pool.items == null || pool.items.Length == 0)
                issues.Add(new Issue(Severity.Error, "ShopPool", "items 비어있음"));
            for (int i = 0; pool.items != null && i < pool.items.Length; i++)
            {
                var it = pool.items[i];
                if (it == null) { issues.Add(new Issue(Severity.Error, "ShopPool", $"items[{i}] null")); continue; }
                if (string.IsNullOrEmpty(it.displayName)) issues.Add(new Issue(Severity.Warning, "ShopPool", $"{it.name}: displayName 비어있음"));
                if (it.icon == null) issues.Add(new Issue(Severity.Warning, "ShopPool", $"{it.name}: icon 미설정"));
                if (it.attackPattern != AttackPatternType.None && it.attackConfig.projectilePrefab == null)
                    issues.Add(new Issue(Severity.Error, "ShopPool", $"{it.name}: pattern={it.attackPattern}인데 projectilePrefab null"));
            }
        }

        private static void ValidateChapters(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.ChapterData");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Error, "ChapterData", "ChapterData 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(path);
                if (ch == null) continue;
                if (ch.totalWaves < 1) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: totalWaves={ch.totalWaves}"));
                if (ch.enemyPool == null || ch.enemyPool.Length == 0)
                    issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: enemyPool 비어있음"));
                else
                    for (int i = 0; i < ch.enemyPool.Length; i++)
                        if (ch.enemyPool[i] == null) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: enemyPool[{i}] null"));
                        else if (ch.enemyPool[i].prefab == null) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: {ch.enemyPool[i].name} prefab null"));
                if (ch.bossEnemy == null) issues.Add(new Issue(Severity.Warning, "ChapterData", $"{path}: bossEnemy null (마지막 웨이브가 일반 spawn으로 처리됨)"));
                else if (ch.bossEnemy.prefab == null) issues.Add(new Issue(Severity.Error, "ChapterData", $"{path}: boss prefab null"));
            }
        }

        private static void ValidateCharacters(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.CharacterData");
            if (guids.Length == 0) issues.Add(new Issue(Severity.Warning, "CharacterData", "CharacterData 인스턴스가 없음"));
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var c = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (c == null) continue;
                if (string.IsNullOrEmpty(c.id)) issues.Add(new Issue(Severity.Warning, "CharacterData", $"{path}: id 비어있음"));
                if (c.maxHealth <= 0)    issues.Add(new Issue(Severity.Error,   "CharacterData", $"{path}: maxHealth={c.maxHealth}"));
                if (c.moveSpeed <= 0)    issues.Add(new Issue(Severity.Error,   "CharacterData", $"{path}: moveSpeed={c.moveSpeed}"));
                if (c.attackDamage <= 0) issues.Add(new Issue(Severity.Error,   "CharacterData", $"{path}: attackDamage={c.attackDamage}"));
            }
        }

        private static void ValidateMemoryPool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.MemoryPool");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Warning, "MemoryPool", "MemoryPool 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<MemoryPool>(path);
                if (p == null || p.memories == null) continue;
                var seenIds = new HashSet<string>();
                for (int i = 0; i < p.memories.Length; i++)
                {
                    var m = p.memories[i];
                    if (m == null) { issues.Add(new Issue(Severity.Error, "MemoryPool", $"{path}: memories[{i}] null")); continue; }
                    if (string.IsNullOrEmpty(m.id)) issues.Add(new Issue(Severity.Error, "MemoryPool", $"{m.name}: id 비어있음 (저장 키)"));
                    else if (!seenIds.Add(m.id))    issues.Add(new Issue(Severity.Error, "MemoryPool", $"{m.name}: id 중복 '{m.id}'"));
                    if (string.IsNullOrEmpty(m.chapterId)) issues.Add(new Issue(Severity.Warning, "MemoryPool", $"{m.name}: chapterId 비어있음 (어떤 챕터에서도 spawn 안됨)"));
                    if (m.spawnChance <= 0f) issues.Add(new Issue(Severity.Warning, "MemoryPool", $"{m.name}: spawnChance={m.spawnChance}"));
                }
            }
        }

        private static void ValidateActiveScene(List<Issue> issues)
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ChapterScenePath)
            {
                issues.Add(new Issue(Severity.Warning, "Scene", $"활성 씬={scene.path} (Chapter.unity가 아니므로 Chapter 검증 일부 생략)"));
                if (scene.path != AtticScenePath) return;
            }

            if (scene.path == ChapterScenePath)
                ValidateChapterScene(issues);
            else if (scene.path == AtticScenePath)
                ValidateAtticScene(issues);
        }

        private static void ValidateChapterScene(List<Issue> issues)
        {
            ValidateMainCamera(issues);
            ValidateCinemachine(issues);
            ValidatePanelDefaults(issues);
            ValidateRequired<ShopController>(issues, "ShopController");
            ValidateRequired<MetaProgression>(issues, "MetaProgression");
            ValidateRequired<ChapterRunner>(issues, "ChapterRunner");
            ValidateRequired<HUDController>(issues, "HUDController");
            ValidateShopPanelWiring(issues);
            ValidateGameOverPanelWiring(issues);
            ValidateChapterClearedPanelWiring(issues);
        }

        private static void ValidateAtticScene(List<Issue> issues)
        {
            ValidateRequired<MetaProgression>(issues, "MetaProgression");
            ValidateRequired<AtticController>(issues, "AtticController");
            var atticCtl = Object.FindFirstObjectByType<AtticController>();
            if (atticCtl == null) return;

            var so = new SerializedObject(atticCtl);
            CheckRef(issues, "Attic", so, "_starFragmentsText");
            CheckRef(issues, "Attic", so, "_selectedCharacterText");
            CheckRef(issues, "Attic", so, "_defaultCharacter");

            var arr = so.FindProperty("_chapterButtons");
            if (arr == null || !arr.isArray) issues.Add(new Issue(Severity.Error, "Attic", "_chapterButtons 배열 미존재"));
            else
            {
                if (arr.arraySize == 0) issues.Add(new Issue(Severity.Error, "Attic", "_chapterButtons 비어있음"));
                for (int i = 0; i < arr.arraySize; i++)
                {
                    var elem = arr.GetArrayElementAtIndex(i);
                    var ch = elem.FindPropertyRelative("chapter");
                    var bt = elem.FindPropertyRelative("button");
                    if (ch == null || ch.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, "Attic", $"chapterButtons[{i}].chapter 미연결"));
                    if (bt == null || bt.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, "Attic", $"chapterButtons[{i}].button 미연결"));
                }
            }

            var carr = so.FindProperty("_characterButtons");
            if (carr == null || !carr.isArray) issues.Add(new Issue(Severity.Error, "Attic", "_characterButtons 배열 미존재"));
            else
            {
                if (carr.arraySize == 0) issues.Add(new Issue(Severity.Error, "Attic", "_characterButtons 비어있음"));
                for (int i = 0; i < carr.arraySize; i++)
                {
                    var elem = carr.GetArrayElementAtIndex(i);
                    var ch = elem.FindPropertyRelative("character");
                    var bt = elem.FindPropertyRelative("button");
                    if (ch == null || ch.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, "Attic", $"characterButtons[{i}].character 미연결"));
                    if (bt == null || bt.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, "Attic", $"characterButtons[{i}].button 미연결"));
                }
            }
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
            Object[] cms = Object.FindObjectsByType(cmType, FindObjectsSortMode.None);
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

        private static void ValidateRequired<T>(List<Issue> issues, string name) where T : Object
        {
            if (Object.FindFirstObjectByType<T>() == null)
                issues.Add(new Issue(Severity.Error, "Scene", $"{name} 컴포넌트 없음"));
        }

        private static void ValidateShopPanelWiring(List<Issue> issues)
        {
            var sp = Object.FindFirstObjectByType<ShopPanel>();
            if (sp == null) { issues.Add(new Issue(Severity.Error, "ShopPanel", "씬에 ShopPanel 없음")); return; }
            var so = new SerializedObject(sp);
            CheckRef(issues, "ShopPanel", so, "_shopController");
            CheckRef(issues, "ShopPanel", so, "_panelRoot");
            CheckRef(issues, "ShopPanel", so, "_slotsParent");
            CheckRef(issues, "ShopPanel", so, "_slotPrefab");
            CheckRef(issues, "ShopPanel", so, "_refreshButton");
            CheckRef(issues, "ShopPanel", so, "_startButton");
            CheckRef(issues, "ShopPanel", so, "_refreshCostText");
        }

        private static void ValidateGameOverPanelWiring(List<Issue> issues)
        {
            var go = Object.FindFirstObjectByType<GameOverPanel>();
            if (go == null) { issues.Add(new Issue(Severity.Error, "GameOverPanel", "씬에 GameOverPanel 없음")); return; }
            var so = new SerializedObject(go);
            CheckRef(issues, "GameOverPanel", so, "_panelRoot");
            CheckRef(issues, "GameOverPanel", so, "_restartButton");
        }

        private static void ValidateChapterClearedPanelWiring(List<Issue> issues)
        {
            var cc = Object.FindFirstObjectByType<ChapterClearedPanel>();
            if (cc == null) { issues.Add(new Issue(Severity.Error, "ChapterClearedPanel", "씬에 ChapterClearedPanel 없음")); return; }
            var so = new SerializedObject(cc);
            CheckRef(issues, "ChapterClearedPanel", so, "_panelRoot");
            CheckRef(issues, "ChapterClearedPanel", so, "_returnHubButton");
            CheckRef(issues, "ChapterClearedPanel", so, "_titleText");
            CheckRef(issues, "ChapterClearedPanel", so, "_rewardText");
        }

        private static void CheckRef(List<Issue> issues, string cat, SerializedObject so, string fieldName)
        {
            var prop = so.FindProperty(fieldName);
            if (prop == null) { issues.Add(new Issue(Severity.Warning, cat, $"필드 {fieldName} 미존재")); return; }
            if (prop.objectReferenceValue == null) issues.Add(new Issue(Severity.Error, cat, $"{fieldName} 미연결"));
        }

        private static void Print(List<Issue> issues)
        {
            int errors = 0, warnings = 0;
            foreach (var i in issues)
            {
                if (i.Sev == Severity.Error) { errors++; Debug.LogError($"[Validator] [{i.Category}] {i.Msg}"); }
                else { warnings++; Debug.LogWarning($"[Validator] [{i.Category}] {i.Msg}"); }
            }
            string summary = $"[Validator] 완료. errors={errors}, warnings={warnings}";
            if (errors > 0) Debug.LogError(summary); else Debug.Log(summary);
        }
    }
}
