# 5040 main_menu_revamp — 탕탕특공대 답습 + 동화책 메타포

> **목적**: LittleHero 시작화면을 탕탕특공대/Survivor.io 패턴 + 동화책 메타포로 재구성. 11 산재 panel → 5-tab 통합. 35 챕터 — 동화책 carousel.
>
> **사용자 요청**: "탕탕특공대 초기화면 자세히 분석하여 초기화면의 메뉴구성과 각 챕터 표현 방식을 채용할 수 있는 부분을 파악해서 새롭게 시작화면을 구성해줘."

---

## 1. 탕탕특공대 답습 5 패턴

| 패턴 | LittleHero 적용 |
|---|---|
| **상단 화폐 고정** + 프로필 / 알림 | TopBar — 4 화폐 칩 (별가루 / 무지개구슬 / 마법결정 / 별빛) + 프로필 / 우편 |
| **중앙 캐릭터 stage** (장비 visual) | CenterStage — 다락방 배경 + 캐릭터 portrait (V5 sticker) |
| **하단 5-tab nav** | BottomNavBar — 영웅 / 장비 / 모험★ / 상점 / 기록 |
| **거대 출격 버튼** (다음 챕터 자동) | MainCTAButton — 다락방 위 "다음 챕터 출격" + 권장 레벨 |
| **사이드 quick FOMO** | SideQuickBar — 일일미션 / 시즌패스 / 한정상점 (작은 알림 dot) |

---

## 2. 11 Panel → 5 그룹 통합

| 그룹 (5-tab) | 흡수 Panel |
|---|---|
| **영웅** (Hero) | CharacterInfoPanel + 캐릭터 선택 + 추억 잠금 |
| **장비** (Equipment) | InventoryPanel + EquipmentCardPanel + EnhanceModalPanel + SlotStarforcePanel + 슬롯 강화 |
| **모험★** (Adventure, default) | ChapterBookCarousel + 보스 도전 + 무한 다락방 + Endgame Daily/Weekly |
| **상점** (Shop) | GachaPanel + StarlightShopPanel + LimitedShopPanel + ConversionForgePanel |
| **기록** (Record) | DiaryPanel + BestiaryPanel (도감) + MetaUpgradePanel (마음 단련) |

---

## 3. ChapterBookCarousel — 동화책 메타포

### 시각
- **다락방 책상 위에 펼쳐진 동화책** sprite (V5 sticker dynamic)
- 좌우 페이지 = 2 챕터 카드
- 좌/우 swipe (또는 화살표 버튼)
- 35 챕터 = **7 권 × 5 챕터** (책 페이지 메타포)

### 챕터 카드
- 챕터 번호 + 제목
- 잠금/해방/완료 상태 (별점 0~3)
- 권장 레벨 (5034 답습)
- 첫 클리어 보상 (별빛 50⋆)
- 큰 thumbnail (Kenney/V5 sticker BG sample)

### V2/Hardcore 페이지 분리
- 페이지 1~7: V2 35 챕터
- 페이지 8~10: Hardcore 36~50 (3 권)

---

## 4. Canvas 계층 (신규)

```
MainMenuCanvas (Attic.unity 신규 또는 기존 Canvas 갱신)
├─ AtticBackground (보존 — 다락방 BG sprite)
├─ TopBar
│   ├─ ProfilePill (Lv + 닉네임 + EP)
│   ├─ CurrencyStripView (4 화폐 칩)
│   └─ Mailbox + Notice icon
├─ CenterStage
│   ├─ CharacterStageView (선택 캐릭터 portrait)
│   └─ MainCTAButton (다음 챕터 자동 출격)
├─ ChapterBookCarousel (active 시 — 모험 tab)
│   ├─ BookSpread (좌우 펼친 동화책 sprite)
│   ├─ ChapterCardLeft (chapterCard prefab)
│   ├─ ChapterCardRight
│   └─ Page Indicator (1~7 / V2 vs Hardcore)
├─ SideQuickBar
│   ├─ DailyMissionDot
│   ├─ SeasonPassDot
│   └─ LimitedShopDot
├─ BottomNavBar
│   ├─ TabHero
│   ├─ TabEquipment
│   ├─ TabAdventure (default selected, 큰 ★)
│   ├─ TabShop
│   └─ TabRecord
└─ ContentPanel (탭 별 동적 컨텐츠 — 기존 11 panel 흡수)
```

---

## 5. 진입 흐름

```
게임 시작 → 다락방 (Attic.unity)
  ↓
Default tab = Adventure (모험)
  ↓
ChapterBookCarousel + MainCTAButton 표시
  ↓
사용자 swipe → 다른 챕터 카드 선택
또는 MainCTAButton tap → 다음 챕터 자동 출격
  ↓
SceneTransitionService.Transition("Chapter") (5004 답습)
```

탭 전환 — 단일 화면 내 ContentPanel 변경 (씬 전환 X).

---

## 6. PG-12 + 지브리 정서 + V5 sticker 보존

- 다락방 BG 보존 — 동화책 펼친 책상
- 캐릭터 stage — V5 sticker portrait
- BottomNav icon — V5 sticker 톤 (또는 Kenney UI 9-slice)
- 챕터 카드 thumbnail — V5 sticker BG sample (5008 baseline)
- 어휘 — "출격" / "모험" / "도전" (잔인 X)

---

## 7. 폐기 vs 보존

### 폐기 (또는 BottomNav 흡수)
- `AtticGroupLayoutPresenter` — BottomNavBar 대체
- `AtticPanelEntryView` — TabContent 흡수
- `AtticChapterEntry` — MainCTAButton + Carousel 대체
- `AtticEndgameMenuView` — Adventure tab 흡수
- `WondrousMapPanel` — ChapterBookCarousel 대체

### 보존
- `AtticController` (111L) — 최상위 컨트롤러 보존, 일부 책임 위임
- `AtticCurrencyView` — TopBar 의 CurrencyStripView 흡수
- `AtticReturnHandler` — Chapter 씬 → Attic 복귀 로직 보존
- 11 panel 자체 (logic) — Tab Content 안에 instantiate 또는 prefab reuse

---

## 8. 위험 + 완화

| 위험 | 완화 |
|---|---|
| Attic.unity scene 큰 변경 → wiring 실패 | 신규 MainMenu.unity scene 별도, 사용자 검증 후 Attic 대체 |
| 11 panel 의 instantiation 경로 복잡 | TabContent 가 panel prefab dynamic load |
| 5037/5038/5038.5 의 cascade 와 충돌 | 모든 cascade 보존 (BottomNav 안에 흡수만) |
| BottomNav UI 영향 (다른 sprint 이벤트 영향 가능) | Step-by-step 진행, 각 단계 사용자 검증 |

---

## 9. 변경 이력
- 2026-05-08: 초기 작성 (Phase 1 Generate)
