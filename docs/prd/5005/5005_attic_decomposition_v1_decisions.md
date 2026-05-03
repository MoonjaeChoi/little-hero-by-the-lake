# 5005 Attic Decomposition — Mediator Decisions

> **Phase 2.3 Mediator** / 모드: ai-assisted (기본)
> 사용자 결정: "권장안대로" (2026-05-04)
> **D1=A, D2=C, D3=A** 권장 일괄 채택

## 결정 표

| ID | 토픽 | 결정 |
|---|---|---|
| **D1** | 컴포넌트 분리 단위 | **A — 7 컴포넌트 (PRD 그대로)** |
| **D2** | NUnit Test 범위 | **C — Reflection 5 + ProjectValidator §15.6 룰 강화** |
| **D3** | 씬 wiring 시점 | **A — Sprint별 commit + 10초+ 대기** |

## 결정 상세

### D1 = A — 7 컴포넌트 (단일 책임 명확)

**채택 이유**:
- 5002 BackgroundApplier / 5004 FadeOverlay 단일 책임 패턴 답습
- SRP 명확 — 한 컴포넌트 변경이 다른 컴포넌트 영향 X
- 회귀 격리 — Sprint 단위 commit 으로 위험 작은 단위 분산

**PRD 영향**: v1 그대로 (8 Sprint × 8 commit × 3시간 25분).

### D2 = C — Reflection NUnit + Validator §15.6 룰

**채택 이유**:
- Play Mode Test 는 Sprint 7 +30분 (3시간 25분 → 3시간 55분 부담)
- Reflection 5 로 Type/메서드 존재 검증 충분
- ProjectValidator §15.6 룰 (AtticController 200L 이내) 강화 — 자동 회귀 방지

**PRD 영향**: Sprint 7 작업에 ProjectValidator 라인 수 강화 룰 추가 (기본 200L+ 룰 + AtticController 특정 200L 검사).

### D3 = A — Sprint별 wiring + 10초+ 대기

**채택 이유**:
- 게임 진입 허브 — 안전 우선
- 5002 Sprint 3.2 / 5003 / 5004 Sprint 2.2 패턴 답습 (씬 저장 후 10초+ 대기, MCP SIGSEGV 회피)
- 일괄 wiring 시 회귀 발생 위치 추적 어려움

**PRD 영향**: v1 그대로.

## v2 PRD 상태

PRD v1 = v2 (자동 수용 0건 + 권장 채택 — 변경 없음).

## 변경 이력

- 2026-05-04: ai-assisted 권장 일괄 채택. D1=A, D2=C, D3=A.
