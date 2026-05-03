---
name: prd-contract
description: |
  Little Hero **Phase 4 Sprint Contract** — 구현 전에 "무엇이 성공인가" 를 객관적·테스트 가능한 형태로 정의한다. Anthropic PGE 패턴의 핵심 — Generator(구현자) 가 자기 작업을 평가하면 편향이 생기므로, 독립 Evaluator 가 사용할 검증 기준을 사전 분리.
  6 카테고리: 컴파일 / Editor Test / ProjectValidator / Reflection 검증 / SO 검증 / Play 검증 시나리오. 모든 기준은 Pass/Fail 로 자동 판정 가능.
  반드시 사용해야 하는 상황: "Sprint Contract", "검증 계약", "검증 기준 정의", "구현 전 성공 기준", "/prd-contract", "<system> contract", "객관 검증 정의", "PGE Planner Contract", "구현 후 검증 매트릭스 사전 정의", "evaluator 기준 작성". breakdown 후 구현 직전 단계에서 적극 활성화. PRD/breakdown 만으로 검증 기준 추출이 모호한 모든 상황에서 자동 호출.
---

# PRD Sprint Contract — Little Hero 구현 전 검증 계약

> **목적**: 구현 전에 객관 검증 기준 + Play 시나리오를 분리 정의 → Evaluator(독립 검증자) 가 사용.
> **Anthropic PGE 핵심**: Generator(구현자) 의 자기평가 편향 제거. Contract 가 단일 진실원.
> Clayve `prd-contract.md` 의 Unity 게임 개발 특화 변형.
>
> **저장 위치 (5001/5002 정착 패턴)**: 입력 PRD v2 + 산출 contract / contract_checklist 모두 `docs/prd/<번호>/` 폴더. 번호 자동 추출 + `mkdir -p`.

## 0. 입력

```bash
# PRD v2 (mediator 최종) 기준
/prd-contract --prd docs/prd/<번호>/<system>_v2_prd.md

# 문서번호 자동 탐색
/prd-contract --prd 5001

# breakdown 참조 (더 구체적 contract)
/prd-contract --prd 5001 --with-breakdown

# 레이어 한정 (Data/Logic/UI/Test)
/prd-contract --prd 5001 --layer Logic
```

PRD 자동 탐색 순서:
1. `<system>_v2_prd.md` (mediator 최종)
2. `<system>_prd.md` (rebuttal/generator)
3. 둘 다 없으면 → `prd-mediator` (또는 `prd-generator`) 자동 호출

## 1. 출력 (2 파일)

| 파일 | 용도 |
|---|---|
| `<system>_contract.md` | Sprint Contract 본체 — 6 카테고리 검증 기준 |
| `<system>_contract_checklist.md` | Evaluator 자동/수동 체크리스트 |

## 2. 6 검증 카테고리 (Little Hero)

| 카테고리 | 검증 방식 | 예시 |
|---|---|---|
| **C1 컴파일** | `mcp__UnityMCP__refresh_unity` + `read_console` errors=0 | 모든 Sprint 후 |
| **C2 Editor Test (NUnit)** | Test Runner Edit Mode 또는 reflection 으로 Test 클래스 실행 | `BossSystemTests.PhaseTransition_OnLowHp_TriggersPhase2` |
| **C3 ProjectValidator** | `Tools/LittleHero/Validate Project` errors=0 + warnings 변동 ±0 또는 감소 | 100줄 룰, public 필드 검사 |
| **C4 Reflection (정적)** | `Type.GetType` + `GetMethod` 존재 검증 | `BossPatternFactory` 클래스 + `Create(...)` 메서드 |
| **C5 SO 검증** | `AssetDatabase.LoadAssetAtPath` + 필드 wiring 검사 | 51 Boss SO 의 `bossTier != BossTier.None` |
| **C6 Play 검증 시나리오** | 사용자가 Play Mode 에서 시나리오 실행 → 콘솔 로그 + SaveData 검증 | "MiniBoss 처치 → BossLootTable.Drop 호출 로그" |

## 3. 워크플로 (5 단계)

### Step 1 — PRD + breakdown 분석

병렬 Read:
- `<system>_v2_prd.md` (mediator 최종)
- `<system>_breakdown_*.md` (있으면 — 구체적 메서드 명세 추출)
- `<system>_decisions.md` (mediator 결정 — Trade-off)
- CLAUDE.md §7 성능 목표 + §15 OOP

### Step 2 — 객관 검증 기준 정의

각 기준은 **Pass/Fail 자동 판정 가능**. 주관적 표현은 수치로 변환:

