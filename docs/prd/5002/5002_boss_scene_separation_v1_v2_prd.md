# 5002 — Boss Scene Separation v2 PRD (Mediator 확정)

> **PRD v2 (Mediator 단계 최종)**
> 원본: `5002_boss_scene_separation_v1_prd.md` · Rebuttal: `5002_*_rebuttal.md` · Decisions: `5002_*_decisions.md`
> Phase 2.3 사용자 결정: **D2=A, D3=A, D4=B** (권장안 일괄 채택, 2026-05-04)
>
> **번호**: 5002 / **출처**: `5002_boss_scene_separation_v1.md` (옵션 C 채택)
> **목표**: 8 Sprint, 10 commit, **3시간 35분** (D1 +5분 + D2 +5분)
> **선행**: 5001 boss_system_v2 완료
>
> ### Rebuttal 자동 수용 (2건)
> - **D1**: `BackgroundApplier` 정적 헬퍼 추출 (§15.1 SRP + DRY) — Sprint 3.1 작업량 +5분, BossEncounterRunner ~170L → ~145L 감소
> - **D5**: §S1 검증 시나리오에 Phase 2 텔레그라프·Premium 피격·DontDestroyOnLoad 4줄 추가
>
> ### Mediator 결정 채택 (3건)
> - **D2 = A**: SaveData `pendingChapterClearedId` 신규 필드 + AtticController 1회 toast (Sprint 4 작업량 +5분)
> - **D3 = A**: 미니보스 씬 분리는 본 5002 스코프 외 (디자인 §6 1줄 후속 명시 — 별 commit 불필요)
> - **D4 = B**: LoadSceneAsync 는 5004 별 최적화 sprint (PRD §6.1 위험 표 1줄 갱신만)

## 0. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | BossEncounterRequest DTO + IBossEncounterLauncher + 정적 BossEncounterLauncher | 2 | 30분 |
| 2 | BossEncounterContext.Mode.ChapterBoss + PendingChapter 필드 | 1 | 15분 |
| 3 | BossEncounterRunner — Mode.ChapterBoss 분기 + BackgroundApplier 정적 헬퍼 (D1) | 2 | 45분 |
| 4 | BossEncounterService.CompleteChapterBoss + BossLootGrantService hook + SaveData pendingChapterClearedId (D2) + AtticController toast | 1 | 30분 |
| 5 | ChapterRunner — wave==totalWaves 진입 시 Launcher.Launch 전환 | 1 | 30분 |
| 6 | EndgameMenuPanel — Launcher 위임 (정적 Context 직접 R/W 제거) | 1 | 20분 |
| 7 | NUnit Test 5+ + ProjectValidator 룰 (정적 Context 외부 직접 참조 검사) | 1 | 30분 |
| 8 | docs + changelog_v1.md 등재 + CLAUDE.md §12/§13 갱신 | 1 | 15분 |
| **총** | | **10** | **3시간 35분** |

## 의존성 그래프

```
Sprint 1 (DTO+Launcher) ──┐
                          ├─→ Sprint 3 (Runner 분기) ──┐
Sprint 2 (Context.Mode) ──┘                            ├─→ Sprint 5 (ChapterRunner 전환) ──┐
                                                       │                                    │
                          Sprint 4 (Service+Loot) ─────┤                                    ├─→ Sprint 7 (Test) → Sprint 8 (docs)
                                                       │                                    │
                          Sprint 6 (EndgameMenuPanel) ─┘                                    │
                                                                                            │
```

병렬 가능: Sprint 4 ↔ Sprint 6 (서로 독립). 단일 작업자라면 순차로 진행 권장.

---

## Sprint 1 — Request DTO + Launcher 인프라 (2 commit)

### 1.1 BossEncounterRequest DTO

**파일**: `Scripts/Core/BossEncounterRequest.cs` (신규)

**산출물**:
```csharp
public struct BossEncounterRequest
{
    public BossEncounterContext.Mode mode;
    public EnemyData boss;
    public EndgameModeData endgameMode;       // Daily/Weekly 만 (null 가능)
    public ChapterData chapter;                // ChapterBoss 만 (null 가능)
    public MythicDefinition[] mythicPool;      // Weekly 만 (null 가능)
    public string returnSceneName;             // 기본 "Attic"
}
```

