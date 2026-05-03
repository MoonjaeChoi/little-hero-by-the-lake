---
name: prd-evaluate
description: |
  Little Hero **Phase 6 Evaluator** — Sprint Contract 의 객관 기준(C1~C6) 으로 구현 결과를 독립 검증한다. Anthropic PGE 패턴의 Evaluator 역할 — 구현(Generator)과 분리되어 자기평가 편향 제거.
  6 카테고리 자동/수동: C1 컴파일 / C2 Editor Test (NUnit) / C3 ProjectValidator / C4 Reflection (Type/Method 정적) / C5 SO 검증 (필드 wiring) / C6 Play 시나리오 (사용자 보고).
  FAIL 시 구체적 수정 지시(파일+메서드 수준) 생성 → Phase 5 재진입 (Generator ↔ Evaluator 반복 루프, max 2회).
  반드시 사용해야 하는 상황: "구현 검증", "PRD 검증", "Sprint Contract 검증", "구현 평가", "/prd-evaluate", "<system> 검증", "Phase 6 검증", "evaluator 실행", "PGE 검증", "검증 매트릭스 실행", "Editor Test 자동 검증", "Validator 검증", "Play 검증 가이드 생성". 구현 직후 또는 Sprint 일괄 commit 후 적극 활성화. Contract 가 있는데 검증 미실행 상태에서 자동 호출.
---

# PRD Evaluate — Little Hero PGE Evaluator (Sprint Contract 기준)

> **목적**: 구현 후 Sprint Contract 의 객관 기준으로 독립 검증. **Generator(Phase 5)와 분리** → 편향 제거.
> **Anthropic PGE 핵심**: 자기평가 편향 제거 + 반복 루프 (FAIL → 수정 지시 → Phase 5 재진입).
> Clayve `prd-evaluate.md` 의 Unity 게임 개발 특화 변형.

## 0. 입력

```bash
# Sprint Contract 기반 검증 (권장 — 단일 진실원)
/prd-evaluate --prd 5001
/prd-evaluate --prd docs/little_hero/<system>_v2_prd.md

# Contract 없이 PRD 직접 (legacy)
/prd-evaluate --prd 5001 --no-contract

# 카테고리 한정
/prd-evaluate --prd 5001 --category compile         # C1 만
/prd-evaluate --prd 5001 --category test            # C2 만
/prd-evaluate --prd 5001 --category validator       # C3 만
/prd-evaluate --prd 5001 --category reflection      # C4 만
/prd-evaluate --prd 5001 --category so              # C5 만
/prd-evaluate --prd 5001 --category play            # C6 만 (가이드 출력)
/prd-evaluate --prd 5001 --category auto            # C1~C5 (자동만)

# 반복 루프 (FAIL → 수정 지시 → Phase 5 재진입 → 재검증)
/prd-evaluate --prd 5001 --iterate 2

# 직전 Sprint commit 만 검증 (작은 단위)
/prd-evaluate --commit HEAD
/prd-evaluate --since 5001_breakdown_03   # 특정 태스크 이후
```

## 1. 검증 대상 자동 탐색

1. `<system>_contract.md` + `<system>_contract_checklist.md` (Phase 4 산출 — 권장)
2. `<system>_v2_prd.md` (mediator 최종, `--no-contract` 모드)
3. `<system>_prd.md` (generator 초안, `--no-contract` + 경고)
4. 모두 없음 → 에러 + `prd-contract` 실행 권장

## 2. 6 카테고리 검증 매트릭스

### C1 컴파일 (자동)

```
mcp__UnityMCP__refresh_unity
sleep 5  # 도메인 리로드 안정화
mcp__UnityMCP__read_console (errors)
```

| 결과 | 판정 |
|---|---|
| errors=0, isCompiling=false | ✅ PASS |
| errors>0 | ❌ FAIL — 에러 list + 파일/라인 수정 지시 |
| isCompiling=true (5s+ 대기 후도) | ⚠️ 추가 대기 (max 30s) → 그래도 true 면 FAIL |

### C2 Editor Test (NUnit) (자동)

```
mcp__UnityMCP__execute_menu_item: Window/General/Test Runner
또는 reflection 으로 Test 클래스 + 메서드 존재 + 실행
```

검증 방식 2가지:
1. **권장**: Editor Test Runner > Run All → 결과 파싱
2. **대체**: reflection 으로 Test 메서드 존재 확인 (CI 모드)

| 결과 | 판정 |
|---|---|
| Contract C2 명시 NUnit 모두 PASS | ✅ |
| 일부 FAIL | ❌ FAIL — 실패 메서드 list + 수정 지시 |
| 신규 Test 누락 (Contract 명세 vs 실제) | ❌ FAIL — Test 작성 지시 |

