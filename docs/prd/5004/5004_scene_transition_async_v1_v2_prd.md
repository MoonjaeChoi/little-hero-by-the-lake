# 5004 — Scene Transition Async + Fade v2 PRD (Mediator 확정)

> **PRD v2 (Mediator 확정)** / Decisions: `5004_*_decisions.md` (D3=A 권장 채택, 2026-05-04)
> 자동 수용 2건 (D1: GameManager 일괄 치환 / D2: BossEncounterLauncher 내부 변경) + D3=A (2 commit 분리)
>
> **번호**: 5004 / **출처**: `docs/prd/5004_scene_transition_async_v1.md` (디자인, 옵션 B)
> **목표**: 6 Sprint, 7 commit, 2시간 25분 (현 PRD 그대로)
> **선행**: 5002 D4=B 결정

> ### Rebuttal 자동 수용 (2건)
> - **D1**: GameManager.cs:82 일괄 치환 (allowedFiles = SceneTransitionService.cs / ProjectValidator.cs 만)
> - **D2**: BossEncounterLauncher.Launch 내부 SceneTransitionService.Transition 호출 (5002 통합)
>
> ### Mediator 결정 (1건)
> - **D3 = A**: Sprint 2 commit 분리 유지 (2 commit) — 5002 패턴 답습

## 0. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | ISceneTransition interface + SceneTransitionService 정적 service | 1 | 25분 |
| 2 | FadeOverlay 컴포넌트 (~70L) + Resources/UI/FadeOverlay.prefab | 2 | 35분 |
| 3 | 11 호출처 LoadScene → SceneTransitionService.Transition 일괄 치환 | 1 | 25분 |
| 4 | Bootstrap — RuntimeInitializeOnLoadMethod 로 FadeOverlay 자동 인스턴스화 | 1 | 15분 |
| 5 | NUnit 3+ + ProjectValidator §15.7 룰 (`LoadScene` 외부 직접 호출 검사) | 1 | 30분 |
| 6 | docs + changelog + CLAUDE.md §11/§12/§13/§14 갱신 | 1 | 15분 |
| **총** | | **7** | **2시간 25분** |

## 의존성 그래프

```
Sprint 1 (Service+Interface) ──┐
                               ├─→ Sprint 3 (호출처 치환) ──┐
Sprint 2 (FadeOverlay+prefab) ─┘                          ├─→ Sprint 5 (Test+Validator) → Sprint 6 (docs)
                                                          │
                              Sprint 4 (Bootstrap) ───────┘
```

병렬 가능: Sprint 1 ↔ Sprint 2 (서로 독립). 나머지 순차.

---

## Sprint 1 — Service + Interface (1 commit, 25분)

### 1.1 ISceneTransition

**파일**: `Scripts/Core/ISceneTransition.cs` (신규 ~15L)

```csharp
using System.Collections;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 씬 전환 fade 인터페이스 (§15.7 DI). FadeOverlay 가 기본 구현.
    /// </summary>
    public interface ISceneTransition
    {
        IEnumerator FadeOut(float duration);
        IEnumerator FadeIn(float duration);
    }
}
```

### 1.2 SceneTransitionService

