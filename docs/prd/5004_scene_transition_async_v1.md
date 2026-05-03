# 5004 — Scene Transition Async + Fade v1

> **목적**: 모든 씬 전환을 `SceneManager.LoadScene` (sync) → `LoadSceneAsync` + 검은 fade in/out 으로 일괄 통합. 모바일 60fps drop 위험 감소 + UX 부드러움 강화.
> **번호**: 5004 / **선행**: 5002 (D4 mediator 결정 — "5004 별 최적화 sprint 일괄 적용")
> **CLAUDE.md §13 #9**: 5004 LoadSceneAsync + fade-in 통합 sprint — 모든 씬 전환 일관 적용.

## 0. 결정 이력

| 날짜 | 결정 | 출처 |
|---|---|---|
| 2026-05-04 | 5002 D4=B 결정: LoadSceneAsync 는 5004 별 최적화 sprint 로 분리 | 5002_decisions.md |
| 2026-05-04 | 본 5004 PRD 작성 | 본 문서 |

## 1. 현재 시스템 분석

### 1.1 SceneManager.LoadScene 호출 위치 (11곳)

| 파일 | 라인 | 전환 흐름 |
|---|---|---|
| `Core/GameManager.cs` | 82 | (보조 — 메인 흐름 외) |
| `Core/BossEncounterLauncher.cs` | 24 | Chapter→BossEncounter / Attic→BossEncounter |
| `Hub/AtticController.cs` | 369 | Attic→Chapter (HandleStart) |
| `Gameplay/Waves/BossEncounterRunner.cs` | 44 | BossEncounter→Attic (PendingBoss null fallback) |
| `Gameplay/Waves/BossEncounterRunner.cs` | 130 | BossEncounter→Attic / BossEncounter→Chapter (LoadSceneAfterDelay) |
| `Gameplay/Waves/InfiniteTowerRunner.cs` | 120 | InfiniteTower→Attic |
| `UI/GameOverPanel.cs` | 39 | Chapter→Attic (사망 후) |
| `UI/ChapterClearedPanel.cs` | 44 | Chapter→Attic (클리어 후) |
| `UI/ChapterPauseButton.cs` | 34 | Chapter→Attic (일시정지) |
| `UI/EndgameMenuPanel.cs` | 173 | Attic→InfiniteTower |
| `UI/WondrousMapPanel.cs` | 138 | Attic→Chapter (지도 선택) |

### 1.2 현재 흐름의 문제점

- **Sync LoadScene** = 메인 스레드 block. 큰 씬 (Chapter 26 SO + Background prefab) 로드 시 60fps drop 발생 가능
- **Fade 없음** = 갑작스러운 화면 전환 (UX 거칠음)
- **씬 전환 시점의 사용자 입력 처리** 모호 — input system 이 인터럽트 받는 경우 발생

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | LoadScene → LoadSceneAsync 단순 치환 (fade 없음) | 코드 변경 최소. async 만 적용 | UX 부드러움 X. fade 효과 없음 |
| **B** ★권장 | `SceneTransitionService` 정적 + `FadeOverlay` (DontDestroyOnLoad Canvas) — fade 0.3s out → LoadSceneAsync → fade 0.3s in | 모든 씬 전환 일관 + UX 부드러움. 단일 진실원 | 신규 파일 2 + UI prefab 1. 11곳 호출처 변경 |
| C | Cinemachine fade — 카메라 기반 페이드 | 카메라 통합 | UI Canvas overlay 가 더 일반적. Cinemachine 부담 |
| D | 개별 씬 전환마다 fade Coroutine 직접 작성 | 씬별 커스터마이징 가능 | 코드 중복 ❌ §15.1 SRP 위반 |

**★ 권장 = B** — 5002 의 BossEncounterLauncher 패턴 답습 (정적 service + interface). FadeOverlay 는 DontDestroyOnLoad Canvas 1개로 모든 씬 공유.