### C3 ProjectValidator (자동)

```
mcp__UnityMCP__execute_menu_item: Tools/LittleHero/Validate Project
mcp__UnityMCP__read_console (warnings)
```

| 결과 | 판정 |
|---|---|
| errors=0, warnings ≤ 기존 | ✅ |
| 신규 warnings (100줄+ / public 필드 / Update in 1500) | ⚠️ 보고 + 사용자 결정 (수정 / 보류) |
| errors>0 | ❌ FAIL |

### C4 Reflection (자동, 정적)

```csharp
// Editor 또는 unity_reflect 호출
foreach (var spec in contract.C4_specs) {
    var t = System.Type.GetType($"{spec.Type}, Assembly-CSharp");
    if (t == null) → FAIL
    foreach (var member in spec.Members) {
        if (t.GetMember(member) == null) → FAIL
    }
}
```

Contract C4 에 명시된 Type / Method / Field / Property 모두 존재.

| 결과 | 판정 |
|---|---|
| 모든 Type + Member 존재 | ✅ |
| 누락 | ❌ FAIL — 누락 list + 신규 작성 지시 |

### C5 SO 검증 (자동)

```csharp
foreach (var spec in contract.C5_specs) {
    var assets = AssetDatabase.LoadAllAssetsAtPath(spec.Path);
    foreach (var so in assets) {
        if (!spec.Validator(so)) → FAIL
    }
}
```

예 (5001 보스):
- BossLootTable 4 인스턴스 모두 tier ∈ {MiniBoss/...}, mythicChance ∈ [0,1]
- EnemyData 51 보스 인스턴스 모두 bossTier != None

| 결과 | 판정 |
|---|---|
| 모든 SO 인스턴스 필드 wiring OK | ✅ |
| 1개라도 default value 또는 잘못된 값 | ❌ FAIL — SO 경로 + 필드 수정 지시 |

### C6 Play 시나리오 (수동 — 사용자 보고)

Contract C6 의 시나리오 N개를 사용자에게 가이드 형식으로 제시:

```
[Play 검증 가이드 — 시나리오 N개]

시나리오 1: <제목>
사전 조건: <씬 / SaveData / SO>
단계:
  1. <단계 1>
  2. <단계 2>
  3. <단계 3>
기대 결과:
  - 콘솔 로그: "<로그 1>"
  - 콘솔 로그: "<로그 2>"
  - SaveData: <필드 변동>

검증 보고 시: "시나리오 1 PASS" 또는 "시나리오 1 FAIL — <증상>"

시나리오 2, 3: ...
```

사용자가 결과 보고 → evaluation.md 에 기록. 자동 판정 X (수동 항목).

## 3. 워크플로 (6 단계)

### Step 1 — Contract 로드

병렬 Read:
- `<system>_contract.md` (단일 진실원)
- `<system>_contract_checklist.md` (체크리스트)
- breakdown 문서 (해당 시 — 구현 범위 추정)

`--no-contract` 모드:
- v2 PRD 의 검증 시나리오 + 신규 클래스 명세에서 자동 추출
- ⚠️ 경고: scope creep 위험 (Contract 가 단일 진실원이 아님)

### Step 2 — 자동 검증 (C1~C5)

순차 실행 (의존성):
1. C1 컴파일 — FAIL 시 이후 중단
2. C2 Editor Test
3. C3 ProjectValidator
4. C4 Reflection
5. C5 SO

각 단계 결과 → `<system>_evaluation.md` 의 매트릭스 갱신.

### Step 3 — 수동 검증 가이드 (C6)

C6 시나리오 가이드 출력. 사용자 보고 대기.

### Step 4 — 판정 + 수정 지시 (FAIL 시)

```markdown
❌ <system> Phase 6 — FAIL

[자동 검증 결과]
  C1 컴파일      ❌ errors=2
  C2 Editor Test ⏸ (C1 FAIL 로 중단)
  ...

[수정 지시]

FAIL-1: BossPatternFactory.cs:23 — 메서드 누락
  - Contract C4 명세: BossPatternFactory.Create(int, EnemyController)
  - 실제: BossPatternFactory.Create(int) — 두 번째 파라미터 누락
  - 수정: Create 시그니처 변경 + IBossPhase 구현체 호출 갱신

FAIL-2: BossLootTable_DailyBoss.asset — mythicChance default
  - Contract C5 명세: mythicChance ∈ [0,1], DailyBoss=0.5
  - 실제: mythicChance=0 (default)
  - 수정: SO Inspector 또는 Editor 메뉴로 0.5 설정

[다음]
  → Phase 5 재진입 (반복 1/2): 위 2건 수정 후 자동 재검증
  또는 사용자 명시 수정 → /prd-evaluate
```

