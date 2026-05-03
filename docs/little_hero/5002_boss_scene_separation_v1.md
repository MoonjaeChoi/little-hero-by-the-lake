# 5002 — Boss Scene Separation v1

> **목적**: 챕터 보스(20웨이브) 전투를 별도 `BossEncounter` 씬으로 분리하여, 의식적 전환점(cinematic transition) + 메모리 격리(일반 적 풀 unload) + 보스 전용 BG/카메라/BGM 일관성을 확보한다.
> **번호**: 5002 / **선행**: 5001 boss_system_v2 (BossTier·IBossPhase·BossLootTable 4 인스턴스 완료)
> **CLAUDE.md §13 #8**: F4/F5 보스 씬 분리 — BossEncounterService 완성, 별도 보스 전투 씬

## 0. 결정 이력

| 날짜 | 결정 | 출처 |
|---|---|---|
| 2026-05-04 | 5001 boss_system_v2 완료 (BossTier 4 + IBossPhase + Factory + LootTable 4) | changelog_v1.md |
| 2026-05-04 | Endgame(Daily/Weekly) BossEncounter 씬 wiring 완료 | EndgameMenuPanel L70/97 |
| 2026-05-04 | **(미결)** 챕터 보스(20웨이브) / 미니보스(5/10/15) 씬 분리 범위 | 본 문서 §2 |

## 1. 현재 시스템 분석

### 1.1 기 구현 (Endgame 한정 분리 완료)

| 파일 | 라인 | 책임 |
|---|---|---|
| `Scenes/BossEncounter.unity` | — | Daily/Weekly 보스 전용 씬 (Build Settings 4번째) |
| `Gameplay/Waves/BossEncounterRunner.cs` | 118 | 단일 보스 spawn → 처치 hook → Attic 복귀 |
| `Core/BossEncounterContext.cs` | 40 | **정적 전역** — Mode/PendingBoss/PendingMode/PendingMythicPool/LastMythic 전달 |
| `Core/BossEncounterService.cs` | 149 | ResolveDailyBoss / ResolveWeeklyBoss / Complete×2 / GuaranteeMythic |
| `UI/EndgameMenuPanel.cs` | 174 | Daily/Weekly 클릭 → Context.Set → SceneManager.LoadScene("BossEncounter") |
| `Hub/AtticController.HandleReturnFromBossEncounter` | (393L 중) | 신화 발현 1회 오버레이 |

### 1.2 분리되지 않은 영역 (미완성)

`Gameplay/Waves/ChapterRunner.cs` L150~173 (`BuildWaveConfig`) — 챕터 보스 + 미니보스가 **같은 씬**에서 처리:

```csharp
bool isMiniB = waveNumber % balance.miniBossWaveInterval == 0
               && waveNumber < _chapterData.totalWaves;
bool isBoss = waveNumber >= _chapterData.totalWaves;
EnemyData mb = isMiniB && _chapterData.miniBossPool != null && _chapterData.miniBossPool.Length > 0
               ? _chapterData.miniBossPool[Random.Range(0, ...)] : null;
EnemyData bs = isBoss ? _chapterData.bossEnemy : null;
return new WaveConfig { ..., miniBoss = mb, boss = bs };
```

### 1.3 위반 사항 (CLAUDE.md §15)

| 항목 | 위반 | 위치 |
|---|---|---|
| §15.7 DI | 정적 전역 `BossEncounterContext` 5 필드 직접 R/W | EndgameMenuPanel L70/97, BossEncounterRunner L39/49/70/82, AtticController L374 |
| §15.5 EventBus | Endgame 보스 처치 결과를 정적 필드(`LastMythicDefinition`)로 전달 | BossEncounterContext L20 |
| §15.6 100줄+ | `EndgameMenuPanel` 174L (씬 진입 + 가용성 표시 + 즉시 처치 시뮬 + 신화 오버레이) | EndgameMenuPanel.cs |
| §15.6 100줄+ | `AtticController` 393L (다중 책임) | AtticController.cs |
| 미완성 hook | `BossLootGrantService.Apply` 가 챕터 보스 처치 시 자동 호출되지 않음 (주석에 "BossEncounterRunner.Begin 흐름에서 처리" — 미구현) | BossLootGrantService L30~32 |
| 씬-환경 격리 | BossEncounter 씬은 챕터 BG 미적용 (`ChapterRunner.ApplyChapterBackground` 미상속) | BossEncounter.unity |

