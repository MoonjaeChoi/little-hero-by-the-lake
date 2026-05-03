# 5006 — Attic Entry UI Redesign v1 — Sprint PRD v2 (mediator 최종)

> **Mediator 결정**: D1=B / D2=C / D3=C / D4=A / D5=B (사용자 "권장안 그대로", 2026-05-04)
> **변경 핵심**: Sprint 4 PNG 임포트 → 5006.1 후속 분리. 본 PRD = 5 Sprint / 8 commit / 3시간 50분.
> **출처**: v1 PRD + critique + rebuttal + decisions

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | AtticGroupLayoutPresenter + 4 그룹 RectTransform + 13 버튼 tint (영구) | 2 | 60분 |
| 2 | AtticPanelEntryView + 10 panel 진입 wiring | 2 | 60분 |
| 3 | CharInfo/Forge OnEnable + EndgameMenuPanel.OnEnable 자체 검증 (D3=C) | 2 | 45분 |
| 4 | NUnit AtticEntryUITests 6 + Validator 통과 | 1 | 40분 |
| 5 | docs (changelog + CLAUDE.md §11/§12/§14) | 1 | 25분 |
| **총** | | **8** | **3시간 50분** |

### 후속 PRD 5006.1 (PNG 도착 후 별도 사이클)

| Sprint | 작업 | commit |
|---|---|---|
| 5006.1.1 | AtticUIIconImporter (9-slice 버튼 border=12, 화폐 0, prefix 분기) | 1 |
| 5006.1.2 | 14 PNG 임포트 + Sprite 교체 + tint Color.white 복원 | 1 |
| **5006.1 총** | | **2** |

## 2. Sprint 명세

### Sprint 1 — AtticGroupLayoutPresenter + tint 영구 (2 commit)

#### 1.1 AtticGroupLayoutPresenter.cs 신규

**파일**: `Assets/_Project/Scripts/Hub/AtticGroupLayoutPresenter.cs`

**산출물**:
- 클래스 `AtticGroupLayoutPresenter : MonoBehaviour` (~50L)
- 4 SerializeField RectTransform: `_group1Combat / _group2Equipment / _group3Shop / _group4Record`
- `_minTouchSize = 44f` SerializeField (CLAUDE.md §2)
- `OnValidate()` — 4 그룹 자식 검사 + 44pt 이하 시 LogWarning
- **D4=A**: 4 RectTransform 자식 GameObject 이름 = `Group1Combat / Group2Equipment / Group3Shop / Group4Record` (Panel 접미어 X → AtticPanelToggleManager 자동 영향 0)

**검증 기준**:
- 컴파일 errors=0
- §15.6 100L 통과 (~50L)
- Inspector 4 RectTransform 드래그 가능
- Attic.unity 4 빈 RectTransform 자식 추가 (anchor 좌상/좌하/우상/우하)

#### Sprint 1.1 commit

```
5006 Sprint 1.1 / T01 — AtticGroupLayoutPresenter + 4 그룹 RectTransform (1/8)

[변경]
- Scripts/Hub/AtticGroupLayoutPresenter.cs 신규 (~50L)
- Scenes/Attic.unity — 4 RectTransform 자식 (Group1Combat/Group2Equipment/Group3Shop/Group4Record)

[검증]
- 컴파일 errors=0
- 44pt 터치 OnValidate
- Panel 접미어 X (D4=A, AtticPanelToggleManager 영향 0)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 1.2 13 버튼 tint 영구 (D2=C)

**작업**:
- 4 그룹 RectTransform 안에 기존 13 버튼 GameObject 재배치 (Inspector parent 변경)
- 그룹별 ButtonImage.color tint 영구 적용 (G1 빨강 / G2 파랑 / G3 보라 / G4 베이지)
- 신규 PNG 임포트 = 5006.1 후속 (별도 사이클)

**검증 기준**:
- Play 시 4 그룹 색 구분 가능 (영구)
- 기존 onClick 동작 회귀 X
- AtticController._gachaButton/_starforceButton/_limitedShopButton 참조 유지 (5005 잔여 책임 — 부모 변경 시 reference 보존)

#### Sprint 1.2 commit

```
5006 Sprint 1.2 / T02 — 13 버튼 4 그룹 재배치 + tint 영구 (D2=C, 2/8)

