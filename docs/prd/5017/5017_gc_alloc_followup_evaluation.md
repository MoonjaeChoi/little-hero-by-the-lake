# 5017 — Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: `5017_gc_alloc_followup_contract.md` · 검증일: 2026-05-06 · 반복: 1/2

## 1. 종합: PARTIAL PASS (자동 PASS, 핵심 시나리오 S4 ❌)

## 2. 자동 검증 매트릭스

| 카테고리 | 결과 | 판정 |
|---|---|---|
| C1 컴파일 | errors=0 (모든 sprint) | ✅ |
| C2 Editor Test | DamageNumberFloaterFormatTests 4/4 PASS, 누적 108 | ✅ |
| C3 ProjectValidator | 5017 GC alloc 룰 통과 (DropContext struct + FormatCacheCount 검증) | ✅ |
| C4 Reflection | DropContext IsValueType ✅ / FormatDamage static ✅ / _lastWobble 필드 ✅ | ✅ |
| C5 SO 검증 | 코드 영역만 — 변경 0 | ✅ |
| **자동 합계** | **PASS** | **✅** |

## 3. 수동 검증 (Play 시나리오 — Mac Player)

| # | 시나리오 | 결과 |
|---|---|---|
| S1 | DamageNumberFloater 시각 (cache hit) | ✅ NUnit 검증 |
| S2 | DropContext 처치 보상 | (사용자 검증 권장 — Play Mode) |
| S3 | wobble 시각 | (사용자 검증 권장 — 60fps) |
| **S4** | **Mac Player GC 시나리오 A (<30 KB/s)** | **❌ 80.76 KB/s (시나리오 C)** |

## 4. FAIL 항목

### S4 — 시나리오 A (<30 KB/s) 미달

- **Contract 명세**: Mac Player 측정 시나리오 A
- **실제**: 80.76 KB/s (시나리오 C, 임계의 1.6×)
- **5016.1 비교**: 70 KB/s → 80 KB/s (측정 변동, fix 효과 미미)

**원인 분석**:
1. **T01 FormatDamage cache** — cap 300 환경 damage 다양 → cache miss ↑
2. **T02 DropContext struct** — 매 처치 1 alloc 절감, hit 빈도 대비 작음
3. **T03 wobble setter** — GC 와 무관 (native interop 절감)
4. **잔여 hot path** — Profiler 없이 식별 불가

## 5. 결정 — 5017 부분 종결 + 후속 사이클 보류

### 5.1 부분 종결 이유

- 자동 검증 모두 PASS ✅ (C1~C5)
- Mac Player 측정 시나리오 C 잔존 — 추측 기반 fix 한계
- Profiler attach 의존성 (Mac Player Development Build 자동 listen 미작동)
- TestFlight 단계 (실 device) 에서 더 정확한 baseline 가능

### 5.2 후속 검토 옵션

| 옵션 | 작업 | 시간 |
|---|---|---|
| 5017.1 PRD | Profiler 정밀 분석 → 정확한 hot path fix | 2~3시간 (사용자 Profiler 진행 필요) |
| TestFlight 단계 검증 | iOS device 실측 후 결정 | Phase 4 마일스톤 |
| 보류 + 다음 시스템 | 70~90 KB/s 를 PG-12 카주얼 게임 + 60fps 환경에서 수용 | 즉시 |

## 6. 비기능 요구사항

- ✅ C# 컴파일 errors=0
- ✅ NUnit 108/108 PASS
- ✅ ProjectValidator warnings 변동 X + 5017 룰 신규 통과
- ⏸ 60fps 유지 (사용자 Play Mode 검증 대기)
- ❌ **GC <30 KB/s 도달** (S4 미달)

## 7. 다음 단계

5017 **부분 종결**. 5018+ 또는 후속 사이클에서:
- Profiler 정밀 분석 (사용자 직접 Profiler attach + Hierarchy GC Alloc 정렬)
- TestFlight 단계 (실 device 측정)

## 8. 변경 이력
- 2026-05-06: 검증 실행 (반복 1/2) — C1~C5 자동 PASS, S4 FAIL (부분 종결)
