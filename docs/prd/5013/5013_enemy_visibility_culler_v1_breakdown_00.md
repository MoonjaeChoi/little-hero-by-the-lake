# 5013 EnemyVisibilityCuller — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-04 · 총 5 태스크 / 5 commit
> 출처: `5013_enemy_visibility_culler_v1_v2.md` (mediator 최종, D1=A 0.1s / D2=A 1.5x)

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | EnemyVisibilityCuller 정적 헬퍼 + Scene hook | Logic | — | 30분 | `_01_culler.md` |
| T02 | EnemyController.Update hook + throttle 0.1s | Logic | T01 | 25분 | `_02_controller_hook.md` |
| T03 | EnemyAnimationManager 정합 (HashSet) | Logic | T01, T02 | 20분 | `_03_animation_manager.md` |
| T04 | NUnit 6 + ProjectValidator 룰 | Test | T01~T03 | 30분 | `_04_test_validator.md` |
| T05 | Profiler 측정 + docs | docs | T04 | 25분 | `_05_profiler_docs.md` |

## 2. 의존성 그래프

```
T01 (Culler 정적 + SceneTransition hook)
  ↓
T02 (EnemyController.Update visibility 체크 + 0.1s throttle)
  ↓
T03 (EnemyAnimationManager HashSet Register/Unregister)
  ↓
T04 (NUnit 6 + Validator 룰)
  ↓
T05 (Profiler 측정 + changelog + CLAUDE.md)
```

순차 — T01 → T02 → T03 → T04 → T05 (각 commit 의존).

## 3. 병렬 가능성

- **순차만**: 모든 태스크가 이전 태스크 의존 (Culler → EnemyController hook → Manager → Test → Profiler)
- 병렬 X (T04 NUnit 이 T01~T03 모두 의존)

## 4. 구현 순서 권장

1. T01 (~30분) — 정적 헬퍼 + Scene hook
2. T02 (~25분) — EnemyController.Update hook
3. T03 (~20분) — Manager 정합
4. T04 (~30분) — NUnit + Validator
5. T05 (~25분) — Profiler + docs

총 **약 2시간** (직렬, 병렬 가능성 X).

## 5. CLAUDE.md §15 정합 자동 검증

| 태스크 | §15 정책 | 정합 여부 |
|---|---|---|
| T01 | §15.1 SRP / §15.6 100L (~80L) / §15.7 정적 cache | ✅ |
| T02 | §15.11 GC (Time.unscaledDeltaTime, Vector3 struct) | ✅ |
| T03 | §15.11 HashSet O(1) (List.Remove O(n) 회피) | ✅ |
| T04 | §15.9 Test (NUnit 6) / §15.12 Validator 룰 신규 | ✅ |
| T05 | docs — 영향 X | — |

## 6. Unity MCP 안전 전략

- **T01 + T02 + T03 = 코드 변경**: 각 commit 후 `refresh_unity` + 5~10초 대기 + `read_console` errors=0 확인
- **T04 NUnit**: 추가 후 도메인 리로드 안정화 → Test Runner 실행
- **T05 docs only**: MCP 영향 X
- 각 commit 사이 5~10초 대기 (CLAUDE.md §13 SIGSEGV 회피)

## 7. PRD 차이점 (v1 → v2)

- **T01 SceneTransition InvalidateCache hook 추가** (Rebuttal §3.3 accepted)
- **T03 HashSet 사용 명시** (Rebuttal §3.4 accepted, GC 0)
- **T04 ProjectValidator 룰 추가** (Rebuttal §3.1 accepted, EnemyController_Visibility_Check_Present)
- **T05 Profiler 측정 절차 구체화** (Rebuttal §3.2 accepted)

## 8. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3 Breakdown). 5 태스크 / 5 commit.
