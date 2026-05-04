# 5013 — EnemyVisibilityCuller — Sprint PRD (v2 최종)

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5013_enemy_visibility_culler_v1_prd.md` · Rebuttal: `5013_enemy_visibility_culler_v1_rebuttal.md` · Decisions: `5013_enemy_visibility_culler_v1_decisions.md`
> 결정 2건 반영 (D1=A 0.1s / D2=A 1.5x — 그대로). 다음 단계: /prd-breakdown

> **출처**: `docs/prd/5013_enemy_visibility_culler_v1_prd.md` (디자인) — 옵션 B (Sprite + Animator + Manager 통합)
> **Rebuttal 반영**: SceneTransition InvalidateCache hook + HashSet GC 0 + Validator 룰 + Profiler 측정 절차 구체화

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | EnemyVisibilityCuller 정적 헬퍼 (~80L) + **SceneTransition InvalidateCache hook** | 1 | 30분 |
| 2 | EnemyController.Update hook + throttle 0.1s | 1 | 25분 |
| 3 | EnemyAnimationManager Register/Unregister 정합 (**HashSet 또는 직접 IsVisible 체크, GC 0**) | 1 | 20분 |
| 4 | NUnit 5+ + **ProjectValidator EnemyController_Visibility_Check_Present 룰** | 1 | 30분 |
| 5 | **Profiler 측정 절차 구체화** + docs | 1 | 25분 |
| **총** | | **5** | **약 2시간** |

## 2. Sprint 명세

### Sprint 1 — EnemyVisibilityCuller 정적 헬퍼 (Rebuttal accepted)

**파일**: `Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs` (~80L)

```csharp
public static class EnemyVisibilityCuller {
    private static Camera _cachedCam;
    public static bool IsVisible(Vector3 worldPos, float margin = 1.5f);
    public static void InvalidateCache();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init() {
        // 5004 SceneTransitionService.OnAfterSceneLoad → InvalidateCache 자동 hook (Rebuttal §3.3)
        SceneTransitionService.OnAfterSceneLoad += InvalidateCache;
    }
}
```

**예상 라인**: ~80L (§15.6 통과)

**검증**: 컴파일 + 정적 cache + Scene 변경 시 자동 invalidate

#### Sprint 1 commit
```
5013 Sprint 1 / T01 — EnemyVisibilityCuller 정적 헬퍼 + Scene hook (1/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs 신규 (~80L)
- IsVisible(Vector3, margin) viewport 변환 + 정적 cache
- SceneTransitionService.OnAfterSceneLoad → InvalidateCache 자동 hook (Rebuttal accepted)

[검증]
- 컴파일 errors=0, §15.6 통과

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — EnemyController.Update hook + throttle

**파일**: `EnemyController.cs` 수정

**변경**:
- Update 첫 줄 throttle (lastVisCheck < 0.1s skip)
- IsVisible false 시 SpriteRenderer.enabled false + early return

> **D1 mediator 결정 후 명세**: throttle = 0.1s (PRD) / 매 프레임 / 0.05s / 0.2s+margin / SerializeField

#### Sprint 2 commit
```
5013 Sprint 2 / T02 — Update hook + throttle (D1 mediator 결정) (2/5)

[변경]
- EnemyController.Update 첫 줄 visibility 체크 (D1 결정)
- off-screen 시 SpriteRenderer.enabled false + return

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 3 — EnemyAnimationManager 정합 (Rebuttal accepted, HashSet GC 0)

**변경**:
- Visibility false 시 EnemyAnimationManager.Unregister
- Visibility true 복원 시 Register
- **HashSet<EnemyController> 사용 (Add/Remove O(1)) 또는 List + 직접 IsVisible 체크**
- 매 프레임 List.Remove O(n) 회피 → §15.11 GC 정합

#### Sprint 3 commit
```
5013 Sprint 3 / T03 — EnemyAnimationManager Register/Unregister + HashSet (3/5)

[변경]
- EnemyController visibility 변동 시 Manager Register/Unregister
- HashSet<EnemyController> 또는 직접 IsVisible 체크 (List Remove O(n) 회피)

[검증]
- §15.11 GC 0 (300 적 환경 매 프레임 검증)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 4 — NUnit + Validator (Rebuttal accepted)

**파일**:
- `Tests/Editor/EnemyVisibilityCullerTests.cs` (5 NUnit)
- `Editor/ProjectValidator.cs` 수정 — `EnemyController_Visibility_Check_Present` 룰 추가

**Test 메서드**:
- `IsVisible_Origin_True`
- `IsVisible_FarOffscreen_False`
- `IsVisible_Margin_True`
- `InvalidateCache_NextCallReinit`
- `IsVisible_NoCamera_DefaultsTrue`
- `InvalidateCache_OnSceneTransition_Triggered` (Rebuttal §3.3)

**Validator 룰** (Rebuttal §3.1):
- 정적 분석: EnemyController.Update 첫 줄에 `EnemyVisibilityCuller.IsVisible(...)` 호출 검사
- 누락 시 warning

#### Sprint 4 commit
```
5013 Sprint 4 / T04 — NUnit 6 + Validator EnemyController_Visibility_Check_Present (4/5)

[변경]
- Tests/Editor/EnemyVisibilityCullerTests.cs (6 NUnit, +1 SceneTransition hook 검증)
- Editor/ProjectValidator.cs — EnemyController_Visibility_Check_Present 룰

[검증]
- 6/6 PASS, 누적 NUnit 81→87
- Validator 정적 분석 정합

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 5 — Profiler 측정 + docs (Rebuttal accepted, 측정 절차 구체화)

**측정 절차** (Rebuttal §3.2):
- **환경**: Unity Editor Play Mode + (해당 시) iPhone 12 빌드
- **시나리오**: cap 300 wave 환경 (test scene 또는 wave 30+ chapter)
- **도구**: Unity Profiler (CPU + Rendering 탭) / Xcode Instruments (iOS)
- **측정 항목**: frame time / GPU draw call / CPU Update 호출 빈도 / GC allocation/sec
- **비교**: cull 도입 전후 frame 평균 (1분 측정)

**기대**: GPU draw call -50%, CPU Update -30%, frame time < 16ms (60fps)

**docs**:
- changelog + CLAUDE.md §11/§12/§14

#### Sprint 5 commit
```
5013 Sprint 5 / T05 — Profiler 측정 + docs (5/5)

[변경]
- Profiler 결과 (GPU draw call -X%, CPU Update -Y%, frame time Z ms)
- changelog + CLAUDE.md §11/§12/§14

[검증]
- iPhone 12 60fps 유지 마진 ↑ (PIVOT v4 §7 cap 300)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
T01 (Culler + Scene hook) → T02 (Update hook) → T03 (Manager 정합) → T04 (NUnit + Validator) → T05 (Profiler + docs)
```

## 4. 검증 시나리오

### S1 — 화면 외 적 SpriteRenderer 비활성
1. CH 1 Wave 5 (적 ~50) 진입
2. 카메라를 한쪽으로 이동
3. 반대편 적 SpriteRenderer 비활성 확인
- **검증**: SpriteRenderer.enabled false, EnemyAnimationManager Unregister

### S2 — Profiler 60fps 유지 (Rebuttal accepted 측정 절차)
1. iPhone 12 또는 Editor cap 300 시나리오
2. Unity Profiler — frame time / draw call / GC
- **검증**: frame time < 16ms, GPU draw call -50%, GC <50KB/s

### S3 — Scene 전환 시 Camera 캐시 invalidate (Rebuttal accepted)
1. Attic → Chapter 진입
- **검증**: Culler.InvalidateCache 자동 호출, 신규 Camera 정상 cache

### S4 — Validator 룰
1. EnemyController.Update 첫 줄에서 IsVisible 호출 제거 (테스트)
2. `Tools/LittleHero/Validate Project` 실행
- **검증**: warning `EnemyController_Visibility_Check_Present` 표시

## 5. Definition of Done

- [ ] 5 commit + 컴파일 errors=0
- [ ] NUnit 6/6 PASS (누적 87)
- [ ] Validator 룰 추가 + 정상 동작
- [ ] Profiler 측정 — GPU draw call 절감, CPU Update 절감, GC <50KB/s
- [ ] 60fps 유지 (300 cap)
- [ ] **SceneTransition InvalidateCache 자동 hook 검증**

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Damage v2 충돌 영향 0 (OverlapCircle 기반) | sprite 비활성 X 충돌 layer 만 사용 |
| margin 1.5x — 진입 적 미리 활성 | **D2 mediator 결정 (1.5x / 1.2x / 2.0x / SerializeField)** |
| 카메라 전환 시 캐시 stale | **Rebuttal accepted: SceneTransitionService hook 자동 InvalidateCache** |
| Manager List Remove O(n) | **Rebuttal accepted: HashSet 또는 직접 IsVisible 체크** |

## 7. 변경 이력

- 2026-05-04: v1 작성.
- 2026-05-04: **v2 초안 (Rebuttal 단계)** — Accepted 4건 반영 (Scene hook + HashSet + Validator 룰 + Profiler 절차). Discuss 2건 (D1 throttle / D2 margin) → discussion_topics → mediator 결정 대기.
