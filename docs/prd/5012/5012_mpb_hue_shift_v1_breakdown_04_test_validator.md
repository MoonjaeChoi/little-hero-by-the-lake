# T04: NUnit 6 + ProjectValidator 룰

> **레이어**: Test
> **의존성**: T01, T02 (T03 권장)
> **예상 시간**: 30분 / 1 commit
> **PRD 출처**: 5012_mpb_hue_shift_v1_v2.md §2 Sprint 4

## 1. 작업 항목

### 1.1 신규 NUnit 파일

**경로**: `LittleHero/Assets/_Project/Tests/Editor/EnemyHueVariationTests.cs`

**Test 메서드 6**:

```csharp
using NUnit.Framework;
using UnityEngine;

namespace LittleHero.Tests.Editor
{
    public class EnemyHueVariationTests
    {
        [Test]
        public void HueShift_Red_Plus50Percent_ReturnsCyan()
        {
            // red (h=0) + 0.5 → cyan (h=0.5)
            var result = EnemyHueVariation.HueShift(Color.red, 0.5f);
            Assert.Greater(result.b, 0.5f, "blue 채널 강화");
            Assert.Greater(result.g, 0.5f, "green 채널 강화");
            Assert.Less(result.r, 0.5f, "red 채널 감소");
        }

        [Test]
        public void HueShift_White_NoChange()
        {
            // white saturation=0 → hue 무관 (white 무변화)
            var result = EnemyHueVariation.HueShift(Color.white, 0.3f);
            Assert.AreEqual(1f, result.r, 0.001f);
            Assert.AreEqual(1f, result.g, 0.001f);
            Assert.AreEqual(1f, result.b, 0.001f);
        }

        [Test]
        public void HueShift_ZeroOffset_Identity()
        {
            var input = new Color(0.7f, 0.3f, 0.5f, 1f);
            var result = EnemyHueVariation.HueShift(input, 0f);
            Assert.AreEqual(input.r, result.r, 0.001f);
            Assert.AreEqual(input.g, result.g, 0.001f);
            Assert.AreEqual(input.b, result.b, 0.001f);
        }

        [Test]
        public void VariationIndex_Wave1_Spawn0_Returns0()
        {
            // (1-1)*7 + 0 = 0 % 5 = 0
            Assert.AreEqual(0, EnemyHueVariation.VariationIndex(1, 0));
        }

        [Test]
        public void VariationIndex_Wave1_Spawn7_Returns2()
        {
            // (1-1)*7 + 7 = 7 % 5 = 2
            Assert.AreEqual(2, EnemyHueVariation.VariationIndex(1, 7));
        }

        [Test]
        public void OFFSETS_HasFive_NoDuplicate()
        {
            var s = new System.Collections.Generic.HashSet<float>();
            foreach (var f in EnemyHueVariation.OFFSETS) s.Add(f);
            Assert.AreEqual(5, EnemyHueVariation.VARIATION_COUNT);
            Assert.AreEqual(5, s.Count, "5 unique offsets");
            Assert.AreEqual(5, EnemyHueVariation.OFFSETS.Length);
        }
    }
}
```

### 1.2 ProjectValidator 룰

**경로**: `Scripts/Editor/ProjectValidator.cs` 수정

신규 메서드 `ValidateEnemyHueVariationHook`:
```csharp
private static void ValidateEnemyHueVariationHook()
{
    var path = "Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs";
    var content = File.ReadAllText(path);
    if (!content.Contains("EnemyHueVariation.HueShift"))
    {
        Debug.LogWarning("[Validator] EnemyController.cs 에 EnemyHueVariation.HueShift 호출 누락 — 5012 회귀 위험");
    }
}
```

`ValidateProject` 진입점에서 호출 추가.

## 2. 검증 기준

### 2.1 컴파일
- refresh + 5초 대기 + errors=0

### 2.2 NUnit
- Test Runner > Edit Mode > Run All
- EnemyHueVariationTests 6/6 PASS
- 누적 NUnit 87 → 93

### 2.3 ProjectValidator
- `Tools/LittleHero/Validate Project` 실행
- errors=0 / warnings 변동 ≤ 0

## 3. commit 메시지

```
5012 Sprint 4 / T04 — EnemyHueVariationTests 6 NUnit + Validator (4/5)

[변경]
- Tests/Editor/EnemyHueVariationTests.cs 신규 (6 NUnit)
- ProjectValidator EnemyController_HueVariation_Hook 룰

[검증]
- 6/6 PASS, 누적 NUnit 87→93
- ValidateProject errors=0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- HSV 변환 부동소수 정밀도 — Assert.AreEqual(..., 0.001f) tolerance
- white sprite 검증 (saturation=0 edge case)
