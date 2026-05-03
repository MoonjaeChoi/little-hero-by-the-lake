# 5005 Attic Decomposition Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 8 태스크 분해
> 일시: 2026-05-04 · 8 commit / 3시간 25분

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 시간 | Sprint |
|---|---|---|---|---|---|
| T01 | AtticCurrencyView (4 화폐 표시) | UI (Hub) | - | 25분 | 1 |
| T02 | AtticCharacterSelector (캐릭터 버튼) | UI (Hub) | T01 | 25분 | 2 |
| T03 | AtticChapterEntry (챕터 + 5002 D2 toast) | UI (Hub) | T02 | 30분 | 3 |
| T04 | AtticEndgameMenuView (Endgame + Cooldown) | UI (Hub) | T03 | 25분 | 4 |
| T05 | AtticPanelToggleManager (Update 분리) | UI (Hub) | T04 | 25분 | 5 |
| T06 | AtticReturnHandler (5002 신화 발현) | UI (Hub) | T05 | 20분 | 6 |
| T07 | AtticController cleanup + AtticDecompositionTests 5+1 + Validator §15.6 강화 | Logic+Test | T06 | 40분 | 7 |
| T08 | docs + changelog + CLAUDE.md §11/§12/§13/§14 | Docs | T07 | 15분 | 8 |

## 2. 의존성 그래프

```
T01 → T02 → T03 → T04 → T05 → T06 → T07 → T08
(각 Sprint 단위로 컴포넌트 1개씩 추출 후 commit + 씬 wiring 검증)
```

순차 진행 권장 (씬 wiring 충돌 회피).

## 3. CLAUDE.md §15 정합

| 태스크 | §15 정합 |
|---|---|
| T01~T06 | §15.1 SRP / §15.6 50~70L 이내 ✅ |
| T07 | AtticController 80L (200L 이내 ✅) / Reflection NUnit 5 / Validator §15.6 강화 |
| T08 | docs |

## 4. Unity MCP 안전 (D3=A)

각 Sprint 1~6 사이클:
1. 컴포넌트 .cs 파일 생성 → refresh + 5초
2. AtticController.cs 변경 (필드/메서드 제거) → refresh + 5초
3. Attic.unity 씬 wiring (manage_components add + Inspector 마이그레이션) → 씬 저장 + **10초+ 대기**
4. commit
5. 다음 Sprint 진입

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3).