**파일**: `Scripts/Core/SceneTransitionService.cs` (신규 ~60L)

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 모든 씬 전환 통합 service. LoadSceneAsync + fade-in/out (§15.1 SRP, §15.7 DI).
    /// </summary>
    public static class SceneTransitionService
    {
        public static ISceneTransition Default { get; set; } = new NoOpTransition();
        public const float DEFAULT_FADE_DURATION = 0.3f;

        public static void Transition(string sceneName, float fadeDuration = DEFAULT_FADE_DURATION)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            var runner = TransitionCoroutineRunner.GetOrCreate();
            runner.StartCoroutine(TransitionRoutine(sceneName, fadeDuration));
        }

        private static IEnumerator TransitionRoutine(string sceneName, float fadeDuration)
        {
            Time.timeScale = 1f;
            yield return Default.FadeOut(fadeDuration);
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op != null) while (!op.isDone) yield return null;
            yield return Default.FadeIn(fadeDuration);
        }

        private class NoOpTransition : ISceneTransition
        {
            public IEnumerator FadeOut(float d) { yield break; }
            public IEnumerator FadeIn(float d) { yield break; }
        }
    }

    internal class TransitionCoroutineRunner : MonoBehaviour
    {
        private static TransitionCoroutineRunner _instance;
        public static TransitionCoroutineRunner GetOrCreate()
        {
            if (_instance == null)
            {
                var go = new GameObject("[SceneTransitionRunner]");
                _instance = go.AddComponent<TransitionCoroutineRunner>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
}
```

### 1.3 검증 기준
- 컴파일 OK
- 기존 11 호출처 변경 X (Sprint 3 까지 NoOpTransition default — 회귀 X)
- §15.6 100줄 룰 통과 (~60L + ~15L)

#### Sprint 1 commit
```
5004 Sprint 1 — ISceneTransition + SceneTransitionService 정적 (1/7)

[변경]
- Scripts/Core/ISceneTransition.cs 신규 (interface 2 메서드)
- Scripts/Core/SceneTransitionService.cs 신규 (정적 service + NoOp default + TransitionCoroutineRunner)

[검증]
- 컴파일 OK
- Default = NoOpTransition (Sprint 3 호출처 치환 시 안전)
- §15.7 DI 진입점 (Default mock 주입 가능)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 2 — FadeOverlay 컴포넌트 + prefab (2 commit, 35분)

### 2.1 FadeOverlay 컴포넌트

**파일**: `Scripts/UI/FadeOverlay.cs` (신규 ~70L)

```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5004 — 검은 fade overlay (DontDestroyOnLoad Canvas). ISceneTransition 구현.
    /// Resources/UI/FadeOverlay.prefab 으로 RuntimeInitializeOnLoadMethod 자동 로드.
    /// </summary>
    public class FadeOverlay : MonoBehaviour, ISceneTransition
    {
        [SerializeField] private Image _blackImage;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            SceneTransitionService.Default = this;
            if (_blackImage != null)
                _blackImage.color = new Color(0, 0, 0, 0);
        }

        public IEnumerator FadeOut(float duration)
        {
            if (_blackImage == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _blackImage.color = new Color(0, 0, 0, Mathf.Clamp01(t / duration));
                yield return null;
            }
            _blackImage.color = Color.black;
        }

        public IEnumerator FadeIn(float duration)
        {
            if (_blackImage == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _blackImage.color = new Color(0, 0, 0, 1f - Mathf.Clamp01(t / duration));
                yield return null;
            }
            _blackImage.color = new Color(0, 0, 0, 0);
        }
    }
}
```

### 2.2 Prefab 작성

**파일**: `Assets/_Project/Resources/UI/FadeOverlay.prefab`

**구조**:
```
FadeOverlay (Canvas, sortingOrder=9999, FadeOverlay 컴포넌트)
└── BlackImage (Image, color=black, alpha=0, raycastTarget=true)
    Anchor: stretch full screen
```

**Inspector wiring**:
- FadeOverlay._blackImage = BlackImage 의 Image 컴포넌트

### 2.3 검증 기준
- 컴파일 OK + prefab 정상 저장
- Image alpha=0 default
- Awake 에서 SceneTransitionService.Default = this 자동 set

#### Sprint 2 commit (2 commit)
```
5004 Sprint 2.1 — FadeOverlay 컴포넌트 (2/7)

[변경]
- Scripts/UI/FadeOverlay.cs 신규 (~70L, ISceneTransition 구현)

[검증]
- 컴파일 OK
- §15.6 100L 이내

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

```
5004 Sprint 2.2 — FadeOverlay.prefab + Inspector wiring (3/7)

[변경]
- Assets/_Project/Resources/UI/FadeOverlay.prefab 신규
- Canvas (sortingOrder=9999) + Image (black, alpha=0, raycastTarget)
- FadeOverlay._blackImage Inspector 드래그

[검증]
- 씬 저장 후 10초 대기 (MCP SIGSEGV 회피)
- prefab Resources.Load 가능 (Sprint 4 검증)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 3 — 11 호출처 LoadScene → Transition 일괄 치환 (1 commit, 25분)

### 3.1 일괄 치환 대상

| # | 파일 | 변경 |
|---|---|---|
| 1 | `Scripts/Core/BossEncounterLauncher.cs:24` | `SceneManager.LoadScene("BossEncounter")` → `SceneTransitionService.Transition("BossEncounter")` |
| 2 | `Scripts/Hub/AtticController.cs:369` | Attic→Chapter |
| 3 | `Scripts/Gameplay/Waves/BossEncounterRunner.cs:44` | (PendingBoss null) BossEncounter→Attic |
| 4 | `Scripts/Gameplay/Waves/BossEncounterRunner.cs:130` | LoadSceneAfterDelay 내부 |
| 5 | `Scripts/Gameplay/Waves/InfiniteTowerRunner.cs:120` | InfiniteTower→Attic |
| 6 | `Scripts/UI/GameOverPanel.cs:39` | Chapter→Attic (사망) |
| 7 | `Scripts/UI/ChapterClearedPanel.cs:44` | Chapter→Attic (클리어) |
| 8 | `Scripts/UI/ChapterPauseButton.cs:34` | Chapter→Attic (일시정지) |
| 9 | `Scripts/UI/EndgameMenuPanel.cs:173` | Attic→InfiniteTower |
| 10 | `Scripts/UI/WondrousMapPanel.cs:138` | Attic→Chapter |
| 11 | `Scripts/Core/GameManager.cs:82` | (보조 — 검토 후 치환) |

**예외**: `BossEncounterRunner.cs:130` 의 `LoadSceneAfterDelay` 는 이미 코루틴 — 내부 `SceneManager.LoadScene` 만 `SceneTransitionService.Transition` 으로 변경.

### 3.2 검증 기준
- 컴파일 OK
- Sprint 4 Bootstrap 미적용 시 Default = NoOpTransition → fade 없이 LoadSceneAsync 만 동작 (회귀 X)
- 5002 BossSceneSeparation 8/8 + 5003 신규 2 = NUnit 10/10 회귀 X

#### Sprint 3 commit
```
5004 Sprint 3 — 11 호출처 LoadScene → SceneTransitionService.Transition (4/7)

[변경]
- 11 파일에서 SceneManager.LoadScene → SceneTransitionService.Transition 일괄 치환

[검증]
- 컴파일 errors=0
- Sprint 4 Bootstrap 미적용 시 NoOp default 로 안전 (LoadSceneAsync 만 동작)
- 5002/5003 NUnit 10/10 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 4 — Bootstrap FadeOverlay 자동 인스턴스화 (1 commit, 15분)

### 4.1 RuntimeInitializeOnLoadMethod

**파일**: `Scripts/Core/SceneTransitionBootstrap.cs` (신규 ~25L)

```csharp
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5004 — FadeOverlay prefab 자동 인스턴스화 (RuntimeInitializeOnLoadMethod).
    /// Resources/UI/FadeOverlay.prefab 1회 로드 + DontDestroyOnLoad.
    /// </summary>
    public static class SceneTransitionBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            var prefab = Resources.Load<GameObject>("UI/FadeOverlay");
            if (prefab == null)
            {
                Debug.LogWarning("[SceneTransitionBootstrap] Resources/UI/FadeOverlay.prefab 미존재 — fade 없이 동작 (NoOp default)");
                return;
            }
            Object.Instantiate(prefab);  // FadeOverlay.Awake 가 DontDestroyOnLoad + Default 설정
        }
    }
}
```

### 4.2 검증 기준
- 첫 씬 진입 시 FadeOverlay 자동 인스턴스화
- DontDestroyOnLoad 적용 — 씬 전환 후 Persist
- prefab 미존재 시 LogWarning + NoOp fallback

#### Sprint 4 commit
```
5004 Sprint 4 — Bootstrap RuntimeInitializeOnLoadMethod (5/7)

