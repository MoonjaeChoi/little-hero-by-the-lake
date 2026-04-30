# Little Hero — 사용자 테스트 가이드 (Phase A~F 완료 시점)

> 2026-05-01 자동 검증 결과: ProjectValidator 0 errors / 0 warnings, Phase C 시스템 단위 호출 OK, Phase F 시드 50/50 챕터 와이어링 + archetype 41 enemy + bossPool 적용 OK.
>
> 본 문서는 **현재 즉시 플레이 가능한 부분**과 **Unity Editor wiring 이 필요한 후속 작업**을 분리해 정리한다.

---

## 1. 즉시 플레이 가능 (씬 wiring 불필요)

### 1.1. 기본 게임플레이 흐름

다음 흐름은 Phase 0~3 의 기존 wiring 만으로 동작합니다.

1. **Unity 에서 `Assets/_Project/Scenes/Attic.unity` 열기 → Play**
2. 다락방 UI:
   - 캐릭터 선택 (3종: 용감한 소년 / 씩씩한 소녀 / 조용한 친구)
   - 챕터 선택 (3 챕터: 빨간 모자 / 잭과 콩나무 / 이상한 나라의 앨리스 — 폐기 동화 모델이지만 동작)
   - 다락방 도감 / 일기장 / 마음 단련(메타 업그레이드) 패널
3. **챕터 진입 → Chapter.unity 자동 로드**
4. 20 웨이브 진행, W5/W10/W15 미니보스, W20 메인 보스
5. 웨이브 사이 상점 페이즈 (별가루 ★ 로 새로고침·구매)
6. 챕터 클리어 → 무지개 구슬 ✦ 보상 → 다락방 복귀
7. 사망 → 다락방 복귀

### 1.2. 직접 검증된 자동 동작

다음은 **Play 모드 진입만으로 자동 동작**이 확인됨 (코드 검증 완료):

| 기능 | 상태 | 검증 방법 |
|---|---|---|
| **별빛 ⋆ 일일 로그인 5⋆** | ✅ 자동 적립 | Play 모드 진입 → MetaProgression.Starlight 5 확인 |
| **IAPService 초기화** | ✅ 백그라운드 자동 | Play 모드 진입 → Initialized=True |
| **PlayerProgress XP 적립** | ✅ 적 처치 시 자동 | 적 처치 → Xp 누적 → 50 도달 시 Lv2 |
| **EnemySpawner 챕터 곱셈** | ✅ A4+C9 통합 | 챕터별 enemyStatMultiplier 자동 적용 |
| **DamageSystem 신화 곱셈** | ✅ F8 자동 hook | 발현된 신화 ID 가 있으면 데미지 자동 보정 |
| **SaveData 영구 저장** | ✅ JSON | `Application.persistentDataPath/save.json` |

### 1.3. Editor 메뉴로 즉시 시드·검증 가능

| 메뉴 | 효과 | 멱등 |
|---|---|---|
| `Tools > LittleHero > Validate Project` | 빌드 settings·SO 검증 | ✅ |
| `Tools > LittleHero > Seed Affix SOs` | Affix 22 + SubAffix 12 SO | ✅ |
| `Tools > LittleHero > Seed Pool SOs` | SlotPool 12 + GlobalPool 1 | ✅ |
| `Tools > LittleHero > Seed BalanceConfig Curves` | 강화 비용 곡선 | ✅ |
| `Tools > LittleHero > Migrate Equipment Templates` | 16 EquipmentData → Template | ✅ |
| `Tools > LittleHero > Seed 35 Chapter SOs` | ChapterData V2 35종 | ✅ |
| `Tools > LittleHero > Seed Endgame Modes` | EndgameModeData 4종 | ✅ |
| `Tools > LittleHero > Seed Mythic SOs` | MythicDefinition 4종 | ✅ |
| `Tools > LittleHero > Seed Phase F Content` | archetype 41 + 35 챕터 wiring + 15 hardcore | ✅ |
| `Tools > LittleHero > Curve Simulator` | 35 챕터 정합 검증 리포트 | ✅ |

### 1.4. SmokeTest (자동 스크린샷)

`Chapter.unity` 의 `SmokeTest` 컴포넌트에서 인스펙터 `_enabledOnStart=true` 토글 후 Play.
1·5·15·30 초 시점 자동 PNG 캡처 → `Assets/Screenshots/`.

---

## 2. 씬 wiring 필요 (Phase D 신규 패널)

Phase D 의 패널 12종은 **MonoBehaviour + [SerializeField] 참조 구조**만 작성됨. Unity Editor 에서 Canvas hierarchy + 프리팹 + 참조 연결이 필요합니다.

