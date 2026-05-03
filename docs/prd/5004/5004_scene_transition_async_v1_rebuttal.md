# 5004 Scene Transition Async — PRD Rebuttal

> **Phase 2.2 Rebuttal** / 일시: 2026-05-04
> 입력: `5004_*_critique.md` (9/10, Accept 8 / Reject 0 / Discuss 3)

## 1. 요약 표

| ID | Category | Criticism | Priority | Status | §15 Aligned |
|---|---|---|---|---|---|
| D1 | 일관성 | GameManager.cs:82 일괄 치환 여부 | Medium | **Accepted** (옵션 A 자동) | ✅ |
| D2 | 5002 통합 | BossEncounterLauncher 내부 변경 | High | **Accepted** (옵션 A 자동) | ✅ |
| D3 | Sprint 분해 | Sprint 2 commit 분리 | Low | **Discuss** (사용자 결정) | △ |

## 2. 품질 메트릭

- 총 Discussion Points: 3
- **Accepted**: 2 (67%) — D1, D2 (일관성 + 5002 통합 강화)
- **Discuss**: 1 (33%) — D3 (Sprint 작업 단위)
- **Rejected**: 0
- §15 정합률: 100%

## 3. Detailed Responses

### 3.1 D1 — GameManager.cs:82 일괄 치환 (Medium / **Accepted**)

**Original Criticism**: 보조 흐름이지만 일관성을 위해 11 곳 모두 치환 권장.

**Rebuttal**: **자동 수용 (옵션 A)**.
- 근거: 일관성 우선. Validator 룰 단순화 (allowedFiles = SceneTransitionService.cs 만).
- 모든 씬 전환에 fade 적용 — UX 일관

**PRD v2 영향**: Sprint 3 일괄 치환 11 곳 그대로 (현 PRD 의 default). Validator 룰 (Sprint 5) 의 allowedFiles 단순화 — `SceneTransitionService.cs / ProjectValidator.cs` 만 (BossEncounterLauncher.cs / GameManager.cs 제외).

### 3.2 D2 — BossEncounterLauncher 내부 변경 (High / **Accepted**)

**Original Criticism**: 5002 BossEncounterLauncher.Launch 가 내부적으로 SceneManager.LoadScene 호출. 5004 변경 시 이중 fade 위험.

**Rebuttal**: **자동 수용 (옵션 A)**.
- 근거: 5002 와 5004 통합. 이중 fade 위험 제거.
- BossEncounterLauncher.Launch 내부의 `SceneManager.LoadScene("BossEncounter")` → `SceneTransitionService.Transition("BossEncounter")` 변경
- 5002 호출 흐름 (EndgameMenuPanel → Launcher → BossEncounter 씬) 에 fade 자동 적용

**PRD v2 영향**: Sprint 3 일괄 치환 11 곳 — BossEncounterLauncher.cs 도 포함 (현 PRD 명시). Validator 룰의 allowedFiles 에서 BossEncounterLauncher.cs 제거 (Service 내부 직접 호출 X).

### 3.3 D3 — Sprint 2 commit 분리 (Low / **Discuss**)

**Status**: discussion_topics.md 로 이동.

**이유**: 작업 단위 결정 (사용자 권한). 5002 의 Sprint 3.1+3.2 분리 패턴 답습 ★권장.

**Resolution Path**: mediator 단계 사용자 결정.

## 4. PRD v2 적용 변경 요약

| ID | 변경 위치 | 변경 |
|---|---|---|
| D1 | Sprint 5 Validator 룰 | allowedFiles = `SceneTransitionService.cs / ProjectValidator.cs` 만 (GameManager / BossEncounterLauncher 제외) |
| D2 | Sprint 3 호출처 치환 | BossEncounterLauncher.cs 의 SceneManager.LoadScene → SceneTransitionService.Transition (현 PRD 의 11곳에 이미 포함됨 — 명시 강화만) |

D3 는 v2 미반영, mediator 후 최종.

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal). Accepted 2 / Discuss 1 / Rejected 0
