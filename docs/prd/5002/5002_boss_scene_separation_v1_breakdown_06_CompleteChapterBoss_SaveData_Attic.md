# T06: CompleteChapterBoss + SaveData pendingChapterClearedId + AtticController toast (D2)

> **레이어**: Logic (Core + Hub + Meta)
> **의존성**: T03 (BossEncounterContext.Mode.ChapterBoss)
> **예상 시간**: 30분 / 1 commit (5002 Sprint 4, 6/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 4, D2 Mediator 결정

## 1. 작업 항목

### 1.1 기존 파일 수정 — BossEncounterService.cs

**경로**: `LittleHero/Assets/_Project/Scripts/Core/BossEncounterService.cs`

**변경**: T04 의 stub 을 본체로 교체:
```csharp
/// <summary>5002 — 챕터 보스 처치. BossLootGrantService.Apply + ChapterCleared + pendingChapterClearedId set (D2).</summary>
public static void CompleteChapterBoss(EnemyData boss, ChapterData chapter)
{
    if (boss == null || chapter == null)
    {
        Debug.LogWarning($"[BossEncounterService] CompleteChapterBoss: boss={boss} chapter={chapter}");
        return;
    }

    // 1) 보상 — BossLootTable 우선
    if (boss.lootTable != null)
        BossLootGrantService.Apply(boss.lootTable);
    else
        Debug.LogWarning($"[BossEncounterService] {boss.id}: lootTable null — fallback 보상 생략");

    // 2) 챕터 클리어 이벤트 (ChapterRunner 가 더 이상 동작 X — 직접 raise)
    GameEvents.RaiseChapterCleared(chapter);

    // 3) MetaProgression — firstClearedChapters bit set + pendingChapterClearedId set (D2)
    if (MetaProgression.Instance?.Data != null)
    {
        MetaProgression.Instance.MarkChapterFirstCleared(chapter);
        MetaProgression.Instance.Data.pendingChapterClearedId = chapter.id;
        SaveSystem.Save(MetaProgression.Instance.Data);
    }

    Debug.Log($"[BossEncounterService] CompleteChapterBoss: {boss.displayName} (chapter {chapter.chapterNumber})");
}
```

라인 수: 149L → ~180L

**선행 확인**:
- `MetaProgression.Instance.MarkChapterFirstCleared(chapter)` 메서드 존재 확인 (없으면 기존 호환 메서드 grep 으로 대체. 예: `firstClearedChapters` 비트 set 메서드)

### 1.2 기존 파일 수정 — SaveData (MetaProgressionData)

**경로**: `LittleHero/Assets/_Project/Scripts/Meta/MetaProgressionData.cs` (또는 SaveData 정의 파일)

**변경**: 신규 SerializeField 추가
```csharp
[Header("5002 — 챕터 클리어 1회 toast 트리거 (D2)")]
public string pendingChapterClearedId = "";  // Attic 진입 시 1회 표시 후 clear
```

**마이그레이션**: Newtonsoft 기본값 자동 → 별 마이그레이션 sprint 불필요.

### 1.3 기존 파일 수정 — AtticController.cs

**경로**: `LittleHero/Assets/_Project/Scripts/Hub/AtticController.cs`

**변경**: `Start()` 에 `HandlePendingChapterCleared()` 호출 + 메서드 신규
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

// ResolveChapterById 헬퍼 — 기존 _chapterButtons 리스트 사용
private ChapterData ResolveChapterById(string id)
{
    if (string.IsNullOrEmpty(id) || _chapterButtons == null) return null;
    foreach (var entry in _chapterButtons)
        if (entry?.chapter != null && entry.chapter.id == id) return entry.chapter;
    return null;
}
```

라인 수: 393L → ~415L (§15.6 임계 — 200L+ Validator 경고 가능. 본 sprint 에서는 허용, 후속 분리 검토)

**Inspector wiring**:
- AtticController 의 `_chapterClearedPanel` SerializeField 가 Attic 씬에 prefab 인스턴스로 wiring 되어 있는지 확인
- 없으면 ChapterClearedPanel prefab 추가 + Inspector 드래그

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` + 10초 대기 + errors=0
- `pendingChapterClearedId` 직렬화 검증 (JSON save/load 1회 round-trip)

### 2.2 Editor Test
- T09 에서 `CompleteChapterBoss_AppliesLootTable` / `CompleteChapterBoss_SetsPendingChapterClearedId` 검증

### 2.3 ProjectValidator
- AtticController 라인 ~415L — 200L+ 경고 (기존 393L 도 동일). 본 sprint 에서는 허용, 후속 분리 task 등록.

### 2.4 회귀
- 기존 Daily/Weekly 흐름 — `pendingChapterClearedId` set X (영향 X)
- AtticController 의 `HandleReturnFromBossEncounter` 회귀 X
- SaveData 마이그레이션 — 기존 save.json 에 필드 없음 → Newtonsoft 기본값 빈 문자열 자동

## 3. commit 메시지

```
5002 Sprint 4 — CompleteChapterBoss + BossLootGrantService hook + Attic toast (D2) (6/10)

[변경]
- Scripts/Core/BossEncounterService.cs — CompleteChapterBoss(boss, chapter) 본체 (T04 stub 교체)
- Scripts/Meta/MetaProgressionData.cs — pendingChapterClearedId 필드 신규 (D2 mediator)
- Scripts/Hub/AtticController.cs — HandlePendingChapterCleared 1회 toast 메서드 + ResolveChapterById 헬퍼
- (필요 시) Scenes/Attic.unity — ChapterClearedPanel prefab 인스턴스 wiring

[검증]
- 컴파일 OK
- 기존 Complete×2 회귀 X
- BossLootTable null safe (fallback 로그)
- 신규 SaveData 필드 마이그레이션 자동 (빈 문자열 기본)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 4
- 디자인 §3.4 ChapterRunner 변경 흐름
- decisions D2 — SaveData 단일 진실원

## 5. 알려진 위험

- `MetaProgression.MarkChapterFirstCleared` 메서드 미존재 가능 → grep 후 호환 메서드 사용 또는 신규 추가
- AtticController 라인 ~415L Validator 경고 — 후속 task 로 분리 권장 (5002 스코프 외)
- ChapterClearedPanel.ShowForChapter 메서드 미존재 가능 → 기존 Show 메서드 시그니처 확인
- SaveData JSON 직렬화 — Unity Serialization 기본 동작 확인 (필드 추가 시 기본값 자동)
