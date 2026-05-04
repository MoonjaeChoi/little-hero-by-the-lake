# T02: EnemyController hook + \_baseColor 갱신 (D1+D3)

> **레이어**: Logic
> **의존성**: T01
> **예상 시간**: 25분 / 1 commit
> **PRD 출처**: 5012_mpb_hue_shift_v1_v2.md §2 Sprint 2

## 1. 작업 항목

### 1.1 EnemyController.cs 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Enemies/EnemyController.cs`

#### 변경 1: Initialize 시그니처

기존 (line 51):
```csharp
public void Initialize(EnemyData data, float statMultiplier = 1f)
```

신규:
```csharp
public void Initialize(EnemyData data, float statMultiplier = 1f, int variationIndex = 0)
```

(default 0 — 기존 호출처 영향 X)

#### 변경 2: ApplyVisual hue 적용 (line 196 부근, D1=B)

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

(BossTier != None 시 hue 적용 X — 보스 시각 일관성)

#### 변경 3: \_baseColor 갱신 (D3=B)

ApplyVisual 메서드 끝부분에 추가 (또는 sr.color 갱신 직후):
```csharp
_baseColor = sr.color;  // hue 결과로 갱신 — flash 후 hue 색상 유지
```

> **주의**: 현 EnemyController.cs:68 `_baseColor = _sr != null ? _sr.color : Color.white;` 는 Initialize 시작 시 캐시. 이후 ApplyVisual 에서 hue 적용 → 같은 \_sr 인스턴스 의 color 갱신 → \_baseColor 재갱신 필수.

### 1.2 ApplyVisual 메서드 — variationIndex 인자 전달

`Initialize` → `ApplyVisual(data, variationIndex)` 시그니처 갱신 (현 코드 흐름 검증 필수 — `static void ApplyVisual` 면 signature 추가):

```csharp
private static void ApplyVisual(GameObject go, EnemyData data, int variationIndex)
{
    SpriteRenderer sr = go.GetComponentInChildren<SpriteRenderer>();
    if (sr == null) return;
    sr.color = data.icon != null ? Color.white : data.tintColor;
    if (data.icon != null) sr.sprite = data.icon;
    if (variationIndex > 0 && data.bossTier == BossTier.None)
    {
        sr.color = EnemyHueVariation.HueShift(sr.color, EnemyHueVariation.OFFSETS[variationIndex]);
    }
}
```

> 현 ApplyVisual 시그니처는 코드 read 시 확인 필요. (line 190 부근)

### 1.3 Initialize 의 \_baseColor 갱신 시점

```csharp
public void Initialize(EnemyData data, float statMultiplier = 1f, int variationIndex = 0)
{
    ...
    _sr = GetComponentInChildren<SpriteRenderer>();
    ApplyVisual(gameObject, data, variationIndex);
    _baseColor = _sr != null ? _sr.color : Color.white;  // hue 적용 후 캐시
    ...
}
```

(line 68 의 `_baseColor = ...` 위치를 ApplyVisual 후로 이동하거나, ApplyVisual 끝에서 EnemyController 인스턴스에 전달)

## 2. 검증 기준

### 2.1 컴파일
- refresh + 5초 대기 + errors=0

### 2.2 회귀
- 기존 Initialize 호출처 (default 0 호환) 동작 변경 X
- BossEncounterRunner / WaveManager.SpawnEnemy 회귀 X

### 2.3 동작 (T04 NUnit + Play 검증)
- BossTier=None + variationIndex>0 → hue 적용
- BossTier!=None → 원색 유지
- flash 후 \_baseColor 복귀 시 hue 색상 유지

## 3. commit 메시지

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
- BossTier 예외 처리 (variationIndex 강제 무시)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- ApplyVisual 시그니처 변경 시 호출처 다수 — grep 으로 확인 필수
- \_baseColor 갱신 시점 누락 시 flash 후 hue 풀림 (D3 회귀)
