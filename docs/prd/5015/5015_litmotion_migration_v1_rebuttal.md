# 5015 — LitMotion Migration PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 자동 적용
> **검토자**: Claude Code · 일시: 2026-05-04

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| 통합 | D1 PRD 전제 무효 — DOTween 0건 | **Critical** | Discuss | discussion_topics 분리 | △ 사용자 결정 |
| 성능 | D2 IEnumerator+Lerp GC 측정 필요 | High | Discuss | discussion_topics 분리 | △ |
| docs | D3 CLAUDE.md §10 outdated | Medium | Accepted | §10 DOTween 행 제거 | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 3
- Accepted: 1 (33%)
- Rejected: 0
- Discuss (사용자 결정): 2 (67%)
- §15 정합률: 100% (위반 0건)

## 3. Detailed Responses

### 3.1 [통합 — Critical] D1 PRD 전제 무효

**Original Criticism**:
- PRD §1.1: "DOTween 사용 현황" — Scripts grep 0건
- PRD §1.3: "마이그레이션 영향 ~50~100 호출" — invalid
- manifest.json: DOTween 패키지 미설치

- **Status**: Discuss → discussion_topics 이동 (사용자 5015 방향 결정)
- **이유**: 5015 PRD 가 사실 X 의 전제 위에 작성됨. 사용자 의도 재확인 필수.
- **Resolution Path**:
  - 옵션 A: 5015 RESERVED (DOTween 마이그레이션 무의미)
  - 옵션 B: 5015 재정의 — IEnumerator+Lerp → LitMotion 전환
  - 옵션 C: §10 docs 정합만 (1 commit fix, 5015 보류)
  - 옵션 D: LitMotion 신규 도입 + 점진

### 3.2 [성능 — High] D2 IEnumerator+Lerp GC 측정 필요

**Original Criticism**: PIVOT v4 §7 GC <50KB/s 마진 측정 0.

- **Status**: Discuss → discussion_topics 이동
- **이유**: LitMotion 도입 ROI 정량 평가 → Profiler 측정 후 결정
- **Resolution Path**: 사용자가 측정 결과 확인 후 D1 옵션 (B/D) 선택 결정

### 3.3 [docs — Medium] D3 CLAUDE.md §10 outdated

**Original Criticism**: §10 "DOTween 커스텀" 행이 사실 X (사용 0건).

- **Rebuttal**: Accepted — 즉시 수정
- **Proposed Action**: §10 외부 라이브러리 표에서 "DOTween" 행 제거 + 사용 라이브러리만 명시
- **PRD Impact**: D1 결정 시점에 통합 fix (CLAUDE.md §10 갱신 + 5015 종결 또는 재정의)

## 4. PRD v2 적용 변경 요약

(Accepted 1건 만 v2 초안 반영. Discuss 2건은 mediator 후 최종 확정)

- §10 외부 라이브러리 — DOTween 행 제거 (D3 accepted)
- D1/D2 결정 후 추가 변경 결정

## 5. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal).
