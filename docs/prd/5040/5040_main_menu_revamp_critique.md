# 5040 main_menu_revamp PRD 리뷰

## 0. 결정 이력
- 2026-05-08: Claude Code 자동 리뷰 (Phase 2.1 Critique)

## 1. 종합 점수: 8.4/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | 5 분리 컴포넌트 ✅ / TabBase abstract ✅ |
| PG-12 톤 | 10/10 | 동화책 메타포 + 지브리 ✅ |
| 성능 | 9/10 | TabContent 동적 instantiate — Pool 권장 (D2) |
| 시스템 통합 | 8/10 | 11 panel 흡수 복잡 (D1) |
| Sprint 분해 | 7/10 | S6 (MainMenuController + 11 panel wiring) 큰 작업 (D3) |

## 2. Accept 항목 (5)

- ✅ **5-tab BottomNav** 패턴 (탕탕특공대 답습)
- ✅ **동화책 메타포** (지브리 정서 + V5 sticker)
- ✅ **컴포넌트 분리** (TopBar/Stage/CTA/Carousel/SideQuick — §15.1 SRP)
- ✅ **default Adventure tab + MainCTAButton** (User Onboarding)
- ✅ **35 챕터 = 7×5 + Hardcore 3 페이지** (V2 + Hardcore 정합)

## 3. Reject 항목 (0)

(없음 — 모든 디자인 §15 정합)

## 4. Discuss 항목 (3)

### 🟡 D1. 11 Panel 흡수 — 통합 vs 보존

**Original**: 11 panel (Inventory/Gacha/Diary 등) 을 5-tab content 로 흡수.
- **옵션 A**: 11 panel prefab reuse (현재 PRD) — 기존 logic 보존, Tab 안에서 instantiate
- **옵션 B**: 11 panel 대체 (Tab 별 신규 view 작성) — 작업 ↑
- **옵션 C**: hybrid — main view (예 InventoryPanel) 보존, modal (EnhanceModal) 폐기 + tab 통합
- **권장**: 옵션 A — 기존 logic 보존, 위험 ↓

### 🟡 D2. TabContent 동적 instantiate — Pool vs SetActive

**Original**: 5 Tab content 동적 변경.
- **옵션 A**: TabContent SetActive (모든 prefab pre-instantiate) — 메모리 ↑, 빠름
- **옵션 B**: 동적 Instantiate + Destroy — 메모리 ↓, GC 영향
- **옵션 C**: ObjectPool 답습 (5037.5 답습) — 균형
- **권장**: 옵션 A — 11 panel 모두 인 메모리, SetActive 만 (CharacterPanel 답습)

### 🟡 D3. S6 (MainMenuController + 11 panel wiring) 분해

**Original**: 1 commit 에 MainMenuController + AtticController 변경 + 11 panel TabContent 매칭.
- **옵션 A**: 1 commit 통합 (현재 PRD) — 큰 commit
- **옵션 B**: S6a (MainMenuController) + S6b (11 panel wiring) 분리 — 안전성 ↑, commit ↑
- **권장**: 옵션 B — 분해 + 단계 검증

## 5. CLAUDE.md §15 정책 위반 점검

- §15.1 SRP: 5+ 컴포넌트 분리 ✅
- §15.4 SO 우선: ChapterData SO 활용 ✅
- §15.6 100L: 모든 신규 클래스 100L 이내 ✅
- §15.7 DI: Initialize 명시 ✅
- §15.9 Test: NUnit 6 ✅
- §15.11 안티: 회피 ✅
- §15.12 Validator: 신규 룰 불필요 (기존 OOPPolicyRule 답습)

## 6. 권장 변경

- **즉시 변경** (Accept):
  - §1 Sprint 표 Sprint 명세 명확
- **사용자 결정 후 변경** (Discuss):
  - D1: 11 panel 흡수 방식
  - D2: TabContent 동적 변경 방식
  - D3: S6 분해 여부

## 7. 변경 이력
- 2026-05-08: 초기 작성 (Phase 2.1 Critique)
