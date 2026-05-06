# 5021 PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> **합계**: 2 (High 1, Medium 1)

---

## High Priority Items

### D1. R3 옵션 — AtticChapterEntry vs WondrousMapPanel

- **Original Criticism**: critique §4.1 — Attic.unity 에 `WondrousMapNode` 10+ instance 발견. AtticChapterEntry 가 dead code 일 가능성.
- **Current Position**: PRD §3.1 의 R3 fix 안 (옵션 A: AtticChapterEntry._buttons[] wiring 복원) 이 production path 와 불일치할 가능성.
- **Discussion Points**:
  - **옵션 A** (PRD 원안): AtticChapterEntry._buttons[] wiring 복원 (legacy path 살림)
    - 장점: 단순 — 5020.2 패턴 답습. 1 챕터 진입 빠른 path 가 디버그/온보딩에 유용
    - 단점: dead code 살림 — WondrousMapPanel 와 path 이중화. Production 검증 영역 ↑
  - **옵션 B** ★ Claude 권장: AtticChapterEntry MonoBehaviour 비활성화 또는 GameObject 제거 + WondrousMapPanel 동작 확인 + 필요 시 35 노드 wiring 보강
    - 장점: production path 단일화, dead code 정리, §15.1 SRP 강화
    - 단점: WondrousMapPanel 35 노드 wiring 상태 검증 추가 (~30분), 대규모 wiring 회복 가능성
  - **옵션 C**: 둘 다 살림 (AtticChapterEntry 빠른 진입 + WondrousMapPanel 35 챕터 grid)
    - 장점: UX 옵션 ↑ (디버그·prod 둘 다)
    - 단점: 중복 wiring 비용, 어느 쪽이 정식인지 모호
- **Proposed Resolution Path** (Claude 권장):
  - 옵션 B 권장 — 5005 attic_decomposition_v1 정신 (production path 단일화) 정합. AtticChapterEntry 가 5020.1/5020.2 fix 시 발견된 dead code 패턴과 동일 cluster.
  - WondrousMapPanel 35 노드 wiring 상태는 사전 grep 또는 Inspector 점검으로 30분 이내 검증 가능.
- **PRD Impact**:
  - 옵션 A → Sprint 3 §4.2 R3 wiring (PRD 원안)
  - 옵션 B → Sprint 3 §4.2 재작성 — AtticChapterEntry 비활성화 + WondrousMapPanel 검증
  - 옵션 C → Sprint 3 작업 +30분 (둘 다 wiring)

---

## Medium Priority Items

### D2. Sprint 1 의 2 commit 분배

- **Original Criticism**: critique §4.2 — Sprint 1 표 의 2 commit 표기 (1/5 + 2/5) 와 §2 본문의 1 commit 메시지 명세 불일치.
- **Current Position**: PRD §1 표 = 2 commit, §2 commit 메시지 = 1 (단일).
- **Discussion Points**:
  - **옵션 A**: 2 commit 명확화 → ① CharacterPool.cs 클래스만 / ② CharacterPool_Default.asset 인스턴스 + ProjectValidator 룰
    - 장점: 회수 용이, 단일 책임 commit 정합
    - 단점: 2 commit 시간 분리 (각 ~15분)
  - **옵션 B**: 1 commit 통합 (Sprint 1 = 1 commit, 합계 4 commit)
    - 장점: 단순, 진행 빠름
    - 단점: SO 클래스 + 인스턴스 + Validator 룰이 한 commit
  - **옵션 C** ★ Claude 권장: PRD 그대로 (2 commit 의도 보존, commit 메시지 1번째 = SO 클래스 / 2번째 = SO 인스턴스+Validator)
    - 장점: 회수 용이 + PRD 표 정합
    - 단점: PRD §2 본문 수정 필요 (commit 메시지 2개로 분리 표기)
- **Proposed Resolution Path** (Claude 권장):
  - 옵션 C — PRD 표 의도 (2 commit) 보존 + §2 본문에 commit 메시지 2개 명시. 작은 분리는 회수 용이성 ↑.
- **PRD Impact**:
  - 옵션 A → Sprint 1 §2 본문에 commit 1, 2 명세 명확화 (~5분)
  - 옵션 B → Sprint 1 표 + 합계 4 commit 변경 (~5분)
  - 옵션 C → §2 commit 메시지 1, 2 분리 표기 (~5분)

---

## 요약

- Total Items: **2**
- High: **1** (D1 R3 옵션) / Medium: **1** (D2 Sprint 1 commit) / Low: **0**
- 권장 mode: **ai-assisted** (Claude 추천 + 사용자 응답 대기)
- 권장 답변: D1=B, D2=C — 둘 다 채택 시 +30~60분 추가 (옵션 B 의 WondrousMapPanel 검증 비용)

---

## mediator 안내 — 결정 입력 형식

```
"D1=A, D2=A"            ← 모두 PRD 원안 유지
"D1=B, D2=C"            ← Claude 권장 채택
"권장 그대로 진행"        ← 일괄 권장 채택
"D1=A 진행, D2 통합"     ← 자연어
```