### Step 5 — 반복 루프 (Generator ↔ Evaluator)

```
┌──────────────┐     ┌──────────────┐
│   구현 수정   │ ←── │   Evaluate   │
│  (Generator) │     │  (Evaluator) │
│   Phase 5    │ ──→ │   Phase 6    │
└──────────────┘     └──────────────┘
   ↑ 반복 (max 2회) ↓
   └─ 같은 항목 2회 연속 FAIL → 수동 검토 ─┘
```

**반복 프로세스**:
1. Phase 6 FAIL → 수정 지시 생성
2. Phase 5 재진입 (수정만 — 신규 기능 X)
3. 컴파일 검증 (사이클 내) → 재 commit
4. Phase 6 재검증
5. PASS 또는 반복 소진

**Context Reset**: 반복 간 이전 컨텍스트 전체가 아닌 **실패 항목 + 수정 지시만** 전달.

**수렴 규칙**: 같은 항목이 2회 연속 FAIL → "수동 검토" 에스컬레이션.

### Step 6 — 평가 보고서 작성

```markdown
# <system> Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: <system>_contract.md · 검증일: YYYY-MM-DD · 반복: N/2

## 1. 종합: PASS / FAIL

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 |
|---|---|---|---|---|
| C1 컴파일 | 1 | 0 | 1 | ✅ |
| C2 Editor Test | 13 | 0 | 13 | ✅ |
| C3 ProjectValidator | - | - | warnings 21 → 21 | ✅ |
| C4 Reflection | 7 | 0 | 7 | ✅ |
| C5 SO | 55 | 0 | 55 | ✅ |
| **자동 합계** | **76** | **0** | **76** | **✅** |

## 3. 수동 검증 (Play 시나리오)

| # | 시나리오 | 결과 |
|---|---|---|
| 1 | MiniBoss 처치 → 보상 드랍 | ⏸ 사용자 보고 대기 |
| 2 | ChapterBoss Phase 2 전환 | ⏸ 대기 |
| 3 | WeeklyBoss 신화 보장 | ⏸ 대기 |

## 4. FAIL 항목 (있으면)

### FAIL-1: <항목>
- **카테고리**: C2 / C4 / ...
- **기대 (Contract)**: <인용>
- **실제**: <관찰>
- **근본 원인 추정**: <코드 / SO / 설정>
- **수정 지시**:
  - 파일: <경로>
  - 라인: <번호>
  - 변경: <구체적>
- **반복 회차**: 1/2

## 5. PASS 항목 요약
- ...

## 6. 비기능 요구사항
- 기존 weapon_v2 회귀 X (수동 검사 권장)
- 60fps 유지 (Profiler 권장)
- 빌드 (Unity Build) 성공 (해당 시)

## 7. 다음 단계

PASS:
  - 사용자 Play 시나리오 1, 2, 3 보고 대기
  - 모두 PASS → changelog 등재 + CLAUDE.md §11 마일스톤 (큰 시스템 시)
  - /prd-pipeline <next system>

FAIL + 반복 남음:
  - 반복 N+1 시작 → Phase 5 수정 → 재검증

FAIL + 반복 소진:
  - 수동 검토 요청
  - 부분 산출 보존 (commit 유지)

## 8. 변경 이력
- YYYY-MM-DD HH:MM: 검증 실행 (반복 N/2)
```

## 4. 사용 패턴

| 발화 | 동작 |
|---|---|
| "구현 검증" | Contract 기반 자동 (C1~C5) + C6 가이드 |
| "<system> 검증" | 시스템 매칭 |
| "/prd-evaluate --prd 5001 --iterate 2" | 명시 + 반복 루프 |
| "컴파일만" | `--category compile` |
| "Test 실행" | `--category test` |
| "Validator" | `--category validator` |
| "Play 검증 가이드" | `--category play` (수동 가이드만 출력) |
| "Phase 5 재진입 후 재검증" | 자동 반복 (반복 카운트 +1) |

## 5. 자동 검증 도구

### 5.1 Reflection (C4)

```csharp
var t = System.Type.GetType("LittleHero.IBossPhase, Assembly-CSharp");
if (t == null) report.Fail("Type missing: IBossPhase");
foreach (var m in new[] {"OnEnter", "Tick", "OnExit", "ShouldTransitionNext"}) {
    if (t.GetMethod(m) == null) report.Fail($"Method missing: {m}");
}
```

### 5.2 SO 검증 (C5)

```csharp
var assets = AssetDatabase.FindAssets("t:BossLootTable")
    .Select(g => AssetDatabase.GUIDToAssetPath(g))
    .Select(p => AssetDatabase.LoadAssetAtPath<BossLootTable>(p));
foreach (var so in assets) {
    if (so.tier == BossTier.None) report.Fail($"BossLootTable tier=None: {so.name}");
    if (so.mythicChance < 0 || so.mythicChance > 1) report.Fail($"mythicChance out of range");
}
```

