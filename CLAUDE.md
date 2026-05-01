# Little Hero by the Lake

> 호숫가 초등학교에 다니는 한 어린이.
> 복층 아파트 다락방에서 아빠의 어린 시절 동화책을 발견한다.
> 책장을 넘기자, 그림 속으로 빨려들어갔다 —
> 아빠도 한때 이 책으로 모험을 떠난 작은 영웅이었다는 걸, 아직은 모른 채.

## 1. 프로젝트 개요

| 항목 | 값 |
|---|---|
| 게임명 | Little Hero by the Lake (호숫가 작은 영웅) |
| 저장소 | `little-hero-by-the-lake` |
| 장르 | Auto-Shooter Roguelite |
| 영감 | Vampire Survivors / Brotato (메커닉) + 지브리 (정서) |
| 진행 방식 | Brotato식 (20웨이브 / 챕터, 웨이브 사이 상점) |
| 플랫폼 | iOS (1차) → Android, macOS |
| 엔진 | Unity 6.3 LTS, URP 2D |
| 언어 | C# (.NET Standard 2.1) |
| 입력 | 가상 조이스틱 (모바일), 키보드 (에디터) |
| 톤 | 귀엽고 비폭력적, 4+ 등급 |
| 1차 시장 | 한국 / 일본 / 미국 가족·캐주얼 |

## 2. 세계관 & 스토리

### 핵심 컨셉
- **현실 허브**: 호숫가 마을 — 초등학교, 호수, 우리집(복층 아파트), 다락방
- **모험 무대**: 다락방에서 발견한 **아빠의 동화책**. 책장을 펼치면 그림 속으로 빨려들어감
- **이중 의미의 "Little Hero"**: 주인공이자, 동시에 어린 시절의 아빠

### 감정 코어
> 아빠도 어렸을 때 이 책으로 모험했다는 사실을 점차 알게 된다.
> 책에 적힌 아빠의 어린 글씨, 낙서, 모은 스티커들이 챕터를 거치며 단서로 풀린다.
> 마지막 페이지에는 아빠가 미완성으로 남긴 챕터가 있다. 그걸 자녀가 마무리한다.

### 챕터 구성 (7권 + 1)
1. 빨간 모자
2. 잭과 콩나무
3. 이상한 나라의 앨리스
4. 헨젤과 그레텔
5. 오즈의 마법사
6. 인어공주
7. 피노키오
- **마지막 페이지** (히든): 아빠가 미처 못 끝낸 챕터 — 진엔딩

### 절대 원칙: 비폭력 톤
- 모든 시각효과는 **별 / 하트 / 꽃잎 / 비눗방울** 파티클
- ❌ 금지: 피, 시체, 잔해, 검은 폭발, 비명 사운드
- ✅ 적 처치 시: 별 흩어짐 + "퐁!" 사운드 + 반짝임
- ✅ 적 피격 시: 만화풍 "별 빙글빙글" KO 표현
- ✅ 보스 사망: 풍선처럼 둥둥 떠올라 사라짐
- 색 채도 60~70% (파스텔), 너무 쨍한 원색 금지

### 텍스트 가이드
- 모든 UI 텍스트는 한국어 우선
- 어린이가 이해 가능한 어휘 (한자어, 영어 줄임말 ❌)
- 적 이름은 친근한 별명풍 ("꼬마 슬라임", "졸린 큰 곰")
- 아빠의 메모는 어린이의 손글씨 폰트로 표현

### 접근성
- 한 손 플레이 가능한 레이아웃 (가상 조이스틱은 좌측 하단 고정)
- 모든 터치 타겟 최소 44pt (Apple 가이드라인)
- 색맹 친화: 데미지/피격은 색이 아닌 형태로도 구분

## 3. 폴더 구조

```
Assets/
└── _Project/
    ├── Scripts/
    │   ├── Core/             # GameManager, EventBus, ObjectPool, SaveSystem
    │   ├── Data/             # ScriptableObject 클래스 정의
    │   │   ├── EquipmentData.cs
    │   │   ├── EnemyData.cs
    │   │   ├── ChapterData.cs
    │   │   ├── CharacterData.cs
    │   │   ├── MemoryData.cs       # 아빠의 추억 아이템
    │   │   └── BalanceConfig.cs
    │   ├── Gameplay/
    │   │   ├── Player/       # PlayerController, Stats, Health
    │   │   ├── Equipment/    # EquipmentManager, IAttackPattern, 7종 패턴
    │   │   ├── Enemies/      # EnemyController, EnemyAI, Spawner
    │   │   ├── Waves/        # WaveManager, ChapterRunner
    │   │   ├── Combat/       # DamageSystem, Projectile, AOE
    │   │   └── Items/        # XPGem, Pickup, MemoryPickup
    │   ├── Hub/              # 호숫가 마을 + 다락방 (현실 허브 씬)
    │   ├── Meta/             # MetaProgression, Collection, Shop
    │   └── UI/               # HUD, ShopPanel, LevelUpPanel, Inventory, Diary
    ├── Data/                 # SO 인스턴스 에셋
    │   ├── Equipment/
    │   ├── Enemies/
    │   ├── Chapters/
    │   ├── Characters/
    │   └── Memories/         # 아빠 메모/낙서/스티커 데이터
    ├── Prefabs/
    ├── Art/
    ├── Audio/
    └── Scenes/
        ├── MainMenu.unity
        ├── LakesideTown.unity    # 호숫가 마을 (허브)
        ├── Attic.unity           # 다락방 (책 선택)
        └── Chapter.unity         # 동화책 챕터 (모든 챕터 공통)
```

## 4. 아키텍처 규칙 (3계층 분리)

### 데이터 흐름
```
[Data Layer]      ScriptableObject — 모든 밸런스/콘텐츠
      ↓ 읽기
[Logic Layer]     MonoBehaviour Manager + EventBus
      ↓ 이벤트
[UI Layer]        uGUI Panel/HUD (데이터 수정 금지, 이벤트만 구독)
```

### 핵심 EventBus 이벤트
```csharp
public static class GameEvents
{
    public static event Action<EnemyData> OnEnemyKilled;
    public static event Action<int> OnLevelUp;
    public static event Action<int> OnWaveCleared;
    public static event Action<SlotType, EquipmentData> OnEquipmentChanged;
    public static event Action OnShopOpened;
    public static event Action OnShopClosed;
    public static event Action<int> OnStarDustChanged;
    public static event Action<int> OnPlayerHealthChanged;
    public static event Action OnPlayerDied;                    // 플레이어 사망
    public static event Action<MemoryData> OnMemoryDiscovered;  // 아빠 추억 발견
    public static event Action<ChapterData> OnChapterCleared;
    public static event Action<int> OnMagicCrystalChanged;      // ◇ 마법 결정 (영구)
    public static event Action<int> OnStarlightChanged;         // ⋆ 별빛 (캐시)
    public static event Action<EquipmentInstance> OnAffixRolled; // C1 굴림/리롤 결과
}
```

### ScriptableObject 데이터 예시
```csharp
[CreateAssetMenu(menuName = "LittleHero/Equipment")]
public class EquipmentData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public SlotType slot;          // Head, Top, Hand, Foot, Acc1, Acc2, Bag
    public Rarity rarity;

    [Header("스탯 보너스")]
    public StatModifiers statBonus;

    [Header("자동 공격 (선택)")]
    public AttackPatternType attackPattern;
    public AttackPatternConfig attackConfig;

    [Header("세트 효과 (선택)")]
    public string setId;
}
```

## 5. 코딩 규칙

- **네이밍**: PascalCase 클래스/메서드, _camelCase private 필드, UPPER_SNAKE 상수
- **MonoBehaviour 100줄 룰**: 100줄 넘으면 책임 분리
- **Update 사용 최소화**: 코루틴, 이벤트, 또는 매니저의 단일 Update에 위임
- **Instantiate 직접 호출 금지**: 반드시 `ObjectPool<T>` 통해 생성
- **GC 최소화**: 매 프레임 new ❌ (특히 Vector3, List)
- **Magic Number 금지**: 모든 수치는 SO 또는 const
- **public 필드 최소화**: SerializeField + private 우선

## 6. 핵심 시스템

### 장비 시스템
- 7개 슬롯: `Head`, `Top`, `Hand`, `Foot`, `Acc1`, `Acc2`, `Bag`
- 모든 슬롯에 능동 효과 부착 가능
- 장착/해제 시 `GameEvents.OnEquipmentChanged` 발행
- `EquipmentManager`가 현재 장착 목록 관리, `IAttackPattern` 활성화

### 자동 공격 패턴 (7종)
모든 패턴은 `IAttackPattern` 인터페이스 구현:
```csharp
public interface IAttackPattern
{
    void Initialize(AttackPatternConfig config, Transform owner);
    void Tick(float deltaTime);
    void Cleanup();
}
```
1. **Projectile** — 가장 가까운 적 방향 발사
2. **Aura** — 플레이어 주변 지속 데미지
3. **Orbit** — 플레이어 주위 회전 오브젝트
4. **Rain** — 화면 위에서 떨어짐
5. **Trail** — 이동 경로에 데미지 영역
6. **Pulse** — 주기적 광역 폭발
7. **Summon** — 작은 동료가 따라다니며 공격

### 웨이브 시스템 (Brotato식)
- 1챕터 = 20웨이브
- 각 웨이브 30~60초
- **5/10/15 웨이브**: 미니보스
- **20 웨이브**: 챕터 보스
- 웨이브 종료 시 자동 일시정지 → 상점 페이즈

