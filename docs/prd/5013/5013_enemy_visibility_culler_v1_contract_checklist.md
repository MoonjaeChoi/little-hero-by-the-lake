# Evaluator Checklist: 5013 enemy_visibility_culler_v1

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일

- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false` (5~10초 후)
- [ ] T01 EnemyVisibilityCuller.cs 컴파일 errors=0
- [ ] T02 EnemyController.cs 수정 후 errors=0
- [ ] T03 EnemyAnimationManager.cs (HashSet) errors=0
- [ ] T04 EnemyVisibilityCullerTests.cs errors=0
- [ ] 5 commit 모두 errors=0

### C2 Editor Test (NUnit)

- [ ] Test Runner > Edit Mode > Run All → 누적 87/87 PASS
- [ ] `EnemyVisibilityCullerTests` 클래스 6 메서드 모두 PASS
  - [ ] IsVisible_Origin_True
  - [ ] IsVisible_FarOffscreen_False
  - [ ] IsVisible_Margin_True
  - [ ] InvalidateCache_NextCallReinit
  - [ ] IsVisible_NoCamera_DefaultsTrue
  - [ ] InvalidateCache_OnSceneTransition_Triggered
- [ ] 기존 81 NUnit 회귀 X

### C3 ProjectValidator

- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings 변동 ±0 (5008 baseline)
- [ ] 신규 위반 0건
- [ ] **EnemyController_Visibility_Check_Present** 룰 추가 + 정상 동작 (현재 통과)

### C4 Reflection 정적 검증

- [ ] `LittleHero.EnemyVisibilityCuller` (static class) 존재
- [ ] `EnemyVisibilityCuller.IsVisible(Vector3, float) → bool` 정적 public 메서드 존재
- [ ] `EnemyVisibilityCuller.InvalidateCache() → void` 정적 public 메서드 존재
- [ ] `EnemyVisibilityCuller._cachedCam` (private static Camera) 존재
- [ ] `EnemyController._wasVisible` field 존재 (NonPublic|Instance)
- [ ] `EnemyController._lastVisCheck` field 존재
- [ ] `EnemyAnimationManager._registered` HashSet<EnemyController> 타입
- [ ] `SceneTransitionService.OnAfterSceneLoad` event/delegate 존재 (5004 회귀 X)

### C5 SO 검증

- [ ] (SO 신규 0 — 본 PRD 는 코드만)
- [ ] 회귀: 기존 SO 412 (BalanceConfig / EnemyData / etc.) 변경 X

## 수동 검증 (사용자 Play Mode)

### C6.1 화면 외 적 SpriteRenderer 비활성

- [ ] 시나리오 S1 단계 1~3 진행 (CH 1 wave 1)
- [ ] 카메라 viewport 외 적 SpriteRenderer.enabled = false 확인
- [ ] 화면 진입 시 SpriteRenderer.enabled = true 복원
- [ ] 콘솔 에러 0건

### C6.2 Profiler 60fps 유지 (cap 300)

- [ ] iPhone 12 또는 Editor cap 300 시나리오 진입
- [ ] Unity Profiler 1분 측정
- [ ] frame time < 16ms (60fps)
- [ ] GPU draw call 절감 (기대 -50%)
- [ ] CPU EnemyController.Update 호출 빈도 절감 (기대 -30%)
- [ ] GC allocation/sec < 50KB (PIVOT v4 §7)

### C6.3 Scene 전환 시 Camera 캐시 invalidate

- [ ] 시나리오 S3 단계 1~3 진행 (Attic → Chapter)
- [ ] _cachedCam null 변동 확인 (Reflection or Debug)
- [ ] 새 씬의 적 sprite 정상 표시

### C6.4 Validator 룰 동작

- [ ] 시나리오 S4 단계 1~4 진행 (IsVisible 임시 제거 + 복원)
- [ ] warning 표시 + 복원 시 사라짐

### C6.5 EnemyAnimationManager HashSet 회귀

- [ ] 시나리오 S5 단계 1~2 진행 (CH 1 wave 5)
- [ ] visible 적 AnimationTick 매 프레임 호출
- [ ] off-screen 적 AnimationTick 호출 0
- [ ] HashSet GC 0 (Profiler 1분)

## 비기능 검증

- [ ] 5008 / 5007 / 5005 회귀 X
- [ ] 60fps 유지 (300 cap PIVOT v4)
- [ ] GC <50KB/s

## 합격 판정

- 자동 검증 모두 ✅ + 수동 시나리오 S1, S4 ✅ → **최소 합격**
- 자동 검증 모두 ✅ + 수동 시나리오 S1~S5 ✅ + 비기능 ✅ → **완전 합격**
- 자동 검증 1개라도 ❌ → **FAIL** (Phase 5 수정)
- Profiler 60fps 미달성 시 → 5013.1 후속 (margin/throttle 조정, D1/D2 SerializeField 도입)
