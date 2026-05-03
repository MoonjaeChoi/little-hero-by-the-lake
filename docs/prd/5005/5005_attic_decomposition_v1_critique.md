# 5005 Attic Controller Decomposition — PRD 리뷰

## 0. 결정 이력
- 2026-05-04: prd-critiquer 1차 리뷰 (Phase 2.1)
- 입력: 5005_v1.md (디자인) + 5005_v1_prd.md (옵션 B 채택, 8 Sprint × 8 commit)

## 1. 종합 점수: 8/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | 7 컴포넌트 모두 100L 이내, SRP 강화 |
| PG-12 톤 | 10/10 | UI 리팩토링 — 톤 영향 0 |
| 성능 | 10/10 | 변경 X (Update 1 → 1 분리) |
| 시스템 통합 | 7/10 | ⚠️ 게임 진입 허브 — wiring 누락 위험 높음 |
| Sprint 분해 | 8/10 | 8 Sprint × 8 commit 균형. ⚠️ 컴포넌트 일부 합치기 검토 |

## 2. Accept 항목 (8)

- ✅ **5002 BackgroundApplier / 5004 FadeOverlay 패턴 답습** — 단일 책임 컴포넌트 확립
- ✅ **AtticController 420L → 80L** (340L 감소) — §15.6 200L 룰 해소
- ✅ **AtticChapterEntry 가 5002 D2 toast + 5004 fade 그대로 보존** — 기존 흐름 회귀 X
- ✅ **AtticReturnHandler 가 5002 신화 발현 toast 그대로 보존**
- ✅ **AtticPanelToggleManager Update 분리** — 다른 컴포넌트가 매 프레임 영향 X
- ✅ **Sprint 단위 commit + Play 검증** — 회귀 격리
- ✅ **NUnit 5 Reflection 기반** — UI MonoBehaviour Test 한계 인정 + 정적 검증 가능
- ✅ **한 GameObject 에 모든 컴포넌트 부착** — 디자이너 친화 + Inspector wiring 통일

## 3. Reject 항목 (0)

(없음)

## 4. Discuss 항목 (3) 🟡

### D1 — 컴포넌트 분리 단위 (7 → 5~6 합치기 검토)

**관찰**: 7 컴포넌트는 적절한 분리이나, 일부 합치기 가능:
- AtticChapterEntry + AtticReturnHandler 합치기 가능 (둘 다 챕터 클리어 후 toast/overlay 처리 — 50+40=90L)
- AtticEndgameMenuView + AtticPanelToggleManager 는 독립 (그대로)

**옵션**:
- **A** ★권장: 7 컴포넌트 그대로 (PRD 그대로) — 단일 책임 명확
- **B**: 6 컴포넌트로 합치기 (AtticChapterFlow = ChapterEntry + ReturnHandler + 5002 D2 toast 모두 통합) — 라인 절약
- **C**: 5 컴포넌트로 더 합치기 (Currency + Character + Chapter 가 Attic 본체 책임이라 통합) — 분리 효과 약화

**권장 사유 (A)**: SRP 명확 + 5002/5004 단일 책임 패턴 답습 + 회귀 격리 (한 컴포넌트 변경이 다른 컴포넌트 영향 X).

### D2 — Sprint 7 의 NUnit Test 범위

**관찰**: PRD 의 NUnit 5 = Reflection 기반 Type 존재 검증만. UI 동작 검증 X.

**옵션**:
- **A**: PRD 그대로 (Reflection 5 + 라인 수 검증 1) — 정적 검증
- **B**: + Play Mode Test (각 컴포넌트 OnEnable/OnDisable 동작 검증) — 동적 검증
- **C** ★권장: PRD 그대로 + ProjectValidator §15.6 룰 (AtticController 200L 이내) 강화

**권장 사유 (C)**: Play Mode Test 는 시간 소요 ↑ (Test 자체로 30분+). Reflection 5 + Validator 룰로 충분.

### D3 — Attic.unity 씬 wiring 자동화 vs 수동

**관찰**: 6 컴포넌트 추가 + Inspector 마이그레이션 = 6번 씬 변경. MCP `manage_components` 자동화 가능?

**옵션**:
- **A** ★권장: 6 Sprint 각 commit 후 씬 wiring + 10초+ 대기 (5002/5003 패턴) — 안전
- **B**: Sprint 1~6 코드 변경 일괄 + Sprint 7 에 씬 wiring 일괄 — 빠르지만 회귀 위험 높음
- **C**: MCP 자동 wiring (manage_components.set_property) — 빠름, 검증 어려움

**권장 사유 (A)**: 게임 진입 허브 — 안전 우선. 5002/5003 패턴 답습.

## 5. CLAUDE.md §15 정책 점검

| §15.x | 항목 | 결과 |
|---|---|---|
| §15.1 SRP | 7 컴포넌트 단일 책임 | ✅ |
| §15.6 100/200줄 | AtticController 80L / 각 컴포넌트 50~70L | ✅ |
| §15.7 DI | SerializeField + 정적 service 직접 사용 | △ (Initialize 명시 주입 X — 기존 패턴 유지) |
| §15.9 Test | Reflection 5 (UI MonoBehaviour 한계) | △ |
| §15.11 안티 | Update in 1 (PanelToggleManager 만) | ✅ |
| §15.12 Validator | AtticController 200L+ 경고 1건 감소 | ✅ |

## 6. 권장 변경

### 즉시 변경 (rebuttal 자동 수용)

(없음 — Discuss 모두 사용자 결정)

### 사용자 결정 후 변경 (mediator)

- D1: 컴포넌트 분리 단위 (7 / 6 / 5)
- D2: NUnit Test 범위 (Reflection 만 / Play Mode 추가)
- D3: 씬 wiring 시점 (Sprint 별 / 일괄 / MCP 자동)

## 7. 변경 이력

- 2026-05-04: 초기 리뷰 (Phase 2.1) — 8/10 / Accept 8 / Reject 0 / Discuss 3
