# 5005 — Attic Controller Decomposition v2 PRD (Mediator 확정)

> **PRD v2 (Mediator 확정)** / Decisions: `5005_*_decisions.md` (D1=A, D2=C, D3=A 권장 채택)
>
> **번호**: 5005 / **출처**: `docs/prd/5005_attic_decomposition_v1.md` (옵션 B Component 추출)
> **목표**: 8 Sprint, 8 commit, 3시간 25분 (v1 그대로 — 자동 수용 0건)
> **선행**: 5002 D2 (AtticController 415L 추가) / CLAUDE.md §13 #10
>
> ### Mediator 결정 (3건)
> - **D1 = A**: 7 컴포넌트 그대로 (SRP 명확)
> - **D2 = C**: Reflection NUnit 5 + ProjectValidator §15.6 룰 강화 (Play Mode Test 회피)
> - **D3 = A**: Sprint별 wiring + 10초+ 대기 (게임 진입 허브 안전 우선)

## 0. 전체 개요

| Sprint | 작업 | commit | 예상 시간 |
|---|---|---|---|
| 1 | AtticCurrencyView 추출 (4 화폐 표시) + 씬 wiring | 1 | 25분 |
| 2 | AtticCharacterSelector 추출 (캐릭터 버튼 + 선택) + 씬 wiring | 1 | 25분 |
| 3 | AtticChapterEntry 추출 (챕터 버튼 + HandleStart + 5002 D2 toast) | 1 | 30분 |
| 4 | AtticEndgameMenuView 추출 (Endgame 가용성 + Cooldown) | 1 | 25분 |
| 5 | AtticPanelToggleManager 추출 (Update — 한 번에 하나만 활성) | 1 | 25분 |
| 6 | AtticReturnHandler 추출 (5002 신화 발현 1회 toast) | 1 | 20분 |
| 7 | AtticController 잔여 cleanup (80L 가챠/한정상점만) + Reflection NUnit 5 + Validator 룰 | 1 | 40분 |
| 8 | docs + changelog + CLAUDE.md §11/§12/§13/§14 | 1 | 15분 |
| **총** | | **8** | **3시간 25분** |

## 의존성 그래프

```
Sprint 1~6 (병렬 가능 — 서로 독립 컴포넌트 추출) ──┐
                                                  ├─→ Sprint 7 (cleanup + Test) → Sprint 8 (docs)
```

각 Sprint 1~6 = 1 commit 단위. 단일 작업자는 순차 진행 권장 (씬 wiring 충돌 회피).

---

## Sprint 1 — AtticCurrencyView 추출 (1 commit, 25분)

### 1.1 신규 파일

**경로**: `Scripts/Hub/AtticCurrencyView.cs` (~50L)

**산출물**:
```csharp
using UnityEngine;
using UnityEngine.UI;

namespace LittleHero
{
    /// <summary>
    /// 5005 Sprint 1 — 다락방 4 화폐 표시 (StarFragments / MagicCrystal / Starlight / StarforceShards).
    /// AtticController 에서 추출 (§15.1 SRP). MetaProgression / GameEvents 정적 이벤트 구독.
    /// </summary>
    public class AtticCurrencyView : MonoBehaviour
    {
        [SerializeField] private Text _starFragmentsText;
        [SerializeField] private Text _magicCrystalText;
        [SerializeField] private Text _starlightText;
        [SerializeField] private Text _starforceShardsHud;

        private void OnEnable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged += UpdateStarFragments;
            GameEvents.OnMagicCrystalChanged += UpdateMagicCrystals;
            GameEvents.OnStarlightChanged += UpdateStarlight;
            GameEvents.OnStarforceShardsChanged += UpdateStarforceShards;
        }

        private void OnDisable()
        {
            if (MetaProgression.Instance != null)
                MetaProgression.Instance.OnStarFragmentsChanged -= UpdateStarFragments;
            GameEvents.OnMagicCrystalChanged -= UpdateMagicCrystals;
            GameEvents.OnStarlightChanged -= UpdateStarlight;
            GameEvents.OnStarforceShardsChanged -= UpdateStarforceShards;
        }

        private void Start()
        {
            UpdateStarFragments(MetaProgression.Instance != null ? MetaProgression.Instance.StarFragments : 0);
            UpdateMagicCrystals(CurrencyService.MagicCrystals);
            UpdateStarlight(CurrencyService.Starlight);
            UpdateStarforceShards(StarforceService.Shards);
        }

        private void UpdateStarFragments(int n) { if (_starFragmentsText != null) _starFragmentsText.text = $"무지개 구슬 ✦ {n}"; }
        private void UpdateMagicCrystals(int n) { if (_magicCrystalText != null) _magicCrystalText.text = $"마법 결정 ◇ {n}"; }
        private void UpdateStarlight(int n) { if (_starlightText != null) _starlightText.text = $"별빛 ⋆ {n}"; }
        private void UpdateStarforceShards(int n) { if (_starforceShardsHud != null) _starforceShardsHud.text = $"마법의돌 조각 ◈ {n}"; }
    }
}
```

