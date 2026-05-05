# 5016 LitMotion 마이그레이션 PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-05
> 입력 critique: `5016_litmotion_migration_critique.md` (종합 9.2/10, Accept 12 / Reject 0 / Discuss 4)

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| Sprint 분해 | Sprint 4 작업량 50분 큼 | Medium | **Discuss → D1** | 사용자 결정 필요 | △ |
| 위험 검증 | Burst App 용량 측정 시점 늦음 | Medium | **Discuss → D2** | 사용자 결정 필요 | △ |
| Coroutine 호환 | yield wrapper 사용 영역 모호 | Medium | **Discuss → D3** | 사용자 결정 필요 | △ |
| 검증 추가 | Sprint 1 직후 smoke test | Low | **Discuss → D4** | 사용자 결정 필요 | △ |
| Accept 12 항목 | OOP §15 정합 / Sprint 분해 / 검증 시나리오 등 | - | Accepted | v2_prd 초안 그대로 | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 16 (12 Accept + 4 Discuss)
- Accepted: 12 (75%)
- Rejected: 0 (0%)
- Discuss (사용자 결정): 4 (25%)
- §15 정합률: 100% (위반 0건)

## 3. Detailed Responses

### 3.1 Accept 항목 (12) — 일괄 수용

**Original Criticism**: 12 Accept 항목 (LitMotion 패키지 / LMotionExtensions wrapper / Validator 룰 / Sprint 분해 / 5012 정합 / WithIgnoreTimeScale / MotionHandle 보유 / NUnit 6 / GCAllocMeasurer 재측정 / docs 갱신).

- **Rebuttal**: 모두 수용. CLAUDE.md §15 OOP 정책 + PIVOT v4 §7 성능 목표 정합 검증 완료. PRD 그대로 v2 초안 반영.
- **Proposed Action**: 변경 X — v2_prd.md 는 원본 그대로 (Discuss 4건 만 mediator 후 적용)
- **PRD Impact**: 없음
- **Little Hero Context**: 5012 hue + 5013 visibility culler + WaveTransitionToast baseline 정합 확인.

### 3.2 D1 — Sprint 4 작업량 분해/통합 [Discuss]

**Original Criticism**: Sprint 4 (50분, DamageNumberFloater + Boss attacks 5종) 작업량 큼.

- **Status**: Discuss → discussion_topics.md 로 이동
- **이유**: Sprint 분해는 사용자 결정 사안 (작업 회수성 vs 의미 단위 균형)
- **Current Position**: PRD 그대로 진행 시 1 commit = 1 의미 단위 ("DamageNumberFloater + Boss Lerp 폐기")
- **Resolution Path**: mediator 단계에서 결정 (옵션 A 통합 / B 분리 2/ C 분리 5)

### 3.3 D2 — Burst App 용량 측정 시점 [Discuss]

**Original Criticism**: Burst 의존성 ~10MB+ → 임계 150MB 위반 시 Sprint 1 직후 발견 가능.

- **Status**: Discuss → discussion_topics.md 로 이동
- **이유**: 검증 시점 결정은 Trade-off (이른 검증 비용 vs 늦은 발견 손실)
- **Current Position**: PRD 그대로 진행 시 Sprint 6 빌드 측정만 (5 Sprint 작업 손실 위험)
- **Resolution Path**: mediator 단계에서 결정 (옵션 A 양시점 측정 / B 현재 PRD / C BasicMode 옵션 명시)

### 3.4 D3 — Coroutine ↔ LitMotion 호환 범위 [Discuss]

**Original Criticism**: WaveManager / ChapterRunner / BossEncounterRunner Coroutine 잔존 가능 — 점진 vs 폐기?

- **Status**: Discuss → discussion_topics.md 로 이동
- **이유**: 5016 범위 정의 사안 (큰 Coroutine 폐기 시 작업량 ↑)
- **Current Position**: PRD §3.4 "yield return ToYieldInteraction()" 만 언급 — Sprint 별 적용 영역 불명확
- **Resolution Path**: mediator 단계에서 결정 (옵션 A 잔존 허용 / B 핫 패스 폐기 + 큰 Coroutine 잔존 / C 22 파일 모두 폐기)

### 3.5 D4 — Sprint 1 직후 smoke test [Discuss]

**Original Criticism**: LitMotion v2.0.1 + Unity 6.3 LTS 호환성 검증 시점 부재.

- **Status**: Discuss → discussion_topics.md 로 이동
- **이유**: 검증 시점 사용자 결정 (이른 5분 smoke test vs 자연스러운 Sprint 2 통합 검증)
- **Current Position**: PRD 그대로 진행 시 Sprint 2 마이그레이션 시 호환 발견
- **Resolution Path**: mediator 단계에서 결정 (옵션 A smoke test 추가 / B 현재 PRD / C Sprint 6 NUnit 까지 통합)

## 4. PRD v2 적용 변경 요약

(Accept 12 항목은 모두 원본 PRD 그대로 — 변경 0. Discuss 4건 만 mediator 후 v2 최종 확정.)

- §0 결정 이력: rebuttal 단계 추가
- §1~7: 변경 X (Discuss 미반영)
- 신규 §0.X: D1~D4 결정 후 mediator 가 갱신

## 5. 변경 이력
- 2026-05-05: 초기 작성 (Phase 2.2 Rebuttal)
