---
name: prd-pipeline
description: |
  Little Hero **PGE 6-Phase 풀파이프라인** — 사용자 요청 하나로 Generate → Review(3단계) → Breakdown → Contract → Implement → Evaluate 까지 자동 실행한다. Anthropic Planner-Generator-Evaluator 패턴 적용 (구현 전 검증 기준 사전 정의 + 독립 검증 + 반복 루프).
  6 Phase: (1) prd-generator → (2) prd-critiquer/rebuttal/mediator 3단 → (3) prd-breakdown → (4) prd-contract → (5) 구현 (little-hero-dev) → (6) prd-evaluate (Contract 기준 검증, FAIL 시 Phase 5 재진입 max 2회). 세션 상태 추적, 재진입, 발화 매핑 강화.
  반드시 사용해야 하는 상황: "PRD 풀파이프라인", "PGE 파이프라인", "PRD 전체 진행", "prd-pipeline", "<system> 전체 진행", "<system> 풀세트", "<system> 디자인부터 검증까지", "review-breakdown-contract-implement-evaluate", "/prd-pipeline", "PRD 끝까지 진행", "<system> OOP 재설계 풀세트". 사용자가 시스템 단위 큰 작업의 자동화를 원할 때 적극 활성화.
---

# PRD Pipeline — Little Hero PGE 6-Phase 풀파이프라인

> **목적**: 사용자 요청 하나로 디자인 → PRD → 리뷰(3단) → 분해 → 검증계약 → 구현 → 검증 자동.
> **Anthropic PGE 패턴**: 구현 전 검증 기준 사전 정의 + 구현/검증 분리 + 반복 루프 (3 핵심 품질 향상 요소).
> Clayve `prd-pipeline` 5단계 + 자체 Generate Phase 1 → 6 Phase 변형.
>
> **저장 위치 (5001/5002 정착 패턴)**:
> - **디자인 1개**: `docs/prd/<system>.md` (시스템 색인, prd/ 루트)
> - **모든 Phase 산출**: `docs/prd/<번호>/<system>_*.md` 하위 폴더 (PRD v1 / critique / rebuttal / discussion / decisions / v2 / breakdown_NN / contract / checklist / evaluation)
> - 번호 자동 추출 (시스템 prefix 4자리 숫자), 폴더 미존재 시 `mkdir -p` 자동 생성
> - 모든 6 Phase 자매 스킬 (generator/critiquer/rebuttal/mediator/breakdown/contract/evaluate) 가 같은 번호 폴더 공유

## 0. 6 Phase 흐름

```
사용자 요청
  ↓
[Phase 1] Generate    → <system>.md + <system>_prd.md
  ↓
[Phase 2] Review (3단)
  2.1 Critique  → <system>_critique.md (Accept/Reject/Discuss)
  2.2 Rebuttal  → <system>_rebuttal.md + _v2_prd.md (초안) + _discussion_topics.md
  2.3 Mediator  → <system>_decisions.md + _v2_prd.md (최종)
  ↓
[Phase 3] Breakdown   → <system>_breakdown_00~NN.md (구현 태스크 N개)
  ↓
[Phase 4] Contract    → <system>_contract.md + _contract_checklist.md (검증 기준)
  ↓
[Phase 5] Implement   → 코드 + SO + Sprint commit (little-hero-dev 활용)
  ↓
[Phase 6] Evaluate    → <system>_evaluation.md (Contract 기준 PASS/FAIL)
  ↓ FAIL
  └─ Phase 5 재진입 (max 2회 반복) → Phase 6 재검증
```

## 1. 입력

```bash
# 전체 6 Phase 자동 (가장 흔한 사용)
"<system> 시스템 OOP 재설계"

# 또는 명시적
/prd-pipeline --system <name>
/prd-pipeline --prd docs/prd/<번호>/<system>_prd.md

# 특정 Phase 부터
/prd-pipeline --from review        # Phase 2 부터 (PRD 가 있을 때)
/prd-pipeline --from breakdown     # Phase 3 부터 (v2 PRD 가 있을 때)
/prd-pipeline --from contract      # Phase 4 부터
/prd-pipeline --from implement     # Phase 5 부터 (Contract 가 있을 때)
/prd-pipeline --from evaluate      # Phase 6 만

# 특정 Phase 까지
/prd-pipeline --to review          # Phase 1+2 만
/prd-pipeline --to contract        # Phase 1~4 만 (구현은 별도)

# 모드 — 사용자가 --mode 명시 안 하면 항상 ai-assisted 자동 적용 (Phase 2.3 mediator 단계 토론·결정 과정)
/prd-pipeline --mode ai-assisted   # ★ 기본값 (명시 안 해도 자동) — Claude 추천 + 사용자 응답 대기
/prd-pipeline --mode user          # 사용자 전체 결정 (Claude 권장 X — 항목별 입력 받음)
/prd-pipeline --mode auto          # Claude best practice 자동 (사용자 확인 X — 빠른 진행)

# 반복 루프
/prd-pipeline --iterate 2          # Phase 6 FAIL 시 재진입 횟수 (기본 2)

# 레거시 (Contract + Evaluate 없이 — 5001 시연 직전 모드)
/prd-pipeline --legacy             # Phase 1+2+3+5 만
```

