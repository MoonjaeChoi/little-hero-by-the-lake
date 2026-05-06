# 5022 PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 3건 결정 기록
> 모드: ai-assisted · 일시: 2026-05-06

---

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| High | D1 | Sprint 1 임시 errors=0 위험 | **B (Claude 권장)** — Sprint 1+2 통합 (3 commit) | Done |
| Medium | D2 | Rule 통합 단위 | **A (Claude 권장)** — 11 Rule 통합 유지 | Done |
| Medium | D3 | 신규 NUnit 추가 여부 | **A (Claude 권장)** — NUnit 신규 X, 5024 위임 | Done |

사용자 결정: **"권장 그대로 진행"** (일괄 채택, AI 권장 채택률 100%).

---

## 2. Detailed Decisions

### 2.1 D1 — Sprint 1+2 통합 (위험 영역 해소)

- **Decision**: **옵션 B** — Sprint 1+2 통합 (Sprint 1.1 = Issue 추출 + 폴더 구조 / Sprint 1.2 = orchestrator 슬림화 + 6 간단 Rule 동시 분리)
- **Rationale**: 항상 룰 작동 보장 → 회귀 검증 안전망 유지. commit 회수 유연성 미미한 손실.
- **PRD Impact**:
  - Sprint 분해: 4 → **3 Sprint** (Sprint 1 = 1.1+1.2 / Sprint 2 = 복잡 5 Rule / Sprint 3 = 검증)
  - commit: 4 → **3 commit** (Sprint 1.1 + Sprint 1.2 + Sprint 2 + Sprint 3 = 4 commit, but Sprint 1.1+1.2 = 1 cycle)

  실제로 다시 정리:
  - Sprint 1.1 (Issue 추출 + 폴더 구조): 1 commit, ~15분
  - Sprint 1.2 (orchestrator + 6 간단 Rule): 1 commit, ~45분
  - Sprint 2 (복잡 5 Rule): 1 commit, ~45분
  - Sprint 3 (검증): 1 commit, ~30분
  - **합계**: 4 commit / 2h15m (~15분 단축, 위험 영역 해소)

### 2.2 D2 — 11 Rule 통합 유지

- **Decision**: **옵션 A** — PRD 원안 11 Rule 분배 유지
- **Rationale**: 100L 룰 충족 (모든 Rule 100L 미만), 같은 카테고리 응집도 우선. 80L 권장 미달 (85L) 은 추후 비대 시 재분리 가능.
- **PRD Impact**: 변동 0 (PRD §4.1 그대로)

### 2.3 D3 — NUnit 신규 X (5024 위임)

- **Decision**: **옵션 A** — NUnit 신규 추가 X
- **Rationale**: 분리는 동작 변경 0 → 기존 NUnit 117 회귀 안전망 충분. ProjectValidator 단위 테스트는 5024 (editor_test_expansion) scope 가 더 적합.
- **PRD Impact**: 변동 0

---

## 3. 통계

- Total Items: 3
- AI Recommendations Followed: 3 / 3 (100%)
- User Overrides: 0
- 모드: ai-assisted

---

## 4. v2_prd.md 변경 diff 요약

(rebuttal 초안 → mediator 최종)

| 영역 | 변경 |
|---|---|
| Sprint 분해 (PRD §1) | 4 Sprint / 4 commit / 2h30m → 4 Sprint / 4 commit / 2h15m (Sprint 1+2 통합 D1=B) |
| Sprint 1 (이전) | "Issue 추출 + orchestrator 슬림화" → **Sprint 1.1**: Issue 추출 + 폴더 (15분) |
| Sprint 2 (이전 = 간단 6) | → **Sprint 1.2**: orchestrator + 6 간단 Rule 통합 (45분) |
| Sprint 3 (이전 = 복잡 5) | → **Sprint 2**: 복잡 5 Rule (45분) |
| Sprint 4 (이전 = 검증) | → **Sprint 3**: 검증 (30분) |
| Rule 분배 (§4.1) | D2=A 변동 0 (11 Rule 유지) |
| NUnit | D3=A 변동 0 (신규 X) |

---

## 5. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.3 Mediator). D1=B / D2=A / D3=A. AI 권장 채택률 100% |
