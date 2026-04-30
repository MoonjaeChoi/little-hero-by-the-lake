# Little Hero — 장비 시스템 v1 초안

> [← Little Hero 인덱스](./README.md) · [원본 잭팟 Part 1](../part_1_equipment_identity.md) · [원본 잭팟 Part 2](../part_2_item_enhancement.md)
>
> 잭팟 ARPG의 5층 아이템 아키텍처를 Little Hero의 7슬롯 시스템·비폭력 톤·아빠 추억 코어에 맞춰 재해석한 v1 초안.

---

## 1. 핵심 결정

- **12슬롯으로 확장** ✦ 2026-04-30 재구성:
  - **주 슬롯 7**: 모자 / 상의 / 하의 / 망토 / 장갑 / 무기 / 신발
  - **장신구 슬롯 5**: 귀걸이 / 안경 / 목걸이 / 반지 / 펫
- **5층 아키텍처 도입** (Part 2 그대로): 베이스 → 강화 +0~+15 → 강화 +16~+25 → 옵션 라인 3개 → 신화 옵션
- **등급 5단계 유지** (기존 `Rarity` enum: Common/Uncommon/Rare/Epic/Legendary)
- **강화 자원**: 별가루(`★`, 런 내) + 강화 보호석(별 조각 ✦로 구매)
- **신화 옵션은 "아빠의 마법"** — 추억 4종(MemoryData)을 다 모으면 신화 풀 언락. 잭팟 ARPG의 마일리지 공식은 그대로 사용.
- **세트 효과는 동화 챕터 테마 + 장신구 트리오**: 빨간모자 / 잭과 콩나무 / 앨리스(장신구 3피스)
- **마이그레이션 모델**: `EquipmentData`(현 SO) → `EquipmentTemplate`(마스터) + `EquipmentInstance`(런타임/세이브)
- **`Bag` 슬롯 폐기**: 기존 가방 2종(`Equip_MagicBag`, `Equip_PulseBag`)은 효과를 다른 슬롯으로 이전하거나 폐기 (마이그레이션 표 참조).

---

## 2. 설계 원칙

### 톤 변환 가이드

> 잭팟 ARPG는 코어 액션 톤. Little Hero는 4+ 키즈. 모든 표현은 **친근하고 비폭력적**으로 변환한다.

| 코어 ARPG 표현 | Little Hero 표현 |
|---|---|
| 강화 실패 | "별가루가 흩어졌어요" |
| 천장 도달 | "이번엔 꼭 성공! (아빠가 도와주는 중)" |
| 신화 발현 | "아빠의 마법이 깨어났어요" |
| 보호 아이템 사용 | "엄마의 응원 부적" |
| 옵션 리롤 | "다른 마법으로 바꿔보기" |
| 옵션 잠금 | "이 마법은 그대로 두기" |
| 세트 2피스 발동 | "두 친구가 함께 모였어요" |

### 결정·잭팟의 균형

- **+0 ~ +15** (결정론, 16단계): 별가루만 있으면 100% 성공. 진입 첫 챕터에서 자연스럽게 채워진다.
- **+16 ~ +25** (RNG + 천장, 10단계): 챕터 4~6 진행 시 도달. 천장 시스템으로 좌절 차단.
- **옵션 라인 3개**: 분/일 단위 잭팟. 별 조각(영구)이 아니라 별가루(런 내)로 리롤하므로 챕터를 도는 동기가 된다.
- **신화 옵션**: 챕터 클리어 + 추억 풀 언락 후의 메가 잭팟. 자부심 코어.

---

## 3. 구체적 사양

### 3.1. 12슬롯 카탈로그 (✦ 재구성)

#### 주 슬롯 (7개) — 의류·무기 코어
HP·공격력·이동속도 등 기본 스탯의 70% 비중. 모든 챕터 진입에 필수.

| 슬롯 ID | 한국어 | 카테고리 | 권장 패턴 | 비고 |
|---|---|---|---|---|
| `Hat` | 모자 | 머리 | Aura | 플레이어 주변 광역 (후광 등) |
| `Top` | 상의 | 몸통 | (스탯) | HP 베이스, 코스메틱 자부심 |
| `Bottom` | 하의 | 다리 | (스탯) | 방어·이동 베이스 |
| `Cloak` | 망토 | 등 | (스탯·특수) | 회피·반사 류 옵션 적합 |
| `Gloves` | 장갑 | 손 | (스탯·공속) | 공격속도·크리티컬 베이스 |
| `Weapon` | 무기 | 메인 무기 | Projectile | DPS의 핵, 단일 슬롯 |
| `Shoes` | 신발 | 발 | Trail | 이동 속도 + 자취 |

