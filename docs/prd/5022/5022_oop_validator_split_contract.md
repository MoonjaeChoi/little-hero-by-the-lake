# Sprint Contract: 5022 OOP Validator Split

> **PGE Planner — Phase 4 Contract**
> PRD: `5022_oop_validator_split_v2.md`
> Breakdown: `5022_oop_validator_split_breakdown_00.md`
> 생성일: 2026-05-06

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화

**신규 / 수정 파일** (12 신규 + 1 수정):
- (신규) `Editor/ProjectValidator/Issue.cs` ≤ 20L
- (신규) `Editor/ProjectValidator/Rules/BuildSettingsRule.cs` ≤ 60L
- (신규) `Editor/ProjectValidator/Rules/BalanceRule.cs` ≤ 50L
- (신규) `Editor/ProjectValidator/Rules/ShopRule.cs` ≤ 60L
- (신규) `Editor/ProjectValidator/Rules/ChapterRule.cs` ≤ 65L
- (신규) `Editor/ProjectValidator/Rules/CharacterRule.cs` ≤ 60L
- (신규) `Editor/ProjectValidator/Rules/MemoryMetaRule.cs` ≤ 65L
- (신규) `Editor/ProjectValidator/Rules/EnemyRule.cs` ≤ 90L
- (신규) `Editor/ProjectValidator/Rules/SceneRule.cs` ≤ 60L
- (신규) `Editor/ProjectValidator/Rules/OOPPolicyRule.cs` ≤ 80L
- (신규) `Editor/ProjectValidator/Rules/WeaponBossRule.cs` ≤ 95L
- (신규) `Editor/ProjectValidator/Rules/PerformanceRule.cs` ≤ 95L
- (수정) `Editor/ProjectValidator.cs` (650L → ~80L, Run + Print + MenuItem 만)

**검증 방법**:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c1
# 기대: error CS 0건
```

---

## 2. C2 Editor Test 기준

- **C2.1**: 기존 NUnit 120/120 회귀 0 (5021 baseline)
- **C2.2**: D3=A 결정 — NUnit 신규 0 (5024 위임)

**검증 방법**:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c2
# 기대: failed=0, total=120
```

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0 (5021 baseline 동일)
- **C3.2**: warnings 173 (5021 baseline) ± 0
- **C3.3**: 분리 전후 검증 결과 동일 (각 Rule 의 errors/warnings 카운트 보존)

**검증 방법**:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c3
# 기대: [Validator] 완료. errors=0, warnings=173
```

**분리 단계 검증** (Sprint 진행 중):
- Sprint 1.1 후: errors=0, warnings 173 (변동 0 — Issue 추출만)
- Sprint 1.2 후: errors=0, warnings 173 (6 분리 + 5 stub = 11 작동)
- Sprint 2 후: errors=0, warnings 173 (11 모두 분리)
- Sprint 3 후: errors=0, warnings 173 (검증)

---

## 4. C4 Reflection 검증

| Type | 검증 |
|---|---|
| `LittleHero.EditorTools.Issue` (public struct) | `Sev` / `Category` / `Msg` field |
| `LittleHero.EditorTools.Severity` (public enum) | `Error` / `Warning` |
| `LittleHero.EditorTools.ProjectValidator` (static) | `Run()` static + `[MenuItem("Tools/LittleHero/Validate Project")]` |
| `LittleHero.EditorTools.Rules.BuildSettingsRule` (static) | `Validate(List<Issue>)` static |
| `LittleHero.EditorTools.Rules.BalanceRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.ShopRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.ChapterRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.CharacterRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.MemoryMetaRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.EnemyRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.SceneRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.OOPPolicyRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.WeaponBossRule` (static) | 동 |
| `LittleHero.EditorTools.Rules.PerformanceRule` (static) | 동 |

**검증 방법**: grep
```bash
ls Editor/ProjectValidator/Rules/*.cs | wc -l  # 기대: 11
grep -l "Validate(List<Issue>" Editor/ProjectValidator/Rules/*.cs | wc -l  # 기대: 11
```

---

## 5. C5 SO + Asset 검증

(N/A — Editor 도구만, SO 변경 0)

---

## 6. C6 Play 검증 시나리오

### 시나리오 1 — Validate Project 메뉴 동작

**단계**:
1. Unity Editor → Tools > LittleHero > Validate Project
2. Console 결과 관찰

**기대 결과**:
- `[Validator] 완료. errors=0, warnings=173` (5021 baseline 동일)
- 11 Rule 카테고리 모두 표시 (Build/Balance/Shop/Chapter/Character/MemoryMeta/Enemy/Scene/OOP/WeaponBoss/Performance)

### 시나리오 2 — 분리 전후 결과 비교

**단계**:
1. 5022 시작 전 baseline: errors=0, warnings 173
2. 5022 종결 후: 동일

**기대 결과**: 동일 (변동 0)

### 시나리오 3 — Editor Test Runner

**단계**:
1. Window > General > Test Runner > EditMode > Run All
2. 결과: 120/120 PASS (회귀 0)

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0
- [ ] NUnit 120 회귀 0
- [ ] ProjectValidator 결과 변동 0 (errors/warnings)
- [ ] 5020/5021 룰 (StarterHero / CharacterPool / 5005 R5) 회귀 0
- [ ] §15.6 100줄 룰 자기위반 해소 (모든 Rule 100L 미만 + ProjectValidator ~80L)

---

## 8. 합격 기준

### 최소 합격
- C1 컴파일 ✅
- C2 NUnit 120 회귀 0 ✅
- C3 Validator 분리 전후 동일 ✅
- C4 Reflection 11 Rule + Issue/Severity ✅
- C6 시나리오 1+2 PASS

### 완전 합격
- 모든 카테고리 ✅
- C6 시나리오 1+2+3 모두 PASS

---

## 9. Trade-off (mediator decisions 기반)

- **D1=B** (Sprint 1+2 통합) → 항상 11 룰 작동 보장. commit 회수 유연성 미미한 손실.
- **D2=A** (11 Rule 유지) → 80~85L 일부 (100L 룰 충족, 80L 권장 약간 초과). 응집도 우선.
- **D3=A** (NUnit 신규 X) → 5024 위임. 분리 자동 검증 안전망 일시 부재 (수동 비교).

---

## 10. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — C1~C6 + 비기능 + 합격 2단 |
