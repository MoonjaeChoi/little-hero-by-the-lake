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
    public static event Action<MemoryData> OnMemoryDiscovered;  // 아빠 추억 발견
    public static event Action<ChapterData> OnChapterCleared;
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
- **별가루 (런 내)**: 상점 구매
- **별 조각 (영구)**: 챕터 클리어 시 획득 → 메타 업그레이드, 캐릭터/장비 언락
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
| 카메라 | Cinemachine (공식) | Unity |
| UI | uGUI (공식) | Unity |
| 사운드 | Unity Audio (필요시 FMOD) | Unity |

신규 라이브러리 도입 시 본 문서에 반드시 기록.

## 11. 마일스톤

| Phase | 기간 | 핵심 산출물 | 상태 |
|---|---|---|---|
| 0 환경 | 1주 | Unity + Xcode + iOS 빌드 성공 | ✅ 완료 (2026-04-29) |
| 1 프로토타입 | 3주 | 다락방 → 1챕터 3웨이브 + 상점 1회 동작 | ✅ 완료 (2026-04-30) |
| 2 확장 | 4주 | 1챕터 20웨이브 완성 + 장비 12종 + 적 6종 | 🔄 진행 예정 |
| 3 콘텐츠 | 4주 | 챕터 3권 + 캐릭터 3명 + 메타 진행 + 추억 시스템 | ⏳ 대기 |
| 4 출시 | 3주 | TestFlight + App Store 심사 | ⏳ 대기 |

## 12. Phase 1 구현 완료 목록

> Claude Code와 함께 작성한 스크립트 목록 (2026-04-30 기준)

### Core
- `GameEvents.cs` — 정적 EventBus, 10개 이벤트
- `GameManager.cs` — 싱글턴, GameState, CurrentWave, StarDust
- `ObjectPool.cs` — 제네릭 `ObjectPool<T>`
- `SaveSystem.cs` — JSON 영구 저장

### Data (ScriptableObject)
- `EquipmentData.cs`, `EnemyData.cs`, `ChapterData.cs`, `CharacterData.cs`, `MemoryData.cs`, `BalanceConfig.cs`

### Gameplay
- **Player**: `PlayerController`, `PlayerStats`, `PlayerHealth`, `PlayerDamageReceiver`
- **Enemies**: `EnemyController`, `EnemyAI`, `EnemySpawner`
- **Waves**: `WaveManager`, `ChapterRunner`
- **Equipment**: `IAttackPattern`, `ProjectileAttack`, `EquipmentManager`, `TestEquip`
- **Combat**: `DamageSystem`, `Projectile`

### UI
- `HUDController` — Slider(체력바) + Legacy Text(웨이브, 별가루, 체력 수치)

### 중요 구현 주의사항
- UI 텍스트: `UnityEngine.UI.Text` (Legacy) — TMP는 URP 셰이더 깨짐
- Rigidbody2D: `linearVelocity` (Unity 6에서 `velocity` deprecated)
- ProjectileAttack.range(15) > EnemySpawner.spawnRadius(10) 유지
- EquipmentManager는 `OnEquipmentChanged` 자기 구독 금지 (무한루프)
- `GameManager.CloseShop()`: `StartWave()` 먼저, `RaiseShopClosed()` 나중에

## 13. Phase 2 다음 작업 목록

1. **상점 UI** — `UI/ShopPanel.cs`, 6칸 슬롯, 별가루 소비, "출발" 버튼
2. **추가 공격 패턴** — `AuraAttack`, `OrbitAttack` (IAttackPattern 구현)
3. **적 추가** — 현재 1종, 6종 목표
4. **장비 SO 데이터** — 현재 1종(테스트용), 12종 목표
5. **Unity MCP 활용** — Claude Code ↔ Unity Editor 실시간 연동

## 14. 변경 이력

- 2026-04-29: 초기 작성 (다락방 동화책)
- 2026-04-29: **프로젝트명 확정 — Little Hero by the Lake**, 호숫가 학교 + 아빠 동화책 스토리 추가
- 2026-04-30: Phase 0/1 완료. 26개 스크립트 작성, Chapter.unity 씬 동작 확인