### 1.4 SO 분포 현황

- `EnemyData.bossTier` (5001) — 262 적 SO 중 MiniBoss 30 / ChapterBoss 35 / DailyBoss 7 / WeeklyBoss 4 / None 186
- `EnemyData.lootTable` — Mini/Chapter/Daily/Weekly 4 BossLootTable wiring 완료
- `ChapterData.bossEnemy` 35 / `ChapterData.miniBossPool` 35 wiring

## 2. 옵션 비교

| 옵션 | 분리 범위 | 장점 | 단점 |
|---|---|---|---|
| **A** | 현 상태 유지 (Endgame Daily/Weekly만) | 변경 0, 안정적 | §13 #8 미완. 챕터 보스 cinematic 약함. BossLootTable hook 미완성 |
| **B** | 챕터 보스(20웨이브)만 추가 분리 | 의식적 전환점·메모리 격리·보스 BG 일관성. 5/10/15는 양념 | 씬 전환 1회/챕터 (~1초). ChapterRunner 종료 흐름 변경 |
| **C** ★ 권장 | 챕터 보스만 분리 + 정적 Context → DI 점진 전환 + LootTable hook 완성 | 옵션 B + §15 정합. BossLootGrantService 자동 호출 | Sprint 추가 (DI 리팩토링) |
| **D** | 챕터 보스 + 미니보스(5/10/15) 모두 분리 | 모든 보스 cinematic 통일 | 씬 전환 4회/챕터. 모바일 부하·UX 단조로움 |
| **E** | LoadSceneAsync + Additive 로 BG/UI overlay 만 교체 | 씬 한 번도 교체 안 함, 메모리 격리는 약함 | 구현 복잡 (Additive Scene Hierarchy 관리) |

**★ 권장 = C** — 챕터 보스만 분리 + §15 정합 작업 동반.
- 미니보스(5/10/15)는 그대로 챕터 씬에 둠 (양념 — UX 단조로움 회피)
- 챕터 보스는 wave==totalWaves 직전에 `BossEncounterRunner` 로 transition (Phase 2 전환·텔레그라프·Premium 피격 살림)
- 보상은 `BossLootGrantService.Apply` 가 챕터 보스 처치 시점에 자동 호출되도록 hook 정비
- `BossEncounterContext` 정적 전역은 유지하되 (호환성), 신규 `BossEncounterRequest` DTO + `IBossEncounterLauncher` 인터페이스로 진입부만 점진 전환 (§15.7 DI)

## 3. 권장 설계 (CLAUDE.md §15 정합)

### 3.1 신규 / 변경 파일 명세

| 종류 | 경로 | 책임 | 라인 예상 |
|---|---|---|---|
| 신규 enum | `Scripts/Core/BossEncounterContext.cs` 갱신 | `Mode` 에 `ChapterBoss` 추가 | +1 |
| 신규 struct | `Scripts/Core/BossEncounterRequest.cs` | DTO (mode/boss/lootTable/returnSceneName/chapterCoverArt) | ~30 |
| 신규 interface | `Scripts/Core/IBossEncounterLauncher.cs` | `Launch(BossEncounterRequest)` (§15.7 DI 진입점) | ~10 |
| 신규 service | `Scripts/Core/BossEncounterLauncher.cs` | 정적 service. 내부적으로 Context 채우고 LoadScene | ~50 |
| 변경 | `Gameplay/Waves/ChapterRunner.cs` | `BuildWaveConfig` 에서 `isBoss` 경로 → BossEncounterLauncher.LaunchChapterBoss 후 종료 | ~189→~210 |
| 변경 | `Gameplay/Waves/BossEncounterRunner.cs` | `Mode.ChapterBoss` 분기 추가 + 챕터 BG 적용 + 처치 시 BossLootGrantService.Apply | 118→~170 |
| 변경 | `Core/BossEncounterService.cs` | `CompleteChapterBoss(EnemyData boss, ChapterData chapter)` 신규. BossLootGrantService.Apply 호출 | 149→~180 |
| 변경 | `UI/EndgameMenuPanel.cs` | `BossEncounterContext.Set` 직접 호출 → `BossEncounterLauncher.Launch` 로 위임 | 174→~150 |
| 변경 | `Scenes/BossEncounter.unity` | Background SpriteRenderer 추가 (ChapterRunner 와 동일 구성) — Editor wiring | — |
| 신규 Test | `Tests/Editor/BossSceneSeparationTests.cs` | Launcher.Launch + Context wiring 5+ NUnit | ~80 |

