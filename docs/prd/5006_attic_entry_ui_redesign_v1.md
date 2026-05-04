# 5006 — 다락방 진입점 UI 재구성 (Attic Entry UI Redesign v1)

> **목적**: 13개 진입 버튼이 모두 동일 `WoodButton.png` 공용 → 시각 구분 불가 + 우측 상단 클러스터 → 한 손 플레이 가독성 저해. 카테고리 그룹화 + 전용 아이콘 + 가독성·접근성 개선.
> **적용 우선순위**: §15 OOP 정합 + §2 PG-12 톤·44pt 터치·색맹 친화 + §10 uGUI Legacy Text.
> **연계 시스템**: 5005 attic_decomposition_v1 (7 Hub 컴포넌트, 책임 분리 직후) — 본 시스템은 책임 분리 후 UI 표현 계층의 후속 사이클.

## 0. 결정 이력

- 2026-05-04: 사용자 발화 "최초 다락방 진입점 UI 재구성, 버튼 배치 및 가독성 향상, 버튼 전용 이미지 생성" → 본 디자인 문서 작성 시작 (PGE Phase 1).
- 2026-05-04: 5005 직후 — Attic GameObject 7 컴포넌트 분리 완료. 이 위에 진입 버튼 wiring 보강 + 신규 아이콘 생성이 자연스러운 다음 단계.

## 1. 현재 시스템 분석

### 1.1 Attic 진입 버튼 13종 (모두 `WoodButton.png` 공용)

| # | 버튼 | 진입 Panel | wiring 상태 | 실제 컴포넌트 |
|---|---|---|---|---|
| 1 | 일기 | DiaryPanel | ✅ Panel._openButton | `DiaryPanel` |
| 2 | 도감 | BestiaryPanel | ✅ Panel._openButton | `BestiaryPanel` |
| 3 | 캐릭터 정보 | CharacterInfoPanel | ⚠️ _closeButton 만 | `CharacterInfoPanel` |
| 4 | 마음 단련 | MetaUpgradePanel | ✅ Panel._openButton | `MetaUpgradePanel` |
| 5 | **가챠** | GachaPanel | ✅ AtticController._gachaButton | `AtticController` |
| 6 | **마법의돌** | SlotStarforcePanel | ✅ AtticController._starforceButton | `AtticController` |
| 7 | **한정 상점** | LimitedShopPanel | ✅ AtticController._limitedShopButton | `AtticController` |
| 8 | 별빛 상점 | StarlightShopPanel | ❌ Open button 미구현 | `StarlightShopPanel` |
| 9 | 신기한 지도 | WondrousMapPanel | ❌ Open button 미구현 | `WondrousMapPanel` |
| 10 | 인벤토리 | InventoryPanel | ❌ Open button 미구현 | `InventoryPanel` |
| 11 | 강화 | EnhanceModalPanel | ❌ (인벤토리 진입 후 modal) | `EnhanceModalPanel` |
| 12 | 변환 모루 | ConversionForgePanel | ⚠️ _closeButton 만 | `ConversionForgePanel` |
| 13 | Endgame | EndgameMenuPanel | ⚠️ AtticEndgameMenuView 가 root 표시만 | `EndgameMenuPanel` |

> **위반**: 13개 버튼이 동일 sprite + 동일 색 → §2 색맹 친화·가독성 위반. 우측 상단 클러스터 → 한 손 플레이 시 손가락 가림.

### 1.2 5005 Hub 컴포넌트 책임 (재확인)

| 컴포넌트 | 책임 | 라인 |
|---|---|---|
| `AtticController` | 가챠/마법의돌/한정상점 (3 버튼) + BGM | 111 |
| `AtticCurrencyView` | ✦◇⋆◈ 4 화폐 표시 | 47 |
| `AtticCharacterSelector` | 3 캐릭터 선택 + highlight | 101 |
| `AtticChapterEntry` | 35 챕터 + toast | 80 |
| `AtticEndgameMenuView` | Endgame root + cooldown | 71 |
| `AtticPanelToggleManager` | 패널 토글 (한 번에 하나만 활성) | 72 |
| `AtticReturnHandler` | 신화 발현 toast | 35 |

> 본 시스템은 7 컴포넌트 안에 진입 버튼 책임을 **재분배** 또는 **신규 컴포넌트 (AtticEntryButtonGrid 등)** 추가 옵션을 비교한다.