[변경]
- Scenes/Attic.unity — 13 Button GameObject 4 그룹 RectTransform 자식으로 이동
- 13 Button.Image.color override (G1 빨강/G2 파랑/G3 보라/G4 베이지) — 5006.1 PNG 임포트 시점에 Color.white 복원

[검증]
- 4 그룹 시각 구분 (Play 모드)
- AtticController 잔여 3 button reference 보존
- onClick 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 2 — AtticPanelEntryView + 10 panel wiring (2 commit)

#### 2.1 AtticPanelEntryView.cs 신규

**파일**: `Assets/_Project/Scripts/Hub/AtticPanelEntryView.cs`

**산출물**:
- 클래스 `AtticPanelEntryView : MonoBehaviour` (~80L)
- 내부 `[Serializable] PanelEntry { id, button, panelRoot }`
- `_entries[]` SerializeField
- `OnEnable` / `OnDisable` 에서 onClick 등록·해제
- 람다 captured local var 패턴 (5005 AtticChapterEntry/AtticCharacterSelector 답습)

**검증 기준**:
- 컴파일 errors=0
- §15.6 100L 통과 (~80L)
- 5005 패턴 정합

#### Sprint 2.1 commit

```
5006 Sprint 2.1 / T03 — AtticPanelEntryView 신규 (3/8)

[변경]
- Scripts/Hub/AtticPanelEntryView.cs 신규 (~80L)
- 5005 패턴 답습 (PanelEntry [Serializable] + onClick captured local)

[검증]
- 컴파일 errors=0
- §15.6 100L 통과
- §15.7 DI — SerializeField 명시

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 2.2 Attic.unity 10 panel wiring

**작업**:
- AtticPanelEntryView 컴포넌트 부착 (Attic GameObject)
- _entries 10개 wiring (Inspector):
  - id="diary" / Button=DiaryButton / panelRoot=DiaryPanel.gameObject
  - id="bestiary" / Button=BestiaryButton / panelRoot=BestiaryPanel.gameObject
  - id="char_info" / Button=CharInfoButton / panelRoot=CharacterInfoPanel.gameObject
  - id="meta_upgrade" / Button=MetaUpgradeButton / panelRoot=MetaUpgradePanel.gameObject
  - id="inventory" / Button=InventoryButton / panelRoot=InventoryPanel.gameObject
  - id="starlight_shop" / Button=StarlightShopButton / panelRoot=StarlightShopPanel.gameObject
  - id="wondrous_map" / Button=WondrousMapButton / panelRoot=WondrousMapPanel.gameObject
  - id="conversion_forge" / Button=ConversionForgeButton / panelRoot=ConversionForgePanel.gameObject
  - id="endgame_menu" / Button=EndgameMenuButton / panelRoot=EndgameMenuPanel.gameObject
  - id="enhance" / Button=EnhanceButton / panelRoot=EnhanceModalPanel.gameObject (또는 InventoryPanel 진입 후 modal)

> 5005 잔여 3 (gacha/starforce/limited_shop) 은 AtticController 그대로

**검증 기준**:
- Play 시 10 버튼 클릭 → panel 활성
- 5005 PanelToggleManager 자동 비활 (한 번에 하나만)
- 5005 잔여 3 회귀 X

#### Sprint 2.2 commit

```
5006 Sprint 2.2 / T04 — Attic.unity 10 panel wiring (4/8)

[변경]
- Scenes/Attic.unity — AtticPanelEntryView._entries 10개 wiring

[검증]
- 10 진입 버튼 → panel 활성 OK
- 5005 PanelToggleManager 정합 (한 번에 하나만)
- 5005 잔여 3 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 3 — Panel OnEnable 보강 (2 commit)

#### 3.1 CharacterInfoPanel/ConversionForgePanel OnEnable 갱신

**파일**:
- `Assets/_Project/Scripts/UI/CharacterInfoPanel.cs`
- `Assets/_Project/Scripts/UI/ConversionForgePanel.cs`

**작업** (필요 시):
- OnEnable 에서 데이터 갱신 (캐릭터 정보 / 변환 모루 화폐 표시) — 이미 구현되어 있으면 변경 X
- 단순 panel.SetActive(true) 만으로 활성 시 데이터 표시 보장

