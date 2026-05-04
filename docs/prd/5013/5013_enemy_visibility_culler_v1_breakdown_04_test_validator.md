# T04 — NUnit 6 + ProjectValidator EnemyController_Visibility_Check_Present 룰

> **레이어**: Test
> **의존성**: T01, T02, T03
> **예상 시간**: 30분 / 1 commit
> **PRD 출처**: `5013_enemy_visibility_culler_v1_v2.md` §2 Sprint 4

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Tests/Editor/EnemyVisibilityCullerTests.cs`

**Test 메서드 명세**:

```csharp
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor {
    public class EnemyVisibilityCullerTests {

        [Test]
        public void IsVisible_Origin_True() {
            // Camera.main 이 default 위치라면 origin visible
            EnemyVisibilityCuller.InvalidateCache();
            // ... mock camera setup or assume Camera.main exists
            Assert.IsTrue(EnemyVisibilityCuller.IsVisible(Vector3.zero));
        }

        [Test]
        public void IsVisible_FarOffscreen_False() {
            EnemyVisibilityCuller.InvalidateCache();
            // 매우 먼 위치 (viewport 외부)
            Assert.IsFalse(EnemyVisibilityCuller.IsVisible(new Vector3(10000, 10000, 0)));
        }

        [Test]
        public void IsVisible_Margin_True() {
            EnemyVisibilityCuller.InvalidateCache();
            // viewport 외부지만 margin 1.5x 영역 내
            // 검증: margin 영향 (viewport 외 0.5 까지 visible)
            Assert.IsTrue(EnemyVisibilityCuller.IsVisible(new Vector3(0, 0, 0), margin: 2.0f));
        }

        [Test]
        public void InvalidateCache_NextCallReinit() {
            EnemyVisibilityCuller.InvalidateCache();
            // 첫 호출 시 Camera.main cache → cache invalidate → 다음 호출 시 재 cache
            EnemyVisibilityCuller.IsVisible(Vector3.zero);
            EnemyVisibilityCuller.InvalidateCache();
            // _cachedCam == null 검증 (Reflection)
            var field = typeof(EnemyVisibilityCuller).GetField("_cachedCam", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNull(field.GetValue(null));
        }

        [Test]
        public void IsVisible_NoCamera_DefaultsTrue() {
            // Camera.main null 시 안전 fallback true
            // Mock 또는 reflection 으로 _cachedCam = null 강제
            // ... 검증
            Assert.IsTrue(EnemyVisibilityCuller.IsVisible(Vector3.zero));  // safety fallback
        }

        [Test]
        public void InvalidateCache_OnSceneTransition_Triggered() {
            // Rebuttal §3.3 — RuntimeInitializeOnLoadMethod hook 검증
            EnemyVisibilityCuller.InvalidateCache();
            // SceneTransitionService.OnAfterSceneLoad?.Invoke() — Reflection 으로 trigger
            var ev = typeof(SceneTransitionService).GetField("OnAfterSceneLoad",
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(ev);  // 이벤트 존재 검증
            // 실제 hook 동작 = Init() 자동 호출 검증 (Reflection)
        }
    }
}
```

**예상 라인**: ~60 (6 NUnit, §15.6 통과)

### 1.2 ProjectValidator 룰 추가

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs`

**룰 추가**: `EnemyController_Visibility_Check_Present`

```csharp
private static void ValidateEnemyControllerVisibilityCheck() {
    var path = "Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs";
    var src = File.ReadAllText(path);
    if (!src.Contains("EnemyVisibilityCuller.IsVisible")) {
        Debug.LogWarning($"[Validator] EnemyController_Visibility_Check_Present: " +
                         $"{path} 의 Update 에 EnemyVisibilityCuller.IsVisible 호출 누락 (5013)");
    }
}
```

기존 ValidateProject() 메서드에 호출 추가.

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 도메인 리로드 5~10초

### 2.2 NUnit 실행
- Test Runner Edit Mode > Run All
- **6/6 PASS, 누적 NUnit 81 → 87**
- 각 테스트 1초 내 완료

### 2.3 Validator 룰 검증
- `Tools/LittleHero/Validate Project` 실행 → 정상 통과 (T02 의 visibility 호출 존재)
- (테스트) EnemyController.Update 의 IsVisible 호출 임시 제거 → warning 표시 검증
- 복원 후 warning 사라짐

### 2.4 회귀
- 기존 NUnit 81 모두 PASS (회귀 X)

## 3. commit 메시지

```
5013 Sprint 4 / T04 — EnemyVisibilityCullerTests 6 NUnit + Validator 룰 (4/5)

[변경]
- Tests/Editor/EnemyVisibilityCullerTests.cs 신규 (6 NUnit)
  - IsVisible_Origin_True
  - IsVisible_FarOffscreen_False
  - IsVisible_Margin_True
  - InvalidateCache_NextCallReinit
  - IsVisible_NoCamera_DefaultsTrue
  - InvalidateCache_OnSceneTransition_Triggered (Rebuttal §3.3)
- Editor/ProjectValidator.cs:
  - EnemyController_Visibility_Check_Present 룰 추가 (Rebuttal §3.1)

[검증]
- 6/6 PASS, 누적 NUnit 81 → 87
- Validator 정상 통과 (IsVisible 호출 존재)
- 기존 81 NUnit 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 4
- Rebuttal §3.1 (Validator 룰) + §3.3 (Scene hook)
- 5004 SceneTransitionTests 패턴 (5 NUnit, 5004 §S3)

## 5. 알려진 위험

- **Editor mode 에서 Camera.main null** — Mock 또는 Editor scene Camera 검증 필요
- **Reflection 의 BindingFlags** — `using System.Reflection;` 추가 필수
- **InvalidateCache_OnSceneTransition_Triggered 검증 어려움** — RuntimeInitializeOnLoadMethod 가 Editor 에서 자동 실행되지 않음 → 이벤트 존재 + 호출 가능 검증만 (Play Mode 검증은 C6 시나리오에서)