## 3. 권장 설계 (CLAUDE.md §15 정합)

### 3.1 신규 파일 명세

| 종류 | 경로 | 책임 | 라인 예상 |
|---|---|---|---|
| 신규 service | `Scripts/Core/SceneTransitionService.cs` | 정적 — TransitionTo(scene, fadeDuration) | ~60 |
| 신규 interface | `Scripts/Core/ISceneTransition.cs` | `IEnumerator FadeOut/FadeIn(float)` (테스트 mock 진입점, §15.7 DI) | ~15 |
| 신규 컴포넌트 | `Scripts/UI/FadeOverlay.cs` | DontDestroyOnLoad Canvas + Image alpha 애니메이션 | ~70 |
| 신규 prefab | `Prefabs/UI/FadeOverlay.prefab` | Canvas (Sort Order 9999) + Image (black, alpha 0) | — |
| 변경 11 | 위 §1.1 호출처 | `SceneManager.LoadScene(x)` → `SceneTransitionService.Transition(x)` | 각 1줄 |
| 변경 (Bootstrap) | `Scripts/Core/Bootstrap.cs` 또는 GameManager.Awake | FadeOverlay prefab 로드 + Initialize | +5 |

### 3.2 SceneTransitionService 시그니처

```csharp
public static class SceneTransitionService
{
    public static ISceneTransition Default { get; set; }   // mock 주입 가능 (§15.7 DI)
    private const float DEFAULT_FADE_DURATION = 0.3f;

    public static Coroutine Transition(string sceneName, float fadeDuration = DEFAULT_FADE_DURATION)
    {
        var runner = TransitionRunner.Get();    // 정적 MonoBehaviour helper
        return runner.StartCoroutine(TransitionRoutine(sceneName, fadeDuration));
    }

    private static IEnumerator TransitionRoutine(string sceneName, float fadeDuration)
    {
        Time.timeScale = 1f;
        yield return Default.FadeOut(fadeDuration);
        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;
        yield return Default.FadeIn(fadeDuration);
    }
}
```

### 3.3 FadeOverlay 컴포넌트

```csharp
public class FadeOverlay : MonoBehaviour, ISceneTransition
{
    [SerializeField] private Image _blackImage;
    private void Awake() {
        DontDestroyOnLoad(gameObject);
        SceneTransitionService.Default = this;
        _blackImage.color = new Color(0, 0, 0, 0);
    }
    public IEnumerator FadeOut(float duration) {
        float t = 0f;
        while (t < duration) { t += Time.unscaledDeltaTime;
            _blackImage.color = new Color(0, 0, 0, t / duration); yield return null; }
        _blackImage.color = Color.black;
    }
    public IEnumerator FadeIn(float duration) {
        float t = 0f;
        while (t < duration) { t += Time.unscaledDeltaTime;
            _blackImage.color = new Color(0, 0, 0, 1f - t / duration); yield return null; }
        _blackImage.color = new Color(0, 0, 0, 0);
    }
}
```

### 3.4 호출처 변경 패턴

**기존**:
```csharp
SceneManager.LoadScene("Attic");
```

**변경**:
```csharp
SceneTransitionService.Transition("Attic");
```

특수 케이스:
- `BossEncounterRunner.LoadSceneAfterDelay` — `WaitForSecondsRealtime(_returnDelay)` 후 호출 → 그대로 패턴 유지
- `BossEncounterLauncher.Launch` — `Time.timeScale = 1f` 직후 호출 → 그대로

### 3.5 FadeOverlay 부트스트랩 시점

신규 SaveData / 첫 씬 (Attic) 진입 시 FadeOverlay prefab 1회 인스턴스화. 이후 DontDestroyOnLoad 로 모든 씬 공유.

**옵션**:
- Bootstrap.cs 신규 (Resources.Load + Instantiate 1회) — RuntimeInitializeOnLoadMethod 활용
- 또는 GameManager.Awake 에서 처리 (기존 GameManager DontDestroyOnLoad 활용)