**검증**: 컴파일 OK. `BossEncounterContext.Mode` 에 `ChapterBoss` 미추가 상태에서는 enum 미존재 컴파일 에러 → Sprint 2 가 해소.

**완료 조건**: 파일 신규. struct 6 필드. namespace `LittleHero`.

### 1.2 IBossEncounterLauncher + BossEncounterLauncher

**파일**: `Scripts/Core/IBossEncounterLauncher.cs` (신규) / `Scripts/Core/BossEncounterLauncher.cs` (신규)

**산출물**:
```csharp
public interface IBossEncounterLauncher
{
    void Launch(BossEncounterRequest request);
}

public static class BossEncounterLauncher
{
    public static IBossEncounterLauncher Default { get; set; } = new DefaultLauncher();

    public static void Launch(BossEncounterRequest req) => Default.Launch(req);

    private class DefaultLauncher : IBossEncounterLauncher
    {
        public void Launch(BossEncounterRequest req)
        {
            BossEncounterContext.Set(req.mode, req.boss, req.endgameMode, req.mythicPool);
            BossEncounterContext.PendingChapter = req.chapter;       // Sprint 2 추가
            BossEncounterContext.PendingReturnScene = req.returnSceneName;  // Sprint 2 추가
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene("BossEncounter");
        }
    }
}
```

**검증**: 컴파일 OK. `Default` setter 노출 (테스트 mock 주입). Sprint 2 필드 미추가 상태에서는 임시로 컴파일 통과 위해 본 commit 에서는 `PendingChapter`/`PendingReturnScene` 라인 주석 처리, Sprint 2 commit 에서 활성화.

**완료 조건**: interface 1 + class 1 신규. `BossEncounterLauncher.Launch(req)` 정적 호출 가능.

#### Sprint 1 commit (2 commit)

```
5002 Sprint 1.1 — BossEncounterRequest DTO 신규 (1/10)

[변경]
- Scripts/Core/BossEncounterRequest.cs 신규 (struct 6 필드)

[검증]
- 컴파일 OK
- 기존 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

```
5002 Sprint 1.2 — IBossEncounterLauncher + 정적 Default Launcher (2/10)

[변경]
- Scripts/Core/IBossEncounterLauncher.cs 신규
- Scripts/Core/BossEncounterLauncher.cs 신규 (Default property + DefaultLauncher 내부 class)

[검증]
- 컴파일 OK
- BossEncounterLauncher.Launch(req) 정적 호출 가능
- Sprint 2 PendingChapter/PendingReturnScene 라인 주석 (Sprint 2 활성화)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 2 — Context.Mode.ChapterBoss + PendingChapter (1 commit)

### 2.1 Mode enum + 신규 필드

**파일**: `Scripts/Core/BossEncounterContext.cs` (변경)

**산출물 (변경부)**:
```csharp
public enum Mode { Daily, Weekly, ChapterBoss }

public static ChapterData PendingChapter;       // 신규 (ChapterBoss 한정)
public static string PendingReturnScene;         // 신규 (기본 "Attic")

public static void Clear()
{
    PendingBoss = null;
    PendingMode = null;
    PendingMythicPool = null;
    PendingChapter = null;
    PendingReturnScene = null;
}
```

**검증**:
- 컴파일 OK
- `BossEncounterLauncher.DefaultLauncher.Launch` 의 `PendingChapter`/`PendingReturnScene` 주석 해제
- 기존 Daily/Weekly 흐름 회귀 X

**완료 조건**: enum 3 항목 / 신규 필드 2 / Clear 갱신.

#### Sprint 2 commit

