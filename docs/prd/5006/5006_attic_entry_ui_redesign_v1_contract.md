# Sprint Contract: 5006 attic_entry_ui_redesign_v1

> **PGE Planner — Phase 4 Contract**
> v2 PRD: `5006_attic_entry_ui_redesign_v1_v2_prd.md` · breakdown: `_breakdown_00.md`
> 생성일: 2026-05-04

## 1. C1 컴파일 기준

- **C1.1**: 신규 .cs 파일 컴파일 errors=0 (T01 AtticGroupLayoutPresenter, T03 AtticPanelEntryView, T07 AtticEntryUITests)
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: 8 commit 모두 errors=0

**검증 방법**:
```
mcp__UnityMCP__refresh_unity (compile=request)
sleep 8  # 도메인 리로드
mcp__UnityMCP__read_console (errors)  # 0
```

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 | 위치 |
|---|---|---|
| `AtticEntryUITests` | 6 | `Tests/Editor/AtticEntryUITests.cs` |

### 신규 NUnit (6)

- `AtticPanelEntryView_Type_Exists` — Type.GetType("LittleHero.AtticPanelEntryView, Assembly-CSharp") != null
- `AtticGroupLayoutPresenter_Type_Exists` — 동
- `AtticPanelEntryView_HasPanelEntry_NestedClass` — Reflection 으로 nested type "PanelEntry" 존재
- `AtticGroupLayoutPresenter_MinTouchSize_44` — Reflection 으로 _minTouchSize SerializeField 기본값 44
- `EndgameMenuPanel_HasOnEnable_Method` — Reflection 으로 EndgameMenuPanel.OnEnable 존재
- `AtticEntryUI_5005_RegressionPass` — 5005 6 컴포넌트 (AtticCurrencyView/CharacterSelector/ChapterEntry/EndgameMenuView/PanelToggleManager/ReturnHandler) 모두 Type.GetType 통과