### 5.3 시뮬레이션 (선택)

PRD/Contract 명시 시:
- `Tools/LittleHero/Curve Simulator`
- `Tools/LittleHero/Boss System Simulation` (신규 작성 가능)
- 결과 markdown 생성 + 이전 세대 비교

## 6. 출력 보고

```
═══════════════════════════════════════════
<system> Phase 6 Evaluation 결과 (반복 N/2)
═══════════════════════════════════════════

자동 검증 (Contract 기준):
  ✅ C1 컴파일       errors=0
  ✅ C2 Editor Test  13/13 PASS
  ✅ C3 Validator    warnings 21 (변동 0)
  ✅ C4 Reflection   7 Type 모두 통과
  ✅ C5 SO 검증      BossLootTable 4 + EnemyData 51 모두 OK

수동 검증 (Play):
  ⏸ 시나리오 1, 2, 3 — 사용자 보고 대기
    [Play 가이드 출력]

종합: PASS (자동 76/76, 수동 보고 대기)

[다음]
  - 사용자 Play 시나리오 1, 2, 3 진행
  - 모두 PASS → /changelog-manager 자동 등재
  - 또는 /prd-pipeline <next system>
═══════════════════════════════════════════
```

## 7. FAIL 시 출력 예

```
❌ <system> Phase 6 — FAIL (반복 1/2)

자동 검증:
  ✅ C1 컴파일       errors=0
  ❌ C2 Editor Test  11/13 PASS (2 FAIL)
  ✅ C3 Validator    warnings 21
  ❌ C4 Reflection   6/7 (BossPatternFactory.Create 시그니처 불일치)
  ✅ C5 SO           OK

[수정 지시]

FAIL-1: BossSystemTests.PhaseTransition_OnLowHp_TriggersPhase2
  Contract C2 명세: HP < 50% 시 Phase 2 진입
  실제: 메서드 시그니처 mismatch
  수정: BossPhase1Default.ShouldTransitionNext 반환값 검사 갱신
  파일: Scripts/Gameplay/Enemies/BossPhase1Default.cs:23

FAIL-2: BossPatternFactory.Create
  Contract C4 명세: Create(int phaseId, EnemyController enemy) → IBossPhase
  실제: Create(int phaseId) — 두 번째 파라미터 누락
  수정: 시그니처 + 호출처 갱신 (BossPhaseManager.cs:45)
  파일: Scripts/Gameplay/Enemies/BossPatternFactory.cs:18

[다음]
  → Phase 5 재진입 (반복 2/2): 위 2건 수정 후 자동 재검증
  또는 /prd-evaluate (수동 재검증)
```

## 8. 원칙 (Anthropic PGE)

- **독립 검증**: 구현 코드를 읽지 않고 동작/존재만 검증 (scope creep 방지)
- **Contract 단일 진실원**: PRD 직접 추출 X (`--no-contract` 시 경고)
- **Pass/Fail 자동 판정**: 주관 X
- **구체적 수정 지시**: 파일 + 라인 + 메서드 수준
- **반복 루프**: max 2회. 같은 항목 2회 FAIL → 수동
- **Generator/Evaluator 분리**: 같은 Claude 라도 Contract 가 단일 진실원 → 편향 제거
- **회귀 방지**: 비기능 요구사항 (기존 시스템 동작 유지) 별도 보고

## 9. 금지 사항

- Contract 에 없는 기준으로 판정 (scope creep)
- 주관적 코드 품질 평가 (포매팅 / 네이밍 의견)
- 구현 방식 변경 지시 (수정 지시 = 기능 동작 보정만)
- 성능 기준이 Contract 에 없을 때 임의 기준 적용

## 10. Workflow 위치

```
구현 (Phase 5 — Generator)
       ↓
/prd-evaluate ← 현재 (Phase 6 — Evaluator)
       ↑       ↓
       └─ 반복 ─┘ (max 2회)
       
PASS → changelog-manager + CLAUDE.md §11
FAIL (반복 소진) → 수동 검토
```

## 11. 출처

- Clayve `prd-evaluate.md` Anthropic PGE Evaluator 패턴
- CLAUDE.md §15.9 단위 테스트 + §15.12 ProjectValidator
- `Tests/Editor/` 23 NUnit 사례 (EnhancementService/DamageDealer/StarforceService)
- `Editor/ProjectValidator.cs` (200줄+ 자동 경고 룰)
- 5001 boss_system_v2 Contract 의 6 카테고리 검증 명세
- weapon_v2 검증 사이클 (refresh + 5~10s + read_console 패턴)
