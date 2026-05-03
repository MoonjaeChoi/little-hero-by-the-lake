# Sprint Contract: 5004 Scene Transition Async + Fade

> **PGE Planner — Phase 4 Contract**
> PRD v2 / Breakdown 7 태스크 / 생성일: 2026-05-04

## 1. C1 컴파일

- 7 commit 모두 errors=0 / refresh + 5~10초 대기
- **Pass/Fail**: ✅ 모든 commit errors=0 → PASS

## 2. C2 Editor Test (NUnit)

| Test 클래스 | 신규 메서드 | 위치 |
|---|---|---|
| `SceneTransitionTests` | 5+ | `Tests/Editor/SceneTransitionTests.cs` |

**신규 NUnit (5)**:
- `Transition_Default_NotNull`
- `Transition_NoOp_FadeOut_Completes`
- `Transition_NoOp_FadeIn_Completes`
- `ISceneTransition_HasFadeOutFadeIn`
- `DEFAULT_FADE_DURATION_Is_0_3`

**검증**: 신규 5 PASS + 기존 56 회귀 X (총 61).

## 3. C3 ProjectValidator

- errors=0 / warnings 변동 0 또는 미세 증가
- 신규 룰 — `SceneManager.LoadScene` 외부 직접 호출 검사:
  - allowedFiles = `SceneTransitionService.cs / ProjectValidator.cs` (D1 자동 수용 — GameManager / BossEncounterLauncher 도 일괄 치환)
  - 11 호출처 변경 후 0 위반 확인

## 4. C4 Reflection 검증

| Type | Member |
|---|---|
| `LittleHero.ISceneTransition` (interface) | `FadeOut(float)` / `FadeIn(float)` |
| `LittleHero.SceneTransitionService` (static) | `Default` property + `Transition(string, float)` 정적 + `DEFAULT_FADE_DURATION` const (0.3f) |
| `LittleHero.FadeOverlay` (MonoBehaviour) | `ISceneTransition` 구현 + `_blackImage` SerializeField |
| `LittleHero.SceneTransitionBootstrap` (static) | `RuntimeInitializeOnLoadMethod` 어트리뷰트 |

## 5. C5 SO + Resources 검증

- `Assets/_Project/Resources/UI/FadeOverlay.prefab` 존재
- prefab 구조: Canvas (sortingOrder=9999) + Image (black, alpha=0, raycastTarget=true)
- FadeOverlay._blackImage Inspector 드래그 wiring
- 5002/5003 SO/SaveData 회귀 X

## 6. C6 Play 시나리오

### S1 — Attic → Chapter 정상 전환
1. Attic 시작 → 챕터 1 클릭
2. **검증**: 0.3s fade out → Chapter 씬 로드 → 0.3s fade in. 콘솔 에러 0.

### S2 — 5002 회귀 (Chapter → BossEncounter → Attic)
1. wave 20 진입 → BossEncounter 씬 fade transition
2. 보스 처치 → Attic fade transition
3. **검증**: 5002 챕터 보스 흐름 회귀 X + fade 적용

### S3 — 5003 회귀 (wave 15 미니보스, BalanceConfig.miniBossSeparation = LastOnly)
1. wave 15 진입 → BossEncounter fade transition
2. 미니보스 처치 → Chapter 씬 fade transition + wave 16 재개
3. **검증**: 5003 옵션 B 흐름 회귀 X + fade 적용

### S4 — InfiniteTower 전환
1. Endgame 메뉴 → 무한 다락방 클릭
2. **검증**: fade transition 후 InfiniteTower 씬

### S5 — FadeOverlay prefab 누락 시 NoOp fallback
1. Resources/UI/FadeOverlay.prefab 임시 삭제 후 Play (또는 prefab 미존재 환경)
2. **검증**: 콘솔 LogWarning + 모든 씬 전환은 fade 없이 정상 동작 (NoOp default)

## 7. 비기능

- 컴파일 errors=0 (7 commit)
- NUnit 56 → 61
- 5002 BossSceneSeparation 8/8 + 5003 신규 2 = 10/10 회귀 X
- 60fps 유지 (LoadSceneAsync 메인 스레드 block 해소)

## 8. 합격 기준

- **최소**: 자동 (C1~C5) + S1+S2 PASS
- **완전**: 자동 (C1~C5) + S1~S5 모두 PASS

## 9. Trade-off

- **D1 = A 자동 수용**: GameManager.cs:82 일괄 치환 — Validator 룰 단순화
- **D2 = A 자동 수용**: BossEncounterLauncher 내부 변경 — 5002 통합, 이중 fade 회피
- **D3 = A 채택**: Sprint 2 commit 분리 유지 — 5002 패턴 답습

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract).
