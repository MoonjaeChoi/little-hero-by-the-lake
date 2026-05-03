# 5006 — Attic Entry UI Redesign v1 — Sprint PRD

> **출처**: `docs/prd/5006_attic_entry_ui_redesign_v1.md` (디자인) — 옵션 D1=B / D2=B / D3=D / D4=B / D5=B
> **PGE Phase 3 (Breakdown) 가능성 알림**: 본 PRD 는 v1. 후속 `/prd-pipeline 5006` 시 Phase 2 (review) 진행 후 v2 + breakdown + contract 자동.

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 | 의존성 |
|---|---|---|---|---|
| 1 | AtticGroupLayoutPresenter + 4 그룹 RectTransform + tint 가독성 1차 | 2 | 60분 | — |
| 2 | AtticPanelEntryView + 10 panel 진입 wiring (5005 잔여 3 제외) | 2 | 60분 | 1 |
| 3 | 누락 panel Open button SerializeField 보강 (CharInfo/Forge/Endgame) | 2 | 45분 | 2 |
| 4 | 사용자 수동 생성 14 PNG 임포트 + AtticUIIconImporter | 2 | 50분 | 1 (병렬) |
| 5 | NUnit AtticEntryUITests + Validator 룰 | 1 | 40분 | 2~4 |
| 6 | docs (changelog + CLAUDE.md §11/§12/§14) | 1 | 25분 | 모두 |
| **총** | | **10** | **4시간 40분** | |

> 사용자 수동 PNG 생성 시간 (외부 ComfyUI/Midjourney) 별도 — Sprint 4 commit 시점 결정.

## 2. Sprint 명세

### Sprint 1 — AtticGroupLayoutPresenter + 4 그룹 + tint 1차 (2 commit)

#### 1.1 AtticGroupLayoutPresenter.cs 신규

**파일**: `Assets/_Project/Scripts/Hub/AtticGroupLayoutPresenter.cs`

**산출물**:
- 클래스 `AtticGroupLayoutPresenter : MonoBehaviour` (~50L)
- 4 SerializeField RectTransform (`_group1Combat / _group2Equipment / _group3Shop / _group4Record`)
- `_minTouchSize = 44f` SerializeField (CLAUDE.md §2 정합)
- `OnValidate()` — 4 그룹 자식 검사 + 44pt 이하 시 LogWarning

**검증 기준**:
- 컴파일 errors=0
- §15.6 100줄 룰 통과 (~50L)
- Inspector 에서 4 RectTransform 드래그 가능

**완료 조건**:
- AtticGroupLayoutPresenter 컴포넌트 Attic.unity 부착
- 4 빈 RectTransform 자식 GameObject 생성 (Group1Combat / Group2Equipment / Group3Shop / Group4Record) — anchor 좌상/좌하/우상/우하

#### Sprint 1.1 commit

```
5006 Sprint 1.1 / T01 — AtticGroupLayoutPresenter + 4 그룹 RectTransform (1/10)

[변경]
- Scripts/Hub/AtticGroupLayoutPresenter.cs 신규 (~50L)
- Scenes/Attic.unity — 4 RectTransform 자식 추가 (Group1~4)

[검증]
- 컴파일 errors=0
- 44pt 터치 검증 OnValidate

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 1.2 13 버튼 tint 1차 (Inspector 만, 코드 변경 X)

**작업**:
- 4 그룹별 ButtonImage.color tint 적용 (G1 빨강 / G2 파랑 / G3 보라 / G4 베이지)
- 즉시 가독성 ↑ (신규 PNG 임포트 전 임시)

**산출물**:
- Attic.unity 13 Button 의 Image.color override (Inspector)
- 메모: 신규 PNG 임포트 시 (Sprint 4) tint Color.white 복원

**검증 기준**:
- Play 시 4 그룹 색 구분 가능
- 기존 onClick 동작 회귀 X

#### Sprint 1.2 commit

```
5006 Sprint 1.2 / T02 — 13 버튼 tint 1차 (4 그룹 색 구분, 2/10)

[변경]
- Scenes/Attic.unity — 13 Button.Image.color override (G1 빨강/G2 파랑/G3 보라/G4 베이지)

[검증]
- 4 그룹 시각 구분 가능 (Play 모드)
- onClick 동작 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — AtticPanelEntryView + 10 panel 진입 wiring (2 commit)

#### 2.1 AtticPanelEntryView.cs 신규

**파일**: `Assets/_Project/Scripts/Hub/AtticPanelEntryView.cs`

