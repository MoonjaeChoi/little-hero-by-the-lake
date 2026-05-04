# Sprint Contract: 5013 enemy_visibility_culler_v1

> **PGE Planner — Phase 4 Contract**
> PRD: `5013_enemy_visibility_culler_v1_v2.md` · breakdown: `5013_enemy_visibility_culler_v1_breakdown_00.md`
> 생성일: 2026-05-04

## 1. C1 컴파일 기준

- **C1.1**: 신규 .cs 파일 (T01 EnemyVisibilityCuller.cs + T04 EnemyVisibilityCullerTests.cs) 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: 5 commit 모두 errors=0

**검증 방법**:
```
mcp__UnityMCP__refresh_unity (compile=request)
sleep 8  # 도메인 리로드
mcp__UnityMCP__read_console (errors)  # 0
```

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 | 위치 |
|---|---|---|
| `EnemyVisibilityCullerTests` | 6 | `Tests/Editor/EnemyVisibilityCullerTests.cs` |

### 신규 NUnit (6)

- `IsVisible_Origin_True` — Camera.main 기본 위치 origin visible
- `IsVisible_FarOffscreen_False` — viewport 외 적 false
- `IsVisible_Margin_True` — margin 영역 내 visible (margin=2.0f)
- `InvalidateCache_NextCallReinit` — Reflection 으로 _cachedCam == null 검증
- `IsVisible_NoCamera_DefaultsTrue` — Camera.main null 시 안전 fallback true
- `InvalidateCache_OnSceneTransition_Triggered` — `SceneTransitionService.OnAfterSceneLoad` 이벤트 존재 검증

**검증**: Test Runner Edit Mode > Run All → 6/6 PASS, 누적 NUnit 81 → 87

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ±0 (5008 종료 시점 기준)
- **C3.3**: 신규 위반 없음 (§15.6 200L LINE_LIMIT 통과)
  - EnemyVisibilityCuller ~35L ✅ (100L 이내)
  - EnemyController.cs +12L (기존 라인 + visibility 체크) — 기존 200L 임계 체크
  - EnemyAnimationManager.cs List → HashSet 변경 (라인 수 변동 미미)

### 신규 Validator 룰 (T04 추가)

