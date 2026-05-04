# T03 — EnemyAnimationManager Register/Unregister 정합 (HashSet)

> **레이어**: Logic
> **의존성**: T01, T02
> **예상 시간**: 20분 / 1 commit
> **PRD 출처**: `5013_enemy_visibility_culler_v1_v2.md` §2 Sprint 3

## 1. 작업 항목

### 1.1 기존 파일 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Enemies/EnemyAnimationManager.cs`

**변경 명세**:

기존 List<EnemyController> _registered → **HashSet<EnemyController> _registered** 로 변경.

```csharp
private static readonly HashSet<EnemyController> _registered = new HashSet<EnemyController>(256);

public static void Register(EnemyController e) {
    if (e != null) _registered.Add(e);  // O(1)
}

public static void Unregister(EnemyController e) {
    if (e != null) _registered.Remove(e);  // O(1)
}

private void Update() {
    foreach (var e in _registered) {
        e.AnimationTick();  // 기존 로직 유지
    }
}
```

> **변경 이유 (Rebuttal §3.4)**: 매 프레임 visibility 변동 시 Register/Unregister O(n) 회피 → HashSet O(1).

### 1.2 EnemyController.Update visibility 변동 hook

T02 의 EnemyController.Update 에 Manager 호출 추가:

```csharp
if (visible != _wasVisible) {
    _wasVisible = visible;
    if (_spriteRenderer != null) _spriteRenderer.enabled = visible;
    if (visible) EnemyAnimationManager.Register(this);
    else EnemyAnimationManager.Unregister(this);
}
```

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 5~10초 대기

### 2.2 List → HashSet 회귀 X
- 기존 `_registered.Count` / iteration 동일 동작
- foreach 순서 비결정 — Animation tick 순서 의존성 검토 (대부분 영향 X)

### 2.3 Play Mode 검증
- 300 적 환경 (test scene 또는 wave 30+)
- 매 프레임 visibility 변동 시 GC 0 (Profiler 확인)
- HashSet.Add/Remove O(1) 검증 (Stopwatch 측정 권장)

### 2.4 회귀
- AnimationTick 호출 누락 0 (모든 visible 적 매 프레임 호출)
- off-screen 적 AnimationTick 호출 X

## 3. commit 메시지

```
5013 Sprint 3 / T03 — EnemyAnimationManager HashSet + Register/Unregister hook (3/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyAnimationManager.cs:
  - List<EnemyController> _registered → HashSet<EnemyController> (256 capacity)
  - Register/Unregister O(1) (List Remove O(n) 회피, Rebuttal §3.4)
- Scripts/Gameplay/Enemies/EnemyController.cs:
  - visibility 변동 hook → Register/Unregister 자동

[검증]
- 컴파일 errors=0
- 300 적 환경 GC 0 (Profiler 1분 측정)
- AnimationTick 회귀 X (모든 visible 적 매 프레임 호출)
- §15.11 정합 (HashSet O(1))

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 3
- Rebuttal §3.4 (HashSet O(1))
- 5005 AtticPanelToggleManager 의 Dictionary 패턴 (유사)

## 5. 알려진 위험

- **HashSet iteration 순서 비결정** — AnimationTick 순서 의존성 있는 경우 문제. 검증 필요 (대부분 frame-independent)
- **HashSet Capacity 256** — 300 적 cap 환경 충분. resize 시 GC 한 번만 발생 (게임 시작 직후)
- **Concurrent modification** — Update 중 Register/Unregister 호출 시 InvalidOperationException 위험. 다만 Update 같은 thread (main) 라 안전 (실제 검증 시 ConcurrentBag 또는 deferred queue 검토)
