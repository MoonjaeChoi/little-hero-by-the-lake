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
| 장르 | Auto-Shooter Idle |
| 영감 | Vampire Survivors / Maplestory Idle (작은 적 무리·메타 진행) + 지브리 (정서) |
| 진행 방식 | Idle식 (20웨이브 / 챕터, 자동 진행 + 작은 적 무리 + 영구 인벤토리) |
| 플랫폼 | iOS (1차) → Android, macOS |
| 엔진 | Unity 6.3 LTS, URP 2D |
| 언어 | C# (.NET Standard 2.1) |
| 입력 | 가상 조이스틱 (모바일), 키보드 (에디터) |
| 톤 | 카주얼 ARPG, **PG-12 등급** (잔인·피·끔찍 ❌, 만화풍 폭발 OK) |
| 1차 시장 | 한국 / 일본 / 미국 — **캐주얼 + 일반 ARPG** (메이플 키우기·뱀파이어 서바이버 톤) |
| 수익화 | **별빛 ⋆ 캐시 + 시즌 패스 + 가챠 (신화 풀)** — Phase C 신규 도입 |

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

### 절대 원칙: PG-12 톤 (2026-05-02 PIVOT — 옵션 Z)

> **잔인·피·끔찍은 명시적으로 금지**. 그 외 카주얼 ARPG 표준 톤은 허용.
> **갱신 배경**: 4+ 키즈 톤 → 12+ 카주얼 ARPG (캐주얼·일반 사용자 대상 확장).

#### ❌ 금지 (PG-12 유지 항목)
- **피·시체·잔해** — 어떤 형태든 ❌
- **비명 사운드·고통 표현** — ❌
- **사실적 폭력** (총상·자상·외상) — ❌
- **공포·끔찍** (귀신·괴기·트라우마 묘사) — ❌

#### ✅ 허용 (4+ 에서 확장)
- **만화풍 폭발·임팩트** (잔해 X, 별/구름 KO 형태 OK)
- **강한 색·임팩트 VFX** (파스텔 강제 X, 채도 자유)
- **적 처치 표현 자유** — 별 흩어짐 / 만화풍 폭발 / 빙글빙글 KO / 풍선처럼 사라짐 모두 OK
- **약한 긴장·도전** (보스 위압감·시계탑 어두운 분위기 등)

#### VFX·SFX 가이드
- 적 처치: 별 흩어짐 (Phase G2 기존 VFX 유지) 또는 만화풍 폭발
- 적 피격: 만화풍 "별 빙글빙글" KO 또는 강한 임팩트
- 보스 사망: 풍선처럼 사라짐 또는 빛 폭발
- 색감: 파스텔 60~70% 강제 폐기 — 챕터별 분위기에 맞춰 자유

### 텍스트 가이드 (PIVOT 후 갱신)

- 모든 UI 텍스트는 한국어 우선
- **일반 게임 어휘 OK** — 한자어·영어 줄임말 허용 (DPS·치명타·드랍률·리롤·천장 등)
- 적 이름은 게임 표준 (12+ 적합) — "꼬마 슬라임" 친근풍·"심연의 거인" 카주얼 ARPG 풍 모두 OK
- 아빠의 메모·세계관 narrative 는 따뜻한 정서 유지 (스토리 코어 보존)

### 접근성
- 한 손 플레이 가능한 레이아웃 (가상 조이스틱은 좌측 하단 고정)
- 터치 타겟 최소 44pt (Apple 가이드라인) 권장 — 카주얼 ARPG 정보 밀도에 따라 일부 32pt 허용
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

### 웨이브 시스템 (Idle식, PIVOT v3 적용)
- 1챕터 = 20웨이브
- 각 웨이브 30~60초
- **5/10/15 웨이브**: 미니보스
- **20 웨이브**: 챕터 보스
- 웨이브 종료 시 **5초 휴식 (HP 50% 회복) → 자동 다음 웨이브** (PIVOT v2 — 상점 페이즈 폐기)
- **챕터당 ~10,000마리 (PIVOT v3 작은 적 무리)** / 동시 active cap **1,000** / 일반 적 1발 처치