```
5002 Sprint 2 — Mode.ChapterBoss + PendingChapter/ReturnScene (3/10)

[변경]
- Scripts/Core/BossEncounterContext.cs — Mode enum 에 ChapterBoss / 신규 정적 필드 2개
- Scripts/Core/BossEncounterLauncher.cs — Sprint 1 주석 해제

[검증]
- 컴파일 OK
- Daily/Weekly 흐름 회귀 X (EndgameMenuPanel 직접 Set 호출 그대로)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 3 — BossEncounterRunner Mode.ChapterBoss 분기 + BackgroundApplier (2 commit, 45분)

### 3.0 BackgroundApplier 정적 헬퍼 (D1 Rebuttal 자동 수용)

**파일**: `Scripts/Core/BackgroundApplier.cs` (신규 ~25L)

**산출물**:
```csharp
namespace LittleHero
{
    /// <summary>
    /// 챕터 BG 적용 정적 헬퍼 — ChapterRunner / BossEncounterRunner 공유 (5002 Rebuttal D1).
    /// §15.1 SRP + DRY — Tiled BG 패턴 단일 진실원.
    /// </summary>
    public static class BackgroundApplier
    {
        public static void Apply(SpriteRenderer renderer, ChapterData chapter)
        {
            if (renderer == null || chapter?.coverArt == null) return;
            renderer.sprite = chapter.coverArt;
            renderer.sortingOrder = -100;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.tileMode = SpriteTileMode.Continuous;
            renderer.size = new Vector2(300f, 300f);
            renderer.transform.position = Vector3.zero;
            renderer.color = Color.white;
        }
    }
}
```

**완료 조건**: 컴파일 OK. 정적 메서드 1.

### 3.1 BossEncounterRunner ChapterBoss 분기 + BackgroundApplier 위임

**파일**: `Scripts/Gameplay/Waves/BossEncounterRunner.cs` (변경 — 118L → ~145L, D1 으로 -25L)

**산출물 (변경부)**:
```csharp
[SerializeField] private SpriteRenderer _backgroundRenderer;  // ChapterBoss 모드 BG

private void Start()
{
    if (!BossEncounterContext.HasPending) { /* 기존 */ return; }

    _wave.Initialize(_spawner);
    GameManager.Instance?.ResetRun();

    var boss = BossEncounterContext.PendingBoss;
    _spawner.RegisterEnemy(boss);

    if (BossEncounterContext.PendingType == BossEncounterContext.Mode.ChapterBoss)
        BackgroundApplier.Apply(_backgroundRenderer, BossEncounterContext.PendingChapter);  // D1: 정적 헬퍼 위임

    // 기존 WaveConfig 시작
    var config = new WaveConfig { ... boss = boss };
    _wave.StartWave(config, OnBossDefeated);
    Debug.Log($"[BossEncounter] 시작 — type={BossEncounterContext.PendingType} ...");
}

private void OnBossDefeated()
{
    if (_resolved) return;
    _resolved = true;

    switch (BossEncounterContext.PendingType)
    {
        case BossEncounterContext.Mode.Daily:    /* 기존 */ break;
        case BossEncounterContext.Mode.Weekly:   /* 기존 */ break;
        case BossEncounterContext.Mode.ChapterBoss:
            BossEncounterService.CompleteChapterBoss(
                BossEncounterContext.PendingBoss,
                BossEncounterContext.PendingChapter);
            break;
    }
    string returnScene = BossEncounterContext.PendingReturnScene ?? _atticSceneName;
    BossEncounterContext.Clear();
    StartCoroutine(LoadSceneAfterDelay(returnScene));
}

// D1 Rebuttal: ApplyChapterBackground 메서드 제거 → BackgroundApplier 정적 헬퍼로 대체

