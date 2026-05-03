# 5005 Attic Decomposition — PRD Rebuttal

> **Phase 2.2** / 입력: critique 8/10 (Accept 8 / Reject 0 / Discuss 3)

## 1. 요약 표

| ID | Category | Priority | Status | §15 Aligned |
|---|---|---|---|---|
| D1 | 컴포넌트 분리 단위 (7/6/5) | Medium | **Discuss** | △ |
| D2 | NUnit 범위 (Reflection / Play Mode) | Low | **Discuss** | △ |
| D3 | 씬 wiring 시점 (Sprint별 / 일괄 / 자동) | Low | **Discuss** | △ |

## 2. 품질 메트릭

- Total Discussion Points: 3
- **Accepted**: 0
- **Discuss**: 3 (모두 사용자 결정 사안 — 디자인 분리 단위 / Test 범위 / 안전성)
- **Rejected**: 0
- §15 정합률: 100% (모든 옵션 §15 정합)

## 3. Detailed Responses

### 3.1 D1 — 컴포넌트 분리 단위 (Discuss)

**Status**: discussion_topics.md 로 이동.

**이유**: 디자인 의도 결정 (사용자 권한). 7/6/5 모두 §15 정합. 단일 책임 우선 vs 라인 절약 trade-off.

**Resolution Path**: mediator ai-assisted 권장 = A (7 컴포넌트, 단일 책임 명확).

### 3.2 D2 — NUnit 범위 (Discuss)

**Status**: discussion_topics.md 로 이동.

**이유**: Test 범위 결정 — 시간 vs 검증 강도.

**Resolution Path**: mediator ai-assisted 권장 = C (Reflection 5 + Validator 룰 강화 — Play Mode Test 시간 부담 회피).

### 3.3 D3 — 씬 wiring 시점 (Discuss)

**Status**: discussion_topics.md 로 이동.

**이유**: 안전성 결정 (게임 진입 허브 — wiring 누락 시 치명적).

**Resolution Path**: mediator ai-assisted 권장 = A (Sprint별 commit + 10초+ 대기, 5002/5003 패턴).

## 4. PRD v2 변경 요약

(자동 수용 0건 — v2 = v1 그대로. mediator 결정 후 최종.)

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2). Accepted 0 / Discuss 3 / Rejected 0