#### 장신구 슬롯 (5개) — 디테일·자부심
특수 옵션·세트 시너지 중심. 일부는 가시적 자랑 요소(펫·반지).

| 슬롯 ID | 한국어 | 카테고리 | 권장 패턴 | 비고 |
|---|---|---|---|---|
| `Earring` | 귀걸이 | 머리 옆 | (스탯·작은 효과) | 크리·치명피해 |
| `Glasses` | 안경 | 시야 | (시야·정확도) | 적 표시·미니맵 옵션 적합 |
| `Necklace` | 목걸이 | 목 | (스탯·트리거) | 발동형 효과 적합 |
| `Ring` | 반지 | 손가락 | (스탯·MUL 비율) | 곱연산 옵션 |
| `Pet` | 펫 | 동행 | Orbit / Summon | 회전체·동료, 자부심 가시화 |

> **무기 1개 + 장갑 1개**가 분리되어 있다. 의류 6피스 풀세트 + 무기 + 장신구 5 = 12슬롯 동시 장착. 주 슬롯 6피스가 채워지면 캐릭터의 외형이 완성된다 → 코스메틱 자부심.

### 3.2. 등급 5단계 (한국어 + 시각)

> 기존 `Rarity` enum을 사용하되, 노출 텍스트는 어린이 어휘로.

| Rarity | 한국어 노출 | 색상 (파스텔) | 등장 |
|---|---|---|---|
| Common | 평범한 | `#9E9E9E` 회색 | 챕터 1~2 |
| Uncommon | 반짝이는 | `#76C36A` 연둣빛 | 챕터 1~3 |
| Rare | 빛나는 | `#5D8FE6` 하늘빛 | 챕터 2~5 |
| Epic | 별빛의 | `#A86CD4` 보라 | 챕터 4~ |
| Legendary | 전설의 | `#F2C84B` 금빛 | 챕터 5+ 또는 신화 발현 시 |

### 3.3. 5층 아키텍처

```
Layer 5 — 신화 옵션 (아빠의 마법)
  - 0 ~ 1개. 발현 시 무지개 빛.
  - "아빠의 추억" 4종 모두 발견 시 풀 언락.
  - 잭팟 ARPG Part 2의 마일리지 공식 그대로:
      base_rate = 0.005, mileage 200 도달 시 보장.

Layer 4 — 옵션 라인 × 3 (마법 효과)
  - 별가루로 리롤, 라인별 잠금 가능.
  - target stat: max_health, attack_damage, attack_speed,
                 move_speed, range, xp_gain, dust_gain, crit_chance
  - op: ADD / MUL

Layer 3 — 강화 +16 ~ +25 (RNG + 천장)
  - p_base(level) = 0.50 × 0.85^(level - 16)
  - 소프트 천장: 5회+ 실패부터 +5%/회
  - 하드 천장: 20회 도달 시 100% 보장 ("아빠가 도와주는 중")

Layer 2 — 강화 +0 ~ +15 (결정론)
  - 별가루만 있으면 100% 성공.
  - 비용 곡선은 챕터 진행과 동조 (Part 2 매트릭스 참조).

Layer 1 — 베이스 스탯
  - 드랍 시 등급에 따라 결정.
  - 회수 가치: 분해 시 별가루로 환산.
```

### 3.4. 강화 비용 곡선 (초안)

> 챕터 진행 ↔ 강화 단계 동조 매트릭스(Part 2)를 Little Hero에 매핑.

| 단계 | 별가루 비용 | 강화석 (있으면) | 권장 챕터 |
|---|---|---|---|
| +0 → +1 | 5 | — | 1 |
| +5 → +6 | 25 | — | 1~2 |
| +10 → +11 | 80 | 1 | 2~3 |
| +15 → +16 | 250 | 3 | 3~4 |
| +18 → +19 | 600 | 8 | 4~5 |
| +20 → +21 | 1,200 | 15 | 5+ |
| +24 → +25 | 4,000 | 40 | 6+ (시즌·메타) |

> 위 표는 시뮬레이터로 검증 필요. "별가루는 챕터 보상으로 누적, 강화석은 보스 처치 + 일일 던전" 정도의 분배가 출발점.