### 상점 페이즈
- 6칸 상품 슬롯 (장비 4 + 강화 2)
- 새로고침 비용 별가루 (점차 비싸짐)
- 인벤토리 드래그&드롭으로 슬롯 교체
- "출발" 버튼으로 다음 웨이브

### 메타 시스템
- **별가루 (런 내)**: `★` 아이콘. 적 처치로 획득, 상점에서 소비. 사망/클리어 시 0으로 리셋. HUD 표시 `별가루 ★ N`, 상점 `새로고침 (별가루 ★ N)` / 슬롯 `★ N`.
- **무지개 구슬 (영구)**: `✦` 아이콘. 챕터 클리어 보상으로만 획득(CH1=5/CH2=8/CH3=12), 마음 단련에서 소비. SaveSystem에 영구 저장. UI 표시 `무지개 구슬 ✦ N` / 강화 비용 `(✦ N)`.
- **마법 결정 (영구, 강화 전용)**: `◇` 아이콘. 보스 처치 시 드랍, +16 이상 강화에만 사용 (economy_v1 §7).
- **별빛 (영구, 캐시)**: `⋆` 아이콘. 실금 + 일일 로그인 + 챕터 첫 클리어 + endgame 보스. 장비 강화 가속 전용 (monetization_v1).
- 네 화폐는 **아이콘과 라벨로 명확히 구분**한다. ★(별가루) / ✦(무지개 구슬) / ◇(마법 결정) / ⋆(별빛)을 혼용하지 않는다.
- **변환 모루** (다락방 신규): 별가루 1000 ★ → 마법 결정 1 ◇, 무지개 구슬 5 ✦ → 마법 결정 1 ◇ (마법 결정 적자 자체 해결).
- **도감**: 처치한 적 자동 등록, 100% 채우면 펫으로 동행 가능

### 아빠의 추억 시스템 (스토리 코어)
- 챕터 진행 중 일정 확률로 **MemoryPickup** 등장
- 종류: 아빠 어릴 적 사진, 메모, 스티커, 그림 낙서
- `OnMemoryDiscovered` 이벤트 발행 → 다락방 일기장에 자동 정리
- 모든 추억을 모으면 **마지막 페이지 (히든 챕터)** 언락

## 7. 성능 목표

- **iPhone 12 이상**에서 **60fps 유지**
- **동시 적 500마리** 이상 가능 (오브젝트 풀 필수)
- **메모리** 200MB 이하
- **앱 용량** 100MB 이하

## 8. 작업 우선순위

> Claude Code는 충돌 시 다음 순서로 결정한다:

1. **성능** (오브젝트 풀, GC 최소화)
2. **일관된 톤** (귀여움, 비폭력, 따뜻한 정서)
3. **UI/UX** (한 손 플레이, 큰 터치 타겟)
4. **코드 유지보수성** (SO + Event 분리)
5. 새 기능

## 9. 작업 요청 가이드 (사용자 → Claude)

- ✅ 한 번에 한 시스템만 요청 ("장비 + 적 동시 ❌")
- ✅ 데이터 우선: ScriptableObject 정의 → 매니저 → UI 순서
- ✅ 새 클래스는 폴더 위치 명시
- ✅ 기존 EventBus 이벤트 재사용 우선, 신규 이벤트 추가 시 본 문서 업데이트
- ❌ 한 함수에 여러 책임 (단일 책임 원칙 준수)
- ❌ MonoBehaviour 직접 인스턴스화

## 10. 외부 라이브러리

| 용도 | 라이브러리 | 라이선스 |
|---|---|---|
| 애니메이션 | DOTween (무료) | 커스텀 |
| 입력 | Unity Input System (공식) | Unity |
| 카메라 | Cinemachine 3.1.6 (공식, 활성) | Unity |
| UI | uGUI (공식) | Unity |
| 사운드 | Unity Audio (필요시 FMOD) | Unity |

신규 라이브러리 도입 시 본 문서에 반드시 기록.

## 11. 마일스톤

| Phase | 기간 | 핵심 산출물 | 상태 |
|---|---|---|---|
| 0 환경 | 1주 | Unity + Xcode + iOS 빌드 성공 | ✅ 완료 (2026-04-29) |
| 1 프로토타입 | 3주 | 다락방 → 1챕터 3웨이브 + 상점 1회 동작 | ✅ 완료 (2026-04-30) |
| 2 확장 | 4주 | 1챕터 20웨이브 완성 + 장비 12종 + 적 6종 | ✅ 완료 (2026-04-30) |
| 3 콘텐츠 | 4주 | 챕터 3권 + 캐릭터 3명 + 메타 진행 + 추억 시스템 | 🔄 진행 중 (메타 진행 골격 완료) |
| 4 출시 | 3주 | TestFlight + App Store 심사 | ⏳ 대기 |

## 12. Phase 1+2 구현 완료 목록

> Claude Code와 함께 작성한 스크립트/에셋 (2026-04-30 기준)

### Core
- `GameEvents.cs` — 정적 EventBus, 14개 이벤트 (A7: OnMagicCrystalChanged ◇·OnStarlightChanged ⋆ 추가; **C1: OnAffixRolled(EquipmentInstance) 추가** — AffixRoller 가 RerollMain/RerollSub 후 발행)
- `GameManager.cs` — 싱글턴, GameState, CurrentWave, StarDust + 사망/챕터클리어 핸들러
- `ObjectPool.cs` — 제네릭 `ObjectPool<T>`
- `SaveSystem.cs` — JSON 영구 저장 (무지개 구슬 등)
  - A6 SaveData 확장: `magicCrystals` ◇, `starlight` ⋆, `level=1`, `xp`, `firstClearedChapters` (35비트마스크). `permanentlyLockedAffixes`·`mythicMileageBonus`는 B2와 함께 도입 예정
  - C6 SaveData Endgame 필드: `lastDailyBossClearTime`/`lastWeeklyBossClearTime`(Unix 초) + `infiniteModeMaxDepth` + `hardcoreChaptersCleared[15]`(CH 36~50 비트)
- `CurrencyService.cs` — A5 4 화폐 정적 facade. Grant/Spend 통합 + 챕터/일일/추억 보상 계산식 (`CalcChapterStarFragmentReward` 등)
- `ConversionForge.cs` — C5 변환 모루 정적 서비스. `ConvertDustToCrystal`(1,000 ★ : 1 ◇) / `ConvertOrbToCrystal`(5 ✦ : 1 ◇) + 시뮬용 `CalcDustCost/CalcOrbCost`. 다락방 메뉴에서만 호출. 역방향 미지원 (마법 결정은 영구 가치 자원).
- `EndgameResetService.cs` — C6 Endgame 일일/주간 가용성 + 무한·고난도 진행 기록. KST 0시 / 월요일 0시 기준. `IsDailyAvailable/IsWeeklyAvailable` + `MarkDailyCleared/MarkWeeklyCleared` + `GetSecondsUntilNextDailyReset/Weekly` + `RecordInfiniteDepth/GetInfiniteMaxDepth` + `IsHardcoreChapterCleared(N)/MarkHardcoreChapterCleared(N)`. 시간 헬퍼는 순수 함수, 상태는 SaveData 직접 R/W (MetaProgression.Instance.Data).
- `StarlightGrantService.cs` — E3 별빛 ⋆ 무료 출처 자동 적립. 일일 로그인 5⋆(KST 0시) / 챕터 첫 클리어 N×2⋆ / 추억 발견 10⋆ / 챕터 도감 100% 50⋆ / 일일 보스 5⋆ / 주간 보스 20⋆. `Bootstrap()` 으로 GameEvents 구독 + 일일 로그인 자동 시도. SaveData 비트로 1회성 보장. F4·F5 보스 처치 매니저는 `GrantDailyBossFirstClear/Weekly` 호출.
- `IAPService.cs` — E1+E2 Unity IAP 통합 (`com.unity.purchasing` 5.0.0, manifest.json 추가). 5 별빛 패키지 + 1 시즌 패스 등록. `Bootstrap` 시 `UnityPurchasing.Initialize`, 구매 성공 시 `ApplyGrant` 가 별빛 적립 또는 시즌 패스 프리미엄. `OnReady`/`OnPurchaseSucceededEvent`/`OnPurchaseFailedEvent` 이벤트. 영수증 검증은 ReceiptValidator 호출 — 검증 실패해도 grant 진행(MVP 관용; 운영 단계 엄격화).
- `ReceiptValidator.cs` — E5 `CrossPlatformValidator` 래퍼. Tangle 키(`AppleTangle`/`GooglePlayTangle`) 는 Window > Services > IAP 에서 생성. Editor·키 미생성 시 검증 스킵(true). 위·변조 감지 시 false.
- `SeasonPassService.cs` — E4 시즌 패스 (출시 3개월차 도입). 4 단계 점수 임계값(100/250/500/1000), free·premium 보상 분리, `AddPoints/TryClaimFree/TryClaimPremium/RotateSeason` API. 코스메틱·칭호는 후속.
- 카메라 추적: Cinemachine 3.x `CinemachineCamera` + `CinemachineFollow` (Chapter 씬에서 Player를 추적)