## 4. CLAUDE.md §15 정합 검증

| 정책 | 검증 |
|---|---|
| §15.1 SOLID-S | SceneTransitionService 단일 책임 (씬 전환 + fade) |
| §15.1 SOLID-O | ISceneTransition interface — FadeOverlay 외 다른 transition (e.g. wipe) 추가 가능 |
| §15.4 SO 우선 | DEFAULT_FADE_DURATION = 0.3f const (Magic Number 회피). 향후 BalanceConfig 이동 가능 |
| §15.5 EventBus | GameEvents 영향 X. 단순 정적 service |
| §15.6 100줄 룰 | SceneTransitionService ~60L, FadeOverlay ~70L (모두 100L 이내) ✅ |
| §15.7 DI | ISceneTransition.Default property 로 mock 주입 (§15.7 패턴, 5002 BossEncounterLauncher 답습) |
| §15.9 Test | NUnit — `Transition_Default_NotNull` / `Transition_StartsCoroutine` / `FadeOverlay_Initial_AlphaZero` (3+) |
| §15.11 안티 패턴 | Update X / Instantiate X (FadeOverlay 1회만, RuntimeInitializeOnLoadMethod) |
| §15.12 Validator | `SceneManager.LoadScene` 외부 직접 호출 검사 룰 추가 (BossEncounterLauncher / 5004 Service / GameManager 한정 허용) |

## 5. Sprint 분해 (개요)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | ISceneTransition + SceneTransitionService 정적 (코루틴 runner 포함) | 1 | 25분 |
| 2 | FadeOverlay 컴포넌트 + prefab (Resources/UI/FadeOverlay.prefab) | 2 | 35분 |
| 3 | 11 호출처 일괄 치환 (LoadScene → Transition) | 1 | 25분 |
| 4 | Bootstrap (FadeOverlay 자동 인스턴스화) | 1 | 15분 |
| 5 | NUnit 3+ + ProjectValidator `LoadScene` 외부 직접 호출 검사 룰 | 1 | 30분 |
| 6 | docs + changelog + CLAUDE.md §11/§13/§14 갱신 | 1 | 15분 |
| **총** | | **7** | **2시간 25분** |

자세한 명세는 → [5004/5004_scene_transition_async_v1_prd.md](5004/5004_scene_transition_async_v1_prd.md)

## 6. 알려진 위험 + 완화

| 위험 | 영향 | 완화 |
|---|---|---|
| FadeOverlay prefab 누락 시 fade 동작 X | UX 거침 | Bootstrap 자동 인스턴스화 + null check (no-op) |
| LoadSceneAsync 중 사용자 입력 | 의도치 않은 동작 | FadeOverlay sortingOrder 9999 + raycast block (Image.raycastTarget=true) |
| Time.timeScale = 0 (일시정지 중) 씬 전환 | fade Coroutine 안 동작 | `Time.unscaledDeltaTime` 사용 |
| 11곳 호출처 변경 시 회귀 | 5002/5003 흐름 깨질 가능성 | Sprint 3 일괄 치환 후 NUnit + Play 검증 강화 |
| Resources.Load 실패 (FadeOverlay prefab 누락) | Bootstrap fail | LogWarning + no-op fallback (5002 BackgroundApplier 패턴 답습) |
| MCP SIGSEGV (Sprint 2 prefab 작업) | Mono crash | Sprint 2 commit 분리 + 씬/prefab 저장 후 10초+ 대기 |

## 7. 변경 이력

- 2026-05-04: v1 초안 — 옵션 B 권장 (SceneTransitionService 정적 + FadeOverlay DontDestroyOnLoad)

## 8. Sources

- `docs/prd/5002/5002_*_decisions.md` (D4=B — 5004 후속 명시)
- 5002 BossEncounterLauncher 패턴 (정적 service + ISomething.Default mock)
- 코드: 11 호출처 (§1.1 표)
