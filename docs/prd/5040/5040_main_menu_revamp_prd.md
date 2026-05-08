# 5040 main_menu_revamp — Sprint PRD

> **Phase 1 Generate 산출** — 디자인 `docs/prd/5040_main_menu_revamp.md` 답습
> 일시: 2026-05-08 · 6 Sprint / ~10 commit / 약 4h

---

## 1. Sprint 표

| # | 영역 | 작업 | 산출 | commit | 시간 |
|---|---|---|---|---|---|
| S1 | UI Logic | BottomNavBar + Tab 시스템 | `BottomNavBar.cs` (~80L) + `TabBase.cs` (~30L) | 1 | 30m |
| S2 | UI | TopBar + CurrencyStripView | `MainMenuTopBar.cs` (~60L) + 기존 AtticCurrencyView 흡수 | 1 | 30m |
| S3 | UI | CenterStage + MainCTAButton | `CharacterStageView.cs` (~50L) + `MainCTAButton.cs` (~50L) | 1 | 40m |
| S4 | UI | ChapterBookCarousel + ChapterCardView | `ChapterBookCarousel.cs` (~90L) + `ChapterCardView.cs` (~60L) | 1 | 50m |
| S5 | UI | SideQuickBar (FOMO) | `SideQuickBar.cs` (~40L) | 1 | 20m |
| S6 | UI Integration | MainMenuController + 11 panel TabContent wiring | `MainMenuController.cs` (~80L) + AtticController 변경 | 1 | 40m |
| S7 | Scene | Attic.unity 갱신 (또는 MainMenu.unity 신규) | scene 변경 | 1 | 30m |
| S8 | Test | NUnit (BottomNavBar / Carousel 분기 / CurrencyStrip) | NUnit 6 | 1 | 30m |
| S9 | Docs | changelog + CLAUDE.md §11 §14 | docs | 1 | 15m |

**합계**: 9 Sprint / 9 commit / 약 4h05m

---

## 2. 신규 클래스 명세

### 2.1 `BottomNavBar.cs` (~80L)
**경로**: `Scripts/UI/MainMenu/BottomNavBar.cs`
- 5 tab button (Hero / Equipment / Adventure★ / Shop / Record)
- TabSelected event (TabBase 인터페이스 호환)
- default = Adventure
- Active tab 시각 강조 (아이콘 색 / scale)

### 2.2 `MainMenuTopBar.cs` (~60L)
**경로**: `Scripts/UI/MainMenu/MainMenuTopBar.cs`
- ProfilePill (Lv + 닉네임 + EP)
- CurrencyStripView (4 화폐 칩 — 별가루 / 무지개구슬 / 마법결정 / 별빛)
- Mailbox + Notice icon
- AtticCurrencyView 흡수 (5037.5 답습)

### 2.3 `CharacterStageView.cs` (~50L)
**경로**: `Scripts/UI/MainMenu/CharacterStageView.cs`
- 선택 캐릭터 portrait 표시
- MetaProgression.OnCharacterChanged 구독
- 다락방 책상 위 위치 (Inspector wiring)

### 2.4 `MainCTAButton.cs` (~50L)
**경로**: `Scripts/UI/MainMenu/MainCTAButton.cs`
- 다음 챕터 자동 추천 (firstClearedChapters 기반)
- 권장 레벨 표시 (5034 BalanceConfig 답습)
- 큰 button (44pt + scale)
- Click → SceneTransitionService.Transition("Chapter")

### 2.5 `ChapterBookCarousel.cs` (~90L)
**경로**: `Scripts/UI/MainMenu/ChapterBookCarousel.cs`
- 7 권 × 5 챕터 (V2) + 3 권 (Hardcore)
- 좌/우 화살표 또는 swipe
- ChapterCardView prefab (좌/우 페이지)
- Page Indicator (1~10)
- WondrousMapPanel 대체

### 2.6 `ChapterCardView.cs` (~60L)
**경로**: `Scripts/UI/MainMenu/ChapterCardView.cs`
- chapterNumber + 제목
- 잠금/해방/완료 상태 (5037.5 답습)
- 별점 0~3 (해방 후 클리어 별 수)
- 권장 레벨 + 첫 클리어 보상
- 큰 thumbnail (chapter.coverArt)
- Click → MainMenuController.SelectChapter