### Data (ScriptableObject)
- 클래스: `EquipmentData`, `EnemyData`, `ChapterData`, `CharacterData`, `MemoryData`, `BalanceConfig`, `ShopPool`, `AffixDefinition`(B3), `SubAffixDefinition`(B4)
- B3+B4 SO 인스턴스: Affix 22종(`Assets/_Project/Data/Affixes/`) + SubAffix 12종(`Assets/_Project/Data/SubAffixes/`). `Editor/AffixSeed.cs` 메뉴(`Tools > LittleHero > Seed Affix SOs`)로 멱등 일괄 생성. 설계 문서 §7.4의 "23종"은 §2 표·§3 슬롯 풀 매핑 unique 합과 다름 — 실제 unique 22로 정확
- B5+B6 SO 인스턴스: SlotAffixPool 12종(`Assets/_Project/Data/SlotPools/SlotPool_Hat.asset` 등) + GlobalSubAffixPool 1종(`Assets/_Project/Data/GlobalPools/GlobalSubAffixPool_Default.asset`). 신규 `EquipSlot` enum 12개(주 7 + 장신구 5). `Editor/PoolSeed.cs` 메뉴(`Tools > LittleHero > Seed Pool SOs`)
- B1+B2 EquipmentTemplate/Instance 인프라: `EquipmentTemplate` SO(12슬롯 EquipSlot, 25강 + 6 affix 라인) + `EquipmentInstance`/`AffixLine`/`InstanceHistory` 직렬화 + SaveData에 `equipmentInstances`/`permanentlyLockedInstanceIds`/`mythicMileageBonus`(MythicMileageEntry) 추가. `Editor/EquipmentTemplateSeed.cs` 메뉴(`Tools > LittleHero > Migrate Equipment Templates`)로 기존 EquipmentData 16종을 신규 12슬롯 SO로 마이그레이션 (§4.4 표). **기존 EquipmentData 16종은 그대로 유지** — C8 IAttackPattern 갱신 시점까지 레거시·신규 병존
- `BalanceConfig` (B9) 확장 — `enhanceCostByLevel[25]` (별가루 ★, economy §4.1) + `enhanceCrystalByLevel[25]` (마법 결정 ◇, +16부터, §7.3) + `rerollCostMain=30 / rerollCostSub=15 / lockMultiplier=0.5` + 무지개 구슬 상점 5/12/30/80/`subAffixUpgradeCost=10` + 드랍 곡선 베이스. `Editor/BalanceConfigSeed.cs` 메뉴(`Tools > LittleHero > Seed BalanceConfig Curves`)
- `EnemyData`에 tintColor / scaleMultiplier / armor (A3, 기본 0) 필드 추가
- `BalanceConfig`에 priceByRarity 추가
- `CharacterData`에 4 코어 비중(`strengthMultiplier`/`agilityMultiplier`/`wisdomMultiplier`/`spiritMultiplier`) 추가 (stats_v1 §3.3)
- `ChapterData`에 `chapterNumber`(A2, 기본 0) + `enemyStatMultiplier`(A4, 기본 1f, 보통 `1.10^(N-1)`) 필드 추가. B7가 1~35 채움
- 인스턴스: 적 7종 (Basic/Rabbit/Bear/Firefly/Pumpkin/StarCloud/BigWolf), 장비 16종, BalanceConfig, Chapter_01_RedHood (totalWaves=20), ShopPool_Default
- B7 V2 ChapterData 35종 SO (`Assets/_Project/Data/Chapters/V2/CH_01~35_*.asset`) — chapter_structure_v2 §3 35 챕터 displayName + chapterNumber + enemyStatMultiplier=1.10^(N-1) + starFragmentReward=round(5×1.10^(N-1)). 적/보스 풀은 F1+ 콘텐츠 작업에서 채움. 기존 Chapter_01_RedHood 등 3종은 폐기 동화 모델로 보존
- B8 EndgameModeData 4종 SO (`Assets/_Project/Data/Endgame/`) — daily_boss / weekly_boss / infinite_tower / hardcore_chapter. ModeType / CurrencyType / RewardEntry. 보상은 endgame §3.4·§4.4·§5.5·§6.4 표 그대로. `Editor/EndgameModeSeed.cs`. 보스 풀은 F4·F5에서 채움
- B10 MythicDefinition 4종 SO (`Assets/_Project/Data/Mythics/`) — 아빠의 나침반/자장가/용기/약속 (equipment §3.7). `Editor/MythicSeed.cs`. 발현 효과 적용 로직은 C4 EffectResolver / C7 MythicMileageService에서

### Gameplay
- **Player**: `PlayerController`, `PlayerStats`, `PlayerHealth`, `PlayerDamageReceiver`, `PlayerProgress`(A2)
  - A1 PlayerStats 확장: 4 코어 스탯(힘/빠름/슬기/마음, raw + rounded) + 11 신규 파생(CritChance/CritDamage/Armor/DamageReduction/DustGainMultiplier/DropRateMultiplier/FinalDamageBonus/ExtraAttackTargets/ArmorPenetration/CompanionDurationBonus/AdditiveDamageBonus) + 코어 100점당 효과(stats_v1 §3.2). `SetLevel(int)` 절대 동기화 API + `ApplyLevelUp()` 래퍼.
  - A2 PlayerProgress: 싱글턴(RuntimeInitializeOnLoadMethod 자동 부트스트랩), L100 piecewise XP 곡선(L1~50 1.15^L, L51~100 1.05^(L-50)), XP 적립 핸들러(적/챕터/추억) + 시뮬 §5.1 베이스(2/20/80/N×40/40), L51+ 둔화(HP+5·공격력+0·부수 0), 사망 시 XP 후퇴 없음, SaveData.level/xp 영구
- **Enemies**: `EnemyController` (외형 + A4 챕터 곱셈 캐시: MaxHealth/AttackDamage + **Phase G2: data.icon 가 있으면 SpriteRenderer.sprite 동적 변경 + scale 0.55(P11)**), `EnemyAI`, `EnemySpawner` (A4+C9: ActiveChapter→SelectedChapter→1f 우선순위로 enemyStatMultiplier 자동 적용. ChapterRunner 가 Start 에서 `SetActiveChapter` 호출하여 인스펙터 직접 챕터에서도 곱셈 작동)
- **Waves**: `WaveManager` (보스 플레이어 기준 spawn), `ChapterRunner`, `InfiniteTowerRunner`(F6)
  - F6 InfiniteTowerRunner: 무제한 깊이, 깊이↑마다 적 스탯 곱셈 누적(`baseStatMultiplier × depthScaling^(depth-1)`, 기본 25.55 × 1.05^d). 미니보스/보스 주기(5/10층). 가상 ChapterData 를 매 깊이 spawner.SetActiveChapter 로 주입. 사망 시 EndgameResetService.RecordInfiniteDepth 영구 저장.
- **Equipment**: `IAttackPattern`(C8 갱신), 7종 패턴 (`ProjectileAttack`, `AuraAttack`, `OrbitAttack`, `RainAttack`, `TrailAttack`, `PulseAttack`, `SummonAttack`), `AttackPatternUtil`(C8), `OrbitOrb`, `TrailField`, `PulseVisual`, `EquipmentManager`, `TestEquip`, `AffixRoller`(C1), `EnhancementService`(C3), `EffectResolver`(C4), `MythicMileageService`(C7)
  - C7 MythicMileageService: 정적 신화 옵션 발현. `TryRoll(instance, pool, rng)` → 마일리지 +1, p = `0.005 + effective/200`(클램프 1.0), 통과 시 슬롯 매칭 신화 1종 추출 → `instance.mythicId` 설정 + 마일리지 0 리셋. 발현 보장 `ForceRoll`(별빛 ⋆ 300, 마일리지 50+ 한정). `IsUnlocked()` = `MetaProgression.AllMemoriesDiscovered()` — 추억 4종 미수집 시 false. effectiveMileage = `instance.history.mileage + lookup(SaveData.mythicMileageBonus[instanceId])`. `AddMileageBonus(instanceId, amount)` — 별빛 ⋆ 100/+20 monetization §4.4 누적용. C2 ItemDropService.Initialize 가 mythics 인자를 받아 굴림마다 호출.
  - C8 IAttackPattern 갱신: `Initialize` 시그니처에 `PlayerStats stats` 추가. 7개 패턴 모두 `_stats` 보관 + `AttackPatternUtil.EffectiveInterval(baseInterval, stats)`(공격 속도) / `EffectiveCount(baseCount, stats)`(추가 타겟) 사용. EquipmentManager 가 `_stats` 주입. AttackSpeed=1, ExtraAttackTargets=0 기본값에서 기존 동작 보존.
  - C1 AffixRoller: 정적 서비스. `RollMain`(슬롯 풀 6:3:1) + `RollSub`(글로벌 풀 globalWeight 합산) + `RollMainSet/RollSubSet` 일괄 + `RerollMain/RerollSub`(잠금 안 한 라인만 갱신, 끝나면 OnAffixRolled 발행) + `CalcRerollCostMain/Sub`(BalanceConfig.rerollCostMain/Sub × (1 + lockedCount × lockMultiplier)). RNG 주입형(System.Random) — 테스트성 ↑.
  - C4 EffectResolver: `Resolve(equipped, templates, mythics)` → `ResolvedEquipmentBonus`(코어 ADD 4 + 전투 ADD/MUL 분리 + 메이플 채널 + 메타·드랍 + ActiveMythicIds 리스트). 각 인스턴스의 baseStats(StatModifiers) → 주옵션 3 + 부옵션 3 → 신화 ID 수집. AffixLine.targetName 을 StatTarget enum 으로 파싱 후 채널 가산. AllStats 는 4 코어 동시. 세트 효과 단계는 생략(폐기). 신화 옵션 발동(HP<30% 등 조건)은 게임플레이 측 책임 — 본 결과의 ActiveMythicIds 만 노출. PlayerStats 와의 통합 결합은 후속(D5/D8 카드 UI 작업과 함께).
  - C3 EnhancementService: 정적 강화 +0~+25. `TryEnhance(instance, config, rng)` → `EnhanceResult{outcome, newLevel, dustCost, crystalCost, wasPityTriggered, rolledChance}`. 성공률 = `+0~+15: 1.0` / `+16~+25: 0.50 × 0.85^(L-16) + softPity (failCount≥5: +0.05/회)` / `failCount≥20: 1.0`(하드 천장). 비용은 시도 시점에 차감(실패해도 환불 없음, dust↗crystal 순 pre-check). 상수: `MAX_ENHANCE_LEVEL=25 / DETERMINISTIC_LEVEL_CAP=16 / HARD_PITY_FAILS=20`. UI(D6)는 `CalcSuccessRate(L, fc)` 로 표기.
