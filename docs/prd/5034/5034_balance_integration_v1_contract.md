# Sprint Contract: 5034 balance_integration_v1

> **PGE Planner — Phase 4 Contract**
> PRD v2 (mediator) · 생성일: 2026-05-07

## 1. C1 컴파일
- 모든 commit (T01~T06) 후 errors=0
- 도메인 리로드 안정화 (isCompiling=false)

## 2. C2 Editor Test (NUnit)

| Test 클래스 | 메서드 수 | 목적 |
|---|---|---|
| `BalanceMatrixTests` | 3 | recommended* 배열 길이 35 + 단조 증가 |
| `DpsCheckTests` | 4 | 챕터 1/10/20/35 권장 진행 시 DPS_player ≥ DPS_target × 0.7 |
| `WeaponClassBonusTests` | 3 | 전사/궁수/마법사 ApplyClassBonus 후 PlayerStats 필드 검증 |
| `StarforceMultiplierTableTests` | 1 | ⭐0~⭐20 단조 증가 (D2=A 검증) |
| `CurveSimulatorTests` | 1 | 통합 시뮬 결과 정합 (D7=A) |

NUnit 누적: baseline +12 (5+ 신규)

## 3. C3 ProjectValidator
- errors=0
- warnings 변동 ±0
- 신규 룰: BalanceConfig.recommended* 배열 길이 35 검증

## 4. C4 Reflection

| Type | Field/Method |
|---|---|
| `BalanceConfig` | recommendedLevelByChapter / recommendedWeaponEnhanceByChapter / recommendedSlotStarforceByChapter (length=35) |
| `PlayerStats` | _bossDamageBonus / _critChanceBonus / _aoeBonus (private) + 6 Add/Remove 메서드 |
| `EffectResolver` 또는 신규 | ApplyClassBonus(PlayerStats, CharacterClass) (public static) |

## 5. C5 SO 검증
- BalanceConfig.asset wiring 확인 (3 배열 모두 길이 35)
- recommendedLevelByChapter[0] = 10, [34] = 50 (대략)
- 단조 증가 (a[i] ≤ a[i+1])

## 6. C6 Play 검증 시나리오

### 시나리오 1 — 챕터 10 권장 진행
1. 마법사 + L20 / 무기 +8 / ⭐3 (D1=A 권장 매트릭스)
2. CH10 진입 → 보스 TTK 30~40s 일관 + DPS check ≥ 0.7×

### 시나리오 2 — 챕터 20 도달
1. L32 / +15 / ⭐10
2. CH20 진입 → 마법 결정 ◇ unlock 확인 + DPS check 통과

### 시나리오 3 — 챕터 35 클리어
1. L50 / +25 / ⭐15~⭐20
2. CH35 보스 TTK 50~67s + 신화 발현 + 엔드콘텐츠 진입

## 7. 합격 기준
- **최소**: C1~C5 모두 ✅ + 시나리오 1+2 ✅
- **완전**: 모두 ✅ + 시나리오 1+2+3 + Profiler GC <50KB/s

## 8. 변경 이력
- 2026-05-07: 초기 작성 (Phase 4 Contract)
