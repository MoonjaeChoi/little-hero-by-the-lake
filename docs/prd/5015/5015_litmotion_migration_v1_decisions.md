# 5015 — LitMotion Migration PRD Decisions

> **Phase 2.3 Mediator 산출** — Critical 발견 + 미결 항목 2건 결정 기록
> 모드: ai-assisted (Claude 권장 + 사용자 "권장안대로" 일괄 채택) · 일시: 2026-05-04

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Critical | D1 | 5015 방향 | **A** — RESERVED + §10 docs fix | **RESERVED** |
| High | D2 | IEnumerator+Lerp GC 측정 | **A** — Profiler 측정 후 5015b 결정 | Done (대기) |

## 2. Detailed Decisions

### 2.1 D1 — 5015 방향 (Critical)

- **Decision**: **옵션 A — RESERVED + CLAUDE.md §10 docs fix (1 commit)**
- **Mode**: ai-assisted (Claude 권장 + 사용자 "권장안대로")
- **Rationale**:
  - PRD 전제 (DOTween 마이그레이션) 가 사실 X — 마이그레이션할 코드 0건
  - Scripts/ `using DG.Tweening` grep = 0 / manifest.json DOTween 미설치
  - CLAUDE.md §10 outdated → 즉시 수정 필요
  - LitMotion 도입은 별도 사이클 (5015b 신규 PRD) 에서 정확한 디자인 가능
  - 옵션 B (재정의) 는 ROI 측정 X 에서 ~3.5h 작업 진행 위험
- **PRD Impact**:
  - **5015 = RESERVED** (5009 패턴 답습)
  - PRD 자체 (디자인 + Sprint PRD) 보존 — 향후 참조 가능
  - **즉시 수정**: CLAUDE.md §10 외부 라이브러리에서 "DOTween" 행 제거 (1 commit)
  - 후속 5015b: IEnumerator 패턴 + GC 측정 결과 기반 LitMotion 도입 디자인
- **Trade-off**:
  - 옵션 B/D 의 LitMotion 즉시 도입 손실 — 다만 ROI 측정 후 정확한 디자인이 우선
  - 옵션 C 와 차이: A 는 후속 5015b 가능, C 는 LitMotion 도입 자체 보류

### 2.2 D2 — IEnumerator+Lerp GC 측정

- **Decision**: **옵션 A — Profiler 측정 후 결정 (5015b 진행 전제)**
- **Mode**: ai-assisted
- **Rationale**:
  - PIVOT v4 §7 GC <50KB/s 마진 측정 0건 → LitMotion 도입 ROI 정량 평가 X
  - 측정 대상: cap 300 환경 + wave 5+ spawn 시 GC alloc/sec
  - 임계: <50KB → 마진 충분 (LitMotion 불필요), >100KB → 도입 정당
- **PRD Impact**: 5015b 신규 PRD 진행 시 사전 측정 결과 첨부 필수
- **Trade-off**: 측정 비용 (~10분) ↔ 잘못된 도입 결정 회피

## 3. 통계

- **Total Items**: 2
- **AI Recommendations Followed**: 2/2 (100%)
- **User Overrides**: 0
- **모드**: ai-assisted (사용자 "권장안대로" 일괄 채택)

## 4. v2 PRD 변경 diff 요약

### D1=A 채택 결과 (RESERVED)

- **5015 = RESERVED** — Phase 3~6 미실행, PRD 보존
- v2.md 상단 "RESERVED — D1=A docs fix only" 명시
- 후속 5015b 진행 시: `/prd-pipeline 5015b --from generate` 신규 디자인

### D2=A 채택 결과 (측정 후 결정)

- 5015b 사전 작업: Profiler 측정 1회 (cap 300 환경 GC alloc/sec)
- 결과 < 50KB → 5015b 보류 (IEnumerator 유지)
- 결과 > 100KB → 5015b 진행 (LitMotion 도입)

## 5. 즉시 fix 작업 (D1=A 의 1 commit)

CLAUDE.md §10 외부 라이브러리:
```diff
- | 애니메이션 | DOTween | 커스텀 |
+ (DOTween 행 제거 — IEnumerator + Lerp 직접 사용)
```

## 6. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.3 Mediator). ai-assisted 모드, 사용자 "권장안대로" 일괄 채택, AI 권장 채택률 2/2. **5015 RESERVED 상태** — Phase 3~6 보류 + CLAUDE.md §10 즉시 fix.