- **Combat**: `DamageSystem`(F8 신화 곱셈 통합), `Projectile`, `DamageCalculator`(A3: 3 버킷 + 방어 관통, 평균/RNG 두 모드), `MythicEffectRuntime`(F8), `SimpleFx`(Phase G2)
  - Phase G2 SimpleFx: 단순 sprite + 0.6s fade-out + scale 0.3→0.8 + 회전. 적 처치 시 EnemyController.Die 가 `Resources/VFX/StarBurstFx.prefab` instantiate. CLAUDE.md §2 별 흩어짐 절대 원칙 첫 구현. 후속 별·하트·꽃잎·비눗방울 4종 분리·확장.
  - F8 MythicEffectRuntime: 4 신화 효과 런타임 곱셈. **mythic_dad_compass** 보스 ×1.20 / **mythic_dad_lullaby** HP<30% 시 받는 데미지 ×0.50 / **mythic_dad_courage** 적 HP<15% 일 때 ×1.50 / **mythic_dad_promise** Trail ×2 + 무료 리롤 1회. `HasMythic(id)` 가 SaveData.equipmentInstances 의 isEquipped+mythicId 인스턴스 검사. DamageSystem.DealDamage 가 자동 호출.

### Meta / Hub
- `Meta/ShopController.cs` — 6칸 롤/구매/새로고침
- `Meta/MetaProgression.cs` — 무지개 구슬 영구 저장 (A5: `MagicCrystals` ◇·`Starlight` ⋆ 보관·Add/Spend 메서드 + 이벤트 발행)
- `Hub/AtticController.cs` — 다락방 허브

### UI
- `HUDController` — 체력바·웨이브·별가루 (OnShopClosed 구독). **D1**: XP 바·레벨 텍스트 + 레벨업 0.8s 색상 강조 연출 추가
- `ShopPanel`, `ShopSlotView` — 상점 UI
- `GameOverPanel` — 사망 시 다락방 복귀
- `ChapterClearedPanel` — 챕터 클리어 + 무지개 구슬 보상 + 다락방 복귀
- **D2 AtticController 갱신** — 4 화폐 표시(★/✦/◇/⋆) + Endgame 메뉴 placeholder(일일/주간/무한 cooldown 텍스트). 추억 4종 발견 시 endgame root 활성
- **D7 ConversionForgePanel** — 변환 모루 UI(★/✦ → ◇), x1·x5 버튼 + 잔액 부족 시 disabled
- **D8 CharacterInfoPanel** — 4 코어(힘/빠름/슬기/마음) + 11 파생 스탯 + 레벨/XP. PlayerStats 우선, 미존재 시 캐릭터 SO+level 폴백
- **D6 EnhanceModalPanel** — +N→+N+1 비용(★/◇) + 성공률 + 천장 게이지(failCount/20). EnhancementService.TryEnhance 결과 텍스트(성공/실패/천장 발동/잔액 부족)
- **D5 EquipmentCardPanel + EquipmentCardRow** — 6단 풀스크린(정체성/베이스/강화 진입/주옵션 보라/부옵션 청록/신화). 라인별 잠금 토글 + 리롤 버튼(주 ★N / 부 ★N). EnhanceModal 연결.
- **D4 InventoryPanel + InventorySlotView** — 12 슬롯 그리드. 주 7 항상 해방 / 장신구 5 = 챕터 6~10 첫 클리어 비트 매핑(귀걸이=6/안경=7/목걸이=8/반지=9/펫=10). 슬롯 클릭 → EquipmentCardPanel.Show
- **D3 WondrousMapPanel + WondrousMapNode** — 35 챕터 선형 지도. CH N 해방 = CH N-1 첫 클리어. 진입 모달(이름/설명/보상=✦+◇+⋆ 첫클리어). 권장 레벨 미달 시 D12 경고 → 사용자 확인 후 진입
- **D10 EndgameMenuPanel** — 일일/주간 보스 cooldown + 무한 다락방 최고 깊이 + 고난도 챕터 진입 4 버튼. EndgameResetService 가용성 → 버튼 interactable. 보스 처치는 placeholder (F4·F5)
- **D11 MythicEffectOverlay** — 신화 발현 무지개 fade-in/hold/out (0.4s/1.4s/0.6s). MythicMileageService.TryRoll 성공 시 호출. 비폭력 톤 — 무지개 휘감음만
- **D12 RecommendedLevelWarningModal** — "권장 레벨 X, 현재 Y. 그래도 도전?" — 도전/취소 2 버튼. WondrousMapPanel 이 OnConfirm 콜백으로 진입 처리

### 씬
- `Scenes/Attic.unity` — 다락방 허브 (무지개 구슬 표시, 3챕터 선택 버튼)
- `Scenes/Chapter.unity` — 챕터 플레이 (Canvas / ShopPanel / GameOverPanel / ChapterClearedPanel). `MetaProgression.SelectedChapter`가 설정되어 있으면 그것을 사용, 아니면 ChapterRunner 인스펙터의 _chapterData 사용

### 챕터 SO
- `Chapter_01_RedHood` — 빨간 모자 (boss: 큰 늑대 아저씨, miniBoss: 밤하늘 별구름)
- `Chapter_02_Beanstalk` — 잭과 콩나무 (boss: 구름 위 큰 거인, miniBoss: 콩알 거인)
- `Chapter_03_Alice` — 이상한 나라의 앨리스 (boss: 하트 여왕, miniBoss: 카드 병정)

### 캐릭터 SO
- `Char_DefaultBoy` — 용감한 소년 (HP 100, Speed 5, Dmg 10, free)
- `Char_BraveGirl` — 씩씩한 소녀 (HP 130, Speed 4.5, Dmg 9, 10 무지개 구슬)
- `Char_QuietFriend` — 조용한 친구 (HP 90, Speed 6, Dmg 11, AtkSpd 1.1, 25 무지개 구슬)
- 캐릭터 선택 흐름: `MetaProgression.SelectCharacter` (Save 자동) → `PlayerStats.Initialize`가 BalanceConfig 대신 캐릭터 스탯 사용 → `PlayerController`가 캐릭터 tint 적용 + **Phase G2: portrait 가 있으면 SpriteRenderer.sprite 동적 변경 + scale 0.6**

### 추억 SO
- `Memory_DadPhoto` (Photo, ch_01_redhood)
- `Memory_DadMemo` (Memo, ch_02_beanstalk)
- `Memory_DadSticker` (Sticker, ch_03_alice)
- `Memory_DadDoodle` (Doodle, ch_03_alice, 낮은 spawn 확률 — 마지막 페이지 단서)
- `MemoryPool_Default` SO가 위 4종을 묶어 보관, MetaProgression이 참조
- 추억 흐름: 챕터 진행 중 `MemorySpawner`가 OnWaveCleared마다 챕터 ID 매칭 + 미발견 + spawnChance 통과 시 플레이어 주변에 별 픽업 spawn → 플레이어 트리거 시 `RaiseMemoryDiscovered` → MetaProgression이 영구 저장 → 다락방 `DiaryPanel`이 발견/미발견 그리드로 표시
- C2 ItemDropService — 정적 드랍 결정 트리(`Gameplay/Items/ItemDropService.cs`). DropContext(source/chapterNumber/forcedSlot/forcedRarity) → 1.확률(Enemy 0.5%/Mini 8%/Boss 100%/BossRepeat 30%/Shop 100%) → 2.슬롯(CH≤10 주 7, CH11+ 12 균등 / forcedSlot 우선) → 3.등급(affix_pool §5 챕터 묶음 분포 + Mini/Boss +1 가중) → 4.템플릿 매칭(slot+rarity 정확→슬롯 폴백) → 5.AffixRoller로 6줄 굴림 → 6.history.mileage += 1(C7 발현은 보류). DropSource enum 5종. `Initialize(slotPools, globalPool, templates, config)` 부트스트랩 필요.

### 메타 업그레이드 SO
- `Meta_HpBoost` — 체력 강화 (+10/Lv, max 5, baseCost 5, costPerLevel 3)
- `Meta_DamageBoost` — 공격력 강화 (+1/Lv, max 5, baseCost 6, costPerLevel 3)
- `Meta_DustGain` — 별가루 행운 (+10%/Lv, max 5, baseCost 5, costPerLevel 4)
- `UpgradePool_Default` SO가 위 3종을 묶어 보관, MetaProgression이 참조
- 흐름: Attic의 `MetaUpgradePanel`에서 강화 버튼 클릭 → `MetaProgression.TryUpgrade` (무지개 구슬 차감 + level 영구 저장) → `PlayerStats.RecalculateStats`가 base 스탯에 메타 보너스 가산, `GameManager.HandleEnemyKilled`가 별가루 드롭에 (1+dustMult) 곱셈