| ❌ 주관 | ✅ 객관 |
|---|---|
| "보스 전투가 재밌다" | "Phase 2 진입 시 attackInterval × 1.3f, 콘솔 로그 'BossPhase2.OnEnter' 출력" |
| "성능 좋다" | "동시 적 cap 300 환경에서 60fps 유지, GC <50KB/s (Profiler 측정)" |
| "UI 깔끔하다" | "BossHealthBar 활성화 시 Phase 2 색상 #FF6B6B 적용 (확인 가능)" |

### Step 3 — Hybrid (Little Hero 등가) 검증 기준

CLAUDE.md §4 3 계층 (Data/Logic/UI) 정합:

| Layer | 추가 검증 |
|---|---|
| **Data Layer** | SO 신규 + 마이그레이션 후 모든 인스턴스 유효 (default value X) |
| **Logic Layer** | EventBus 정합 (구독 / 발행 짝) + Singleton 직접 참조 X (grep) |
| **UI Layer** | Manager 데이터 직접 수정 X (이벤트 구독만) |

### Step 4 — Play 검증 시나리오 (구체적)

각 시나리오:
1. 사전 조건 (씬 / SaveData / SO 인스턴스)
2. 사용자 단계 (1, 2, 3...)
3. 기대 결과 (정확한 동작)
4. 검증 방법 (콘솔 로그 / SaveData 필드 / 시각적 변화)

### Step 5 — 문서 작성

#### 5.1 `<system>_contract.md`

```markdown
# Sprint Contract: <system>

> **PGE Planner — Phase 4 Contract**
> PRD: <system>_v2_prd.md · breakdown: <breakdown_00.md>
> 생성일: YYYY-MM-DD

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5초 내 안정화 (`editor_state.isCompiling == false`)

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 5  # 또는 isCompiling 폴링
mcp__UnityMCP__read_console (errors)  # 0 이어야 함
```

---

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 수 | 목적 |
|---|---|---|
| `BossSystemTests` | 13 | Phase / Factory / LootTable 단위 검증 |

### 2.1 BossSystemTests 명세

- `BossPhaseManager_Initialize_DefaultsToPhase1` — Phase 1 시작 검증
- `BossPhase1_ShouldTransitionNext_OnHpBelow50_ReturnsTrue` — 임계값 검증
- `BossPhase2_OnEnter_AppliesAttackSpeedMultiplier` — 속도 1.3× 검증
- `BossPatternFactory_Create_BossPhaseId1_ReturnsBossPhase1Default`
- `BossLootTable_GetReward_WeeklyBoss_GuaranteesMythic` — mythicChance 1.0
- ... (총 13)

**검증 방법**: Test Runner Edit Mode > Run All → 13/13 PASS

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings ≤ 기존 (5001 시작 시점 기준 — 21)
- **C3.3**: 신규 위반 없음 (100줄+ / public 필드 / Update in 1500)

**검증 방법**: `Tools/LittleHero/Validate Project` 실행 → 콘솔 출력 비교

**Validator 룰 추가 명세** (해당 시):
- `BossSO_BossTier_NotNone` — 51 Boss SO 의 bossTier != None 강제

---

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.BossTier` (enum) | `MiniBoss / ChapterBoss / DailyBoss / WeeklyBoss / None` |
| `LittleHero.IBossPhase` (interface) | `OnEnter / Tick / OnExit / ShouldTransitionNext` |
| `LittleHero.BossPhase1Default` | `IBossPhase` 구현 |
| `LittleHero.BossPhase2Aggressive` | `IBossPhase` 구현 |
| `LittleHero.BossPatternFactory` (sealed) | `Create(int, EnemyController) → IBossPhase` (정적) |
| `LittleHero.BossLootTable` (SO) | `tier / mythicChance / equipmentChance / starDustAmount` |
| `LittleHero.BossPhaseManager` | `Update(EnemyController, float deltaTime)` (정적) |

**검증 방법** (script):
```csharp
// Editor 시뮬 또는 reflection 직접
var t = System.Type.GetType("LittleHero.IBossPhase, Assembly-CSharp");
return t != null && t.GetMethod("OnEnter") != null;
```

---

## 5. C5 SO 검증

| SO 클래스 | 인스턴스 수 | 필드 검증 |
|---|---|---|
| `BossLootTable` | 4 | tier ∈ {MiniBoss/ChapterBoss/DailyBoss/WeeklyBoss}, mythicChance ∈ [0,1] |
| `EnemyData` (기존 + 마이그레이션) | 51 (보스만) | bossTier != None |

**검증 방법**:
- `AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Data/Enemies/")` → bossTier 확인
- Editor 메뉴 `Tools/LittleHero/Validate Boss SO` (신규 작성)