// LoadAtticAfterDelay → LoadSceneAfterDelay(returnScene) 로 변경
private IEnumerator LoadSceneAfterDelay(string scene)
{
    yield return new WaitForSecondsRealtime(_returnDelay);
    Time.timeScale = 1f;
    SceneManager.LoadScene(scene);
}
```

**검증**:
- 컴파일 OK (`CompleteChapterBoss` Sprint 4 추가 — 본 Sprint 에서 메서드 stub 만 BossEncounterService 에 미리 추가 OR Sprint 순서 조정)
- Sprint 4 까지 완료 후 ChapterBoss 분기 정상 동작 검증 (Mode 만 Daily/Weekly 라면 회귀 X)

**완료 조건**: BossEncounterRunner 3 변경 (Start/OnBossDefeated/LoadSceneAfterDelay). ApplyChapterBackground 메서드 X (D1: BackgroundApplier 정적 헬퍼 위임).

### 3.2 BossEncounter.unity 씬 wiring

**파일**: `LittleHero/Assets/_Project/Scenes/BossEncounter.unity` (Editor 작업)

**산출물**:
- Background GameObject 추가 (SpriteRenderer / sortingOrder=-100 / drawMode=Tiled / size=300×300)
- BossEncounterRunner._backgroundRenderer SerializeField 에 Background.SpriteRenderer 드래그
- BGM AudioManager wiring 확인 (DontDestroyOnLoad 검증)

**검증**:
- Play Mode 진입 → Daily 보스 wiring 회귀 없음
- BG renderer 가 ChapterBoss 모드에서만 sprite 설정됨

**완료 조건**: 씬 변경 + Inspector wiring. `_backgroundRenderer` null 일 때 BackgroundApplier.Apply no-op (안전).

#### Sprint 3 commit (2 commit)

```
5002 Sprint 3.1 — BossEncounterRunner ChapterBoss 분기 + BackgroundApplier 정적 헬퍼 (4/10)

[변경]
- Scripts/Core/BackgroundApplier.cs 신규 (D1 Rebuttal — §15.1 SRP + DRY)
- Scripts/Gameplay/Waves/BossEncounterRunner.cs — Mode.ChapterBoss 분기 / BackgroundApplier.Apply 위임 / LoadSceneAfterDelay(returnScene)
- _backgroundRenderer SerializeField 신규

[검증]
- 컴파일 OK
- Daily/Weekly 흐름 회귀 X
- ChapterBoss 분기는 Sprint 4 CompleteChapterBoss 후 활성

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

```
5002 Sprint 3.2 — BossEncounter.unity Background SpriteRenderer wiring (5/10)

[변경]
- Scenes/BossEncounter.unity — Background GameObject + SpriteRenderer
- BossEncounterRunner._backgroundRenderer Inspector 드래그

[검증]
- Play Mode — Daily 보스 BG null 안전 (no-op)
- ChapterBoss 모드는 Sprint 5 wiring 후 검증

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 4 — BossEncounterService.CompleteChapterBoss + BossLootGrantService hook 완성 (1 commit)

### 4.1 BossEncounterService.CompleteChapterBoss

**파일**: `Scripts/Core/BossEncounterService.cs` (변경 — 149L → ~180L)

**산출물 (변경부)**:
```csharp
/// <summary>챕터 보스 처치 — BossLootGrantService 자동 호출 + ChapterCleared 이벤트 + pendingChapterClearedId set (D2).</summary>
public static void CompleteChapterBoss(EnemyData boss, ChapterData chapter)
{
    if (boss == null || chapter == null)
    {
        Debug.LogWarning($"[BossEncounterService] CompleteChapterBoss: boss={boss} chapter={chapter}");
        return;
    }

    // 1) 보상 — BossLootTable 우선, fallback 으로 EnemyData.starDustDrop/xpDrop
    if (boss.lootTable != null)
        BossLootGrantService.Apply(boss.lootTable);
    else
        Debug.LogWarning($"[BossEncounterService] {boss.id}: lootTable null — fallback 보상 생략");

    // 2) 챕터 클리어 이벤트 (ChapterRunner 가 더 이상 동작 X — 직접 raise)
    GameEvents.RaiseChapterCleared(chapter);

    // 3) MetaProgression — firstClearedChapters bit set + pendingChapterClearedId set (D2 mediator)
    if (MetaProgression.Instance?.Data != null)
    {
        MetaProgression.Instance.MarkChapterFirstCleared(chapter);
        MetaProgression.Instance.Data.pendingChapterClearedId = chapter.id;  // D2: Attic 1회 toast 트리거
        SaveSystem.Save(MetaProgression.Instance.Data);
    }

    Debug.Log($"[BossEncounterService] CompleteChapterBoss: {boss.displayName} (chapter {chapter.chapterNumber})");
}
```

**SaveData 신규 필드 (D2 mediator)**:
```csharp
// MetaProgressionData (또는 SaveData) 에 추가
public string pendingChapterClearedId = "";  // 챕터 클리어 후 Attic 1회 toast 표시용 (1회 후 clear)
```
Newtonsoft 기본값 자동 → 마이그레이션 sprint 불필요.

**AtticController 변경 (D2 mediator)**:
```csharp
private void Start()
{
    /* 기존 */
    HandleReturnFromBossEncounter();
    HandlePendingChapterCleared();   // D2: 신규 1회 toast
    /* 기존 */
}

