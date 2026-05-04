# Sprint Contract: 5012 — MPB Hue Shift v1

> **PGE Planner — Phase 4 Contract**
> PRD: 5012_mpb_hue_shift_v1_v2.md · breakdown: _breakdown_00.md
> 생성일: 2026-05-04

---

## 1. C1 컴파일 기준

- **C1.1**: 신규/수정 .cs 파일 컴파일 errors=0
  - EnemyHueVariation.cs (신규)
  - EnemyController.cs (수정)
  - EnemySpawner.cs (수정)
  - WaveManager.cs (수정)
  - EnemyHueVariationTests.cs (신규)
  - ProjectValidator.cs (수정)
- **C1.2**: 도메인 리로드 후 5초 내 안정화 (`editor_state.isCompiling == false`)

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 5
mcp__UnityMCP__read_console (errors)  # 0
```

---

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 수 | 목적 |
|---|---|---|
| `EnemyHueVariationTests` | 6 | HueShift / VariationIndex / OFFSETS 단위 검증 |

### 2.1 EnemyHueVariationTests 명세

- `HueShift_Red_Plus50Percent_ReturnsCyan` — red(h=0) + 0.5 → cyan(h=0.5) hue rotation
- `HueShift_White_NoChange` — saturation=0 edge (white 무변화)
- `HueShift_ZeroOffset_Identity` — offset=0 항등
- `VariationIndex_Wave1_Spawn0_Returns0` — (1-1)*7+0 % 5 = 0
- `VariationIndex_Wave1_Spawn7_Returns2` — 7 prime spread (7 % 5 = 2)
- `OFFSETS_HasFive_NoDuplicate` — 5 unique const

**검증 방법**: Test Runner Edit Mode > Run All → 6/6 PASS, 누적 87 → 93

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings ≤ 기존 (5013 종료 시점 기준)
- **C3.3**: 신규 위반 없음 (100줄+ / public 필드 / Update in 1500)
- **C3.4**: 신규 룰 `EnemyController_HueVariation_Hook` — EnemyController.cs 에 `EnemyHueVariation.HueShift` 호출 라인 존재

**검증 방법**: `Tools/LittleHero/Validate Project` 실행 → 콘솔 출력 비교

---

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.EnemyHueVariation` (static class) | sealed=true, abstract=true |
| `LittleHero.EnemyHueVariation.OFFSETS` (public static readonly float[]) | Length=5 |
| `LittleHero.EnemyHueVariation.VARIATION_COUNT` (public const int) | =5 |
| `LittleHero.EnemyHueVariation.HueShift` (public static) | (Color, float) → Color |
| `LittleHero.EnemyHueVariation.VariationIndex` (public static) | (int, int) → int |
| `LittleHero.EnemyController.Initialize` | (EnemyData, float, int) → void (3 params, default 0) |
| `LittleHero.EnemySpawner.Spawn` | (EnemyData, int) → EnemyController (2 params, default 0) |

**검증 방법**:
```csharp
var t = System.Type.GetType("LittleHero.EnemyHueVariation, Assembly-CSharp");
return t != null && t.IsAbstract && t.IsSealed
    && t.GetField("OFFSETS") != null
    && t.GetMethod("HueShift", new[]{typeof(Color), typeof(float)}) != null;
```

---

## 5. C5 SO 검증

(5012 는 신규 SO 없음 — 기존 EnemyData 의 bossTier / tintColor 만 사용)

| 검증 항목 | 기준 |
|---|---|
| EnemyData.bossTier 51 보스 인스턴스 | != BossTier.None (5001 호환) |
| EnemyData.tintColor 모든 인스턴스 | hue shift 결과 합리 (Color.white 또는 saturated color) |

**검증 방법**: 기존 5001 검증 재활용 (회귀 검사)

---

## 6. C6 Play 검증 시나리오

### S1: Wave 1~5 sprite hue 5단계 분포

**사전 조건**: CH 1 진입 가능 / 5012 코드 적용

**단계**:
1. CH 1 진입 → wave 1 시작
2. 일반 적 (BossTier=None) 5+ 마리 출현 확인
3. 각 적 sprite 색상 시각 비교 (5 hue 분포)
4. wave 2~5 진행 → 각 wave 의 sprite 색상 비교

**기대 결과**:
- 일반 적 sprite 색상 5단계 분포 (-20%, -10%, 0%, +10%, +20% hue)
- 같은 archetype 도 색상 다양화

---

### S2: Boss 원색 유지

**사전 조건**: CH 1 wave 5 (MiniBoss) 또는 wave 20 (ChapterBoss)

**단계**:
1. wave 5 진입 → MiniBoss 출현
2. Boss sprite 색상 확인

**기대 결과**:
- Boss sprite 원색 (variationIndex=0 강제, hue 적용 X)
- 시각 일관성 유지

---

### S3: flash 후 hue 색상 유지 (D3 검증)

**사전 조건**: 일반 적 (variationIndex>0) 처치 시도

**단계**:
1. 일반 적 1마리 hit → flash 효과 (red 또는 yellow)
2. flash 종료 후 \_baseColor 복귀
3. sprite 색상 확인

**기대 결과**:
- flash 후 hue 색상 유지 (white 로 복귀 X)
- D3=B \_baseColor 갱신 정합

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0
- [ ] Editor Test 6/6 PASS (누적 93)
- [ ] ProjectValidator warnings 변동 ≤ 0
- [ ] 기존 시스템 회귀 X (BossEncounterRunner / InfiniteTowerRunner Spawn 호출 default 0 호환)
- [ ] PIVOT v4 §7 cap 300 60fps 유지 (HSV 변환 1회/spawn — Update X)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 5/6 이상 ✅
- C3 Validator errors=0 ✅
- 시나리오 S1, S2 PASS

**완전 합격**:
- 모든 카테고리 ✅
- 시나리오 S1, S2, S3 모두 PASS
- 비기능 요구사항 모두 ✅

## 9. Trade-off (mediator 결정 기반)

- D1=B → SpriteRenderer.color multiplicative tint. Trade-off: white sprite 무변화 (디자인 의도 — sprite 자체 색상 강하면 변형 적음 OK).
- D2=A → const 유지. Trade-off: 디자이너 후속 조정 시 코드 수정 (변경 빈도 낮음).
- D3=B → \_baseColor hue 결과 캐시. Trade-off: Initialize 코드 흐름 명시 필수 (회귀 방지).

## 10. 변경 이력
- 2026-05-04: 초기 작성 (Phase 4 Contract).
