# Little Hero by the Lake — 한정 상점 (Limited Shop / 핫딜) 시스템 v1

> 메이플 키우기 "핫딜" 시스템을 차용한 **레벨·진행 트리거 기반 임시 상점**.
> **PIVOT 2026-05-02**: 시뮬 §13 의 비과금 retention 위험을 캐시 패키지·확정 보상으로 보완.

## 0. 출처

- [MapleStory Idle RPG Wiki — Hot Deals](https://idle.maplestorywiki.net/w/Hot_Deals)
- [Vortex Gaming — Maple Raising Hot Deal Guide](https://vortexgaming.io/en/postdetail/587586)
- [메이플 키우기 핫딜 소과금 빌드 (DCinside)](https://m.dcinside.com/board/maplerpg/26947)
- [메이플 키우기 시스템 (나무위키)](https://namu.wiki/w/메이플%20키우기/시스템)

## 1. 디자인 원칙

### 1.1 핵심 컨셉
- **레벨·진행 milestone 도달 시** 자동 활성화되는 **시간 한정 패키지**
- 정상 가챠로는 못 뽑거나 극악 확률인 **상위 등급 확정 무기** + **진행 가속 자원**
- **구매 1회 제한** (대부분) + 방금 클리어한 콘텐츠와 연계된 보상
- **20~50% 할인** — 정상 캐시 패키지 대비 가성비 우월

### 1.2 메이플 차이
| 메이플 키우기 | Little Hero by the Lake |
|---|---|
| 동료 뽑기 6레벨 → 유니크 소환권 핫딜 | 소환 레벨 ⭐3/⭐5/⭐7 도달 → Rare/Epic/Legendary 확정 무기 |
| 스테이지 3-3 → 유니크 무기 | 챕터 5/10/15/20/25/30/35 첫 클리어 → 진행도별 패키지 |
| 직업 전직 핫딜 | 캐릭터 변경 핫딜 (3 캐릭터) |
| 월드보스 8단계 | 일일/주간 보스 첫 처치 핫딜 |
| 캐시 다이아 only | **별빛 ⋆ 무료 핫딜 + 캐시 핫딜 혼합** (비과금 친화) |

### 1.3 비과금/캐시 비율
- **무료 핫딜 (별빛 ⋆)**: 트리거 도전 보상 — 비과금 진행 가속
- **캐시 핫딜 (Unity IAP)**: 확정 상위 등급 무기 — P2W 영역
- 비율 약 **30% 무료 / 70% 캐시** (메이플 키우기 100% 캐시 대비 완화)

## 2. 트리거 (Trigger)

| TriggerType | 조건 | 예시 |
|---|---|---|
| `SummonLevelReached` | 소환 레벨 N 도달 | ⭐2 → 빨간 단검 무료 / ⭐3 → Rare 확정 5,500원 |
| `ChapterFirstCleared` | 챕터 N 첫 클리어 | CH5/10/15/20/25/30/35 7개 milestone |
| `NewUser` | 게임 첫 시작 후 7일 | 입문자 패키지 (1,500원) |
| `DailyBossFirstClear` | 일일 보스 첫 처치 (출시 후) | 시즌 한정 패키지 |
| `WeeklyBossFirstClear` | 주간 보스 첫 처치 (출시 후) | 신화 보장권 |
| `CharacterFirstSelect` | 캐릭터 선택 시 (3 캐릭터별) | 캐릭터 입문 패키지 |

## 3. 상품 풀 (Product Pool)

### 3.1 무료 핫딜 (별빛 ⋆ 결제, 출시 시)

| ID | 트리거 | 가격 | 보상 |
|---|---|---|---|
| `free_summon_lv2` | ⭐2 도달 | 별빛 ⋆ 200 | Rare 무기 1개 확정 (가챠 풀에서) |
| `free_chapter_05` | CH5 첫 클리어 | 별빛 ⋆ 100 | 스타포스 조각 1,000 + 무기 소환권 5 |
| `free_chapter_15` | CH15 첫 클리어 | 별빛 ⋆ 500 | 조각 5,000 + 소환권 10 |
| `free_chapter_25` | CH25 첫 클리어 | 별빛 ⋆ 1,000 | 조각 15,000 + Epic 무기 1개 확정 |

### 3.2 캐시 핫딜 (Unity IAP, 출시 시)

| ID | 트리거 | 가격 | 보상 | 정상가 대비 |
|---|---|---|---|---|
| `cash_newuser_starter` | NewUser (7일) | 1,500원 | 별빛 ⋆ 1,000 + 조각 1,000 + 소환권 5 | -50% |
| `cash_summon_lv3_rare` | ⭐3 도달 | 5,500원 | Rare 무기 1개 확정 + 조각 3,000 | -40% |
| `cash_summon_lv5_epic` | ⭐5 도달 | 12,000원 | Epic 무기 1개 확정 + 조각 10,000 + 별빛 1,100 | -45% |
| `cash_summon_lv7_legendary` | ⭐7 도달 (출시 후) | 25,000원 | Legendary 무기 1개 확정 + 조각 30,000 | -50% |
| `cash_chapter_10` | CH10 첫 클리어 | 3,000원 | 조각 5,000 + 별빛 500 + 소환권 10 | -35% |
| `cash_chapter_20` | CH20 첫 클리어 | 6,000원 | Rare 무기 1개 확정 + 조각 15,000 | -40% |
| `cash_chapter_30` | CH30 첫 클리어 | 12,000원 | Epic 무기 1개 확정 + 조각 50,000 + 별빛 3,500 | -45% |
| `cash_character_starter` | CharacterFirstSelect | 1,500원/캐릭터 | 별빛 ⋆ 500 + 조각 500 (3 캐릭터 × 1 = 3 핫딜) | -30% |

### 3.3 출시 후 시즌 (Phase E5+)

| ID | 트리거 | 가격 | 보상 |
|---|---|---|---|
| `cash_weekly_boss_mythic_1st` | WeeklyBossFirstClear | 30,000원 | Mythic 발현 보장권 1개 |
| `cash_summon_lv10_mythic` | ⭐10 도달 | 50,000원 | Mythic 무기 1개 확정 |

## 4. 만료 (Duration)

| TriggerType | duration | 비고 |
|---|---|---|
| `NewUser` | 7일 | 게임 첫 시작 +7일 |
| `SummonLevelReached` | 14일 | 도달 +14일 |
| `ChapterFirstCleared` | 7일 | 클리어 +7일 |
| `DailyBoss/WeeklyBoss` | 3일 | 처치 +3일 |
| `CharacterFirstSelect` | 14일 | 선택 +14일 |

## 5. 데이터 모델

### 5.1 LimitedShopOffer SO
```csharp
[CreateAssetMenu(menuName = "LittleHero/LimitedShopOffer")]
public class LimitedShopOffer : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;

    public TriggerType triggerType;
    public int triggerValue;            // 소환 레벨 N / 챕터 N
    public int durationDays;            // 트리거 +N일 만료

    // 가격 (둘 중 하나만)
    public int starlightPrice;          // 별빛 ⋆ (무료 핫딜)
    public string iapProductId;         // Unity IAP productId (캐시 핫딜)

    // 보상
    public RewardEntry[] rewards;
}

public enum TriggerType {
    SummonLevelReached, ChapterFirstCleared, NewUser,
    DailyBossFirstClear, WeeklyBossFirstClear, CharacterFirstSelect
}

[Serializable]
public class RewardEntry {
    public RewardType type;
    public int amount;
    public string equipmentTemplateId;  // GuaranteedWeapon 시
    public Rarity rarity;                // 등급 풀에서 1개 확정 시
}

public enum RewardType {
    GuaranteedWeapon, GuaranteedRarityPool,
    StarforceShards, StarlightBundle, SummonTickets, MagicCrystal, RainbowOrb
}
```

### 5.2 SaveData 확장
```csharp
public LimitedOfferTrigger[] limitedOfferTriggers;  // 트리거 시각
public string[] purchasedLimitedOfferIds;          // 구매 완료
```

```csharp
[Serializable]
public class LimitedOfferTrigger {
    public string offerId;
    public long triggeredUnix;  // 0 = 미트리거, > 0 = 트리거 시각
}
```

### 5.3 GameEvents 신규
```csharp
public static event Action<LimitedShopOffer> OnLimitedOfferActivated;
public static event Action<LimitedShopOffer> OnLimitedOfferPurchased;
public static event Action<LimitedShopOffer> OnLimitedOfferExpired;
```

## 6. Service

### 6.1 LimitedShopService (정적 facade)
```csharp
public static class LimitedShopService
{
    public static void Initialize(LimitedShopOffer[] catalog);

    // 트리거 hook (각 hook 은 SaveData.limitedOfferTriggers 갱신 + 이벤트 발행)
    public static void HandleSummonLevelUp(int newLevel);
    public static void HandleChapterFirstClear(int chapterN);
    public static void HandleNewUser();
    public static void HandleDailyBossClear();
    public static void HandleWeeklyBossClear();
    public static void HandleCharacterFirstSelect(string characterId);

    // 조회
    public static List<LimitedShopOffer> GetActiveOffers();
    public static bool IsOfferAvailable(LimitedShopOffer offer);
    public static bool IsOfferPurchased(LimitedShopOffer offer);
    public static long GetExpirationUnix(LimitedShopOffer offer);
    public static long GetSecondsRemaining(LimitedShopOffer offer);

    // 구매
    public static PurchaseResult TryPurchase(LimitedShopOffer offer);
}

public enum PurchaseResult {
    Success, AlreadyPurchased, NotTriggered, Expired,
    InsufficientStarlight, IAPRequired, IAPFailed
}
```

### 6.2 트리거 hook 흐름

```
GachaService.RollSingle 
  → SaveData.summonLevel ↑
  → LimitedShopService.HandleSummonLevelUp(newLevel)
  → catalog 에서 SummonLevelReached + triggerValue == newLevel 매칭
  → SaveData.limitedOfferTriggers 에 (offerId, now) 추가
  → OnLimitedOfferActivated 이벤트 발행
  → AtticController 의 한정 상점 버튼에 빨간 뱃지 표시

StarlightGrantService.TryGrantChapterFirstClear
  → LimitedShopService.HandleChapterFirstClear(chapterN)
  → 동일 흐름
```

### 6.3 구매 흐름

```
사용자가 LimitedShopPanel 에서 핫딜 클릭
  → LimitedShopService.TryPurchase(offer)
  → IsOfferAvailable() && !IsOfferPurchased() 검증
  → starlightPrice > 0 분기: CurrencyService.SpendStarlight
  → iapProductId 분기: IAPService.InitiatePurchase(productId, ApplyRewards)
  → ApplyRewards: rewards[] 순회 → RewardType 별 grant
    - GuaranteedWeapon: GachaService.GrantSpecificWeapon(equipmentTemplateId)
    - GuaranteedRarityPool: GachaService.RollGuaranteed(rarity)
    - StarforceShards: StarforceService.GrantShards
    - StarlightBundle: CurrencyService.GrantStarlight
    - SummonTickets: GachaService.GrantTickets
    - MagicCrystal: CurrencyService.GrantMagicCrystal
    - RainbowOrb: CurrencyService.GrantStarFragment
  → SaveData.purchasedLimitedOfferIds 추가
  → OnLimitedOfferPurchased 이벤트
```

## 7. UI

### 7.1 LimitedShopPanel
- 다락방 새 진입 버튼 `🔥 한정 상점`
- 활성 핫딜 N개 빨간 뱃지 표시
- 패널 진입 시 활성 핫딜 그리드 (4×N)
- 각 카드: 아이콘 + 이름 + 보상 미리보기 + 가격 + 남은 시간 (예: "남은 시간 2일 4시간")
- 구매 버튼 (별빛 또는 캐시)

### 7.2 LimitedShopItemView
- 카드 1개 시각화
- 잔여 시간 카운트다운 (Update 폴링)
- 잔여 시간 < 1시간 시 빨간색 경고

### 7.3 알림 — OnLimitedOfferActivated
- HUD 우상단 토스트 알림 "🔥 새로운 핫딜이 열렸어요!"
- 다락방 한정 상점 버튼에 N 뱃지

## 8. 시뮬 영향 (D1 시뮬 §11~13 갱신 후속)

### 8.1 비과금 retention 보호 (옵션 A 단순 강화 대비)
- ⭐2 도달 (=가챠 100회) 시 별빛 ⋆ 200 결제 → Rare 무기 1개 확정
  → **비과금이 1개월에 적어도 Rare 1개 보유** 보장
- CH5/15/25 첫 클리어 무료 핫딜 → 일일 보스·주간 보스 처치 동기 ↑
- 챕터 진행 → 핫딜 트리거 → 추가 자원 → 진행 가속 → retention loop

### 8.2 캐시 매출 영향
- ⭐3 Rare 확정 5,500원: 정상 5,500⋆ 패키지 = 60 단판 = Rare 0~3개 확률.
  핫딜로 1개 확정 + 조각 3,000 → **+30% 가성비 매력**
- 시뮬 §13 의 "캐시 5500⋆ = 비과금 17~20일분" 가속 효과 + 확정 무기 보너스

## 9. 작업 분해

| 단계 | 작업 | 시간 |
|---|---|---|
| 1 | LimitedShopOffer SO 클래스 + SaveData 확장 + GameEvents 3 신규 | 1시간 |
| 2 | LimitedShopService 정적 facade + 트리거 hook 6 + 구매 흐름 | 3시간 |
| 3 | SO 인스턴스 12개 (출시 시 4 무료 + 8 캐시) | 1시간 |
| 4 | 기존 hook 통합 — GachaService.ApplyResult / StarlightGrantService / BossEncounterService | 1시간 |
| 5 | LimitedShopPanel + LimitedShopItemView UI | 2시간 |
| 6 | AtticController 진입 버튼 + 알림 토스트 + 뱃지 | 1시간 |
| 7 | 씬 wiring 자동화 + Unity IAP productId 등록 | 1시간 |
| 8 | CurveSimulator §15 한정 상점 영향 시뮬 추가 | 1시간 |

총 ~11시간 (~2일).

## 10. MVP 범위

**출시 시 (Phase 1)**:
- 데이터 모델 + Service + 무료 핫딜 4개 + 캐시 핫딜 4개 (newuser/⭐3/⭐5/CH10)
- LimitedShopPanel 단순 그리드 + 카운트다운
- AtticController 진입 + 뱃지

**출시 후 (Phase 2)**:
- ⭐7 Legendary / ⭐10 Mythic / 보스 핫딜
- 시즌 한정 핫딜 (시즌 패스 통합)
- 한정 상점 통계 (캐시 매출 추적)