### 도감 (적)
- `EnemyPool_Default` — 11종 적 마스터 풀 (정규 5 + 미니보스/보스 6)
- `MetaProgression`이 `OnEnemyKilled` 구독 → `SaveData.defeatedEnemyIds`에 영구 추가, `OnEnemyDefeated` 이벤트 발행
- `BestiaryPanel`(다락방) — 6열 그리드, 발견 시 색·이름·HP/공격 표시, 미발견 시 회색 "???". `Enemy_Basic.id`는 빈 문자열에서 `basic_slime`으로 보강(저장 키 충돌 방지).

### 에셋
- `Prefabs/UI/ShopSlot.prefab`
- `Art/Generated/Icon_Equip_*.png` × 16 — 장비 아이콘 자동 생성
- `Art/Generated/Sprite_Circle.png`, `Sprite_GroundTile.png`

### 검증 인프라
- `Core/SmokeTest.cs` — Play 모드 1·5·15·30s 자동 스크린샷 + 상태 덤프 (Chapter 씬에 부착, 기본 비활성, `_enabledOnStart=true`로 토글)
- `Editor/ProjectValidator.cs` — 메뉴 `Tools > LittleHero > Validate Project`. Build Settings, BalanceConfig, ShopPool, ChapterData, Camera Lens(Cinemachine 포함), 패널 와이어링 등 일괄 점검

### 중요 구현 주의사항
- UI 텍스트: `UnityEngine.UI.Text` (Legacy) — TMP는 URP 셰이더 깨짐
- Rigidbody2D: `linearVelocity` (Unity 6에서 `velocity` deprecated)
- EquipmentManager는 `OnEquipmentChanged` 자기 구독 금지 (무한루프)
- `GameManager.CloseShop()`: `StartWave()` 먼저, `RaiseShopClosed()` 나중에
- HUD 웨이브 라벨은 `OnShopClosed` 구독 (OnWaveCleared는 CurrentWave 증가 전 발행)
- 보스/미니보스는 player 기준 4f 반경 spawn (spawner 기준이면 카메라 밖 가능)
- 모든 공격 패턴 Cleanup은 풀 루트 GameObject Destroy로 잔상 방지
- ShopPool/ChapterData 등 SO 인스턴스는 `execute_code`로 생성 (manage_asset이 LittleHero.* 타입 미지원)

## 13. Phase 3 다음 작업 목록

1. ~~**챕터 2/3** — 잭과 콩나무, 이상한 나라의 앨리스~~ ✅ 완료 (2026-04-30)
2. ~~**캐릭터 시스템** — `CharacterData` 활용, 캐릭터 선택, 시작 스탯 차별화~~ ✅ 완료 (2026-04-30)
3. ~~**추억 시스템** — `MemoryPickup` (월드 픽업) + 다락방 일기장 UI~~ ✅ 부분 완료 (2026-04-30, 히든 챕터 unlock UI는 추후)
4. ~~**메타 업그레이드** — 무지개 구슬으로 영구 스탯 강화 (HP, 데미지, 별가루 획득량)~~ ✅ 완료 (2026-04-30)
5. ~~**컬렉션/도감** — 처치한 적 자동 등록~~ ✅ 부분 완료 (2026-04-30, 100% 채우면 펫 동행 기능은 추후)
6. ~~**사운드** — Unity Audio 효과음 + BGM~~ ✅ 부분 완료 (2026-05-01, P9 절차적 SFX 5종. BGM·보스 fanfare 후속)
7. **아트** — 단색 더미 → 실 스프라이트 교체 ✅ 부분 완료 (Phase G2 P0~P10 — 138 PNG + walk 코드 애니메이션)

## 14. 변경 이력

