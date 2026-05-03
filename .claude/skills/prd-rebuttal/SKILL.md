---
name: prd-rebuttal
description: |
  Little Hero PRD 리뷰 3단계 중 **Phase 2.2 Rebuttal** — prd-critiquer 가 만든 critical_review 의 토론 포인트에 우선순위별 반박/수용을 수행하고, discussion_topics(미해결) 와 PRD v2 초안(수용 반영) 을 분리 산출한다.
  Anthropic PGE 패턴의 Planner 강화 단계. CLAUDE.md §15 OOP 정책 위반 지적은 반드시 수용, PG-12 톤 위반도 수용. 디자인 옵션 변경·Sprint 분해 조정은 discussion_topics 로 사용자 결정 유보.
  반드시 사용해야 하는 상황: "rebuttal 실행", "critique 반박", "비평 반박", "토론 주제 추출", "수용 거부 결정", "discussion topics 생성", "/prd-rebuttal", "<system> 반박 검토", "PRD v2 초안 생성", "비평 응답 작성", "rebuttal_agent" 발화. critique.md 가 이미 있는데 다음 단계가 모호한 모든 상황에서 적극 활성화.
---

# PRD Rebuttal — Little Hero 비평 반박

> **목적**: critique.md 의 각 Discussion Point 에 우선순위별 반박/수용 후, **수용된 항목은 PRD v2 초안에 반영**·**미결 항목은 discussion_topics 로 분리**.
> Clayve `prd-rebuttal-agent` 의 Unity 게임 개발 특화 변형 (PGE 패턴 Planner 강화).
>
> **저장 위치 (5001/5002 정착 패턴)**:
> - 입력 PRD: `docs/prd/<번호>/<system>_prd.md`
> - 산출 (rebuttal / v2_prd 초안 / discussion_topics): 모두 같은 `docs/prd/<번호>/` 폴더
> - 디자인 컨텍스트: `docs/prd/<system>.md` (prd/ 루트)
> - 번호는 시스템 prefix 4자리 자동 추출, 폴더 미존재 시 `mkdir -p`

## 0. 입력

- `--prd <path>` (Required): 원본 PRD 파일 (예 `docs/prd/5001/5001_boss_system_v2_prd.md`)
- `--crv <path>` (Optional): critical_review 파일. 생략 시 자동 탐색:
  1. `<prd_dir>/<prd_basename>_critique.md`
  2. `<prd_dir>/<prd_basename>_critical_review.md`
  3. 둘 다 없으면 → `prd-critiquer` 자동 호출 후 진행
- `--no-dis` (Optional): discussion_topics 분리 비활성화 (기본 활성화)
- `--pri <high|medium|low|all>` (Optional): 우선순위 필터

## 1. 출력 (3 파일)

`docs/prd/<번호>/` (PRD 디렉토리) 내:

| 파일 | 생성 조건 |
|---|---|
| `<prd>_rebuttal.md` | 항상 — 반박 요약 + Detailed Responses |
| `<prd>_v2_prd.md` | 항상 — 수용 항목 반영된 PRD 초안 (mediator 가 최종 확정) |
| `<prd>_discussion_topics.md` | 기본 — 미결 항목만 추출 (`--no-dis` 시 생략) |

## 2. 워크플로 (4 단계)

### Step 1 — 컨텍스트 로드 (병렬 Read)

- 원본 PRD 전체
- critical_review 전체
- `CLAUDE.md` §15 (OOP 정책)
- `docs/prd/<system>.md` (디자인 — 옵션 비교 컨텍스트, prd/ 루트)

### Step 2 — 우선순위별 대응 깊이

| Priority | 대응 방식 |
|---|---|
| **High** | 상세 분석 + 구체적 액션 플랜 + CLAUDE.md §15 근거 인용 + 반영 시 PRD diff 명시 |
| **Medium** | 명확한 방향 + 추가 논의 필요 시 명시 |
| **Low** | 수용/거부 간단 표기 + 향후 반영 메모 |

