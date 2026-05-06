# 5022 — OOP Validator Split

> **목적**: ProjectValidator.cs 650L → 책임별 분리. §15.6 100줄 룰 자기위반 해소.
> **scope**: 코드 리팩토링만 — 동작 변경 0 (검증 룰 결과 동일).

---

## 1. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5020 evaluation 시 ProjectValidator.cs 650L OOP warning 1번 등재 — 룰 자기위반 모순 발견 → 5022 분리 |

---

## 2. 현재 시스템 분석

### 2.1 ProjectValidator.cs 구조 (650L)

**메서드 분포** (`grep -E "private static void Validate"`):
| 메서드 | 라인 (~) | 카테고리 |
|---|---|---|
| Run (orchestrator) | 25 | Core |
| ValidateBuildSettings | 30 | Build |
| ValidateBalance | 25 | Balance |
| ValidateShopPool | 35 | Shop |
| ValidateChapters | 40 | Chapter |
| ValidateCharacters | 18 | Character |
| ValidateMemoryPool | 25 | Memory |
| ValidateMetaUpgradePool | 25 | Meta |
| ValidateEnemyPool | 30 | Enemy |
| ValidateActiveScene | 35 | Scene |
| ValidateOOPPolicy | 50 | OOP §15 |
| ValidateWeaponSystem | 30 | Weapon |
| ValidateBossSystem | 30 | Boss |
| ValidateBossEncounterContextAccess | 40 | Boss/§15.7 |
| ValidateSceneManagerDirectAccess | 35 | Scene/§15.7 |
| ValidateEnemyVisibilityCheck | 25 | Enemy/Perf |
| ValidateEnemyHueVariationHook | 25 | Enemy/Visual |
| ValidateLitMotionMigration | 50 | LitMotion |
| ValidateGCAllocPatterns | 30 | GC |
| ValidateStarterHeroPortraits | 20 | 5020 |
| Print (orchestrator output) | 12 | Core |

**위반**:
- §15.6 100줄 룰: ProjectValidator.cs 650L (룰 자체는 200줄+ 룰만 검사하지만 100줄+ 권장)
- §15.1 SRP: 검증 카테고리 다수 (Build/Balance/Shop/Chapter/Character/Memory/Meta/Enemy/Scene/OOP/Weapon/Boss/LitMotion/GC/...)

### 2.2 영향 범위

- **출시 후 추가 룰 작성 시**: 신규 시스템마다 ProjectValidator.cs 가 점점 비대해짐 (5020 추가로 +20L)
- **개발자 인지 부담**: 단일 파일 650L 안에서 룰 위치 찾기 어려움
- **CLAUDE.md §15.12 권장**: "Editor/ProjectValidator.cs 룰 추가 권장" — 분리되면 신규 룰 위치 명확

---

## 3. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 카테고리별 partial class | C# partial 활용, 단일 ProjectValidator namespace | partial 은 IDE 탐색 유리하지만 컴파일 단위 동일 — §15.6 효과 작음 |
| B (★권장) | 카테고리별 별도 정적 클래스 + Run orchestrator 호출 | 명확 분리, 신규 룰 추가 용이, §15.6 정합 | 파일 수 증가 (+8) |
| C | partial + 인터페이스 IValidationRule | OOP 모범, 자동 등록 가능 | 과도한 추상화 (현재 룰 ~20개, 동적 등록 불필요) |

**권장 옵션 B** — Strategy 보다 단순한 정적 분리. CLAUDE.md §15.8 "Service Locator (정적)" 답습.

---

## 4. 권장 설계

### 4.1 분리 구조

```
Editor/ProjectValidator/
├── ProjectValidator.cs              (Run + Issue/Severity + Print, ~80L)
├── Rules/
│   ├── BuildSettingsRule.cs         (~50L)
│   ├── BalanceRule.cs               (~40L)
│   ├── ShopRule.cs                  (~50L)
│   ├── ChapterRule.cs               (~55L)
│   ├── CharacterRule.cs             (~30L)  — 5020 StarterHeroPortraits 포함
│   ├── MemoryMetaRule.cs            (~50L) — Memory + MetaUpgrade 통합
│   ├── EnemyRule.cs                 (~80L) — Enemy + Visibility + HueVariation 통합
│   ├── SceneRule.cs                 (~50L) — ActiveScene + SceneManagerDirectAccess
│   ├── OOPPolicyRule.cs             (~70L)
│   ├── WeaponBossRule.cs            (~85L) — Weapon + Boss + BossEncounterContext 통합
│   ├── PerformanceRule.cs           (~85L) — LitMotion + GCAlloc 통합
```

