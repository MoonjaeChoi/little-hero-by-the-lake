# 5040 main_menu_revamp Decisions

> **Phase 2.3 Mediator 산출** — 미결 3건 결정
> 모드: ai-assisted (사용자 "권장 그대로") · 일시: 2026-05-08

## 1. 요약

| # | Topic | Decision |
|---|---|---|
| D1 | 11 panel 흡수 | **A — prefab reuse + Tab instantiate** |
| D2 | TabContent 동적 변경 | **A — 모두 SetActive** |
| D3 | S6 분해 | **B — S6a + S6b 분리** |

## 2. PRD v2 변경

- §1 Sprint 표: S6 → S6a (MainMenuController, ~50L) + S6b (11 panel TabContent wiring, ~30L). 총 9 → 10 commit.
- §2 신규 클래스: TabContent SetActive 패턴 명시 (Instantiate + Destroy X)
- §3 의존성: S6a → S6b 직렬

## 3. 통계
- Total: 3 / Accepted: 3/3 (100%)
- 모드: ai-assisted "권장 그대로"

## 4. 변경 이력
- 2026-05-08: 초기 작성 (Phase 2.3 Mediator)