## 2. Phase 별 동작 + 스킵 조건

### Phase 1 — Generate (prd-generator)

**호출 조건**: `<system>.md` + `<system>_prd.md` 둘 중 하나라도 없음

**산출**:
- `<system>.md` (디자인)
- `<system>_prd.md` (Sprint PRD)

**스킵 조건**:
- 두 파일 모두 존재 + `--from review` 미지정 → 사용자 확인 후 스킵
- `--from review/breakdown/contract/implement/evaluate` → 자동 스킵

### Phase 2 — Review (3단)

#### 2.1 Critique (prd-critiquer)

**호출 조건**: `<system>_critique.md` 없음 또는 PRD 가 critique 보다 신규

**산출**: `<system>_critique.md` (5 카테고리 점수 + Accept/Reject/Discuss)

#### 2.2 Rebuttal (prd-rebuttal)

**호출 조건**: critique.md 존재 + rebuttal.md 없음 또는 신규 critique

**산출**: 3 파일
- `<system>_rebuttal.md`
- `<system>_v2_prd.md` (초안)
- `<system>_discussion_topics.md`

**자동 처리**:
- §15 위반 / PG-12 톤 위반 → 자동 수용 (Reject 불가)
- Sprint 분해·디자인 옵션 → discussion_topics 로 분리

#### 2.3 Mediator (prd-mediator)

**호출 조건**: discussion_topics.md 존재 + decisions.md 없음

**사용자 인터랙션** (사용자가 mode 옵션 명시 안 하면 항상 ai-assisted 적용 — `/prd-pipeline 5005` 같은 단순 호출도 자동 ai-assisted):
- `--mode ai-assisted` ★ **기본값 (명시 안 해도 자동)**: Claude 권장 + 사용자 응답 대기 (예: "권장안대로", "D1=A, D2=B" 등). Phase 2.3 mediator 단계의 토론·결정은 항상 이 모드로 진행
- `--mode auto`: 자동 결정 (사용자 확인 X, 빠른 진행)
- `--mode user`: 항목별 사용자 입력 (Claude 권장 X)

**사용자 결정 입력**:
```
"D1=A, D2=옵션B 1.2x, D3=A"     # 컴팩트
"권장 그대로 진행"                # 일괄 채택
"D1 통합, D2 SerializeField, D3 그대로"  # 자연어
```

**산출**:
- `<system>_decisions.md`
- `<system>_v2_prd.md` (최종 — 초안 덮어쓰기)

**스킵 조건**: discussion_topics 항목 0건 → 즉시 v2 = 초안 확정

### Phase 3 — Breakdown (prd-breakdown)

**호출 조건**: `<system>_breakdown_00.md` 없음

**산출**:
- `<system>_breakdown_00.md` (개요 + 의존성 + 병렬 가능성)
- `<system>_breakdown_01~NN_*.md` (각 태스크 1 commit 단위)

**자동 진행** (사용자 인터랙션 X)

**스킵 조건**: breakdown_00.md 존재 + `--from breakdown` 미지정

### Phase 4 — Contract (prd-contract)

**호출 조건**: `<system>_contract.md` 없음

**산출**:
- `<system>_contract.md` (6 카테고리: C1 컴파일 / C2 Test / C3 Validator / C4 Reflection / C5 SO / C6 Play 시나리오)
- `<system>_contract_checklist.md` (Evaluator 자동/수동 체크리스트)

**자동 진행**

**스킵 조건**: contract.md 존재 + `--from contract` 미지정

### Phase 5 — Implement (Generator)

**호출**: `little-hero-dev` 스킬 또는 직접 구현

#### 5.1 태스크 사이클 (각 breakdown 문서마다 반복)

