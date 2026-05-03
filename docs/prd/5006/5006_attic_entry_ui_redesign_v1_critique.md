# 5006 attic_entry_ui_redesign_v1 PRD 리뷰 (Phase 2.1 Critique)

> **검토자**: Claude Code (자동) · **일시**: 2026-05-04
> **PRD**: `docs/prd/5006/5006_attic_entry_ui_redesign_v1_prd.md` v1
> **디자인**: `docs/prd/5006_attic_entry_ui_redesign_v1.md`

## 0. 결정 이력

- 2026-05-04: 디자인 v1 + PRD v1 작성 직후 자동 critique 진행

## 1. 종합 점수: 8.7 / 10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | 신규 클래스 모두 100L 이내, SRP 명확. (-0.5: AtticPanelEntryView 의 `OnEnable/OnDisable` 람다 captured 정합 5005 답습 ✅) |
| PG-12 톤 | 10/10 | 만화풍 + 따뜻 색감, 보스 실루엣도 위협적 X. 위반 0건 |
| 성능 | 9/10 | UI 한정 변경, GC ↑ 위험 0. (-1: AtticGroupLayoutPresenter.OnValidate Editor 만 호출, runtime 영향 X) |
| 시스템 통합 | 8.5/10 | 5005 (PanelToggleManager 자동 탐색) + 5004 (씬 전환) + gacha_v1/limited_shop_v1 진입 명세 정합. (-1.5: 외부 PNG 사용자 수동 의존, Sprint 4 차단 가능) |
| Sprint 분해 | 7.5/10 | 6 Sprint × 10 commit, 4시간 40분. (-2.5: Sprint 4 사용자 수동 PNG 의존 + Sprint 1.2 tint 1차가 Sprint 4.2 에서 다시 복원되는 작업 중복) |

## 2. Accept 항목 (8건)

- ✅ **D1=B 4 그룹 좌우 분산** — 한 손 플레이 + 그룹 가독성 동시 충족 (CLAUDE.md §2 절대 원칙)
- ✅ **D2=B 4 그룹 카테고리** (전투/장비/상점/기록) — 자연스러운 분류, 13 버튼 균형 분산
- ✅ **D4=B 2 컴포넌트 분할** — AtticPanelEntryView (~80L) + AtticGroupLayoutPresenter (~50L) 각 §15.6 통과 + SRP 명확
- ✅ **D5=B 화폐 4 아이콘 동시** — V5 톤 일관성 (별도 사이클 X)
- ✅ **5005 Hub 컴포넌트 보존** — AtticController 의 가챠/스타포스/한정상점 잔여 책임 유지 (재진입 X)
- ✅ **§15.7 DI** — 모든 reference SerializeField 명시 (Singleton 직접 참조 X)
- ✅ **44pt 터치 타겟 OnValidate** — Editor 자동 검증 (CLAUDE.md §2)
- ✅ **NUnit Reflection 정적 검증** — 5005 패턴 답습 (Type_Exists 6 + 5005 회귀)

## 3. Reject 항목 (0건)

(없음 — §15 위반 / PG-12 톤 위반 0)

## 4. Discuss 항목 (5건)

### D1. Sprint 1.2 tint 1차 → Sprint 4.2 복원 작업 중복

- **현재**: Sprint 1.2 가 13 버튼 Image.color 를 4 그룹 색으로 tint, Sprint 4.2 에서 신규 PNG 임포트 시 Color.white 복원
- **문제**: 같은 13 버튼 Inspector wiring 을 두 번 만지는 작업 (시간 손실 추정 15분)
- **옵션**:
  - **A**: 현재 PRD 그대로 — 단계적 가독성 ↑ 즉시 효과
  - **B**: Sprint 1.2 폐기 — 신규 PNG 도착까지 tint 없이 진행 (사용자 PNG 생성 우선)
  - **C**: Sprint 1.2 만 (tint) — 신규 PNG 임포트 = 별도 사이클 (5006.1 후속)
- **권장**: **A** — 단계적 가독성 즉시 효과가 사용자 PNG 생성 대기 시간을 보완. 복원 작업은 Inspector tint reset 1번만 (시간 미미).

### D2. 사용자 수동 PNG 14종 — 외부 도구 의존

- **현재**: Sprint 4 가 외부 ComfyUI/Midjourney 로 14 PNG 생성 후 임포트
- **문제**: 외부 도구 환경 + V5 톤 시드 22222 일관성 사용자 책임
- **옵션**:
  - **A**: 사용자 수동 (외부 도구) — 현재 PRD
  - **B**: Sprint 4 분리 — 본 PRD 는 Sprint 1~3+5+6 만 (PNG 도착 시 별도 5006.1 사이클)
  - **C**: 본 PRD 에 ComfyUI batch 워크플로 신규 (`tools/comfyui_attic_buttons/`) — 자동화 + 시드 일관성
- **권장**: **B** — 본 PRD 에서 Sprint 4 분리, PNG 도착 후 5006.1 후속. 이유: PNG 부재 상태에서도 Sprint 1~3 (코드 + tint + wiring) 완전 가능 + 5 (NUnit) + 6 (docs) 진행. 사용자 PNG 생성 시간이 변수.

### D3. EndgameMenuPanel — 5005 AtticEndgameMenuView 와 협업 명세 부족

