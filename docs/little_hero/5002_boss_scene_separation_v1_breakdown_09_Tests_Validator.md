# T09: BossSceneSeparationTests NUnit 5+ + ProjectValidator 룰

> **레이어**: Test
> **의존성**: T01~T08 (모든 코드 변경 완료)
> **예상 시간**: 30분 / 1 commit (5002 Sprint 7, 9/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 7

## 1. 작업 항목

### 1.1 신규 파일 — BossSceneSeparationTests.cs

**경로**: `LittleHero/Assets/_Project/Tests/Editor/BossSceneSeparationTests.cs`

**테스트 7+개** (Phase 6 Contract C2/C4 검증):

```csharp
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests
{
    public class BossSceneSeparationTests
    {
        [Test]
        public void Launcher_Default_NotNull()
        {
            Assert.NotNull(BossEncounterLauncher.Default);
        }

        [Test]
        public void Mode_Enum_Has_ChapterBoss()
        {
            var values = System.Enum.GetValues(typeof(BossEncounterContext.Mode));
            CollectionAssert.Contains(values, BossEncounterContext.Mode.ChapterBoss);
        }

        [Test]
        public void Launcher_Launch_FillsContext_ChapterBoss()
        {
            BossEncounterContext.Clear();
            var mockLauncher = new MockLauncher();
            var prevDefault = BossEncounterLauncher.Default;
            BossEncounterLauncher.Default = mockLauncher;

            var boss = ScriptableObject.CreateInstance<EnemyData>();
            boss.id = "test_boss";
            var chapter = ScriptableObject.CreateInstance<ChapterData>();
            chapter.id = "ch1";

            BossEncounterLauncher.Launch(new BossEncounterRequest {
                mode = BossEncounterContext.Mode.ChapterBoss,
                boss = boss, chapter = chapter, returnSceneName = "Attic" });

            Assert.AreEqual(1, mockLauncher.LaunchCount);
            Assert.AreEqual(BossEncounterContext.Mode.ChapterBoss, mockLauncher.LastRequest.mode);
            Assert.AreEqual("test_boss", mockLauncher.LastRequest.boss.id);
            Assert.AreEqual("ch1", mockLauncher.LastRequest.chapter.id);

            BossEncounterLauncher.Default = prevDefault;
            Object.DestroyImmediate(boss);
            Object.DestroyImmediate(chapter);
        }

        [Test]
        public void Launcher_Launch_FillsContext_Daily()
        {
            BossEncounterContext.Clear();
            var mockLauncher = new MockLauncher();
            var prevDefault = BossEncounterLauncher.Default;
            BossEncounterLauncher.Default = mockLauncher;

            var boss = ScriptableObject.CreateInstance<EnemyData>();
            var endgameMode = ScriptableObject.CreateInstance<EndgameModeData>();

            BossEncounterLauncher.Launch(new BossEncounterRequest {
                mode = BossEncounterContext.Mode.Daily,
                boss = boss, endgameMode = endgameMode, returnSceneName = "Attic" });

            Assert.AreEqual(BossEncounterContext.Mode.Daily, mockLauncher.LastRequest.mode);

            BossEncounterLauncher.Default = prevDefault;
            Object.DestroyImmediate(boss);
            Object.DestroyImmediate(endgameMode);
        }

        [Test]
        public void CompleteChapterBoss_Null_NoOp()
        {
            // 예외 없이 return
            Assert.DoesNotThrow(() => BossEncounterService.CompleteChapterBoss(null, null));
        }

        [Test]
        public void Context_Clear_ResetsAllFields()
        {
            BossEncounterContext.PendingBoss = ScriptableObject.CreateInstance<EnemyData>();
            BossEncounterContext.PendingMode = ScriptableObject.CreateInstance<EndgameModeData>();
            BossEncounterContext.PendingChapter = ScriptableObject.CreateInstance<ChapterData>();
            BossEncounterContext.PendingReturnScene = "Attic";

            BossEncounterContext.Clear();

            Assert.IsNull(BossEncounterContext.PendingBoss);
            Assert.IsNull(BossEncounterContext.PendingMode);
            Assert.IsNull(BossEncounterContext.PendingChapter);
            Assert.IsNull(BossEncounterContext.PendingReturnScene);
        }

        [Test]
        public void BackgroundApplier_Null_NoOp()
        {
            Assert.DoesNotThrow(() => BackgroundApplier.Apply(null, null));
            var ch = ScriptableObject.CreateInstance<ChapterData>();
            Assert.DoesNotThrow(() => BackgroundApplier.Apply(null, ch));
            Object.DestroyImmediate(ch);
        }

        [Test]
        public void BossEncounterRequest_Has_AllFields()
        {
            // Reflection — 6 필드 검증
            var t = typeof(BossEncounterRequest);
            Assert.NotNull(t.GetField("mode"));
            Assert.NotNull(t.GetField("boss"));
            Assert.NotNull(t.GetField("endgameMode"));
            Assert.NotNull(t.GetField("chapter"));
            Assert.NotNull(t.GetField("mythicPool"));
            Assert.NotNull(t.GetField("returnSceneName"));
        }

        // Mock helper
        private class MockLauncher : IBossEncounterLauncher
        {
            public int LaunchCount;
            public BossEncounterRequest LastRequest;
            public void Launch(BossEncounterRequest req)
            {
                LaunchCount++;
                LastRequest = req;
            }
        }
    }
}
```

라인 수: ~120L

### 1.2 기존 파일 수정 — ProjectValidator.cs

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs`

**룰 추가**: `BossEncounterContext.Set(` 외부 직접 호출 검사

```csharp
// 5002 — BossEncounterContext.Set 외부 직접 호출 검사 (§15.7 DI)
private static void ValidateBossEncounterContextDirectAccess(StringBuilder report)
{
    var allowedFiles = new HashSet<string>
    {
        "BossEncounterLauncher.cs",
        "BossEncounterRunner.cs",
        "BossEncounterContext.cs",  // 정의 파일 자체
    };
    var scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
    int violations = 0;
    foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
    {
        string filename = Path.GetFileName(file);
        if (allowedFiles.Contains(filename)) continue;
        var content = File.ReadAllText(file);
        if (content.Contains("BossEncounterContext.Set(")
            || content.Contains("BossEncounterContext.PendingBoss =")
            || content.Contains("BossEncounterContext.PendingChapter ="))
        {
            report.AppendLine($"  [5002 §15.7 위반] {filename} — BossEncounterContext 외부 직접 R/W");
            violations++;
        }
    }
    if (violations == 0) report.AppendLine("  ✅ BossEncounterContext 외부 직접 R/W 0건");
    else report.AppendLine($"  ⚠️ BossEncounterContext 외부 직접 R/W {violations}건 — Launcher 위임 권장");
}
```

ProjectValidator.Run() 메서드에 본 호출 추가.

## 2. 검증 기준

### 2.1 컴파일
- 신규 Test 파일 컴파일 OK
- Validator 신규 메서드 컴파일 OK

### 2.2 Editor Test
- Test Runner EditMode → 신규 7+ 통과
- 기존 46 회귀 X (총 53+)

### 2.3 ProjectValidator
- `Tools > LittleHero > Validate Project` 실행 → 위반 0
- T08 EndgameMenuPanel 변경 후 `BossEncounterContext.Set` 외부 호출 0건

### 2.4 회귀
- 기존 NUnit 46 PASS 유지
- Validator 기존 룰 (200L+ 경고 등) 변동 X

## 3. commit 메시지

```
5002 Sprint 7 — NUnit 7+ + ProjectValidator BossEncounterContext.Set 외부 호출 룰 (9/10)

[변경]
- Tests/Editor/BossSceneSeparationTests.cs 신규 (~120L, NUnit 7+)
  - Launcher_Default_NotNull
  - Mode_Enum_Has_ChapterBoss
  - Launcher_Launch_FillsContext_ChapterBoss/Daily (Mock 주입)
  - CompleteChapterBoss_Null_NoOp
  - Context_Clear_ResetsAllFields
  - BackgroundApplier_Null_NoOp
  - BossEncounterRequest_Has_AllFields
- Editor/ProjectValidator.cs — BossEncounterContext.Set 외부 호출 검사 룰

[검증]
- Test Runner EditMode 통과 (46 → 53+)
- Validate Project 실행 — 위반 0
- BossEncounterLauncher.Default mock 주입 가능 (§15.7 DI 검증)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 7
- 디자인 §4 §15.9 Test, §15.12 Validator

## 5. 알려진 위험

- `_instantCompleteForTesting` 영역 — EndgameMenuPanel 의 `_instantCompleteForTesting==true` 분기는 `BossEncounterService.CompleteDailyBoss` 직접 호출 (정적 Context.Set 호출 X). Validator 룰은 `BossEncounterContext.Set` 만 검사하므로 통과 ✅
- ScriptableObject Test 인스턴스 destroy 누락 시 Editor 메모리 누수 — 모든 테스트 마지막에 `DestroyImmediate` 필수
- Mock Launcher 의 prev/restore 패턴 — 다른 테스트 간섭 회피 (각 테스트에서 Default 복원)