### 3.2 BossEncounterRequest DTO

```csharp
public struct BossEncounterRequest
{
    public BossEncounterContext.Mode mode;
    public EnemyData boss;
    public EndgameModeData endgameMode;        // Daily/Weekly 만
    public ChapterData chapter;                 // ChapterBoss 만 (BG/복귀 정보)
    public MythicDefinition[] mythicPool;       // Weekly 만
    public string returnSceneName;              // "Attic" 또는 "Chapter"
}
```

### 3.3 IBossEncounterLauncher

```csharp
public interface IBossEncounterLauncher
{
    void Launch(BossEncounterRequest request);
}
```

기본 구현 = 정적 `BossEncounterLauncher.Default` (§15.8 Service Locator). 테스트 시 mock 주입.

### 3.4 ChapterRunner 변경 흐름

```
StartNextWave()
  → wave == totalWaves 직전 분기
  → BossEncounterLauncher.Launch(new BossEncounterRequest {
       mode = Mode.ChapterBoss,
       boss = _chapterData.bossEnemy,
       chapter = _chapterData,
       returnSceneName = "Chapter",  // 클리어 시 → ChapterCleared 패널
    })
  → SceneManager.LoadScene("BossEncounter")
  → 보스 처치 → BossEncounterRunner.OnBossDefeated
       → BossEncounterService.CompleteChapterBoss(boss, chapter)
            → BossLootGrantService.Apply(boss.lootTable)
            → GameEvents.RaiseChapterCleared(chapter)
       → SceneManager.LoadScene("Attic") (returnSceneName)
```

**중요**: 챕터 클리어 시 ChapterRunner 가 더 이상 동작 X. ChapterCleared 패널은 다락방 복귀 직전에 `ChapterClearedToast` overlay 로 1회 표시 (또는 Attic 진입 후 1회).

### 3.5 BossEncounterRunner Mode.ChapterBoss 분기

```csharp
private void OnBossDefeated()
{
    switch (BossEncounterContext.PendingType)
    {
        case Mode.Daily:    /* 기존 */ break;
        case Mode.Weekly:   /* 기존 */ break;
        case Mode.ChapterBoss:
            var boss = BossEncounterContext.PendingBoss;
            var chapter = BossEncounterContext.PendingChapter;
            BossEncounterService.CompleteChapterBoss(boss, chapter);
            break;
    }
    StartCoroutine(LoadAtticAfterDelay());
}
```

### 3.6 챕터 BG 적용 (BossEncounterRunner)

```csharp
private void Start()
{
    ...
    if (BossEncounterContext.PendingType == Mode.ChapterBoss)
        ApplyChapterBackground(BossEncounterContext.PendingChapter);
    ...
}

private void ApplyChapterBackground(ChapterData ch) { /* ChapterRunner.ApplyChapterBackground 와 동일 */ }
```

`ChapterRunner.ApplyChapterBackground` 정적 헬퍼화 (`BackgroundApplier.Apply(ChapterData, SpriteRenderer)`) 가 §15.6 100줄 룰에도 도움.

## 4. CLAUDE.md §15 정합 검증

| 정책 | 검증 |
|---|---|
| §15.1 SOLID-S | `BossEncounterLauncher` (씬 진입) / `BossEncounterRunner` (전투) / `BossEncounterService` (보상) — 책임 분리 |
| §15.1 SOLID-O | `IBossEncounterLauncher` interface — 향후 Additive Scene 옵션 E 도 동일 인터페이스 |
| §15.1 SOLID-D | DTO `BossEncounterRequest` 명시 주입 (정적 Context 직접 R/W 회피) |
| §15.4 SO 우선 | `BossLootTable` (4 SO) 그대로 활용. 마법 숫자 없음 |
| §15.5 EventBus | `GameEvents.OnBossSpawned/Despawned` 그대로. ChapterCleared 도 기존 정적 EventBus |
| §15.6 100줄 룰 | 신규 `BossEncounterLauncher` ~50줄. `BossEncounterRunner` ~170줄로 증가 — 분리 한계점 검토 |
| §15.7 DI | `IBossEncounterLauncher` 진입 + `BossEncounterRequest` DTO. 기존 정적 Context 는 호환성 유지 |
| §15.8 Pattern | Strategy (모드별 Complete 분기) — switch 유지 (3 분기는 허용 범위) |
| §15.9 Test | `BossSceneSeparationTests` — Launcher.Launch / Request wiring / CompleteChapterBoss 5+ NUnit |
| §15.11 안티 패턴 | public 필드 신규 X (DTO struct 는 §15.2 SerializeField 패턴 예외) |
| §15.12 Validator | 추가 룰 — `BossEncounterContext` 정적 직접 참조 검사 (Launcher 외부 직접 R/W ❌) |