private void HandlePendingChapterCleared()
{
    var data = MetaProgression.Instance?.Data;
    if (data == null || string.IsNullOrEmpty(data.pendingChapterClearedId)) return;
    var ch = ResolveChapterById(data.pendingChapterClearedId);
    if (ch != null && _chapterClearedPanel != null)
        _chapterClearedPanel.ShowForChapter(ch);   // 또는 기존 Show 메서드
    data.pendingChapterClearedId = "";
    SaveSystem.Save(data);
}
```
ChapterClearedPanel 가 Attic 씬에 없을 경우 Sprint 4 wiring 단계에서 prefab 인스턴스 추가.

**검증**:
- 컴파일 OK (`MarkChapterFirstCleared` 메서드 부재 시 — 기존 메서드 grep 으로 호환 메서드 사용)
- BossLootTable null fallback 안전
- ChapterCleared 이벤트 1회만 raise

**완료 조건**: 메서드 1 신규 + SaveData 필드 1 + AtticController 1 메서드. BossLootGrantService.Apply 자동 호출 + Attic 1회 toast.

#### Sprint 4 commit

```
5002 Sprint 4 — CompleteChapterBoss + BossLootGrantService hook + Attic toast (D2) (6/10)

[변경]
- Scripts/Core/BossEncounterService.cs — CompleteChapterBoss(boss, chapter) 신규
- Scripts/Meta/MetaProgressionData (또는 SaveData) — pendingChapterClearedId 필드 신규
- Scripts/Hub/AtticController.cs — HandlePendingChapterCleared 1회 toast 메서드
- BossLootTable.Apply / RaiseChapterCleared / MarkChapterFirstCleared / pendingChapterClearedId set
- (필요 시) Scenes/Attic.unity — ChapterClearedPanel prefab 인스턴스 wiring

[검증]
- 컴파일 OK
- 기존 Complete×2 회귀 X
- BossLootTable null safe (fallback 로그)
- 신규 SaveData 필드 마이그레이션 자동 (빈 문자열 기본)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 5 — ChapterRunner 전환 (1 commit)

### 5.1 wave == totalWaves 진입 시 Launcher.Launch

**파일**: `Scripts/Gameplay/Waves/ChapterRunner.cs` (변경 — 189L → ~210L)

**산출물 (변경부)**:
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

// BuildWaveConfig 의 isBoss / bossEnemy 분기는 제거 (보스는 더 이상 같은 씬에서 spawn 안 함)
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

**검증**:
- Play Mode — 챕터 1 wave 1~19 까지 정상 (미니보스 5/10/15 OK), wave 20 진입 시 BossEncounter 씬 전환
- BossEncounter 씬에서 보스 처치 → Attic 복귀 + ChapterCleared 통지
- 사망 시 보상 X (기존)
- 일시정지/재개 흐름 보존

**완료 조건**: ChapterRunner 3 변경 (StartNextWave 분기 + BuildWaveConfig.boss=null + ApplyChapterBackground → BackgroundApplier.Apply 통합).

### 5.2 ChapterRunner ApplyChapterBackground → BackgroundApplier 통합 (D1 Rebuttal)

**파일**: `Scripts/Gameplay/Waves/ChapterRunner.cs` (계속)

**변경부**:
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

기존 19줄 (L130~148) → 6줄로 축소. ChapterRunner 라인 수 ~189 → ~175.

