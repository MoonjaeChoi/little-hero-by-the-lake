# 5012 — MPB Hue Shift Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-04 · 총 5 태스크 / 5 commit

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | EnemyHueVariation 정적 헬퍼 | Logic | - | 25분 | _01_hue_variation.md |
| T02 | EnemyController hook + \_baseColor 갱신 | Logic | T01 | 25분 | _02_controller_hook.md |
| T03 | EnemySpawner + WaveManager variation 전달 | Logic | T02 | 30분 | _03_spawner_wave.md |
| T04 | NUnit 6 + ProjectValidator 룰 | Test | T01,T02 | 30분 | _04_test_validator.md |
| T05 | docs (changelog + CLAUDE.md) | Docs | T04 | 15분 | _05_docs.md |

## 2. 의존성 그래프

```
T01 (HueVariation) → T02 (Controller) → T03 (Spawner) ─┐
              ↓                                       ├─→ T04 (Test) → T05 (docs)
              └─ T04 직접 의존 (정적 헬퍼만 검증) ────┘
```

## 3. 병렬 가능성

- 순차 (T01 → T02 → T03 → T04 → T05)
- T04 는 T01 만 직접 의존 (Test 가 정적 헬퍼만 호출) — 다만 Validator 룰이 EnemyController 변경 의존 → T02+T03 후 안전

## 4. 구현 순서 권장

순차 (T01 → T02 → T03 → T04 → T05) — 약 2시간 5분.

## 5. CLAUDE.md §15 정합 자동 검증

- T01: 정적 헬퍼 — §15.1 SRP / §15.7 DI ✅
- T02: 시그니처 default 0 — 회귀 방지 / §15.4 D2=A const 유지 ✅
- T03: spawn 시그니처 default 0 — 호환 ✅
- T04: NUnit 6 (§15.9) + Validator 룰 (§15.12) ✅
- T05: docs ✅

## 6. Unity MCP 안전 전략

- 한 사이클 = 1 태스크 + commit
- 각 commit 사이 컴파일 검증 (refresh + 5초 대기 + read_console)
- T02 EnemyController 수정 시 도메인 리로드 영향 — 5초 대기 필수

## 7. 변경 이력
- 2026-05-04: 초기 작성 (Phase 3 Breakdown).