### 1.3 UI 아트 자산 inventory

| 폴더 | 자산 수 | 비고 |
|---|---|---|
| `Art/Generated_v2/UI/Attic/` | 4 | AtticBackground / CurrencyFrame / PaperFrame / **WoodButton (공용)** |
| `Art/Generated_v2/Currency/` | 2 | starforce_shard / summon_ticket — ★ ✦ ◇ ⋆ 4종 누락 |
| `Art/Generated_v2/Frames/` | 6 | common/magic/rare/epic/legendary/mythic |
| **누계 V5 PNG** | 412 | CLAUDE.md §12 (PIVOT 후 통일) |

> **결정 필요**: 신규 진입 버튼 아이콘 9~14개. 기존 V5 sticker dynamic tone 시드 22222 일관성 유지.

### 1.4 ComfyUI / 이미지 생성 인프라

| 항목 | 상태 |
|---|---|
| `tools/comfyui_*` 워크플로 JSON | ❌ 없음 |
| `scripts/imagen/` 배치 | ❌ 없음 |
| `tools/bgm_generate.sh` | ✅ (음악만) |

> V5 sticker tone 자산은 외부 (수동 또는 별도 환경) 에서 생성된 것으로 추정. 후속 작업 시 **자동화 스크립트 신규 구축** 또는 **사용자 수동 생성 후 임포트** 두 옵션.

## 2. 옵션 비교

### 2.1 D1 — 버튼 배치 정책

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | 우측 상단 13 버튼 클러스터 유지 + 색만 tint | 코드 변경 최소 | 한 손 가림·구별성 미흡 |
| **B (★권장)** | **카테고리 4 그룹 + 좌우 분산** (좌측 진행/우측 상점) | 한 손 플레이 ✅ + 그룹화 가독성 ✅ | 버튼 위치 재배치 + Inspector wiring 9곳 |
| **C** | 하단 탭바 (Bottom Navigation) 5탭 | 모바일 표준 | uGUI 재구조 큼 + 다락방 하단 캐릭터 영역 침범 |
| **D** | 햄버거 메뉴 1버튼 + 모달 | 화면 청정 | 한 단계 더 클릭 (UX 마찰) |

**권장**: **B** — 한 손 플레이 우선 + 그룹화 가독성 + 코드 변경 중간.

### 2.2 D2 — 버튼 카테고리 구조

| 옵션 | 그룹화 | 버튼 수 |
|---|---|---|
| **A** | 6 그룹 (콘텐츠/장비/화폐/상점/Endgame/캐릭터) | 13~14 |
| **B (★권장)** | **4 그룹** (전투/장비/상점/진행) | 13 |
| **C** | 3 그룹 (Play/Shop/Meta) | 13 |

**권장 B 구조**:

```
Group 1 — 전투 진행 (좌측 상단)
  • 챕터 (이미 별도 — AtticChapterEntry.버튼 35종 그리드)
  • Endgame 메뉴 (CH 35 클리어 후 활성)
  • 신기한 지도 (챕터 권장 레벨 표시)

Group 2 — 캐릭터·장비 (좌측 하단)
  • 캐릭터 정보
  • 인벤토리
  • 강화 (인벤토리 → modal 이라면 직진입 X)
  • 마음 단련 (메타 업그레이드)

Group 3 — 상점·소환 (우측 상단)
  • 가챠 (소환)
  • 마법의돌
  • 한정 상점
  • 별빛 상점
  • 변환 모루

Group 4 — 진행 기록 (우측 하단)
  • 일기 (추억)
  • 도감 (적)
```

### 2.3 D3 — 신규 아이콘 생성 방식

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A** | ComfyUI batch 워크플로 신규 (`tools/comfyui_attic_buttons/`) | 자동화 + 시드 22222 일관성 | 환경 설치 부담 |
| **B (★권장)** | **사용자 수동 생성 (외부 ComfyUI/Midjourney) + 임포트 단계만 자동화** | 진행 즉시 가능 + V5 톤 사용자 통제 | 자동화 X (1회 작업) |
| **C** | 색상 tint 만 — Inspector Color override (코드 변경 X) | 0 자산 | 시각 구별성 약함 |
| **D** | A + C 혼합 (1차 tint 즉시 + 후속 ComfyUI 자산) | 단계적 진행 | Sprint 2회 분할 |