[변경]
- Scripts/Core/SceneTransitionBootstrap.cs 신규
- BeforeSceneLoad 에서 Resources/UI/FadeOverlay.prefab 1회 인스턴스화

[검증]
- 첫 씬 진입 시 FadeOverlay GameObject 활성
- prefab 미존재 시 NoOp fallback (LogWarning)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 5 — NUnit + ProjectValidator 룰 (1 commit, 30분)

### 5.1 SceneTransitionTests.cs

**파일**: `Tests/Editor/SceneTransitionTests.cs` (신규 ~80L)

```csharp
using NUnit.Framework;

namespace LittleHero.Tests
{
    public class SceneTransitionTests
    {
        [Test]
        public void Transition_Default_NotNull()
        {
            Assert.NotNull(SceneTransitionService.Default);
        }

        [Test]
        public void Transition_NoOp_FadeOut_Completes()
        {
            var noOp = new NoOpHelper();
            Assert.DoesNotThrow(() => { var e = noOp.FadeOut(0.3f); while (e.MoveNext()) {} });
        }

        [Test]
        public void Transition_NoOp_FadeIn_Completes()
        {
            var noOp = new NoOpHelper();
            Assert.DoesNotThrow(() => { var e = noOp.FadeIn(0.3f); while (e.MoveNext()) {} });
        }

        [Test]
        public void ISceneTransition_HasFadeOutFadeIn()
        {
            var t = typeof(ISceneTransition);
            Assert.NotNull(t.GetMethod("FadeOut"));
            Assert.NotNull(t.GetMethod("FadeIn"));
        }

        [Test]
        public void DEFAULT_FADE_DURATION_Is_0_3()
        {
            Assert.AreEqual(0.3f, SceneTransitionService.DEFAULT_FADE_DURATION, 0.001f);
        }

        private class NoOpHelper : ISceneTransition
        {
            public System.Collections.IEnumerator FadeOut(float d) { yield break; }
            public System.Collections.IEnumerator FadeIn(float d) { yield break; }
        }
    }
}
```

