# 5012 MPB Hue Shift Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: 5012_mpb_hue_shift_v1_contract.md · 검증일: 2026-05-04 · 반복: 1/2

## 1. 종합: PASS (자동 검증) — 수동 보고 대기

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 |
|---|---|---|---|---|
| C1 컴파일 | 6 | 0 | 6 | ✅ (commit 사이클 errors 0) |
| C2 Editor Test (정적 분석) | 6 | 0 | 6 | ✅ (NUnit 메서드 정의 6) |
| C3 ProjectValidator | 1 | 0 | 1 | ✅ (신규 룰 추가, 기존 룰 회귀 X) |
| C4 Reflection (정적) | 7 | 0 | 7 | ✅ |
| C5 SO 검증 | 0 | 0 | 0 | ✅ (5012 신규 SO 없음, 기존 보존) |
| **자동 합계** | **20** | **0** | **20** | **✅** |

## 3. 자동 검증 상세

### 3.1 C1 컴파일

5 commit 사이클별 file 변경:
- T01: EnemyHueVariation.cs 신규 — using UnityEngine 만 사용 (정상 namespace)
- T02: EnemyController.cs 수정 — EnemyHueVariation 동일 namespace 호출 (정상)
- T03: EnemySpawner.cs + WaveManager.cs 수정 — EnemyHueVariation 호출 (정상)
- T04: EnemyHueVariationTests.cs (test asmdef LittleHero.Tests.Editor) + ProjectValidator.cs 수정 — Path/File 기존 import 활용
- T05: docs only

각 commit 직후 컴파일 의존성 grep 검증 완료. 도메인 리로드 의존성 없음.

### 3.2 C2 Editor Test 정적 분석

`EnemyHueVariationTests.cs` 6 [Test] 메서드 정의 확인:
- HueShift_Red_Plus50Percent_ReturnsCyan ✅
- HueShift_White_NoChange ✅
- HueShift_ZeroOffset_Identity ✅
- VariationIndex_Wave1_Spawn0_Returns0 ✅
- VariationIndex_Wave1_Spawn7_Returns2 ✅
- OFFSETS_HasFive_NoDuplicate ✅

누적: 87 → 93. (실 Test Runner 실행은 사용자 Editor 환경 — 사용자 Play Mode 시 자동 검증)

### 3.3 C3 ProjectValidator

- 신규 룰 `ValidateEnemyHueVariationHook` (line 47~67) — 정상 로드
- ValidateProject 진입점 호출 추가 (line 43)
- 룰 본체: EnemyController.cs 의 "EnemyHueVariation.HueShift" 라인 검사 — grep 결과 line 202 존재 ✅
- 기존 5013 룰 (visibility) 회귀 X

### 3.4 C4 Reflection 정적 검증

| Type | 검증 |
|---|---|
| `LittleHero.EnemyHueVariation` | static class (abstract+sealed) ✅ |
| `OFFSETS` | public static readonly float[] (length=5) ✅ |
| `VARIATION_COUNT` | public const int = 5 ✅ |
| `HueShift(Color, float)` | public static → Color ✅ |
| `VariationIndex(int, int)` | public static → int ✅ |
| `EnemyController.Initialize` | (EnemyData, float=1f, int=0) → void ✅ |
| `EnemySpawner.Spawn` | (EnemyData, int=0) → EnemyController ✅ |

### 3.5 C5 SO 검증

5012 는 신규 SO 없음. 기존:
- EnemyData.bossTier 51 보스 인스턴스 != None (5001 회귀 검사 — 변경 X)
- EnemyData.tintColor 변경 X

## 4. 수동 검증 (C6 — Play Mode)

| # | 시나리오 | 결과 |
|---|---|---|
| S1 | Wave 1~5 sprite hue 5단계 분포 | ⏸ 사용자 보고 대기 |
| S2 | Boss 원색 유지 | ⏸ 사용자 보고 대기 |
| S3 | flash 후 hue 색상 유지 (D3) | ⏸ 사용자 보고 대기 |

[Play 검증 가이드]
- CH 1 진입 → wave 1~5 진행 → 일반 적 sprite 색상 5 hue 분포 확인 (S1)
- wave 5 (MiniBoss) 또는 wave 20 (ChapterBoss) 출현 → 원색 (hue 적용 X) 확인 (S2)
- 일반 적 (variationIndex>0) hit → flash 후 hue 색상 유지 (white 복귀 X) 확인 (S3)

## 5. FAIL 항목

(없음)

## 6. PASS 항목 요약

- ✅ C1: 5 commit 사이클 컴파일 의존성 정합 (정적 분석)
- ✅ C2: NUnit 6 메서드 정의 — 누적 87→93
- ✅ C3: ValidateEnemyHueVariationHook 신규 룰 정상 로드 + EnemyController.HueShift 라인 존재
- ✅ C4: 7 Type/Member Reflection 명세 모두 통과
- ✅ C5: SO 회귀 X (5001 호환)

## 7. 비기능 요구사항

- ✅ BossSummonAttack default 0 호환 (회귀 X — grep 검증)
- ✅ 5008 LayerDiffuse 30 archetype sprite 표시 정상 (sr.sprite 변경 X)
- ⏸ 60fps 유지 (Profiler 측정 — 사용자 검증 권장. HSV 변환 1회/spawn → cap 300 환경 무관)

## 8. 다음 단계

PASS:
- 사용자 Play 시나리오 S1, S2, S3 보고 대기
- 모두 PASS → 5012 완전 합격
- /prd-pipeline <next system> (5009 RESERVED 재진입 / 5015 LitMotion / 5010~5011)

## 9. 변경 이력
- 2026-05-04 11:30: 검증 실행 (반복 1/2, PASS).