### 1.2 AtticController 변경
- 4 화폐 SerializeField 제거 (`_starFragmentsText`, `_magicCrystalText`, `_starlightText`, `_starforceShardsHud`)
- 4 Update 메서드 제거
- OnEnable/OnDisable 의 4 이벤트 구독 라인 제거
- Start 의 4 초기 표시 호출 제거

### 1.3 Attic.unity 씬 wiring
- AtticController GameObject 에 `AtticCurrencyView` 컴포넌트 추가 (`AddComponent`)
- 4 SerializeField 를 `AtticCurrencyView` 의 동일 필드로 Inspector 드래그 마이그레이션

### 1.4 검증
- 컴파일 errors=0
- AtticController 420 → ~395L
- Play Mode — Attic 진입 시 4 화폐 정상 표시
- MetaProgression 이벤트 정상 발생 (별가루 획득 시 화폐 갱신)

#### Sprint 1 commit

```
5005 Sprint 1 — AtticCurrencyView 추출 (1/8)

[변경]
- Scripts/Hub/AtticCurrencyView.cs 신규 (~50L)
- Scripts/Hub/AtticController.cs — 4 화폐 SerializeField + 4 Update 메서드 + 이벤트 구독 제거 (-30L)
- Scenes/Attic.unity — AtticCurrencyView 컴포넌트 추가 + 4 Inspector wiring 마이그레이션

[검증]
- 컴파일 errors=0
- AtticController 420 → ~395L
- Play Mode — 4 화폐 표시 회귀 X

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 2 — AtticCharacterSelector 추출 (1 commit, 25분)

### 2.1 신규 파일

**경로**: `Scripts/Hub/AtticCharacterSelector.cs` (~70L)

**산출물**:
```csharp
public class AtticCharacterSelector : MonoBehaviour
{
    [SerializeField] private CharacterButton[] _buttons;  // AtticController.CharacterButton 재사용
    [SerializeField] private Text _selectedCharacterText;
    [SerializeField] private CharacterData _defaultCharacter;

    private void OnEnable() { /* 버튼 wiring + MetaProgression.OnCharacterChanged 구독 */ }
    private void OnDisable() { /* 미러 */ }
    private void Start() { /* 저장 캐릭터 복원 + 첫 시작 강제 선택 */ }
    private void HandleSelectCharacter(CharacterData ch) { ... }
    private void UpdateSelectedCharacter(CharacterData ch) { ... }
    private CharacterData ResolveCharacterById(string id) { ... }
    private void ShowFirstStartCharacterPrompt() { ... }
}
```

### 2.2 AtticController 변경
- `_selectedCharacterText` / `_characterButtons` / `_defaultCharacter` 제거
- `CharacterButton` 클래스 → 별 파일 또는 `AtticCharacterSelector` 내부로 이동
- 5 메서드 제거

### 2.3 씬 wiring + 검증
- AtticCharacterSelector 컴포넌트 추가 + Inspector 마이그레이션
- Play — 첫 시작 캐릭터 선택 / 저장 캐릭터 복원 / 화면 갱신

#### Sprint 2 commit
```
5005 Sprint 2 — AtticCharacterSelector 추출 (2/8)
[변경] Scripts/Hub/AtticCharacterSelector.cs 신규 (~70L) / AtticController.cs -50L / Attic.unity wiring
[검증] 컴파일 OK / 첫 시작 캐릭터 강제 선택 회귀 X / 저장 복원 회귀 X
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 3 — AtticChapterEntry 추출 (1 commit, 30분)