---

## 6. C6 Play 검증 시나리오

### 시나리오 1: MiniBoss 처치 → 보상 드랍

**사전 조건**:
- 챕터 1 시작 / Wave 5 (MiniBoss 출현 wave)
- `BossLootTable.MiniBoss` SO 인스턴스 wiring

**단계**:
1. 챕터 1 진입 → Wave 5 시작
2. MiniBoss 출현 (Phase 1 — `BossPhase1Default`)
3. HP 0 으로 처치
4. `BossLootTable.MiniBoss.GetReward()` 호출 → 보상 인벤토리 추가

**기대 결과**:
- 콘솔 로그: `[BossPhaseManager] MiniBoss defeated, BossLootTable applied`
- 콘솔 로그: `[ItemDropService] StarDust +X (from BossLootTable)`
- SaveData: `MetaProgression.starDust += X`

---

### 시나리오 2: ChapterBoss Phase 2 전환

**사전 조건**:
- 챕터 1 Wave 20 (ChapterBoss)
- HP 50% 이상 시작

**단계**:
1. Wave 20 ChapterBoss 출현 (Phase 1)
2. HP 50% 시점 도달
3. `BossPhase1.ShouldTransitionNext == true` 반환
4. `BossPhase2Aggressive.OnEnter` 호출
5. Phase 2 — attackInterval × 1.3f, telegraph 단축

**기대 결과**:
- 콘솔 로그: `[BossPhase1] Transition triggered (HP < 50%)`
- 콘솔 로그: `[BossPhase2.OnEnter] Aggressive mode (×1.3 speed)`
- 시각 변화: BossHealthBar 색 변경 (해당 시)

---

### 시나리오 3: WeeklyBoss 신화 보장

**사전 조건**:
- Endgame 잠금 해제 (CH 35 클리어)
- 주간 보스 출현 (월요일 KST 0시 리셋 후)

**단계**:
1. Endgame 메뉴 → 주간 보스 진입
2. `BossEncounterRunner.Begin(WeeklyBoss)` 호출
3. WeeklyBoss 처치
4. `BossLootTable.WeeklyBoss.mythicChance == 1.0` → 신화 발현 보장

**기대 결과**:
- 콘솔 로그: `[BossLootTable.WeeklyBoss] Mythic guaranteed (chance 1.0)`
- 신화 등록: `MythicMileageService.Grant(<id>)`
- UI: 신화 발현 토스트 표시

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0
- [ ] Editor Test 13/13 PASS
- [ ] ProjectValidator warnings ≤ 21 (변동 ≤ 0)
- [ ] 기존 시스템 회귀 X (weapon_v2 / stats_v2 동작 유지)
- [ ] 빌드 (Unity → Build) 성공 (해당 시)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 11/13 이상 ✅
- C3 Validator errors=0 ✅
- 시나리오 1, 2 PASS

**완전 합격**:
- 모든 카테고리 ✅
- 시나리오 1, 2, 3 모두 PASS
- 비기능 요구사항 모두 ✅

## 9. Trade-off (mediator 결정 기반)

(decisions.md 의 "Trade-off" 항목을 여기에 인용 — 해당 시)

- D1 통합 결정 → Sprint 7 = changelog + CLAUDE.md (1 commit). 분리 시 추적성 향상의 손실은 작음.
- D2 SerializeField 결정 → 디자이너 후속 조정 가능. Trade-off: PRD 의 1.3f 명시값이 SO 기본값으로 이동.