**권장**: **D** (단계적) — Sprint 1 (tint 즉시 가독성 ↑) → Sprint 후속 (신규 아이콘 임포트). 본 PRD 는 Sprint 1+2 까지 (tint + wiring), 신규 아이콘 임포트는 사용자 작업 후 별도 사이클.

### 2.4 D4 — 신규 컴포넌트 도입 여부

| 옵션 | 설명 | 라인 추정 |
|---|---|---|
| **A** | `AtticEntryButtonGrid.cs` 신규 — 13 진입 버튼 통합 관리 | ~120L (§15.6 약간 위반 — 분할 필요) |
| **B (★권장)** | **2 신규 컴포넌트 분할**: `AtticPanelEntryView` (10 panel 진입) + 기존 컴포넌트는 그대로 | 각 ~70L (§15.6 통과) |
| **C** | 기존 AtticController 에 통합 | 111→230L (§15.6 위반) |

**권장**: **B** — `AtticPanelEntryView` (panel 10 진입 버튼) + `AtticGroupLayoutPresenter` (4 그룹 RectTransform 정렬). AtticController 잔여 책임 (BGM) 만 유지.

### 2.5 D5 — 화폐 아이콘 4종 누락 처리

| 옵션 | 설명 |
|---|---|
| **A** | 본 PRD 범위 외 (별도 사이클) |
| **B (★권장)** | **본 PRD 와 함께 ★/✦/◇/⋆ 4 아이콘 임포트** (사용자 수동 생성) |

**권장 B**: 진입 버튼 + 화폐 아이콘 동시 통일 — V5 톤 일관성.

## 3. 권장 설계 (CLAUDE.md §15 정합)

### 3.1 신규 파일 (3개)

| 파일 | 책임 | 추정 라인 |
|---|---|---|
| `Scripts/Hub/AtticPanelEntryView.cs` | 10 panel 진입 버튼 (Diary/Bestiary/CharInfo/MetaUpgrade/Inventory/StarlightShop/WondrousMap/ConversionForge/EndgameMenu/그 외) wiring | ~80 |
| `Scripts/Hub/AtticGroupLayoutPresenter.cs` | 4 그룹 RectTransform 위치/순서 명시 (Inspector SerializeField, 코드 hardcode X) | ~50 |
| `Scripts/Editor/AtticUIIconImporter.cs` | 사용자 수동 생성 PNG 임포트 후 Sprite import settings 일괄 정합 (PixelsPerUnit / Border / Filter) | ~60 |

> AtticController 의 `_gachaButton / _starforceButton / _limitedShopButton` 은 그대로 유지 (5005 결정 잔여 책임). AtticPanelEntryView 는 다른 10개 panel 만 담당 → 책임 중복 X.

### 3.2 RectTransform 4 그룹 레이아웃

```
┌─────────────────────────────────────────────────────────┐
│ AtticBackground.png                                     │
│ ┌─────────┐                              ┌─────────┐   │
│ │ G1 전투  │                              │ G3 상점  │   │
│ │ • 챕터   │                              │ • 가챠   │   │
│ │ • Endgame│                              │ • 마법의돌│   │
│ │ • 지도   │                              │ • 한정   │   │
│ └─────────┘                              │ • 별빛   │   │
│                                           │ • 모루   │   │
│                                           └─────────┘   │
│                                                          │
│         ┌─ 캐릭터 영역 (3 portrait, 5005 S2) ─┐         │
│         │     • 용감한 소년                  │          │
│         │     • 씩씩한 소녀                  │          │
│         │     • 조용한 친구                  │          │
│         └────────────────────────────────────┘         │
│                                                          │
│ ┌─────────┐                              ┌─────────┐   │
│ │ G2 장비  │                              │ G4 기록  │   │
│ │ • 정보   │                              │ • 일기   │   │
│ │ • 인벤토리│                              │ • 도감   │   │
│ │ • 강화   │                              └─────────┘   │
│ │ • 단련   │                                            │
│ └─────────┘                                              │
│                                                          │
│  💰 ✦ ◇ ⋆ ◈ (AtticCurrencyView, 5005 S1, 하단 띠)      │
└─────────────────────────────────────────────────────────┘
```

### 3.3 신규 아이콘 명세 (D5 포함 14개)

