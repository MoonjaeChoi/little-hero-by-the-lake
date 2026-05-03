# T08: EndgameMenuPanel Launcher 위임

> **레이어**: UI
> **의존성**: T02 (BossEncounterLauncher)
> **예상 시간**: 20분 / 1 commit (5002 Sprint 6, 8/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 6

## 1. 작업 항목

### 1.1 기존 파일 수정 — EndgameMenuPanel.cs

**경로**: `LittleHero/Assets/_Project/Scripts/UI/EndgameMenuPanel.cs`

**변경**: `OnDailyClicked` / `OnWeeklyClicked` 에서 정적 `BossEncounterContext.Set` 직접 호출 제거 → `BossEncounterLauncher.Launch` 위임

```csharp
private void OnDailyClicked()
{
    if (!EndgameResetService.IsDailyAvailable() || _dailyBoss == null) return;
    var boss = BossEncounterService.ResolveDailyBoss(_dailyBoss);
    if (boss == null) { Debug.LogWarning("[EndgameMenu] 일일 보스 풀 비어있음"); return; }
    Debug.Log($"[EndgameMenu] 일일 보스: {boss.displayName}");

    if (_instantCompleteForTesting)
    {
        bool ok = BossEncounterService.CompleteDailyBoss(_dailyBoss);
        Debug.Log($"[EndgameMenu] 일일 보스 완료 처리: {ok}");
        Refresh();
        return;
    }

    BossEncounterLauncher.Launch(new BossEncounterRequest
    {
        mode = BossEncounterContext.Mode.Daily,
        boss = boss,
        endgameMode = _dailyBoss,
        returnSceneName = "Attic",
    });
}

private void OnWeeklyClicked()
{
    if (!EndgameResetService.IsWeeklyAvailable() || _weeklyBoss == null) return;
    var boss = BossEncounterService.ResolveWeeklyBoss(_weeklyBoss);
    if (boss == null) { Debug.LogWarning("[EndgameMenu] 주간 보스 풀 비어있음"); return; }
    Debug.Log($"[EndgameMenu] 주간 보스: {boss.displayName}");

    if (_instantCompleteForTesting)
    {
        var result = BossEncounterService.CompleteWeeklyBoss(_weeklyBoss, _mythicPool);
        Debug.Log($"[EndgameMenu] 주간 보스 완료={result.success} 신화발현={(result.mythicGranted != null ? result.mythicGranted.instanceId : "없음")}");
        if (result.mythicGranted != null)
        {
            var overlay = FindAnyObjectByType<MythicEffectOverlay>();
            var def = ResolveMythicDefinition(result.mythicGranted.mythicId);
            if (overlay != null && def != null) overlay.Play(def);
        }
        Refresh();
        return;
    }

    BossEncounterLauncher.Launch(new BossEncounterRequest
    {
        mode = BossEncounterContext.Mode.Weekly,
        boss = boss,
        endgameMode = _weeklyBoss,
        mythicPool = _mythicPool,
        returnSceneName = "Attic",
    });
}
```

라인 수: 174L → ~150L (정적 Set 직접 호출 + LoadScene 직접 호출 라인 정리)

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` + 5초 대기 + errors=0
- T09 ProjectValidator 룰 — `BossEncounterContext.Set` 외부 직접 호출 EndgameMenuPanel 에서 0건 (Launcher 내부만)

### 2.2 Editor Test
- T09 에서 reflection 으로 EndgameMenuPanel 이 Launcher.Launch 호출 검증 (선택)
- Play 검증 시나리오 S3, S4

### 2.3 ProjectValidator
- EndgameMenuPanel 라인 ~150L — 200L 경계 내 ✅

### 2.4 회귀 (Play 검증 — 시나리오 S3, S4)
- Daily 클릭 → BossEncounter 씬 전환 + 보스 처치 + Starlight 보상 + 일일 마크
- Weekly 클릭 → BossEncounter 씬 전환 + 보스 처치 + 신화 발현 + Attic Mythic Overlay
- `_instantCompleteForTesting=true` 모드 회귀 X (테스트 흐름 그대로)

## 3. commit 메시지

```
5002 Sprint 6 — EndgameMenuPanel Launcher 위임 (8/10)

[변경]
- Scripts/UI/EndgameMenuPanel.cs — OnDailyClicked/OnWeeklyClicked 가 BossEncounterLauncher.Launch 호출
- 정적 BossEncounterContext.Set 직접 호출 제거 (T09 Validator 룰 통과 준비)

[검증]
- Daily/Weekly 회귀 X
- _instantCompleteForTesting 흐름 회귀 X
- 174L → ~150L (DI 진입점 정리)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 6
- 디자인 §3.3 IBossEncounterLauncher

## 5. 알려진 위험

- `_instantCompleteForTesting` 분기 — 정적 Service 직접 호출 유지 (테스트 한정 — Validator 룰에서 EndgameMenuPanel 경로 화이트리스트 또는 instant 분기 인지)
- T09 Validator 룰 작성 시 `_instantCompleteForTesting` 영역 인지 필요
