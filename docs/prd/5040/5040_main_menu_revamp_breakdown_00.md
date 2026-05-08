# 5040 main_menu_revamp Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 10 태스크 / 10 commit / 약 4h
> 일시: 2026-05-08

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존 | 시간 |
|---|---|---|---|---|
| T01 | TabBase abstract + BottomNavBar | UI | - | 30m |
| T02 | MainMenuTopBar + CurrencyStripView | UI | - | 30m |
| T03 | CharacterStageView + MainCTAButton | UI | - | 40m |
| T04 | ChapterCardView + ChapterBookCarousel | UI | - | 50m |
| T05 | SideQuickBar | UI | - | 20m |
| T06a | MainMenuController + tab 흡수 | Logic | T01~T05 | 30m |
| T06b | 11 Panel TabContent wiring (SetActive 패턴) | UI | T06a | 30m |
| T07 | Attic.unity scene 갱신 + Canvas 신규 hierarchy | Scene | T06b | 30m |
| T08 | NUnit 6 (BottomNav / Carousel / TopBar) | Test | T07 | 30m |
| T09 | changelog + CLAUDE.md §11 §14 | Docs | T08 | 15m |

**합계**: 10 commit / ~4h05m

## 2. 의존성 그래프

```
T01 (BottomNav) ─┐
T02 (TopBar) ────┤
T03 (Stage+CTA) ─┼─→ T06a (Controller) ─→ T06b (11 panel) ─→ T07 (Scene) ─→ T08 (Test) ─→ T09 (Docs)
T04 (Carousel) ──┤
T05 (SideQuick) ─┘
```

**병렬 가능**: T01~T05 (5 컴포넌트 독립)
**직렬**: T06a → T06b → T07 → T08 → T09

## 3. 사용자 검증 단계 (Phase 5 단계별)

각 태스크 commit 후 사용자 검증:
- T01: BottomNav 5 button 표시 + tab event
- T02: TopBar 화폐 4종 표시
- T03: 캐릭터 portrait + CTA button 시각
- T04: 동화책 carousel + chapter card
- T05: SideQuickBar dot
- T06a+T06b: 통합 화면 — 다른 panel 진입 정상
- T07: scene 변경 후 다락방 진입 정상
- T08: NUnit 6/6 PASS
- T09: docs

## 4. 위험 + 완화

| 단계 | 위험 | 완화 |
|---|---|---|
| T07 | Attic.unity 큰 변경 | 5037.5/5038.7 답습 — scene 변경 후 즉시 검증 |
| T06b | 11 panel 통합 — wiring 실패 가능 | SetActive 패턴 (보존) |
| 전체 | UI 변경 누적 영향 | 각 태스크 git commit + 사용자 검증 + 즉시 revert 옵션 |

## 5. 변경 이력
- 2026-05-08: 초기 작성 (Phase 3 Breakdown)
