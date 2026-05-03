# 5003 — Mini Boss Scene Separation v2 PRD (재진입 — 옵션 B 채택)

> **PRD v2 (재진입 후 Mediator 확정)**
> 원본: `5003_*_v1_prd.md` · Decisions: `5003_*_decisions.md` (재진입 2차)
> 재진입: 사용자 `/prd-pipeline 5003 --from breakdown` (2026-05-04)
> Phase 2.3 재결정: **D1=B (wave 15만 분리), D2=A (§11 등재)** — RESERVED 영구 해제
>
> **상태**: 🟢 **진행 중** — Phase 3 Breakdown 시작. 5 Sprint × 5 commit × 1시간 40분.
>
> **번호**: 5003 / **출처**: `docs/prd/5003_mini_boss_scene_separation_v1.md` (디자인)
> **본 PRD Sprint 1~5 명세는 미래 옵션 변경 시 활용 가능** (옵션 B 가정 그대로 보존).
>
> ### Mediator 결정 채택 (2건)
> - **D1 = E**: 분리 X — 본 PRD 폐기 (RESERVED 영구). UX 단조로움 회피 + 5002 의도 보존.
> - **D2 = C**: §11 마일스톤 미등재 — D1 결과 따름. Sprint 5 자체 미실행.
>
> ### Rebuttal 자동 수용 (1건, 미실현)
> - **D3**: BalanceConfig enum 통합 — D1=E 결정으로 **구현 X** (Sprint 1 자체 미실행)
>
> ### 미래 재진입 조건
> 사용자 발화 시 본 PRD 재활성화:
> - "5003 옵션 B 진행" → `/prd-pipeline 5003 --from breakdown` (Sprint 1~5 활용)
> - "5003 옵션 A 진행" → PRD v3 재작성 (Sprint 분해 변경)
> - "5003 폐기" → `docs/prd/5003/` 삭제 또는 `_archive` 이동

## 0. 전체 개요 (옵션 B 기준)

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | BossEncounterContext.Mode.MiniBoss + BalanceConfig.separateLastMiniBoss 토글 | 1 | 15분 |
| 2 | BossEncounterRunner Mode.MiniBoss 분기 + BossEncounterService.CompleteMiniBoss | 1 | 25분 |
| 3 | ChapterRunner wave==15 분기 → Launcher.Launch(MiniBoss req) | 1 | 25분 |
| 4 | BossSceneSeparationTests 갱신 (NUnit 8 → 10) + ProjectValidator 회귀 | 1 | 20분 |
| 5 | docs + changelog_v1.md 등재 + CLAUDE.md §12/§14 갱신 | 1 | 15분 |
| **총** | | **5** | **1시간 40분** |

## 의존성 그래프

```
Sprint 1 (Mode + Toggle) ──┐
                           ├─→ Sprint 3 (ChapterRunner) ──┐
Sprint 2 (Runner+Service) ─┘                              │
                                                          ├─→ Sprint 4 (Test) → Sprint 5 (docs)
```

병렬 가능: Sprint 1 ↔ Sprint 2 (서로 독립)

---

## Sprint 1 — Mode.MiniBoss + BalanceConfig 토글 (1 commit)

### 1.1 BossEncounterContext.Mode 확장

**파일**: `Scripts/Core/BossEncounterContext.cs` (변경, 기존 5002 정합)

**변경**:
```csharp
public enum Mode { Daily, Weekly, ChapterBoss, MiniBoss }   // 5003 — MiniBoss 추가
```

### 1.2 BalanceConfig 토글

**파일**: `Scripts/Data/BalanceConfig.cs` (변경)

**변경**:
```csharp
[Header("5003 — 미니보스 씬 분리")]
[Tooltip("true = 마지막 미니보스 wave (15) 처치를 BossEncounter 씬으로 분리. 옵션 B 채택 (디자인 §2)")]
public bool separateLastMiniBoss = true;   // 기본 활성
[Tooltip("true = 모든 미니보스 wave (5/10/15) 분리. 옵션 A. separateLastMiniBoss 보다 우선")]
public bool separateAllMiniBosses = false;
```

### 1.3 검증 기준
- 컴파일 OK
- 기존 NUnit 8/8 회귀 X (Mode_Enum_Has_ChapterBoss → MiniBoss 추가 후도 통과)
- BalanceConfig SO 인스턴스 마이그레이션 자동 (default 값 적용)

