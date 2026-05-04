# 5012 — MaterialPropertyBlock Hue Shift v2 (최종, Mediator 확정)

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5012_mpb_hue_shift_v1_prd.md` · Rebuttal: `5012_mpb_hue_shift_v1_rebuttal.md` · Decisions: `5012_mpb_hue_shift_v1_decisions.md`
> 결정 3건 반영 (D1=B / D2=A / D3=B). 다음 단계: /prd-breakdown

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | EnemyHueVariation 정적 헬퍼 (~70L) | 1 | 25분 |
| 2 | EnemyController.Initialize variationIndex hook + \_baseColor 갱신 (D1+D3) | 1 | 25분 |
| 3 | EnemySpawner.Spawn 시그니처 + WaveManager variation 계산 | 1 | 30분 |
| 4 | Editor Test 6 NUnit + ProjectValidator | 1 | 30분 |
| 5 | docs | 1 | 15분 |
| **총** | | **5** | **약 2시간 5분** |

## 2. Sprint 명세

### Sprint 1 — EnemyHueVariation 정적 헬퍼

**파일**: `Scripts/Gameplay/Enemies/EnemyHueVariation.cs`

```csharp
namespace LittleHero
{
    public static class EnemyHueVariation
    {
        public static readonly float[] OFFSETS = { -0.20f, -0.10f, 0.0f, +0.10f, +0.20f };
        public const int VARIATION_COUNT = 5;

        public static Color HueShift(Color rgb, float hueOffset);
        public static int VariationIndex(int waveNumber, int spawnOrder);
    }
}
```

**예상 라인**: ~70L (§15.6 통과)

**구현 핵심** (Color.RGBToHSV / HSVToRGB):
```csharp
public static Color HueShift(Color rgb, float hueOffset)
{
    Color.RGBToHSV(rgb, out float h, out float s, out float v);
    h = (h + hueOffset + 1f) % 1f;  // wrap [0,1)
    var shifted = Color.HSVToRGB(h, s, v);
    shifted.a = rgb.a;
    return shifted;
}

public static int VariationIndex(int waveNumber, int spawnOrder)
{
    return ((waveNumber - 1) * 7 + spawnOrder) % VARIATION_COUNT;  // 7 prime → spread
}
```

**검증 기준**: 컴파일 errors=0 / `HueShift(Color.red, 0.5f).b > 0.5f` 회전 검증

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

### Sprint 2 — EnemyController.Initialize hook + \_baseColor 갱신 (D1+D3)

**파일**: `Scripts/Gameplay/Enemies/EnemyController.cs` 수정

**변경**:

#### 2.1 Initialize 시그니처 갱신
```csharp
public void Initialize(EnemyData data, float statMultiplier = 1f, int variationIndex = 0)
```
default 0 으로 호환성 유지 (기존 호출처 영향 X).

#### 2.2 D1 — Hue shift 적용 위치 (line 196 부근)

기존:
```csharp
sr.color = data.icon != null ? Color.white : data.tintColor;
```

신규:
```csharp
sr.color = data.icon != null ? Color.white : data.tintColor;
if (variationIndex > 0 && data.bossTier == BossTier.None)
{
    sr.color = EnemyHueVariation.HueShift(sr.color, EnemyHueVariation.OFFSETS[variationIndex]);
}
```

(BossTier != None 시 variationIndex 강제 0 — 보스 시각 일관성)

#### 2.3 D3 — \_baseColor 갱신

기존 (line 68):
```csharp
_baseColor = _sr != null ? _sr.color : Color.white;
```

→ Initialize 의 hue 적용 **후** \_baseColor 가 hue 색상으로 캐시되도록 순서 보장. 현 코드는 Initialize 시작 시 \_sr 캐시하고 \_baseColor 캐시 → ApplyVisual (line 196) 에서 hue 적용 → 그 시점에 \_baseColor 재갱신:

```csharp
// ApplyVisual 끝부분 (sr.color = ... HueShift 후)
_baseColor = sr.color;  // hue 결과로 갱신 — flash 후 hue 색상 유지
```

**검증 기준**: 컴파일 + Boss 원색 / 일반 적 hue 적용 / flash 후 hue 색상 복귀

#### Sprint 2 commit
```
5012 Sprint 2 / T02 — EnemyController hue hook + _baseColor 갱신 (2/5)

[변경]
- EnemyController.Initialize 시그니처 +variationIndex (default 0)
- ApplyVisual: data.bossTier=None + variationIndex>0 시 HueShift 적용
- _baseColor 갱신 — hue 결과로 캐시 (flash 후 hue 색상 유지)

