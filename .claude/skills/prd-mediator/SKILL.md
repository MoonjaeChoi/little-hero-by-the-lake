---
name: prd-mediator
description: |
  Little Hero PRD 리뷰 3단계의 **Phase 2.3 Mediator** — discussion_topics 의 각 미결 항목에 대해 결정을 내리고 최종 PRD v2 + decisions 문서를 산출한다.
  3 모드 지원: user(사용자 전체 결정) / ai-assisted(Claude 추천 + 사용자 확인, 기본) / auto(Claude best practice 결정).
  사용자 결정 입력 형식: `D1=A, D2=옵션B 1.2x, D3=A` 또는 자연어 응답. 결정 → PRD 섹션 자동 diff + commit.
  반드시 사용해야 하는 상황: "mediator 실행", "결정 통합", "PRD v2 확정", "토론 결정", "discuss 항목 결정", "/prd-mediator", "<system> 결정", "D1=A, D2=B 같은 결정 입력", "PGE 파이프라인 mediation", "discussion_topics 처리". rebuttal 후 discussion_topics.md 가 있는데 다음 단계가 모호한 모든 상황에서 적극 활성화.
---

# PRD Mediator — Little Hero 토론 주제 결정 통합

> **목적**: discussion_topics.md 의 미결 항목 각각에 결정을 내리고, **decisions.md (결정 + 근거)** 와 **v2_prd.md (최종)** 을 산출.
> Clayve `prd-mediator.md` 의 Unity 게임 개발 특화 변형 (PGE 패턴 Planner 마무리).

## 0. 입력

- `--dis <path>` (Required): discussion_topics 파일
- `--prd-v2 <path>` (Optional): rebuttal 단계의 v2 PRD 초안. 미지정 시 자동 추론 (`<prd>_v2_prd.md`)
- `--mode <user|ai-assisted|auto>` (Optional): 결정 모드. 기본 `ai-assisted`
- `--priority <high|medium|low|all>` (Optional): 우선순위 필터
- `--decisions <text>` (Optional): 사용자 결정 인라인 입력 (예 `"D1=A, D2=옵션B 1.2x, D3=A"`)

## 1. 출력 (2 파일)

| 파일 | 용도 |
|---|---|
| `<prd>_decisions.md` | 결정 기록 + 근거 + PRD diff 명세 |
| `<prd>_v2_prd.md` (덮어쓰기) | 결정 반영된 **최종** PRD v2 |

> rebuttal 단계의 `_v2_prd.md` 초안을 mediator 가 덮어쓰기 — 한 파일이 단계별 진화. commit 으로 추적.

## 2. 모드별 특성

| 모드 | 소요 시간 | 사용자 관여 | 적합 상황 |
|---|---|---|---|
| **user** | 30~60분 | 전체 통제 | 핵심 시스템 (보스/가챠/스타포스 등) |
| **ai-assisted** (기본) | 5~15분 | 확인만 | 균형 잡힌 기본 — 시연·중간 시스템 |
| **auto** | <2분 | 최소 | 명확한 §15 best practice 존재 |

### 2.1 ai-assisted 동작

각 미결 항목에 Claude 가:
1. 옵션별 장단점 다시 제시 (discussion_topics 요약)
2. **권장 + 근거** 명시 (CLAUDE.md §15·기존 시스템 정합·데이터 기반)
3. 사용자 응답 대기 (간단 형식 OK: "1=A, 2=B" / "다 권장대로")
4. 응답 → 즉시 PRD 갱신

### 2.2 사용자 결정 입력 형식

세 가지 입력 모두 수용:

```
"D1=A, D2=옵션B 1.2x, D3=A"      # 컴팩트
"D1 그대로 진행, D2 1.2배만, D3 MiniBoss Phase 2 도입" # 자연어
"권장 그대로 진행"                  # 일괄 권장 채택
```

## 3. 워크플로 (5 단계)

### Step 1 — 컨텍스트 로드

병렬 Read:
- discussion_topics.md
- v2_prd.md (rebuttal 초안)
- 원본 PRD (변경 영향 추정)
- CLAUDE.md §15 (auto 모드 기준)

### Step 2 — 우선순위별 결정 진행

HIGH → MEDIUM → LOW 순. 각 항목:

| 모드 | 처리 |
|---|---|
| user | "D1: 옵션 A/B/C 중 선택?" 사용자 응답 대기 |
| ai-assisted | 권장 + 근거 제시 → 사용자 응답 대기 (확인/오버라이드) |
| auto | best practice 자동 결정 + decisions 기록 |

### Step 3 — 결정 → PRD diff 생성

각 결정마다 **PRD 의 정확한 위치 + 변경 내용** 명세:

```
D1 결정: Sprint 7+8 통합 (옵션 A 통합)
PRD 영향:
  - §1 Sprint 표 (8행 → 7행)
  - §6 Sprint 7 = changelog + CLAUDE.md (1 commit)
  - 총 commit 9 → 8, 시간 2h50m → 2h30m
```

### Step 4 — v2_prd.md 갱신

rebuttal 단계의 v2 초안에 결정 반영:
- 신규 섹션 추가
- 기존 §N.M 수정
- Sprint 표 갱신
- 의존성 그래프 재정렬 (필요 시)

