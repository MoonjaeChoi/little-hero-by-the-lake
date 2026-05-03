# T04: BackgroundApplier 정적 헬퍼 + BossEncounterRunner ChapterBoss 분기

> **레이어**: Logic (Core + Waves)
> **의존성**: T03 (BossEncounterContext.Mode.ChapterBoss + PendingChapter)
> **예상 시간**: 35분 / 1 commit (5002 Sprint 3.1, 4/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 3.1, D1 Rebuttal 자동 수용

## 1. 작업 항목

### 1.1 신규 파일 (D1 Rebuttal — §15.1 SRP + DRY)

**경로**: `LittleHero/Assets/_Project/Scripts/Core/BackgroundApplier.cs`

```csharp
using UnityEngine;

namespace LittleHero
{
    /// <summary>
    /// 5002 — 챕터 BG 적용 정적 헬퍼 (D1 Rebuttal — §15.1 SRP + DRY).
    /// ChapterRunner / BossEncounterRunner 공유. Tiled BG 패턴 단일 진실원.
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

**예상 라인**: ~25 (100줄 이내 ✅)

### 1.2 기존 파일 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Gameplay/Waves/BossEncounterRunner.cs`

**변경 내용** (118L → ~145L):
1. `[SerializeField] private SpriteRenderer _backgroundRenderer;` 신규 필드
2. `Start()` — `BossEncounterContext.PendingType == Mode.ChapterBoss` 시 `BackgroundApplier.Apply(_backgroundRenderer, BossEncounterContext.PendingChapter)` 호출
3. `OnBossDefeated()` — switch 에 `case Mode.ChapterBoss: BossEncounterService.CompleteChapterBoss(...) break;` 추가 (T06 메서드 stub 필요 — T04 commit 시점에 BossEncounterService 에 임시 stub 추가 필요)
4. `LoadAtticAfterDelay` → `LoadSceneAfterDelay(string scene)` 변경 + `BossEncounterContext.PendingReturnScene ?? _atticSceneName` 사용

**T06 의존 회피**: T04 commit 시 BossEncounterService 에 `CompleteChapterBoss` 메서드 stub 만 미리 추가 (Debug.Log 만, 실제 보상은 T06):
```csharp
public static void CompleteChapterBoss(EnemyData boss, ChapterData chapter)
{
    Debug.Log($"[BossEncounterService] CompleteChapterBoss stub: {boss?.displayName}");
    // T06 에서 BossLootGrantService.Apply / RaiseChapterCleared / pendingChapterClearedId set
}
```

또는 T04 와 T06 commit 순서 조정 — 본 breakdown 은 T04 → T06 순서 가정 (stub 사용).

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` + **10초 대기** (다중 파일 변경)
- `mcp__UnityMCP__read_console` errors=0

### 2.2 Editor Test
- N/A (T09)

### 2.3 ProjectValidator
- BackgroundApplier 정적 헬퍼 — §15.1 SRP 통과
- BossEncounterRunner 라인 수 145 — 200L 경계 내 ✅

### 2.4 회귀
- Daily/Weekly 흐름 — Mode 분기 switch 에서 기존 case 그대로
- BG 미설정 시 (Daily/Weekly) `_backgroundRenderer == null` 또는 `chapter == null` → no-op (안전)
- ChapterBoss 분기는 T07 ChapterRunner 까지 활성화 안 됨

## 3. commit 메시지

```
5002 Sprint 3.1 — BossEncounterRunner ChapterBoss 분기 + BackgroundApplier 정적 헬퍼 (4/10)

[변경]
- Scripts/Core/BackgroundApplier.cs 신규 (D1 Rebuttal — §15.1 SRP + DRY)
- Scripts/Gameplay/Waves/BossEncounterRunner.cs — Mode.ChapterBoss 분기 / BackgroundApplier.Apply 위임 / LoadSceneAfterDelay(returnScene)
- _backgroundRenderer SerializeField 신규
- Scripts/Core/BossEncounterService.cs — CompleteChapterBoss stub (T06 에서 본체 구현)

[검증]
- 컴파일 OK (errors=0)
- Daily/Weekly 흐름 회귀 X
- ChapterBoss 분기는 T07 ChapterRunner 활성 후 검증

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 3.1, §3.0 BackgroundApplier
- 디자인 §3.5/3.6 BossEncounterRunner Mode.ChapterBoss 분기
- Rebuttal D1 — §15.1 SRP + DRY

## 5. 알려진 위험

- **MCP SIGSEGV 위험 (다중 파일 변경)** — refresh + 10초 대기 + 단일 commit
- T06 stub 누락 시 컴파일 에러. 본 commit 에 stub 추가 필수
- BossEncounterRunner 라인 수 — 145L 정확 측정 필요. 150L+ 시 ApplyChapterBackground 메서드 추가 분리 검토