**검증 기준**:
- AtticPanelEntryView 진입 시 데이터 표시 (콘솔 에러 X)

#### Sprint 3.1 commit

```
5006 Sprint 3.1 / T05 — CharacterInfoPanel/ConversionForgePanel OnEnable 데이터 갱신 (5/8)

[변경]
- Scripts/UI/CharacterInfoPanel.cs / ConversionForgePanel.cs — OnEnable 갱신 hook (필요 시)

[검증]
- AtticPanelEntryView 진입 시 데이터 표시 OK

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

#### 3.2 EndgameMenuPanel.OnEnable 자체 검증 (D3=C)

**파일**: `Assets/_Project/Scripts/UI/EndgameMenuPanel.cs`

**산출물**:
- OnEnable 메서드에 firstClearedChapters 35 검사 + 추억 4종 발견 둘 중 하나 만족
- 미해당 시 `gameObject.SetActive(false)` + Debug.Log 토스트
- 기존 OnEnable 로직 보존

**검증 기준**:
- CH 35 미클리어 — 진입 시 즉시 비활 + 콘솔 토스트
- CH 35 클리어 후 — 정상 표시
- AtticEndgameMenuView 와 결합도 0 (Panel 자체 검증)

#### Sprint 3.2 commit

```
5006 Sprint 3.2 / T06 — EndgameMenuPanel.OnEnable 자체 검증 (D3=C, 6/8)

[변경]
- Scripts/UI/EndgameMenuPanel.cs — OnEnable 에 firstClearedChapters 35 검사 추가 (~5L)

[검증]
- CH 35 미클리어 시 진입 즉시 비활 + 토스트
- CH 35 클리어 후 정상

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

### Sprint 4 — NUnit AtticEntryUITests + Validator (1 commit)

#### 4.1 NUnit Test 6+

**파일**: `Assets/_Project/Tests/Editor/AtticEntryUITests.cs`

**산출물**:
- `AtticPanelEntryView_Type_Exists`
- `AtticGroupLayoutPresenter_Type_Exists`
- `AtticPanelEntryView_HasPanelEntry_NestedClass`
- `AtticGroupLayoutPresenter_MinTouchSize_44`
- `EndgameMenuPanel_HasOnEnable_Validation` (Reflection — OnEnable 메서드 존재)
- `AtticEntryUI_5005_RegressionPass` (5005 6 컴포넌트 모두 Type.GetType — 회귀 검사)

**검증 기준**:
- Test Runner 6/6 PASS
- 누적 NUnit 67 → 73

#### Sprint 4 commit

```
5006 Sprint 4 / T07 — AtticEntryUITests 6 NUnit + Validator 통과 (7/8)

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

### Sprint 5 — docs + CLAUDE.md (1 commit)

**작업**:
- `docs/little_hero/changelog_v1.md` — 5006 attic_entry_ui_redesign_v1 항목 (8 commit 묶음)
- `CLAUDE.md §11` — 마일스톤 row 추가
- `CLAUDE.md §12` — UI 시스템 레이어에 AtticPanelEntryView/AtticGroupLayoutPresenter 등재 + Tests/Editor 67→73
- `CLAUDE.md §14` — 최근 5건 회전 (5006 등재)

#### Sprint 5 commit

```
5006 Sprint 5 / T08 — docs (changelog + CLAUDE.md §11/§12/§14) (8/8)

[변경]
- docs/little_hero/changelog_v1.md — 5006 항목 추가
- CLAUDE.md §11 — attic_entry_ui_redesign_v1 마일스톤
- CLAUDE.md §12 — UI 컴포넌트 등재 + NUnit 67→73
- CLAUDE.md §14 — 최근 5건 회전 (5006 등재)

[검증]
- Phase 5 종결 (8/8 commit)
- 5006.1 후속 anchor 명시 (PNG 도착 후 별도)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

## 3. 의존성 그래프

```
Sprint 1 (Layout + tint 영구) ─→ Sprint 2 (PanelEntryView + wiring) ─→ Sprint 3 (Panel OnEnable) ─┐
                                                                                                    ├→ Sprint 4 (NUnit) ─→ Sprint 5 (docs)
```

