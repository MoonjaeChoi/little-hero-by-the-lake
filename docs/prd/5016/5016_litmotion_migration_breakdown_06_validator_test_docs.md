# T06: Validator + NUnit 6 + GCAllocMeasurer 재측정 + docs

> **레이어**: Test + Docs
> **의존성**: T02 ~ T05 (모두)
> **예상 시간**: 35분 / 1 commit
> **PRD 출처**: `5016_litmotion_migration_v2.md` §2 Sprint 6

## 1. 작업 항목

### 1.1 ProjectValidator.cs 룰 추가

**파일**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs`

**산출물**: `ValidateLitMotionMigration` 메서드 신규 (~20L)
```csharp
private static void ValidateLitMotionMigration(List<Issue> issues) {
    string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
    int doTweenViolations = 0;
    int litMotionImports = 0;
    foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories)) {
        string content = File.ReadAllText(file);
        if (content.Contains("using DG.Tweening")) doTweenViolations++;
        if (content.Contains("using LitMotion")) litMotionImports++;
    }
    if (doTweenViolations > 0)
        issues.Add(new Issue(Severity.Warning, "5016 LitMotion", $"DOTween 재도입: {doTweenViolations}"));
    if (litMotionImports == 0)
        issues.Add(new Issue(Severity.Warning, "5016 LitMotion", "LitMotion import 0"));
    else
        Debug.Log($"[ProjectValidator] 5016 LitMotion OK — {litMotionImports} files");
}
```

### 1.2 NUnit 6 — LMotionExtensionsTests.cs

**파일**: `LittleHero/Assets/_Project/Tests/Editor/LMotionExtensionsTests.cs` (신규, ~80L)

**Test 명세**:
- `DOFadeAlpha_SpriteRenderer_ReturnsActiveHandle`
- `DOFadeAlpha_Image_ReturnsActiveHandle`
- `DOFade_CanvasGroup_ReturnsActiveHandle`
- `DOPunchScale_Transform_ReturnsActiveHandle`
- `DOMove_Transform_ReturnsActiveHandle`
- `Type_LMotionExtensions_IsStaticClass`

각 Test:
1. 임시 GameObject + 컴포넌트 생성
2. Extension method 호출
3. `MotionHandle.IsActive() == true` 확인
4. `Cancel()` + GameObject 정리

(T01 의 Smoke test 1 + T06 의 Tests 6 = 누적 100 NUnit)

### 1.3 GCAllocMeasurer 재측정 (사용자 검증)

**호출**: 사용자가 `Tools/LittleHero/Measure GC Alloc (60s)` 실행
- 60초 측정
- 결과: **시나리오 A (< 30 KB/s)** 도달 — 5016 성공
- 시나리오 B/C — 핫 패스 누락 fix (5016.1)

### 1.4 docs

**파일**:
- `docs/little_hero/changelog_v1.md` — 5016 항목 추가
- `CLAUDE.md` §10 — LitMotion MIT 행 추가
- `CLAUDE.md` §11 — 마일스톤 행 추가
- `CLAUDE.md` §12 — 시스템 현황 갱신
- `CLAUDE.md` §14 — 직전 5건 회전 (5009 또는 5010 제거, 5016 추가)

## 2. 검증 기준

### 2.1 컴파일
- errors=0, 5초 대기

### 2.2 Editor Test (NUnit)
- `LMotionExtensionsSmokeTest.Sprint1_LMotion_Create_Bind_ProducesActiveHandle` (T01) PASS
- `LMotionExtensionsTests` 6/6 PASS
- 누적 100 NUnit PASS

### 2.3 ProjectValidator
- `Tools/LittleHero/Validate Project` 실행
- DOTween 재도입 0 (errors=0)
- LitMotion import ≥ 22 files

### 2.4 GCAllocMeasurer (사용자)
- 시나리오 A (< 30 KB/s) 도달

## 3. commit 메시지

(PRD §2 Sprint 6 commit 메시지 그대로)

## 4. 관련 문서

- PRD §2 Sprint 6
- 디자인 §3.5 ProjectValidator 룰

## 5. 알려진 위험

- GC 재측정 시나리오 B/C 회귀 — 핫 패스 누락 fix (5016.1 추가 commit)
- LitMotion Editor Test 실행 시 LMotion runtime 의존성 — `[UnityTest] IEnumerator` 또는 즉시 검증 패턴 활용
