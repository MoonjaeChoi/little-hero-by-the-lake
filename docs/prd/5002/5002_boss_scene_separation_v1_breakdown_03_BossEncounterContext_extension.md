# T03: BossEncounterContext.Mode.ChapterBoss + 필드 2개

> **레이어**: Logic (Core)
> **의존성**: T01, T02
> **예상 시간**: 15분 / 1 commit (5002 Sprint 2, 3/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 2

## 1. 작업 항목

### 1.1 기존 파일 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Core/BossEncounterContext.cs`

**변경**:
```csharp
public enum Mode { Daily, Weekly, ChapterBoss }   // ChapterBoss 추가

// 신규 필드 (ChapterBoss 한정)
public static ChapterData PendingChapter;
public static string PendingReturnScene;

public static void Clear()
{
    PendingBoss = null;
    PendingMode = null;
    PendingMythicPool = null;
    PendingChapter = null;          // 신규
    PendingReturnScene = null;       // 신규
}
```

**예상 라인 변동**: 40L → ~50L

### 1.2 기존 파일 수정 (T02 주석 해제)

**경로**: `LittleHero/Assets/_Project/Scripts/Core/BossEncounterLauncher.cs`

**변경**:
```csharp
public void Launch(BossEncounterRequest req)
{
    BossEncounterContext.Set(req.mode, req.boss, req.endgameMode, req.mythicPool);
    BossEncounterContext.PendingChapter = req.chapter;            // T03 활성
    BossEncounterContext.PendingReturnScene = req.returnSceneName; // T03 활성
    Time.timeScale = 1f;
    SceneManager.LoadScene("BossEncounter");
}
```

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` + 5초 대기 + errors=0
- enum `BossEncounterContext.Mode.ChapterBoss` 사용 가능

### 2.2 Editor Test
- N/A (T09 에서 enum 존재 reflection 검증)

### 2.3 ProjectValidator
- 정적 전역 필드 추가 — 외부 직접 R/W 검사는 T09 Validator 룰

### 2.4 회귀
- 기존 Daily/Weekly 흐름 — `Set` 호출 시 PendingChapter 는 null 로 초기화 (struct default)
- EndgameMenuPanel 의 직접 Set 호출 회귀 X (T08 변경)
- BossEncounterRunner 의 분기 — T04 변경 전까지 Mode.ChapterBoss 진입 X

## 3. commit 메시지

```
5002 Sprint 2 — Mode.ChapterBoss + PendingChapter/PendingReturnScene (3/10)

[변경]
- Scripts/Core/BossEncounterContext.cs — Mode enum 에 ChapterBoss / 신규 정적 필드 2 / Clear 갱신
- Scripts/Core/BossEncounterLauncher.cs — T02 주석 해제 (PendingChapter/PendingReturnScene 활성)

[검증]
- 컴파일 OK
- Daily/Weekly 흐름 회귀 X (EndgameMenuPanel 직접 Set 호출 그대로)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 2
- 디자인 §3.4 ChapterRunner 변경 흐름

## 5. 알려진 위험

- 정적 전역 필드 추가 — 외부 직접 R/W 가능성 (T09 Validator 룰로 차단)