각 Rule 클래스:
```csharp
namespace LittleHero.EditorTools.Rules
{
    public static class CharacterRule
    {
        public static void Validate(List<Issue> issues)
        {
            ValidateCharacters(issues);
            ValidateStarterHeroPortraits(issues);
        }
        private static void ValidateCharacters(List<Issue> issues) { ... }
        private static void ValidateStarterHeroPortraits(List<Issue> issues) { ... }
    }
}
```

ProjectValidator.Run:
```csharp
public static void Run() {
    var issues = new List<Issue>();
    Rules.BuildSettingsRule.Validate(issues);
    Rules.BalanceRule.Validate(issues);
    Rules.ShopRule.Validate(issues);
    Rules.ChapterRule.Validate(issues);
    Rules.CharacterRule.Validate(issues);
    Rules.MemoryMetaRule.Validate(issues);
    Rules.EnemyRule.Validate(issues);
    Rules.SceneRule.Validate(issues);
    Rules.OOPPolicyRule.Validate(issues);
    Rules.WeaponBossRule.Validate(issues);
    Rules.PerformanceRule.Validate(issues);
    Print(issues);
}
```

### 4.2 Issue/Severity 공유

`Editor/ProjectValidator/Issue.cs` (신규) — `Issue` struct + `Severity` enum public 노출:
```csharp
public enum Severity { Error, Warning }
public struct Issue
{
    public Severity Sev; public string Category; public string Msg;
    public Issue(Severity s, string c, string m) { Sev = s; Category = c; Msg = m; }
}
```

각 Rule 클래스가 `using LittleHero.EditorTools` → Issue/Severity 사용.

### 4.3 CLAUDE.md §15 정합

| 정책 | 검증 |
|---|---|
| §15.1 SRP | ✅ 각 Rule 클래스 단일 카테고리 |
| §15.6 100줄 룰 | ✅ 모든 Rule 100L 미만 (orchestrator 80L) |
| §15.7 DI | ✅ 정적 함수 시그니처 통일 (List<Issue> 전달) |
| §15.8 Service Locator | ✅ 정적 클래스 패턴 답습 |
| §15.12 ProjectValidator | ✅ 신규 룰 위치 명확 (Rules/ 하위) |

---

## 5. 마이그레이션 단계

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Issue/Severity 추출 + 신규 폴더 구조 + ProjectValidator orchestrator 슬림화 | 1 | 30분 |
| 2 | Rules 분리 — Build/Balance/Shop/Chapter/Character/MemoryMeta (간단 카테고리 6개) | 1 | 45분 |
| 3 | Rules 분리 — Enemy/Scene/OOPPolicy/WeaponBoss/Performance (복잡 카테고리 5개) | 1 | 45분 |
| 4 | NUnit 회귀 검증 + Validator 결과 비교 (분리 전후 errors/warnings 동일) | 1 | 30분 |
| **합계** | | **4** | **2h30m** |

---

## 6. 알려진 위험

| 위험 | 완화 |
|---|---|
| 분리 후 누락된 룰 (Run 등록 빠뜨림) | NUnit Run 호출 카운트 검증 (기존 11 → 신규 11) |
| Issue/Severity 가시성 변경 → 컴파일 에러 | private → public 명시, namespace 통일 |
| 분리된 Rule 클래스가 100L 초과 | EnemyRule/WeaponBossRule/PerformanceRule 통합으로 80L 이내 (사전 분배 표) |
| Editor 메뉴 동작 변경 (Tools/LittleHero/Validate Project) | MenuItem 위치 보존 (ProjectValidator.Run) |

---

## 7. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초기 — 5020 evaluation 의 OOP warning 1 등재 → 5022 분리 |