#### Sprint 1 commit

```
5003 Sprint 1 — Mode.MiniBoss + BalanceConfig 토글 (1/5)

[변경]
- Scripts/Core/BossEncounterContext.cs — Mode enum 에 MiniBoss 추가
- Scripts/Data/BalanceConfig.cs — separateLastMiniBoss / separateAllMiniBosses SerializeField

[검증]
- 컴파일 OK
- 5002 NUnit 8/8 회귀 X
- SO 마이그레이션 자동 (default true / false)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
EOF
```

---

## Sprint 2 — BossEncounterRunner Mode.MiniBoss 분기 + Service (1 commit)

### 2.1 BossEncounterRunner 분기

**파일**: `Scripts/Gameplay/Waves/BossEncounterRunner.cs`

**변경 (OnBossDefeated)**:
```csharp
case BossEncounterContext.Mode.MiniBoss:
    BossEncounterService.CompleteMiniBoss(
        BossEncounterContext.PendingBoss,
        BossEncounterContext.PendingChapter);
    break;
```

ChapterBoss 와 동일한 BG 적용 (BackgroundApplier) + LoadSceneAfterDelay(returnScene).

### 2.2 BossEncounterService.CompleteMiniBoss

**파일**: `Scripts/Core/BossEncounterService.cs`

**신규**:
```csharp
public static void CompleteMiniBoss(EnemyData boss, ChapterData chapter)
{
    if (boss == null || chapter == null) return;
    if (boss.lootTable != null) BossLootGrantService.Apply(boss.lootTable);  // BossLootTable.MiniBoss tier
    Debug.Log($"[BossEncounterService] CompleteMiniBoss: {boss.displayName} (chapter {chapter.chapterNumber})");
    // 챕터 클리어 X (미니보스는 보스 클리어 아님). pendingChapterClearedId set X.
    // GameEvents.OnEnemyKilled 는 일반 흐름에서 raise (별도 처리 X)
}
```

라인 변동: BossEncounterService 180L → ~205L

### 2.3 검증 기준
- 컴파일 OK
- 기존 Daily/Weekly/ChapterBoss 회귀 X
- MiniBoss 처치 시 BossLootTable.MiniBoss 보상 (★50 + ✦1) 만 grant — firstClearedChapters 변동 X