- 2026-04-29: 초기 작성 (다락방 동화책)
- 2026-04-29: **프로젝트명 확정 — Little Hero by the Lake**, 호숫가 학교 + 아빠 동화책 스토리 추가
- 2026-04-30: Phase 0/1 완료. 26개 스크립트 작성, Chapter.unity 씬 동작 확인
- 2026-04-30: **Phase 2 완료** — 상점 UI, 7종 공격 패턴, 적 7종, 장비 16종, totalWaves=20 + 보스, GameOver/ChapterCleared 패널, CameraFollow, 풀 cleanup 강화. 메타 진행 골격(MetaProgression + Attic 허브) 추가.
- 2026-04-30: 검증 인프라 도입 — `SmokeTest`(자동 스크린샷·상태 덤프) + `ProjectValidator`(에디터 메뉴 일괄 점검).
- 2026-04-30: 챕터 2/3 추가 — 콩알 거인·구름 거인·카드 병정·하트 여왕 SO, `MetaProgression.SelectedChapter`로 챕터 선택 흐름, Attic 3버튼 UI.
- 2026-04-30: 캐릭터 시스템 — CharacterData에 tintColor, MetaProgression.SelectedCharacter(영구 저장), PlayerStats가 캐릭터 스탯 적용, Attic에 3캐릭터 선택 UI, 3종 SO(용감한 소년/씩씩한 소녀/조용한 친구).
- 2026-04-30: 추억 시스템 — MemoryPool SO + 4종 추억 데이터(사진/메모/스티커/낙서), MemoryPickup 프리팹·MemorySpawner(웨이브 클리어 시 챕터 매칭+확률), MetaProgression 발견 영구 저장, 다락방 DiaryPanel(4슬롯 그리드 + n/4 카운트). 히든 챕터 unlock은 미구현.
- 2026-04-30: 메타 업그레이드 — MetaUpgradeData/Pool SO + 3종(체력/공격력/별가루), SaveData.metaUpgrades 영구화, PlayerStats가 base에 메타 보너스 가산, GameManager가 별가루 드롭에 ×(1+bonus), Attic MetaUpgradePanel(3행 + 강화 버튼).
- 2026-04-30: UI 화폐 구분 — 별가루 `★`(런 내) / 무지개 구슬 `✦`(영구) 라벨·아이콘 분리. HUD `별가루 ★ N`, 상점 새로고침 `(별가루 ★ N)`, 마음 단련 강화 `(✦ N)`.
- 2026-04-30: 도감 시스템 — EnemyPool SO + 11종 적 등록, SaveData.defeatedEnemyIds 영구화, MetaProgression이 OnEnemyKilled 자동 추적, 다락방 BestiaryPanel(6열 그리드 + n/N 카운트). Enemy_Basic.id 보강(basic_slime).
- 2026-04-30: 화폐 명칭 일괄 갱신 — `별 조각` → `무지개 구슬` ✦ (영구), 신규 `마법 결정` ◇ (강화 전용, +16 이상). 영문 필드명(`StarFragments` 등)은 유지. 다른 게임 사례 조사 기반 차별화 (메이플 키우기·디아블로 4·로스트 아크 참조).
- 2026-04-30: A1 PlayerStats 확장 — 4 코어 스탯(힘/빠름/슬기/마음) + 11 신규 파생(메이플 키우기 채널 4 + 데미지 버킷 1 + 크리/방어/배수 6) + 코어 100점당 효과 통합 + `ApplyLevelUp()` 메서드 (A2 호출 대기). `CharacterData`에 4 코어 비중 필드 추가. 신규 필드 기본값 0/베이스로 기존 게임플레이 영향 0 (참조: docs/little_hero/stats_v1.md §6).
- 2026-04-30: A6 SaveData 확장 — `magicCrystals` ◇, `starlight` ⋆, `level`(=1), `xp`, `firstClearedChapters`(35비트) 추가. A2/A5/E3가 의존. `permanentlyLockedAffixes`/`mythicMileageBonus`는 B2 EquipmentInstance와 함께 도입 (참조: economy §9.2 + character_progression §7.4 + monetization §7.1).
- 2026-04-30: A7 GameEvents 신규 이벤트 — `OnMagicCrystalChanged(int)` ◇, `OnStarlightChanged(int)` ⋆ 추가. `OnLevelUp(int)`은 기존 시그니처 유지. `OnAffixRolled`는 B2 EquipmentInstance 시그니처 결합으로 B2와 함께 도입. CLAUDE.md §4 EventBus 목록도 갱신.
- 2026-04-30: A5 CurrencyService — 4 화폐(★/✦/◇/⋆) 통합 정적 facade. MetaProgression에 `MagicCrystals`/`Starlight` 보관·Add·Spend·이벤트 발행. 챕터 보상 공식(`CalcChapterStarFragmentReward = 5×1.10^(N-1)` / `CalcChapterMagicCrystalReward = 1+N/3` / `CalcChapterStarlightReward = N×2`) + 일일 로그인 5⋆ + 추억 발견 10⋆.
- 2026-05-01: A2 PlayerProgress — 싱글턴(자동 부트스트랩), L100 piecewise XP 곡선(L1~50 1.15^L / L51~100 1.05^(L-50), L1→L2=50 하드코드), XP 적립(적 2/미니 20/보스 80/챕터 N×40/추억 40 + 챕터 모디파이어 1+N×0.1), 사망 시 XP 후퇴 없음, SaveData.level/xp 영구. PlayerStats에 `SetLevel(int)` 절대 동기화 API + L51+ 둔화(HP+5·공격력+0·부수 0). ChapterData에 `chapterNumber` 필드 추가(B7가 1~35 채움). PlayerController.Start에서 SyncStatsToCurrentLevel 호출.
- 2026-05-01: A3 DamageCalculator — 3 데미지 버킷(합산/최종/크리) × 적 방어력÷(1+effective/100) + 방어 관통. `CalculateExpectedDamage`(평균, UI/시뮬용)와 `RollDamage`(RNG, 런타임 1회 공격, isCrit out) 두 시그니처. EnemyData에 `armor` 필드 추가(기본 0). 호출처 통합은 C8 IAttackPattern 갱신에서.
- 2026-05-01: A4 EnemyController 챕터 곱셈 — ChapterData에 `enemyStatMultiplier`(기본 1f, B7가 `1.10^(N-1)` 채움) 추가. EnemyController.Initialize 시그니처에 `statMultiplier` 옵션 인자, MaxHealth/AttackDamage 캐시 프로퍼티. EnemySpawner가 SelectedChapter.enemyStatMultiplier 자동 적용. PlayerDamageReceiver는 `enemy.AttackDamage` 참조로 변경(기존 `enemy.Data.damage` 직접 참조는 SO 베이스값이라 곱셈 누락). xpDrop 곱셈은 PlayerProgress의 1+N×0.1 모디파이어와 충돌하므로 미도입. **Sprint 1 (Foundation) 완료**.
- 2026-05-01: B3+B4 Affix/SubAffix SO 시드 — AffixDefinition·SubAffixDefinition 클래스 + StatTarget 22종·AffixOp(ADD/MUL) enum + Editor 시드 메뉴 `Tools > LittleHero > Seed Affix SOs`로 22+12 = 34 SO 일괄 생성. 등급별 [min,max] 수치는 affix_pool §2 / item_drop §3.1 표 그대로. 설계 §7.4의 "23종"은 표 합계와 차이 — 실제 unique 22 (문서 오류 확인).
- 2026-05-01: B5+B6 Pool SO 시드 — SlotAffixPool 12종 + GlobalSubAffixPool 1종 (Editor 메뉴 `Tools > LittleHero > Seed Pool SOs`). 신규 `EquipSlot` enum 12개(주 7 Hat~Shoes + 장신구 5 Earring~Pet, 메이플 키우기 매칭). 가중치 6:3:1, 글로벌 부옵션 가중치는 SubAffixDefinition.globalWeight 합 1.0.
- 2026-05-01: B1+B2 EquipmentTemplate/Instance 인프라 — EquipmentTemplate SO 클래스(EquipSlot/Rarity/baseStats/attackPattern/maxEnhanceLevel=25/mainAffixLineCount=3/subAffixLineCount=3). EquipmentInstance/AffixLine/InstanceHistory 직렬화. SaveData에 `equipmentInstances` 배열·`permanentlyLockedInstanceIds`·`mythicMileageBonus`(MythicMileageEntry) 추가(A6에서 보류한 affix 관련 필드 통합). 기존 EquipmentData 16종 → EquipmentTemplate 16종 마이그레이션(§4.4 표 그대로, Editor 메뉴 `Tools > LittleHero > Migrate Equipment Templates`). 레거시 EquipmentData는 보존 — C8 통합 시점까지 병존.
- 2026-05-01: B9 BalanceConfig 확장 — enhanceCostByLevel[25] 별가루 곡선(+0→+25, 5~4000) + enhanceCrystalByLevel[25] 마법 결정(+15→+16부터 3, +24→+25 40, 누적 165) + 리롤 30★ 주/15★ 부 + 잠금 ×1.5 + 무지개 구슬 상점가(Common 5 ~ Epic 80) + 드랍 곡선 베이스. `Editor/BalanceConfigSeed.cs` 메뉴로 기존 BalanceConfig.asset 갱신. AnimationCurve 보상 필드는 B7와 함께 도입 예정.
- 2026-05-01: B7 V2 ChapterData 35종 SO — chapter_structure_v2 §3 displayName + chapterNumber + `enemyStatMultiplier=1.10^(N-1)` + `starFragmentReward=round(5×1.10^(N-1))`. `Editor/ChapterDataSeed.cs` 메뉴(`Tools > LittleHero > Seed 35 Chapter SOs`)로 일괄 시드 (`Assets/_Project/Data/Chapters/V2/`). 기존 동화책 3개(빨간모자/잭과콩나무/앨리스)는 폐기 모델로 보존. 적/보스 풀은 F1+ 콘텐츠 작업에서 채움.
- 2026-05-01: B8 EndgameModeData 4종 SO — daily_boss/weekly_boss/infinite_tower/hardcore_chapter (endgame §3·§4·§5·§6). ModeType/CurrencyType/RewardEntry 인프라. `Editor/EndgameModeSeed.cs` 메뉴. 보스 풀(bossPool)은 F4·F5 콘텐츠.
- 2026-05-01: B10 MythicDefinition 4종 SO — 아빠의 나침반/자장가/용기/약속 (equipment §3.7). 발현 로직은 C4 EffectResolver / C7 MythicMileageService에서. **Phase B (B1~B10) 10/10 완료**.
- 2026-05-01: C1 AffixRoller — 정적 굴림·리롤 서비스 (`Gameplay/Equipment/AffixRoller.cs`). `RollMain`(SlotAffixPool 가중 6:3:1) + `RollSub`(GlobalSubAffixPool globalWeight 합산) + `RollMainSet/Set`(일괄) + `RerollMain/Sub`(잠금 안 한 라인만 갱신, 끝나면 OnAffixRolled 발행) + `CalcRerollCostMain/Sub`(BalanceConfig.rerollCostMain/Sub × (1 + lockedCount × lockMultiplier)). RNG는 System.Random 주입형(테스트성). GameEvents에 `OnAffixRolled(EquipmentInstance)` 신규 추가 (A7 보류분 해소). 참조: affix_pool §4·§7.3 + item_drop §5.4·§6.
- 2026-05-01: C5 ConversionForge — 변환 모루 정적 서비스(`Core/ConversionForge.cs`). `ConvertDustToCrystal(N)` 1,000★:1◇ / `ConvertOrbToCrystal(N)` 5✦:1◇ + `CalcDustCost/CalcOrbCost` 시뮬 헬퍼. GameManager.SpendStarDust 와 CurrencyService.SpendStarFragment 를 거쳐 차감, CurrencyService.GrantMagicCrystal 로 적립. 역방향 미지원. UI(D7)는 후속. 참조: economy §11.4.
- 2026-05-01: C7 MythicMileageService — 신화 옵션 발현 정적 서비스(`Gameplay/Equipment/MythicMileageService.cs`). `TryRoll(instance, pool, rng)` → 마일리지 +1, p = `BASE_RATE(0.005) + effectiveMileage / GUARANTEE_THRESHOLD(200)`(Clamp01), 통과 시 슬롯 매칭 신화 풀에서 1종 → `instance.mythicId` 설정 + 마일리지 0 리셋. `ForceRoll`(별빛 ⋆ 300 monetization §4.4 — 마일리지 50+ 한정). `IsUnlocked()` = `MetaProgression.AllMemoriesDiscovered()` — 추억 4종 모두 발견 후만 발현. `AddMileageBonus(instanceId, +20)` — 별빛 ⋆ 100 monetization §4.4 누적, SaveData.mythicMileageBonus 갱신. C2 ItemDropService 가 `Initialize(...)` 의 옵션 mythics 배열을 받아 드랍마다 굴림 1회 시도(추억 잠금 시 미동작). 슬롯 매칭은 슬롯 제한 없는 신화 + (template lookup 추가 후속). 참조: equipment §3.7 + monetization §4.4·§7.1.
- 2026-05-01: C6 EndgameResetService — 일일/주간 가용성 + 무한·고난도 진행 기록 정적 서비스(`Core/EndgameResetService.cs`). KST(UTC+9) 기준 일일 0시·주간 월요일 0시 리셋. `IsDailyAvailable/IsWeeklyAvailable` + `MarkDailyCleared/Weekly` + `GetSecondsUntilNextDailyReset/Weekly` + `RecordInfiniteDepth(int)`(최고만 갱신)·`GetInfiniteMaxDepth` + `IsHardcoreChapterCleared(N)/MarkHardcoreChapterCleared(N)`. SaveData 확장: `lastDailyBossClearTime`/`lastWeeklyBossClearTime`(long, Unix 초) + `infiniteModeMaxDepth`(int) + `hardcoreChaptersCleared[15]`(bool[], CH 36~50). 보스 처치·보상은 F4·F5 콘텐츠 작업에서. 참조: endgame §8.2·§8.3.
- 2026-05-01: C8 IAttackPattern 갱신 — `Initialize(config, owner, PlayerStats stats)` 로 시그니처 확장. 7개 패턴(ProjectileAttack/AuraAttack/OrbitAttack/RainAttack/TrailAttack/PulseAttack/SummonAttack) 모두 `_stats` 보관. 새 헬퍼 `AttackPatternUtil.EffectiveInterval(base, stats) = base / max(0.1, AttackSpeed)` / `EffectiveCount(base, stats) = max(1, base) + ExtraAttackTargets`. `EquipmentManager.ActivatePattern` 가 `_stats` 주입. `AttackSpeed=1`, `ExtraAttackTargets=0` 기본값에서 기존 동작 보존. Orbit 의 orb 개수는 init 시점 1회 반영(런타임 추가 안 함). 참조: stats_v1 §6 + ui_v1 §9.
- 2026-05-01: C9 EnemySpawner 챕터 곱셈 명시적 통합 — `EnemySpawner` 에 `ActiveChapter` 프로퍼티·`SetActiveChapter(ChapterData)` API 추가. `Spawn` 우선순위: ActiveChapter → SelectedChapter → 1f. `ChapterRunner.Start` 에서 `_spawner.SetActiveChapter(_chapterData)` 호출 — 인스펙터 직접 지정 챕터에서도 enemyStatMultiplier 가 정상 작동(A4 보강). 참조: chapter_curve §9.1.
- 2026-05-01: C4 EffectResolver — 장비·affix·신화 통합 stats 해석 정적 서비스(`Gameplay/Equipment/EffectResolver.cs`). `Resolve(equipped, templates, mythics)` → `ResolvedEquipmentBonus`(코어 ADD 4 + ADD/MUL 분리 전투 채널 + 메이플 채널(FinalDamage/ExtraTargets/ArmorPen 등) + 메타(Xp/Dust/Drop) + ActiveMythicIds 리스트). 각 인스턴스: baseStats → mainAffixes 3 → subAffixes 3 → mythicId 수집. AffixLine.targetName → StatTarget enum 파싱 후 채널 가산(AllStats 는 4 코어 동시). 세트 효과 단계 생략(폐기). 신화 효과는 대부분 조건부(HP<30%, 보스 등)이므로 ActiveMythicIds 만 노출 — 발동 책임은 DamageSystem/PlayerHealth 등 게임플레이 측. PlayerStats 통합 결합은 후속. 참조: equipment §4.3. **Phase C (C1~C9) 9/9 완료**.
- 2026-05-01: D1~D12 Phase D UI 패널 일괄 — D9(별빛 상점, E1 Unity IAP 의존)만 보류, 나머지 11/12 완료.
  - **D1** HUDController 갱신: XP 바 + 레벨 텍스트 + 레벨업 0.8s 색상 강조 fx (Coroutine, OnLevelUp 구독).
  - **D2** AtticController 갱신: 4 화폐 표시(★/✦/◇/⋆) + Endgame 메뉴 placeholder(일일/주간/무한 cooldown). OnMagicCrystalChanged·OnStarlightChanged 구독. CH35 클리어 검증은 firstClearedChapters int 32bit 한계로 임시 AllMemoriesDiscovered 대용 (long 승격은 후속).
  - **D3** WondrousMapPanel + WondrousMapNode: 35 챕터 선형 지도, CH N 해방 = CH(N-1) 첫 클리어 비트. 진입 모달(이름·설명·보상). 권장 레벨 매트릭스(1~5:lvl1, 6~10:5, 11~15:10, 16~20:18, 21~25:28, 26~30:38, 31~35:48). 미달 시 D12 경고 → 사용자 확인 후 SceneManager.LoadScene("Chapter").
  - **D4** InventoryPanel + InventorySlotView: 12 슬롯 그리드. 주 7 항상 해방, 장신구 5 = 챕터 6~10 비트 매핑(귀걸이=6/안경=7/목걸이=8/반지=9/펫=10, equipment §3.7). 슬롯 클릭 → EquipmentCardPanel.Show. 자물쇠 오버레이.
  - **D5** EquipmentCardPanel + EquipmentCardRow: 6단(정체성+베이스+강화진입+주옵션 보라+부옵션 청록+신화). 라인별 잠금 토글 + 리롤(주 ★N/부 ★N). AffixRoller.RerollMain/Sub 호출, GameManager.SpendStarDust 차감. EnhanceModal 진입.
  - **D6** EnhanceModalPanel: 비용(★/◇) + 성공률(`CalcSuccessRate`) + 천장 게이지(`failCount/20`) + 결과 텍스트(성공/실패/천장 발동/잔액). EnhancementService.TryEnhance 결과 분기.
  - **D7** ConversionForgePanel: ★→◇ x1·x5 + ✦→◇ x1·x5 버튼. 잔액 부족 시 disabled. ConversionForge 호출.
  - **D8** CharacterInfoPanel: 4 코어 + 11 파생 + 레벨/XP. PlayerStats 우선, 다락방에서는 캐릭터 SO + level 시뮬 폴백.
  - **D10** EndgameMenuPanel: 일일/주간 cooldown + 무한 깊이 + 고난도 4 버튼. EndgameResetService 가용성 → interactable. 진입은 F4·F5 placeholder.
  - **D11** MythicEffectOverlay: 무지개 fade-in/hold/out 0.4/1.4/0.6s. WaitForSecondsRealtime — Time.timeScale 영향 없음.
  - **D12** RecommendedLevelWarningModal: WondrousMapPanel 이 OnConfirm 콜백 패스. 도전/취소 2 버튼.
  - 모든 패널은 Legacy `UnityEngine.UI.Text` 사용 (CLAUDE.md §12 규칙). 씬 wiring(Canvas hierarchy + 프리팹 [SerializeField] 연결)은 Unity Editor 작업으로 후속(ProjectValidator 룰 추가 권장).
  - **D9 별빛 상점은 Phase E (Unity IAP) 후 진행** — 현재 별빛 ⋆ 적립 경로(챕터 첫 클리어)는 동작하지만 소비 UI 미구현.
  - **MVP UI 8/8 + Phase 2 UI 3/4 완료. Phase D 11/12.**
