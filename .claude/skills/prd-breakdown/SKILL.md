---
name: prd-breakdown
description: |
  Little Hero **Phase 3 Breakdown** — mediator 가 확정한 PRD v2 를 실행 가능한 구현 태스크로 분해하여 세부 구현 문서 N개를 자동 생성한다. 4 레이어(Data SO → Logic Manager → UI Panel → Test) 또는 Sprint commit 단위로 분해.
  PRD 의 Sprint 표를 더 구체적인 파일·메서드 수준 명세로 확장 — 각 문서는 1 commit 또는 1 클래스/메서드 단위.
  반드시 사용해야 하는 상황: "PRD 분해", "구현 태스크 분해", "implementation breakdown", "구현 계획 생성", "PRD를 태스크로", "작업 분해", "/prd-breakdown", "/imb", "<system> 분해", "Sprint 세부화", "구현 문서 생성". mediator 가 확정한 v2 PRD 가 있는데 곧바로 구현하기엔 단위가 너무 큰 모든 상황에서 적극 활성화.
---

# PRD Breakdown — Little Hero 구현 태스크 분해

> **목적**: 확정된 PRD v2 → 구현 가능한 세부 태스크 N개. 4 레이어 분해 (Data SO / Logic / UI / Test).
> Clayve `prd-implementation-breakdown` 의 Unity 게임 개발 특화 변형 (PGE Planner 단계).
>
> **저장 위치 (5001/5002 정착 패턴)**:
> - 입력 PRD v2: `docs/prd/<번호>/<system>_v2.md`
> - 산출 breakdown_00 ~ breakdown_NN: 모두 같은 `docs/prd/<번호>/` 폴더
> - 번호 자동 추출 + `mkdir -p`

## 0. 입력

```bash
# Mode 1: 새 PRD 분해
/prd-breakdown --prd docs/prd/<번호>/<system>_v2.md

# Mode 2: 문서번호 자동 탐색
/prd-breakdown --prd 5001    # docs/prd/5001/5001_*_v2.md 자동 매칭

# Mode 3: 범위 재작성
/prd-breakdown --rewrite 5001_03~5001_06
```

PRD 자동 탐색 순서:
1. `docs/prd/<번호>/<번호>_*_v2.md` (mediator 최종)
2. `docs/prd/<번호>/<번호>_*_prd.md` (rebuttal 또는 generator)
3. 둘 다 없으면 → `prd-mediator` (또는 `prd-generator`) 자동 호출 후 진행

## 1. 출력 (N+1 파일)

`docs/prd/<번호>/` 내 평탄 구조 (번호 폴더 미존재 시 자동 생성):

| 파일 | 용도 |
|---|---|
| `<번호>_<system>_breakdown_00.md` | 전체 태스크 목록 + 의존성 그래프 + 구현 순서 |
| `<번호>_<system>_breakdown_01_<area>.md` ~ `_NN_*.md` | 태스크별 세부 명세 (1 파일 = 1 commit 또는 1 클래스) |

> 5001 시리즈 예시:
> - `5001_boss_system_v2_breakdown_00.md`
> - `5001_boss_system_v2_breakdown_01_BossTier_enum.md`
> - `5001_boss_system_v2_breakdown_02_IBossPhase_interface.md`
> - `5001_boss_system_v2_breakdown_03_BossPatternFactory.md`
> - `5001_boss_system_v2_breakdown_04_BossLootTable_SO.md`
> - `5001_boss_system_v2_breakdown_05_EnemyData_migration.md`
> - `5001_boss_system_v2_breakdown_06_BossEncounterRunner_hook.md`
> - `5001_boss_system_v2_breakdown_07_BossSystemTests.md`
> - `5001_boss_system_v2_breakdown_08_changelog_CLAUDE_md.md`

## 2. 4 레이어 분해 (Little Hero 컨텍스트)

| 레이어 | Clayve 등가 | Little Hero 구현 |
|---|---|---|
| **Data (SO)** | DB 마이그레이션 | ScriptableObject 클래스 신규 + SO 인스턴스 마이그레이션 (Editor 메뉴) |
| **Logic** | Backend Service/API | MonoBehaviour Manager + 정적 Service + interface/Strategy/Factory |
| **UI** | Frontend Component | uGUI Panel/Card + EventBus 구독 + DOTween |
| **Test** | Vitest/Playwright | Editor Test (NUnit) + ProjectValidator 룰 + Play 시나리오 |

### 2.1 분해 우선순위

PRD 의 Sprint 가 작업량 단위라면, breakdown 은 **commit / 클래스 / 메서드 단위**:

| Sprint 작업 | breakdown 분할 |
|---|---|
| Sprint 1 (3 commit, 인프라 신규) | 3개 breakdown 문서 (enum / interface / 정적 헬퍼) |
| Sprint 2 (1 commit, SO 마이그레이션) | 1개 (Editor 메뉴 + N SO 갱신 일괄) |
| Sprint 3 (2 commit, 검증 + Bonus) | 2개 (Manager 진입부 검증 / 정적 헬퍼) |
| Sprint 4 (1 commit, UI) | 1개 (Panel + 라벨) |
| Sprint 5 (1 commit, Test) | 1개 (NUnit 13 + Validator) |