## 5. Sprint 분해 (개요)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 인프라 (Request DTO + IBossEncounterLauncher + BossEncounterLauncher) | 2 | 30분 |
| 2 | BossEncounterContext.Mode.ChapterBoss + PendingChapter 추가 | 1 | 15분 |
| 3 | BossEncounterRunner Mode.ChapterBoss 분기 + ApplyChapterBackground | 2 | 40분 |
| 4 | BossEncounterService.CompleteChapterBoss + BossLootGrantService hook 완성 | 1 | 25분 |
| 5 | ChapterRunner 챕터 보스 → Launcher.Launch 전환 (wave==totalWaves 분기) | 1 | 30분 |
| 6 | EndgameMenuPanel 리팩토링 → Launcher 위임 | 1 | 20분 |
| 7 | Test + ProjectValidator 룰 (5+ NUnit) | 1 | 30분 |
| 8 | docs + changelog_v1.md 등재 + CLAUDE.md §12/§13 갱신 | 1 | 15분 |
| **총** | | **10** | **3시간 25분** |

자세한 명세는 → [5002_boss_scene_separation_v1_prd.md](5002_boss_scene_separation_v1_prd.md)

## 6. 알려진 위험 + 완화

| 위험 | 영향 | 완화 |
|---|---|---|
| 씬 전환 1회 추가 (~1초) | 모바일 UX 미세 저하 | LoadSceneAsync + 검은 fade-in. ChapterRunner 종료 직전 1초 페이드 아웃 |
| `ChapterRunner` 종료 후 ChapterCleared 패널 표시 시점 | 패널 미표시 위험 | Attic 복귀 후 1회 toast (`MetaProgression.PendingChapterClearedId` 1회 플래그) |
| 정적 Context 5 필드 → struct 전환 시 회귀 | EndgameMenuPanel/BossEncounterRunner 동시 변경 필요 | Sprint 1~2 분리 commit. Sprint 1 = DTO 추가만, Sprint 2 = Context 확장 |
| 신화 오버레이 wiring (Endgame Weekly만) | ChapterBoss 모드에서도 발현 가능? | ChapterBoss 모드는 신화 보장 X (Weekly 한정 — `lootTable.guaranteedMythicChance < 1.0`) |
| MCP SIGSEGV (CLAUDE.md §13) | 다중 execute_code 시 도메인 리로드 | Sprint 단위 commit 분리. 씬 wiring(2x BossEncounter.unity 변경) 후 10초+ 대기 |
| Phase 2 텔레그라프·Premium 피격 회귀 | BossEncounter 씬에 CinematicEffects/VfxPool 미존재 가능 | DontDestroyOnLoad 검증 (GameManager/AudioManager/VfxPool) |

## 7. 후속 작업 (mediator 결정)

- **5003 미니보스(5/10/15) 씬 분리 검토** (사용자 결정 시) — 옵션 D 비교 답습. 5002 종료 후 사용자 의사 확인 필요. UX 단조로움 위험 → 검토 후 진행.
- **5004 LoadSceneAsync + fade-in 통합 sprint** — 5002/Chapter↔Attic/Attic↔InfiniteTower 모든 씬 전환에 일관 적용. 별 최적화 sprint.

## 8. 변경 이력

- 2026-05-04: v1 초안 — 옵션 C 권장 (챕터 보스 분리 + §15 정합)
- 2026-05-04: §7 후속 작업 추가 (D3/D4 mediator 결정 반영)

## 9. Sources

- `docs/little_hero/5001_boss_system_v2.md` (BossTier·IBossPhase·BossLootTable 4)
- `docs/little_hero/endgame_v1.md` §4·§5 (Daily/Weekly 보스 흐름)
- `docs/little_hero/architecture_analysis_v1.md` (§15 OOP 정책 출처)
- `CLAUDE.md` §13 #8 "F4/F5 보스 씬 분리"
- 코드: `BossEncounterRunner` / `BossEncounterContext` / `ChapterRunner` / `BossEncounterService` / `EndgameMenuPanel`
