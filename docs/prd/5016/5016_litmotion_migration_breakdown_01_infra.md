# T01: LitMotion 인프라 + wrapper + smoke + 빌드

> **레이어**: Core
> **의존성**: 없음
> **예상 시간**: 40분 (30분 + smoke 5분 + 빌드 5분) / 1 commit
> **PRD 출처**: `5016_litmotion_migration_v2.md` §2 Sprint 1, §5 S0

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Packages/manifest.json` (수정)

**변경**:
```json
{
  "dependencies": {
    "com.annulusgames.lit-motion": "https://github.com/annulusgames/LitMotion.git?path=src/LitMotion/Assets/LitMotion",
    "com.unity.burst": "1.6.0",
    "com.unity.collections": "1.5.1",
    "com.unity.mathematics": "1.0.0"
  }
}
```

**경로**: `LittleHero/Assets/_Project/Scripts/Core/LMotionExtensions.cs` (신규, ~70L)

**클래스 명세** (정적 wrapper, §15.6 100L):
```csharp
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero {
    public static class LMotionExtensions {
        public static MotionHandle DOFadeAlpha(this SpriteRenderer sr, float endAlpha, float duration) {...}
        public static MotionHandle DOFadeAlpha(this Image img, float endAlpha, float duration) {...}
        public static MotionHandle DOFade(this CanvasGroup cg, float endValue, float duration) {...}
        public static MotionHandle DOPunchScale(this Transform tr, float strength, float duration) {...}
        public static MotionHandle DOMove(this Transform tr, Vector3 endValue, float duration) {...}
        public static MotionHandle DOFillAmount(this Image img, float endValue, float duration) {...}
    }
}
```

**경로**: `LittleHero/Assets/_Project/Tests/Editor/LMotionExtensionsSmokeTest.cs` (신규, ~30L)

**Smoke test 명세** (D4=A):
```csharp
using NUnit.Framework;
using LitMotion;

namespace LittleHero.Tests.Editor {
    public class LMotionExtensionsSmokeTest {
        [Test]
        public void Sprint1_LMotion_Create_Bind_ProducesActiveHandle() {
            float result = 0f;
            var handle = LMotion.Create(0f, 1f, 0.1f).Bind(v => result = v);
            Assert.IsTrue(handle.IsActive(), "MotionHandle should be active after Create+Bind");
            handle.Cancel();
        }
    }
}
```

### 1.2 D2=A Player 빌드 측정

Sprint 1 commit 후 사용자가:
1. File → Build Settings → Mac/iOS → Build
2. Builds/<platform>/ 산출물 용량 측정
3. App 용량 < 150MB 확인

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 10초 대기 (도메인 리로드 — Burst 컴파일 후 안정화)

### 2.2 Editor Test
- `LMotionExtensionsSmokeTest.Sprint1_LMotion_Create_Bind_ProducesActiveHandle` PASS

### 2.3 ProjectValidator
- 신규 warnings 0 (LMotionExtensions ~70L < 100L ✅)

### 2.4 회귀
- 기존 코드 동작 변경 X (wrapper 만 추가, 호출처 0)

### 2.5 Player 빌드 (사용자 D2=A)
- App 용량 < 150MB

## 3. commit 메시지

(PRD §2 Sprint 1 commit 메시지 그대로)

## 4. 관련 문서

- PRD §2 Sprint 1
- 디자인 §3.1, §3.2
- decisions D2=A (양시점 측정), D4=A (smoke test)

## 5. 알려진 위험

- LitMotion v2.0.1 + Unity 6.3 호환 X — D4=A smoke test 로 이른 발견 → 5015c 재정의
- Burst 의존성 ~10MB+ — D2=A Sprint 1 빌드 측정 → 임계 위반 시 BasicMode 또는 5015c 폴백
