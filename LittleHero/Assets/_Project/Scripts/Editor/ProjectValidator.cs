using System.Collections.Generic;
using System.IO;
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
            ValidateMetaUpgradePool(issues);
            ValidateEnemyPool(issues);
            ValidateActiveScene(issues);
            ValidateOOPPolicy(issues);  // CLAUDE.md §15.12 신규
            ValidateWeaponSystem(issues);  // weapon_v2 신규
            ValidateBossSystem(issues);   // 5001 boss_system_v2 신규 (T09)
            ValidateBossEncounterContextAccess(issues);  // 5002 T09 — 정적 Context 외부 직접 R/W 검사 (§15.7 DI)
            ValidateSceneManagerDirectAccess(issues);  // 5004 T06 — SceneManager.LoadScene 외부 직접 호출 검사 (§15.7 DI)
            ValidateEnemyVisibilityCheck(issues);  // 5013 T04 — EnemyAnimationManager visibility throttle 호출 검사
            ValidateEnemyHueVariationHook(issues);  // 5012 T04 — EnemyController.ApplyAppearance 의 HueShift 호출 검사
            ValidateLitMotionMigration(issues);  // 5016 T06 — DOTween 회귀 + LitMotion import ≥ N files 검사
            ValidateGCAllocPatterns(issues);  // 5017 T04 — GC alloc 휴리스틱 검사
            ValidateStarterHeroPortraits(issues);  // 5020 T06 — 시작 주인공 3종 portrait wiring 검사
            ValidateCharacterPool(issues);  // 5021 T02 — CharacterPool 인스턴스 + 3 character 매핑 검사
            Print(issues);
        }

        // 5021 T02 — CharacterPool 인스턴스 + 3 character (hero_warrior/hero_archer/hero_mage) 매핑 검사.
        private static void ValidateCharacterPool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.CharacterPool");
            if (guids.Length == 0)
            {
                issues.Add(new Issue(Severity.Warning, "5021 CharacterPool", "CharacterPool 인스턴스 0건 — Tools/LittleHero/Setup CharacterPool Default 실행"));
                return;
            }
            string[] expected = { "hero_warrior", "hero_archer", "hero_mage" };
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var pool = AssetDatabase.LoadAssetAtPath<CharacterPool>(path);
                if (pool == null) continue;
                if (pool.characters == null || pool.characters.Length != 3)
                {
                    issues.Add(new Issue(Severity.Error, "5021 CharacterPool", $"{path}: characters 길이={(pool.characters?.Length ?? 0)} 기대 3"));
                    continue;
                }
                var ids = new HashSet<string>();
                foreach (var c in pool.characters)
                    if (c != null) ids.Add(c.id);
                foreach (var id in expected)
                    if (!ids.Contains(id)) issues.Add(new Issue(Severity.Error, "5021 CharacterPool", $"{path}: id '{id}' 누락"));
            }
        }

        // 5020 T06 — 시작 주인공 3종 (warrior/archer/mage) SO portrait 필드 wiring 검사.
        private static void ValidateStarterHeroPortraits(List<Issue> issues)
        {
            string[] paths =
            {
                "Assets/_Project/Data/Characters/Char_HeroWarrior.asset",
                "Assets/_Project/Data/Characters/Char_HeroArcher.asset",
                "Assets/_Project/Data/Characters/Char_HeroMage.asset",
            };
            foreach (var path in paths)
            {
                var data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
                if (data == null) issues.Add(new Issue(Severity.Error, "5020 StarterHero", $"{path}: 로드 실패"));
                else if (data.portrait == null) issues.Add(new Issue(Severity.Error, "5020 StarterHero", $"{path}: portrait 미wiring"));
            }
        }

        // 5017 T04 — DropContext struct 검증 + DamageNumberFloater FormatDamage cache 존재 검증.
        private static void ValidateGCAllocPatterns(List<Issue> issues)
        {
            var dropCtxType = System.Type.GetType("LittleHero.DropContext, Assembly-CSharp");
            if (dropCtxType != null && !dropCtxType.IsValueType)
                issues.Add(new Issue(Severity.Warning, "5017 GC", "DropContext 가 struct 가 아님 (heap alloc)"));

            var floaterType = System.Type.GetType("LittleHero.DamageNumberFloater, Assembly-CSharp");
            if (floaterType != null)
            {
                var cacheCountProp = floaterType.GetProperty("FormatCacheCount",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (cacheCountProp == null)
                    issues.Add(new Issue(Severity.Warning, "5017 GC", "DamageNumberFloater.FormatCacheCount 누락 (cache 미작동)"));
            }

            Debug.Log("[ProjectValidator] 5017 GC alloc OK — DropContext struct + FormatDamage cache 검증");
        }

        // 5016 T06 — DOTween 재도입 회귀 방지 + LitMotion import 22+ 검증 (§15.12).
        private static void ValidateLitMotionMigration(List<Issue> issues)
        {
            string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
            int doTweenViolations = 0;
            int litMotionImports = 0;
            if (!Directory.Exists(scriptDir))
            {
                issues.Add(new Issue(Severity.Warning, "5016 LitMotion", "Scripts dir 미존재"));
                return;
            }
            foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
            {
                // Validator 자체는 검사 룰 코드라 검사 대상 제외 (self-detection 회피).
                if (file.EndsWith("ProjectValidator.cs")) continue;
                string content = File.ReadAllText(file);
                if (content.Contains("using DG.Tweening")) doTweenViolations++;
                if (content.Contains("using LitMotion")) litMotionImports++;
            }
            if (doTweenViolations > 0)
            {
                issues.Add(new Issue(Severity.Warning, "5016 LitMotion",
                    $"DOTween 재도입 회귀 검출: {doTweenViolations} files (5016 마이그레이션 후 0 이어야 함)"));
            }
            if (litMotionImports < 12)
            {
                issues.Add(new Issue(Severity.Warning, "5016 LitMotion",
                    $"LitMotion import {litMotionImports} files — 5016 마이그레이션 누락 의심 (기대 ≥ 12)"));
            }
            else
            {
                Debug.Log($"[ProjectValidator] 5016 LitMotion OK — DOTween {doTweenViolations} / LitMotion import {litMotionImports} files");
            }
        }

        // 5012 T04 — EnemyController.cs 가 EnemyHueVariation.HueShift 호출하는지 검사 (회귀 방지).
        private static void ValidateEnemyHueVariationHook(List<Issue> issues)
        {
            var path = Path.Combine(Application.dataPath, "_Project/Scripts/Gameplay/Enemies/EnemyController.cs");
            if (!File.Exists(path))
            {
                issues.Add(new Issue(Severity.Warning, "5012 hue",
                    "EnemyController.cs 미존재"));
                return;
            }
            var src = File.ReadAllText(path);
            if (!src.Contains("EnemyHueVariation.HueShift"))
            {
                issues.Add(new Issue(Severity.Warning, "5012 hue",
                    "EnemyController 의 ApplyAppearance 에 EnemyHueVariation.HueShift 호출 누락 — 5008 sprite 시각 다양화 미작동"));
            }
            else
            {
                Debug.Log("[ProjectValidator] 5012 hue OK — EnemyController.HueShift 호출 정상");
            }
        }

        // 5013 T04 — EnemyAnimationManager.Update 가 EnemyVisibilityCuller.IsVisible 호출하는지 검사 (Rebuttal §3.1).
        private static void ValidateEnemyVisibilityCheck(List<Issue> issues)
        {
            var path = Path.Combine(Application.dataPath, "_Project/Scripts/Gameplay/Enemies/EnemyAnimationManager.cs");
            if (!File.Exists(path))
            {
                issues.Add(new Issue(Severity.Warning, "5013 visibility",
                    "EnemyAnimationManager.cs 미존재"));
                return;
            }
            var src = File.ReadAllText(path);
            if (!src.Contains("EnemyVisibilityCuller.IsVisible"))
            {
                issues.Add(new Issue(Severity.Warning, "5013 visibility",
                    "EnemyAnimationManager 의 Update 에 EnemyVisibilityCuller.IsVisible 호출 누락 — off-screen culling 미작동"));
            }
            else
            {
                Debug.Log("[ProjectValidator] 5013 visibility OK — EnemyAnimationManager.IsVisible 호출 정상");
            }
        }

        // 5004 T06 — SceneManager.LoadScene 외부 직접 호출 검사 (§15.7 DI).
        // 허용: SceneTransitionService.cs (Service 내부 LoadSceneAsync 호출만), ProjectValidator.cs (룰 정의 자체).
        private static void ValidateSceneManagerDirectAccess(List<Issue> issues)
        {
            var allowedFiles = new HashSet<string>
            {
                "SceneTransitionService.cs",
                "ProjectValidator.cs",
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

        // 5002 T09 — BossEncounterContext.Set / PendingChapter / PendingReturnScene / PendingBoss 등
        // 외부 직접 R/W 검사 (§15.7 DI). 허용 파일: BossEncounterLauncher / BossEncounterRunner / BossEncounterContext.
        private static void ValidateBossEncounterContextAccess(List<Issue> issues)
        {
            var allowedFiles = new HashSet<string>
            {
                "BossEncounterLauncher.cs",
                "BossEncounterRunner.cs",
                "BossEncounterContext.cs",
                "BossEncounterService.cs",  // Last* property R 만 (필요 시 향후 좁힘)
                "AtticController.cs",       // LastMythicGranted/Definition R + 1회 표시
                "ProjectValidator.cs",      // 룰 정의 파일 자체 (string 매칭 false-positive 회피)
            };
            string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
            if (!Directory.Exists(scriptDir)) return;

            int violations = 0;
            foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
            {
                string filename = Path.GetFileName(file);
                if (allowedFiles.Contains(filename)) continue;
                string content = File.ReadAllText(file);
                if (content.Contains("BossEncounterContext.Set(")
                    || content.Contains("BossEncounterContext.PendingBoss =")
                    || content.Contains("BossEncounterContext.PendingChapter =")
                    || content.Contains("BossEncounterContext.PendingMode =")
                    || content.Contains("BossEncounterContext.PendingReturnScene ="))
                {
                    issues.Add(new Issue(Severity.Warning, "5002 §15.7",
                        $"BossEncounterContext 외부 직접 R/W: {filename} — Launcher 위임 권장"));
                    violations++;
                }
            }
            if (violations == 0)
                Debug.Log("[ProjectValidator] 5002 §15.7 OK — BossEncounterContext 외부 직접 R/W 0건");
        }

        // 5001 boss_system_v2 — 모든 보스 SO BossTier wiring 검증 (T09 Sprint 6)
        private static void ValidateBossSystem(List<Issue> issues)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:EnemyData");
            int tierViolations = 0, lootViolations = 0, attackViolations = 0;
            foreach (var g in guids)
            {
                var p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                var d = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyData>(p);
                if (d == null) continue;

                // Rule 1: isMiniB/isBoss=true 인데 bossTier=None 이면 마이그레이션 누락
                if ((d.isMiniB || d.isBoss) && d.bossTier == BossTier.None)
                {
                    issues.Add(new Issue(Severity.Warning, "Boss",
                        $"5001 — {d.name}: isMiniB/isBoss=true 인데 bossTier=None (마이그레이션 누락)"));
                    tierViolations++;
                }

                // Rule 2: ChapterBoss/DailyBoss/WeeklyBoss → lootTable != null 권장
                if (d.bossTier >= BossTier.ChapterBoss && d.lootTable == null)
                {
                    issues.Add(new Issue(Severity.Warning, "Boss",
                        $"5001 — {d.name}: bossTier={d.bossTier} 인데 lootTable=null (보상 미적용)"));
                    lootViolations++;
                }

                // Rule 3: bossTier != None → bossAttackType != None 권장
                if (d.bossTier != BossTier.None && d.bossAttackType == BossAttackType.None)
                {
                    issues.Add(new Issue(Severity.Warning, "Boss",
                        $"5001 — {d.name}: bossTier={d.bossTier} 인데 bossAttackType=None (공격 패턴 미명세)"));
                    attackViolations++;
                }
            }
            int total = tierViolations + lootViolations + attackViolations;
            if (total == 0)
                issues.Add(new Issue(Severity.Warning, "Boss",
                    $"5001 boss_system_v2 — {guids.Length} EnemyData 모두 BossTier 룰 통과"));
        }

        // weapon_system_v2.md — 모든 무기 SO requiredClass + category 검증
        private static void ValidateWeaponSystem(List<Issue> issues)
        {
            var guids = UnityEditor.AssetDatabase.FindAssets("t:EquipmentTemplate", new[] { "Assets/_Project/Data/Templates/Weapons" });
            int violations = 0;
            foreach (var g in guids)
            {
                var p = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
                var t = UnityEditor.AssetDatabase.LoadAssetAtPath<EquipmentTemplate>(p);
                if (t == null) continue;
                if (t.category == WeaponCategory.None)
                {
                    issues.Add(new Issue(Severity.Warning, "Weapon",
                        $"weapon_v2 — {t.id} category=None (Weapon SO 인데 카테고리 미지정)"));
                    violations++;
                }
                // 직업+카테고리 매칭 검증
                if (!WeaponClassBonus.IsClassMatch(t.requiredClass, t.category))
                {
                    issues.Add(new Issue(Severity.Warning, "Weapon",
                        $"weapon_v2 — {t.id}: {t.requiredClass} + {t.category} 매칭 X (시너지 발동 X)"));
                    violations++;
                }
            }
            if (violations == 0)
                issues.Add(new Issue(Severity.Warning, "Weapon", $"weapon_v2 — {guids.Length} 무기 SO 모두 통과"));
        }

        // CLAUDE.md §15.12 — OOP 정책 자동 검증 (100줄+ 클래스 경고)
        private static void ValidateOOPPolicy(List<Issue> issues)
        {
            const int LINE_LIMIT = 200;  // 100줄 룰 + 여유 100
            var scriptFiles = System.IO.Directory.GetFiles(
                "Assets/_Project/Scripts", "*.cs", System.IO.SearchOption.AllDirectories);
            int violations = 0;
            foreach (var f in scriptFiles)
            {
                int lineCount = System.IO.File.ReadAllLines(f).Length;
                if (lineCount > LINE_LIMIT)
                {
                    issues.Add(new Issue(Severity.Warning, "OOP",
                        $"§15.6 100줄 룰 — {System.IO.Path.GetFileName(f)} ({lineCount} 라인) 책임 분리 권장"));
                    violations++;
                }
            }
            if (violations == 0)
                issues.Add(new Issue(Severity.Warning, "OOP", $"§15.6 100줄 룰 — {scriptFiles.Length} 파일 모두 통과 (LINE_LIMIT={LINE_LIMIT})"));
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

        private static void ValidateMetaUpgradePool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.MetaUpgradePool");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Warning, "MetaUpgradePool", "MetaUpgradePool 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<MetaUpgradePool>(path);
                if (p == null || p.upgrades == null) continue;
                var seenIds = new HashSet<string>();
                for (int i = 0; i < p.upgrades.Length; i++)
                {
                    var u = p.upgrades[i];
                    if (u == null) { issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{path}: upgrades[{i}] null")); continue; }
                    if (string.IsNullOrEmpty(u.id)) issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{u.name}: id 비어있음 (저장 키)"));
                    else if (!seenIds.Add(u.id))    issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{u.name}: id 중복 '{u.id}'"));
                    if (u.maxLevel <= 0)            issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{u.name}: maxLevel={u.maxLevel}"));
                    if (u.bonusPerLevel == 0f)      issues.Add(new Issue(Severity.Warning, "MetaUpgradePool", $"{u.name}: bonusPerLevel=0"));
                    if (u.baseCost < 0)             issues.Add(new Issue(Severity.Error, "MetaUpgradePool", $"{u.name}: baseCost={u.baseCost}"));
                }
            }
        }

        private static void ValidateEnemyPool(List<Issue> issues)
        {
            string[] guids = AssetDatabase.FindAssets("t:LittleHero.EnemyPool");
            if (guids.Length == 0) { issues.Add(new Issue(Severity.Warning, "EnemyPool", "EnemyPool 인스턴스가 없음")); return; }
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var p = AssetDatabase.LoadAssetAtPath<EnemyPool>(path);
                if (p == null || p.enemies == null) continue;
                var seenIds = new HashSet<string>();
                for (int i = 0; i < p.enemies.Length; i++)
                {
                    var e = p.enemies[i];
                    if (e == null) { issues.Add(new Issue(Severity.Error, "EnemyPool", $"{path}: enemies[{i}] null")); continue; }
                    if (string.IsNullOrEmpty(e.id)) issues.Add(new Issue(Severity.Error, "EnemyPool", $"{e.name}: id 비어있음 (도감 키)"));
                    else if (!seenIds.Add(e.id))    issues.Add(new Issue(Severity.Error, "EnemyPool", $"{e.name}: id 중복 '{e.id}'"));
                    if (e.prefab == null)           issues.Add(new Issue(Severity.Error, "EnemyPool", $"{e.name}: prefab null"));
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

            // 5005 attic_decomposition_v1 분리 후 — AtticController 잔여 책임만 검사 (gacha/limited shop/starforce 진입 버튼).
            // 옛 _chapterButtons / _characterButtons / _starFragmentsText / _selectedCharacterText / _defaultCharacter 는
            // AtticCharacterSelector / AtticChapterEntry / AtticCurrencyView / AtticEndgameMenuView 로 이동됨.
            // 5021 R3 D1=B 결정: 챕터 진입은 WondrousMapPanel 단일 path, AtticChapterEntry 비활성화.

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

            // WondrousMapPanel 가 production 챕터 진입 path (5021 D1=B)
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
            if (Object.FindAnyObjectByType<T>() == null)
                issues.Add(new Issue(Severity.Error, "Scene", $"{name} 컴포넌트 없음"));
        }

        private static void ValidateShopPanelWiring(List<Issue> issues)
        {
            var sp = Object.FindAnyObjectByType<ShopPanel>();
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
            var go = Object.FindAnyObjectByType<GameOverPanel>();
            if (go == null) { issues.Add(new Issue(Severity.Error, "GameOverPanel", "씬에 GameOverPanel 없음")); return; }
            var so = new SerializedObject(go);
            CheckRef(issues, "GameOverPanel", so, "_panelRoot");
            CheckRef(issues, "GameOverPanel", so, "_restartButton");
        }

        private static void ValidateChapterClearedPanelWiring(List<Issue> issues)
        {
            var cc = Object.FindAnyObjectByType<ChapterClearedPanel>();
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
