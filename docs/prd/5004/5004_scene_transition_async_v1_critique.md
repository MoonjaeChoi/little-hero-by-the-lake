# 5004 Scene Transition Async — PRD 리뷰

## 0. 결정 이력
- 2026-05-04: prd-critiquer 1차 리뷰 (Phase 2.1)
- 입력: 5004_v1.md (디자인) + 5004_v1_prd.md (옵션 B 채택, 6 Sprint × 7 commit)

## 1. 종합 점수: 9/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 정적 service + interface, FadeOverlay 70L, 5002 BossEncounterLauncher 패턴 답습 |
| PG-12 톤 | 10/10 | 메커니즘 — 톤 영향 0 |
| 성능 | 10/10 | LoadSceneAsync 메인 스레드 block 해소, fade 0.3s 미미 |
| 시스템 통합 | 9/10 | 5002 회귀 검증 명시. ⚠️ 11 호출처 일괄 치환 회귀 위험 |
| Sprint 분해 | 8/10 | 6 Sprint × 7 commit 균형. ⚠️ Sprint 2 prefab 작업 + Sprint 3 일괄 치환 의존 명확화 필요 |

## 2. Accept 항목 (8)

- ✅ **5002 BossEncounterLauncher 패턴 답습** — 정적 service + Default property mock 주입 (§15.7 DI 검증 사례)
- ✅ **NoOpTransition fallback** — FadeOverlay prefab 누락 시 LoadSceneAsync 만 동작 (회귀 안전)
- ✅ **DontDestroyOnLoad Canvas** — 한 번 인스턴스화 후 모든 씬 공유 (§15.11 Instantiate in Update 회피)
- ✅ **Time.unscaledDeltaTime** — Time.timeScale=0 일시정지 중에도 fade 동작
- ✅ **raycastTarget=true** — 씬 전환 중 사용자 입력 block (의도치 않은 동작 회피)
- ✅ **RuntimeInitializeOnLoadMethod Bootstrap** — BeforeSceneLoad 자동 인스턴스화 (수동 wiring 회피)
- ✅ **NUnit 5+ 명시** — Default mock + Coroutine NoOp + Reflection (§15.9)
- ✅ **ProjectValidator §15.7 룰 신규** — `SceneManager.LoadScene` 외부 직접 호출 검사 (5002 룰 패턴 답습)

## 3. Reject 항목 (0)

(없음)

## 4. Discuss 항목 (3) 🟡

### D1 — GameManager.cs:82 의 LoadScene 처리

**관찰**: `Scripts/Core/GameManager.cs:82` 의 LoadScene 호출은 보조 흐름 (메인 흐름 외). 일괄 치환 11 곳 중 1.

**옵션**:
- **A** ★권장: 일괄 치환 (11 곳 모두 SceneTransitionService.Transition)
- **B**: 보조 흐름이라 직접 호출 유지 (Validator 룰 allowedFiles 에 GameManager.cs 추가) — 5004 PRD 의 default
- **C**: GameManager.cs:82 의 호출 맥락 검토 후 결정 (사용자 결정)

**권장 사유 (A)**: 일관성 — 모든 씬 전환에 fade 적용. Validator 룰 단순화 (allowedFiles = SceneTransitionService.cs 만).

### D2 — BossEncounterLauncher.Launch 내부 SceneManager.LoadScene

**관찰**: `BossEncounterLauncher.Launch` (5002) 가 내부적으로 `SceneManager.LoadScene("BossEncounter")` 호출. 5004 적용 시 변경 필요.

**옵션**:
- **A** ★권장: BossEncounterLauncher.Launch 내부도 `SceneTransitionService.Transition` 으로 변경 (Phase 5 / Sprint 3 에서 처리)
- **B**: BossEncounterLauncher.Launch 그대로 유지 → fade 두 번 발생 가능 (Launch 호출 시 + transition 자체)
- **C**: BossEncounterLauncher 가 SceneTransitionService 를 명시 호출 (DI 주입 패턴)

**권장 사유 (A)**: 5002 인프라 와 5004 통합. 이중 fade 위험 제거.

### D3 — Sprint 2 의 prefab 작업 (2 commit) 분해

**관찰**: Sprint 2 = 컴포넌트 (1 commit) + prefab (1 commit). 합쳐서 1 commit 도 가능?

**옵션**:
- **A** ★권장: 2 commit 그대로 유지 (PRD 명세) — 컴포넌트와 prefab wiring 분리, MCP SIGSEGV 위험 회피
- **B**: 1 commit 통합 (작업 단위 작은 시스템)

**권장 사유 (A)**: 5002 의 Sprint 3.1+3.2 분리 패턴 답습. 씬/prefab 변경 후 10초+ 대기 안전.

## 5. CLAUDE.md §15 정책 위반 점검

| §15.x | 항목 | 결과 |
|---|---|---|
| §15.1 SOLID-S | SceneTransitionService 단일 책임 | ✅ |
| §15.1 SOLID-D | ISceneTransition interface | ✅ |
| §15.4 SO 우선 | DEFAULT_FADE_DURATION = 0.3f const | ✅ |
| §15.6 100줄 | Service 60L / FadeOverlay 70L | ✅ |
| §15.7 DI | Default property mock 주입 | ✅ |
| §15.9 Test | NUnit 5+ | ✅ |
| §15.11 안티 | public 필드 X / Instantiate 1회 | ✅ |
| §15.12 Validator | LoadScene 외부 직접 호출 룰 신규 | ✅ |

## 6. 권장 변경

### 즉시 변경 (rebuttal 자동 수용)

(없음 — Discuss 모두 사용자 결정 사안)

### 사용자 결정 후 변경 (mediator)

- D1: GameManager.cs:82 일괄 치환 여부
- D2: BossEncounterLauncher.Launch 내부 변경 여부
- D3: Sprint 2 commit 분리 여부

## 7. 변경 이력

- 2026-05-04: 초기 리뷰 (Phase 2.1) — 종합 9/10 / Accept 8 / Reject 0 / Discuss 3