상단 표기:
```markdown
> **PRD v2 (최종, Mediator 확정)**
> 원본: <prd> · Rebuttal: <rebuttal.md> · Decisions: <decisions.md>
> 결정 N건 반영. 다음 단계: /prd-breakdown
```

### Step 5 — decisions.md 작성

```markdown
# <system> PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 N건 결정 기록
> 모드: <mode> · 일시: YYYY-MM-DD

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| Medium | D1 | Sprint 7+8 통합 | 통합 (옵션 A) | Done |
| Medium | D2 | Phase 2 속도 1.3× | SerializeField (옵션 C) | Done |
| Medium | D3 | MiniBoss Phase 2 | PRD 그대로 (옵션 A) | Done |

## 2. Detailed Decisions

### 2.1 D1 — <Topic>

- **Decision**: <선택 옵션>
- **Mode**: ai-assisted (Claude 권장 + 사용자 확인)
- **Rationale**: <근거 — CLAUDE.md §15 또는 사용자 의도>
- **PRD Impact**:
  - §<섹션>: <변경 요약>
  - Sprint X: <변경>
  - commit X→Y, 시간 X→Y
- **Trade-off**: <대안 옵션 의 손실>

### 2.2 D2 — ...

### 2.3 D3 — ...

## 3. 통계

- Total Items: N
- AI Recommendations Followed: X / N
- User Overrides: K
- 모드: <mode>

## 4. v2_prd.md 변경 diff 요약

(rebuttal 단계 → mediator 최종 의 변경사항)

- Sprint 표: 8행 → 7행
- §6.7 통합 (changelog + CLAUDE.md)
- §3.2 Phase 2 속도 SerializeField (1.3f 기본값)
- §3.3 MiniBoss Phase 2 — 변경 X (그대로)

## 5. 변경 이력
- YYYY-MM-DD: 초기 작성 (Phase 2.3 Mediator)
```

## 4. 사용 패턴

| 발화 | 동작 |
|---|---|
| "mediator 실행" | 직전 discussion_topics 자동 매칭 |
| "<system> 결정" | 시스템 매칭 |
| "권장 그대로 진행" | ai-assisted 권장 일괄 채택 |
| "D1=A, D2=B, D3=A" | 사용자 결정 인라인 |
| "/prd-mediator --dis <path> --mode auto" | auto 모드 |
| "high priority 만 결정" | `--priority high` |

## 5. ai-assisted 권장 결정 휴리스틱

미결 항목별 Claude 권장 기준:

| 카테고리 | 권장 |
|---|---|
| **Sprint 통합/분리** | 통합 (작업량 < 30분 + docs 변경 동일) |
| **수치 밸런스 (1.3× vs 1.2× vs SerializeField)** | SerializeField (디자이너 후속 조정 가능, §15.4 SO 우선) |
| **일관성 (MiniBoss Phase 2 도입)** | PRD 그대로 (단순성 우선, 후속 사이클에서 도입 가능) |
| **신규 enum 분리 vs flag 추가** | enum (명료성, switch → Factory 정합) |
| **OOP §15 강화 옵션 (Test 추가, Validator 룰)** | 채택 (정합 검증 자동화) |
| **UI 작업 분리/통합** | 큰 작업(60분+) 은 분리, 작은 작업 통합 |

## 6. 출력 보고

```
✅ <system> Mediator 완료

[Decisions]
  D1 → <옵션>: <한줄 요약>
  D2 → <옵션>: <한줄 요약>
  D3 → <옵션>: <한줄 요약>

[모드] ai-assisted (Claude 권장 채택률 N/M)

[산출 파일]
  - <prd>_decisions.md (결정 + 근거)
  - <prd>_v2_prd.md (최종 — Sprint 7행, X commit, Y시간)

[v2 주요 변경]
  - <변경 1>
  - <변경 2>
  - <변경 3>

[다음 단계]
  → /prd-breakdown --prd <v2> (구현 분해)
  또는 /prd-pipeline 자동 진행
```

## 7. 원칙

- **사용자 결정 우선** — ai-assisted 도 항상 사용자 확인 단계 (auto 만 예외)
- **근거 인용 필수** — CLAUDE.md §15 / 기존 SO 분포 / 직전 시스템 정합
- **PRD diff 명시** — 각 결정 → §N.M 변경 / Sprint X 변경 / commit Y→Z
- **rebuttal 단계 v2 덮어쓰기** — 한 파일 단계별 진화 (commit 추적)
- **decisions.md 가 단일 진실원** — 향후 회고 시 의사결정 추적

## 8. Workflow 위치

```
/prd-critiquer → /prd-rebuttal → /prd-mediator ← 현재
                                       ↓
                         decisions.md + v2_prd.md (최종)
                                       ↓
                            /prd-breakdown (Phase 3)
                                       ↓
                            /prd-contract (Phase 4)
                                       ↓
                            구현 (Phase 5) → /prd-evaluate (Phase 6)
```

## 9. 출처

- Clayve `prd-mediator.md` 3 mode 결정 패턴
- CLAUDE.md §15 OOP 정책 (auto 모드 기준)
- 5001_boss_system_v2 critique 의 Discuss 3건 (D1/D2/D3 실증)
