# 5022 Sprint PRD v2 — OOP Validator Split

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5022_oop_validator_split_prd.md` · Rebuttal: `5022_oop_validator_split_rebuttal.md` · Decisions: `5022_oop_validator_split_decisions.md`
> 결정 3건 반영 (D1=B, D2=A, D3=A). 다음 단계: `/prd-breakdown` (Phase 3)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1.1 | Issue 추출 + 폴더 구조 | 1 | 15분 |
| 1.2 | orchestrator 슬림화 + 6 간단 Rule (D1=B 통합) | 1 | 45분 |
| 2 | Rules 복잡 5개 분리 | 1 | 45분 |
| 3 | NUnit 회귀 + 결과 비교 | 1 | 30분 |
| **합계** | | **4** | **2h15m** |

---

## 2. Sprint 1.1 — Issue 추출 + 폴더 구조 (1 commit)

### 2.1 Issue/Severity 별도 파일

**파일**: `Editor/ProjectValidator/Issue.cs` (신규, ~15L)
- `public struct Issue { public Severity Sev; public string Category; public string Msg; ... }`
- `public enum Severity { Error, Warning }`

### 2.2 폴더 구조 신규

```
Editor/ProjectValidator/
├── Issue.cs                       (Sprint 1.1 신규)
└── Rules/                         (Sprint 1.2/2 채움)
```

### Sprint 1.1 commit
```
5022 Sprint 1.1 / T01 — Issue 추출 + 폴더 구조 (1/4)

[변경]
  + Editor/ProjectValidator/Issue.cs (~15L public struct + enum)
  + Editor/ProjectValidator/Rules/ (폴더 — Sprint 1.2/2 에서 채움)

[검증]
  - 컴파일 errors=0
  - ProjectValidator.cs 의 private Issue / Severity 가 public Issue/Severity 로 import 가능
```

---

## 3. Sprint 1.2 — orchestrator 슬림화 + 6 간단 Rule (D1=B 통합, 1 commit)

### 3.1 ProjectValidator.cs 슬림화

**파일**: `Editor/ProjectValidator.cs` (수정, 650L → ~70L)
- Run 메서드 + MenuItem 만 보존
- 모든 Validate* 메서드 → Rule 클래스로 이동
- Print 메서드 보존 (~12L)

### 3.2 Rules 간단 6개 분리

| Rule 클래스 | 통합 카테고리 | 라인 (~) |
|---|---|---|
| `BuildSettingsRule` | BuildSettings | 50 |
| `BalanceRule` | Balance | 40 |
| `ShopRule` | ShopPool | 50 |
| `ChapterRule` | Chapters | 55 |
| `CharacterRule` | Characters + StarterHeroPortraits (5020) + CharacterPool (5021) | 50 |
| `MemoryMetaRule` | MemoryPool + MetaUpgradePool | 55 |

각 파일 위치: `Editor/ProjectValidator/Rules/<RuleName>.cs`

### 3.3 Run() 갱신 (Sprint 1.2 시점)

```csharp
public static void Run() {
    var issues = new List<Issue>();
    Rules.BuildSettingsRule.Validate(issues);
    Rules.BalanceRule.Validate(issues);
    Rules.ShopRule.Validate(issues);
    Rules.ChapterRule.Validate(issues);
    Rules.CharacterRule.Validate(issues);
    Rules.MemoryMetaRule.Validate(issues);
    // Sprint 2 에서 5 추가 (Enemy/Scene/OOPPolicy/WeaponBoss/Performance)
    // 일시적으로 ProjectValidator.cs 내부에 5 룰의 stub 호출 보존 (검증 안전 — 6 분리 + 5 잔존 = 11 호출)
    Rules.EnemyRule_Stub(issues);  // Sprint 2 까지 임시
    ...
    Print(issues);
}
```

> **D1=B 핵심**: 6 룰 분리 시점에 5 잔존 룰은 ProjectValidator.cs 의 method body 임시 보존 → 항상 11 룰 작동 보장. Sprint 2 에서 분리 완료 후 stub 제거.

### Sprint 1.2 commit
```
5022 Sprint 1.2 / T02 — orchestrator + 6 간단 Rule 분리 (D1=B, 2/4)

[변경]
  ~ Editor/ProjectValidator.cs (650L → ~250L)
    - Run + Print 보존
    - 6 간단 Rule 호출 (Rules.X.Validate)
    - 5 잔존 룰 (Enemy/Scene/OOPPolicy/WeaponBoss/Performance) 임시 보존
  + Rules/BuildSettingsRule.cs (~50L)
  + Rules/BalanceRule.cs (~40L)
  + Rules/ShopRule.cs (~50L)
  + Rules/ChapterRule.cs (~55L)
  + Rules/CharacterRule.cs (~50L) — 5020 StarterHero + 5021 CharacterPool 통합
  + Rules/MemoryMetaRule.cs (~55L)

[검증 — D1=B]
  - 컴파일 errors=0
  - Validate Project 메뉴 동작 (11 룰 정상 — 6 분리 + 5 잔존)
  - errors=0, warnings 동일 (5021 baseline 173)