- 2026-05-01: **Phase F (F1~F8) 8/8 완료** — 콘텐츠 스캐폴드 + 런타임 hook.
  - **F1+F2+F3 챕터 4~35 적 풀 와이어링**: `Editor/PhaseFContentSeed.cs` 메뉴(`Tools > LittleHero > Seed Phase F Content`)로 5 카테고리(MetaMemory/Nature/Animal/Civilization/Fantasy) × 6 archetype(레귤러 3 + 미니 2 + 보스 1) = 30 generic enemy SO 생성. 35 ChapterData V2 의 enemyPool/miniBossPool/bossEnemy 자동 매핑(카테고리별). archetype 베이스 스탯 1세트만 유지 — 챕터 곱셈은 EnemySpawner(C9) 가 자동 적용. **실 콘텐츠(고유 적 이름·아트·BGM·정밀 밸런싱)는 후속 디자인 패스**.
  - **F4 일일 보스 7종**: 7 enemy SO(`Enemy_DailyBoss_00~06`, 요일 라벨), `daily_boss.asset.bossPool` 와이어링. 요일별 회전 로직은 보스 처치 매니저에서 `DateTime.Now.DayOfWeek` 로 인덱스.
  - **F5 주간 보스 4종**: 4 enemy SO(`Enemy_WeeklyBoss_00~03`, 4주 사이클), `weekly_boss.asset.bossPool` 와이어링. 신화 발현 보장은 보스 처치 매니저가 `MythicMileageService.ForceRoll` 호출 (후속 wiring).
  - **F6 InfiniteTowerRunner**: `Gameplay/Waves/InfiniteTowerRunner.cs` — 무제한 깊이, 가상 ChapterData 동적 생성·spawner 주입. 사망 시 `EndgameResetService.RecordInfiniteDepth` 영구 저장. 별도 Scene 또는 Chapter.unity 의 ChapterRunner 교체로 사용.
  - **F7 고난도 챕터 36~50**: CH 21~35 클론 15종(`CH_36~50_Hardcore_*.asset`), enemyStatMultiplier × 2.0, starFragmentReward × 2. `hardcoreChaptersCleared[15]` SaveData 비트 (C6 도입분)와 연동.
  - **F8 신화 옵션 발현 효과**: `Gameplay/Combat/MythicEffectRuntime.cs` 신설. 4 신화(나침반/자장가/용기/약속) 의 조건부 곱셈을 정적 헬퍼로 노출. **DamageSystem.DealDamage/DealDamageToPlayer** 가 자동 호출 — 베이스 데미지 × `CalcOutgoingDamageMultiplier(target)` / × `CalcIncomingDamageMultiplier(player)`. `mythic_dad_promise` 의 Trail 길이 ×2 와 무료 리롤은 호출자(TrailAttack/EquipmentCardPanel) 별도 호출.
  - **알려진 후속 작업**:
    - F1~F3 placeholder 적 → 디자인 패스에서 챕터별 고유 명·외형·밸런싱
    - F4 보스 처치 매니저(요일 인덱스 → bossPool[idx] spawn)
    - F5 보스 처치 매니저 + 신화 보장 ForceRoll
    - F6 InfiniteTowerScene 분리 (현재는 Chapter.unity 에서 컴포넌트 교체로 사용)
    - F8 mythic_dad_promise 의 무료 리롤 wiring (D5 EquipmentCardPanel)
- 2026-05-01: **Phase E (E1~E5) 5/5 + D9 완료** — 수익화 인프라 일괄.
  - **E1** Unity IAP 패키지 (`com.unity.purchasing` 5.0.0) manifest.json 추가. 패키지 캐시 자동 fetch 확인 (Library/PackageCache).
  - **E2** IAPService — IDetailedStoreListener 구현, 6 productId(STARLIGHT_100/500/1100/3500/12000 + SEASON_PASS) 등록. `Bootstrap`/`InitiatePurchase`/`GetLocalizedPrice`. 구매 완료 시 `ApplyGrant` 가 별빛 ⋆ 적립 or `SeasonPassService.GrantPremium`. 이벤트 시그니처 충돌 회피로 `OnReady`/`OnPurchaseSucceededEvent`/`OnPurchaseFailedEvent` 명명. App Store Connect / Google Play Console 의 productId 등록은 출시 전 후속.
  - **E3** StarlightGrantService — 별빛 무료 출처 자동 적립. 일일 로그인 5⋆ + 챕터 첫 클리어 N×2⋆ + 추억 10⋆ + 도감 100% 50⋆ + 일일/주간 보스. SaveData 비트로 1회성 보장. MetaProgression.Awake 에서 자동 Bootstrap. SaveData 확장: `lastDailyLoginUnix`/`chapterStarlightGranted[35]`/`memoryStarlightGranted[8]`/`bestiaryFullStarlightGranted[35]`.
  - **E4** SeasonPassService — 4 단계(100/250/500/1000 pt) free + premium 보상 비트마스크. `AddPoints/TryClaimFree/TryClaimPremium/RotateSeason/GrantPremium`. SaveData 확장: `seasonId`/`seasonPassPremium`/`seasonPoints`/`seasonRewardClaimedFreeMask`/`seasonRewardClaimedPremiumMask`. UI 패널·코스메틱·시즌 로테이션 트리거는 후속.
  - **E5** ReceiptValidator — Unity IAP `CrossPlatformValidator` 래퍼. AppleTangle/GooglePlayTangle 키는 reflection 으로 우회 로드 (키 미생성 상태에서도 컴파일 OK). Editor 모드·키 미생성·미지원 플랫폼 → 검증 스킵 (true 반환). 운영 단계 권장: App Store Server API 서버 검증으로 보강.
  - **D9 StarlightShopPanel** — E1+E2 의존 해제 후 D9 동시 추가. 별빛 → 마법 결정 변환(5⋆/22⋆/40⋆) + 5 패키지 구매 버튼. IAPService.GetLocalizedPrice 우선, fallback 한국 원화. 강화 보장권/영구 잠금 등은 인스턴스 컨텍스트 필요 — D5 카드 패널에서 직접 호출 (후속 wiring).
  - **출시 전 후속 작업**:
    - App Store Connect/Google Play Console 에 6 productId 등록
    - Window > Services > In-App Purchasing 에서 Unity Gaming Services 활성화 + Tangle 키 생성
    - 시즌 로테이션 cron/scheduler (출시 3개월차 도입 시점)
