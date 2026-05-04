# 5013 EnemyVisibilityCuller v1 — PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> 일시: 2026-05-04 · 출처: critique 3 Discuss 중 D3 자동 수용 + 추가 관찰 3건 자동 수용 → 2 Discuss 잔여 (D1 throttle / D2 margin)

## High Priority Items

(없음)

## Medium Priority Items

(없음 — Discuss 모두 Low Priority 수치 밸런스)

## Low Priority Items

### D1. Throttle 빈도

- **Original Criticism**: Sprint 2 의 throttle = 0.1s. 너무 짧으면 비용 ↑ / 너무 길면 화면 진입 적 sprite 깜빡임.
- **Current Position**: 0.1s 로 진행 시 60fps 환경 = 6 frame 간격, 사용자 인지 어려움.
- **Discussion Points**:
  - **옵션 A. 0.1s (PRD)** — 균형 (10 회/초 visibility 체크)
    - 장점: 비용 / 정확성 균형
    - 단점: 빠른 카메라 이동 시 sprite 깜빡임 가능
  - **옵션 B. 매 프레임** — 정확성 ↑
    - 장점: 즉각 응답
    - 단점: 300 적 환경 0.5ms+ 비용 (60fps 마진 ↓)
  - **옵션 C. 0.05s** — 더 정확 (20 회/초)
    - 장점: 깜빡임 회피 + 비용 적당
    - 단점: B 와 큰 차이 없음
  - **옵션 D. 0.2s + margin 2.0x** — 비용 최소
    - 장점: 비용 ↓ + sprite 깜빡임 회피 (margin 으로 보완)
    - 단점: D2 margin 결정과 결합 (옵션 D2=C 강제)
  - **옵션 E. SerializeField (BalanceConfig)** — 디자이너 후속 조정
    - 장점: §15.4 SO 우선 정합, A/B 테스트 가능
    - 단점: BalanceConfig 변경 commit
- **Proposed Resolution Path**: **옵션 A 권장** — 검증된 5004 패턴 (Time.unscaledDeltaTime) + 60fps 환경 6 frame 간격 사용자 인지 어려움.
- **PRD Impact**:
  - 옵션 A: 변경 X (Sprint 2 그대로)
  - 옵션 D: D2 margin = 2.0x 강제 + Sprint 1 수정
  - 옵션 E: BalanceConfig.cullerThrottleSec SerializeField 신규 + Sprint 1+2 수정

---

### D2. Margin 크기

- **Original Criticism**: Sprint 1 의 margin = 1.5x viewport. 화면 진입 적 sprite 깜빡임 회피 + 비용 균형.
- **Current Position**: 1.5x 로 진행 시 viewport 외부 50% 영역도 활성. cap 300 환경 ~50 visible + ~50 margin = 100, off-screen 200 cull.
- **Discussion Points**:
  - **옵션 A. 1.5x (PRD)** — 균형
    - 장점: 화면 진입 적 사전 활성, cull 200 적
    - 단점: margin 50 적 활성 (frame time 영향 일부)
  - **옵션 B. 1.2x (보수)** — 화면 가까운 적만
    - 장점: cull 효과 ↑ (~240 cull)
    - 단점: 빠른 카메라 이동 시 깜빡임
  - **옵션 C. 2.0x (안전)** — 화면 외 100% 활성
    - 장점: 깜빡임 X
    - 단점: cull 효과 ↓ (~150 cull, frame time 마진 ↓)
  - **옵션 D. SerializeField (BalanceConfig)** — D1 옵션 E 와 결합
    - 장점: A/B 테스트 가능
    - 단점: 위와 동일
- **Proposed Resolution Path**: **옵션 A 권장** — 균형 + 60fps 마진 ↑ (cull 200 적 = CPU 30% 절감).
- **PRD Impact**:
  - 옵션 A: 변경 X
  - 옵션 D: BalanceConfig.cullerMargin SerializeField 신규

---

## 요약

- **Total Items**: 2
- **High**: 0 / **Medium**: 0 / **Low**: 2 (D1 throttle / D2 margin)
- **권장 mode**: ai-assisted (Claude 추천 + 사용자 확인)
- **권장 결정 일괄 입력**: `D1=A, D2=A` 또는 `권장안대로`
- **대안**: `D1=E, D2=D` (둘 다 SerializeField — 디자이너 후속 조정)