```

---

## 4. Sprint 2 — Rules 복잡 5개 분리 (1 commit)

### 4.1 분리 대상

| Rule 클래스 | 통합 카테고리 | 라인 (~) |
|---|---|---|
| `EnemyRule` | EnemyPool + EnemyVisibilityCheck + EnemyHueVariationHook | 80 |
| `SceneRule` | ActiveScene + SceneManagerDirectAccess | 50 |
| `OOPPolicyRule` | OOP §15 (200줄+ / public 필드 / Update 멀티플) | 70 |
| `WeaponBossRule` | WeaponSystem + BossSystem + BossEncounterContextAccess | 85 |
| `PerformanceRule` | LitMotionMigration + GCAllocPatterns | 85 |

### 4.2 ProjectValidator.cs 최종 슬림화

```csharp
public static void Run() {
    var issues = new List<Issue>();
    Rules.BuildSettingsRule.Validate(issues);
    Rules.BalanceRule.Validate(issues);
    Rules.ShopRule.Validate(issues);
    Rules.ChapterRule.Validate(issues);
    Rules.CharacterRule.Validate(issues);
    Rules.MemoryMetaRule.Validate(issues);
    Rules.EnemyRule.Validate(issues);          // Sprint 2 신규
    Rules.SceneRule.Validate(issues);          // Sprint 2 신규
    Rules.OOPPolicyRule.Validate(issues);      // Sprint 2 신규
    Rules.WeaponBossRule.Validate(issues);     // Sprint 2 신규
    Rules.PerformanceRule.Validate(issues);    // Sprint 2 신규
    Print(issues);
}
```

ProjectValidator.cs 최종 ~80L (Run + Print + MenuItem).

### Sprint 2 commit
```
5022 Sprint 2 / T03 — Rules 복잡 5개 분리 (3/4)

[변경]
  + Rules/EnemyRule.cs (~80L)
  + Rules/SceneRule.cs (~50L)
  + Rules/OOPPolicyRule.cs (~70L)
  + Rules/WeaponBossRule.cs (~85L)
  + Rules/PerformanceRule.cs (~85L)
  ~ ProjectValidator.cs (~250L → ~80L) — 5 stub 제거, 11 Rule 모두 분리 완료

[검증]
  - 컴파일 errors=0
  - Validate Project 결과 분리 전과 동일 (errors=0, warnings 173)
```

---

## 5. Sprint 3 — NUnit 회귀 + 결과 비교 (1 commit, D3=A NUnit 신규 X)

### 5.1 분리 전후 결과 비교

```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c3
grep "\[Validator\] 완료" /tmp/lh_verify_c3.log
# 기대: errors=0, warnings=173 (5021 baseline 동일)
```

### 5.2 NUnit 회귀

```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c2
# 기대: 120/120 PASS, failed=0 (5021 baseline 회귀 0)
```

### 5.3 changelog + CLAUDE.md

### Sprint 3 commit
```
5022 Sprint 3 / T04 — NUnit 회귀 + 결과 비교 (4/4) — 5022 종결

[검증]
  - C2 NUnit 120/120 PASS (회귀 0)
  - C3 Validator errors=0, warnings=173 (5021 baseline 동일)
  - §15.6 100줄 룰 자기위반 해소 (모든 Rule 100L 미만, ProjectValidator ~80L)

[변경]
  ~ docs/little_hero/changelog_v1.md (5022 항목)
  ~ CLAUDE.md §11 마일스톤 + §14 직전 5건
```

---

## 6. 의존성 그래프

```
Sprint 1.1 (Issue + 폴더) → Sprint 1.2 (orchestrator + 6 간단) → Sprint 2 (복잡 5) → Sprint 3 (검증)
```

직렬. Sprint 1.1 의 Issue struct 가 모든 Rule 의존.

---

## 7. 완료 정의

- [ ] Issue/Severity public 노출 (Editor/ProjectValidator/Issue.cs)
- [ ] 11 Rule 클래스 모두 100L 미만
- [ ] ProjectValidator.cs ≤ 100L (~80L)
- [ ] C3 결과 분리 전후 동일 (errors=0, warnings=173)
- [ ] C2 NUnit 120/120 회귀 0
- [ ] `/prd-evaluate 5022` PASS

---

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Sprint 1.2 의 5 stub 호출이 Sprint 2 분리 후 제거 누락 | Sprint 2 commit 후 grep "_Stub" 으로 0 confirm |
| Issue struct 의 가시성 변경 → 컴파일 에러 | private → public 명시, namespace 통일 |
| Rule 클래스 100L 초과 (PerformanceRule 85L 가 80L 권장 약간 초과) | D2=A 결정 — 100L 룰 충족, 80L 권장 약간 초과 일시 허용 |
| MenuItem 위치 변경 시 사용자 muscle memory 깨짐 | `Tools/LittleHero/Validate Project` 메뉴 path 보존 |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | v2 초안 (Rebuttal 단계). 자동 수용 0건. D1/D2/D3 mediator 대기 |
| 2026-05-06 | **v2 최종 (Mediator)** — D1=B (Sprint 1+2 통합) + D2=A (11 Rule 유지) + D3=A (NUnit 신규 X) 반영. 4 Sprint → 4 commit / 2h15m (-15분 단축, 위험 영역 해소) |