| 카테고리 | 아이콘 | 모티브 | V5 톤 색 강조 |
|---|---|---|---|
| G1 | btn_endgame | 보스 실루엣 + 별 | 빨강 (위협) |
| G1 | btn_wondrous_map | 두루마리 지도 | 갈색 (탐험) |
| G2 | btn_char_info | 캐릭터 portrait outline | 파랑 (정체성) |
| G2 | btn_inventory | 가방 + 별 | 갈색 (수납) |
| G2 | btn_enhance | 망치 + 별 sparkle | 노랑 (강화) |
| G2 | btn_meta_upgrade | 별 + 위쪽 화살 | 보라 (상승) |
| G3 | btn_gacha | 마법 결정 박스 | 보라 (희귀) |
| G3 | btn_starforce | ⭐ + 빛 | 노랑 (별) |
| G3 | btn_limited_shop | 시계 + 가격표 | 주황 (시간 제한) |
| G3 | btn_starlight_shop | 별 + 동전 | 청록 (별빛 ⋆) |
| G3 | btn_conversion_forge | 모루 + 화살표 | 회갈색 (변환) |
| G4 | btn_diary | 책 + 추억 사진 | 베이지 (따뜻) |
| G4 | btn_bestiary | 적 실루엣 + 책 | 녹색 (자연) |
| 화폐 | icon_★/✦/◇/⋆ | 별가루/무지개구슬/마법결정/별빛 | 4종 |

> **주의** PG-12 톤 (CLAUDE.md §2): 만화풍 + 따뜻 색감. 전투 아이콘도 위협적 X (보스 실루엣 ≠ 피·잔해).

### 3.4 AtticPanelEntryView 명세

```csharp
namespace LittleHero
{
    /// <summary>
    /// 5006 Sprint 2 — 10 panel 진입 버튼 wiring (5005 AtticController 잔여 3개 제외).
    /// §15.1 SRP: 진입 버튼 list + onClick → panel.SetActive(true).
    /// §15.7 DI: 각 panel reference 는 SerializeField 명시 (Singleton 직접 참조 X).
    /// </summary>
    public class AtticPanelEntryView : MonoBehaviour
    {
        [System.Serializable]
        public class PanelEntry
        {
            public string id;
            public Button button;
            public GameObject panelRoot;
        }

        [SerializeField] private PanelEntry[] _entries;

        private void OnEnable()
        {
            if (_entries == null) return;
            for (int i = 0; i < _entries.Length; i++)
            {
                var e = _entries[i];
                if (e?.button == null || e.panelRoot == null) continue;
                var captured = e.panelRoot;
                e.button.onClick.AddListener(() => captured.SetActive(true));
            }
        }

        private void OnDisable()
        {
            if (_entries == null) return;
            foreach (var e in _entries)
                if (e?.button != null) e.button.onClick.RemoveAllListeners();
        }
    }
}
```

### 3.5 AtticGroupLayoutPresenter 명세

```csharp
namespace LittleHero
{
    /// <summary>
    /// 5006 Sprint 1 — 4 그룹 RectTransform 위치 명시. Inspector 만 사용 (코드 hardcode X).
    /// 한 손 플레이 ✅ — 좌상/우상/좌하/우하 4 그룹 anchor 사전 설정.
    /// </summary>
    public class AtticGroupLayoutPresenter : MonoBehaviour
    {
        [Header("4 그룹 RectTransform 컨테이너")]
        [SerializeField] private RectTransform _group1Combat;     // 좌상 — 전투
        [SerializeField] private RectTransform _group2Equipment;  // 좌하 — 장비
        [SerializeField] private RectTransform _group3Shop;       // 우상 — 상점
        [SerializeField] private RectTransform _group4Record;     // 우하 — 기록

        [Header("44pt 터치 타겟 검증 (CLAUDE.md §2)")]
        [SerializeField] private float _minTouchSize = 44f;

        private void OnValidate()
        {
            ValidateTouchSize(_group1Combat);
            ValidateTouchSize(_group2Equipment);
            ValidateTouchSize(_group3Shop);
            ValidateTouchSize(_group4Record);
        }

        private void ValidateTouchSize(RectTransform group)
        {
            if (group == null) return;
            foreach (Transform child in group)
            {
                var rect = child.GetComponent<RectTransform>();
                if (rect == null) continue;
                if (rect.sizeDelta.x < _minTouchSize || rect.sizeDelta.y < _minTouchSize)
                    Debug.LogWarning($"[AtticGroup] {child.name} 터치 타겟 < {_minTouchSize}pt");
            }
        }
    }
}
```

### 3.6 §15 OOP 정합 검증