### 3.5. 옵션 라인 3개

각 옵션 라인은 다음을 포함:

```ts
type Affix = {
  target: 'max_health' | 'attack_damage' | 'attack_speed'
        | 'move_speed' | 'range' | 'xp_gain' | 'dust_gain'
        | 'crit_chance' | 'damage_reduction'
  op: 'ADD' | 'MUL'
  value: number
  locked: boolean
}
```

**리롤 규칙**:
- 잠금 안 한 라인만 새로 굴림.
- 비용: **별가루**(런 내). 라인 잠금 시 추가 비용 +50%.
- 잭팟 ARPG의 잠금 해제 시 무료 vs 잠금 시 비용 모델 그대로.

**노출 가이드** (Part 5 모바일 카드):
- 잠금된 라인은 자물쇠 아이콘 + 황금 테두리.
- 미잠금 라인은 점선 테두리 + 리롤 버튼 옆에 별가루 비용 표시.

### 3.6. 세트 효과 (챕터 테마 + 장신구 트리오) ✦ 재설계

12슬롯 확장에 맞춰 세트 멤버 슬롯을 재정의.

| 세트 ID | 이름 | 멤버 슬롯 | 2피스 | 3피스 | 5피스 (해당 시) |
|---|---|---|---|---|---|
| `set_red_hood` | 빨간 모자의 용기 | 모자 + 망토 | HP +30, 공격력 +5 | — | — |
| `set_beanstalk` | 잭과 콩나무의 모험 | 무기 + 장갑 | 공격력 +10, 공속 +0.15 | — | — |
| `set_alice` | 앨리스의 호기심 | 귀걸이 + 안경 + 목걸이 | XP +10%, 별가루 +10% | XP +20%, 별가루 +20%, 크리 +5% | — |
| `set_full_outfit` ✦ | 작은 영웅의 정장 | 모자 + 상의 + 하의 + 망토 + 신발 | HP +20 | HP +50, 공격력 +5 | HP +120, 공격력 +15, 이동속도 +0.3 |

**원칙**:
- **챕터 테마 세트**(빨간모자/잭과콩나무/앨리스)는 **서로 다른 챕터의 보스 보상**으로 일부 멤버를 드롭 → 챕터를 골고루 돌아야 완성.
- **`set_full_outfit`(작은 영웅의 정장)은 의류 5피스 세트** → 풀 정장 코스메틱 자부심 보상. 모든 슬롯이 같은 등급 이상이면 외형이 빛난다.
- 장신구 트리오 세트(앨리스)는 **3피스 단계가 핵심 보너스** — 장신구만으로 큰 효과를 낼 수 있어 빌드 다양성에 기여.

### 3.7. 신화 옵션 — 아빠의 마법

**언락 조건** (Little Hero 코어):
- `MetaProgression.AllMemoriesDiscovered() == true` (4종 추억 모두 발견)
- 위 조건을 만족하면 **아빠의 마법 풀**이 활성화되고, 이후 강화 시도에서 잭팟 ARPG Part 2 마일리지 공식대로 발현 시도.

**아빠의 마법 4종 초안**:

| ID | 이름 | 컨셉 | 효과 (Effect) |
|---|---|---|---|
| `mythic_dad_compass` | 아빠의 나침반 | 아빠가 길을 알려준다 | 보스 위치 미니맵 표시 + 보스 데미지 +20% (`MUL`) |
| `mythic_dad_lullaby` | 아빠의 자장가 | 위험할 때 진정시켜준다 | HP 30% 이하 시 받는 데미지 -50% (`condition: when_hp_below_30`) |
| `mythic_dad_courage` | 아빠의 용기 | 마지막 순간 폭발 | 적이 처치 직전 일정 시간 공격력 +50% |
| `mythic_dad_promise` | 아빠의 약속 | 자취가 두 배로 | Trail 자취가 두 배 길게 + 옵션 리롤 1회 무료 |

> 신화 옵션은 슬롯 제한이 있을 수 있다 (`slot_restriction`). 위 초안은 시뮬·플레이 테스트 후 수치 보정.

---

## 4. 데이터 모델 (구현 가이드)

### 4.1. 마스터 데이터 (`EquipmentTemplate`)

기존 `EquipmentData` SO를 확장. 운영 중 변하지 않는 마스터.

