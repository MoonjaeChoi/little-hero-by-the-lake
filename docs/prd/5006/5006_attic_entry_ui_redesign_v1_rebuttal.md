# 5006 attic_entry_ui_redesign_v1 PRD Rebuttal (Phase 2.2)

> **검토자**: Claude Code (자동) · **일시**: 2026-05-04
> **Critique 출처**: `5006_attic_entry_ui_redesign_v1_critique.md` (8.7/10, Accept 8 / Reject 0 / Discuss 5)

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| Sprint 분해 | Sprint 1.2 tint → Sprint 4.2 복원 중복 | Medium | Discuss | discussion_topics → mediator | △ |
| 외부 도구 의존 | 사용자 수동 PNG 14종 | High | Discuss | discussion_topics → mediator | △ |
| 시스템 통합 | EndgameMenuPanel 협업 인터페이스 모호 | Medium | Discuss | discussion_topics → mediator | △ |
| 5005 호환 | AtticPanelToggleManager 자동 탐색 회피 | Low | Discuss | discussion_topics → mediator | ✅ |
| 인프라 | AtticUIIconImporter 9-slice border 분기 | Low | Discuss | discussion_topics → mediator | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 5
- Accepted: 0 (Critique 의 Accept 항목은 이미 PRD v1 에 반영됨 — rebuttal 단계 변경 X)
- Rejected: 0 (Reject 항목 없음)
- Discuss (사용자 결정 필요): 5 (100%)
- §15 정합률: 100% (모든 Discuss 항목 §15 위반 X)

## 3. Detailed Responses

### 3.1 [Sprint 분해 — Medium] Sprint 1.2 tint 중복

**Original Criticism**:
> Sprint 1.2 가 13 버튼 Image.color 를 4 그룹 색으로 tint, Sprint 4.2 에서 신규 PNG 임포트 시 Color.white 복원 — 같은 13 버튼 Inspector wiring 두 번

- **Rebuttal**: 단계적 가독성 ↑ 즉시 효과 vs 작업 중복 trade-off. 권장 A 채택 시 작업 중복 ~15분 (Inspector tint reset), 권장 B 채택 시 사용자 PNG 도착 전까지 가독성 미흡 (현 상태 그대로). C 채택 시 본 PRD 가 Sprint 1.2 만 (5006.1 후속 분리).
- **Status**: Discuss → discussion_topics.md
- **Resolution Path**: mediator 단계 사용자 결정

### 3.2 [외부 도구 — High] 사용자 수동 PNG

**Original Criticism**:
> Sprint 4 가 외부 ComfyUI/Midjourney 로 14 PNG 생성 후 임포트 — V5 톤 시드 22222 일관성 사용자 책임

- **Rebuttal**: 본 PRD 의 가장 큰 외부 의존. 옵션 B (Sprint 4 분리) 가 본 PRD 진행 즉시성 확보. 옵션 C (ComfyUI batch 자동화) 는 환경 설치 부담 + 본 PRD 범위 ↑ (별도 사이클 적합).
- **Status**: Discuss → discussion_topics.md
- **PRD Impact**: 옵션 B 채택 시 Sprint 4 → 5006.1 후속, 본 PRD 는 Sprint 1+2+3+5+6 (8 commit, 3시간 50분). 옵션 A 유지 시 PRD 그대로 (10 commit).

### 3.3 [시스템 통합 — Medium] EndgameMenuPanel 협업 인터페이스

**Original Criticism**:
> Sprint 3.2 의 "AtticEndgameMenuView 와 협업" 모호 — IsAvailable() / 이벤트 / Panel.OnEnable 자체 검증 옵션 3종

- **Rebuttal**: 권장 C (Panel.OnEnable 자체 검증) 가 가장 단순 + AtticEndgameMenuView 와의 결합도 ↓. EndgameMenuPanel 에 firstClearedChapters 35 검사 + 미해당 시 SetActive(false) + 콘솔 토스트 추가.
- **Status**: Discuss → discussion_topics.md
- **PRD Impact**: 권장 C 채택 시 Sprint 3.2 명세 명확화 + EndgameMenuPanel.cs 수정 (~5L 추가).

### 3.4 [5005 호환 — Low] AtticPanelToggleManager 자동 탐색

**Original Criticism**:
> 신규 panel 도입 시 자동 탐색 정책 영향 — 옵션 A (RectTransform 그룹 이름 규칙 회피) vs B (_excludePanelNames 추가)

- **Rebuttal**: 권장 A 가 더 안전. 4 그룹 컨테이너 이름 = `Group1Combat / Group2Equipment / Group3Shop / Group4Record` (Panel/Modal/Overlay 접미어 X) → AtticPanelToggleManager 자동 탐색 영향 0.
- **Status**: Discuss → discussion_topics.md (확인 차원)

### 3.5 [인프라 — Low] AtticUIIconImporter 9-slice border

**Original Criticism**:
> "Border 적당" 모호 — 옵션 A (모두 동일) vs B (버튼 9-slice border=12, 화폐 0) vs C (수동 폐기)

- **Rebuttal**: 권장 B 가 표준. 파일명 prefix (`btn_*` / `icon_*`) 자동 분기 → Importer 코드 ~10L 추가. 다양한 버튼 size 대응 ↑.
- **Status**: Discuss → discussion_topics.md

## 4. PRD v2 적용 변경 요약

(rebuttal 단계 — Accept 0건이므로 v2 PRD 초안은 v1 과 동일.)

- §1.2 Sprint 표 — 변경 X
- §3 Definition of Done — 변경 X
- §6 위험 + 완화 — 변경 X

> mediator 단계 결정 후 v2 PRD 최종 작성.

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal 자동, 모든 Discuss → mediator)