### 3.1 신규 파일

**경로**: `Scripts/Hub/AtticChapterEntry.cs` (~50L)

**산출물**:
```csharp
public class AtticChapterEntry : MonoBehaviour
{
    [SerializeField] private ChapterButton[] _buttons;
    [SerializeField] private string _chapterSceneName = "Chapter";

    private void OnEnable() { /* 버튼 wiring */ }
    private void OnDisable() { /* 미러 */ }
    private void Start() { HandlePendingChapterCleared(); }   // 5002 D2 toast
    private void HandleStart(ChapterData ch) { /* SelectedChapter set + SceneTransitionService.Transition */ }
    private void HandlePendingChapterCleared() { /* 5002 D2 1회 toast + Save */ }
    private ChapterData ResolveChapterById(string id) { ... }
}
```

### 3.2 AtticController 변경
- `_chapterButtons` / `_chapterSceneName` 제거
- `ChapterButton` 클래스 → AtticChapterEntry 내부 또는 별 파일
- HandleStart / HandlePendingChapterCleared / ResolveChapterById 제거

### 3.3 씬 wiring + 검증
- AtticChapterEntry 컴포넌트 추가
- Play S1 — Attic → 챕터 1 클릭 → fade transition + Chapter 씬
- Play S2 — 챕터 1 보스 처치 후 Attic 복귀 → ChapterClearedPanel toast (5002 D2 회귀 X)

#### Sprint 3 commit
```
5005 Sprint 3 — AtticChapterEntry 추출 + 5002 D2 toast 보존 (3/8)
[변경] AtticChapterEntry.cs 신규 (~50L) / AtticController -50L / Attic.unity wiring
[검증] 챕터 진입 fade / 5002 D2 pendingChapterClearedId toast 회귀 X
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 4 — AtticEndgameMenuView 추출 (1 commit, 25분)

### 4.1 신규 파일

**경로**: `Scripts/Hub/AtticEndgameMenuView.cs` (~70L)

**산출물**:
```csharp
public class AtticEndgameMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Text _dailyCooldown / _weeklyCooldown / _infiniteDepth;

    private void Start() { Refresh(); }
    private void Refresh() { /* AllChaptersFirstCleared + cooldown */ }
    private static bool AllChaptersFirstCleared() { ... }
    private static string FormatHm(long s) / FormatDh(long s) { ... }
}
```

또는 별 헬퍼 클래스: `Scripts/Hub/EndgameTimeFormat.cs` (정적, FormatHm/FormatDh)

### 4.2 검증
- Play — CH 35 모두 클리어 시 Endgame 메뉴 활성. cooldown 정상 표시.

#### Sprint 4 commit
```
5005 Sprint 4 — AtticEndgameMenuView 추출 (4/8)
[변경] AtticEndgameMenuView.cs ~70L / AtticController -35L
[검증] Endgame 메뉴 활성 조건 (firstClearedChapters 35비트) 회귀 X
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 5 — AtticPanelToggleManager 추출 (1 commit, 25분)

### 5.1 신규 파일

**경로**: `Scripts/Hub/AtticPanelToggleManager.cs` (~50L)

**산출물**:
```csharp
public class AtticPanelToggleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _managedPanels;
    [SerializeField] private string[] _excludePanelNames = { "CharacterPanel" };
    private GameObject _lastActivePanel;

    private void Awake() { if (_managedPanels.Length == 0) AutoDiscoverPanels(); }
    private void Update() { /* 한 번에 하나만 활성 */ }
    private void AutoDiscoverPanels() { ... }
}
```