```csharp
[CreateAssetMenu(menuName = "LittleHero/EquipmentTemplate")]
public class EquipmentTemplate : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public SlotType slot;
    public Rarity rarity;

    [Header("베이스 스탯")]
    public StatModifiers baseStats;

    [Header("자동 공격 (선택)")]
    public AttackPatternType attackPattern;
    public AttackPatternConfig attackConfig;

    [Header("세트")]
    public string setId;

    [Header("강화·옵션 사양")]
    public int maxEnhanceLevel = 25;
    public int affixLineCount = 3;
}
```

### 4.2. 인스턴스 데이터 (`EquipmentInstance`)

플레이어가 굴려서 만든 결과물. 세이브 데이터의 일부.

```csharp
[Serializable]
public class EquipmentInstance
{
    public string instanceId;     // GUID
    public string templateId;     // EquipmentTemplate.id
    public int enhanceLevel;
    public int failCount;         // 소프트/하드 천장 카운터
    public Affix[] affixes;       // 길이 3
    public string mythicId;       // null = 미발현
    public bool isLocked;         // 잠긴 슬롯/장착 보호
    public History history;       // 굴림 히스토리 (Part 5)
}

[Serializable]
public class Affix
{
    public string target;
    public string op;             // "ADD" | "MUL"
    public float value;
    public bool locked;
}

[Serializable]
public class History
{
    public int totalEnhanceAttempts;
    public int pityTriggeredCount;
    public int mythicAttempts;
    public int mileage;
}
```

### 4.3. 효과 리졸버 (Part 4 그대로)

```csharp
public static class EffectResolver
{
    public static FinalStats Resolve(
        CharacterProgress character,             // Part 1b
        IReadOnlyList<EquipmentInstance> equipped,
        IReadOnlyDictionary<string, EquipmentTemplate> templates,
        IReadOnlyDictionary<string, MythicDefinition> mythics,
        IReadOnlyDictionary<string, SetDefinition> sets)
    {
        // 0. 캐릭터 베이스 (Part 1b의 base_stats)
        // 1. 장착 아이템에서 모든 Effect 수집:
        //    - template.baseStats
        //    - instance.affixes (옵션 3개)
        //    - mythics[instance.mythicId]?.effects
        // 2. 세트 멤버십 카운트 → 충족된 threshold 효과 추가
        // 3. target별 그룹핑
        // 4. ADD → MUL → OVERRIDE 순으로 적용
        // 5. FinalStats 반환
    }
}
```

### 4.4. 마이그레이션 — 기존 16종 EquipmentData (✦ 12슬롯 재매핑)

기존 `EquipmentData` 16종을 `EquipmentTemplate`으로 변환하는 1회성 에디터 스크립트.

| 기존 ID | 신규 슬롯 | 등급 | 패턴 | 세트 | 비고 |
|---|---|---|---|---|---|
| `Equip_StarWand` | Weapon | Common | Projectile | — | |
| `Equip_MeteorRod` | Weapon | Rare | Projectile | `set_beanstalk` | |
| `Equip_HaloLight` | Hat | Common | Aura | — | 머리 위 후광 |
| `Equip_StarlightHalo` | Hat | Rare | Aura | — | |
| `Equip_BraveHat` | Hat | Uncommon | None | `set_red_hood` | |
| `Equip_PetalCloak` | Cloak | Common | None | `set_red_hood` | 꽃잎 망토 → 망토 슬롯 |
| `Equip_DragonflyWings` | Cloak | Uncommon | None | — | 잠자리 날개 → 망토 슬롯 |
| `Equip_HeroOutfit` | Top | Epic | None | `set_full_outfit` | |
| `Equip_RunShoes` | Shoes | Common | None | — | |
| `Equip_RainBoots` | Shoes | Rare | Rain | — | |
| `Equip_SparkleTrail` | Shoes | Uncommon | Trail | — | |
| `Equip_FairyFriend` | Pet | Uncommon | Orbit | — | 요정 친구 → 펫 |
| `Equip_SummonBuddy` | Pet | Rare | Summon | — | 아기 별친구 → 펫 |
| `Equip_DancingStarDust` | Necklace | Rare | Orbit | `set_alice` | 춤추는 별가루 → 목걸이 |
| `Equip_MagicBag` ✦ | **Necklace** | Common | None | `set_alice` | 가방 슬롯 폐기 → 목걸이 트리거형 효과로 흡수 (xp_gain MUL) |
| `Equip_PulseBag` ✦ | **Necklace** | Uncommon | Pulse | — | 가방 폐기 → 목걸이 트리거형으로 (목걸이가 발동하는 광역 폭발) |

