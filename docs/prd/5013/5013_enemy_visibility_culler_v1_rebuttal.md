# 5013 EnemyVisibilityCuller v1 — PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-04

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| OOP §15 | 위반 0 | — | — | 신규 정적 헬퍼 + 80L | ✅ |
| PG-12 톤 | 영향 없음 | — | — | 성능 영역 | ✅ |
| Sprint 분해 (D3) | Validator 룰 명시 | Low | **Accepted** | §15.12 정책 — Validator EnemyController_Update_Visibility 룰 추가 | ✅ |
| 측정 (§7.1) | Profiler 측정 절차 모호 | Low | **Accepted** | Sprint 5 의 측정 절차 구체화 (Editor / iPhone 12 / Unity Profiler / 측정 시점) | ✅ |
| 시스템 통합 (§7.2) | InvalidateCache hook (5004 연계) | Low | **Accepted** | SceneTransitionService.OnAfterSceneLoad → Culler.InvalidateCache 연계 | ✅ |
| 성능 (§7.3) | HashSet vs List | Low | **Accepted** | EnemyAnimationManager 의 Manager 내부 cache HashSet 권장 | ✅ |
| 수치 (D1) | Throttle 빈도 (0.1s) | Low | **Discuss** | discussion_topics D1 (4 옵션) | △ |
| 수치 (D2) | Margin 크기 (1.5x) | Low | **Discuss** | discussion_topics D2 (3 옵션) | △ |

## 2. 품질 메트릭

- 총 Discussion Points: 6 (Critique Discuss 3 + 추가 관찰 3)
- **Accepted** (수정 반영): 4건 (67%)
- **Rejected**: 0건 (0%)
- **Discuss** (사용자 결정): 2건 (33%)
- §15 정합률: 100% (위반 0건)

## 3. Detailed Responses

### 3.1 [Sprint 분해 D3 — Low — Accepted]

**Original Criticism** (critique §4 D3):
> Sprint 4 ProjectValidator 룰 1줄 명시 — "EnemyController 의 Update 첫 줄이 visibility 체크" — 그러나 구체적 검증 룰 미명시.

- **Rebuttal**: **수용**. §15.12 정책 + 5004 SceneTransitionService Validator 패턴 답습 (allowedFiles 제한). 정합 검증 자동화 가치 ↑.
- **Proposed Action**: Sprint 4 의 ProjectValidator 룰 명세 추가 — `EnemyController_Visibility_Check_Present` (정적 분석으로 EnemyController.Update 첫 줄에 `EnemyVisibilityCuller.IsVisible(...)` 호출 검사). 향후 EnemyController 변경 시 누락 자동 경고.
- **PRD Impact**: Sprint 4 commit 메시지 + 검증 기준에 Validator 룰 명세 1단락.
- **Little Hero Context**: 5004 (SceneTransition) 의 §15.7 allowedFiles 룰과 동일 정합 패턴.

---

### 3.2 [측정 §7.1 — Low — Accepted]

**Original Criticism** (critique §7.1):
> Sprint 5 의 Profiler 측정 — "GPU draw call -50%, CPU Update -30%" 추정. 실측 기준 모호.

- **Rebuttal**: **수용**. PIVOT v4 §7 cap 300 환경 검증을 위해 측정 절차 명확화 필요.
- **Proposed Action**: Sprint 5 commit 메시지에 측정 절차 구체화:
  - 환경: Unity Editor Play Mode + iPhone 12 빌드 (해당 시)
  - 시나리오: cap 300 wave 환경 (test scene 또는 wave 30+ chapter)
  - 도구: Unity Profiler (CPU + Rendering 탭) / Xcode Instruments (iOS 빌드)
  - 측정 항목: frame time / GPU draw call / CPU Update 호출 빈도 / GC allocation/sec
  - 비교: cull 도입 전후 frame 평균 (1분 측정)
