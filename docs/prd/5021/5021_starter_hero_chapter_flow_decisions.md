# 5021 PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 2건 결정 기록
> 모드: ai-assisted · 일시: 2026-05-06
> 입력: `5021_starter_hero_chapter_flow_discussion_topics.md` + `5021_starter_hero_chapter_flow_v2.md` (rebuttal 초안)

---

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| High | D1 | R3 옵션 — AtticChapterEntry vs WondrousMapPanel | **B (Claude 권장)** — AtticChapterEntry 비활성화 + WondrousMapPanel 검증 | Done |
| Medium | D2 | Sprint 1 의 2 commit 분배 | **C (Claude 권장)** — PRD 표 2 commit 보존 + §2 본문 commit 메시지 2개 명시 | Done |

사용자 결정: **"권장 그대로 진행"** (D1=B, D2=C 일괄 채택, ai-assisted 권장 100% 따름).

---

## 2. Detailed Decisions

### 2.1 D1 — R3 옵션: AtticChapterEntry → WondrousMapPanel 단일화

- **Decision**: **옵션 B** — AtticChapterEntry MonoBehaviour 비활성화 또는 GameObject 제거 + WondrousMapPanel 35 노드 wiring 검증
- **Mode**: ai-assisted (Claude 권장 + 사용자 "권장 그대로" 응답)
- **Rationale**:
  - 5005 attic_decomposition_v1 정신 — production path 단일화
  - Attic.unity grep 결과 `WondrousMapNode` 10+ instance 확인 → 35 노드 모두 존재 추정 → WondrousMapPanel 가 prod path
  - AtticChapterEntry._buttons[] 빈 배열 = dead code (5020.2 R2 와 동일 패턴)
  - §15.1 SRP 강화 + §15.6 100줄 룰 cascade (dead code 정리)
- **PRD Impact**:
  - §3.1 R3 fix (PRD 디자인 §) — "AtticChapterEntry._buttons[] wiring 또는 WondrousMapPanel production path 정합 검증" → "AtticChapterEntry 비활성화 + WondrousMapPanel 검증/wiring 보강" 으로 명확화
  - **Sprint 3 §4.2 재정의** — R3 작업 항목:
    - AtticChapterEntry GameObject 비활성화 (또는 Component 제거)
    - WondrousMapPanel `_chapters[]` (35 ChapterData 참조) wiring 상태 grep
    - WondrousMapPanel `_nodes[]` (35 WondrousMapNode 참조) wiring 상태 grep
    - 누락 시 Inspector 또는 scene YAML 으로 보강
  - **Sprint 3 시간**: 30분 → 60분 (+30분)
  - **합계**: 2h15m → 2h45m / 5 commit (commit 수 동일)
- **Trade-off**: 옵션 A (PRD 원안) 의 단순함 손실, 그러나 dead code 살림 회피.

### 2.2 D2 — Sprint 1 commit 메시지 2개 명시

- **Decision**: **옵션 C** — PRD 표 2 commit 보존 + §2 본문에 commit 메시지 2개 명시
- **Mode**: ai-assisted (Claude 권장 + 사용자 확인)
- **Rationale**:
  - CLAUDE.md 머리말 "단일 책임 commit" 정합 — 작은 분리는 회수 용이성 ↑
  - SO 클래스 (`CharacterPool.cs`) 와 SO 인스턴스 (`CharacterPool_Default.asset`) + Validator 룰 의 의미 단위가 분리 가능
  - 분리 시 한 commit 회수 시 다른 commit 영향 0 (예: SO 인스턴스 잘못 wiring 시 commit 2 만 revert)
- **PRD Impact**:
  - **Sprint 1 §2 본문 갱신** — commit 1 (SO 클래스만) + commit 2 (SO 인스턴스 + ProjectValidator 룰) 분리 명시
  - 시간 영향 0 (각 ~15분, 합계 30분)
- **Trade-off**: 1 commit 의 단순함 손실, 그러나 단일 책임 commit 정합.

---

## 3. 통계

- Total Items: 2
- AI Recommendations Followed: 2 / 2 (100%)
- User Overrides: 0
- 모드: ai-assisted

---

## 4. v2_prd.md 변경 diff 요약

(rebuttal 초안 → mediator 최종)

| 영역 | 변경 |
|---|---|
| Sprint 3 §4.2 (R3 부분) | "AtticChapterEntry._buttons[] wiring 결정 후 적용" → "AtticChapterEntry 비활성화 + WondrousMapPanel 검증" (D1=B) |
| Sprint 3 시간 | 30분 → 60분 (D1=B cascade) |
| Sprint 1 §2 commit 메시지 | 1개 (통합) → 2개 (분리) (D2=C) |
| 합계 표 | 2h15m → 2h45m (D1 영향) |
| commit 수 | 5 (동일 — D2 표는 이미 2 commit 표기) |

---

## 5. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.3 Mediator). D1=B + D2=C 결정. AI 권장 채택률 100% |