| 정책 | 검증 |
|---|---|
| §15.1 SOLID-S | AtticPanelEntryView (진입만) + AtticGroupLayoutPresenter (레이아웃만) — 분리 ✅ |
| §15.4 SO 우선 | 모든 panel reference = SerializeField (hardcode X) ✅ |
| §15.6 100줄 룰 | 신규 클래스 모두 100L 이내 (~80/~50/~60) ✅ |
| §15.7 DI | Initialize 명시 X — Inspector wiring 만 (단순 view) ✅ |
| §15.11 안티 패턴 | public 필드 X / Update X / Instantiate X ✅ |
| §15.12 Validator | AtticPanelEntryView/AtticGroupLayoutPresenter 100L 이내 — 기존 룰로 충분 |

## 4. Sprint 분해 (개요)

자세한 Sprint × commit × 시간 표는 `docs/prd/5006/5006_attic_entry_ui_redesign_v1_prd.md` 참조.

| Sprint | 작업 | 의존성 |
|---|---|---|
| 1 | AtticGroupLayoutPresenter + 4 그룹 RectTransform + tint 가독성 1차 | — |
| 2 | AtticPanelEntryView + 10 panel 진입 wiring | 1 |
| 3 | 누락 panel Open button SerializeField 보강 (CharInfo/Forge/Endgame) | 2 |
| 4 | 사용자 수동 생성 신규 14 PNG 임포트 + AtticUIIconImporter | 1 (병렬 가능) |
| 5 | NUnit AtticEntryUITests + Validator 룰 | 2~4 |
| 6 | docs (changelog + CLAUDE.md §11/§12/§14) | 모두 |

## 5. 알려진 위험 + 완화

| 위험 | 영향 | 완화 |
|---|---|---|
| 사용자 수동 PNG 생성 의존 | Sprint 4 차단 | Sprint 1~3+5+6 진행 가능, Sprint 4 는 PNG 도착 시점 진행 |
| 13 버튼 → 4 그룹 재배치 시 기존 wiring 끊김 | Inspector 작업 누락 위험 | 사용자 wiring 단계 명세 + Sprint별 검증 시나리오 |
| 5005 AtticPanelToggleManager 와 충돌 | 새 panel 자동 탐색 시 정책 누락 | _excludePanelNames 갱신 검토 (CharacterPanel 외) |
| TMP 사용 불가 (CLAUDE.md §10) | 폰트 가독성 한계 | Legacy Text + outline component 활용 권장 |
| 한 손 플레이 시 우측 그룹 손가락 가림 | UX 저하 | 좌/우 분산 + 하단 띠 화폐 ✅ + 시안 검증 |
| 색맹 친화 부족 | 등급 구별 미흡 | 색 외 형태 (아이콘 모티브 + 텍스트 라벨) 병행 |

## 6. Definition of Done

- [ ] AtticPanelEntryView + AtticGroupLayoutPresenter 신규 (각 100L 이내)
- [ ] AtticController 잔여 책임 그대로 (BGM + 가챠/마법의돌/한정상점 3 버튼)
- [ ] 13 진입 버튼 모두 onClick 수신 → 해당 panel 활성화
- [ ] 4 그룹 RectTransform 좌상/좌하/우상/우하 anchor 적용
- [ ] 신규 아이콘 14 PNG 임포트 (사용자 작업 후) + Sprite import 일괄 정합
- [ ] NUnit 6+ (AtticPanelEntryView_HasEntries / AtticGroupLayoutPresenter_HasGroups / etc)
- [ ] ProjectValidator 신규 위반 0
- [ ] CLAUDE.md §11/§12/§14 갱신
- [ ] changelog_v1.md 항목 추가

## 7. 변경 이력

- 2026-05-04: 초기 작성 (Phase 1 Generate). 옵션 D1=B / D2=B / D3=D / D4=B / D5=B 권장. Sprint 1~6 분해.

## 8. 출처

- 5005 attic_decomposition_v1 (`docs/prd/5005_attic_decomposition.md` + 산출 8 파일)
- CLAUDE.md §2 PG-12·접근성 / §10 uGUI / §12 UI 자산 inventory / §15 OOP 정책
- Explore agent 보고서 (2026-05-04, Hub 7 컴포넌트 + 13 진입 버튼 + Art/Generated_v2/UI 자산 분석)
- gacha_v1.md §5 진입 버튼 명세 / limited_shop_v1.md §7.3