**병렬 가능**: 없음 (순차 — 각 commit 의 컴파일 검증 의존)

## 4. 검증 시나리오 (Play 검증)

### S1 — 4 그룹 시각 구분

1. Play Mode 진입 → Attic 씬
2. **검증**: 13 버튼 4 그룹 색 구분 (G1 빨강 / G2 파랑 / G3 보라 / G4 베이지, tint 영구)
3. **검증**: 좌측 (G1+G2) / 우측 (G3+G4) 분산 → 한 손 플레이 가독성

### S2 — 10 panel 진입 동작

1. 일기 버튼 클릭 → DiaryPanel 활성
2. **검증**: Panel 표시 + 데이터 갱신
3. 도감 버튼 클릭
4. **검증**: DiaryPanel 자동 비활 (5005) + BestiaryPanel 활성
5. 10 버튼 모두 반복 — 가챠/스타포스/한정상점 (5005 잔여) 회귀 X

### S3 — EndgameMenu 가용성 (D3=C)

1. 새 SaveData (CH 35 미클리어) → Attic 진입
2. EndgameMenu 버튼 클릭
3. **검증**: EndgameMenuPanel 즉시 비활 + 콘솔 로그 "[EndgameMenuPanel] CH 35 클리어 후 도전 가능"
4. Editor 메뉴 `Set Endgame Menu Active` 실행 후 재시도
5. **검증**: 정상 표시

### S4 — 44pt 터치 타겟

1. AtticGroupLayoutPresenter._minTouchSize = 44
2. Inspector OnValidate
3. **검증**: 콘솔 warning 0건 (모든 자식 44+ 통과)

### S5 — 5005 회귀

1. AtticController 가챠/스타포스/한정상점 → 동작 유지
2. AtticCurrencyView 4 화폐 → 동작 유지
3. AtticCharacterSelector 3 캐릭터 → 동작 유지
4. AtticChapterEntry 35 챕터 + 5002 D2 toast → 동작 유지
5. AtticEndgameMenuView 가용성 → 동작 유지
6. AtticPanelToggleManager 한 번에 하나만 → 정상

## 5. Definition of Done

- [ ] Sprint 1~5 모두 commit (8 commit)
- [ ] AtticPanelEntryView (~80L) + AtticGroupLayoutPresenter (~50L) — 모두 §15.6 100L 이내
- [ ] AtticController 본체 라인 변동 X (BGM + 가챠/스타포스/한정상점 그대로, 111L 유지)
- [ ] 13 진입 버튼 모두 onClick → panel 활성화
- [ ] 13 버튼 4 그룹 RectTransform 안에 재배치 + tint 영구
- [ ] EndgameMenuPanel.OnEnable 자체 검증 (D3=C)
- [ ] NUnit AtticEntryUITests 6 PASS (누적 67 → 73)
- [ ] ProjectValidator 신규 위반 0
- [ ] CLAUDE.md §11/§12/§14 갱신 + changelog 등재
- [ ] 5005 6 컴포넌트 회귀 X (S5)
- [ ] 한 손 플레이 + 색맹 친화 + 44pt 통과 (S1/S4)
- [ ] **5006.1 후속 사이클 anchor 명시** (PNG 도착 후 PNG 임포트 + Color.white 복원)

## 6. 알려진 위험 + 완화

| 위험 | Sprint | 완화 |
|---|---|---|
| 5005 PanelToggleManager 자동 탐색 누락 | 2 | 4 그룹 이름 규칙 회피 (D4=A) — Group1Combat/Group2Equipment 등 |
| Inspector wiring 누락 (10 entry) | 2.2 | 사용자 wiring 단계 명세 + Sprint 4 NUnit 회귀 검사 |
| TMP 사용 불가 | 모두 | Legacy Text + outline 활용 |
| 한 손 가림 | 1 | Sprint 1 시안 사용자 확인 후 진행 |
| 5005 잔여 3 button reference 끊김 | 1.2 | parent 이동 시 reference 보존 (Inspector 자동 유지) |

## 7. 변경 이력

- 2026-05-04: v2 작성 (Mediator 결정 5건 일괄 채택, Sprint 4 → 5006.1 분리, 8 commit / 3시간 50분)