#### Sprint 2 commit
```
5003 Sprint 2 — BossEncounterRunner.MiniBoss 분기 + CompleteMiniBoss (2/5)

[변경]
- Scripts/Gameplay/Waves/BossEncounterRunner.cs — Mode.MiniBoss switch 분기
- Scripts/Core/BossEncounterService.cs — CompleteMiniBoss(boss, chapter) 신규

[검증]
- 컴파일 OK
- Daily/Weekly/ChapterBoss 회귀 X
- MiniBoss = 챕터 클리어 X (firstClearedChapters 변동 X)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 3 — ChapterRunner wave==15 분기 (1 commit)

### 3.1 wave == 15 진입 시 Launcher.Launch(MiniBoss req)

**파일**: `Scripts/Gameplay/Waves/ChapterRunner.cs`

**변경 (StartNextWave)**:
```csharp
private void StartNextWave()
{
    int wave = GameManager.Instance.CurrentWave;
    BalanceConfig balance = GameManager.Instance.Balance;

    // 5002 — 챕터 보스 wave 진입 시 BossEncounter 씬으로 분리
    if (wave >= _chapterData.totalWaves && _chapterData.bossEnemy != null) { /* 5002 분기 */ return; }

    // 5003 — 미니보스 wave 분리 (옵션 B 기본 / 옵션 A 토글)
    bool isMiniBossWave = wave > 0 && wave % balance.miniBossWaveInterval == 0 && wave < _chapterData.totalWaves;
    bool shouldSeparate = isMiniBossWave && (
        balance.separateAllMiniBosses
        || (balance.separateLastMiniBoss && wave == _chapterData.totalWaves - balance.miniBossWaveInterval)  // 옵션 B = wave 15
    );
    if (shouldSeparate && _chapterData.miniBossPool?.Length > 0)
    {
        var mb = _chapterData.miniBossPool[Random.Range(0, _chapterData.miniBossPool.Length)];
        SaveSession();
        BossEncounterLauncher.Launch(new BossEncounterRequest
        {
            mode = BossEncounterContext.Mode.MiniBoss,
            boss = mb,
            chapter = _chapterData,
            returnSceneName = "Chapter",  // 챕터 씬 복귀 (다음 wave 진행)
        });
        return;
    }

    // 기존 일반/미니보스 흐름 그대로
    WaveConfig config = BuildWaveConfig(wave);
    if (config.miniBoss != null) AudioManager.Instance?.PlayBgm("miniboss_chase", crossFadeSec: 1.0f);
    _waveManager.StartWave(config, OnWaveComplete);
}
```

### 3.2 ChapterRunner — Chapter 씬 복귀 + wave 재개 흐름

**중요 결정 — returnSceneName**: MiniBoss 처치 후 Attic 복귀? 또는 Chapter 씬 복귀 + wave 재개?

- 옵션 X: Attic 복귀 — 챕터 흐름 끊김. 사용자 다시 챕터 진입 필요.
- 옵션 Y ★권장: Chapter 씬 복귀 + activeChapterWave 재개 (5002 의 SaveSession 재활용).

**구현 (옵션 Y)**: `BossEncounterRequest.returnSceneName = "Chapter"` + ChapterRunner.Start 의 resumeWave 흐름 활용 (이미 5002 이전부터 존재).

미니보스 처치 후 BossEncounter 씬 종료 → Chapter 씬 재로드 → ChapterRunner.Start 가 `activeChapterWave` 검사 → wave 16부터 재개.

### 3.3 검증 기준
- Play Mode — 챕터 1 wave 1~14 정상 (미니보스 5/10 챕터 씬 그대로 등장 — 옵션 B 기본)
- wave 15 진입 시 BossEncounter 씬 전환 (BG = chapter_1)
- 미니보스 처치 → Chapter 씬 재진입 → wave 16부터 재개
- wave 16~19 일반 적 → wave 20 = 챕터 보스 (5002)

#### Sprint 3 commit
```
5003 Sprint 3 — ChapterRunner wave==15 Launcher.Launch(MiniBoss) (3/5)

[변경]
- Scripts/Gameplay/Waves/ChapterRunner.cs — StartNextWave 에 isMiniBossWave + shouldSeparate 분기
- BossEncounterRequest.returnSceneName = "Chapter" (Attic X — wave 재개)

[검증]
- 챕터 1 wave 1~14 회귀 X (미니보스 5/10 챕터 씬)
- wave 15 → BossEncounter 씬
- 처치 → Chapter 씬 재진입 + wave 16 재개

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 4 — Test + Validator 회귀 (1 commit)

### 4.1 BossSceneSeparationTests 갱신

**파일**: `Tests/Editor/BossSceneSeparationTests.cs` (변경)

**추가 NUnit (2)**:
```csharp
[Test]
public void Mode_Enum_Has_MiniBoss()
{
    var values = System.Enum.GetValues(typeof(BossEncounterContext.Mode));
    CollectionAssert.Contains(values, BossEncounterContext.Mode.MiniBoss);
}

[Test]
public void CompleteMiniBoss_Null_NoOp()
{
    Assert.DoesNotThrow(() => BossEncounterService.CompleteMiniBoss(null, null));
}
```

NUnit 8 → 10. 전체 EditMode 53/54 → 55/56 예상 (5003 신규 2 + 기존 1 무관 FAIL 그대로).

### 4.2 ProjectValidator 회귀

기존 `ValidateBossEncounterContextAccess` 룰 그대로. 변경 X.

#### Sprint 4 commit
```
5003 Sprint 4 — BossSceneSeparationTests 8→10 + Validator 회귀 (4/5)

[변경]
- Tests/Editor/BossSceneSeparationTests.cs — Mode_Enum_Has_MiniBoss / CompleteMiniBoss_Null_NoOp 추가

[검증]
- Test Runner EditMode 10/10 PASS (5003 신규)
- 5002 NUnit 8/8 회귀 X
- Validator §15.7 룰 0 위반

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 5 — docs + changelog + CLAUDE.md (1 commit)

### 5.1 changelog_v1.md 등재

```markdown
- 2026-XX-XX: **5003 mini_boss_scene_separation_v1** — 미니보스 씬 분리 (옵션 B 채택, wave 15만). 5 Sprint 5 commit. BossEncounterContext.Mode.MiniBoss + BalanceConfig 토글 (separateLastMiniBoss / separateAllMiniBosses) + CompleteMiniBoss + ChapterRunner 분기. NUnit 54→56.
```

### 5.2 CLAUDE.md 갱신

- §11 마일스톤: `mini_boss_scene_separation_v1` 행 추가
- §12 활성 시스템: `BossEncounterLauncher` (5003 — Mode.MiniBoss 추가)
- §12 Tests: 54 → 56 (BossSceneSeparation 8 → 10)
- §13 다음 작업 후보: "5003 미니보스 씬 분리" 완료 표시
- §14: 최근 핵심 결정 5건 회전

#### Sprint 5 commit
```
5003 Sprint 5 — docs + changelog + CLAUDE.md (5/5)

