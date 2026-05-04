# T01 — EnemyVisibilityCuller 정적 헬퍼 + Scene hook

> **레이어**: Logic
> **의존성**: 없음
> **예상 시간**: 30분 / 1 commit
> **PRD 출처**: `5013_enemy_visibility_culler_v1_v2.md` §2 Sprint 1

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs`

**클래스 명세**:
```csharp
using UnityEngine;

namespace LittleHero {
    /// <summary>
    /// 5013 — Off-screen 적 visibility 판정 정적 헬퍼.
    /// PIVOT v4 §7 cap 300 환경 60fps 마진 강화. PlayerFinder 패턴 답습 (Camera cache).
    /// </summary>
    public static class EnemyVisibilityCuller {
        private const float DEFAULT_MARGIN = 1.5f;
        private static Camera _cachedCam;

        public static bool IsVisible(Vector3 worldPos, float margin = DEFAULT_MARGIN) {
            var cam = GetCamera();
            if (cam == null) return true;  // 안전 fallback
            var vp = cam.WorldToViewportPoint(worldPos);
            float lo = (1f - margin) * 0.5f;
            float hi = 1f - lo;
            return vp.z > 0 && vp.x >= lo && vp.x <= hi && vp.y >= lo && vp.y <= hi;
        }

        public static void InvalidateCache() => _cachedCam = null;

        private static Camera GetCamera() {
            if (_cachedCam == null) _cachedCam = Camera.main;
            return _cachedCam;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init() {
            // 5004 SceneTransitionService.OnAfterSceneLoad → InvalidateCache 자동 hook
            SceneTransitionService.OnAfterSceneLoad += InvalidateCache;
        }
    }
}
```

**예상 라인**: ~35 (§15.6 100L 통과)

### 1.2 기존 파일 수정

(없음 — 신규 정적 헬퍼만)

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 5~10초 대기 (도메인 리로드)

### 2.2 정적 호출 가능
- Reflection: `LittleHero.EnemyVisibilityCuller.IsVisible(Vector3.zero)` 호출 가능
- `_cachedCam == null` 시 Camera.main 자동 cache

### 2.3 SceneTransition hook
- `SceneTransitionService.OnAfterSceneLoad` 이벤트 구독 확인 (RuntimeInitializeOnLoadMethod)

### 2.4 회귀
- 기존 EnemyController / EnemyAnimationManager 영향 X (이 commit 만)

## 3. commit 메시지

```
5013 Sprint 1 / T01 — EnemyVisibilityCuller 정적 헬퍼 + Scene hook (1/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs 신규 (~35L)
- IsVisible(Vector3, margin=1.5f) viewport 변환 + 정적 Camera cache
- InvalidateCache() public + RuntimeInitializeOnLoadMethod 자동 SceneTransition hook (Rebuttal §3.3)
- DEFAULT_MARGIN const 1.5f (D2=A mediator)

[검증]
- 컴파일 errors=0
- §15.6 100L 통과 (~35L)
- §15.7 정적 cache (PlayerFinder 패턴)
- §15.11 GC 0 (Vector3 struct)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 1
- 디자인 §3.1 EnemyVisibilityCuller 명세
- decisions D2=A (margin 1.5x) + Rebuttal §3.3 (SceneTransition hook)

## 5. 알려진 위험

- **SceneTransitionService.OnAfterSceneLoad 이벤트 미존재** → 5004 검증 필요. 없으면 InvalidateCache 수동 호출만 (자동 hook 누락 시 docs 명시)
- **Camera.main null 시점** (씬 진입 직후) → 안전 fallback `return true` 적용
- **viewport 변환 GC** → Vector3 struct, 0 GC 검증