[검증]
- 컴파일 errors=0
- D1=B (SpriteRenderer.color multiplicative tint)
- D3=B (_baseColor hue 결과 캐시)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 3 — EnemySpawner + WaveManager variation 계산

**파일**: `EnemySpawner.cs` + `WaveManager.cs` 수정

#### 3.1 EnemySpawner.Spawn 시그니처 갱신

```csharp
public EnemyController Spawn(EnemyData data, int variationIndex = 0)
{
    ...
    enemy.Initialize(data, multi, variationIndex);
    ...
}
```

#### 3.2 WaveManager variation 계산

`SpawnEnemy(EnemyData data)` 진입점 (line 118) 갱신:
- `_currentWave` 보유 + spawn 호출 카운터 추가
- `int variationIndex = EnemyHueVariation.VariationIndex(_currentWave, _spawnOrder++);`
- `EnemySpawner.Spawn(data, variationIndex)` 전달

**검증 기준**: Play Mode wave 1~5 진입 시 sprite 색상 5 hue 분포 확인

#### Sprint 3 commit
```
5012 Sprint 3 / T03 — EnemySpawner + WaveManager variation 계산 (3/5)

[변경]
- EnemySpawner.Spawn(data, variationIndex=0) 시그니처
- WaveManager.SpawnEnemy — variationIndex 계산 + 전달
- _currentWave + spawn order 기반 5 cycle

[검증]
- wave 1~5 sprite 색상 5단계 분포

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 4 — Editor Test 6 NUnit + Validator

**파일**: `Tests/Editor/EnemyHueVariationTests.cs` 신규

**Test 메서드**:
- `HueShift_Red_Plus50Percent_ReturnsCyan` — 정확한 hue rotation
- `HueShift_White_NoChange` — saturation=0 → hue 무관 (white 무변화)
- `HueShift_ZeroOffset_Identity`
- `VariationIndex_Wave1_Spawn0_Returns0`
- `VariationIndex_Wave1_Spawn7_Returns2` (7 prime spread)
- `OFFSETS_HasFive_NoDuplicate`

**ProjectValidator 룰**: `EnemyController_HueVariation_Hook` — EnemyController.cs 에 `EnemyHueVariation.HueShift` 호출 라인 존재 정적 검사.

#### Sprint 4 commit
```
5012 Sprint 4 / T04 — EnemyHueVariationTests 6 NUnit + Validator (4/5)

[변경]
- Tests/Editor/EnemyHueVariationTests.cs 신규 (6 NUnit)
- ProjectValidator EnemyController_HueVariation_Hook 룰

[검증]
- 6/6 PASS, 누적 NUnit 87→93

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

### Sprint 5 — docs

**작업**:
- changelog_v1.md 5012 항목
- CLAUDE.md §11 마일스톤 + §12 시스템 현황 + §14 직전 5건 회전

#### Sprint 5 commit
```
5012 Sprint 5 / T05 — docs (5/5) — 5012 종결

[변경]
- changelog_v1.md + CLAUDE.md §11/§12/§14
- D1=B / D2=A / D3=B 결정 반영

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

### S3 — flash 후 hue 색상 유지 (D3)
1. 일반 적 (variationIndex>0) hit
2. flash → \_baseColor 복귀
- **검증**: hue 색상 유지 (white 로 복귀 X)

### S4 — Editor Test
1. Test Runner Edit Mode > Run All
- **검증**: EnemyHueVariationTests 6/6 PASS

## 5. Definition of Done

- [ ] 5 commit + 컴파일 errors=0
- [ ] NUnit 6/6 PASS (누적 87→93)
- [ ] Play wave 1~5 색상 변화 시각 확인
- [ ] Boss 원색 유지
- [ ] flash 후 hue 색상 유지

## 6. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| HSV 변환 GC | Color struct (no boxing), Mathf 정적 사용 |
| White sprite 무변화 | D1=B 정합 — sprite 자체 색상 강하면 변형 적음 OK |
| variationIndex 호환성 | default 0 (기존 호출처 영향 X) |
| flash 후 hue 풀림 | D3=B \_baseColor 갱신 — flash 후 hue 색상 유지 |
| 5단계 변형 부족 | 5012.1 후속 — 7단계 확장 검토 |

## 7. 변경 이력

- 2026-05-04: v1 작성 (Phase 1)
- 2026-05-04: v2 초안 (Phase 2.2 Rebuttal)
- 2026-05-04: v2 최종 (Phase 2.3 Mediator, D1=B / D2=A / D3=B)
