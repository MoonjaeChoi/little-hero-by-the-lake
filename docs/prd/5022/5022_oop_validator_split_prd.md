# 5022 Sprint PRD — OOP Validator Split

> 디자인: `docs/prd/5022_oop_validator_split_prd.md`
> Sprint: 4 / commit: 4 / 시간: ~2h30m
> 의존성: 없음 (독립). 5020 룰 호환.

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Issue 추출 + 폴더 구조 + orchestrator 슬림화 | 1 | 30분 |
| 2 | Rules 분리 (간단 6개) | 1 | 45분 |
| 3 | Rules 분리 (복잡 5개) | 1 | 45분 |
| 4 | NUnit 회귀 + 결과 비교 | 1 | 30분 |
| **합계** | | **4** | **2h30m** |

---

## 2. Sprint 1 — Issue 추출 + orchestrator 슬림화

### 2.1 Issue/Severity 별도 파일

**파일**: `Editor/ProjectValidator/Issue.cs` (신규, ~15L)
- `public struct Issue { public Severity Sev; public string Category; public string Msg; ... }`
- `public enum Severity { Error, Warning }`

### 2.2 ProjectValidator.cs 슬림화

**파일**: `Editor/ProjectValidator.cs` (수정, 650L → ~80L)
- Run 메서드만 보존, 모든 Validate* 메서드 제거 (다음 Sprint 에서 Rule 클래스로 이동)
- Print 메서드 보존
- 임시로 모든 Rules.X.Validate(issues) 호출 stub (Sprint 2/3 에서 채움)

**임시 동작**: 분리 직후 errors/warnings 모두 0 (모든 룰 비어있음). 실제 룰은 Sprint 2/3 에서 등록.

### Sprint 1 commit
```
5022 Sprint 1 / T01 — Issue 추출 + orchestrator 슬림화 (1/4)

[변경]
  + Editor/ProjectValidator/Issue.cs (15L, public Issue struct + Severity enum)
  ~ Editor/ProjectValidator.cs (650L → 80L) — Run + Print 만, 모든 Validate* 메서드 임시 제거
  ⚠️ 분리 직후 검증 결과 errors=0/warnings=0 (Sprint 2/3 에서 룰 복원)
```

---

## 3. Sprint 2 — Rules 간단 6개 분리

### 3.1 분리 대상

| Rule 클래스 | 통합 카테고리 | 라인 (~) |
|---|---|---|
| `BuildSettingsRule` | BuildSettings | 50 |
| `BalanceRule` | Balance | 40 |
| `ShopRule` | ShopPool | 50 |
| `ChapterRule` | Chapters | 55 |
| `CharacterRule` | Characters + StarterHeroPortraits (5020) | 40 |
| `MemoryMetaRule` | MemoryPool + MetaUpgradePool | 55 |

각 파일 위치: `Editor/ProjectValidator/Rules/<RuleName>.cs`

### 3.2 ProjectValidator.cs Run 갱신

```csharp
public static void Run() {
    var issues = new List<Issue>();
    Rules.BuildSettingsRule.Validate(issues);
    Rules.BalanceRule.Validate(issues);
    Rules.ShopRule.Validate(issues);
    Rules.ChapterRule.Validate(issues);
    Rules.CharacterRule.Validate(issues);
    Rules.MemoryMetaRule.Validate(issues);
    // Sprint 3 에서 5 추가
    Print(issues);
}
```

### Sprint 2 commit
```
5022 Sprint 2 / T02 — Rules 간단 6개 분리 (2/4)

[변경]
  + Rules/BuildSettingsRule.cs (50L)
  + Rules/BalanceRule.cs (40L)
  + Rules/ShopRule.cs (50L)
  + Rules/ChapterRule.cs (55L)
  + Rules/CharacterRule.cs (40L) — 5020 StarterHeroPortraits 포함
  + Rules/MemoryMetaRule.cs (55L)
  ~ ProjectValidator.cs Run 6 룰 호출

[검증]
  - 컴파일 errors=0
  - Validate Project 메뉴 동작 (6 카테고리 정상 검증)
```

---

## 4. Sprint 3 — Rules 복잡 5개 분리

### 4.1 분리 대상

