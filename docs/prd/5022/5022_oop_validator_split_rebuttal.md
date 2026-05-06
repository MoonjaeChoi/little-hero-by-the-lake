# 5022 PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude (자동) / **일시**: 2026-05-06
> 입력: `5022_oop_validator_split_critique.md` (점수 8.7/10)

---

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| Sprint 분해 | Sprint 1 임시 errors=0 위험 | High | **Discuss** | discussion_topics 이관 | △ |
| OOP §15.6 | Rule 통합 단위 (80~85L vs 분리) | Medium | **Discuss** | discussion_topics 이관 | △ |
| OOP §15.9 | NUnit 신규 추가 여부 | Medium | **Discuss** | discussion_topics 이관 | △ |
| OOP §15.6 자기위반 | ProjectValidator 650L → 분리 | (Accept) | Accepted | PRD 원안 유지 | ✅ |
| OOP §15.8 | Service Locator 정적 패턴 | (Accept) | Accepted | PRD 원안 유지 | ✅ |
| 시스템 통합 | 분리 전후 결과 동일 | (Accept) | Accepted | Sprint 4 검증 명시 | ✅ |
| PG-12 톤 | 영향 0 | (Accept) | Accepted | - | ✅ |
| 성능 | Editor 도구 — 런타임 0 영향 | (Accept) | Accepted | - | ✅ |

---

## 2. 품질 메트릭

- 총 Discussion Points: **3** (D1 Sprint 1 임시 errors=0, D2 Rule 통합 단위, D3 NUnit 신규)
- Accepted (수정 반영): **0** (PRD 원안 유지)
- Rejected: **0**
- Discuss (사용자 결정): **3** (100%)
- §15 정합률: **100%** (자동 수용 대상 0건 — PRD 가 이미 §15 정합)

---

## 3. Detailed Responses

### 3.1 [Sprint 분해 — High Priority]

**Original Criticism (D1)**: critique §4.1 — Sprint 1 commit 후 ~ Sprint 3 commit 사이 검증 룰이 비어있는 상태. 그 기간 회귀 발견 X.

- **Rebuttal**: **Discuss 로 이관** — 사용자의 위험 감수 vs 회수 유연성 trade-off.
- **Proposed Action**: `5022_discussion_topics.md` 의 D1 으로 분리.
- **PRD Impact**: 결정에 따라 Sprint 분해 재정의 (옵션 A: PRD 원안 / B: Sprint 1+2 통합 / C: 1 commit 통째 분리).
- **Little Hero Context**: 본 프로젝트가 단일 사용자 dev (push 부재) — 일시 위험 영향 작음. Sprint 1~3 ~1h45m 내 통과 시 회귀 가능성 낮음.

### 3.2 [OOP §15.6 — Medium Priority]

**Original Criticism (D2)**: critique §4.2 — EnemyRule (80L) / WeaponBossRule (85L) / PerformanceRule (85L) 가 80L 권장 약간 초과 (100L 룰은 충족).

- **Rebuttal**: **Discuss 로 이관** — 응집도 vs SRP 강화 trade-off.
- **Proposed Action**: `5022_discussion_topics.md` 의 D2 로 분리.
- **PRD Impact**: 결정에 따라 11 vs 16 Rule 클래스 (PRD §4.1 Rule 분배 재정의).
- **Little Hero Context**: 같은 카테고리 (Enemy/WeaponBoss/Performance) 내 Rule 들은 호출 cluster — 분리 시 import overhead 발생.

### 3.3 [OOP §15.9 — Medium Priority]

**Original Criticism (D3)**: critique §4.3 — ProjectValidator 분리 자체 검증 NUnit 없음. Sprint 4 비교 검증이 사용자 수동.

- **Rebuttal**: **Discuss 로 이관** — 자동 회귀 안전망 vs Editor 어셈블리 복잡도 trade-off.
- **Proposed Action**: `5022_discussion_topics.md` 의 D3 으로 분리.
- **PRD Impact**: 결정에 따라 Sprint 4 NUnit 0~5 추가.
- **Little Hero Context**: ProjectValidator 는 Editor 어셈블리 (Assembly-CSharp-Editor) — NUnit Test 어셈블리 (LittleHero.Tests.Editor) 와 reference 추가 필요.

---

## 4. PRD v2 적용 변경 요약

(자동 수용 0건 → v2 변경 = 0건. 단 헤더에 v2 표기 추가, Discuss 3건 mediator 후 최종 확정.)

원본 PRD (`5022_oop_validator_split_prd.md`) 와 v2 초안 (`5022_oop_validator_split_v2.md`) 본문 동일.

---

## 5. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.2 Rebuttal). Accepted 0 / Discuss 3 / Reject 0. PRD §15 정합 100% |
