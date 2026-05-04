# 5012 — MaterialPropertyBlock Hue Shift — Sprint PRD

> **출처**: `docs/prd/5012_mpb_hue_shift_v1_prd.md` (디자인) — 옵션 B (HSV → SpriteRenderer.color)
> **범위**: 5 단계 hue 변형 + EnemyController hook + Boss 예외 + Test

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | EnemyHueVariation 정적 헬퍼 (~70L) | 1 | 25분 |
| 2 | EnemyController.Initialize variationIndex hook | 1 | 20분 |
| 3 | WaveManager / EnemySpawner variation 계산 + 전달 | 1 | 25분 |
| 4 | Editor Test 5+ NUnit + ProjectValidator | 1 | 30분 |
| 5 | docs | 1 | 15분 |
| **총** | | **5** | **약 2시간** |

## 2. Sprint 명세

### Sprint 1 — EnemyHueVariation 정적 헬퍼

**파일**: `Scripts/Gameplay/Enemies/EnemyHueVariation.cs`

**클래스 명세**:
```csharp
public static class EnemyHueVariation {
    public static readonly float[] OFFSETS = { -0.20f, -0.10f, 0.0f, +0.10f, +0.20f };
    public static Color HueShift(Color rgb, float hueOffset);
    public static int VariationIndex(int waveNumber, int spawnOrder);
}
```

**예상 라인**: ~70L (§15.6 통과)

**검증 기준**: 컴파일 errors=0 / `HueShift(Color.red, 0.5f)` 회전 검증

#### Sprint 1 commit
```
5012 Sprint 1 / T01 — EnemyHueVariation 정적 헬퍼 (1/5)

[변경]
- Scripts/Gameplay/Enemies/EnemyHueVariation.cs 신규 (~70L)
- HueShift(Color rgb, float hueOffset) HSV 변환
- 5 단계 OFFSETS const + VariationIndex(wave, order)

[검증]
- 컴파일 errors=0
- §15.6 100L 통과

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 2 — EnemyController.Initialize hook

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` 수정

**변경**:
- `Initialize(EnemyData data, int variationIndex = 0)` 시그니처 변경
- BossTier != None 시 variationIndex = 0 (원색 유지)
- `_spriteRenderer.color = EnemyHueVariation.HueShift(data.tintColor, OFFSETS[variationIndex])`

**검증 기준**: 컴파일 + 1 Boss + 1 일반 적 색상 차이 확인

#### Sprint 2 commit
```
5012 Sprint 2 / T02 — EnemyController.Initialize variationIndex hook (2/5)

[변경]
- EnemyController.Initialize 시그니처 +variationIndex
- BossTier != None 예외 처리
- _spriteRenderer.color = HueShift(data.tintColor, OFFSETS[variationIndex])

[검증]
- 컴파일 errors=0
- Boss 원색 / 일반 hue shift 적용 확인

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 3 — WaveManager / EnemySpawner variation 계산

**파일**: `Scripts/Gameplay/Waves/WaveManager.cs` 또는 `EnemySpawner.cs` 수정

**변경**:
- spawn 시 `variationIndex = EnemyHueVariation.VariationIndex(currentWave, spawnOrder)` 계산
- Initialize 호출 시 전달

**검증 기준**: Play Mode wave 1~5 진입 시 sprite 색상 5단계 분포 확인

#### Sprint 3 commit
```
5012 Sprint 3 / T03 — WaveManager variation 계산 + 전달 (3/5)

[변경]
- WaveManager / EnemySpawner — spawn 시 variationIndex 전달
- wave % 5 + spawn order 기반

[검증]
- wave 1~5 sprite 색상 5단계 분포

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 4 — Editor Test 5+ NUnit

**파일**: `Tests/Editor/EnemyHueVariationTests.cs` 신규

**Test 메서드**:
- `HueShift_Red_Plus50Percent_ReturnsBlue` — 정확한 hue rotation
- `HueShift_White_NoChange` — 색상 변동 X (white)
- `HueShift_ZeroOffset_Identity`
- `VariationIndex_Wave1_Spawn0_Returns0`
- `VariationIndex_Wave6_Returns0` (5 cycle)
- `OFFSETS_HasFive_NoDuplicate`

**ProjectValidator 룰**: `Boss_Variation_Index_Zero` — BossTier != None EnemyData 의 variation 처리 검증.

#### Sprint 4 commit
```
5012 Sprint 4 / T04 — EnemyHueVariationTests 6 NUnit + Validator (4/5)

[변경]
- Tests/Editor/EnemyHueVariationTests.cs 신규 (6 NUnit)
- ProjectValidator Boss_Variation_Index_Zero 룰

[검증]
- 6/6 PASS, 누적 NUnit 81→87

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 5 — docs

**작업**:
- changelog_v1.md 5012 항목
- CLAUDE.md §11/§12/§14 갱신

#### Sprint 5 commit
```
5012 Sprint 5 / T05 — docs (5/5)

[변경]
- changelog_v1.md + CLAUDE.md §11/§12/§14
- 후속 anchor 5013~5015

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
T01 → T02 → T03 → T04 → T05
```

순차 (T02 가 T01 의존, T03 이 T02 의존).

## 4. 검증 시나리오

### S1 — Play Mode wave 1~5 sprite 분포
1. CH 1 진입 → wave 1~5 진행
2. 각 wave 의 일반 적 sprite 색상 확인
- **검증**: 5 wave 의 sprite 색상이 5단계 hue 분포

### S2 — Boss 예외
1. CH 1 wave 5 (MiniBoss) 출현
- **검증**: Boss sprite 원색 (variationIndex=0)

### S3 — Editor Test
1. Test Runner Edit Mode > Run All
- **검증**: EnemyHueVariationTests 6/6 PASS

## 5. Definition of Done

- [ ] 5 commit + 컴파일 errors=0
- [ ] NUnit 6/6 PASS (누적 87)
- [ ] Play wave 1~5 색상 변화 시각 확인
- [ ] Boss 원색 유지

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| HSV 변환 GC | Color struct (no boxing), Mathf 정적 사용 |
| White sprite 무변화 | 디자인 의도 (saturation 영역만 영향) |
| 5단계 변형 부족 | 5012.1 후속 — 7단계 확장 검토 |

## 7. 변경 이력

- 2026-05-04: v1 작성.