## 3. 분해 원칙

| 원칙 | 설명 |
|---|---|
| **최소 단위** | 1 문서 = 1 commit (또는 1 enum/interface/클래스) |
| **독립성** | 의존성 그래프 명시. 순환 의존 X. |
| **§15 OOP 정합** | 신규 클래스 100줄 이내 / SerializeField + private / Strategy 패턴 |
| **테스트 포함** | 각 문서에 검증 기준 (컴파일 / Editor Test / Play) |
| **MCP 안전** | Unity MCP 호출 패턴 명시 (refresh + 5~10s 대기, 도메인 리로드 회피) |

## 4. 워크플로 (5 단계)

### Step 1 — PRD v2 + 디자인 로드

병렬 Read:
- `<system>_v2.md` (mediator 최종)
- `<system>.md` (디자인 — 옵션 비교 + 권장 설계)
- `<system>_decisions.md` (mediator 결정)
- CLAUDE.md §3 폴더 구조 + §4 아키텍처 + §15 OOP

### Step 2 — 태스크 추출 (Sprint × commit 단위)

PRD 의 Sprint 표 + 각 Sprint 의 commit 분해 → N 태스크 list:

```
Sprint 1 (3 commit, 인프라):
  T01: BossTier enum 신규 (Data 레이어, 1 commit)
  T02: IBossPhase interface (Logic 레이어, 1 commit)
  T03: BossPhase 1+2 구체 구현 (Logic 레이어, 1 commit)

Sprint 2 (1 commit, Factory):
  T04: BossPatternFactory 분리 (Logic 레이어)

Sprint 3 (2 commit, SO):
  T05: BossLootTable SO 클래스 + 4 인스턴스 (Data 레이어)
  T06: EnemyData bossTier 필드 + 마이그레이션 (Data 레이어)

...
```

### Step 3 — 개요 문서 작성 (`<번호>_breakdown_00.md`)

```markdown
# <system> Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: YYYY-MM-DD · 총 N 태스크 / X commit

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | BossTier enum | Data | - | 5분 | _01_*.md |
| T02 | IBossPhase interface | Logic | - | 10분 | _02_*.md |
| T03 | BossPhase 1+2 구현 | Logic | T02 | 25분 | _03_*.md |
| T04 | BossPatternFactory | Logic | - | 15분 | _04_*.md |
| ...

## 2. 의존성 그래프

```
T01 (BossTier) ─┐
T02 (IBossPhase) ─┐                ┌─→ T07 (Test)
                 ├─→ T03 (Phases) ─┤
                 │                 ├─→ T08 (changelog)
T04 (Factory) ───┤                 │
T05 (LootTable) ─┤                 │
T06 (Migration) ─┘                 │
```

## 3. 병렬 가능성

- 병렬 그룹 1: T01, T02, T04, T05 (서로 독립)
- 병렬 그룹 2: T03, T06 (T02 / T01 의존)
- 직렬: T07 (모두 의존), T08 (T07 의존)

## 4. 구현 순서 권장

1. T01 + T02 + T04 + T05 (병렬 가능, 30분)
2. T03 + T06 (T02/T01 의존, 30분)
3. T07 (Test, 30분)
4. T08 (docs, 10분)

총 1h 40m (병렬 시) / 2h 30m (직렬 시)

## 5. CLAUDE.md §15 정합 자동 검증

- T01: enum 신규 — §15.4 SO 우선 정합 ✅
- T02: interface — §15.1 ISP 정합 ✅
- T03: 구체 구현 — §15.6 100줄 이내 (각 30~40 라인) ✅
- T04: Factory — §15.1 OCP 정합 ✅
- ...

## 6. Unity MCP 안전 전략

- 한 사이클 = 1 태스크 + commit + 5~10s 대기
- 도메인 리로드 위험: T03 (interface 구현체 신규) 후 5초 대기
- Validator 실행: 모든 태스크 후 1회 일괄

## 7. 변경 이력
- YYYY-MM-DD: 초기 작성 (Phase 3 Breakdown)
```

### Step 4 — 태스크별 세부 문서 작성

각 `_NN_<area>.md` 템플릿:

```markdown
# T<NN>: <제목>

> **레이어**: <Data/Logic/UI/Test>
> **의존성**: T<MM>, T<KK> (또는 없음)
> **예상 시간**: N분 / 1 commit
> **PRD 출처**: <system>_v2.md §<섹션>

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/<폴더>/<File>.cs`

**클래스 명세**:
```csharp
// 1줄 요약 + §15 정합 (예: SOLID-O, 100줄 이내)
public sealed class BossPatternFactory {
    public static IBossPhase Create(BossPhaseId id, EnemyController enemy) {...}
    // 메서드 명세
}
```

**예상 라인**: ~30 (100줄 이내 ✅)

### 1.2 기존 파일 수정

**경로**: `<File>.cs`

