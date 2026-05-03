# 5005 Attic Decomposition — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 결정 필요한 미결 항목 3.

## High / Medium

### D1 — 컴포넌트 분리 단위

- **Discussion**:
  - **A** ★권장: 7 컴포넌트 (PRD 그대로) — SRP 명확, 5002/5004 단일 책임 패턴 답습
  - **B**: 6 컴포넌트 (AtticChapterFlow = ChapterEntry + ReturnHandler 통합) — 90L, 챕터 클리어 후 toast/overlay 통합 책임
  - **C**: 5 컴포넌트 (Currency + Character + Chapter 가 Attic 본체) — 분리 효과 약화

- **Claude 권장**: A (7 컴포넌트) — SRP 명확, 회귀 격리.

## Low

### D2 — NUnit Test 범위

- **Discussion**:
  - A: Reflection 5 + 라인 수 검증 1 (정적)
  - B: + Play Mode Test (각 컴포넌트 OnEnable/OnDisable 동적 검증) — Sprint 7 +30분
  - **C** ★권장: Reflection 5 + ProjectValidator §15.6 룰 강화 — Play Mode Test 시간 부담 회피

- **Claude 권장**: C (Validator 강화).

### D3 — 씬 wiring 시점

- **Discussion**:
  - **A** ★권장: 6 Sprint 각 commit 후 씬 wiring + 10초+ 대기 (5002/5003 패턴) — 안전
  - B: Sprint 1~6 코드 + Sprint 7 일괄 wiring — 빠르지만 회귀 위험
  - C: MCP 자동 wiring (manage_components) — 검증 어려움

- **Claude 권장**: A (안전 우선, 게임 진입 허브).

## 요약

- Total: 3 (Medium 1 / Low 2)
- **Claude 권장 일괄 채택**: D1=A, D2=C, D3=A
- 사용자 결정 입력 형식: `D1=A, D2=C, D3=A` / `권장대로` 등