**산출물**:
- 클래스 `AtticPanelEntryView : MonoBehaviour` (~80L)
- 내부 `[Serializable] PanelEntry { id, button, panelRoot }`
- `_entries[]` SerializeField
- `OnEnable` / `OnDisable` 에서 onClick 등록·해제
- 람다 captured local var 패턴 (CharacterButton/ChapterButton 답습, 5005)

**검증 기준**:
- 컴파일 errors=0
- §15.6 100줄 룰 통과 (~80L)
- 5005 패턴 정합 (AtticChapterEntry / AtticCharacterSelector)

**완료 조건**:
- Attic.unity GameObject 에 컴포넌트 부착
- _entries 10개 wiring (Diary/Bestiary/CharInfo/MetaUpgrade/Inventory/StarlightShop/WondrousMap/ConversionForge/EndgameMenu + 1 reserve)

#### Sprint 2.1 commit

```
5006 Sprint 2.1 / T03 — AtticPanelEntryView 신규 (3/10)

[변경]
- Scripts/Hub/AtticPanelEntryView.cs 신규 (~80L)
- 5005 패턴 답습 (PanelEntry [Serializable] 내부 클래스 + onClick captured local)

[검증]
- 컴파일 errors=0
- §15.6 100L 통과
- §15.7 DI — SerializeField 명시

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 2.2 Attic.unity 10 panel wiring

**작업**:
- AtticPanelEntryView._entries 배열 채우기 (Inspector)
- 각 entry: id="diary"/"bestiary"/.../"endgame" + Button + Panel.gameObject 드래그
- 5005 AtticPanelToggleManager 자동 탐색 결과와 정합 (Panel/Modal/Overlay 접미어)

**검증 기준**:
- Play 시 10개 버튼 클릭 → 해당 panel 활성화
- AtticPanelToggleManager 가 이전 panel 자동 비활성화 (한 번에 하나만)
- 가챠/스타포스/한정상점 (5005 잔여) 동작 유지

#### Sprint 2.2 commit

```
5006 Sprint 2.2 / T04 — Attic.unity 10 panel wiring (4/10)

[변경]
- Scenes/Attic.unity — AtticPanelEntryView._entries 10개 wiring

[검증]
- 10 진입 버튼 → panel 활성 OK
- 5005 PanelToggleManager 정합 (한 번에 하나만)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — 누락 panel Open button 보강 (2 commit)

#### 3.1 CharacterInfoPanel / ConversionForgePanel 의 Open API

**파일**:
- `Assets/_Project/Scripts/UI/CharacterInfoPanel.cs`
- `Assets/_Project/Scripts/UI/ConversionForgePanel.cs`

**작업**:
- 각 Panel 에 `public void Open() { _panelRoot.SetActive(true); /* refresh */ }` 추가
- 또는 AtticPanelEntryView 가 panelRoot.SetActive(true) 만 호출 → Panel.OnEnable 자동 갱신 (단순)

**선택**: **B (단순)** — Panel 코드 수정 X. AtticPanelEntryView 가 panelRoot.SetActive(true) 만. Panel 의 OnEnable 이 데이터 갱신 (DiaryPanel/BestiaryPanel 패턴).

**검증 기준**:
- Play 시 CharacterInfoPanel 진입 시 데이터 (캐릭터 정보) 표시
- 컴파일 회귀 X

#### Sprint 3.1 commit

```
5006 Sprint 3.1 / T05 — CharacterInfoPanel/ConversionForgePanel OnEnable 데이터 갱신 (5/10)

[변경]
- Scripts/UI/CharacterInfoPanel.cs / ConversionForgePanel.cs — OnEnable 갱신 hook 추가 (필요 시)

[검증]
- AtticPanelEntryView 진입 시 데이터 표시 OK

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 3.2 EndgameMenuPanel — AtticEndgameMenuView 와 협업

**작업**:
- `AtticEndgameMenuView` 가 root 가용성만 표시 (5005 결과)
- AtticPanelEntryView 의 endgame 진입 = root 활성화 (AtticEndgameMenuView 이미 활성 상태일 때만)
- 비활성 (CH 35 미클리어) 시 진입 버튼 자체를 disable

**검증 기준**:
- CH 35 미클리어 — Endgame 진입 버튼 회색
- CH 35 클리어 후 — 진입 가능

#### Sprint 3.2 commit

```
5006 Sprint 3.2 / T06 — EndgameMenu 진입 버튼 가용성 연동 (6/10)

[변경]
- Scripts/Hub/AtticPanelEntryView.cs — endgame entry 가용성 검사 (AtticEndgameMenuView 와 협업)

[검증]
- CH 35 미클리어 시 endgame 버튼 회색
- CH 35 클리어 후 활성

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — 14 PNG 임포트 + AtticUIIconImporter (2 commit)

