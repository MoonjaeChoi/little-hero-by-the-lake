# 5006 attic_entry_ui_redesign_v1 — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.

## High Priority Items

### D1. 사용자 수동 PNG 14종 — 외부 도구 의존

- **Original Criticism**: Sprint 4 가 외부 ComfyUI/Midjourney 로 14 PNG 생성 후 임포트 — V5 톤 시드 22222 일관성 사용자 책임
- **Current Position**: PRD 그대로 진행 시 사용자 수동 PNG 생성 시간이 Sprint 4 차단
- **Discussion Points**:
  - **A (현재 PRD)**: 사용자 수동 (외부 ComfyUI/Midjourney) — Sprint 4 도착 시까지 대기. 10 commit / 4시간 40분
  - **B (권장 ★)**: **Sprint 4 분리 → 5006.1 후속 사이클**. 본 PRD 는 Sprint 1+2+3+5+6 (8 commit / 3시간 50분, PNG 부재로도 즉시 진행). PNG 도착 후 5006.1 별도 (2 commit / 50분)
  - **C**: ComfyUI batch 워크플로 신규 (`tools/comfyui_attic_buttons/`) — 자동화 + 환경 설치 부담 (별도 사이클 적합)
- **Proposed Resolution Path**: **B** — 본 PRD 즉시 진행 가능 + 사용자 PNG 생성 시간 변수 흡수 + 5006.1 후속 자연스러운 분리
- **PRD Impact**:
  - 채택 시 Sprint 4 → 5006.1 분리. 본 PRD = 8 commit / 3시간 50분
  - Sprint 1.2 tint 1차 → Sprint 4.2 복원 작업이 5006.1 로 이동 (D2 와 함께 결정)

### D2. Sprint 1.2 tint 1차 → Sprint 4.2 복원 작업 중복

- **Original Criticism**: Sprint 1.2 가 13 버튼 Image.color 를 4 그룹 색으로 tint, Sprint 4.2 에서 신규 PNG 임포트 시 Color.white 복원 — 같은 13 버튼 Inspector wiring 두 번 (시간 손실 ~15분)
- **Current Position**: PRD 그대로 진행 시 작업 중복
- **Discussion Points**:
  - **A (현재 PRD)**: Sprint 1.2 tint + Sprint 4.2 복원 (단계적 가독성 ↑ 즉시 효과)
  - **B**: Sprint 1.2 폐기 (PNG 도착 시까지 가독성 미흡, 그러나 작업 중복 X)
  - **C (권장 ★)**: **Sprint 1.2 만 (tint 영구) — 신규 PNG 임포트 = 5006.1 분리 후 별도**. 본 PRD 종료 시점 가독성 확보 + 5006.1 에서 PNG 임포트 시 복원
- **Proposed Resolution Path**: **C** — D1=B 와 자연스럽게 결합. 본 PRD 가 가독성 (tint) 까지 보장 후 5006.1 이 시각적 완성도 PNG 처리.
- **PRD Impact**: D1=B + D2=C 결합 시 본 PRD = 8 commit (Sprint 1+2+3+5+6, Sprint 4 = 5006.1 후속).

## Medium Priority Items

### D3. EndgameMenuPanel — AtticEndgameMenuView 와 협업 인터페이스

- **Original Criticism**: Sprint 3.2 가 "AtticEndgameMenuView 와 협업" 모호 — 신규 public API 또는 이벤트 발행 미정의
- **Current Position**: 현재 PRD Sprint 3.2 가 1 commit 으로 모호 처리
- **Discussion Points**:
  - **A**: AtticPanelEntryView 가 AtticEndgameMenuView.IsAvailable() 신규 public API 호출 (DI 결합도 ↑)
  - **B**: AtticPanelEntryView entry 의 Button.interactable 을 AtticEndgameMenuView 가 이벤트 발행 (이벤트 결합도 ↑)
  - **C (권장 ★)**: **EndgameMenuPanel.OnEnable 자체 검증** — firstClearedChapters 35 검사 → 미해당 시 SetActive(false) + 토스트 "CH 35 클리어 후 도전 가능"
- **Proposed Resolution Path**: **C** — 가장 단순. 결합도 0, AtticPanelEntryView 가 단순 panel.SetActive(true) 만, Panel 자체가 검증.
- **PRD Impact**: Sprint 3.2 명세 명확화 + EndgameMenuPanel.cs ~5L 수정 (시간 변경 X)

### D5. AtticUIIconImporter — Sprite 9-slice border 분기

- **Original Criticism**: "Border 적당" 모호. 14 PNG (버튼 9-slice 필요, 화폐 9-slice X) 분기 필요
- **Current Position**: PRD 가 모호 명세
- **Discussion Points**:
  - **A**: 모든 PNG 동일 default (9-slice border = 0)
  - **B (권장 ★)**: **버튼 9-slice border = 12 (Unity 권장), 화폐 = 0** — 파일명 prefix `btn_*` / `icon_*` 자동 분기 (~10L 추가)
  - **C**: 사용자 수동 import settings (Importer 폐기)
- **Proposed Resolution Path**: **B** — 버튼 다양한 size 대응 ↑, Importer 자동 분기 명확
- **PRD Impact**: AtticUIIconImporter ~60L → ~70L

## Low Priority Items

### D4. AtticPanelToggleManager `_excludePanelNames` 갱신

- **Original Criticism**: 신규 4 그룹 RectTransform 컨테이너가 자동 탐색 (Panel/Modal/Overlay 접미어) 영향?
- **Current Position**: 4 그룹 이름 = Group{N}{Category} → Panel 접미어 X → 자동 영향 0
- **Discussion Points**:
  - **A (권장 ★)**: **이름 규칙 회피만** (`Group1Combat` 등) — _excludePanelNames 변경 X
  - **B**: _excludePanelNames 에 명시 추가 (방어적)
- **Proposed Resolution Path**: **A** — 이름 규칙 자연스러운 회피, 변경 X
- **PRD Impact**: 0 (확인 차원)

## 요약

- Total Items: 5
- High: 2 / Medium: 2 / Low: 1
- 권장 mode: **ai-assisted** (Claude 권장 + 사용자 응답 대기)
- **권장 일괄**: D1=B / D2=C / D3=C / D4=A / D5=B

## 결정 입력 형식 예시

```
"권장안 그대로"            → D1=B / D2=C / D3=C / D4=A / D5=B 일괄 채택
"D1=A, D2=A, 나머지 권장"  → 사용자 PNG 즉시 생성 가정
"D2=B, 나머지 권장"        → tint 폐기 (가독성 즉시 효과 포기)
```