### 상점 페이즈 (archive — PIVOT v2 폐기)
- 코드 보존, 호출 X. 다락방 (가챠/슬롯강화/한정상점/변환모루/마음단련) 으로 분리됨.

### 메타 시스템
- **별가루 (영구, PIVOT v2)**: `★` 아이콘. 적 처치로 획득, 다락방 메타 강화·변환 모루에서 소비. SaveData.starDust 영구 저장. HUD 표시 `별가루 ★ N`.
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

## 7. 성능 목표 (PIVOT v3 갱신)

- **iPhone 12 이상**에서 **60fps 유지**
- **동시 적 1,000마리 cap** (PIVOT v3 작은 적 무리, ObjectPool 200/type pre-warm 필수)
- **GC Allocation/sec <50KB** (Idle 게임 표준, Phase 2 7 hotspot 해결)
- **GPU draw call <10/frame** (EnemyAtlas SpriteAtlas SRP Batcher)
- **메모리** 250MB 이하 (PIVOT v3 ×10 적 + atlas + pool 반영)
- **앱 용량** 150MB 이하

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

## 13. 다음 세션 시작 지점 (2026-05-01 갱신)

> **Phase A~F 51개 태스크 100% 완료**. Phase G(출시) 일부 진행 중. `docs/little_hero/implementation_plan_v1.md` §0 이 단일 진실원.

### 다음 사이클 후보 (우선순위)

| 순위 | 작업 | 이유 | 추정 |
|---|---|---|---|
| 🟡 1 | **시뮬 §13 위험 보정** — TTK 6개 챕터 ⚠️ + 저챕터 XP 과잉 | CurveSimulator 재실행 + BalanceConfig 미세 조정. 실측 보정으로 콘텐츠 밸런스 안정 | 1~2시간 |
| 🟢 2 | **G3 한국어 검수** | UI 텍스트 어린이 어휘 정합 (한자어/영어 줄임말 점검) | 2~3시간 |
| 🟢 3 | **G4 iOS 빌드 + TestFlight** | Xcode 빌드 + Apple Developer 인증 + 베타 배포 | 4~6시간 |
| 🟢 4 | **F1~F3 placeholder 적 디자인 패스** | 30 generic archetype 을 챕터별 고유 명·외형·BPM·밸런싱 으로 정밀화 | 디자인 작업 |
| 🟢 5 | **F4/F5 실 보스 씬 분리** | BossEncounterService 는 완성, 현재 즉시 처치 시뮬. 별도 보스 전투 씬 필요 | 2~3시간 |

### 알려진 제약 — Unity MCP 비정상 종료 (2026-05-01 16:45 KST)

**근본 원인**: Unity Mono 런타임이 MCP 플러그인의 socket 워커 스레드와 domain reload 가 동시 발생할 때 SIGSEGV. F4·F5·F8 사이클에서 다중 `execute_code`(스크립트 편집) + `refresh_unity` + 씬 저장이 연속으로 일어나며 트리거됨.

**스택 트레이스** (`~/Library/Logs/Unity/Editor-prev.log` 마지막 부분):
```
#0  mono_arch_patch_callsite          ← JIT 가 콜사이트 패치 중
#1  common_call_trampoline
#2  mono_magic_trampoline
...
#5  System.Net.Sockets.Socket:ReceiveFrom_internal   ← 동시에 socket 수신 중
#6  System.Net.Sockets.Socket:ReceiveFrom
#9  System.Threading._ThreadPoolWaitCallback
```
JIT가 콜사이트를 패치하는 동안 MCP HTTP 서버(:8080)의 ThreadPool 워커가 socket 수신 중이라 충돌. **com.coplaydev.unity-mcp 패키지의 알려진 패턴**.

**기여 요인**:
- Unity 6000.4.4f1 가 **Rosetta x86_64 모드**로 실행 (Apple Silicon 인데). Rosetta + Mono 6.x = 안정성 ↓
- F6 사이클: InfiniteTower 씬 생성(execute_code) → 스크립트 편집 → refresh_unity 강제 호출 연쇄로 도메인 리로드 다수 발생