### 2.1. 우선순위 (테스트 가치 ↑)

#### 🔴 1순위 — D2 Attic 4 화폐 표시 (5분)

**현재 상태**: AtticController 에 `_starFragmentsText` 만 연결, `_magicCrystalText` / `_starlightText` / endgame 메뉴 root 미연결.

**작업**:
1. Attic.unity 의 Canvas 에 Text 3개 추가 (마법 결정 ◇ / 별빛 ⋆ / endgame placeholder)
2. AtticController 의 [SerializeField] 슬롯에 드래그
3. (선택) `_endgameMenuRoot` 에 GameObject 그룹

**테스트**: Play 진입 시 별빛 ⋆ 5 표시 확인 → 일일 로그인 보상 가시화.

#### 🔴 2순위 — D7 ConversionForgePanel (15분)

**작업**:
1. Attic 에 새 Canvas Panel 생성
2. `ConversionForgePanel` 컴포넌트 부착
3. 화폐 Text 3 + 변환 버튼 4 (★→◇ x1·x5, ✦→◇ x1·x5) + 닫기 버튼
4. AtticController 에 변환 모루 진입 버튼 추가 → `ConversionForgePanel.gameObject.SetActive(true)`

**테스트**: 별가루를 미리 100,000 부여(`MetaProgression.Instance.Data.starFragments = 9999`) → 변환 버튼으로 ◇ 1 적립 확인.

#### 🟡 3순위 — D1 HUD XP 바 (10분)

**작업**:
1. Chapter.unity 의 HUD Canvas 에 Slider + Text 2개 추가 (XP 바 + 레벨 텍스트)
2. HUDController 의 `_xpBar` / `_levelText` / `_xpFill` 에 연결

**테스트**: 적 처치 → XP 바 채워짐 → 50 도달 시 노란색 강조 0.8s 후 복귀.

#### 🟡 4순위 — D8 CharacterInfoPanel (20분)

**작업**:
1. Attic 에 풀스크린 Panel
2. `CharacterInfoPanel` 부착, 4 코어 + 11 파생 + 레벨/XP Text 슬롯 연결
3. 다락방에서 "캐릭터 정보" 버튼 → Show()

#### 🟠 5~12 — 나머지 Phase D 패널

D3 신기한 지도 / D4 인벤토리 / D5 장비 카드 6단 / D6 강화 모달 / D9 별빛 상점 / D10 Endgame 메뉴 / D11 신화 오버레이 / D12 권장 레벨 경고. 각 패널의 [SerializeField] 슬롯은 **클래스 상단 Header attribute** 로 그룹핑되어 있어 Inspector 에서 한눈에 파악 가능.

### 2.2. wiring 가이드 — D5 EquipmentCardPanel 예시

가장 복잡한 패널의 wiring 절차:

```
EquipmentCardPanel (Canvas/Panel)
├── ItemIcon (Image)               ← _itemIcon
├── ItemNameText (Text)            ← _itemNameText
├── EnhanceBadge (Text)            ← _enhanceBadgeText
├── HistoryText (Text)             ← _historyText
├── BaseStatsText (Text)           ← _baseStatsText
├── OpenEnhanceButton (Button)     ← _openEnhanceButton
│   → onClick: EquipmentCardPanel.OpenEnhance() (자동)
├── MainAffixSection (RectTransform)
│   └── (Row prefab 동적 생성)     ← _mainAffixRowRoot, _mainAffixRowPrefab
├── RerollMainButton (Button)      ← _rerollMainButton
├── RerollMainCostText (Text)      ← _rerollMainCostText
├── SubAffixSection (RectTransform) ← _subAffixRowRoot, _subAffixRowPrefab
├── RerollSubButton (Button)       ← _rerollSubButton
├── RerollSubCostText (Text)       ← _rerollSubCostText
├── MythicSection (GameObject)     ← _mythicRoot
│   ├── MythicNameText (Text)      ← _mythicNameText
│   └── MythicFlavorText (Text)    ← _mythicFlavorText
└── CloseButton (Button)           ← _closeButton

EnhanceModalPanel (Canvas/Panel, 다른 Layer)
└── ... → _enhanceModal 슬롯에 연결

[Inspector 추가 SerializeField]
- _balanceConfig: Assets/_Project/Data/BalanceConfig.asset
- _slotPools: SlotPool_Hat ~ SlotPool_Pet (12개)
- _globalSubPool: GlobalSubAffixPool_Default
- _mythicLookup: Mythic_DadCompass ~ Mythic_DadPromise (4개)
```