[변경]
- docs/little_hero/changelog_v1.md — 5003 항목 등재
- CLAUDE.md §11 마일스톤 / §12 시스템·Tests / §13 후속 / §14 회전

[검증]
- 마크다운 lint X / 5002 패턴 일관

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 검증 시나리오 (Play 검증)

### S1 — 챕터 1 wave 15 미니보스 분리 (옵션 B 기본)

1. 새 SaveData → 챕터 1 시작 → wave 1~14 진행
2. wave 5/10 — 챕터 씬에 미니보스 + 일반 적 (분리 X 확인)
3. wave 15 직전 BossEncounter 씬 전환
4. BG = chapter_1 (red_riding_hood) + 미니보스 단독 spawn
5. 미니보스 처치 → Chapter 씬 재진입 → wave 16 재개
6. **검증**:
   - 콘솔 `[BossEncounter] 시작 — type=MiniBoss boss=...`
   - 콘솔 `[BossEncounterService] CompleteMiniBoss: ...`
   - 콘솔 `[BossLootGrantService] Applied MiniBoss (1 reward)`
   - SaveData StarFragments / StarDust 증가 (firstClearedChapters 변동 X)
   - Chapter 씬 wave 16 정상 진행

### S2 — 옵션 A 토글 (모든 미니보스 분리)

1. BalanceConfig.separateAllMiniBosses = true 설정
2. 챕터 1 진입 → wave 5/10/15 모두 BossEncounter 씬 전환
3. **검증**: 4 transitions 모두 정상 (5/10/15 + 보스 20)

### S3 — 옵션 E 토글 (분리 X)

1. BalanceConfig.separateLastMiniBoss = false / separateAllMiniBosses = false
2. 챕터 1 wave 5/10/15 모두 챕터 씬 그대로 (5002 종료 상태와 동일)
3. **검증**: 5002 흐름 회귀 X

### S4 — 5002 회귀 (Daily/Weekly/ChapterBoss)

1. Endgame 일일/주간 보스 + 챕터 보스 (wave 20)
2. **검증**: 5002 흐름 회귀 X (5/8 시나리오 모두)

---

## 완료 정의 (Definition of Done)

- [ ] 5 commit 모두 push
- [ ] 컴파일 errors=0
- [ ] NUnit 56 PASS (10/10 5003 신규 + 5002 회귀 X)
- [ ] Validator warnings 변동 0
- [ ] Play S1, S4 PASS (S2, S3 토글 검증은 선택)

---

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 씬 전환 1회 추가 (wave 15) ~1초 | 5004 LoadSceneAsync + fade (모든 씬 전환 일괄 적용 시 자동 해소) |
| Chapter 씬 재진입 시 wave 재개 실패 | 5002 SaveSession + activeChapterWave 흐름 재활용 (이미 검증된 패턴) |
| 미니보스 처치 후 ChapterCleared 잘못 raise | BossEncounterService.CompleteMiniBoss 에서 RaiseChapterCleared 호출 X (CompleteChapterBoss 와 차이) |
| 옵션 변경 시 회귀 | BalanceConfig 토글로 분리 — 원샷 ON/OFF 가능 |
| 5002 회귀 | Sprint 4 NUnit 회귀 검증 + Phase 6 자동 |

---

## Source

- `docs/prd/5003_mini_boss_scene_separation_v1.md` (디자인)
- `docs/prd/5002/5002_*_decisions.md` (D3=A, 5003 후속 명시)
- 5002 인프라: BossEncounterLauncher / BossEncounterContext / BossEncounterRunner / BackgroundApplier / BossSceneSeparationTests