## 10. 변경 이력
- YYYY-MM-DD: 초기 작성 (Phase 4 Contract)
```

#### 5.2 `<system>_contract_checklist.md`

```markdown
# Evaluator Checklist: <system>

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일
- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false`

### C2 Editor Test (NUnit)
- [ ] Test Runner > Edit Mode > Run All → 13/13 PASS
- [ ] `BossSystemTests` 클래스 존재
- [ ] (자동) reflection 으로 13 메서드 명세 확인

### C3 ProjectValidator
- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings ≤ 21
- [ ] 신규 위반 0건

### C4 Reflection 정적 검증
- [ ] `LittleHero.BossTier` enum 5 값 존재
- [ ] `LittleHero.IBossPhase` 4 메서드
- [ ] `BossPhase1Default / BossPhase2Aggressive` IBossPhase 구현
- [ ] `BossPatternFactory.Create` 정적 메서드
- [ ] `BossLootTable` SO 클래스 + 4 필드
- [ ] `BossPhaseManager.Update` 정적

### C5 SO 검증
- [ ] `BossLootTable` 4 인스턴스 (Data/Loot/Boss/)
- [ ] mythicChance 합리 (MiniBoss 0 / ChapterBoss 0.05 / DailyBoss 0.5 / WeeklyBoss 1.0)
- [ ] `EnemyData` 51 보스 인스턴스 모두 bossTier != None
- [ ] (자동) `Tools/LittleHero/Validate Boss SO` 실행

## 수동 검증 (사용자 Play Mode)

### C6.1 MiniBoss 처치
- [ ] 시나리오 1 단계 1~4 진행
- [ ] 콘솔 로그 3종 확인
- [ ] SaveData StarDust 증가 확인

### C6.2 ChapterBoss Phase 2
- [ ] 시나리오 2 단계 1~5 진행
- [ ] 콘솔 로그 2종 확인
- [ ] 시각 변화 (BossHealthBar) 확인

### C6.3 WeeklyBoss 신화 보장
- [ ] 시나리오 3 단계 1~4 진행
- [ ] 콘솔 로그 확인
- [ ] 신화 발현 UI 확인

## 비기능 검증

- [ ] 기존 weapon_v2 가챠 정상 (회귀 검사)
- [ ] 기존 stats_v2 데미지 공식 정상
- [ ] 60fps 유지 (Profiler 확인 — 권장)

## 합격 판정

- 자동 검증 모두 ✅ + 수동 시나리오 1, 2 ✅ → **최소 합격**
- 자동 검증 모두 ✅ + 수동 시나리오 1, 2, 3 모두 ✅ → **완전 합격**
- 자동 검증 1개라도 ❌ → **FAIL** (Phase 5 수정)
```

## 4. 사용 패턴

| 발화 | 동작 |
|---|---|
| "Sprint Contract" | 직전 v2 PRD 자동 매칭 |
| "<system> contract" | 시스템 매칭 |
| "/prd-contract --prd 5001" | 명시 |
| "검증 기준 정의" | Contract 작성 |
| "/prd-contract --with-breakdown" | breakdown 참조 |

## 5. 원칙 (Anthropic PGE 핵심)

- **객관성**: 모든 기준은 Pass/Fail 자동 판정 (주관 X)
- **완전성**: PRD 의 모든 요구사항이 최소 1개 기준에 매핑
- **독립성**: 구현 코드를 보지 않고 결과만 검증
- **분리**: Generator(Phase 5) 와 Evaluator(Phase 6) 가 같은 Claude 라도, Contract 가 사전 정의되었기에 편향 제거
- **수치화**: "재밌다" / "빠르다" / "깔끔하다" → 수치 또는 정확한 동작으로 변환

## 6. 금지 사항

- 코드 구현 (계약만 작성)
- 구현 방법 지시 (결과만 검증)
- 측정 불가 기준
- 기존 PRD/breakdown 수정 (별도 파일)

## 7. 출력 보고

```
✅ <system> Sprint Contract 완료

[검증 카테고리] 6
  - C1 컴파일 (errors=0)
  - C2 Editor Test (13 NUnit 명세)
  - C3 ProjectValidator (warnings ≤ 21)
  - C4 Reflection (7 Type 검증)
  - C5 SO 검증 (BossLootTable 4 + EnemyData 51)
  - C6 Play 시나리오 (3 시나리오)

[합격 기준]
  - 최소: 자동 모두 + 시나리오 1+2
  - 완전: 자동 모두 + 시나리오 1+2+3 + 비기능

[산출 파일]
  - <system>_contract.md (계약 본체)
  - <system>_contract_checklist.md (Evaluator 체크리스트)

[다음 단계]
  → Phase 5 구현 시작 (little-hero-dev 또는 직접)
  구현 후 → /prd-evaluate --prd <system>
```

## 8. Workflow 위치

```
/prd-breakdown → /prd-contract ← 현재
                       ↓
                 검증 계약 (단일 진실원)
                       ↓
                Phase 5: 구현 (Generator)
                       ↓
                /prd-evaluate (Phase 6 — Evaluator, Contract 기준)
                       ↑       ↓
                       └─ 반복 ┘
```

## 9. 출처

- Clayve `prd-contract.md` 5 카테고리 + Hybrid Architecture 패턴
- Anthropic Planner-Generator-Evaluator 연구 (구현 전 기준 분리 → 품질 향상)
- CLAUDE.md §7 성능 목표 + §15.9 단위 테스트
- 기존 `Editor Test 23 NUnit` 사례 (EnhancementService/DamageDealer/StarforceService) — Test 클래스 명세 패턴
- 5001 boss_system v2 PRD 의 검증 시나리오 3 → Contract C6 변환