- **현재**: Sprint 3.2 가 "EndgameMenu 진입 버튼 가용성 검사 (AtticEndgameMenuView 와 협업)" 모호
- **문제**: AtticEndgameMenuView 는 root 가용성만 표시 (5005), 진입 버튼은 별도 panel 활성화 필요. 협업 인터페이스 미정의
- **옵션**:
  - **A**: AtticPanelEntryView 가 Endgame 진입 시 AtticEndgameMenuView.IsAvailable() 검사 (신규 public API)
  - **B**: AtticPanelEntryView entry 의 `Button.interactable` 을 AtticEndgameMenuView 가 직접 조작 (이벤트 발행)
  - **C**: AtticPanelEntryView 가 단순 panel.SetActive(true), Panel 자체가 진입 시 검증 (panel.OnEnable 에서 미해당 시 SetActive(false))
- **권장**: **C** — 가장 단순. EndgameMenuPanel.OnEnable 이 firstClearedChapters 35 검사 → 미해당 시 자동 닫힘 + 토스트 "CH 35 클리어 후 도전 가능".

### D4. AtticPanelToggleManager `_excludePanelNames` 갱신 검토

- **현재**: 5005 AtticPanelToggleManager._excludePanelNames = { "CharacterPanel" } (CharacterSelector 본체)
- **문제**: Sprint 2 후 신규 panel 도입 시 자동 탐색에 포함 → CurrencyView/EndgameMenu root 가 Panel 접미어면 토글 정책 영향
- **옵션**:
  - **A**: AtticGroupLayoutPresenter 의 4 그룹 RectTransform 자식은 모두 Panel 접미어 X (Group1Combat 등) — 자동 영향 X
  - **B**: _excludePanelNames 에 AtticEndgameMenuView root 추가
- **권장**: **A** — RectTransform 그룹 컨테이너 이름 규칙 (Group{N}{Category}) → Panel 접미어 회피 자연스럽게. _excludePanelNames 변경 X.

### D5. AtticUIIconImporter — Sprite 9-slice border 명세 부족

- **현재**: Sprint 4.1 의 AtticUIIconImporter 가 PixelsPerUnit=100, Filter=Bilinear, Border 적당
- **문제**: "Border 적당" 모호. 14 PNG 별 9-slice border 명세 (버튼은 9-slice, 화폐는 9-slice X) 미분리
- **옵션**:
  - **A**: 모든 PNG 동일 default (9-slice border = 0)
  - **B**: 버튼 9-slice border = 12 (Unity 권장), 화폐 = 0
  - **C**: 사용자 수동 import settings (Importer 폐기)
- **권장**: **B** — 버튼 9-slice 가 다양한 size 대응 ↑. AtticUIIconImporter 내부에서 파일명 prefix (`btn_*` vs `icon_*`) 로 자동 분기.

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 |
|---|---|
| §15.1 SOLID-S | ✅ — 2 컴포넌트 분리 (Entry/Layout) 책임 명확 |
| §15.1 SOLID-O | ✅ — switch X, _entries 배열 Strategy 패턴 답습 |
| §15.4 SO 우선 | ✅ — 모든 panel SerializeField, hardcode 0 |
| §15.6 100줄 룰 | ✅ — 신규 모두 100L 이내 (~80/~50/~60) |
| §15.7 DI | ✅ — Initialize 명시 X (UI view 단순), SerializeField 우선 |
| §15.9 Test | ✅ — Reflection 6 NUnit 명시 |
| §15.11 안티 패턴 | ✅ — public 필드 X, Update X, Instantiate X, GameObject.Find X |
| §15.12 Validator | ✅ — 기존 200L LINE_LIMIT 룰 충분 |

## 6. PG-12 톤 점검

- ✅ "보스 실루엣 + 별" — 위협적 X, 만화풍
- ✅ "마법 결정 박스" — 보랏빛 sparkle, 잔인 X
- ✅ "망치 + 별 sparkle" — 강화 모티브, 폭력 X
- ✅ 4 그룹 색감 (빨강/파랑/보라/베이지) 모두 따뜻
- ✅ 텍스트 라벨 일반 게임 어휘 (가챠/소환/일기/도감 — PG-12 OK)

## 7. 권장 변경 (mediator 단계)

### 즉시 변경 (Accept 자동 반영)
- 없음 (Reject 0건)

### 사용자 결정 후 변경 (5건 → discussion_topics.md)
- D1 Sprint 1.2 tint 중복 처리
- D2 사용자 수동 PNG (Sprint 4 분리 검토)
- D3 EndgameMenuPanel 협업 인터페이스
- D4 AtticPanelToggleManager 자동 탐색 회피
- D5 9-slice border 분기

## 8. 비기능 검증

- ✅ 한 손 플레이 (좌우 분산)
- ✅ 44pt 터치 타겟 OnValidate
- ✅ 색맹 친화 (모티브 형태 + 색 병행)
- ⏸ 60fps 유지 (UI 변경, 영향 미미 추정)
- ✅ TMP 미사용 (Legacy Text)

## 9. 요약

```
종합: 8.7/10
Accept: 8 (자동 반영)
Reject: 0
Discuss: 5 (mediator 진행)
```

PRD 전체적으로 OOP §15 정합 양호하고 PG-12 위반 0. Discuss 5건 모두 Sprint 분해 효율성 / 외부 도구 의존 / 협업 인터페이스 명세 — 사용자 결정 사안.

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.1 Critique 자동)