### 5.2 ProjectValidator 룰 신규

**파일**: `Scripts/Editor/ProjectValidator.cs` (변경)

`SceneManager.LoadScene` 외부 직접 호출 검사 — 허용 파일: `SceneTransitionService.cs` / `BossEncounterLauncher.cs` / `GameManager.cs` (Bootstrap 보조).

```csharp
private static void ValidateSceneManagerDirectAccess(List<Issue> issues)
{
    var allowedFiles = new HashSet<string>
    {
        "SceneTransitionService.cs",
        "BossEncounterLauncher.cs",
        "GameManager.cs",
        "ProjectValidator.cs",
    };
    string scriptDir = Path.Combine(Application.dataPath, "_Project/Scripts");
    int violations = 0;
    foreach (var file in Directory.GetFiles(scriptDir, "*.cs", SearchOption.AllDirectories))
    {
        string filename = Path.GetFileName(file);
        if (allowedFiles.Contains(filename)) continue;
        string content = File.ReadAllText(file);
        if (content.Contains("SceneManager.LoadScene("))
        {
            issues.Add(new Issue(Severity.Warning, "5004 §15.7",
                $"SceneManager.LoadScene 외부 직접 호출: {filename} — SceneTransitionService.Transition 권장"));
            violations++;
        }
    }
    if (violations == 0)
        Debug.Log("[ProjectValidator] 5004 §15.7 OK — SceneManager.LoadScene 외부 직접 호출 0건");
}
```

### 5.3 검증 기준
- NUnit 5+ PASS
- Validator 신규 룰 — 11 호출처 변경 후 0 위반