**검증**: 시각적 동등성 — 챕터 진입 시 BG sprite/sortingOrder/Tiled/size 회귀 X.

#### Sprint 5 commit

```
5002 Sprint 5 — ChapterRunner — 챕터 보스 wave BossEncounter 씬 전환 (7/10)

[변경]
- Scripts/Gameplay/Waves/ChapterRunner.cs — StartNextWave 에 wave==totalWaves 분기 / BuildWaveConfig.boss=null
- BossEncounterLauncher.Launch(ChapterBoss request) 호출

[검증]
- 챕터 1 wave 1~19 회귀 X
- wave 20 진입 → BossEncounter 씬 전환
- 보스 처치 → Attic + ChapterCleared 통지

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 6 — EndgameMenuPanel Launcher 위임 (1 commit)

### 6.1 정적 Context 직접 R/W 제거

**파일**: `Scripts/UI/EndgameMenuPanel.cs` (변경 — 174L → ~150L)

**산출물 (변경부)**:
```csharp
private void OnDailyClicked()
{
    if (!EndgameResetService.IsDailyAvailable() || _dailyBoss == null) return;
    var boss = BossEncounterService.ResolveDailyBoss(_dailyBoss);
    if (boss == null) { Debug.LogWarning("[EndgameMenu] 일일 보스 풀 비어있음"); return; }

    if (_instantCompleteForTesting) { /* 기존 즉시 처치 */ return; }

    BossEncounterLauncher.Launch(new BossEncounterRequest
    {
        mode = BossEncounterContext.Mode.Daily,
        boss = boss,
        endgameMode = _dailyBoss,
        returnSceneName = "Attic",
    });
}

// OnWeeklyClicked 동일 패턴
```

**검증**:
- 컴파일 OK
- Daily/Weekly 회귀 X
- 정적 `BossEncounterContext.Set` 직접 호출 코드 라인 수 감소 (Launcher 내부 1곳만)

**완료 조건**: 2 메서드 (OnDailyClicked / OnWeeklyClicked) 변경. `BossEncounterContext.Set` 외부 호출 EndgameMenuPanel 에서 제거.

#### Sprint 6 commit

```
5002 Sprint 6 — EndgameMenuPanel Launcher 위임 (8/10)

[변경]
- Scripts/UI/EndgameMenuPanel.cs — OnDailyClicked/OnWeeklyClicked 가 BossEncounterLauncher.Launch 호출
- 정적 BossEncounterContext.Set 직접 호출 제거

[검증]
- Daily/Weekly 회귀 X
- 174L → ~150L (DI 진입점 정리)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 7 — Test + ProjectValidator 룰 (1 commit)

### 7.1 BossSceneSeparationTests.cs (5+ NUnit)

**파일**: `Assets/_Project/Tests/Editor/BossSceneSeparationTests.cs` (신규)

**산출물 (테스트 5+)**:
1. `Launcher_Default_NotNull` — `BossEncounterLauncher.Default != null`
2. `Launcher_Launch_FillsContext_ChapterBoss` — Mock launcher 주입 후 Launch → Context.PendingType==ChapterBoss / PendingBoss==boss / PendingChapter==chapter
3. `Launcher_Launch_FillsContext_Daily` — Daily 모드 동일 검증
4. `CompleteChapterBoss_Null_NoOp` — null boss/chapter → 예외 없이 return
5. `CompleteChapterBoss_AppliesLootTable` — Mock BossLootTable 주입 → CurrencyService.StarDust 증가 확인
6. `Context_Clear_ResetsAllFields` — Set 후 Clear → 5 필드 모두 null
7. `Mode_Enum_Has_ChapterBoss` — Reflection 검증 (Mode.ChapterBoss 존재)

**검증 (NUnit)**:
- Test Runner EditMode → 5+ 통과
- 기존 Test 46 회귀 X (총 51+)

### 7.2 ProjectValidator 룰 — 정적 Context 외부 직접 R/W 검사

**파일**: `Assets/_Project/Scripts/Editor/ProjectValidator.cs` (변경)

