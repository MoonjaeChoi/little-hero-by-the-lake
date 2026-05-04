# T02 — EnemyController.Update hook + throttle 0.1s

> **레이어**: Logic
> **의존성**: T01 (EnemyVisibilityCuller)
> **예상 시간**: 25분 / 1 commit
> **PRD 출처**: `5013_enemy_visibility_culler_v1_v2.md` §2 Sprint 2

## 1. 작업 항목

### 1.1 기존 파일 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs`

**변경 명세**:

```csharp
public class EnemyController : MonoBehaviour {
    private const float VISIBILITY_THROTTLE_SEC = 0.1f;  // D1=A mediator
    private float _lastVisCheck;
    private bool _wasVisible = true;
    private SpriteRenderer _spriteRenderer;

    private void Update() {
        // 5013 visibility cull — Update 첫 줄 (D1=A throttle 0.1s)
        if (Time.unscaledTime - _lastVisCheck >= VISIBILITY_THROTTLE_SEC) {
            _lastVisCheck = Time.unscaledTime;
            bool visible = EnemyVisibilityCuller.IsVisible(transform.position);
            if (visible != _wasVisible) {
                _wasVisible = visible;
                if (_spriteRenderer != null) _spriteRenderer.enabled = visible;
                // T03: EnemyAnimationManager Register/Unregister 추가
            }
        }
        if (!_wasVisible) return;  // off-screen → AI/이동 skip

        // 기존 Update 로직 (AI / 이동 / etc.)
        // ...
    }
}
```

**라인 변경**: +12 (Update 진입 visibility 체크 + early return)

### 1.2 SpriteRenderer 캐시 (Awake)

```csharp
private void Awake() {
    // 기존 Awake + 추가
    if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
}
```

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 5~10초 대기

### 2.2 Play Mode 검증
- CH 1 wave 1 진입
- 카메라 viewport 외부 적 SpriteRenderer.enabled = false 확인 (Inspector)
- 카메라 진입 시 SpriteRenderer.enabled = true 복원

### 2.3 Throttle 검증
- 매 프레임 IsVisible 호출 X (0.1s 간격)
- `_lastVisCheck` Reflection 으로 시간 진행 확인

### 2.4 회귀
- 기존 EnemyController.Update 의 AI / 이동 / 공격 로직 영향 X
- visible == true 시 기존 로직 그대로 실행

## 3. commit 메시지

```
5013 Sprint 2 / T02 — EnemyController.Update visibility hook + 0.1s throttle (2/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyController.cs:
  - VISIBILITY_THROTTLE_SEC const 0.1f (D1=A mediator)
  - Update 첫 줄 EnemyVisibilityCuller.IsVisible 체크 (Time.unscaledTime throttle)
  - off-screen 시 SpriteRenderer.enabled false + early return
  - Awake 에 SpriteRenderer cache

[검증]
- 컴파일 errors=0
- Play Mode wave 1 — viewport 외 적 SpriteRenderer 비활성
- 기존 AI/이동 로직 회귀 X (visible=true 시)
- §15.11 GC 0 (Time.unscaledTime float)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §2 Sprint 2
- decisions D1=A (throttle 0.1s)

## 5. 알려진 위험

- **첫 프레임 visibility 미체크** — `_wasVisible = true` 초기값 → 화면 밖 적이 첫 프레임 sprite 보임 (다음 0.1s 후 제거)
- **Time.unscaledTime vs Time.time** — pause 시 영향 X (unscaledTime 권장)
- **SpriteRenderer 누락 prefab** — null 체크로 안전 처리