> **사용자 수동 작업 의존**: 외부 ComfyUI/Midjourney 로 14 PNG 생성 (V5 sticker tone, 시드 22222). 임포트 후 본 Sprint 진행.

#### 4.1 AtticUIIconImporter.cs (Editor 도구)

**파일**: `Assets/_Project/Scripts/Editor/AtticUIIconImporter.cs`

**산출물**:
- 클래스 `AtticUIIconImporter` (Editor static, ~60L)
- 메뉴 `Tools > LittleHero > Import Attic Button Icons`
- 자동: `Assets/_Project/Art/Generated_v2/UI/AtticButtons/` 의 PNG 일괄 검색 → Sprite import settings 정합 (PixelsPerUnit 100, Filter Bilinear, Border 적당)

**검증 기준**:
- 컴파일 errors=0
- 메뉴 실행 시 모든 PNG → Sprite 자동 변환

#### Sprint 4.1 commit

```
5006 Sprint 4.1 / T07 — AtticUIIconImporter (사용자 수동 PNG 임포트 자동화, 7/10)

[변경]
- Scripts/Editor/AtticUIIconImporter.cs 신규 (~60L)

[검증]
- 메뉴 실행 시 PNG → Sprite 일괄 변환

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 4.2 14 PNG 임포트 + Button.image 교체

**작업**:
- 사용자가 외부에서 생성한 14 PNG 를 `Assets/_Project/Art/Generated_v2/UI/AtticButtons/` 에 복사
- AtticUIIconImporter 메뉴 실행 → Sprite 자동 변환
- Attic.unity 의 13 Button + 4 화폐 아이콘 → 신규 Sprite 교체 (Inspector)
- Sprint 1.2 의 tint 1차 → Color.white 복원

**검증 기준**:
- Play 시 13 버튼 + 4 화폐 아이콘 신규 PNG 표시
- V5 톤 일관성 시각 검증 (사용자)

#### Sprint 4.2 commit

```
5006 Sprint 4.2 / T08 — 14 PNG (13 버튼 + 화폐 4) 임포트 + Sprite 교체 (8/10)

[변경]
- Art/Generated_v2/UI/AtticButtons/btn_*.png 14 신규
- Scenes/Attic.unity — 13 Button.image + 4 화폐 Image 교체
- 1.2 tint → Color.white 복원

[검증]
- Play 시 신규 아이콘 표시
- V5 톤 일관성

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 5 — NUnit AtticEntryUITests + Validator (1 commit)

#### 5.1 NUnit Test 6+

**파일**: `Assets/_Project/Tests/Editor/AtticEntryUITests.cs`

**산출물**:
- `AtticPanelEntryView_Type_Exists`
- `AtticGroupLayoutPresenter_Type_Exists`
- `AtticUIIconImporter_Type_Exists`
- `AtticPanelEntryView_HasPanelEntry_NestedClass`
- `AtticGroupLayoutPresenter_MinTouchSize_44`
- `AtticEntryUI_5005_RegressionPass` (5005 6 컴포넌트 모두 Type.GetType 통과 — 회귀 검사)

**검증 기준**:
- Test Runner 6/6 PASS
- 누적 NUnit 67 → 73

#### Sprint 5 commit

```
5006 Sprint 5 / T09 — AtticEntryUITests 6 NUnit + Validator 통과 (9/10)

[변경]
- Tests/Editor/AtticEntryUITests.cs 신규
  - 6 NUnit (Reflection 정적 + 5005 회귀)
- ProjectValidator §15.6 200L LINE_LIMIT 통과 (신규 모두 100L 이내)

[검증]
- Test Runner 6/6 PASS
- 누적 67 → 73

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 6 — docs + CLAUDE.md (1 commit)

**작업**:
- `docs/little_hero/changelog_v1.md` — 5006 attic_entry_ui_redesign_v1 항목 (10 commit 묶음)
- `CLAUDE.md §11` — 마일스톤 row 추가
- `CLAUDE.md §12` — UI 시스템 레이어에 AtticPanelEntryView/AtticGroupLayoutPresenter 등재 + Tests/Editor 73
- `CLAUDE.md §14` — 최근 5건 회전 (5006 등재)

#### Sprint 6 commit

```
5006 Sprint 6 / T10 — docs (changelog + CLAUDE.md §11/§12/§14) (10/10)

[변경]
- docs/little_hero/changelog_v1.md — 5006 항목 추가
- CLAUDE.md §11/§12/§14 갱신

[검증]
- Phase 5 종결 (10/10 commit)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (Layout + tint) ─┬→ Sprint 2 (PanelEntryView) ─→ Sprint 3 (누락 보강) ─┐
                           │                                                      ├→ Sprint 5 (Test) ─→ Sprint 6 (docs)
                           └→ Sprint 4 (PNG 임포트, 사용자 PNG 도착 시) ─────────┘