- 2026-05-01: C3 EnhancementService — 강화 +0~+25 정적 서비스(`Gameplay/Equipment/EnhancementService.cs`). `TryEnhance(instance, config, rng)` → `EnhanceResult{outcome=Success/Failure/AtMaxLevel/InsufficientDust/InsufficientCrystal/InvalidInstance, newLevel, dustCost, crystalCost, wasPityTriggered, rolledChance}`. +0~+15 결정론(1.0). +16~+25 RNG: `0.50 × 0.85^(L-16)` + 소프트 천장 `failCount≥5: +0.05/회` + 하드 천장 `failCount≥20: 1.0`. 비용은 시도 시점에 차감(실패해도 환불 없음, dust↗crystal 순 pre-check). `CalcSuccessRate(L, failCount)` 헬퍼로 UI에 미리보기. 상수: MAX_ENHANCE_LEVEL=25, DETERMINISTIC_LEVEL_CAP=16, HARD_PITY_FAILS=20. 참조: equipment §3.3·§3.4 + economy §4.1·§7.3.
- 2026-05-01: C2 ItemDropService — 정적 드랍 결정 트리(`Gameplay/Items/ItemDropService.cs`). DropContext(source/chapterNumber/forcedSlot/forcedRarity) → ① 확률 굴림(Enemy 0.5% / Mini 8% / Boss 100% / BossRepeat 30% / Shop 100%) → ② 슬롯(CH≤10 주 7, CH11+ 12 균등 / forcedSlot 우선) → ③ 등급(affix_pool §5 챕터 묶음 분포 + Mini/Boss +1 가중 / forcedRarity 우선) → ④ 템플릿 매칭(slot+rarity 정확→슬롯 폴백) → ⑤ AffixRoller로 주 3 + 부 3 일괄 굴림 → ⑥ history.mileage += 1(C7 신화 발현 보류). `Initialize(slotPools, globalPool, templates, config)` 부트스트랩 필요. DropSource enum 5종(Enemy/MiniBoss/Boss/BossRepeat/Shop). 참조: item_drop §4·§5.3.
- 2026-05-01: **Phase G2 아트 P0~P5 진행** — RTX 5070 (192.168.75.194) + DreamShaperXL Turbo SFW + 옵션 C 하이브리드 톤. 116 placeholder 에셋 생성 + 92 SO wiring.
  - **P0** 5 카테고리 reference (Selected_Plush 2 + Selected_Flat 3): Meta·Animal=plush / Nature·Civ·Fantasy=flat
  - **P1** 3 캐릭터 idle (Char_DefaultBoy/BraveGirl/QuietFriend, plush 톤). CharacterData.icon 필드 부재로 wiring skip
  - **P2** 30 적 archetype (5 카테고리 × 6 역할 Small/Medium/Large/MiniA/MiniB/Boss, plush 톤)
  - **P3** 11 보스 (일일 7 Mon~Sun + 주간 4 W1~W4, plush 톤)
  - **P4** 35 챕터 배경 (chapter_structure_v2 §3 35 챕터 1024×768, flat illustration 톤)
  - **P5** 32 UI 아이콘 (Equip 16 + Slot 12 + Currency 4, flat illustration 톤)
  - 폴더: `Assets/_Project/Art/Generated_v2/{Characters,Enemies,Bosses,Backgrounds,UI/{Equipment,Slots,Currency}}/` 총 111 PNG
  - **SO 자동 wiring**: ChapterData.coverArt 35/35 + EquipmentTemplate.icon 16/16 + EnemyData.icon 41/52 (archetype 30 + Daily 7 + Weekly 4 = 41 매핑, 기존 Phase 1·2 적 11종은 PNG 없음)
  - **D3 WondrousMapNode 보강**: 35 노드에 Icon Image 추가 + `_icon` 필드 wiring. Attic Play 시 신기한 지도 진입 → 35 챕터 coverArt 표시 ✅
  - **P6 추억·메타·파티클 15장**: 4 추억 (DadPhoto/DadMemo/DadSticker/DadDoodle) + 3 메타 (HpBoost/DamageBoost/DustGain) + 8 파티클 (StarBurst/HeartFloat/PetalDrop/BubblePop/SparkleTrail/StarTwinkle/HeartGlow/BubbleFloat). flat illustration 톤. CLAUDE.md §2 절대 원칙 (별·하트·꽃잎·비눗방울) 준수.
  - **P6 SO wiring**: MemoryData.image 4/4 + MetaUpgradeData.icon 3/3. VFX 8장은 ParticleSystem texture 로 후속 wiring.
  - **알려진 후속**: Krita 후처리 (transparent BG 제거), CharacterData.icon 필드 추가 + 캐릭터 sprite 동적 변경, EnemyController SpriteRenderer 동적 변경, ParticleSystem 셰이더·시트 wiring
  - **누적**: P0~P6 131 PNG 생성 + 99 SO wiring (Chapter 35 + Equipment 16 + Enemy 41 + Memory 4 + Meta 3) = **138 / 138 (100%)**
  - 참조: `docs/art_pipeline_v2.md` / `Assets/Screenshots/wondrousmap_v2_overlay.png`
- 2026-05-01: **Phase G2 P10 캐릭터 walk 애니메이션 (코드 대체)** — txt2img sprite sheet 시도 실패(동일 seed 8000 + prompt 미세 변경, 본체는 일관성 OK 였으나 가방·신발·표정이 프레임마다 발산. ControlNet/IP-Adapter 없이는 sprite sheet 보증 불가) → PlayerController 코드 애니메이션으로 대체. `Update()` 에서 `_rb.linearVelocity.x` 부호로 SpriteRenderer.flipX, 이동 중일 때 `Mathf.Sin(Time.time * 8f) * 0.05f` 만큼 transform.localPosition.y bob. 1장 idle PNG 만으로 "걷는 듯한" 느낌 — 캐주얼 게임 지브리 톤 충분. 추후 IP-Adapter 환경 정리 시 sprite sheet 재시도 가능.
- 2026-05-01: **Phase G2 P11 EnemyController 스케일 미세 조정** — `EnemyController.ApplyAppearance` 의 spriteScale 베이스 0.4 → 0.55 (37.5% 증가). 적용 후 player(0.6) 대비 small=0.50(83%) / medium=0.55(92%) / large=0.63(105%) / miniBoss=0.77(128%) / boss=0.94(156%) / daily=0.83(138%) / weekly=1.21(200%) — 보스 위압감 확보 + small enemy 시각적 식별성 유지. 41 적 SO 의 scaleMultiplier(0.85~2.2)는 그대로 — 단일 베이스 변경이 전체 archetype 균등 적용. icon 미보유 11 적(Phase 1·2 placeholder)은 spriteScale=1.0 경로 그대로.
- 2026-05-01: **Phase G2 P9 사운드 효과 (절차적 합성)** — `Core/AudioManager.cs` 신설. RuntimeInitializeOnLoadMethod 자동 부트스트랩 + DontDestroyOnLoad. 5종 SfxType(EnemyPop/Pickup/LevelUp/MemoryFound/PlayerHit) 을 코드로 합성(sine wave + envelope, AudioClip.SetData) — 외부 음원 의존 0. **EnemyPop "퐁!"**: 880→220Hz exp sweep + decay 0.15s (CLAUDE.md §2 절대 원칙 첫 사운드 구현). **LevelUp**: C5-E5-G5-C6 4음 아르페지오 0.4s. **MemoryFound**: A4+E5 동시 + attack-decay 0.6s 따뜻한 차임. **Pickup**: 1320+1760Hz 듀얼 사인 0.1s. **PlayerHit**: 200Hz 펄스 + 노이즈 0.08s 만화풍 KO. 자동 wiring: GameEvents.OnEnemyKilled→Pop / OnLevelUp→LevelUp / OnMemoryDiscovered→MemoryFound. PlayerDamageReceiver.OnCollisionStay2D 에서 PlayerHit 직접 호출. AudioSource volume=0.6, spatialBlend=0(2D). 후속: BGM 트랙(허브·챕터·보스), 보스 처치 fanfare, UI 클릭 사운드.