EquipmentCardRow.prefab 1개 생성:
- Background (Image) ← _background
- Label (Text) ← _label
- LockToggle (Toggle) ← _lockToggle

---

## 3. 콘솔 호출로 검증 가능한 시스템 (UI 없이)

UI wiring 없이도 Editor `execute_code` 로 직접 호출하여 검증 가능합니다.

### 3.1. 강화 시뮬레이션

```csharp
// Editor 메뉴 또는 execute_code 로:
var balance = AssetDatabase.LoadAssetAtPath<BalanceConfig>("Assets/_Project/Data/BalanceConfig.asset");
var instance = new EquipmentInstance { enhanceLevel = 0, failCount = 0 };
for (int i = 0; i < 25; i++) {
    var result = EnhancementService.TryEnhance(instance, balance);
    Debug.Log($"+{instance.enhanceLevel} fc={instance.failCount} outcome={result.outcome}");
}
```

### 3.2. 드랍 시뮬레이션

```csharp
ItemDropService.Initialize(slotPools, globalSub, templates, balance);
for (int i = 0; i < 100; i++) {
    var inst = ItemDropService.TryDrop(new DropContext { source = DropSource.Enemy, chapterNumber = 5 });
    if (inst != null) Debug.Log(inst.templateId);
}
```

### 3.3. 변환 모루 시뮬

```csharp
GameManager.Instance.AddStarDust(5000);
ConversionForge.ConvertDustToCrystal(5);  // 5,000 ★ → 5 ◇
Debug.Log(CurrencyService.MagicCrystals);  // 5
```

### 3.4. 별빛 무료 출처 시뮬

```csharp
StarlightGrantService.TryGrantDailyLogin();           // +5 ⋆
StarlightGrantService.TryGrantChapterFirstClear(7);   // +14 ⋆ (7×2)
StarlightGrantService.TryGrantMemoryFound("Memory_DadPhoto");  // +10 ⋆
StarlightGrantService.TryGrantBestiaryFull(3);        // +50 ⋆
```

### 3.5. CurveSimulator (자동 리포트)

`Tools > LittleHero > Curve Simulator` 메뉴 — 35 챕터 × Lv 1~50 매트릭스, TTK·DPS·HP·자원 곡선을 `simulation_report.md` 로 자동 생성.

---

## 4. 출시 전 후속 (수동 작업)

### Phase E (수익화)
1. **App Store Connect / Google Play Console** 에 6 productId 등록 (`com.littlehero.starlight_*` + `com.littlehero.season_pass`)
2. **Window > Services > In-App Purchasing** 활성화 + Tangle 키 생성 (`AppleTangle.cs` / `GooglePlayTangle.cs` 자동 생성)
3. **시즌 로테이션 cron** (출시 3개월차)

### Phase F (콘텐츠)
1. archetype 적 SO → 챕터별 고유 명·외형·정밀 밸런싱 (디자인 패스)
2. 일일/주간 보스 처치 매니저 (요일/주차 인덱스 → bossPool[idx] spawn)
3. F5 신화 보장 ForceRoll wiring (주간 보스 처치 시)
4. F6 InfiniteTowerScene 분리

### Phase G (출시)
- G1 사운드 16종 + BGM 6종
- G2 단색 더미 → 실 스프라이트 교체
- G3 한국어 어휘 검수 (어린이 친화)
- G4 iOS 빌드 + TestFlight
- G5 App Store 심사 + 4+ 등급 인증
- G6 출시 후 라이브 운영

---

## 5. 알려진 제약 (현재 시점)

- **D9 별빛 상점**: 패키지 가격은 IAPService 미초기화 시 fallback 한국 원화. App Store Connect 미등록 상태에서는 InitiatePurchase 가 실패함.
- **Glasses / Earring / Bottom / Gloves / Ring 슬롯 EquipmentTemplate 부재**: equipment §4.4 비고대로 MVP 후 콘텐츠 패치에서 추가. 현재 ItemDropService 에서 forced slot 으로 요청 시 null 반환(graceful fallback).
- **Phase D 패널 wiring 미완**: 위 §2 참조. 실제 화폐·강화·인벤토리 UI 는 wiring 후 사용 가능.
- **챕터 1~3 (폐기 동화 모델)** 과 **CH_01~35 V2 + CH_36~50 hardcore** 가 공존. Attic 의 "챕터 선택" 버튼은 폐기 모델의 3 챕터를 가리킴 — 신규 35 챕터 사용은 D3 WondrousMapPanel wiring 필요.
- **firstClearedChapters int 32bit 한계**: 35 비트 표현 불가. CH35 클리어 비트 마스킹은 long 승격 후속 작업.
