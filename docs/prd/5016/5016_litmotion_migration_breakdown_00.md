# 5016 LitMotion 마이그레이션 — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-05 · 총 6 태스크 / 6 commit / ~4시간

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | LitMotion 패키지 + LMotionExtensions wrapper + Sprint 1 smoke + Player 빌드 | Core | - | 30분 + 10분 (smoke + 빌드) | _01_*.md |
| T02 | EnemyController PunchRoutine + FlashRoutine | Logic | T01 | 40분 | _02_*.md |
| T03 | CinematicEffects HitPause + Shake + Zoom | Logic | T01 | 40분 | _03_*.md |
| T04 | DamageNumberFloater + Boss attacks 5종 (Vine/Tornado/Summon/Spread/Pulse) | Logic | T01 | 50분 | _04_*.md |
| T05 | UI 영역 — FadeOverlay + WaveTransitionToast + HUDController + MythicEffectOverlay | UI | T01 | 45분 | _05_*.md |
| T06 | Validator + NUnit 6 + GCAllocMeasurer 재측정 + docs (changelog + CLAUDE.md) | Test+Docs | T02-T05 | 35분 | _06_*.md |
| **총** | | | | **~4시간** | |

## 2. 의존성 그래프

```
T01 (인프라 + wrapper + smoke + 빌드) ──┐
                                       ├→ T02 (EnemyController) ──┐
                                       ├→ T03 (CinematicEffects) ─┤
                                       ├→ T04 (Floater + Boss 5)──┼→ T06 (Validator + Test + docs)
                                       └→ T05 (UI 4) ──────────────┘
```

T02~T05 = 영역 독립 (병렬 가능). 컴파일 통합 + 회귀 검증 위해 순차 권장.

## 3. 병렬 가능성

- **병렬 그룹 1**: T01 (단독 — 패키지 + wrapper 의존성 없음)
- **병렬 그룹 2** (T01 후): T02 / T03 / T04 / T05 — 4 영역 독립
- **직렬**: T06 (T02~T05 모두 의존 — 통합 검증 + GC 재측정)

## 4. 구현 순서 권장 (순차 진행)

1. T01 (40분, smoke + 빌드 포함)
2. T02 (40분)
3. T03 (40분)
4. T04 (50분)
5. T05 (45분)
6. T06 (35분)

총 ~4시간 (병렬 시 ~3.5시간 — 효과 작음, 순차 권장).

## 5. CLAUDE.md §15 정합 자동 검증

- T01: LMotionExtensions ~70L — §15.6 100L ✅, 정적 헬퍼 — §15.7 ✅, MIT — §15.10 ✅
- T02: PunchRoutine → DoPunch (LSequence), FlashRoutine → DoFlash — Coroutine state machine alloc 0 ✅
- T03: HitPauseRoutine → PlayHitPause (WithOnComplete), ShakeRoutine → PlayScreenShake (LMotion.Shake new Vector3 alloc 0 ✅)
- T04: DamageNumberFloater Lerp → LMotion.Bind (매 프레임 Lerp 폐기), Boss attacks 5종 정합
- T05: FadeOverlay/WaveTransitionToast/HUDController/MythicEffectOverlay → LMotion (UI fade/scale/floater)
- T06: ProjectValidator LitMotion_Migration 룰 — §15.12 ✅, NUnit 6 — §15.9 ✅

## 6. Unity MCP 안전 전략 (CLAUDE.md §13)

- 한 사이클 = 1 태스크 + commit + 5~10s 대기
- 도메인 리로드 위험: T01 (패키지 설치 + 신규 wrapper) 직후 10s+ 대기
- T02~T05 각 commit 후 5s 대기 (단순 메서드 마이그레이션)
- T06 Validator 실행 — 모든 commit 후 1회 일괄

## 7. 5016 범위 외 (Mediator D3=B 결정)

다음 Coroutine 은 5016 범위 외 (IEnumerator 잔존):

- `WaveManager` 의 5초 휴식 / wave 진행 timing
- `ChapterRunner` 의 Wave 1→2→3 timing
- `BossEncounterRunner` 의 보스 진입 + 종결 timing

이유: 단순 `WaitForSeconds` + 진행 — IEnumerator 가 가독성 ↑ + GC 효과 미미 (호출 빈도 낮음). 후속 사이클 (5017+) 에서 필요 시 재방문.

## 8. 변경 이력
- 2026-05-05: 초기 작성 (Phase 3 Breakdown)
