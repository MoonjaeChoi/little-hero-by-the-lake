# 5004 Scene Transition Async Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 7 태스크 분해
> 일시: 2026-05-04 · 7 commit / 2시간 25분

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 시간 | Sprint |
|---|---|---|---|---|---|
| T01 | ISceneTransition + SceneTransitionService 정적 (TransitionCoroutineRunner 포함) | Logic (Core) | - | 25분 | Sprint 1 |
| T02 | FadeOverlay 컴포넌트 (~70L, ISceneTransition 구현) | UI | T01 | 20분 | Sprint 2.1 |
| T03 | Resources/UI/FadeOverlay.prefab + Inspector wiring | Editor (Scene) | T02 | 15분 | Sprint 2.2 |
| T04 | 11 호출처 LoadScene → SceneTransitionService.Transition (BossEncounterLauncher 포함) | Logic (Core+Hub+Waves+UI) | T01 | 25분 | Sprint 3 |
| T05 | SceneTransitionBootstrap (RuntimeInitializeOnLoadMethod) | Logic (Core) | T02, T03 | 15분 | Sprint 4 |
| T06 | SceneTransitionTests 5+ NUnit + ProjectValidator §15.7 룰 | Test | T01~T05 | 30분 | Sprint 5 |
| T07 | docs + changelog + CLAUDE.md §11/§12/§13/§14 | Docs | T01~T06 | 15분 | Sprint 6 |

## 2. 의존성 그래프

```
T01 (Service+Interface) ──┬─→ T04 (호출처 11곳) ──┐
                          │                       │
                          └─→ T02 (FadeOverlay) ──┴─→ T05 (Bootstrap) ──┐
                                  │                                      │
                                  └─→ T03 (prefab) ────────────────────┤
                                                                        │
                                                                        └─→ T06 (Test+Validator) → T07 (docs)
```

T01 ↔ T02 병렬 가능 (서로 독립). T04 와 T03 도 병렬.

## 3. 구현 순서 (단일 작업자)

```
Step 1 (25분):  T01 ISceneTransition + SceneTransitionService
Step 2 (20분):  T02 FadeOverlay 컴포넌트
Step 3 (15분):  T03 prefab + Inspector wiring  ← 씬/prefab 저장 후 10초+ 대기
Step 4 (25분):  T04 11 호출처 일괄 치환
Step 5 (15분):  T05 Bootstrap
Step 6 (30분):  T06 NUnit 5 + Validator 룰
Step 7 (15분):  T07 docs
총: 약 2시간 25분
```

## 4. CLAUDE.md §15 정합

| 태스크 | §15 정합 |
|---|---|
| T01 | §15.1 SOLID-D / §15.6 60L / §15.7 DI Default property ✅ |
| T02 | §15.6 70L / §15.11 Time.unscaledDeltaTime ✅ |
| T03 | Editor wiring — 정합 영향 X |
| T04 | 일괄 치환 (BossEncounterLauncher 내부 D2 자동 수용 포함) |
| T05 | RuntimeInitializeOnLoadMethod (§15.11 Instantiate 1회만) ✅ |
| T06 | §15.9 NUnit 5+ + §15.12 Validator |

## 5. Unity MCP 안전

| 태스크 | 위험 | 완화 |
|---|---|---|
| T01, T02 | 낮음 | refresh + 5초 |
| **T03** | **높음 (prefab 작업)** | 씬/prefab 저장 후 10초+ |
| T04 | 중간 (11 파일) | refresh + 10초 |
| T05 | 낮음 | refresh + 5초 |
| T06 | Test Runner 검증 | run_tests + PASS 확인 |

## 6. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3 Breakdown).
