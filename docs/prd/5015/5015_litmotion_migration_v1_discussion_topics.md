# 5015 — LitMotion Migration Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.

## High Priority Items

### 🔴 D1. PRD 전제 무효 — 5015 방향 재정의 (Critical)

- **Original Criticism**: PRD 가 "DOTween → LitMotion 마이그레이션" 인데 프로젝트에 DOTween 사용 0건. PRD §1.1 + §1.3 invalid.
- **Current Position**: PRD 그대로 진행 시 — 마이그레이션할 코드 X → 작업 무의미.
- **Discussion Points**:
  - **A (★권장)**: **5015 RESERVED** + CLAUDE.md §10 DOTween 항목 제거 (1 commit fix). LitMotion 도입은 별도 사이클 (예 5015b — IEnumerator → LitMotion 디자인부터).
    - 장점: 잘못된 전제로 작업 회피. 향후 LitMotion 도입 시 정확한 디자인.
    - 단점: 5015 anchor "처리됨" 표시 어려움 (RESERVED 패턴 5009 답습).
  - **B**: 5015 재정의 → "**IEnumerator+Lerp → LitMotion 전환**". scope 동일하지만 의미 변경.
    - 장점: 5015 안전하게 종결 가능 (안전성 ↑).
    - 단점: 사용자 LitMotion 학습 + 작업 ~3.5h. ROI 측정 X (D2 의존).
  - **C**: 5015 보류 + §10 docs fix 만 (5015 = 1 commit minor patch). LitMotion 도입 안 함.
    - 장점: 빠른 종결.
    - 단점: 5015 의 의도 (성능 강화) 미달성.
  - **D**: LitMotion 신규 도입 + **신규 코드만** LitMotion 사용 (점진).
    - 장점: 부담 ↓.
    - 단점: 일관성 ↓ (IEnumerator + LitMotion 혼재).
- **Proposed Resolution Path**: **A 권장** — 잘못된 전제 위 작업 회피 + 정확한 디자인 후 5015b 신규.
- **PRD Impact**: D1 선택 따라 v2 의 §1.1/§1.3 + Sprint 표 전면 재작성 필요.

### 🟡 D2. IEnumerator+Lerp 패턴 GC 측정

- **Original Criticism**: PIVOT v4 §7 GC <50KB/s 마진 측정 0건. LitMotion ROI 정량 평가 X.
- **Current Position**: 측정 없이 LitMotion 도입 결정은 over-engineering 위험.
- **Discussion Points**:
  - **A (★권장)**: **Profiler 측정 후 결정** (D1 의 B/D 옵션 진행 전 사전 검증)
    - 측정 대상: cap 300 환경 + wave 5 spawn 시 GC alloc/sec
    - 임계: <50KB → 마진 충분 (LitMotion 불필요), >100KB → 도입 정당
  - B: 마진 충분 가정 → IEnumerator 유지 (5015 종결, D1=A/C)
- **Proposed Resolution Path**: **A 권장** — 측정 데이터 없이 큰 작업 진행 위험.
- **PRD Impact**: D2 결정 (측정 결과) 가 D1 옵션 선택에 영향.

## 요약

- Total Items: 2 (D3 Accepted 처리됨)
- High: 1 (D1 Critical)
- Medium: 1 (D2)
- 권장 mode: ai-assisted (Claude 추천 + 사용자 결정)
- 권장 결정: D1=A (RESERVED) + D2=A (측정 후 결정) → 5015b 별도 PRD 권장
