# T03: EnemySpawner + WaveManager variation 전달

> **레이어**: Logic
> **의존성**: T02
> **예상 시간**: 30분 / 1 commit
> **PRD 출처**: 5012_mpb_hue_shift_v1_v2.md §2 Sprint 3

## 1. 작업 항목

### 1.1 EnemySpawner.Spawn 시그니처

**경로**: `Scripts/Gameplay/Enemies/EnemySpawner.cs`

기존 (line 43):
```csharp
public EnemyController Spawn(EnemyData data)
{
    ...
    enemy.Initialize(data, multi);
}
```

신규:
```csharp
public EnemyController Spawn(EnemyData data, int variationIndex = 0)
{
    ...
    enemy.Initialize(data, multi, variationIndex);
}
```

(default 0 — 기존 호출처 호환)

### 1.2 WaveManager variation 계산

**경로**: `Scripts/Gameplay/Waves/WaveManager.cs`

#### 변경 1: spawn order 카운터

```csharp
private int _spawnOrder = 0;
```

(필드 추가, 클래스 상단)

#### 변경 2: SpawnEnemy 진입점 (line 118)

기존:
```csharp
private void SpawnEnemy(EnemyData data)
{
    EnemyController enemy = _spawner.Spawn(data);
    ...
}
```

신규:
```csharp
private void SpawnEnemy(EnemyData data)
{
    int variationIndex = EnemyHueVariation.VariationIndex(_currentWave, _spawnOrder++);
    EnemyController enemy = _spawner.Spawn(data, variationIndex);
    ...
}
```

(`_currentWave` 필드 보유 가정 — 코드 read 후 확인 필수, 없으면 wave 시작 시 캡처)

#### 변경 3: \_spawnOrder 리셋 (선택)

각 wave 시작 시 `_spawnOrder = 0` (또는 누적 유지). 누적 유지 시 wave 간 hue 분포 더 다양화.

권장: 누적 유지 (단순 + 분포 ↑).

## 2. 검증 기준

### 2.1 컴파일
- refresh + 5초 대기 + errors=0

### 2.2 회귀
- 기존 Spawn 호출처 (BossEncounterRunner.cs / InfiniteTowerRunner.cs 등) default 0 호환

### 2.3 동작 (Play 검증)
- wave 1~5 sprite 색상 5단계 분포 확인 (S1 시나리오)

## 3. commit 메시지

```
5012 Sprint 3 / T03 — EnemySpawner + WaveManager variation 계산 (3/5)

[변경]
- EnemySpawner.Spawn(data, variationIndex=0) 시그니처
- WaveManager.SpawnEnemy — variationIndex 계산 + 전달
- _currentWave + _spawnOrder 누적 — 7 prime spread 활용

[검증]
- 컴파일 errors=0
- 기존 호출처 default 0 호환

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 알려진 위험
- WaveManager._currentWave 필드 부재 시 — wave 시작 콜백에서 캡처 또는 GameEvents 구독 (코드 read 후 결정)
- \_spawnOrder 누적 vs 리셋 — 누적 권장 (수정 비용 없음)
