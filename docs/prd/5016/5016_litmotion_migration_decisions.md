# 5016 LitMotion 마이그레이션 — PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 4건 결정 기록
> 모드: ai-assisted (사용자 "권장안대로" 일괄 채택) · 일시: 2026-05-05

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | Sprint 4 작업량 분해/통합 | **A 통합 유지** (1 commit) | Done |
| Medium | D2 | Burst App 용량 측정 시점 | **A 양시점** (Sprint 1 + 6) | Done |
| Medium | D3 | Coroutine 호환 범위 | **B 핫 패스 22 폐기 + 큰 timing Coroutine 잔존** | Done |
| Medium | D4 | Sprint 1 smoke test | **A 추가** (5분) | Done |

## 2. Detailed Decisions

### 2.1 D1 — Sprint 4 작업량 분해/통합

- **Decision**: **A 통합 유지** (Sprint 4 = DamageNumberFloater + Boss attacks 5종 1 commit)
- **Mode**: ai-assisted (Claude 권장 + 사용자 일괄 채택)
- **Rationale**:
  - 1 commit = 1 의미 단위 ("Lerp 폐기 묶음")
  - Sprint 5 (45분) 와 유사 작업량 — 6 Sprint 균형 유지
  - 회수 시 5종 boss attacks + Floater 영역 차이 작음 (각 ~5L 변경)
  - CLAUDE.md §15.10 "단일 책임 commit" 정합 (한 commit = "Lerp 폐기")
- **PRD Impact**:
  - §4 Sprint 표 그대로 (6 commit / 4시간)
  - 변경 없음
- **Trade-off**: 영역 분리 시 부분 회수성 ↑. 그러나 작업량 차이 작음 → 의미 단위 우선.

### 2.2 D2 — Burst App 용량 측정 시점

- **Decision**: **A 양시점 측정** (Sprint 1 직후 + Sprint 6 양시점)
- **Mode**: ai-assisted (사용자 일괄 채택)
- **Rationale**:
  - Burst native ~10MB+ → 임계 150MB 위반 시 Sprint 1 직후 발견하면 5015c 빠른 재정의
  - 5 Sprint 작업 손실 (5h) 회피
  - Player 빌드 측정 비용 +5분 (cheap 검증)
- **PRD Impact**:
  - §4 Sprint 1 commit 메시지 [검증] 항목 추가:
    - "Player 빌드 (Mac/iOS) — App 용량 임계 150MB 내 확인"
  - §6 위험 표 갱신 — "Burst 의존성 (~10MB+) App 용량" 행에 "Sprint 1 직후 + Sprint 6 양시점 측정 (조기 발견)" 추가
  - §5 검증 시나리오 S0.1 신규 — Sprint 1 commit 후 빌드 측정
- **Trade-off**: Sprint 1 +5분 추가 비용. 그러나 5 Sprint 손실 위험 회피 가치 큼.

### 2.3 D3 — Coroutine ↔ LitMotion 호환 범위

- **Decision**: **B 핫 패스 22 파일 폐기 + 큰 timing Coroutine 잔존** (5016 범위 외 명시)
- **Mode**: ai-assisted (사용자 일괄 채택)
- **Rationale**:
  - 핫 패스 (cap 300 환경 매 hit/매 critical) GC 0 효과 명확 — 5016 핵심 가치
  - 큰 Coroutine (WaveManager 5초 휴식, ChapterRunner Wave 진행, BossEncounterRunner 진입) = timing-only Sequencer
    - IEnumerator 가 더 자연스러움 (yield WaitForSeconds + 단순 진행)
    - LSequence 분해 시 가독성 ↓ + GC 효과 미미 (호출 빈도 낮음)
  - 5016 범위 명확 → 후속 사이클 (5017+) 에서 필요 시 재방문
- **PRD Impact**:
  - §1.1 인벤토리 표 갱신 — "핫 패스 22 파일 만, 큰 timing Coroutine (WaveManager/ChapterRunner/BossEncounterRunner) 잔존" 명시
  - §3.4 Coroutine 호환 — "ToYieldInteraction() 활용은 Sprint 5 일부만, 큰 Coroutine 은 5016 범위 외" 추가
  - §6 위험 표 갱신 — "WaveManager/ChapterRunner Coroutine 회귀" 행 → "5016 범위 외, IEnumerator 그대로 유지"
- **Trade-off**: 시각/패러다임 일관성 ↓. 그러나 GC 0 효과 큰 핫 패스 우선.

### 2.4 D4 — Sprint 1 직후 smoke test

- **Decision**: **A 5분 smoke test 추가** (Sprint 1 commit 후 LMotion.Create + Bind 동작 검증)
- **Mode**: ai-assisted (사용자 일괄 채택)
- **Rationale**:
  - LitMotion v2.0.1 (2025-02-09) + Unity 6.3 LTS (Unity 6000.4.4f1) 호환 검증 0
  - Sprint 1 commit 후 5분 추가 검증 → 실패 시 5015c (DOTween/UniTask) 재정의 빠름
  - D2 양시점 측정 (Player 빌드) 와 동시 진행 가능 — 5분 비용 ↓
- **PRD Impact**:
  - §4 Sprint 1 작업 항목 추가:
    - "1.3 Smoke test — Editor 메뉴 또는 단순 NUnit 1 (LMotion.Create + Bind 동작 검증)"
  - §5 검증 시나리오 S0 신규:
    - "S0.2 — Sprint 1 직후 5분 smoke test (LMotion.Create(0f, 1f, 0.1f).Bind(...) → 결과 검증)"
  - §4 Sprint 1 commit 메시지 [검증] 항목 추가
- **Trade-off**: Sprint 1 +5분. 그러나 호환 검증 이른 발견 가치 큼.

## 3. 통계

- Total Items: 4
- AI Recommendations Followed: 4 / 4 (100%)
- User Overrides: 0
- 모드: ai-assisted (사용자 "권장안대로" 일괄 채택)

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 v2 초안 → mediator 최종 변경사항)

### Sprint 표

- Sprint 표 그대로 (6 commit / 4시간) — D1=A 결정
- Sprint 1 작업 항목 +2건 추가 (D2 양시점 + D4 smoke test)
  - 1.3 Smoke test (5분)
  - 1.4 Player 빌드 후 App 용량 측정 (5분)

### §1.1 인벤토리 명확화 (D3=B)

- 표 상단에 명시: "핫 패스 22 파일 만 마이그레이션. 큰 timing Coroutine (WaveManager/ChapterRunner/BossEncounterRunner) = 5016 범위 외, IEnumerator 잔존 허용"

### §5 검증 시나리오 신규 S0

- S0.1: Sprint 1 commit 후 Player 빌드 (App 용량 임계 150MB 내)
- S0.2: Sprint 1 직후 5분 smoke test (LMotion.Create + Bind 동작)

### §6 위험 표 갱신

- "Burst 의존성" 행 → "Sprint 1 직후 + Sprint 6 양시점 측정"
- "WaveManager/ChapterRunner Coroutine 회귀" 행 → "5016 범위 외, IEnumerator 그대로 유지"

## 5. 변경 이력
- 2026-05-05: 초기 작성 (Phase 2.3 Mediator, ai-assisted 권장안 일괄 채택)
