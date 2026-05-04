# 5013 EnemyVisibilityCuller v1 — PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 2건 결정 기록
> 모드: ai-assisted (사용자 "권장안대로" 일괄 채택) · 일시: 2026-05-04

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Low | D1 | Throttle 빈도 | **A** — 0.1s (PRD) | Done (변경 X) |
| Low | D2 | Margin 크기 | **A** — 1.5x (PRD) | Done (변경 X) |

## 2. Detailed Decisions

### 2.1 D1 — Throttle 빈도

- **Decision**: **옵션 A — 0.1s (PRD)**
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - 60fps 환경 = 6 frame 간격, 사용자 인지 어려움
  - 5004 SceneTransitionService 의 Time.unscaledDeltaTime 패턴 답습 — pause 시 영향 X
  - 옵션 B (매 프레임) 의 300 적 환경 0.5ms+ 비용 회피
  - 옵션 E (SerializeField) 의 BalanceConfig 변경 commit 회피 — 첫 사이클 단순성
- **PRD Impact**: 변경 X (Sprint 2 그대로)
- **Trade-off**:
  - 옵션 B (매 프레임) 의 즉각 응답 손실 — 빠른 카메라 이동 시 sprite 깜빡임 가능 (미세, margin 1.5x 로 보완)
  - 옵션 E (SerializeField) 의 디자이너 후속 조정 X — 향후 SerializeField 전환 가능 (5013.1 anchor)

### 2.2 D2 — Margin 크기

- **Decision**: **옵션 A — 1.5x viewport (PRD)**
- **Mode**: ai-assisted (사용자 일괄 채택)
- **Rationale**:
  - cap 300 환경 = visible ~50 + margin ~50 = 100, off-screen 200 cull → CPU 30% 절감
  - 옵션 B (1.2x) 의 깜빡임 위험 (빠른 카메라 이동 시 진입 적 sprite 0 frame 활성)
  - 옵션 C (2.0x) 의 cull 효과 ↓ (~150 cull, frame time 마진 ↓)
  - PIVOT v4 §7 cap 300 환경 60fps 마진 ↑ 우선
- **PRD Impact**: 변경 X (Sprint 1 그대로)
- **Trade-off**:
  - 옵션 B 의 cull 효과 ↑ 손실 — Sprint 5 Profiler 결과 부족 시 1.2x 재고 검토 (5013.1 후속)
  - 옵션 D (SerializeField) 의 A/B 테스트 X — 향후 BalanceConfig 도입 가능

## 3. 통계

- **Total Items**: 2
- **AI Recommendations Followed**: 2/2 (100%)
- **User Overrides**: 0
- **모드**: ai-assisted (사용자 "권장안대로" 일괄 채택)

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 v2 초안 → mediator 최종 의 변경사항)

### 변경 0 (D1=A / D2=A 그대로)

- Sprint 1: margin 1.5x 그대로
- Sprint 2: throttle 0.1s 그대로
- 의존성 그래프 + Sprint 표 + Definition of Done 그대로

### 상단 표기 갱신

- "PRD v2 초안 (Rebuttal 단계)" → "PRD v2 (최종, Mediator 확정)"
- "결정 2건 반영 (D1=A / D2=A — 그대로)"

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.3 Mediator). ai-assisted 모드, 사용자 "권장안대로" 일괄 채택. AI 권장 채택률 2/2. **변경 0 (D1=A / D2=A 그대로) — Phase 3 Breakdown 진입 가능**.
