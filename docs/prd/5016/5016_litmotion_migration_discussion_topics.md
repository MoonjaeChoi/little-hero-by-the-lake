# 5016 LitMotion 마이그레이션 — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.

## High Priority Items

(없음)

## Medium Priority Items

### D1. Sprint 4 작업량 (50분) 분해/통합

- **Original Criticism**: Sprint 4 = DamageNumberFloater + Boss attacks 5종 (50분, 큰 작업량)
- **Current Position**: PRD 그대로 진행 시 1 commit ("DamageNumberFloater + Boss Lerp 폐기")
- **Discussion Points**:
  - **옵션 A (★권장)**: 통합 유지 (1 commit, 6 commit 총)
    - 장점: 1 commit = 1 의미 단위 ("Lerp 폐기 묶음"), 단순성
    - 단점: 회수 시 5종 boss attacks + Floater 동시 회수
  - **옵션 B**: Sprint 4a (Floater) + Sprint 4b (Boss attacks 5종) 분리 (2 commit, 7 commit 총)
    - 장점: 핫 패스 (Floater) vs 콜드 패스 (Boss attacks) 분리, 부분 회수 용이
    - 단점: 의미 단위 약화 (양쪽 모두 Lerp 폐기)
  - **옵션 C**: Boss attacks 5종 → T04~T08 분리 (5 commit, 10 commit 총)
    - 장점: 보스 별 Lerp 패턴 다름 → 영역 분리 명확, 회귀 추적 쉬움
    - 단점: 작업량 ↑ (commit 메시지 5회 작성)
- **Proposed Resolution Path**: 옵션 A — 핫 패스 Floater 와 콜드 Boss 영역이 묶여도 1 commit 의미 명확 ("Lerp 폐기 묶음"). Sprint 4 50분은 1 commit 적정 크기 (Sprint 5 의 45분과 유사).
- **PRD Impact**: §4 Sprint 표 (옵션 B/C 시 commit 수 갱신)

### D2. Burst 의존성 App 용량 측정 시점

- **Original Criticism**: Burst native ~10MB+ → 임계 150MB 위반 시 Sprint 6 까지 발견 늦음
- **Current Position**: PRD 그대로 진행 시 Sprint 6 빌드 측정만 (5 Sprint 작업 손실 위험)
- **Discussion Points**:
  - **옵션 A (★권장)**: Sprint 1 직후 + Sprint 6 양시점 측정 (이른 검증 + 최종 검증)
    - 장점: 이른 발견 (5 Sprint 손실 회피), 검증 비용 +5분 (Player 빌드)
    - 단점: Sprint 1 commit 후 추가 시간
  - **옵션 B**: Sprint 6 만 (현재 PRD)
    - 장점: 단순 (검증 1회)
    - 단점: 발견 늦음 (5 Sprint 작업 손실 가능)
  - **옵션 C**: Sprint 1 직후 + Sprint 6 + Burst 미사용 옵션 (LitMotion BasicMode) 사전 명시
    - 장점: 최악 시 폴백 옵션 명시
    - 단점: PRD 복잡도 ↑
- **Proposed Resolution Path**: 옵션 A — Sprint 1 직후 5분 추가 측정으로 5 Sprint 작업 손실 위험 회피. Burst 의존성 임계 150MB 영향 사전 검증 가치 큼.
- **PRD Impact**: §4 Sprint 1 검증 항목 + §6 위험 표 갱신

### D3. Coroutine ↔ LitMotion 호환 범위

- **Original Criticism**: WaveManager / ChapterRunner / BossEncounterRunner Coroutine 잔존 가능
- **Current Position**: PRD §3.4 "yield return ToYieldInteraction()" 만 언급 — Sprint 별 영역 불명확
- **Discussion Points**:
  - **옵션 A**: Sprint 5 일부 (FadeOverlay) 만 호환 wrapper — Coroutine 잔존 허용 (5016 범위 외)
    - 장점: 작업량 ↓, 큰 Coroutine (WaveManager 의 5초 휴식, ChapterRunner Wave 진행) 안정성 ↑
    - 단점: 시각 불일관 (일부 Lerp 잔존)
  - **옵션 B (★권장)**: 핫 패스 (Sprint 2~5 의 22 파일) 모두 MotionHandle 폐기 + 큰 Coroutine (WaveManager/ChapterRunner/BossEncounterRunner 의 timing-only Sequencer) 잔존
    - 장점: GC 0 효과 명확 (핫 패스), 큰 Coroutine 의 단순한 timing 은 IEnumerator 가 더 자연스러움
    - 단점: 5016 범위 명시 필요 (22 파일 의 세부 영역)
  - **옵션 C**: 22 파일 + 큰 Coroutine 도 LSequence 분해 (모든 Coroutine 폐기)
    - 장점: 단일 패러다임
    - 단점: 작업량 ↑ (Sprint 7~8 추가)
- **Proposed Resolution Path**: 옵션 B — 핫 패스 22 파일 폐기 (PRD §1.1 인벤토리 그대로) + 큰 Coroutine timing-only Sequencer 잔존 (5016 범위 외 명시).
- **PRD Impact**: §1.1 인벤토리 명확화 ("핫 패스 22 파일 만 마이그레이션, 큰 timing Coroutine 잔존") + §6 위험 표 갱신

### D4. Sprint 1 직후 smoke test

- **Original Criticism**: LitMotion v2.0.1 + Unity 6.3 LTS 호환성 검증 시점 부재
- **Current Position**: PRD 그대로 진행 시 Sprint 2 EnemyController 마이그레이션 시 발견
- **Discussion Points**:
  - **옵션 A (★권장)**: Sprint 1 직후 5분 smoke test (LMotion.Create + Bind + 결과 검증) 추가
    - 장점: 이른 호환 검증, 실패 시 5015c 재정의 빠름
    - 단점: Sprint 1 +5분
  - **옵션 B**: Sprint 2 마이그레이션 시 통합 검증 (현재 PRD 묵시적)
    - 장점: 자연스러운 검증
    - 단점: Sprint 1 commit 시점 알 수 없음
  - **옵션 C**: Sprint 6 NUnit 까지 통합 (가장 늦음)
    - 장점: 최종 NUnit 가 단일 진실원
    - 단점: 5 Sprint 작업 손실 가능
- **Proposed Resolution Path**: 옵션 A — Sprint 1 commit 직후 5분 smoke test (Editor 메뉴 또는 단순 NUnit 1) 추가. D2 양시점 측정 및 호환 검증 동시 진행 가능.
- **PRD Impact**: §4 Sprint 1 검증 항목 추가 + §5 검증 시나리오 (S0 신규 — Sprint 1 직후 smoke test)

## Low Priority Items

(없음)

## 요약
- Total Items: 4
- High: 0 / Medium: 4 / Low: 0
- 권장 mode: ai-assisted (Claude 추천 + 사용자 확인)

## Claude 권장 일괄 (사용자 "권장안대로" 시 적용)

| Item | Recommendation | Rationale |
|---|---|---|
| D1 | A 통합 유지 | 1 commit = 1 의미 단위 + Sprint 5 (45분) 와 유사 작업량 |
| D2 | A 양시점 측정 | 5 Sprint 손실 위험 회피 + 5분 추가 비용 작음 |
| D3 | B 핫 패스 22 파일 + 큰 Coroutine 잔존 | GC 0 효과 명확 + 큰 timing Coroutine 자연스러움 |
| D4 | A 5분 smoke test | 호환 검증 이른 발견 + D2 와 동시 |