**산출물 (룰 추가)**:
- `BossEncounterContext.Set(` 정규식 검색 — 허용 위치: `BossEncounterLauncher.cs` / `BossEncounterRunner.cs` 만. 그 외 파일 발견 시 경고.
- 검색 대상: `Assets/_Project/Scripts/**/*.cs`
- 보고: 위반 파일·라인 목록

**검증**:
- Tools > LittleHero > Validate Project 실행 → 위반 0
- 위반 1+ 도입 시 경고 메시지 노출

**완료 조건**: NUnit 5+ 통과 + Validator 룰 1 추가.

#### Sprint 7 commit

```
5002 Sprint 7 — NUnit 5+ + ProjectValidator BossEncounterContext.Set 외부 호출 룰 (9/10)

[변경]
- Tests/Editor/BossSceneSeparationTests.cs 신규 (5+ NUnit)
- Editor/ProjectValidator.cs — BossEncounterContext.Set 외부 호출 검사 룰

[검증]
- Test Runner EditMode 통과 (46 → 51+)
- Validate Project 실행 — 위반 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 8 — docs + changelog + CLAUDE.md (1 commit)

### 8.1 changelog_v1.md 등재 + CLAUDE.md §12/§13 갱신

**파일**:
- `docs/little_hero/changelog_v1.md` (changelog-manager 스킬)
- `CLAUDE.md` §12 시스템 현황 / §13 다음 작업 후보 / §14 최근 핵심 결정

**산출물**:
- changelog_v1.md 에 "5002 boss_scene_separation_v1 — 챕터 보스 BossEncounter 씬 분리, 8 Sprint 10 commit" 항목 추가
- CLAUDE.md §12 활성 시스템 — `BossEncounterLauncher` / `BossEncounterRequest` 추가
- §12 SO 인스턴스 — 변경 없음 (코드만)
- §12 Tests/Editor — 46 → 51+ (BossSceneSeparation 5+)
- §13 다음 작업 후보 — "🟢 F4/F5 보스 씬 분리" 완료 표시 / 다음 작업 추가 (Mini boss 분리 옵션 D 후속 검토 등)
- §14 최근 핵심 결정 — 2026-05-04: 5002 boss_scene_separation_v1 추가

**검증**:
- changelog_v1.md 형식 일관 (5001 패턴 답습)
- CLAUDE.md 라인 수 변화 합리 범위

#### Sprint 8 commit

```
5002 Sprint 8 — docs 통합 + changelog + CLAUDE.md §12/§13/§14 (10/10)

[변경]
- docs/little_hero/changelog_v1.md — 5002 항목 등재
- CLAUDE.md §12 활성 시스템 (Launcher 추가) / §12 Tests 46→51+
- CLAUDE.md §13 — F4/F5 보스 씬 분리 완료 표시
- CLAUDE.md §14 — 최근 핵심 결정 5건 회전

[검증]
- 마크다운 lint X
- 라인 수 합리 범위

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 검증 시나리오 (Play 검증)

### S1 — 챕터 1 보스 처치 (정상 흐름)

1. Attic → 챕터 1 시작 → wave 1~19 진행 (미니보스 5/10/15 정상 등장)
2. wave 20 진입 직전 BossEncounter 씬으로 fade in
3. BossEncounter 씬: 챕터 1 BG (red_riding_hood) + 보스 단독 spawn + Phase 1 패턴
4. HP 50% 도달 → Phase 2 전환 + 1.3× 속도
5. 보스 처치 → 별 흩어짐 VFX 2초 → Attic 복귀
6. **검증**:
   - 콘솔 `[BossEncounter] 시작 — type=ChapterBoss boss=...`
   - 콘솔 `[BossEncounterService] CompleteChapterBoss: ... (chapter 1)`
   - 콘솔 `[BossLootGrantService] Applied ChapterBoss (4 rewards)`
   - SaveData `firstClearedChapters` bit 0 set
   - StarFragments / Starlight / MagicCrystal 증가
   - **(D5 Rebuttal)** Phase 2 진입 (HP 50%) 시 텔레그라프 0.7s + 1.3× 속도 정상
   - **(D5 Rebuttal)** Critical 적중 시 slow-mo 0.08s + screen shake 정상
   - **(D5 Rebuttal)** VfxPool.SpawnHitStar / DamageFloater pop 정상
   - **(D5 Rebuttal)** DontDestroyOnLoad: GameManager / AudioManager / VfxPool 객체 BossEncounter 씬에서 참조 가능