**변경**:
- `<메서드>` 호출 신규 추가
- switch → Factory 호출로 대체

## 2. 검증 기준

### 2.1 컴파일
- `mcp__UnityMCP__refresh_unity` 후 errors=0
- 5초 대기 (도메인 리로드)

### 2.2 Editor Test
- (Test 태스크가 아니면 N/A)
- (Test 태스크면) `BossPatternFactoryTests` 5+ NUnit

### 2.3 ProjectValidator
- 신규 warnings 0 (200줄+ 검사 통과)

### 2.4 회귀
- 기존 EquipmentPatternFactory 동작 변경 X
- BossEncounterRunner 호환

## 3. commit 메시지

```
<system> Sprint <N>.<M> — <작업 한 줄> (<N>/<M>)

[변경]
  - <파일> 신규 (NN 라인)
  - <파일> 수정 (메서드 X)

[검증]
  - 컴파일 errors=0
  - Editor Test PASS (X/X)
  - ProjectValidator warnings 변동 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 4. 관련 문서

- PRD §<섹션>
- 디자인 §<섹션>
- decisions D<번호> (해당 시)

## 5. 알려진 위험

- <위험 1> + 완화
```

### Step 5 — 완료 보고

```
✅ <system> Breakdown 완료

[분해 결과]
  - 총 N 태스크 / X commit
  - 4 레이어: Data N / Logic M / UI K / Test L
  - 병렬 가능: G 그룹

[산출 파일]
  - <system>_breakdown_00.md (개요 + 의존성)
  - <system>_breakdown_01~NN_*.md (각 태스크)

[다음 단계]
  → /prd-contract --prd <v2>  (Sprint Contract — 검증 기준 정의)
  또는 /prd-pipeline 자동 진행
```

## 5. 사용 패턴

| 발화 | 동작 |
|---|---|
| "PRD 분해" | 직전 v2 PRD 자동 매칭 |
| "<system> 분해" | 시스템 매칭 |
| "/prd-breakdown --prd 5001" | 명시 번호 |
| "범위 재작성 5001_03~05" | `--rewrite` |
| "/imb 5001" | 약칭 (Clayve 호환) |

## 6. Little Hero 컨텍스트 규칙

### 6.1 폴더 구조 (CLAUDE.md §3)

신규 파일은 다음 폴더 중 하나:
- `Scripts/Core/` — Manager / Pool / Save / EventBus
- `Scripts/Core/Events/` — SO Event Channel
- `Scripts/Data/` — ScriptableObject 클래스
- `Scripts/Editor/` — Tools 메뉴 + Validator + Simulator
- `Scripts/Gameplay/<영역>/` — Player / Equipment / Enemies / Waves / Combat / Items
- `Scripts/UI/` — uGUI Panel
- `Tests/Editor/` — NUnit
- `Data/` — SO 인스턴스 (.asset)

### 6.2 SO 마이그레이션 패턴

신규 필드 추가 시:
1. 클래스에 SerializeField 추가
2. Editor/Tools 메뉴에 일괄 마이그레이션 (예 `Tools/LittleHero/Migrate Boss SO`)
3. `AssetDatabase.SaveAssets()` 호출
4. Validator 룰 추가 (필드 != default 검사)

### 6.3 Unity MCP 안전 패턴

각 태스크 commit 사이:
1. `mcp__UnityMCP__refresh_unity` (사용 시)
2. `editor_state.isCompiling == false` 폴링 또는 5~10초 대기
3. `mcp__UnityMCP__read_console` errors 체크
4. errors=0 확인 후 다음 태스크

### 6.4 신규 클래스 100줄 룰 (§15.6)

breakdown 단계에서 미리 추정:
- 메서드 수 × 평균 라인 = 추정
- 100줄+ 추정 시 — Component 분리 (예: Model + View 또는 Service + Helper)
- breakdown 문서에 "**예상 라인**" 명시 필수

## 7. 원칙

- **1 문서 = 1 commit** — 태스크 단위 명확
- **의존성 명시** — 병렬 가능성 표시
- **§15 정합 사전 검증** — breakdown 단계에서 100줄 룰 추정 + Strategy 강제
- **Unity MCP 안전** — 각 태스크 사이 도메인 리로드 회피
- **Test 분리 태스크** — 5+ NUnit 명시 (단위 / 정적 헬퍼 / SO 검증)

## 8. Workflow 위치

```
/prd-mediator → /prd-breakdown ← 현재
                      ↓
            구현 태스크 N 문서
                      ↓
              /prd-contract (Phase 4 — 검증 기준)
                      ↓
                    구현 (Phase 5)
                      ↓
              /prd-evaluate (Phase 6 — Contract 검증)
```

## 9. 출처

- Clayve `prd-implementation-breakdown` 4 레이어 패턴
- CLAUDE.md §3 폴더 + §4 아키텍처 + §15 OOP
- 5001 PRD 8 commit Sprint 분해 → breakdown 8 태스크 매핑
- weapon_v2 12 commit 시연 (Sprint 와 commit 1:1 또는 1:N 매핑 패턴)