- **EnemyController_Visibility_Check_Present** — 정적 분석으로 EnemyController.Update 의 첫 줄에 `EnemyVisibilityCuller.IsVisible(...)` 호출 검사
  - 누락 시 warning

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.EnemyVisibilityCuller` (static class) | `IsVisible(Vector3, float) → bool` (정적 public) + `InvalidateCache() → void` (정적 public) |
| `LittleHero.EnemyVisibilityCuller._cachedCam` | (private static Camera, NonPublic|Static, Reflection 검증) |
| `LittleHero.EnemyController` | `_wasVisible` field 존재 (NonPublic|Instance) + `_lastVisCheck` field |
| `LittleHero.EnemyAnimationManager._registered` | HashSet<EnemyController> 타입 (Reflection 으로 GetGenericArguments 검증) |
| `LittleHero.SceneTransitionService.OnAfterSceneLoad` | event 또는 정적 delegate field 존재 (5004) |

**검증 방법**:
```csharp
var t = System.Type.GetType("LittleHero.EnemyVisibilityCuller, Assembly-CSharp");
Assert.NotNull(t);
var m = t.GetMethod("IsVisible", BindingFlags.Public | BindingFlags.Static);
Assert.NotNull(m);
Assert.AreEqual(typeof(bool), m.ReturnType);
```

## 5. C5 SO 검증

- **5013 신규 SO 0** — 본 시스템은 SO 추가 X (정적 헬퍼 + Logic 변경만)
- **회귀 검증**: 기존 SO (BalanceConfig / EnemyData / etc.) 변경 X

## 6. C6 Play 시나리오

### S1 — 화면 외 적 SpriteRenderer 비활성

**사전 조건**:
- T02 commit 후 (EnemyController.Update visibility hook)
- CH 1 wave 1 진입

**단계**:
1. Play Mode 진입 → CH 1
2. Wave 1 적 ~10 spawn 후 카메라를 화면 한쪽으로 이동
3. 반대편 적 SpriteRenderer.enabled 확인 (Inspector 또는 Frame Debugger)

**기대 결과**:
- 화면 viewport 외부 적 SpriteRenderer.enabled = false
- 화면 진입 시 SpriteRenderer.enabled = true 복원
- 콘솔 에러 0건

---

### S2 — Profiler 60fps 유지 (cap 300 환경)

**사전 조건**:
- T05 commit 후 (모든 변경 적용)
- iPhone 12 빌드 또는 Editor Play Mode

**단계**:
1. Test scene 또는 wave 30+ chapter 진입 (~150~300 적)
2. Unity Profiler (CPU + Rendering 탭) 또는 Xcode Instruments
3. 1분 측정 → cull 도입 전후 비교

**기대 결과**:
- frame time < 16ms (60fps 유지)
- GPU draw call 절감 (cull 도입 전 대비, 기대 -50%)
- CPU EnemyController.Update 호출 빈도 절감 (기대 -30%)
- GC allocation/sec < 50KB (PIVOT v4 §7)

---

### S3 — Scene 전환 시 Camera 캐시 invalidate

**사전 조건**:
- T01 commit 후 (Culler + RuntimeInitializeOnLoadMethod hook)
- 5004 SceneTransitionService 가용

**단계**:
1. Play Mode 진입 → Attic
2. 챕터 1 진입 (Attic → Chapter scene 전환)
3. Reflection 또는 Debug.Log 으로 EnemyVisibilityCuller._cachedCam 변동 확인

**기대 결과**:
- 콘솔 로그 (T01 명시): Scene 전환 후 _cachedCam == null → 다음 IsVisible 호출 시 새 Camera.main cache
- 새 씬의 적 sprite 정상 표시 (visible=true 시)

---

### S4 — Validator 룰 동작

**사전 조건**:
- T04 commit 후 (Validator 룰 추가)

**단계**:
1. EnemyController.cs 의 Update 첫 줄 IsVisible 호출 임시 제거 (테스트용)
2. `Tools/LittleHero/Validate Project` 실행
3. warning 확인
4. 복원 후 재실행 → warning 사라짐

**기대 결과**:
- warning 표시: `[Validator] EnemyController_Visibility_Check_Present: ...EnemyController.cs 의 Update 에 EnemyVisibilityCuller.IsVisible 호출 누락 (5013)`
- 복원 시 warning 사라짐

---

### S5 — EnemyAnimationManager HashSet 동작 (회귀 검증)

**사전 조건**:
- T03 commit 후 (List → HashSet)

**단계**:
1. CH 1 Wave 5 진입 (적 ~30)
2. AnimationTick 호출 빈도 확인 (Debug.Log 또는 Profiler)

**기대 결과**:
- 모든 visible 적 매 프레임 AnimationTick 호출
- off-screen 적 AnimationTick 호출 0
- HashSet.Add/Remove O(1) — 매 프레임 GC 0

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (5 commit)
- [ ] Editor Test 6/6 PASS (누적 87)
- [ ] ProjectValidator warnings 변동 ±0
- [ ] 5008 / 5007 / 5005 등 회귀 X
- [ ] 60fps 유지 (300 cap PIVOT v4 §7)
- [ ] GC <50KB/s
- [ ] EnemyAnimationManager HashSet 회귀 X (AnimationTick 호출 정상)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 5/6 이상 ✅
- C3 Validator errors=0 ✅
- C4 Reflection 5 Type 통과 ✅
- 시나리오 S1, S4 PASS

**완전 합격**:
- 모든 카테고리 ✅
- 시나리오 S1~S5 모두 PASS
- 비기능 요구사항 모두 ✅
- Profiler 60fps + GC <50KB/s 검증

## 9. Trade-off (mediator 결정 기반)

- **D1=A (Throttle 0.1s)** — 60fps 환경 6 frame 간격 사용자 인지 어려움. 옵션 B (매 프레임) 의 즉각 응답 손실 — margin 1.5x 로 보완.
- **D2=A (Margin 1.5x)** — 균형 (cap 300 환경 cull 200 적 = CPU 30% 절감). 옵션 B (1.2x) 의 cull 효과 ↑ 손실 — Sprint 5 Profiler 결과 부족 시 1.2x 재고 (5013.1 anchor).
- **Rebuttal accepted**: SceneTransition InvalidateCache + HashSet GC 0 + Validator 룰 + Profiler 측정 절차 — 정합 강화 + 사용자 검증 명확.

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract). 6 카테고리 검증 + 5 Play 시나리오.