### 2.7 `SideQuickBar.cs` (~40L)
**경로**: `Scripts/UI/MainMenu/SideQuickBar.cs`
- DailyMission / SeasonPass / LimitedShop 알림 dot
- Click → 해당 panel open

### 2.8 `MainMenuController.cs` (~80L)
**경로**: `Scripts/UI/MainMenu/MainMenuController.cs`
- 5-tab 컨텐츠 wiring
- AtticController 협업 (또는 흡수)
- Default tab = Adventure
- Tab 전환 → ContentPanel 동적 변경

### 2.9 `TabBase.cs` (~30L) — 추상 base
**경로**: `Scripts/UI/MainMenu/TabBase.cs`
- abstract class — Hero/Equip/Adv/Shop/Record 자식
- OnEnter / OnExit hooks

---

## 3. Sprint 의존성

```
S1 (BottomNavBar) ─┐
S2 (TopBar) ───────┤
S3 (Stage + CTA) ──┤
                   ├─→ S6 (MainMenuController) ──→ S7 (Scene) ──→ S8 (Test) ──→ S9 (Docs)
S4 (Carousel) ─────┤
S5 (SideQuickBar) ─┘
```

**병렬 가능**: S1 + S2 + S3 + S4 + S5 (모두 독립 UI 컴포넌트)
**직렬**: S6 → S7 → S8 → S9

---

## 4. 검증 시나리오 (Phase 6 C6 반영)

### 시나리오 1: BottomNav 전환
- Default = Adventure tab → ChapterBookCarousel 표시
- Hero tab tap → CharacterInfo 표시
- Shop tab tap → Gacha + Starlight + Limited 표시
- 각 tab 전환 정상

### 시나리오 2: ChapterBookCarousel
- 다락방 진입 → 동화책 펼친 시각
- 좌/우 화살표 → 다음/이전 페이지
- 챕터 카드 tap → ChapterEntryConfirmModal (현재 답습)
- 권장 레벨 / 별점 / 보상 표시 정상

### 시나리오 3: MainCTAButton
- 다음 챕터 자동 추천 (마지막 클리어 + 1)
- Click → 권장 레벨 경고 (DevUnlockAllChapters 시 skip)
- Chapter 씬 진입 정상

---

## 5. 폐기 / 흡수

### 폐기
- `WondrousMapPanel` (ChapterBookCarousel 대체)
- `AtticGroupLayoutPresenter` (BottomNavBar 대체)
- `AtticPanelEntryView` (TabContent 흡수)
- `AtticChapterEntry` (MainCTAButton + Carousel 대체)
- `AtticEndgameMenuView` (Adventure tab 흡수)

### 흡수
- `AtticCurrencyView` → MainMenuTopBar.CurrencyStripView
- `AtticReturnHandler` → MainMenuController (또는 보존)
- 11 Panel logic → Tab Content 안에 instantiate

### 보존
- `AtticController` (111L) — 최상위 진입점, MainMenuController 위임
- 다른 11 panel logic (Inventory/Gacha 등) — Tab 안에서 호출

---

## 6. CLAUDE.md §15 OOP 정합

| 정책 | 본 PRD |
|---|---|
| §15.1 SRP | TopBar/Stage/CTA/Carousel/SideQuick 분리 — 단일 책임 |
| §15.4 SO 우선 | ChapterCardView가 ChapterData SO 사용 |
| §15.6 100L | 모든 신규 클래스 30~90L (TabBase 30L + Carousel 90L) |
| §15.7 DI | Initialize 명시 주입 (controller → 자식) |
| §15.9 Test | NUnit 6 (BottomNav / Carousel 분기) |
| §15.11 안티 | public 필드 X / Update X (event 기반) |

---

## 7. 5037~5038 호환

- 5037 ChapterVariantSeeder + Variant SO — 영향 X (적 구조 보존)
- 5037.5 EndgameModeData enemyPool — Adventure tab 의 Endgame 진입 wiring
- 5038 ChapterData.waves[] — ChapterCardView 가 wave 정보 표시 가능
- 5038.5 ChapterBackgroundWirer — ChapterCard.thumbnail 활용
- 5036 DevUnlockAllChapters — MainCTAButton + Carousel 가드 보존

---

## 8. 변경 이력
- 2026-05-08: 초기 작성 (Phase 1 Generate)