### 5.2 검증
- Play — 패널 2 동시 활성 시도 시 직전 패널 자동 비활성화 (출시 전 fix #1 회귀 X)

#### Sprint 5 commit
```
5005 Sprint 5 — AtticPanelToggleManager 추출 (Update 분리) (5/8)
[변경] AtticPanelToggleManager.cs ~50L / AtticController.Update -33L
[검증] 패널 토글 알고리즘 회귀 X
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 6 — AtticReturnHandler 추출 (1 commit, 20분)

### 6.1 신규 파일

**경로**: `Scripts/Hub/AtticReturnHandler.cs` (~40L)

**산출물**:
```csharp
public class AtticReturnHandler : MonoBehaviour
{
    private void Start() { HandleReturnFromBossEncounter(); }
    private void HandleReturnFromBossEncounter()
    {
        var def = BossEncounterContext.LastMythicDefinition;
        if (def == null) return;
        var overlay = FindFirstObjectByType<MythicEffectOverlay>(FindObjectsInactive.Include);
        if (overlay != null) overlay.Play(def);
        BossEncounterContext.LastMythicDefinition = null;
        BossEncounterContext.LastMythicGranted = null;
    }
}
```

### 6.2 검증
- Play — 주간 보스 처치 후 신화 발현 시 Attic 복귀 1회 Mythic Overlay (5002 회귀 X)

#### Sprint 6 commit
```
5005 Sprint 6 — AtticReturnHandler 추출 (5002 신화 발현 toast) (6/8)
[변경] AtticReturnHandler.cs ~40L / AtticController -25L
[검증] 5002 weekly mythic overlay 회귀 X
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 7 — AtticController cleanup + NUnit 5 + Validator (1 commit, 40분)

### 7.1 AtticController 잔여 cleanup

최종 AtticController (~80L):
```csharp
public class AtticController : MonoBehaviour
{
    [SerializeField] private Button _gachaButton / _starforceButton / _limitedShopButton;
    [SerializeField] private GachaPanel _gachaPanel;
    [SerializeField] private SlotStarforcePanel _starforcePanel;
    [SerializeField] private LimitedShopPanel _limitedShopPanel;
    [SerializeField] private Text _limitedShopBadgeText;

    private void OnEnable() { /* 3 버튼 wiring + LimitedOffer 이벤트 */ }
    private void OnDisable() { /* 미러 */ }
    private void Start() { Time.timeScale = 1f; AudioManager.Instance?.PlayBgm("attic_nostalgic"); RefreshLimitedShopBadge(); }
    private void OpenGachaPanel() / OpenStarforcePanel() / OpenLimitedShopPanel() { /* SetActive(true) */ }
    private void HandleLimitedOfferChanged(LimitedShopOffer _) { RefreshLimitedShopBadge(); }
    private void RefreshLimitedShopBadge() { /* 활성 핫딜 N개 뱃지 */ }
}
```

### 7.2 AtticDecompositionTests.cs (~80L)

**파일**: `Tests/Editor/AtticDecompositionTests.cs`

**NUnit 5+** (Reflection 기반 — UI MonoBehaviour Test 어려움):
```csharp
public class AtticDecompositionTests
{
    [Test] public void AtticCurrencyView_Type_Exists() {
        Assert.NotNull(System.Type.GetType("LittleHero.AtticCurrencyView, Assembly-CSharp"));
    }
    [Test] public void AtticCharacterSelector_Type_Exists() { /* 동일 */ }
    [Test] public void AtticChapterEntry_Type_Exists() { /* 동일 */ }
    [Test] public void AtticEndgameMenuView_Type_Exists() { /* 동일 */ }
    [Test] public void AtticPanelToggleManager_Type_Exists() { /* 동일 */ }
    [Test] public void AtticReturnHandler_Type_Exists() { /* 동일 */ }
    [Test] public void AtticController_Reduced_LineCount() {
        // 정적 검증 — AtticController.cs 라인 수 측정 (200L 이내)
    }
}
```

### 7.3 ProjectValidator 룰 (선택)

`Editor/ProjectValidator.cs` — 5005 §15.6 룰: `Hub/AtticController.cs` 라인 수 검사 (200L 이내). 또는 기존 200L+ 자동 룰로 충분.

### 7.4 검증
- AtticController 80L (200L 이내 ✅)
- NUnit 6+ PASS / 누적 67+
- Validator AtticController 200L+ 위반 해소

#### Sprint 7 commit
```
5005 Sprint 7 — AtticController cleanup + AtticDecompositionTests 6+ (7/8)
[변경] AtticController.cs 80L (잔여 가챠/한정상점/BGM) / Tests/Editor/AtticDecompositionTests.cs 신규 6 NUnit
[검증] AtticController 200L 이내 ✅ / NUnit 61→67 / Validator 200L+ 경고 1건 감소
Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## Sprint 8 — docs (1 commit, 15분)

### 8.1 changelog_v1.md
```
- 2026-XX-XX: **5005 attic_decomposition_v1** — AtticController 420L → 80L (340L 감소). 6 컴포넌트 추출 (CurrencyView/CharacterSelector/ChapterEntry/EndgameMenuView/PanelToggleManager/ReturnHandler). 8 Sprint 8 commit. NUnit 61→67. §15.6 200L+ 위반 해소.
```

### 8.2 CLAUDE.md
- §11 마일스톤: `attic_decomposition_v1` 행
- §12 활성 시스템: 6 신규 컴포넌트 추가
- §12 Tests: 61 → 67
- §13: ✅ 표시
- §14: 5건 회전

---

## 검증 시나리오 (Play 검증)

### S1 — Attic 진입 정상
1. 새 SaveData → Attic 진입
2. **검증**: 4 화폐 표시 / 캐릭터 선택 강제 / 챕터 버튼 활성

### S2 — 챕터 보스 처치 후 toast (5002 D2 회귀)
1. 챕터 1 wave 20 보스 처치 → Attic 복귀
2. **검증**: ChapterClearedPanel toast 1회 / pendingChapterClearedId clear

### S3 — 주간 보스 신화 발현 (5002 회귀)
1. Endgame 주간 보스 처치 → Attic 복귀
2. **검증**: Mythic Overlay 1회 / LastMythicDefinition clear

### S4 — 패널 토글 (fix #1 회귀)
1. GachaPanel 활성 후 LimitedShopPanel 활성
2. **검증**: GachaPanel 자동 비활성화 (한 번에 하나만)

### S5 — Endgame 메뉴 활성 (CH 35 클리어 후)
1. SaveData firstClearedChapters = 0x7FFFFFFFF (Editor 메뉴 set)
2. Attic 진입 → Endgame 메뉴 활성 + cooldown 표시

---

## 완료 정의 (Definition of Done)

- [ ] 8 commit 모두 push
- [ ] 컴파일 errors=0
- [ ] AtticController 200L 이내 (현 420L → 80L)
- [ ] NUnit 67+ PASS (5 신규 + 61 기존, 회귀 X)
- [ ] Validator 200L+ 경고 1건 감소
- [ ] Play S1+S2+S4 PASS

---

## 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Inspector wiring 누락 (게임 진입 허브) | Sprint 단위 commit + 즉시 Play 검증 |
| 5002 D2 toast 회귀 | Sprint 3 AtticChapterEntry 에 그대로 보존 |
| 5002 신화 발현 회귀 | Sprint 6 AtticReturnHandler 에 그대로 보존 |
| 5004 fade 회귀 | AtticChapterEntry.HandleStart 의 SceneTransitionService.Transition 보존 |
| MCP SIGSEGV (씬 wiring 6번) | 각 Sprint 후 씬 저장 + 10초+ 대기 |
| 5001~5004 NUnit 회귀 | Sprint 7 누적 검증 (67/67) |

---

## Source

- `docs/prd/5005_attic_decomposition_v1.md` (디자인)
- 5002 BackgroundApplier / 5004 FadeOverlay 단일 책임 컴포넌트 패턴
- AtticController 420L 코드 분석 (8 도메인 책임)
