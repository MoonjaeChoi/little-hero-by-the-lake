# Sprint Contract: 5005 Attic Decomposition

> **PGE Planner — Phase 4 Contract** / 생성일: 2026-05-04

## 1. C1 컴파일

- 8 commit 모두 errors=0 / refresh + 5~10초

## 2. C2 Editor Test (NUnit)

| Test 클래스 | 신규 메서드 | 위치 |
|---|---|---|
| `AtticDecompositionTests` | 5+1 | `Tests/Editor/AtticDecompositionTests.cs` |

**신규 NUnit (6)**:
- `AtticCurrencyView_Type_Exists`
- `AtticCharacterSelector_Type_Exists`
- `AtticChapterEntry_Type_Exists`
- `AtticEndgameMenuView_Type_Exists`
- `AtticPanelToggleManager_Type_Exists`
- `AtticReturnHandler_Type_Exists`

추가 (선택): AtticController 라인 수 정적 검증 (200L 이내).

**검증**: 신규 6 PASS + 기존 61 회귀 X (총 67).

## 3. C3 ProjectValidator (D2=C 강화)

- errors=0 / warnings 변동 — AtticController 200L+ 위반 1건 감소 예상
- 신규 룰 (선택): AtticController.cs 200L 이내 검증 (D2=C 채택)
- 기존 §15.6 200L+ 자동 룰로도 충분

## 4. C4 Reflection 검증

| Type | 검증 |
|---|---|
| `LittleHero.AtticCurrencyView` | UI/Hub 컴포넌트 존재 |
| `LittleHero.AtticCharacterSelector` | 동 |
| `LittleHero.AtticChapterEntry` | 동 |
| `LittleHero.AtticEndgameMenuView` | 동 |
| `LittleHero.AtticPanelToggleManager` | 동 |
| `LittleHero.AtticReturnHandler` | 동 |
| `LittleHero.AtticController` | 잔여 (가챠/한정상점/BGM 책임) |

## 5. C5 SO + Scene 검증

- Attic.unity 의 AtticController GameObject 에 7 컴포넌트 부착 (AddComponent)
- 각 컴포넌트 SerializeField wiring (Inspector 드래그)
- 5001~5004 SO 회귀 X

## 6. C6 Play 시나리오

### S1 — Attic 진입 정상
1. 새 SaveData → Attic
2. **검증**: 4 화폐 / 캐릭터 강제 선택 / 챕터 버튼 활성

### S2 — 5002 D2 toast 회귀
1. 챕터 보스 처치 → Attic 복귀
2. **검증**: ChapterClearedPanel toast 1회

### S3 — 5002 신화 발현 회귀
1. 주간 보스 처치 → Attic
2. **검증**: Mythic Overlay 1회

### S4 — 패널 토글 (fix #1 회귀)
1. GachaPanel 활성 후 LimitedShopPanel
2. **검증**: GachaPanel 자동 비활성화

### S5 — Endgame 메뉴 활성
1. Editor 메뉴 `Set Endgame Menu Active` 실행 → Attic 진입
2. **검증**: Endgame 메뉴 활성 + cooldown 표시

## 7. 비기능

- 컴파일 errors=0 (8 commit)
- NUnit 61 → 67
- 5001~5004 회귀 X (Validator + NUnit)
- 60fps 유지 (변경 X)

## 8. 합격 기준

- 최소: 자동 (C1~C5) + S1 PASS
- 완전: 자동 (C1~C5) + S1~S5 모두 PASS

## 9. Trade-off

- **D1 = A**: 7 컴포넌트 — 라인 분산 X, SRP 명확
- **D2 = C**: Reflection 5 + Validator 강화 — Play Mode Test 시간 절약
- **D3 = A**: Sprint별 wiring — 안전 우선

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4).
