# 5009 Chapter Overrides v1 — PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> 일시: 2026-05-04 · 출처: critique (3 Discuss 중 D3 자동 수용) → 2 Discuss 잔여

## High Priority Items

(없음)

## Medium Priority Items

### D1. Chapter Override 의 V2 시스템 미사용 — ROI 의문

- **Original Criticism**: 디자인 §1.2 가 chapter override 가 PhaseFContentSeed V2 시스템 미사용 명시. 5008 Auditor V2 fallback = OK 통과. 즉시 활용처 X — 5009 진행 시 ~3.5~5h 투자, 즉각 게임플레이 영향 X.
- **Current Position**: PRD 진행 시 210 PNG 재생성 — legacy 호환 + 미래 활용 anchor 만, 즉각 ROI 약함.
- **Discussion Points**:
  - **옵션 A. 진행 (legacy + 미래 활용)**: 디자인 일관성 ↑, 5009.x 후속에서 chapter override wiring 시 즉시 활용 가능
    - 장점: 자산 인프라 정비 / 5008 baseline 안정 검증 + retention 시나리오 (chapter 별 시각 차별)
    - 단점: 즉각 효과 X, ~4h 투자 vs 5012/5013/5015 코드 작업 ROI 큼
  - **옵션 B. 후순위 (5012/5013/5015 종결 후)**: 코드 작업 (성능/다양성/라이선스) 우선
    - 장점: 60fps 마진 + GC 0 + DOTween → LitMotion 즉각 효과
    - 단점: 5009 anchor 잔여
  - **옵션 C. 보류 (사용자 회귀 보고 시에만)**: 5008 baseline 으로 1~2 PNG 즉시 재생성 가능
    - 장점: 자원 절감 (4h → 0)
    - 단점: chapter override 인프라 미정비, 향후 활용 시 baseline 재정비 필요
- **Proposed Resolution Path**: **옵션 B 권장** — 5012/5013/5015 코드 작업이 즉각 게임플레이 영향 ↑. 5009 는 후순위 또는 5008.x 후속 사이클로.
- **PRD Impact**:
  - 옵션 A 채택 시: PRD 그대로 진행 (Phase 3 Breakdown)
  - 옵션 B 채택 시: 5009 RESERVED 해제 + 5012 또는 5013 또는 5015 우선 진행
  - 옵션 C 채택 시: 5009 PRD 보존 (Phase 1+2 만), 사용자 회귀 보고 시 즉시 사이클 호출

---

### D2. Sprint 5 (Fantasy 48 PNG) 분할 검토

- **Original Criticism**: Sprint 5 = Fantasy 8 챕터 × 6 = 48 PNG, 한 번 사용자 review. 다른 Sprint (30~54) 와 비교 가장 큼은 아니나 review 부담 ↑.
- **Current Position**: 6 commit 유지, 사용자 5 batch review.
- **Discussion Points**:
  - **옵션 A. 그대로 (1 sprint)**: 5008 패턴 답습, 사용자 5 batch review
    - 장점: commit 수 일관 (6), 카테고리 톤 일관 검증 효율
    - 단점: 48 PNG 한 번에 review 부담
  - **옵션 B. Nature 54 + Fantasy 48 분할 (8 sprint)**: 큰 batch 둘 다 분할
    - 장점: review 단위 균등 (~24~30 PNG/batch)
    - 단점: commit 8 (+2), 7~8 review 사이클
  - **옵션 C. 가장 큰 Nature 만 분할 (7 sprint)**: Sprint 2A (CH 6~10, 30 PNG) + Sprint 2B (CH 11~14, 24 PNG)
    - 장점: 균형 + commit 1 만 추가
    - 단점: 비대칭 (Fantasy 48 그대로)
- **Proposed Resolution Path**: **옵션 A 권장** — 5008 검증된 batch 단위가 안정. 48 PNG 한 번 review 도 카테고리 톤 일관 검증 효율 ↑.
- **PRD Impact**:
  - 옵션 A: 변경 X
  - 옵션 B: Sprint 표 6 → 8, commit 6 → 8
  - 옵션 C: Sprint 표 6 → 7, commit 6 → 7

---

## Low Priority Items

(없음 — Low Priority 2건 모두 rebuttal 단계 Accepted, v2 초안 직접 반영)

## 요약

- **Total Items**: 2
- **High**: 0 / **Medium**: 1 (D1) / **Low**: 1 (D2)
- **권장 mode**: ai-assisted (Claude 추천 + 사용자 확인)
- **권장 결정 일괄 입력**: `D1=B, D2=A` 또는 `권장안대로`