**현재 상태 (16:58 사용자가 Bug Reporter 취소 후)**:
- 17:00 Unity 재시작 → 17:02 정상 종료 (clean shutdown)
- `Assets/_Recovery/0.unity` (113KB) — Unity 자동 복구 스냅샷 보관 (다음 실행 시 복원 제안 가능)
- 모든 씬 파일 디스크 정상 (Chapter.unity / InfiniteTower.unity / Attic.unity)
- Git 작업물 모두 commit 완료, 손실 없음

**사용자가 다음에 할 일 (우선순위)**:
1. **Unity 재실행 + 컴파일 확인** — F4·F5·F6·F8 의 최종 컴파일 미검증 (grep 교차 검증으로 대체). Unity 콘솔에 컴파일 에러 표시 시 알려주세요.
2. **(선택) Apple Silicon 네이티브 Unity 설치** — Unity Hub → 6000.4.4f1 의 macOS Apple silicon 빌드 추가. Rosetta 의존 제거로 Mono 안정성 ↑.
3. **(선택) MCP 플러그인 업데이트 점검** — `Library/PackageCache/com.coplaydev.unity-mcp@13fb3ee12774` — 최신 버전 확인.

**다음 MCP 사용 시 안전 패턴**:
- 스크립트 편집 후 `refresh_unity`/`read_console` 호출 시 사이에 5~10초 대기 (도메인 리로드 안정화)
- 씬 저장(`execute_code`로 SaveScene) 후 다음 MCP 호출 전 10초+ 대기
- MCP 가 응답 없을 때 즉시 재시도 X — 1분 대기 후 1회만 재시도, 그래도 안 되면 사용자에게 보고
- 한 사이클에 씬 편집 + 다중 스크립트 편집 + 다중 refresh 를 묶지 말 것 (분리 commit 권장)

**진단 로그 위치**:
- `~/Library/Logs/Unity/Editor-prev.log` — 크래시 직전 로그 (8.2MB, 16:58 까지)
- `~/Library/Logs/Unity/Editor.log` — 재시작 후 로그 (476KB, 17:02 정상 종료)
- `~/Library/Logs/Unity/UnityBugReporter.log` — Bug Reporter 이벤트 (사용자 취소함)

### 빠른 시작 명령

- `리틀히어로 구현 계속` → 자동 선정 (위 순위 1번부터)
- `구현 상태 점검` → Phase 진척만 확인
- `<태스크 ID> 시작` → 특정 태스크 직행 (예: "G3 한국어 검수")

### Phase 3 마일스톤 vs 실제

CLAUDE.md §11 의 Phase 3 "🔄 진행 중" 은 **사실상 종료** (메타 진행·캐릭터·추억·도감·메타 업그레이드 모두 완성). 실질적으로 **Phase 4 (출시)** 진입 단계.

## 14. 변경 이력

> **위치 변경 (2026-05-03)**: 변경 이력은 `docs/little_hero/changelog_v1.md` 로 분리.
> CLAUDE.md 는 핵심만 유지 (architecture_analysis_v1.md §3.3.E + changelog-manager 스킬).
> 신규 항목은 changelog_v1.md 에 추가.

### 최근 핵심 결정 (직전 5건)
- 2026-05-03: **stats_v2.5** — L100 cap 복원 + 장비 무한 강화 (사용자 결정)
- 2026-05-03: **stats_v2_infinite.md** — 3 코어 (힘/민첩/지혜) + 보스/일반 분기 제거 + 마음 폐기
- 2026-05-03: **architecture_analysis_v1.md** — Unity OOP + Claude Code 방법론 평가 (8/10)
- 2026-05-03: **InventorySelectionPanel v2** — 페이징 + 썸네일 + 옵션 (메이플 키우기 풍)
- 2026-05-03: **Damage System v2** — 충돌 폐기 + DamageQuery + 메이플 다중 타깃 보정

자세한 이력은 → [docs/little_hero/changelog_v1.md](docs/little_hero/changelog_v1.md)
