# T07: ChapterRunner — wave==totalWaves Launcher 전환 + BackgroundApplier 통합

> **레이어**: Logic (Waves)
> **의존성**: T02 (BossEncounterLauncher), T04 (BackgroundApplier)
> **예상 시간**: 30분 / 1 commit (5002 Sprint 5, 7/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 5

## 1. 작업 항목

### 1.1 기존 파일 수정 — ChapterRunner.cs

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Waves/ChapterRunner.cs`

**변경 1**: `StartNextWave()` 에 wave==totalWaves 분기 추가
```csharp
private void StartNextWave()
{
    int wave = GameManager.Instance.CurrentWave;

    // 5002 — 챕터 보스 wave 진입 시 BossEncounter 씬으로 분리 (옵션 C)
    if (wave >= _chapterData.totalWaves && _chapterData.bossEnemy != null)
    {
        SaveSession();   // 진행 저장
        BossEncounterLauncher.Launch(new BossEncounterRequest
        {
            mode = BossEncounterContext.Mode.ChapterBoss,
            boss = _chapterData.bossEnemy,
            chapter = _chapterData,
            returnSceneName = "Attic",
        });
        return;
    }

    // 기존 일반/미니보스 흐름 그대로
    WaveConfig config = BuildWaveConfig(wave);
    if (config.boss != null) AudioManager.Instance?.PlayBgm("boss_chapter", crossFadeSec: 1.5f);
    else if (config.miniBoss != null) AudioManager.Instance?.PlayBgm("miniboss_chase", crossFadeSec: 1.0f);
    _waveManager.StartWave(config, OnWaveComplete);
}
```

**변경 2**: `BuildWaveConfig` 의 `bossEnemy` 분기 제거
```csharp
private WaveConfig BuildWaveConfig(int waveNumber)
{
    BalanceConfig balance = GameManager.Instance.Balance;
    bool isMiniB = waveNumber % balance.miniBossWaveInterval == 0
                   && waveNumber < _chapterData.totalWaves;
    int spawnCount = Mathf.RoundToInt(10f * (1f + balance.waveScalingFactor * waveNumber));
    EnemyData mb = isMiniB && _chapterData.miniBossPool?.Length > 0
                   ? _chapterData.miniBossPool[Random.Range(0, _chapterData.miniBossPool.Length)] : null;
    return new WaveConfig {
        enemyPool = _chapterData.enemyPool,
        spawnCount = spawnCount,
        duration = _chapterData.waveDuration,
        miniBoss = mb,
        boss = null,   // ★ 5002 — 챕터 보스는 BossEncounter 씬에서 처리
    };
}
```

**변경 3**: `ApplyChapterBackground` → `BackgroundApplier.Apply` 통합 (D1 Rebuttal)
```csharp
private void ApplyChapterBackground()
{
    if (_backgroundRenderer == null)
    {
        var bgGO = GameObject.Find("Background");
        if (bgGO != null) _backgroundRenderer = bgGO.GetComponent<SpriteRenderer>();
    }
    BackgroundApplier.Apply(_backgroundRenderer, _chapterData);  // D1: 정적 헬퍼 위임
}
```

기존 19줄 → 6줄 축소.

라인 수 변동: 189L → ~175L (D1 통합으로 -14L) → +10L (분기 추가) = ~185L

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` + 10초 대기 + errors=0

### 2.2 Editor Test
- T09 에서 ChapterRunner 의 `BuildWaveConfig.boss == null` reflection 검증 (선택)
- Play 검증 시나리오 S1 (PRD)

### 2.3 ProjectValidator
- ChapterRunner 라인 ~185L — 200L 경계 내 ✅

### 2.4 회귀 (Play 검증 — 시나리오 S1)
- 챕터 1 wave 1~19 정상 (미니보스 5/10/15 OK)
- wave 20 진입 직전 BossEncounter 씬 전환
- BossEncounter 씬에서 챕터 1 BG (red_riding_hood) 표시
- 보스 처치 → Attic 복귀 + ChapterCleared toast (D2) + StarFragments/Starlight 증가

## 3. commit 메시지

```
5002 Sprint 5 — ChapterRunner wave==totalWaves Launcher 전환 + BackgroundApplier 통합 (7/10)

[변경]
- Scripts/Gameplay/Waves/ChapterRunner.cs
  - StartNextWave 에 wave==totalWaves 분기 → BossEncounterLauncher.Launch
  - BuildWaveConfig.boss=null (챕터 보스는 별 씬에서 처리)
  - ApplyChapterBackground → BackgroundApplier.Apply 통합 (D1 Rebuttal DRY)

[검증]
- 컴파일 OK
- 챕터 1 wave 1~19 회귀 X (미니보스 5/10/15 OK)
- wave 20 → BossEncounter 씬 전환 (T05 wiring 활성)
- 보스 처치 → Attic + ChapterCleared toast (D2)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 5, §5.2 (D1 통합)
- 디자인 §3.4 ChapterRunner 변경 흐름

## 5. 알려진 위험

- 챕터 클리어 흐름 변경 — `OnWaveComplete` 의 `RaiseChapterCleared` 호출 경로 확인. T06 의 `CompleteChapterBoss` 가 이미 raise 함 → ChapterRunner 의 raise 가 중복되지 않도록 검증
- `_chapterData.bossEnemy == null` 인 챕터 (예외 케이스) — 분기 진입 X, BuildWaveConfig.boss=null 로 정상 종료. `RaiseChapterCleared` 가 어느 경로에서도 호출되지 않을 위험 — 검증 필수
- 일시정지/재개 흐름 (`activeChapterId` / `activeChapterWave`) — wave 20 진입 시 `SaveSession` 호출, BossEncounter 씬에서 사망 시 `activeChapterId` 잔존 가능 → Attic 복귀 후 처리 검토