**커버 안 된 슬롯**: 하의(Bottom), 장갑(Gloves), 귀걸이(Earring), 안경(Glasses), 반지(Ring) — 합계 5슬롯.
- MVP는 위 5슬롯을 비워두고 진행 가능 (선택 슬롯).
- 후속에서 이 5슬롯을 채우는 신규 16~20종을 추가하는 게 자연스러운 콘텐츠 패치 (Phase 1.x).

**가방 슬롯 폐기 처리**:
- `Equip_MagicBag`(xp_gain +20%) → 목걸이로 이전. 효과 그대로.
- `Equip_PulseBag`(Pulse 패턴) → 목걸이로 이전. 패턴은 "목걸이가 빛날 때 광역 폭발"이라는 트리거형 컨셉으로 재해석.

기존 SO의 `statBonus`/`attackPattern`/`attackConfig`은 그대로 복사 (베이스 스탯 = 현재 statBonus). `setId`는 위 표대로 새로 부여.

---

## 5. UI/UX (Little Hero에 매핑된 Part 5)

### 5.1. 카드 5단 위계 (모바일 풀스크린)

```
┌────────────────────────────────────┐
│ 1. 정체성                           │  아이콘 + "별 마법봉 +12 [반짝이는]"
│    굴림 히스토리 (작게)              │  "총 강화 17회 / 천장 0 / 신화 0"
├────────────────────────────────────┤
│ 2. 히어로 스탯 (큼지막하게 36px)    │  공격력 145 → +8 (강화로)
├────────────────────────────────────┤
│ 3. 강화 영역 (회색 배경)            │  [강화] 별가루 ★ 250  /  천장 게이지 ▓▓░░░ (5/20)
├────────────────────────────────────┤
│ 4. 옵션 라인 × 3                   │
│    [잠금] 공격력 +5%                │
│    공격속도 +12%                    │  [리롤 ★ 30]
│    이동속도 +0.3                    │  [리롤 ★ 30]
├────────────────────────────────────┤
│ 5. 신화 영역 (보라색 배경)          │
│    아빠의 나침반                    │  발현되어 있을 때만 표시
│    "보스 위치가 보여요. +20% 데미지" │
└────────────────────────────────────┘
```

### 5.2. 강화 결과 연출 (1초 도파민 — 톤 친화)

| 결과 | 시각 | 사운드 |
|---|---|---|
| 성공 | 별이 무기에 빨려 들어감 + 스탯 카운트업 | 맑은 "쨍-" |
| 실패 | 별이 사르르 흩어짐 + 천장 게이지 채워짐 | "푸슝~" (귀엽게) |
| 천장 발동 | 아빠 손글씨가 잠깐 비치고 100% 성공 | "두근-" + 따뜻한 종소리 |
| 신화 발현 | 무지개빛이 카드 전체를 휘감음 + 추억 한 장면 | "찰랑~" (반짝임 강함) |

`result_type` enum은 잭팟 ARPG Part 5 그대로:
`success / fail / mythic_unlocked / pity_triggered`.

---

## 6. MVP 범위 (Little Hero v1) ✦ 12슬롯 정합

> Part 8 MVP + 12슬롯 재구성. 모든 슬롯을 한 번에 채우려 하지 않고, **핵심 7슬롯 + 펫 = 8슬롯 활성**으로 시작한다.

**MVP 활성 슬롯 (8개)**:
- 모자, 상의, 망토, 무기, 신발 (의류·무기 5)
- 펫 (장신구 대표 1)
- (선택) 장갑·하의 — 신규 콘텐츠가 충분히 준비되면 포함

**MVP 비활성 슬롯 (4~5개)**: 귀걸이 / 안경 / 목걸이 / 반지 / (장갑·하의 중 미포함분)
- UI에 슬롯은 보이되 "잠금"으로 처리. 후속 패치에서 해제.

**MVP 포함**:
- 12슬롯 정의 + 16종 `EquipmentTemplate` 마이그레이션 (위 매핑표대로)
- 5층 아키텍처: 베이스 + 강화 +0 ~ +15 (결정론) + 옵션 라인 3개 (리롤/잠금)
- 등급 5단계 시각 적용
- 세트: `set_red_hood`(2피스) + `set_beanstalk`(2피스) — 챕터 1·2에서 자연 발생
- 챕터 1 (빨간 모자) + 보스(큰 늑대) — 이미 구현됨, 강화 비용 곡선과 동조 검증
- 별가루(`★`)·별 조각(`✦`) 두 화폐 + 강화석(신규)
- 강화 모달 + 카드 5단 위계 UI

