# T02: IBossEncounterLauncher + 정적 BossEncounterLauncher

> **레이어**: Logic (Core)
> **의존성**: T01 (BossEncounterRequest)
> **예상 시간**: 15분 / 1 commit (5002 Sprint 1.2, 2/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 1.2

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Core/IBossEncounterLauncher.cs`

```csharp
namespace LittleHero
{
    /// <summary>
    /// 5002 — 보스 씬 진입 인터페이스 (§15.1 DIP, §15.7 DI).
    /// 기본 구현 = BossEncounterLauncher.Default. 테스트 시 mock 주입.
    /// </summary>
    public interface IBossEncounterLauncher
    {
        void Launch(BossEncounterRequest request);
    }
}
```

**예상 라인**: ~10

### 1.2 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Core/BossEncounterLauncher.cs`

```csharp
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LittleHero
{
    /// <summary>
    /// 5002 — 정적 진입점 (§15.8 Service Locator). Default property 로 mock 주입 가능.
    /// 내부적으로 BossEncounterContext 를 채우고 LoadScene("BossEncounter") 호출.
    /// </summary>
    public static class BossEncounterLauncher
    {
        public static IBossEncounterLauncher Default { get; set; } = new DefaultLauncher();

        public static void Launch(BossEncounterRequest req) => Default.Launch(req);

        private class DefaultLauncher : IBossEncounterLauncher
        {
            public void Launch(BossEncounterRequest req)
            {
                BossEncounterContext.Set(req.mode, req.boss, req.endgameMode, req.mythicPool);
                // T03 에서 추가될 필드 — 본 commit 시점에는 주석 처리
                // BossEncounterContext.PendingChapter = req.chapter;
                // BossEncounterContext.PendingReturnScene = req.returnSceneName;
                Time.timeScale = 1f;
                SceneManager.LoadScene("BossEncounter");
            }
        }
    }
}
```

**예상 라인**: ~30

## 2. 검증 기준

### 2.1 컴파일
- T01 BossEncounterRequest 와 함께 컴파일
- T03 미적용 상태 → `PendingChapter`/`PendingReturnScene` 주석 유지
- `mcp__UnityMCP__refresh_unity` + 5초 대기 + `read_console` errors=0

### 2.2 Editor Test
- N/A (T09 에서 일괄 검증)

### 2.3 ProjectValidator
- 신규 warnings 0

### 2.4 회귀
- 기존 EndgameMenuPanel `BossEncounterContext.Set` 직접 호출은 그대로 유지 (T08 에서 변경)
- Launcher 호출 0건 시 기존 흐름 회귀 X

## 3. commit 메시지

```
5002 Sprint 1.2 — IBossEncounterLauncher + 정적 Default Launcher (2/10)

[변경]
- Scripts/Core/IBossEncounterLauncher.cs 신규 (~10L, interface 1 메서드)
- Scripts/Core/BossEncounterLauncher.cs 신규 (~30L, Default property + DefaultLauncher 내부 class)
- T03 PendingChapter/PendingReturnScene 라인 주석 (T03 활성화)

[검증]
- 컴파일 OK
- BossEncounterLauncher.Launch(req) 정적 호출 가능
- 기존 흐름 회귀 X (Launcher 호출 0건)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 1.2
- 디자인 §3.3 IBossEncounterLauncher

## 5. 알려진 위험

- T03 미적용 시 `PendingChapter` 등 주석 누락 → 컴파일 에러. 주석 처리 필수.