```
1. 태스크 N 시작
   - breakdown_NN.md 의 작업 항목 확인
   - 해당 파일 신규/수정
2. 컴파일 검증
   - mcp__UnityMCP__refresh_unity (사용 시)
   - 5~10초 대기 (도메인 리로드)
   - mcp__UnityMCP__read_console (errors)
   - errors=0 확인
3. 단위 검증 (Test 태스크면)
   - reflection 으로 Test 클래스 + 메서드 존재 확인
4. commit
   - breakdown_NN.md 의 commit 메시지 형식 사용
   - 단일 책임 commit
5. (선택) changelog 등재
   - changelog-manager 호출 (큰 시스템 도입 시)
6. 다음 태스크 진입
```

#### 5.2 Unity MCP 안전 패턴 (CLAUDE.md §13)

- **다중 execute_code + refresh_unity 연쇄 시 SIGSEGV 위험** → 한 사이클 분리
- 스크립트 편집 후 5~10초 대기 (도메인 리로드 안정화)
- 씬 저장 후 10초+ 대기
- 한 commit = 한 의미 단위 (회수 용이)

#### 5.3 컴파일 에러 처리

| 상황 | 동작 |
|---|---|
| errors=0 | 다음 태스크 진입 |
| errors=N (1차) | 자동 수정 시도 (오타·import 누락) |
| errors>0 (2차) | 사용자 보고 + 사이클 일시 중단 |

#### 5.4 병렬 가능 태스크

breakdown_00.md 의 의존성 그래프 따라 병렬 표시. 단, 컴파일 의존 시 순차.

### Phase 6 — Evaluate (Evaluator)

**호출**: `prd-evaluate` 스킬 (Contract 기준)

#### 6.1 자동 검증 (Contract C1~C5)

```
C1 컴파일      — refresh + isCompiling=false + errors=0
C2 Editor Test — Test Runner / reflection 으로 NUnit PASS 확인
C3 Validator   — Tools/LittleHero/Validate Project 실행
C4 Reflection  — Type/Method/Field 존재 정적 검증
C5 SO 검증     — AssetDatabase.LoadAllAssetsAtPath 필드 검사
```

#### 6.2 수동 검증 (Contract C6)

```
[Play 검증 가이드]
시나리오 1, 2, 3 — 사용자가 Play Mode 에서 실행 후 결과 보고
```

#### 6.3 판정

| 결과 | 동작 |
|---|---|
| **PASS** (자동 모두 ✅ + 시나리오 1+2 ✅) | changelog 등재 + 사용자 Play 시나리오 3 보고 대기 (선택) |
| **FAIL — 자동 검증 1개라도 ❌** | 수정 지시 → Phase 5 재진입 (반복 N/2회) |
| **FAIL — 반복 소진 (2회)** | 사용자 보고 + 수동 검토 요청 |

#### 6.4 산출

- `<system>_evaluation.md` (Pass/Fail 매트릭스 + 수정 지시 — FAIL 시)
- `<system>_evaluation_iter1.md`, `_iter2.md` (반복 시 회차별)

## 3. 세션 상태 추적

각 Phase 시작/완료 시 콘솔 출력:

```
[Pipeline 5001 — Phase 3/6 in progress] Breakdown
  ✓ Phase 1 Generate (5001_v1.md, _prd.md)
  ✓ Phase 2 Review  (5001_critique.md, _rebuttal.md, _decisions.md, _v2_prd.md)
  → Phase 3 Breakdown (in progress)
  ⏸ Phase 4 Contract
  ⏸ Phase 5 Implement
  ⏸ Phase 6 Evaluate
```

재진입 시 자동 감지:
1. PRD/디자인 파일 존재 → Phase 1 완료
2. critique + rebuttal + decisions + v2 → Phase 2 완료
3. breakdown_00 → Phase 3 완료
4. contract → Phase 4 완료
5. 코드 commit log 분석 → Phase 5 진척
6. evaluation → Phase 6 완료/실패

## 4. 사용자 발화 매핑

