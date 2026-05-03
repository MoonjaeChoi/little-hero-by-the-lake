# T01: BossEncounterRequest DTO 신규

> **레이어**: Logic (Core)
> **의존성**: 없음
> **예상 시간**: 15분 / 1 commit (5002 Sprint 1.1, 1/10)
> **PRD 출처**: `5002_*_v2_prd.md` Sprint 1.1

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Core/BossEncounterRequest.cs`

**클래스 명세**:
```csharp
namespace LittleHero
{
    /// <summary>
    /// 5002 — BossEncounterLauncher 진입 DTO (§15.7 DI).
    /// 모드별 필드 nullable — Daily/Weekly: endgameMode/mythicPool, ChapterBoss: chapter.
    /// </summary>
    public struct BossEncounterRequest
    {
        public BossEncounterContext.Mode mode;
        public EnemyData boss;
        public EndgameModeData endgameMode;       // Daily/Weekly 만 (null 가능)
        public ChapterData chapter;                // ChapterBoss 만 (null 가능)
        public MythicDefinition[] mythicPool;      // Weekly 만 (null 가능)
        public string returnSceneName;             // 기본 "Attic"
    }
}
```

**예상 라인**: ~20 (100줄 이내 ✅)

## 2. 검증 기준

### 2.1 컴파일
- `BossEncounterContext.Mode` 가 현재 `Daily`, `Weekly` 만 가짐 — `ChapterBoss` 는 T03 에서 추가
- 본 commit 에서는 컴파일 OK 만 검증 (struct 자체는 enum 값과 무관)

### 2.2 Editor Test
- N/A (DTO struct 만)

### 2.3 ProjectValidator
- 신규 warnings 0
- public 필드 검사 — DTO struct 는 §15.11 안티 패턴 예외 (data carrier 목적)

### 2.4 회귀
- 기존 흐름 영향 X (struct 신규만)

## 3. commit 메시지

```
5002 Sprint 1.1 — BossEncounterRequest DTO 신규 (1/10)

[변경]
- Scripts/Core/BossEncounterRequest.cs 신규 (~20L, struct 6 필드)

[검증]
- 컴파일 OK
- 기존 회귀 X
- §15.11 예외 — DTO struct 는 data carrier 목적 (public 필드 허용)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §Sprint 1.1
- 디자인 §3.2 BossEncounterRequest DTO

## 5. 알려진 위험

- 없음 (정적 데이터 구조)
