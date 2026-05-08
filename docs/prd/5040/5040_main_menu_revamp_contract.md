# Sprint Contract: 5040 main_menu_revamp

> **Phase 4 Contract** — Phase 6 Evaluator 단일 진실원
> 생성: 2026-05-08

## C1 컴파일
- 모든 신규 .cs (~9 파일) errors=0
- isCompiling=false (5초 내)

## C2 Editor Test (NUnit 6)
- `BottomNavBarTests` — Tab 전환 / default Adventure / OnTabSelected event
- `ChapterBookCarouselTests` — 7+3 페이지 / 챕터 카드 chapter map / 잠금 상태
- `MainMenuTopBarTests` — 4 화폐 칩 / Update on currency change

## C3 ProjectValidator
- errors=0
- warnings ≤ 5037.5 baseline (변동 ±2 허용)
- 신규 OOPPolicyRule 위반 0 (모두 100L 이내)

## C4 Reflection
| Type | 필수 |
|---|---|
| `BottomNavBar` (MonoBehaviour) | `OnTabSelected` event, `SelectedTab` Property |
| `MainMenuTopBar` | `Initialize` |
| `CharacterStageView` | `Initialize` |
| `MainCTAButton` | `OnClick` |
| `ChapterBookCarousel` | `Initialize`, `Refresh`, `OnPageChanged` |
| `ChapterCardView` | `Bind` (chapter, state) |
| `SideQuickBar` | `Initialize` |
| `MainMenuController` | `SwitchTab`, `Initialize` |
| `TabBase` (abstract) | `OnEnter`, `OnExit` |

## C5 SO 검증
- 신규 SO X (UI logic only)
- ChapterData (50 챕터) 영향 0 — 기존 보존

## C6 Play 시나리오

### 시나리오 1: 다락방 진입 + BottomNav default Adventure
- Play 시작 → 다락방 진입
- BottomNavBar 5 button 표시 (Hero/Equip/**Adv ★**/Shop/Record)
- Default = Adventure tab (highlight)
- ChapterBookCarousel + MainCTAButton 표시

### 시나리오 2: ChapterBookCarousel 좌우 swipe + chapter card 선택
- 동화책 펼친 시각 (다락방 책상 위)
- 좌/우 화살표 → 페이지 전환 (1~10)
- chapter card tap → ChapterEntryConfirmModal

### 시나리오 3: BottomNav 다른 tab 전환 + 11 panel
- Hero tab tap → CharacterInfoPanel 표시
- Shop tab tap → Gacha + Starlight + Limited 표시
- 다른 tab 전환 정상

## 합격 기준
- 자동 ✅ + 시나리오 1+2 → **최소 합격**
- 자동 ✅ + 시나리오 1+2+3 → **완전 합격**

## 변경 이력
- 2026-05-08: 초기 작성