### S2 — 챕터 보스 사망 (실패 흐름)

1. wave 20 진입 → BossEncounter 씬
2. 의도적 사망
3. **검증**:
   - 콘솔 `[BossEncounter] 사망 — 보상 없이 Attic 복귀`
   - SaveData firstClearedChapters 변동 X
   - BossLootGrantService.Apply 호출 X

### S3 — 일일 보스 (회귀 — Daily)

1. Attic → Endgame 메뉴 → 일일 보스 클릭
2. BossEncounter 씬 (BG 미적용 — 기본 BG)
3. 보스 처치 → Starlight 보상 + 일일 마크
4. **검증**:
   - 콘솔 `[EndgameMenu] 일일 보스: ...`
   - BossEncounterLauncher.Launch(Daily req) 호출 (정적 Context.Set 직접 호출 X)

### S4 — 주간 보스 신화 발현 (회귀 — Weekly)

1. Endgame 메뉴 → 주간 보스 클릭 → 처치
2. 신화 발현 → Attic 복귀 후 1회 Mythic Overlay
3. **검증**:
   - WeeklyBossResult.success=true / mythicGranted != null
   - AtticController.HandleReturnFromBossEncounter 1회 발동
   - LastMythicDefinition Clear (재진입 시 중복 X)

### S5 — 챕터 BG 적용 검증 (5002 신규)

1. 챕터 5 (오즈의 마법사) 보스 진입
2. BossEncounter 씬 BG = chapter_5_oz coverArt
3. **검증**: SpriteRenderer.sprite == ChapterData.coverArt / sortingOrder=-100

---

## 완료 정의 (Definition of Done)

- [ ] 10 commit 모두 push (Sprint 1~8)
- [ ] 컴파일 0 에러 (Editor + Player)
- [ ] NUnit 51+ 통과 (BossSceneSeparation 5+ 신규)
- [ ] Validate Project 위반 0
- [ ] Play 검증 S1~S5 모두 통과
- [ ] CLAUDE.md §12/§13/§14 갱신
- [ ] changelog_v1.md 등재
- [ ] git status clean

---

## 알려진 위험 + 완화 (요약)

| 위험 | 완화 |
|---|---|
| 씬 전환 1회 추가 (~1초) | **D4=B mediator**: 5002 sync 유지 → 5004 별 최적화 sprint 에서 LoadSceneAsync + fade-in 모든 씬 전환 일괄 적용 |
| ChapterCleared 패널 표시 시점 변경 | Attic 복귀 직후 1회 toast (`PendingChapterClearedId` 플래그) |
| 정적 Context 호환성 (회귀) | Sprint 1~2 분리 commit. Daily/Weekly 흐름 유지 |
| BossEncounter 씬 wiring 누락 | Sprint 3.2 Editor 작업 + Inspector 검증 |
| MCP SIGSEGV (CLAUDE.md §13) | Sprint 단위 commit, 씬 wiring 후 10초+ 대기 |
| Phase 2 텔레그라프 회귀 | DontDestroyOnLoad 검증 (GameManager / AudioManager / VfxPool / DamageFloaterPool) |
| BossLootTable null fallback | Sprint 4 LogWarning + boss.starDustDrop / xpDrop 미세 보상 가능 (사용자 선택) |

---

## Source

- 디자인 문서: `5002_boss_scene_separation_v1.md`
- 선행 시스템: `5001_boss_system_v2_prd.md` (Sprint 분해 패턴 답습)
- CLAUDE.md §13 #8 / §15 OOP 정책
- 코드: `BossEncounterRunner` / `BossEncounterContext` / `BossEncounterService` / `ChapterRunner` / `EndgameMenuPanel` / `BossLootGrantService`
