# 5005 attic_decomposition_v1 Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: `5005_attic_decomposition_v1_contract.md` · 검증일: 2026-05-04 · 반복: 1/2

## 1. 종합: PASS (자동 모두 ✅, C6 사용자 보고 대기)

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 |
|---|---|---|---|---|
| C1 컴파일 | 1 | 0 | 1 | ✅ errors=0 (Sprint 1~8 each refresh OK) |
| C2 Editor Test (AtticDecomposition) | 6 | 0 | 6 | ✅ Test Runner 0.97s |
| C3 ProjectValidator | - | - | warnings 변동 ±0 (기존 §15.6 200L+ 룰로 충분 — D2=C 채택) | ✅ |
| C4 Reflection | 6 | 0 | 6 | ✅ AtticCurrencyView/CharacterSelector/ChapterEntry/EndgameMenuView/PanelToggleManager/ReturnHandler |
| C5 SO 검증 | - | - | 5005 는 신규 SO 없음 (5001~5004 회귀 X) | ✅ |
| **자동 합계** | **13** | **0** | **13** | **✅** |

## 3. 수동 검증 (Play 시나리오)

| # | 시나리오 | 결과 |
|---|---|---|
| S1 | Attic 진입 정상 (4 화폐/캐릭터 강제 선택/챕터 버튼) | ⏸ 사용자 보고 대기 |
| S2 | 5002 D2 toast 회귀 (챕터 보스 처치 → ChapterClearedPanel 1회) | ⏸ 대기 |
| S3 | 5002 신화 발현 회귀 (주간 보스 처치 → Mythic Overlay 1회) | ⏸ 대기 |
| S4 | 패널 토글 (GachaPanel 활성 후 LimitedShopPanel → 자동 비활) | ⏸ 대기 |
| S5 | Endgame 메뉴 활성 (CH 35 firstClearedChapters → cooldown 표시) | ⏸ 대기 |

## 4. PASS 항목 요약

### C1 컴파일
- 8 Sprint commit 모두 refresh + 8s 대기 후 errors=0
- 마지막 사이클: AtticReturnHandler.cs 컴파일 성공

### C2 Editor Test (AtticDecomposition 6 NUnit)
- AtticChapterEntry_Type_Exists ✅ (54ms)
- AtticCharacterSelector_Type_Exists ✅ (3ms)
- AtticCurrencyView_Type_Exists ✅ (<1ms)
- AtticEndgameMenuView_Type_Exists ✅ (<1ms)
- AtticPanelToggleManager_Type_Exists ✅ (<1ms)
- AtticReturnHandler_Type_Exists ✅ (<1ms)
- **Total**: 6/6 PASS, 0.97s

### C3 ProjectValidator
- D2=C 결정대로 기존 §15.6 LINE_LIMIT=200 룰로 충분
- AtticController 420L → 111L (200L 안전, 위반 1건 감소)
- 신규 6 컴포넌트 모두 100L 이내 (Validator 무경고)

### C4 Reflection
- 6 신규 Type 모두 Assembly-CSharp 에 존재 (NUnit 으로 자동 검증)
- 기존 5001~5004 Type 회귀 X

### C5 SO 검증
- 5005 는 신규 SO 도입 X (책임 분리 작업)
- 5001 BossLootTable 4 / 5002 ChapterRunner / 5003 BalanceConfig.miniBossSeparation / 5004 FadeOverlay.prefab 회귀 X

## 5. 비기능 요구사항

- ✅ 컴파일 errors=0 (8 commit 누적)
- ✅ NUnit 61 → 67 (+6, 5001 BossSystem 10 / 5002 BossSceneSeparation 10 / 5004 SceneTransition 5 회귀 X)
- ✅ 5001~5004 회귀 X (Validator + NUnit 통과)
- ⏸ 60fps 유지 (Play 시 측정, Profiler 권장)

## 6. AtticController 라인 추이

| 시점 | 라인 | 설명 |
|---|---|---|
| 시작 (T00) | 420L | 모든 책임 통합 |
| T01 후 | 385L | 4 화폐 view 분리 |
| T02 후 | 303L | 캐릭터 selector 분리 |
| T03 후 | 249L | 챕터 entry + 5002 D2 toast 분리 |
| T04 후 | 196L | Endgame 메뉴 분리 |
| T05 후 | 130L | 패널 토글 분리 |
| T06 후 | 111L | 보스 씬 복귀 토스트 분리 |
| **누적** | **-309L** | **§15.6 LINE_LIMIT=200 안전, §15.1 SRP 충족** |

## 7. Trade-off 회고 (mediator 결정 기반)

- D1=A (7 컴포넌트): 실제 6 컴포넌트로 정착. 잔여 책임 (가챠/한정상점/BGM/RefreshLimitedShopBadge) 은 AtticController 본체 유지 — 후속 분리 검토 가능
- D2=C (Validator): 기존 §15.6 룰로 충분. 별도 신규 룰 미추가 (Contract 의 "선택" 옵션)
- D3=A (Sprint별 wiring): 각 commit 마다 manage_components.add → 사용자 Inspector wiring 잔존

## 8. 다음 단계

PASS 시 (사용자 Play 시나리오 보고 후):
- changelog 등재 ✅ (T08 에서 완료)
- CLAUDE.md §11 마일스톤 ✅ (T08 에서 완료)
- /prd-pipeline <next system>

사용자 후속 작업:
- Inspector wiring (Attic.unity GameObject "AtticController" 의 6 신규 컴포넌트 SerializeField 드래그)
- Play 시나리오 S1~S5 보고

## 9. 변경 이력

- 2026-05-04: 초기 작성 (Phase 6 자동 검증 완료)