#### Sprint 5 commit
```
5004 Sprint 5 — NUnit 5+ + ProjectValidator §15.7 룰 (6/7)

[변경]
- Tests/Editor/SceneTransitionTests.cs 신규 (~80L, NUnit 5)
- Editor/ProjectValidator.cs — SceneManager.LoadScene 외부 호출 검사 룰

[검증]
- NUnit 56 → 61 (BossSceneSeparation 10 + SceneTransition 5 + 기존 41)
- Validator 신규 룰 0 위반 (11 호출처 변경 후)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 6 — docs + changelog + CLAUDE.md (1 commit, 15분)

### 6.1 changelog_v1.md

```
- 2026-XX-XX: **5004 scene_transition_async_v1** — 모든 씬 전환 LoadSceneAsync + fade-in/out 통합 (옵션 B). 6 Sprint 7 commit. SceneTransitionService 정적 + ISceneTransition + FadeOverlay (DontDestroyOnLoad Canvas) + Bootstrap RuntimeInitializeOnLoadMethod. 11 호출처 일괄 치환. NUnit 56→61. ProjectValidator SceneManager.LoadScene 외부 호출 검사 룰 신규.
```

### 6.2 CLAUDE.md
- §11 마일스톤: `scene_transition_async_v1` 행 추가
- §12 Tests: 56 → 61 (SceneTransition 5 신규)
- §13: 5004 ✅ 표시
- §14: 5건 회전

#### Sprint 6 commit
```
5004 Sprint 6 — docs + changelog + CLAUDE.md §11/§12/§13/§14 (7/7)

[변경]
- docs/little_hero/changelog_v1.md — 5004 항목 등재
- CLAUDE.md §11 마일스톤 + §12 Tests 56→61 + §13 ✅ + §14 회전

[검증]
- 마크다운 lint X / 5002/5003 패턴 일관

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 검증 시나리오 (Play 검증)

### S1 — Attic → Chapter 전환 (정상)
1. Attic 시작 → 챕터 1 클릭
2. **검증**: 0.3s fade out → Chapter 씬 로드 → 0.3s fade in. 콘솔 에러 0.

### S2 — Chapter → BossEncounter → Attic (5002 회귀)
1. wave 20 진입
2. **검증**: 챕터→보스 fade transition / 보스 처치 후 Attic fade transition

### S3 — wave 15 미니보스 (5003 옵션 B 회귀)
1. BalanceConfig.miniBossSeparation = LastOnly
2. wave 15 진입 → BossEncounter fade
3. 미니보스 처치 → Chapter 씬 fade transition + wave 16 재개

### S4 — InfiniteTower 전환
1. Endgame 메뉴 → 무한 다락방 클릭
2. **검증**: fade transition 후 InfiniteTower 씬

### S5 — FadeOverlay prefab 누락 시
1. Resources/UI/FadeOverlay.prefab 임시 삭제 후 Play
2. **검증**: 콘솔 LogWarning + 모든 씬 전환은 fade 없이 (NoOp) 정상 동작

---

## 완료 정의 (Definition of Done)

- [ ] 7 commit 모두 push
- [ ] 컴파일 errors=0
- [ ] NUnit 61 PASS (5 신규 + 56 기존)
- [ ] Validator 신규 룰 0 위반
- [ ] Play S1+S2 PASS (회귀 검증)
- [ ] FadeOverlay prefab Resources/UI/ 에 wiring

---

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| FadeOverlay prefab 누락 | NoOp fallback + LogWarning |
| 11곳 호출처 변경 회귀 | Sprint 3 일괄 commit + NUnit 회귀 |
| Time.timeScale=0 일시정지 중 fade | Time.unscaledDeltaTime 사용 |
| LoadSceneAsync 중 사용자 입력 | FadeOverlay raycastTarget=true (입력 block) |
| MCP SIGSEGV (prefab 작업) | Sprint 2 commit 분리 + 10초+ 대기 |

---

## Source

- 디자인: `docs/prd/5004_scene_transition_async_v1.md`
- 5002 D4=B mediator 결정
- 5002 BossEncounterLauncher 패턴 (정적 service + Default mock)
- 11 호출처 grep 결과
