# 5022 PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> **합계**: 3 (High 1, Medium 2)

---

## High Priority Items

### D1. Sprint 1 임시 errors=0 위험

- **Original Criticism**: critique §4.1 — Sprint 1 commit 후 ~ Sprint 3 commit 사이 검증 룰 비어있는 상태. 회귀 발견 X.
- **Current Position**: PRD §3 의 Sprint 1 commit 메시지에 "분리 직후 검증 결과 errors=0/warnings=0 (Sprint 2/3 에서 룰 복원)" 명시.
- **Discussion Points**:
  - **옵션 A** (PRD 원안): Sprint 1 → Sprint 2 → Sprint 3 직렬 진행, 일시 위험 허용 (~1h45m 내 통과)
    - 장점: PRD 원안 유지, 회수 유연성 (Sprint 별 commit)
    - 단점: 1h45m 동안 회귀 검증 X
  - **옵션 B** ★ Claude 권장: Sprint 1 + Sprint 2 (간단 6) 통합 — Sprint 1.1 = Issue 추출, Sprint 1.2 = orchestrator + 6 Rule 통합 commit
    - 장점: 항상 룰 작동 보장 (위험 영역 없음)
    - 단점: commit 통합 = 회수 유연성 약간 ↓
  - **옵션 C**: Sprint 1 의 ProjectValidator 슬림화 시 모든 11 룰을 한 번에 이동 (1 commit 으로 통째 분리)
    - 장점: 최단 시간 + 항상 정합
    - 단점: 큰 commit (Issue + 11 Rule + ProjectValidator 슬림 = 600+ 라인 변경) = 회수 어려움
- **Proposed Resolution Path** (Claude 권장):
  - 옵션 B 권장 — Little Hero 프로젝트는 단일 사용자 dev 라 push 부재로 일시 위험 작지만, "항상 검증 가능 상태" 가 더 좋은 안전망. commit 회수 유연성 손실은 미미 (1.1+1.2 = 2 commit 도 작은 단위).
- **PRD Impact**:
  - 옵션 A → Sprint 분해 변동 0
  - 옵션 B → Sprint 1+2 통합 → 합계 3 Sprint / 3 commit (Sprint 1.1 + 1.2 + Sprint 3 + Sprint 4)
  - 옵션 C → Sprint 분해 변동 → 합계 2 Sprint / 2 commit

---

## Medium Priority Items

### D2. Rule 통합 단위 (80~85L 권장 약간 초과)

- **Original Criticism**: critique §4.2 — EnemyRule (80L) / WeaponBossRule (85L) / PerformanceRule (85L) 가 §15.6 80L 권장 약간 초과 (100L 룰 충족).
- **Current Position**: PRD §4.1 의 11 Rule 분배 — 같은 카테고리 응집도 우선.
- **Discussion Points**:
  - **옵션 A** (PRD 원안): 11 Rule 통합 유지 (80~85L 일부)
    - 장점: 같은 카테고리 응집도 ↑, import overhead 작음
    - 단점: §15.6 80L 권장 약간 초과
  - **옵션 B**: 더 세분화 → 16 Rule 클래스 (각 30~50L)
    - 장점: 단일 책임 강화
    - 단점: 파일 수 +5, import overhead, ProjectValidator.Run() 의 호출 줄 16
  - **옵션 C** ★ Claude 권장: 통합 유지 (옵션 A) — 100L 룰 만족, 80L 은 권장. 추후 비대 시 분리 가능.
- **Proposed Resolution Path** (Claude 권장):
  - 옵션 C — 80L 은 권장 영역, 100L 룰 충족이 핵심. 같은 카테고리 응집도 우선.
- **PRD Impact**:
  - 옵션 A/C → 변동 0
  - 옵션 B → §4.1 Rule 분배 16 으로 재정의

### D3. 신규 NUnit 추가 여부

- **Original Criticism**: critique §4.3 — ProjectValidator 분리 자체 검증 NUnit 없음. 사용자 수동 비교 검증.
- **Current Position**: PRD 가 NUnit 회귀 0 만 명시. Sprint 4 = 사용자 비교.
- **Discussion Points**:
  - **옵션 A** (PRD 원안): NUnit 신규 X, 사용자 수동 비교
    - 장점: PRD 원안 유지, 시간 추가 0
    - 단점: 자동 회귀 안전망 부재
  - **옵션 B**: ProjectValidatorTests 신규 — Run 호출 + Issue 카운트 검증
    - 장점: 자동 회귀 안전망
    - 단점: Editor 어셈블리 reference 복잡 (NUnit 어셈블리 가 Editor 룰 호출)
  - **옵션 C**: Rule 별 NUnit (각 Rule 의 Validate 메서드 reflection 호출)
    - 장점: 분리 검증 자동
    - 단점: Editor 어셈블리 복잡 + NUnit 11+ 추가
  - **옵션 D** ★ Claude 권장: 옵션 A 채택 (PRD 원안) — 분리는 동작 변경 0 이므로 기존 NUnit 117 회귀 안전망 충분. NUnit 추가는 5024 (editor_test_expansion) 에서 검토.
- **Proposed Resolution Path** (Claude 권장):
  - 옵션 D — 5024 가 editor_test_expansion 별도 시스템. ProjectValidator NUnit 은 그쪽 scope 가 더 적합.
- **PRD Impact**:
  - 옵션 A/D → 변동 0
  - 옵션 B/C → Sprint 4 작업 +30~60분, NUnit 117 → 118+

---

## 요약

- Total Items: **3**
- High: **1** (D1) / Medium: **2** (D2, D3) / Low: **0**
- 권장 mode: **ai-assisted** (Claude 추천 + 사용자 응답 대기)
- 권장 답변: D1=B, D2=C (=A), D3=D (=A) — 위험 최소화 + PRD 원안 보존

---

## mediator 안내 — 결정 입력 형식

```
"D1=A, D2=A, D3=A"          ← 모두 PRD 원안
"D1=B, D2=C, D3=D"          ← Claude 권장 (위험 최소)
"권장 그대로 진행"            ← 일괄 권장 채택
"D1 통합, 나머지 그대로"      ← 자연어
```
