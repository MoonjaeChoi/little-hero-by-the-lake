# 5004 Scene Transition Async — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 결정 필요한 미결 항목.
> 입력: 3 Discuss → 자동 수용 2 (D1, D2) 제외 → 본 문서 1건 (D3).

## High / Medium

(없음)

## Low Priority

### D3 — Sprint 2 commit 분리 vs 통합

- **Original Criticism**: Sprint 2 = 컴포넌트 (1 commit) + prefab (1 commit). 작은 시스템이라 1 commit 통합도 가능?

- **Discussion Points**:
  - **A** ★권장: 2 commit 그대로 — 5002 Sprint 3.1+3.2 분리 패턴 답습 (씬/prefab 변경 후 10초+ 대기, MCP SIGSEGV 회피)
  - **B**: 1 commit 통합 — 작업 단위 작은 시스템

- **Proposed Resolution Path**: **A** (2 commit 분리, 안전)

- **PRD Impact**:
  - 옵션 A: PRD 그대로 (6 Sprint × 7 commit)
  - 옵션 B: Sprint 2 가 1 commit 으로 변경 (6 Sprint × 6 commit, 5분 단축)

## 요약

- Total Items: 1 (Low)
- **Claude 권장 일괄 채택**: D3=A (2 commit 분리)
- 사용자 결정 입력 형식: `D3=A` / `권장대로` / `통합으로 진행 D3=B` 등