### Step 3 — 핵심 판단 기준 (Little Hero)

| 비평 유형 | 기본 판정 | 근거 |
|---|---|---|
| **§15.1 SRP 위반 (1 클래스 다중 책임)** | 수용 | 100줄 룰 자동 위반 가능 |
| **§15.1 OCP 위반 (switch 다중 분기 신규)** | 수용 | Strategy/Factory 강제 |
| **§15.4 SO 우선 위반 (Magic Number/hardcode)** | 수용 | SO 또는 const 강제 |
| **§15.6 100줄 룰 위반 (신규 클래스 100줄+ 추정)** | 수용 | Component 분리 강제 |
| **§15.7 DI 위반 (Singleton 직접 참조)** | 수용 | Initialize 명시 주입 |
| **§15.11 안티 패턴 (public 필드, Update in 1500, Instantiate in Update)** | 수용 | 무조건 수정 |
| **PG-12 톤 위반 (잔인·피·끔찍)** | 수용 | CLAUDE.md §2 절대 원칙 |
| **성능 cap 300 / GC 50KB 위반 가능성** | 수용 | PIVOT v4 §7 목표 |
| **Sprint 분해 작업량 불균형** | Discuss | 사용자 결정 (통합/분리/유지) |
| **디자인 옵션 변경 권장 (A→B 등)** | Discuss | 사용자 결정 |
| **수치 밸런스 조정 (1.3× → 1.2×)** | Discuss | 사용자 결정 또는 SerializeField |
| **MiniBoss/ChapterBoss 등 일관성 검토** | Discuss | 사용자 디자인 의도 확인 |
| **단순 코딩 스타일 의견 (네이밍 등)** | Reject | §15.2 컨벤션 준수면 OK |

### Step 4 — 산출 3 파일 작성

#### 4.1 `<prd>_rebuttal.md`

```markdown
# <system> PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: YYYY-MM-DD

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| OOP §15.1 | switch 9 분기 신규 | High | Accepted | Strategy 패턴 도입 | ✅ |
| Sprint 분해 | Sprint 7+8 통합 권장 | Medium | Discuss | 사용자 결정 → discussion_topics | △ |
| ...

## 2. 품질 메트릭

- 총 Discussion Points: N
- Accepted: M (P%)
- Rejected: K (Q%)
- Discuss (사용자 결정): J (R%)
- §15 정합률: X%

## 3. Detailed Responses

### 3.1 [Category — High Priority]

**Original Criticism**: <인용>

- **Rebuttal**: 수용/거부 + 근거 (CLAUDE.md §15.X 인용)
- **Proposed Action**: 구체적 변경 (파일 + 메서드 수준)
- **PRD Impact**: §N.M 수정 / §N.M 추가 / Sprint X 변경
- **Little Hero Context**: <기존 시스템 정합 확인>

### 3.2 [Category — Discuss (Medium)]

**Original Criticism**: <인용>

- **Status**: Discuss → discussion_topics.md 로 이동
- **이유**: 사용자 결정 사안 (옵션 A/B/C 선택)
- **Current Position**: PRD 그대로 진행 시 영향
- **Resolution Path**: mediator 단계에서 결정

## 4. PRD v2 적용 변경 요약

(수용 항목만 v2_prd.md 에 반영. 미결은 v2 미반영, mediator 후 최종.)

- §3.1 BossPatternFactory 신규 (High accepted)
- §4.2 IBossPhase 인터페이스 (High accepted)
- Sprint 5: NUnit Test 13개 명시 (Medium accepted)

## 5. 변경 이력
- YYYY-MM-DD: 초기 작성 (Phase 2.2 Rebuttal)
```

#### 4.2 `<prd>_discussion_topics.md`