- **PRD Impact**: Sprint 5 검증 기준 1 표 추가.

---

### 3.3 [시스템 통합 §7.2 — Low — Accepted]

**Original Criticism** (critique §7.2):
> InvalidateCache 호출 hook — 5004 SceneTransitionService 의 OnAfterSceneLoad 이벤트와 연계 권장.

- **Rebuttal**: **수용**. 5004 SceneTransitionService 의 정적 이벤트 활용 — Culler 의 Camera cache 자동 invalidate.
- **Proposed Action**: Sprint 1 의 InvalidateCache 명세에 자동 hook 추가 — `SceneTransitionService.OnAfterSceneLoad += InvalidateCache` 정적 초기화 (RuntimeInitializeOnLoadMethod).
- **PRD Impact**: Sprint 1 명세 + Sprint 4 NUnit `InvalidateCache_OnSceneTransition_Triggered` 추가 검증.
- **Little Hero Context**: 5004 의 SceneTransitionBootstrap 패턴 답습 (RuntimeInitializeOnLoadMethod BeforeSceneLoad).

---

### 3.4 [성능 §7.3 — Low — Accepted]

**Original Criticism** (critique §7.3):
> EnemyAnimationManager 의 Register/Unregister 빈도 — 매 프레임 visibility 변동 시 GC 가능성. List add/remove 비용 검토 필요.

- **Rebuttal**: **수용**. §15.11 GC 최소화 정책. 매 프레임 List.Remove 는 O(n) → 300 적 환경 비용.
- **Proposed Action**: Sprint 3 의 EnemyAnimationManager Register/Unregister 구현 — HashSet 사용 + Register/Unregister 가 O(1).
  - 또는 Manager 가 매 프레임 IsVisible 직접 체크 + List 유지 (cull 분기) — 더 단순
- **PRD Impact**: Sprint 3 명세에 HashSet (또는 직접 IsVisible 체크) 명시.
- **Little Hero Context**: 5005 AtticPanelToggleManager 의 Dictionary 패턴 답습.

---

### 3.5 [수치 D1 — Low — Discuss]

**Original Criticism** (critique §4 D1):
> Throttle 0.1s 가 적절한가?

- **Status**: **Discuss** → discussion_topics D1
- **이유**: 수치 밸런스 (4 옵션) — 사용자 결정 또는 SerializeField 옵션
- **Resolution Path**: mediator A (0.1s) / B (매 프레임) / C (0.05s) / D (0.2s+margin) 결정. Claude 권장 A.

---

### 3.6 [수치 D2 — Low — Discuss]

**Original Criticism** (critique §4 D2):
> Margin 1.5x 가 적절한가?

- **Status**: **Discuss** → discussion_topics D2
- **이유**: 수치 밸런스 (3 옵션) — 사용자 결정.
- **Resolution Path**: mediator A (1.5x) / B (1.2x) / C (2.0x) 결정. Claude 권장 A.

## 4. PRD v2 적용 변경 요약

(수용 항목만 v2.md 에 반영. 미결은 v2 미반영, mediator 후 최종.)

### Accepted (4건 반영)

- **Sprint 1 InvalidateCache hook** — SceneTransitionService.OnAfterSceneLoad 자동 연계 (RuntimeInitializeOnLoadMethod)
- **Sprint 3 EnemyAnimationManager 정합** — HashSet 또는 직접 IsVisible 체크 (GC 0)
- **Sprint 4 Validator 룰** — `EnemyController_Visibility_Check_Present` 추가 명세
- **Sprint 5 Profiler 측정 절차** — 환경 / 시나리오 / 도구 / 측정 항목 / 비교 명시

### Discuss (2건 — mediator 결정 대기)

- D1 Throttle (0.1s vs 매 프레임 vs 0.05s vs 0.2s+margin)
- D2 Margin (1.5x vs 1.2x vs 2.0x)

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal). Accepted 4 / Discuss 2 / Rejected 0.
