# 5013 — EnemyVisibilityCuller v1 (Off-screen Culling, 디자인)

> **목적**: Off-screen 적의 SpriteRenderer/Animator/Update 비활성화로 GC + frame time 개선. 300 cap 환경에서 60fps 유지 강화.
> **우선순위**: 높음 (PIVOT v4 §7 성능 목표 — iPhone 12 60fps).

## 0. 결정 이력

- 2026-05-04: 5008 종결 시 후속 anchor 등록 (이전 5010 → 재부여 5013)

## 1. 현재 시스템 분석

### 1.1 적 cap 300 환경 부하

- 동시 적 300 (PIVOT v4 cinematic_combat_v1 cap)
- 화면 visible 영역: ~50~100 적 (카메라 viewport + margin)
- 나머지 200~250 적: off-screen (사용자 비가시) — 그러나 SpriteRenderer 활성 + Animator 작동 + Update 호출

### 1.2 현재 EnemyAnimationManager (단일 Update)

- §15 OOP 정책 적용 — 단일 Manager + List 일괄 Update (이미 GC 최소화됨)
- 다만 SpriteRenderer/Animator 자체 활성 — GPU/CPU 비용 잔존

### 1.3 측정 (예상)

- iPhone 12 cap 300 환경 — frame time ~16ms 이하 (60fps) 마진 미세
- Off-screen 200 적 cull 시 — GPU draw call ~50% 절감, CPU Update ~30% 절감 추정

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | SpriteRenderer.enabled false (off-screen) | 단순 | Update / Animator 잔존 |
| **B (★권장)** | **SpriteRenderer + Animator + EnemyAnimationManager 등록 해제 통합** | 완전 cull, 60fps 마진 ↑ | EnemyController 진입부 변경 |
| C | Camera Frustum + GameObject SetActive(false) | 가장 강력 | trigger 영향 + 회복 시 cost |

> 권장 **B** — 적당히 강력 + EnemyAnimationManager 패턴 활용.

## 3. 권장 설계

### 3.1 신규 파일

- `Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs` (~80L)
  - `static bool IsVisible(Vector3 worldPos, Camera cam, float margin = 1.5f)` 정적 헬퍼
  - viewport 변환 + margin (화면 외부도 일부 활성)
  - 정적 buffer (GC 0)

### 3.2 EnemyController hook

- `Update()` 첫 줄: 
  ```
  if (!EnemyVisibilityCuller.IsVisible(transform.position, _cam)) {
      _spriteRenderer.enabled = false;
      EnemyAnimationManager.Unregister(this);
      return;  // 나머지 AI/이동 skip
  } else {
      _spriteRenderer.enabled = true;
      EnemyAnimationManager.Register(this);
  }
  ```

### 3.3 Update 빈도 조절

- 매 프레임 visibility 체크 vs 0.1초 마다 (성능 + 정확성 trade-off)
- **권장**: 0.1초 throttle (UnityEngine.Time.fixedDeltaTime 기반)

### 3.4 PlayerFinder 와의 통합

- 카메라 = MainCamera 정적 cache (EnemyVisibilityCuller 내부)
- PlayerFinder 패턴 답습

## 4. Sprint 분해

| Sprint | 작업 | commit |
|---|---|---|
| 1 | EnemyVisibilityCuller 정적 헬퍼 + Test | 1 |
| 2 | EnemyController.Update hook + throttle | 1 |
| 3 | EnemyAnimationManager Register/Unregister 정합 | 1 |
| 4 | Editor Test (IsVisible 5+ NUnit) + Validator | 1 |
| 5 | Profiler 측정 + docs | 1 |
| **총** | | **5** |

## 5. CLAUDE.md §15 정합

- §15.1 SRP — Culler 단일 책임 ✓
- §15.6 100L — ~80L ✓
- §15.7 정적 헬퍼 ✓
- §15.11 GC — Vector3 struct, Camera cache ✓

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Trigger 영역 cull 시 충돌 누락 | Damage v2 가 OverlapCircle 기반 (Trigger X) — 영향 0 |
| 화면 진입 시 sprite 늦게 보임 | margin 1.5x viewport (사전 활성화) |
| 0.1초 throttle 정확도 | Time.unscaledDeltaTime — pause 시 영향 X |

## 7. 변경 이력

- 2026-05-04: v1 작성.