| 발화 | 해석 | 동작 |
|---|---|---|
| "<system> OOP 재설계" | 전체 6 Phase | Phase 1~6 자동 |
| "<system> 디자인 + PRD" | Phase 1 만 | `--to generate` (1 Phase) |
| "<system> 리뷰" | Phase 2 만 | `--from review --to review` |
| "<system> Sprint 1+2+3 일괄" | Phase 5 부분 | `--from implement` + 태스크 명시 |
| "<system> 검증" | Phase 6 만 | `--from evaluate` |
| "<system> D1=A, D2=B" | Phase 2.3 결정 입력 | mediator 호출 + 인라인 결정 |
| "<system> 후속 Sprint" | 후속 PRD | Phase 1~3 (후속), Phase 5 직행 |
| "<system> 권장 그대로" | mediator 일괄 | mediator ai-assisted 권장 채택 |
| "/prd-pipeline <system>" | 명시 호출 | 전체 6 Phase |
| "5001 풀세트" | 전체 6 Phase | Phase 1~6 자동 |

## 5. 출력 보고 (완료)

```
═══════════════════════════════════════════
<system> Pipeline Complete (PGE 6-Phase)
═══════════════════════════════════════════

Phase 1 — Generate:
  ✓ 디자인 <system>.md (X 라인)
  ✓ PRD    <system>_prd.md (Y 라인, Z Sprint)

Phase 2 — Review (3단):
  ✓ Critique  점수 8.6/10, Accept N / Reject 0 / Discuss M
  ✓ Rebuttal  Accepted N건 / Discuss M건 분리
  ✓ Mediator  ai-assisted, 결정 M건, v2 PRD 확정

Phase 3 — Breakdown:
  ✓ N 태스크 / X commit
  ✓ 4 레이어 (Data/Logic/UI/Test)
  ✓ 병렬 가능 G 그룹

Phase 4 — Contract:
  ✓ 6 카테고리 검증 기준 (C1~C6)
  ✓ Play 시나리오 3
  ✓ Evaluator 체크리스트

Phase 5 — Implement:
  ✓ commit X 생성 / Y시간
  ✓ 컴파일 errors=0 (각 사이클)
  ✓ Unity MCP 안전 (도메인 리로드 회피)

Phase 6 — Evaluate:
  ✓ C1 컴파일 errors=0
  ✓ C2 Editor Test M/M PASS
  ✓ C3 Validator warnings 변동 ±0
  ✓ C4 Reflection 7 Type 모두 통과
  ✓ C5 SO 검증 (BossLootTable 4 + EnemyData 51)
  ⏸ C6 Play 시나리오 — 사용자 보고 대기 (시나리오 1, 2, 3)

종합: PASS (자동 모두 ✅, 수동 보고 대기)

[다음]
  - 사용자 Play 시나리오 1, 2, 3 진행
  - changelog 등재 (changelog-manager 자동)
  - CLAUDE.md §11 마일스톤 갱신 (큰 시스템 도입 시)
  - /prd-pipeline <next system>
═══════════════════════════════════════════
```

## 6. 에러 처리

| 상황 | 동작 |
|---|---|
| PRD 파일 없음 (Phase 1 미실행) | Phase 1 자동 호출 |
| 디자인+PRD 둘 다 존재 | 사용자 확인 후 스킵 또는 갱신 |
| Discuss 항목 미해결 | mediator 진입 → 사용자 결정 대기 (Phase 3 진입 안 함) |
| breakdown 의존성 불명확 | Claude 검토 → 사용자 결정 요청 |
| Contract 누락 (--from implement) | Phase 4 자동 호출 |
| 컴파일 에러 (Phase 5) | 1차 자동 수정, 2차 사용자 보고 |
| Test FAIL (Phase 6) | 수정 지시 → Phase 5 재진입 (max 2회) |
| Validator 신규 위반 | 보고 + 사용자 결정 (수정 / 보류) |
| 반복 소진 | 수동 검토 요청 + 부분 산출 보존 |

## 7. changelog + CLAUDE.md 자동 등재

### 7.1 각 Phase 완료 시 (Phase 5 sprint 단위)

`changelog-manager` 자동 호출:
- 직전 commit 분석
- 시스템 도입 / 사용자 결정 / 큰 fix 인지 판단
- `changelog_v1.md` 항목 추가
- CLAUDE.md §14 5건 maintain (직전 5건만)

### 7.2 전체 PRD 완료 시

묶음 changelog 항목 (예 "5001 boss_system_v2 — BossTier enum + IBossPhase + Factory + LootTable, 8 commit").

### 7.3 큰 시스템 도입 시

CLAUDE.md §11 마일스톤 행 추가 (예 "boss_system_v2 — 4 Tier + Phase State + Factory ✅ 2026-05-04").

## 8. PGE 패턴 핵심 (Anthropic 연구)

### 8.1 3 가지 품질 향상 요소