| Rule 클래스 | 통합 카테고리 | 라인 (~) |
|---|---|---|
| `EnemyRule` | EnemyPool + EnemyVisibilityCheck + EnemyHueVariationHook | 80 |
| `SceneRule` | ActiveScene + SceneManagerDirectAccess | 50 |
| `OOPPolicyRule` | OOP §15 (200줄+ / public 필드 / Update 멀티플) | 70 |
| `WeaponBossRule` | WeaponSystem + BossSystem + BossEncounterContextAccess | 85 |
| `PerformanceRule` | LitMotionMigration + GCAllocPatterns | 85 |

### 4.2 ProjectValidator.cs Run 최종

```csharp
Rules.BuildSettingsRule.Validate(issues);
Rules.BalanceRule.Validate(issues);
Rules.ShopRule.Validate(issues);
Rules.ChapterRule.Validate(issues);
Rules.CharacterRule.Validate(issues);
Rules.MemoryMetaRule.Validate(issues);
Rules.EnemyRule.Validate(issues);          // 신규
Rules.SceneRule.Validate(issues);          // 신규
Rules.OOPPolicyRule.Validate(issues);      // 신규
Rules.WeaponBossRule.Validate(issues);     // 신규
Rules.PerformanceRule.Validate(issues);    // 신규
```

### Sprint 3 commit
```
5022 Sprint 3 / T03 — Rules 복잡 5개 분리 (3/4)

[변경]
  + Rules/EnemyRule.cs (80L)
  + Rules/SceneRule.cs (50L)
  + Rules/OOPPolicyRule.cs (70L)
  + Rules/WeaponBossRule.cs (85L)
  + Rules/PerformanceRule.cs (85L)
  ~ ProjectValidator.cs Run 모든 11 룰 호출 완료

[검증]
  - 컴파일 errors=0
  - Validate Project 결과 분리 전과 동일 (warnings 172, errors 0)
```

---

## 5. Sprint 4 — NUnit 회귀 + 결과 비교

### 5.1 분리 전후 결과 비교 (Bash)

```bash
# 분리 전 baseline (5020 시점)
echo "before: errors=0, warnings=172"

# 5022 Sprint 3 후 batch 실행
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c3
grep "\[Validator\] 완료" /tmp/lh_verify_c3.log
# 기대: errors=0, warnings=172 (변동 0)
```

### 5.2 NUnit 회귀

기존 117 NUnit 회귀 0 확인:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c2
# 기대: 117/117 PASS, failed=0
```

### 5.3 changelog + CLAUDE.md

### Sprint 4 commit
```
5022 Sprint 4 / T04 — NUnit 회귀 + 결과 비교 (4/4) — 5022 종결

[검증]
  - C2 NUnit 117/117 PASS (회귀 0)
  - C3 Validator errors=0, warnings=172 (분리 전 동일)
  - §15.6 100줄 룰 자기위반 해소 (모든 Rule 100L 미만)

[변경]
  ~ docs/little_hero/changelog_v1.md
  ~ CLAUDE.md §11 마일스톤
```

---

## 6. 의존성 그래프

```
Sprint 1 (Issue + slim) → Sprint 2 (간단 6) → Sprint 3 (복잡 5) → Sprint 4 (검증)
```

직렬. Sprint 1 의 Issue struct 가 모든 Rule 의존.

---

## 7. 완료 정의

- [ ] Issue/Severity public 노출
- [ ] 11 Rule 클래스 모두 100L 미만
- [ ] ProjectValidator.cs ≤ 100L
- [ ] C3 결과 분리 전후 동일 (errors=0, warnings=172)
- [ ] C2 NUnit 117/117 회귀 0
- [ ] `/prd-evaluate 5022` PASS

---

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 룰 통합 시 책임 분리 깨짐 (예 EnemyRule 80L) | 80L 이내 — 100L 룰 정합. 더 비대 시 추가 분리 (EnemyVisualRule + EnemyPerfRule) |
| Issue struct 의 Severity enum 가시성 충돌 | namespace LittleHero.EditorTools.Issue 명확 분리 |
| MenuItem 위치 변경 시 사용자 muscle memory 깨짐 | `Tools/LittleHero/Validate Project` 메뉴 path 보존 |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 Sprint PRD — 4 Sprint / 4 commit / 2h30m |