**MVP 제외 (후속 도입)**:
- 강화 +16 ~ +25 (천장)
- 신화 옵션 (Layer 5) — 추억 4종 모두 발견 + 시즌 콘텐츠 패치 후
- `set_alice` 장신구 트리오 — 장신구 슬롯이 활성된 후속 패치
- `set_full_outfit` 5피스 — 의류 풀세트 보유 가능해진 후
- 시즌제·듀얼 실현 (당분간 영구 제외 검토)
- 거래소 (당분간 영구 제외 — 키즈)
- 비활성 슬롯 5종을 채우는 신규 16~20종 — Phase 1.x 콘텐츠 패치

---

## 7. 미해결 질문

- 강화석 한국어 명칭 ("별 모래"? "반짝이 가루"?)
- 별가루 비용 곡선의 정확한 수치 (시뮬 필요)
- 옵션 라인 리롤 비용 (라인당 별가루 30이 맞는지)
- 신화 옵션 4종의 정확한 효과 수치
- ✦ `set_full_outfit` 5피스 보너스의 정확한 수치
- ✦ 비활성 슬롯 5종(귀걸이/안경/목걸이/반지/하의·장갑 일부)을 채울 신규 16~20종 카탈로그
- ✦ 펫 슬롯이 외형 노출에서 차지하는 비중 (캐릭터 옆에 따라다니는 시각)
- 분해 → 별가루 환산 비율
- 굴림 히스토리에 어떤 항목까지 노출할지 (자랑 대상 vs 인지부담)
- 강화 보호석 (Part 2의 보호 아이템)을 Little Hero에 넣을지

### UX 메모: 인벤토리 화면 12슬롯 레이아웃

```
┌─────────────────────── 캐릭터 외형 (좌)  /  슬롯 그리드 (우) ──────────────────┐
│                                                                             │
│   [캐릭터 풀바디]              ┌───────── 주 슬롯 (7) ─────────┐              │
│   ┌─ 모자                     │ 모자  | 상의  | 하의           │              │
│   │ 안경                      │ 망토  | 장갑  | 무기  | 신발   │              │
│   │ 귀걸이                    └────────────────────────────┘              │
│   │ 목걸이                    ┌────── 장신구 슬롯 (5) ───────┐              │
│   │ 망토 (등 뒤)              │ 귀걸이 | 안경 | 목걸이         │              │
│   │ 상의                      │ 반지   | 펫                  │              │
│   │ 장갑                      └─────────────────────────┘              │
│   │ 반지                                                              │
│   │ 하의                      [잠금 슬롯은 자물쇠 + "후속 패치" 배지]  │
│   │ 신발                                                              │
│   │ 펫 (옆에 동행)                                                     │
│   └─                                                                  │
└──────────────────────────────────────────────────────────────────────────┘
```

세로 모드 모바일에서는 캐릭터 풀바디 위, 슬롯 그리드 아래 배치.

---

## 8. 다음 단계

1. **EquipmentTemplate SO 변환 스크립트** — 기존 16종을 `_Project/Data/EquipmentTemplates/`로 복사·변환
2. **EquipmentInstance + 세이브 통합** — `SaveData.equipmentInstances[]` 추가, 장착 슬롯도 인스턴스 ID로 변경
3. **EnhancementService 구현** — 잭팟 ARPG Part 7의 강화 1회 트랜잭션 흐름을 Unity로 포팅 (서버 없이 단말 단일 트랜잭션, 추후 서버 도입 시 같은 인터페이스로 분리 가능하도록)
4. **EffectResolver 구현** — 캐릭터 베이스 + 인스턴스 affix + 신화 + 세트 통합
5. **카드 UI 모바일 5단 모달** — 기존 ShopSlot/Inventory를 새 카드로 대체
6. **별가루 비용 곡선 시뮬** — 챕터 1 클리어까지 +0 ~ +15 도달 가능성 검증
7. **세트 효과 데이터** — `SetDefinition` SO 신설 + 3종 인스턴스
8. **신화 옵션 데이터** — `MythicDefinition` SO 신설 + 4종 초안 (아빠의 마법)