| 요소 | 본 파이프라인 |
|---|---|
| **구현 전 검증 기준 사전 정의** | Phase 4 Contract — 6 카테고리, Pass/Fail 자동 |
| **구현 (Generator) 과 분리된 독립 평가 (Evaluator)** | Phase 5 Implement vs Phase 6 Evaluate. 같은 Claude 라도 Contract 가 단일 진실원 → 자기 평가 편향 제거 |
| **반복 루프** | Phase 6 FAIL → Phase 5 재진입 (max 2회) |

### 8.2 Generator/Evaluator 책임 분리

| 책임 | Generator (Phase 5) | Evaluator (Phase 6) |
|---|---|---|
| 코드 생성 | ✅ | ❌ |
| 검증 기준 작성 | ❌ | ❌ (Contract 가 사전 작성) |
| Pass/Fail 판정 | ❌ | ✅ |
| 수정 지시 | ❌ | ✅ |
| 코드 수정 | ✅ (Evaluator 지시 받아) | ❌ |

### 8.3 Context Reset (반복 간)

반복 시 이전 컨텍스트 전체가 아닌 **실패 항목 + 수정 지시만** 전달 → noise 감소.

## 9. 원칙

- **기본 모드 = ai-assisted** — 사용자가 `--mode` 옵션을 명시하지 않으면 항상 `ai-assisted` 모드 적용. Phase 2.3 mediator 의 토론·결정 과정은 Claude 가 권장안을 제시하고 사용자 응답 (예: "권장안대로", "D1=A, D2=B", "권장 그대로 진행" 등) 을 대기. `auto` 모드 (사용자 확인 X) 는 명시 호출 시에만 사용.
- **사용자 결정 우선** — Phase 2.3 mediator 명시적 결정 후 진행
- **단일 책임 commit** — 한 commit = 한 의미 단위 (Sprint × commit 분해)
- **OOP §15 정합** — Phase 2 Review 자동 검증 (rebuttal 수용 강제)
- **PG-12 톤 절대 원칙** — Phase 2 Review 자동 검증
- **회귀 방지** — Phase 6 Editor Test + Validator + Play 검증 필수
- **CLAUDE.md changelog** — Phase 5 sprint 단위 등재 + 5건 maintain
- **Unity MCP 안전 패턴** — Phase 5 refresh 사이 5~10s, 도메인 리로드 회피
- **Contract 단일 진실원** — Phase 6 검증 기준은 Contract 만 (PRD 직접 추출 X, scope creep 방지)

## 10. 6 스킬 통합

```
prd-generator       → Phase 1
prd-critiquer       → Phase 2.1
prd-rebuttal        → Phase 2.2
prd-mediator        → Phase 2.3
prd-breakdown       → Phase 3
prd-contract        → Phase 4
little-hero-dev     → Phase 5 (활용)
prd-evaluate        → Phase 6
prd-pipeline        → 본 스킬 (전체 통합 + 세션 상태)
changelog-manager   → 자동 등재 (Phase 5/완료 시)
```

## 11. 적합/부적합

| 적합 | 부적합 |
|---|---|
| 신규 시스템 OOP 재설계 (5001 보스, weapon_v2 등) | 단순 버그 수정 |
| 큰 PRD (5+ Sprint) | 1 commit 핫픽스 |
| 다수 Discuss 항목 | 명확한 단일 작업 |
| 디자인 옵션 비교 필요 | 이미 결정된 작업 |

작은 작업은 `little-hero-dev` 또는 직접 구현 권장.

## 12. 출력 보고 (단계별 진행 시)

각 Phase 완료 시:

```
✅ Phase 2.2 Rebuttal 완료
  - rebuttal.md (NN 라인)
  - v2_prd 초안
  - discussion_topics M건

[Phase 2/6 진행] 다음: 2.3 Mediator
  → /prd-mediator (자동 호출 — ai-assisted)
  → 사용자 결정 입력 형식: "D1=A, D2=B, D3=A"
```

## 13. 출처

- Clayve `prd-pipeline.md` 5단계 PGE 패턴
- Anthropic Planner-Generator-Evaluator 연구 (3 품질 향상 요소)
- CLAUDE.md §15 OOP 정책 + §13 Unity MCP 안전 패턴
- 5001_boss_system_v2 시연 (Phase 1+2.1 사례)
- weapon_v2 12 commit 시연 (Phase 5 Sprint 사이클 사례)
- prd-* 자매 스킬 (generator/critiquer/rebuttal/mediator/breakdown/contract/evaluate)
