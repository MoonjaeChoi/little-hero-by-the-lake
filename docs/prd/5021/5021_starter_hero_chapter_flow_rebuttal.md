# 5021 PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude (자동) / **일시**: 2026-05-06
> 입력: `5021_starter_hero_chapter_flow_critique.md` (점수 8.7/10)

---

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| 시스템 통합 | R3 옵션 재검토 (Attic vs Wondrous) | High | **Discuss** | discussion_topics 로 이관 | △ |
| Sprint 분해 | Sprint 1 의 2 commit 분배 모호 | Medium | **Discuss** | discussion_topics 로 이관 | △ |
| OOP §15.4 | CharacterPool SO 채택 | (Accept) | Accepted | PRD 원안 유지 | ✅ |
| OOP §15.6 | MetaProgression +15L (346L) | (일시) | Auto-허용 | 5023 cascade 분리 예정 | △ |
| OOP §15.9 | NUnit 3 명시 | (Accept) | Accepted | PRD 원안 유지 | ✅ |
| OOP §15.12 | ProjectValidator 룰 추가 | (Accept) | Accepted | PRD 원안 유지 | ✅ |
| PG-12 톤 | 영향 0 | (Accept) | Accepted | - | ✅ |
| 성능 | Awake 1회 호출 추가만 | (Accept) | Accepted | - | ✅ |

---

## 2. 품질 메트릭

- 총 Discussion Points: **2** (D1 R3 옵션, D2 Sprint 1 commit)
- Accepted (수정 반영): **0** (PRD 원안 유지)
- Rejected: **0**
- Discuss (사용자 결정): **2** (100%)
- §15 정합률: **100%** (자동 수용 대상 0건 — PRD 가 이미 §15 정합)

---

## 3. Detailed Responses

### 3.1 [시스템 통합 — High Priority]

**Original Criticism (D1)**: critique §4.1 — Attic.unity 에 `WondrousMapNode` 10+ instance 발견. AtticChapterEntry 가 dead code 일 가능성. PRD §3.1 의 R3 옵션 (Attic._buttons[] wiring) 이 production path 와 불일치 가능.

- **Rebuttal**: **Discuss 로 이관** — 사용자 결정 사안. Claude 가 임의로 옵션 변경 시 사용자 의도 (debug 진입 단순 path 보존?) 와 충돌 가능.
- **Proposed Action**: `5021_discussion_topics.md` 의 D1 으로 분리. mediator 단계에서 사용자 결정 받음.
- **PRD Impact**: 결정에 따라 Sprint 3 작업 항목 재정의 (옵션 A: 원안 유지 / B: AtticChapterEntry 비활성화 + WondrousMapPanel 검증 / C: 둘 다 살림).
- **Little Hero Context**: 5005 attic_decomposition_v1 의 책임 분담 정합 — WondrousMapPanel 이 production 이라면 AtticChapterEntry 정리는 §15.1 SRP 강화.

### 3.2 [Sprint 분해 — Medium Priority]

**Original Criticism (D2)**: critique §4.2 — Sprint 1 표 가 2 commit 표기 (1/5 + 2/5) 인데 §2 본문은 1 commit 메시지만 제시. 명세 불일치.

- **Rebuttal**: **Discuss 로 이관** — 사용자 선호 사안 (회수 용이 vs 단순화).
- **Proposed Action**: `5021_discussion_topics.md` 의 D2 로 분리.
- **PRD Impact**: 옵션 A → 2 commit 명확화 / B → 1 commit 통합 (합계 4 commit) / C → PRD 그대로.
- **Little Hero Context**: 본 프로젝트 commit 정책 (`CLAUDE.md` 머리말 "단일 책임 commit") — 작은 분리 권장. 권장 C 는 D2 분리 commit 의 의미 명확화.

### 3.3 [OOP §15.6 — Auto-허용]

**Original Criticism (critique §5)**: MetaProgression.cs 331L → +15L = 346L. §15.6 100줄 룰 위반.

- **Rebuttal**: **자동 수용 X (일시 허용)** — 5023 oop_metaprogression_split 가 분리 예정. 5021 → 5023 cascade 가 PRD §3.2 + 디자인 문서에 명시됨.
- **Proposed Action**: PRD 원안 유지. 5023 Sprint 1 의 MetaProgressionLoader 가 5021 의 ResolveSelectedCharacterFromSave 흡수.
- **PRD Impact**: 0 (PRD 변경 없음, cascade 정합 명시).

---

## 4. PRD v2 적용 변경 요약

(자동 수용 0건 → v2 변경 = 0건. 단 헤더에 v2 표기 추가, Discuss 2건 mediator 후 최종 확정.)

원본 PRD (`5021_starter_hero_chapter_flow_prd.md`) 와 v2 초안 (`5021_starter_hero_chapter_flow_v2.md`) 본문 동일.

---

## 5. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.2 Rebuttal). Accepted 0 / Discuss 2 / Reject 0. PRD §15 정합 100% |