```markdown
# <system> PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.

## High Priority Items

(없으면 생략)

## Medium Priority Items

### D1. <Topic 제목>

- **Original Criticism**: <critique 의 해당 항목 요약>
- **Current Position**: PRD 그대로 진행하면 X
- **Discussion Points**:
  - 옵션 A: <설명> — 장점 / 단점
  - 옵션 B: <설명> — 장점 / 단점
  - 옵션 C: <설명> — 장점 / 단점
- **Proposed Resolution Path**: <Claude 권장 + 근거>
- **PRD Impact**: §N.M 수정 / Sprint X 영향

### D2. ...

## Low Priority Items

(있으면)

## 요약
- Total Items: N
- High: 0 / Medium: M / Low: K
- 권장 mode: ai-assisted (Claude 추천 + 사용자 확인)
```

#### 4.3 `<prd>_v2_prd.md` (PRD 초안)

원본 PRD 복제 + Accept 된 변경만 반영. **Discuss 항목은 미반영** (mediator 가 최종 확정).

상단에 표기:
```markdown
> **PRD v2 초안 (Rebuttal 단계)**
> 원본: <prd> · Rebuttal: <rebuttal.md>
> 미결 항목 N건 → discussion_topics.md → mediator
> 최종 v2: mediator 단계에서 확정.
```

## 3. CLAUDE.md §15 자동 검증 (정적)

PRD 텍스트에서 다음 키워드 발견 시 — **반박이 아닌 수용** 처리:

| 키워드 / 패턴 | 자동 수용 + 수정 지시 |
|---|---|
| `public float damage` | SerializeField + private + readonly property |
| `switch (X)` (9 분기 이상) | Strategy/Factory 패턴 |
| `Update()` in 1500+ 객체 | 단일 Manager Update + List |
| `Instantiate()` in Update/Tick | ObjectPool |
| `GameManager.Instance.X` (Singleton 직접) | Initialize 명시 주입 |
| `new List<T>()` in hot loop | 정적 buffer |
| 신규 클래스 100줄+ 추정 | Component 분리 |

## 4. 사용 패턴 (사용자 발화)

| 발화 | 동작 |
|---|---|
| "rebuttal 실행" | 직전 critique 자동 매칭 + 3 파일 산출 |
| "<system> 반박 검토" | 시스템 매칭 후 진행 |
| "/prd-rebuttal --prd <path>" | 명시 경로 |
| "discussion topics 추출" | rebuttal + topics 산출 (v2 PRD 생략) |
| "high priority 만 반박" | `--pri high` 동일 효과 |

## 5. 출력 보고

```
✅ <system> Rebuttal 완료

[Decisions]
  - Accepted (수정 반영): N건
  - Discuss (사용자 결정 필요): M건
  - Rejected: K건

[산출 파일]
  - <prd>_rebuttal.md (반박 + 근거)
  - <prd>_v2_prd.md (Accept 반영 초안)
  - <prd>_discussion_topics.md (미결 M건)

[다음 단계]
  → /prd-mediator --dis <topics> --mode ai-assisted
  또는 mediator 가 자동 호출 (pipeline 모드)
```

## 6. 원칙

- **§15 위반은 무조건 수용** — Reject 불가, 사용자 결정 X
- **PG-12 톤 위반도 무조건 수용** — CLAUDE.md §2 절대 원칙
- **디자인 옵션은 항상 Discuss** — 사용자 결정 사안 (mediator 처리)
- **명확한 근거 인용** — CLAUDE.md §15.X / docs 경로
- **PRD v2 초안만 작성** — 최종은 mediator 후 (decisions 반영)
- **원본 PRD 보존** — 절대 수정 X (v2 가 별도 파일)

## 7. Workflow 위치

```
/prd-generator → /prd-critiquer → /prd-rebuttal ← 현재
                                       ↓
                            v2 PRD 초안 + discussion_topics
                                       ↓
                                /prd-mediator (Phase 2.3)
                                       ↓
                       _v2_prd.md (최종) + decisions.md
                                       ↓
                              /prd-breakdown / /prd-contract
```

## 8. 출처

- Clayve `prd-rebuttal-agent.md` 우선순위 대응 패턴
- CLAUDE.md §15 OOP 정책 (자동 수용 기준)
- 5001 critique → rebuttal 실증 (Discuss 3건 → topics 분리 사례)