```

**병렬 가능**:
- Sprint 1 후 — Sprint 2 와 Sprint 4 병렬 (단, Sprint 4 는 사용자 PNG 생성 후)
- Sprint 5 는 Sprint 2/3/4 완료 후

## 4. 검증 시나리오 (Play 검증)

### S1 — 4 그룹 시각 구분

1. Play Mode 진입 → Attic 씬
2. **검증**: 13 버튼이 4 그룹 색으로 구분되어 보임 (G1 빨강 / G2 파랑 / G3 보라 / G4 베이지) — Sprint 1.2 tint 또는 Sprint 4.2 신규 PNG
3. **검증**: 좌측 (G1+G2) / 우측 (G3+G4) 분산 → 한 손 플레이 시 손가락 가림 ↓

### S2 — 10 panel 진입 동작

1. 일기 버튼 클릭 → DiaryPanel 활성
2. **검증**: 콘솔 로그 없음 (자동 wiring), Panel 표시
3. 도감 버튼 클릭
4. **검증**: DiaryPanel 자동 비활 (5005 PanelToggleManager) + BestiaryPanel 활성
5. 10개 버튼 모두 반복 — 가챠/스타포스/한정상점 (5005 잔여) 동작 회귀 X

### S3 — 신규 아이콘 V5 톤 일관성

1. Sprint 4.2 후 Play Mode
2. **검증**: 14 PNG (13 버튼 + 4 화폐) 신규 아이콘 표시, V5 sticker dynamic tone 일관 (시드 22222 시각 검증)

### S4 — 44pt 터치 타겟

1. AtticGroupLayoutPresenter._minTouchSize = 44
2. Inspector 에서 Group 자식 RectTransform sizeDelta 가 44 이하면 OnValidate LogWarning
3. **검증**: 콘솔 warning 0건 (모든 자식 44+ 통과)

### S5 — 색맹 친화 (텍스트 라벨 보조)

1. 14 신규 아이콘 모두 모티브 (책/가방/모루/시계 등) 형태로 구별 가능
2. **검증**: 색 외 형태로도 구별 가능 (그레이스케일 시뮬레이션 권장)

### S6 — 5005 회귀

1. AtticController 의 가챠/스타포스/한정상점 진입 — 동작 유지
2. AtticCurrencyView 4 화폐 표시 — 동작 유지
3. AtticCharacterSelector 3 캐릭터 선택 — 동작 유지
4. AtticChapterEntry 35 챕터 진입 + 5002 D2 toast — 동작 유지
5. AtticEndgameMenuView 가용성 — 동작 유지
6. AtticPanelToggleManager 한 번에 하나만 — 정상

## 5. Definition of Done

- [ ] Sprint 1~6 모두 commit (10 commit)
- [ ] AtticPanelEntryView (~80L) + AtticGroupLayoutPresenter (~50L) + AtticUIIconImporter (~60L) 신규 — 모두 §15.6 100L 이내
- [ ] AtticController 본체 라인 변동 X (BGM + 가챠/스타포스/한정상점 잔여 책임 그대로, 111L 유지)
- [ ] 13 진입 버튼 모두 onClick → panel 활성화
- [ ] 14 PNG 신규 임포트 (13 버튼 + 4 화폐, V5 톤)
- [ ] NUnit AtticEntryUITests 6 PASS (누적 67 → 73)
- [ ] ProjectValidator 신규 위반 0
- [ ] CLAUDE.md §11/§12/§14 갱신 + changelog 등재
- [ ] 5005 6 컴포넌트 회귀 X (S6 시나리오)
- [ ] 한 손 플레이 + 색맹 친화 + 44pt 통과 (S1/S4/S5)

## 6. 알려진 위험 + 완화

| 위험 | Sprint | 완화 |
|---|---|---|
| 사용자 수동 PNG 미도착 | 4 | Sprint 1~3+5+6 진행. PNG 도착 시 Sprint 4 진입 |
| 5005 PanelToggleManager 자동 탐색 누락 | 2 | _excludePanelNames 검토 (CharacterPanel 외 신규 추가 panel 검토) |
| Inspector wiring 누락 (10 entry) | 2.2 | 사용자 wiring 단계 명세 + Sprint 5 Test 로 체크 |
| TMP 사용 불가 | 모두 | Legacy Text + outline 활용 |
| 한 손 가림 시 우측 그룹 침범 | 1 | Sprint 1 시안 사용자 확인 후 진행 |

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 1 산출 — 디자인 v1 옵션 권장 채택 가정).
