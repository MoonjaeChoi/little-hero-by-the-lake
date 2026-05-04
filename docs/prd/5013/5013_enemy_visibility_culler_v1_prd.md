# 5013 — EnemyVisibilityCuller — Sprint PRD

> **출처**: `docs/prd/5013_enemy_visibility_culler_v1_prd.md` (디자인) — 옵션 B (Sprite + Animator + Manager 통합)

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | EnemyVisibilityCuller 정적 헬퍼 (~80L) | 1 | 25분 |
| 2 | EnemyController.Update hook + throttle 0.1s | 1 | 25분 |
| 3 | EnemyAnimationManager Register/Unregister 정합 | 1 | 20분 |
| 4 | NUnit 5+ + Validator 룰 | 1 | 30분 |
| 5 | Profiler 측정 + docs | 1 | 25분 |
| **총** | | **5** | **약 2시간** |

## 2. Sprint 명세

### Sprint 1 — EnemyVisibilityCuller 정적 헬퍼

**파일**: `Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs` (~80L)

```csharp
public static class EnemyVisibilityCuller {
    private static Camera _cachedCam;
    public static bool IsVisible(Vector3 worldPos, float margin = 1.5f);
    public static void InvalidateCache(); // scene change
}
```

**검증**: 컴파일 + 정적 cache (PlayerFinder 패턴)

#### Sprint 1 commit
```
5013 Sprint 1 / T01 — EnemyVisibilityCuller 정적 헬퍼 (1/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyVisibilityCuller.cs 신규 (~80L)
- IsVisible(Vector3, margin) viewport 변환 + 정적 cache

[검증]
- 컴파일 errors=0, §15.6 통과

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — EnemyController.Update hook + throttle

**파일**: `EnemyController.cs` 수정

**변경**:
- Update 첫 줄 throttle (lastVisCheck < 0.1s skip)
- IsVisible false 시 SpriteRenderer.enabled false + early return

#### Sprint 2 commit
```
5013 Sprint 2 / T02 — Update hook + 0.1s throttle (2/5)

[변경]
- EnemyController.Update 첫 줄 visibility 체크 (0.1s throttle)
- off-screen 시 SpriteRenderer.enabled false + return

[검증]
- 컴파일 errors=0
- 화면 외 적 SpriteRenderer 비활성

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 3 — EnemyAnimationManager 정합

**변경**:
- Visibility false 시 EnemyAnimationManager.Unregister
- Visibility true 복원 시 Register
- 단일 Manager Update 의 List 에서 cull 제거 → CPU 절감

#### Sprint 3 commit
```
5013 Sprint 3 / T03 — EnemyAnimationManager Register/Unregister 정합 (3/5)

[변경]
- EnemyController visibility 변동 시 Manager Register/Unregister
- 단일 Manager List 에서 off-screen 적 제외

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 4 — NUnit + Validator

**파일**: `Tests/Editor/EnemyVisibilityCullerTests.cs`

**Test**:
- `IsVisible_Origin_True`
- `IsVisible_FarOffscreen_False`
- `IsVisible_Margin_True` (margin 영역 내 visible)
- `InvalidateCache_NextCallReinit`
- `IsVisible_NoCamera_DefaultsTrue` (안전 fallback)

#### Sprint 4 commit
```
5013 Sprint 4 / T04 — EnemyVisibilityCullerTests 5 NUnit (4/5)

[변경]
- Tests/Editor/EnemyVisibilityCullerTests.cs (5 NUnit)

[검증]
- 5/5 PASS, 누적 NUnit 86→91

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 5 — Profiler + docs

**작업**:
- iPhone 12 (또는 Editor) Profiler 측정 — 300 cap 환경 frame time 비교
- 기대: GPU draw call -50%, CPU Update -30%
- changelog + CLAUDE.md §11/§12/§14

#### Sprint 5 commit
```
5013 Sprint 5 / T05 — Profiler 측정 + docs (5/5)

[변경]
- changelog + CLAUDE.md
- Profiler 결과 (GPU draw call -X%, CPU Update -Y%)

[검증]
- iPhone 12 60fps 유지 마진 ↑

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
T01 → T02 → T03 → T04 → T05
```

## 4. 검증 시나리오

### S1 — 화면 외 적 SpriteRenderer 비활성
1. CH 1 Wave 5 (적 ~50) 진입
2. 카메라를 한쪽으로 이동
3. 반대편 적 SpriteRenderer 비활성 확인 (Inspector)
- **검증**: SpriteRenderer.enabled false, EnemyAnimationManager 등록 해제

### S2 — Profiler 60fps 유지
1. iPhone 12 또는 Editor Play
2. cap 300 환경 진입 (테스트 시나리오)
- **검증**: frame time < 16ms (60fps)

## 5. Definition of Done

- [ ] 5 commit + 컴파일 errors=0
- [ ] NUnit 5/5 PASS (누적 91)
- [ ] Profiler 측정 — GPU draw call 절감, CPU Update 절감
- [ ] 60fps 유지 (300 cap)

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Damage v2 충돌 영향 0 (OverlapCircle 기반) | sprite 비활성 X 충돌 layer 만 사용 |
| margin 1.5x — 진입 적 미리 활성 | 0.1s throttle 으로 빈도 조절 |
| 카메라 전환 시 캐시 stale | InvalidateCache 호출 (씬 변경 hook) |

## 7. 변경 이력

- 2026-05-04: v1 작성.
