# Sprint Contract: 5017 GC Alloc Follow-up

> **PGE Planner — Phase 4 Contract**
> PRD: `5017_gc_alloc_followup_v2.md` · 일시: 2026-05-06

## 1. C1 컴파일

- 모든 신규 .cs 컴파일 errors=0
- 5초 도메인 리로드 안정화

## 2. C2 Editor Test

| Test 클래스 | 신규 메서드 |
|---|---:|
| `DamageNumberFloaterFormatTests` (T04) | 4 |
| **누적** | **104 → 108** |

명세:
- `FormatDamage_BelowThousand_NoSuffix`
- `FormatDamage_OverMillion_MSuffix`
- `FormatDamage_Cache_RepeatedCallReturnsSameInstance`
- `Type_DamageNumberFloater_FormatDamageIsStatic`

## 3. C3 ProjectValidator

- errors=0
- 신규 위반 0
- `ValidateGCAllocPatterns` 통과 로그

## 4. C4 Reflection

| Type | Member |
|---|---|
| `DamageNumberFloater` | `FormatDamage` static method |
| `LittleHero.DropContext` | struct (`IsValueType == true`) |
| `EnemyController` | `_lastWobble` / `_lastPunch` 필드 |

## 5. C5 SO 검증

- 코드 영역 — SO 변경 0

## 6. C6 Play 시나리오

### S1 — DamageNumberFloater 시각 (T01 후)
- floater 표시 동일 (1.2K / 15K / 1.5M)
- cache hit 검증 (NUnit)

### S2 — DropContext 처치 보상 (T02 후)
- 처치 → 별가루 + 아이템 드랍 정상

### S3 — wobble 시각 (T03 후)
- 적 cap 300 wobble 자연스러움
- frame rate 60 fps

### S4 — Mac Player 재측정 (T05 후) ⭐ 핵심
- **시나리오 A (<30 KB/s) 도달** ← 5017 성공 기준

## 7. 합격 기준

**최소**: C1~C4 + S1 + S4
**완전**: 모든 카테고리 + S1~S4 + 시나리오 A 도달

## 8. Trade-off (decisions 기반)

- D1=A: cap 1024 메모리 30KB 영구
- D2=A: struct by-value 변경 lost 위험 (호출처 grep)
- D3=B: 0.005f 임계 — 시각 미미 차이

## 9. 변경 이력
- 2026-05-06: 초기 작성