**검증**: Test Runner Edit Mode > Run All → 6/6 PASS, 누적 NUnit 67 → 73

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ±0 (5005 종료 시점 기준)
- **C3.3**: 신규 위반 없음 (§15.6 200L LINE_LIMIT 통과)
  - AtticGroupLayoutPresenter ~50L ✅
  - AtticPanelEntryView ~80L ✅
  - AtticController 본체 변동 X (111L 그대로) ✅
  - EndgameMenuPanel +5L (현재 ~180L → ~185L, 200L 안전) ✅

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.AtticGroupLayoutPresenter` | `_group1Combat / _group2Equipment / _group3Shop / _group4Record` (RectTransform SerializeField) + `_minTouchSize` (float) + `OnValidate()` |
| `LittleHero.AtticPanelEntryView` | `_entries` (PanelEntry[] SerializeField) + nested `PanelEntry { id, button, panelRoot }` + `OnEnable()` / `OnDisable()` |
| `LittleHero.EndgameMenuPanel` | `OnEnable()` 메서드 존재 (D3=C 검증 hook) |
| 5005 회귀 7 Type | AtticController / AtticCurrencyView / AtticCharacterSelector / AtticChapterEntry / AtticEndgameMenuView / AtticPanelToggleManager / AtticReturnHandler 모두 존재 |

**검증 방법**:
```csharp
var t = System.Type.GetType("LittleHero.AtticPanelEntryView, Assembly-CSharp");
Assert.NotNull(t);
var nested = t.GetNestedType("PanelEntry");
Assert.NotNull(nested);
```

## 5. C5 SO 검증

- **5006 신규 SO 0** — 본 시스템은 SO 추가 X
- **회귀 검증**: 기존 SO (BalanceConfig / EquipmentTemplate / CharacterData / ChapterData / EnemyData / BossLootTable / GachaPool 등) 변경 X

## 6. C6 Play 시나리오

### S1 — 4 그룹 시각 구분

**사전 조건**:
- 새 SaveData (또는 기존)
- T02 commit 후 (4 그룹 재배치 + tint 영구)

**단계**:
1. Play Mode 진입 → Attic 씬 자동 로드
2. 13 버튼 시각 확인

**기대 결과**:
- 13 버튼이 4 그룹 색으로 구분 (G1 빨강 / G2 파랑 / G3 보라 / G4 베이지)
- 좌측 (G1+G2 = 전투+장비) / 우측 (G3+G4 = 상점+기록) 분산
- 콘솔 warning 0건 (44pt 통과)

---

### S2 — 10 panel 진입 동작

**사전 조건**:
- T04 commit 후 (10 wiring)

**단계**:
1. 일기 버튼 클릭
2. 도감 버튼 클릭 (일기 panel 활성 상태에서)
3. 10 버튼 모두 순차 클릭
4. 가챠 버튼 클릭 (5005 잔여)

**기대 결과**:
- 일기 panel 활성 → 도감 panel 활성 시 일기 자동 비활 (5005 PanelToggleManager)
- 콘솔 에러 0건
- 10 panel 모두 표시 + 데이터 갱신
- 가챠 panel 정상 (5005 잔여 회귀 X)

---

### S3 — EndgameMenu 가용성 (D3=C 검증)

**사전 조건**:
- T06 commit 후
- 새 SaveData (CH 35 미클리어, firstClearedChapters=0)

**단계**:
1. Play Mode 진입 → Attic
2. EndgameMenu 버튼 클릭

**기대 결과**:
- EndgameMenuPanel 즉시 비활 (gameObject.SetActive(false))
- 콘솔 로그: `[EndgameMenuPanel] CH 35 클리어 후 도전 가능`

**후속 검증**:
1. Editor 메뉴 `Tools/LittleHero/Set Endgame Menu Active` 실행
2. Play Mode 재진입
3. EndgameMenu 버튼 클릭 → **정상 표시** (panel 활성)

---

### S4 — 44pt 터치 타겟 (Editor 검증)

**단계**:
1. Inspector 에서 AtticGroupLayoutPresenter 컴포넌트 선택
2. 4 그룹 RectTransform 자식 sizeDelta 검사

**기대 결과**:
- OnValidate 콘솔 warning 0건 (모든 자식 44+ 통과)

---

### S5 — 5005 6 컴포넌트 회귀

**단계**:
1. Play Mode 진입 → Attic
2. 캐릭터 선택 (3 portrait)
3. 챕터 1 진입 → 클리어 → 복귀 (5002 D2 toast)
4. 4 화폐 표시 갱신 (★/✦/◇/⋆)
5. 가챠/스타포스/한정상점 진입 (5005 잔여)

**기대 결과**:
- 5005 6 컴포넌트 모두 정상
- ChapterClearedPanel 1회 toast (5002 D2)
- AtticController 잔여 3 button 동작 회귀 X

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (8 commit)
- [ ] Editor Test 6/6 PASS (누적 73)
- [ ] ProjectValidator warnings 변동 ±0
- [ ] 5005 회귀 X (NUnit AtticDecomposition 6 + AtticEntryUI_5005_RegressionPass)
- [ ] 60fps 유지 (UI 변경, 영향 미미)
- [ ] 빌드 성공 (해당 시)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 5/6 이상 ✅
- C3 Validator errors=0 ✅
- C4 Reflection 7 Type 통과 ✅
- C5 SO 회귀 X ✅
- 시나리오 S1, S2 PASS

**완전 합격**:
- 모든 카테고리 ✅
- 시나리오 S1~S5 모두 PASS
- 비기능 요구사항 모두 ✅
- 5006.1 anchor 명시 (PNG 도착 후 별도)

## 9. Trade-off (mediator 결정 기반)

- **D1=B**: Sprint 4 PNG 임포트 → 5006.1 분리. 본 PRD 즉시 진행, PNG 도착 변수 흡수.
- **D2=C**: Sprint 1.2 tint 영구 — 5006.1 시점에 Color.white 복원. 작업 중복 1회 (5006.1 책임).
- **D3=C**: EndgameMenuPanel.OnEnable 자체 검증. 결합도 0 + 가장 단순.
- **D4=A**: 4 그룹 이름 규칙 회피 (Group{N}{Cat}). _excludePanelNames 변경 X.
- **D5=B**: AtticUIIconImporter 9-slice 분기 (5006.1 시점, 본 PRD 범위 X).

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract).
