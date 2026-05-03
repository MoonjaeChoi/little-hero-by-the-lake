# 5005 — Attic Controller Decomposition v1

> **목적**: AtticController 420L → 책임별 컴포넌트로 분해 (§15.6 200L 룰 + §15.1 SRP 준수). 다락방 = 게임 진입 허브 — 회귀 위험 높아 Component 추출 패턴 채택.
> **번호**: 5005 / **선행**: CLAUDE.md §13 #10 "AtticController 책임 분리" / 5002 D2 mediator 결정 (415L 추가, 후속 분리 권장)

## 0. 결정 이력

| 날짜 | 결정 | 출처 |
|---|---|---|
| 2026-05-04 | 5002 D2 채택 (pendingChapterClearedId toast) — AtticController 393L → 415L | 5002_decisions.md |
| 2026-05-04 | CLAUDE.md §13 #10 "AtticController 분리 권장" 등재 | CLAUDE.md §13 |
| 2026-05-04 | 본 5005 PRD 작성 — 옵션 B 권장 (Component 추출, 5 컴포넌트) | 본 문서 |

## 1. 현재 시스템 분석

### 1.1 AtticController 구조 (420L, 22 메서드, 5 [Header] 섹션)

| Header 섹션 | SerializeField | 메서드 그룹 | 라인 수 |
|---|---|---|---|
| 본체 (캐릭터+챕터+화폐) | `_starFragmentsText` / `_selectedCharacterText` / `_chapterSceneName` / `_chapterButtons` / `_characterButtons` / `_defaultCharacter` | UpdateStarFragments / HandleSelectCharacter / UpdateSelectedCharacter / ResolveCharacterById / ShowFirstStartCharacterPrompt / HandleStart | ~80L |
| **D2 — 추가 화폐** | `_magicCrystalText` / `_starlightText` | UpdateMagicCrystals / UpdateStarlight / UpdateStarforceShards | ~25L |
| **D2 — Endgame 메뉴** | `_endgameMenuRoot` / 3 cooldown text | RefreshEndgameMenu / AllChaptersFirstCleared / FormatHm / FormatDh | ~35L |
| **패널 토글 (fix #1)** | `_managedPanels` / `_excludePanelNames` | Awake / AutoDiscoverPanels / Update (33L 매 프레임) | ~50L |
| **PIVOT 가챠** | `_gachaButton` / `_gachaPanel` / `_starforceButton` / `_starforcePanel` / `_starforceShardsHud` | OpenGachaPanel / OpenStarforcePanel | ~15L |
| **PIVOT 한정 상점** | `_limitedShopButton` / `_limitedShopPanel` / `_limitedShopBadgeText` | OpenLimitedShopPanel / RefreshLimitedShopBadge / HandleLimitedOfferChanged | ~25L |
| 5002 D2 toast | (재사용) | HandlePendingChapterCleared / HandleReturnFromBossEncounter | ~30L |

### 1.2 라이프사이클 메서드 (대부분 라인)

- `OnEnable` (40L) — 6 이벤트 구독 + 4 버튼 wiring
- `OnDisable` (25L) — 미러
- `Start` (35L) — 7 init + 캐릭터 복원
- `Update` (33L) — 패널 토글 알고리즘 (매 프레임)

총 라이프사이클: **~133L** (전체 32%)

### 1.3 위반 사항

| §15.x | 위반 | 영향 |
|---|---|---|
| §15.6 100줄 / 200줄 | **420L** (200L 의 2.1×, ProjectValidator 경고) | 가독성 ↓, 테스트 어려움 |
| §15.1 SRP | 8 도메인 책임 (화폐/캐릭터/챕터/Endgame/패널 토글/가챠/한정상점/신화 toast) | 변경 시 다른 도메인 영향 |
| §15.11 Update | 패널 토글 알고리즘 매 프레임 (저비용이지만 다른 매니저로 분리 권장) | GC 영향 미미하나 책임 분리 측면 |
| §15.9 Test | NUnit 0 (UI MonoBehaviour) | Component 추출 시 정적 헬퍼 검증 가능 |

### 1.4 의존성

- MetaProgression (이벤트 + Data) — 캐릭터 / 화폐 / firstClearedChapters / pendingChapterClearedId
- GameEvents (정적 EventBus) — 5 이벤트 구독
- CurrencyService / StarforceService / LimitedShopService / EndgameResetService (정적)
- BossEncounterContext (5002 신화 발현)
- AudioManager.PlayBgm / SceneTransitionService.Transition (5004)
- 8 UI prefab 인스턴스 (GachaPanel / SlotStarforcePanel / LimitedShopPanel 등)

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | MVP 분리 (AtticModel + AtticView + AtticPresenter 3분리) | 정통 패턴, Test 용이 | Unity uGUI 와 정합 약함, wiring 복잡 |
| **B** ★권장 | **Component 추출 (5~6 컴포넌트)** — 도메인별 분리 + AtticController 는 컴포넌트 통합만 | 점진 분리 가능 + 회귀 격리 + Inspector 친화 | 컴포넌트 5~6 신규 (라인 분산) |
| C | Sub-controller 2~3 분리 (AtticEndgameController + AtticPanelController) | 가벼운 분리 | 부분만 해소 (여전히 250L+) |
| D | EventBus 강화 (각 도메인 컴포넌트가 GameEvents 구독, AtticController 가 이벤트 발행만) | 결합도 ↓ | EventBus 신규 이벤트 다수 — 복잡도 ↑ |
| E | 분리 X (현 상태 유지) | 변경 0 | §15.6 위반 영구 |

**★ 권장 = B (Component 추출)** —
- 5002 BackgroundApplier / 5004 FadeOverlay 패턴 답습 (작은 단일 책임 컴포넌트)
- Inspector 친화 (Unity 디자이너 워크플로 보존)
- 점진 마이그레이션 가능 (한 컴포넌트씩 추출, 회귀 격리)
- 각 컴포넌트 100~150L 이내 (§15.6 정합)

## 3. 권장 설계 (옵션 B 채택)

### 3.1 분리 후 구조

```
Hub/
├── AtticController.cs            (~80L — 라이프사이클 + 컴포넌트 wiring)
├── AtticCurrencyView.cs          (~50L — 4 화폐 표시)
├── AtticCharacterSelector.cs     (~70L — 캐릭터 버튼 + 선택)
├── AtticChapterEntry.cs          (~50L — 챕터 버튼 + HandleStart + pendingChapterClearedId toast)
├── AtticEndgameMenuView.cs       (~70L — Endgame 가용성 + Cooldown 표시)
├── AtticPanelToggleManager.cs    (~50L — 패널 한 번에 하나만 활성, fix #1)
└── AtticReturnHandler.cs         (~40L — HandleReturnFromBossEncounter 신화 발현)
```

총 7 파일 / ~410L (현 420L 와 유사하나 책임 분리). AtticController 는 **80L 이내** 로 축소.

### 3.2 신규 컴포넌트 명세

**AtticCurrencyView** (~50L):
```csharp
public class AtticCurrencyView : MonoBehaviour
{
    [SerializeField] private Text _starFragmentsText;
    [SerializeField] private Text _magicCrystalText;
    [SerializeField] private Text _starlightText;
    [SerializeField] private Text _starforceShardsHud;
    private void OnEnable() { /* 4 이벤트 구독 */ }
    private void OnDisable() { /* 미러 */ }
    private void Start() { /* 초기 표시 */ }
    // Update*Text 메서드 4개 (각 4줄)
}
```

**AtticCharacterSelector** (~70L):
```csharp
public class AtticCharacterSelector : MonoBehaviour
{
    [SerializeField] private CharacterButton[] _buttons;
    [SerializeField] private Text _selectedCharacterText;
    private void OnEnable() { /* 버튼 + MetaProgression.OnCharacterChanged 구독 */ }
    private void Start() { /* 저장 캐릭터 복원 + ShowFirstStartCharacterPrompt */ }
    // HandleSelectCharacter / UpdateSelectedCharacter / ResolveCharacterById
}
```

**AtticChapterEntry** (~50L):
```csharp
public class AtticChapterEntry : MonoBehaviour
{
    [SerializeField] private ChapterButton[] _buttons;
    [SerializeField] private string _chapterSceneName = "Chapter";
    private void OnEnable() { /* 버튼 wiring */ }
    private void Start() { /* HandlePendingChapterCleared (5002 D2 toast) */ }
    // HandleStart / ResolveChapterById
}
```

**AtticEndgameMenuView** (~70L):
```csharp
public class AtticEndgameMenuView : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private Text _dailyCooldown / _weeklyCooldown / _infiniteDepth;
    private void Start() { Refresh(); }
    private void Refresh() { /* AllChaptersFirstCleared + cooldown */ }
    // FormatHm / FormatDh 정적 (또는 별 헬퍼 클래스)
}
```

**AtticPanelToggleManager** (~50L):
```csharp
public class AtticPanelToggleManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _managedPanels;
    [SerializeField] private string[] _excludePanelNames = { "CharacterPanel" };
    private void Awake() { AutoDiscoverPanels(); }
    private void Update() { /* 한 번에 하나만 활성 */ }
    // AutoDiscoverPanels
}
```

**AtticReturnHandler** (~40L):
```csharp
public class AtticReturnHandler : MonoBehaviour
{
    private void Start() { HandleReturnFromBossEncounter(); }
    private void HandleReturnFromBossEncounter() { /* 5002 신화 발현 1회 toast */ }
}
```

**AtticController (~80L 잔여)** — 통합 wiring + 가챠/한정상점 진입 버튼만:
```csharp
public class AtticController : MonoBehaviour
{
    [SerializeField] private Button _gachaButton / _starforceButton / _limitedShopButton;
    [SerializeField] private GachaPanel / SlotStarforcePanel / LimitedShopPanel;
    [SerializeField] private Text _limitedShopBadgeText;
    private void OnEnable() { /* 3 버튼 wiring + LimitedOffer 이벤트 */ }
    private void Start() { Time.timeScale = 1f; AudioManager.Instance?.PlayBgm("attic_nostalgic"); RefreshLimitedShopBadge(); }
    // OpenGachaPanel / OpenStarforcePanel / OpenLimitedShopPanel / RefreshLimitedShopBadge
}
```

### 3.3 Attic.unity 씬 wiring 변경

기존 AtticController GameObject 1개에 모든 SerializeField → 7 컴포넌트로 분산:
- AtticController GameObject 에 6 신규 컴포넌트 추가 (`AddComponent`)
- 기존 SerializeField 를 새 컴포넌트의 SerializeField 로 마이그레이션 (Inspector 드래그)

**중요**: 한 GameObject 에 모든 컴포넌트 부착 (분산 X) — 디자이너 친화 + Wiring 위치 통일.

## 4. CLAUDE.md §15 정합 검증

| 정책 | 검증 |
|---|---|
| §15.1 SRP | 7 컴포넌트 각 단일 책임 ✅ |
| §15.6 100줄 룰 | AtticController 80L, 각 컴포넌트 50~70L (모두 100L 이내) ✅ |
| §15.7 DI | 각 컴포넌트 SerializeField 직접 wiring (Singleton 직접 참조 그대로 — Service/MetaProgression 정적) |
| §15.9 Test | UI MonoBehaviour Test 어려움 — Reflection 기반 Type 존재 검증 |
| §15.11 안티 | Update in 1 (PanelToggleManager 만, 기존과 동일) |
| §15.12 Validator | AtticController 200L+ 경고 해소 (위반 1건 감소 예상) |

## 5. Sprint 분해 (개요)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | AtticCurrencyView 추출 + Attic 씬 wiring | 1 | 25분 |
| 2 | AtticCharacterSelector 추출 + 씬 wiring | 1 | 25분 |
| 3 | AtticChapterEntry 추출 + 씬 wiring + pendingChapterClearedId toast | 1 | 30분 |
| 4 | AtticEndgameMenuView 추출 + 씬 wiring | 1 | 25분 |
| 5 | AtticPanelToggleManager 추출 + 씬 wiring (Update 분리) | 1 | 25분 |
| 6 | AtticReturnHandler 추출 + 씬 wiring | 1 | 20분 |
| 7 | AtticController 잔여 80L cleanup + Reflection NUnit 5 + Validator 룰 | 1 | 40분 |
| 8 | docs + changelog + CLAUDE.md §11/§12/§13/§14 갱신 | 1 | 15분 |
| **총** | | **8** | **3시간 25분** |

자세한 명세는 → [5005/5005_attic_decomposition_v1_prd.md](5005/5005_attic_decomposition_v1_prd.md)

## 6. 알려진 위험 + 완화

| 위험 | 영향 | 완화 |
|---|---|---|
| Inspector wiring 누락 | UI 작동 X (게임 진입 허브 — 치명적) | 각 Sprint 마다 Play Mode 검증 + 컴포넌트 단위 commit (회수 용이) |
| 이벤트 구독 순서 변경 | 첫 구동 시 표시 누락 | OnEnable/Start 순서 보존 (각 컴포넌트 self-init) |
| 컴포넌트 간 의존 | 컴포넌트 A 가 B 필요 시 wiring | MetaProgression / Service 정적 사용 (직접 참조 X) |
| 5002 D2 toast 회귀 | 챕터 클리어 toast 미발생 | AtticChapterEntry.Start 에서 처리 (현 위치 그대로) |
| 5004 fade 회귀 | HandleStart 의 SceneTransitionService.Transition 호출 보존 | AtticChapterEntry 로 그대로 이동 |
| MCP SIGSEGV (Sprint 1~6 씬 wiring 다수) | 도메인 리로드 위험 | 각 Sprint commit 분리 + 씬 저장 후 10초+ 대기 |
| 5001~5004 NUnit 회귀 | 테스트 영향 (UI 만 변경 — 회귀 적음) | Sprint 7 Reflection NUnit + 기존 61 회귀 검증 |

## 7. 변경 이력

- 2026-05-04: v1 초안 — 옵션 B 권장 (Component 추출, 7 컴포넌트)

## 8. Sources

- 5002 D2 mediator 결정 (415L 추가)
- CLAUDE.md §13 #10 후속 항목
- 5002 BackgroundApplier / 5004 FadeOverlay 단일 책임 컴포넌트 패턴
- 코드: AtticController 420L (Hub/AtticController.cs)
