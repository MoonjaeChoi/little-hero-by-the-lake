# Little Hero — 아이템 드랍·이중 옵션 v1

> [← Little Hero 인덱스](./README.md) · [장비 v1](./equipment_v1.md) · [옵션 풀 v1](./affix_pool_v1.md) · [챕터 곡선 v1](./chapter_curve_v1.md)
>
> 메이플 키우기의 **잠재능력 + 에디셔널 잠재능력** 이중 모델 채용. 주옵션 3줄(슬롯 고유) + 부옵션 3줄(글로벌) + 드랍 결정 트리.

---

## 1. 핵심 결정

- **이중 옵션**: 모든 장비는 **주옵션 3줄 + 부옵션 3줄 = 총 6줄**.
  - 주옵션 = 슬롯 고유 풀 (affix_pool_v1 §3 그대로) — 강력 채널
  - 부옵션 = 글로벌 공용 풀 (본 문서 §3) — 보조 채널, 수치 ≈ 주옵션의 30~50%
- **드랍 시스템**: 적 처치·챕터 클리어 시 확률 굴림 → 슬롯·등급·affix 6줄 일괄 결정.
- **드랍원 4가지**:
  1. 일반 적 (낮은 확률)
  2. 미니보스 (중간 확률)
  3. 메인 보스 (보장 첫 클리어 / 반복은 30%)
  4. 별 조각 상점 (영구 자원으로 직구매)
- **리롤 분리**: 주옵션과 부옵션은 **별도 리롤** 가능. 주 비싸고 부 저렴.
- **잠금 규칙**: 6줄 각각 독립 잠금. 잠금당 비용 +50%.
- **신화 옵션**: 부옵션과 무관, 별도 Layer 5 (equipment_v1 §3.7).

> 기존 affix_pool_v1 §3 의 "옵션 라인 3개"는 이 문서 시점부터 **"주옵션 3줄"** 로 명칭 통일. 부옵션 풀은 본 문서가 신설.

---

## 2. 설계 원칙

### 2.1. 왜 이중 옵션인가

- **단일 풀 3줄**(이전 모델): 슬롯 정체성은 강하지만 빌드 다양성이 낮음. 망토는 무조건 최종 데미지 굴리는 게 정답.
- **이중 풀 6줄**(이 문서): 주옵션은 슬롯 정체성을 유지하면서, 부옵션이 빌드 다양성·캐릭터 보강·소소한 편의(드랍률·xp)를 추가. 메이플 키우기의 검증된 패턴.

### 2.2. 주/부 비중

| 옵션 | 채널 | 평균 기여도 |
|---|---|---|
| 주옵션 | 슬롯 정체성 (최종 데미지·공격 속도·코어 +N 등) | DPS·생존의 **70%** |
| 부옵션 | 글로벌 보조 (소소한 +%·코어·드랍 편의) | DPS·생존의 **30%** |

→ 부옵션은 **있으면 좋은 보조**, 주옵션이 빌드의 코어. 빌드 결정은 주옵션이 주도.

### 2.3. 결정론·잭팟의 분리 (재확인)

| 채널 | 메커니즘 |
|---|---|
| 캐릭터 진행 50% | 결정론 (XP → 레벨업) |
| 강화 +0~+15 | 결정론 |
| 강화 +16~+25 | 잭팟 (천장) |
| **주옵션 3줄** | 잭팟 (리롤) |
| **부옵션 3줄** | 잭팟 (리롤, 저비용) |
| 신화 옵션 | 메가 잭팟 (마일리지) |

---

## 3. 부옵션 글로벌 풀

> 모든 슬롯이 같은 풀을 공유. 슬롯 정체성은 주옵션이 책임지므로 부옵션은 **광범위·균등**.

### 3.1. 부옵션 affix 정의 (12종)

| ID | 이름 | target | op | Common | Uncommon | Rare | Epic | Legendary |
|---|---|---|---|---|---|---|---|---|
| `sub_strength` | 힘 +N | `Strength` | ADD | +1 | +1 | +2 | +3 | +5 |
| `sub_agility` | 빠름 +N | `Agility` | ADD | +1 | +1 | +2 | +3 | +5 |
| `sub_wisdom` | 슬기 +N | `Wisdom` | ADD | +1 | +1 | +2 | +3 | +5 |
| `sub_spirit` | 마음 +N | `Spirit` | ADD | +1 | +1 | +2 | +3 | +5 |
| `sub_max_hp` | 최대 HP +% | `MaxHealth` | MUL | 1~2 | 2~3 | 3~5 | 5~7 | 7~10 |
| `sub_attack_dmg` | 공격력 +% | `AttackDamage` | MUL | 1~2 | 2~3 | 3~5 | 5~8 | 8~12 |
| `sub_armor` | 방어력 +N | `Armor` | ADD | 1~3 | 3~6 | 6~10 | 10~15 | 15~22 |
| `sub_crit_chance` | 크리티컬 확률 +% | `CritChance` | ADD | 0.2~0.5 | 0.5~1.0 | 1.0~1.5 | 1.5~2.5 | 2.5~4.0 |
| `sub_xp_gain` | xp 획득 +% | `XpGainMultiplier` | MUL | 1~2 | 2~3 | 3~5 | 5~7 | 7~10 |
| `sub_dust_gain` | 별가루 획득 +% | `DustGainMultiplier` | MUL | 2~4 | 4~6 | 6~10 | 10~14 | 14~20 |
| `sub_drop_rate` | 드랍률 +% | `DropRateMultiplier` | MUL | 2~4 | 4~6 | 6~10 | 10~14 | 14~20 |
| `sub_hp_regen` | 자원 회복 +/s | `HpRegen` | ADD | 0.05~0.1 | 0.1~0.2 | 0.2~0.3 | 0.3~0.5 | 0.5~0.8 |

> 수치는 주옵션(affix_pool_v1 §2)의 약 **30~50%** 수준. 부옵션 6줄 합산이 주옵션 3줄과 비슷한 가치가 되도록 균형.

### 3.2. 부옵션 풀 가중치 (모든 슬롯 공통)

| affix | 가중치 |
|---|---|
| 4 코어 (힘/빠름/슬기/마음) 각 | 15% × 4 = 60% |
| 최대 HP +% | 10% |
| 공격력 +% | 8% |
| 방어력 +N | 5% |
| 크리티컬 확률 +% | 5% |
| 별가루 획득 +% | 4% |
| xp 획득 +% | 3% |
| 드랍률 +% | 3% |
| 자원 회복 +/s | 2% |

> 합 = 100%. 코어 4종이 60%로 압도적 — 캐릭터 진행과 보강 시너지 강화.

### 3.3. 등급별 부옵션 가치 비교

| 등급 | 평균 부옵션 1줄 | 평균 부옵션 3줄 | 주옵션 3줄과의 비율 |
|---|---|---|---|
| Common | ~2% / +1 | ~6% / +3 | 32% |
| Uncommon | ~3% / +2 | ~9% / +6 | 36% |
| Rare | ~5% / +3 | ~15% / +9 | 42% |
| Epic | ~7% / +4 | ~21% / +12 | 44% |
| Legendary | ~10% / +6 | ~30% / +18 | 48% |

> 등급이 오를수록 부옵션의 상대 기여도도 약간 상승 (Epic 이상부터 부옵션도 중요).

---

## 4. 드랍 결정 트리

```
[적 처치 / 챕터 클리어 / 상점 구매] 이벤트 발생
  │
  ├── 1. 드랍 발생 굴림
  │     일반 적         : 0.5%
  │     미니보스        : 8%
  │     메인 보스 첫번  : 100% (챕터 클리어 보상)
  │     메인 보스 반복  : 30%
  │     상점 구매       : 100%
  │
  ├── 2. 슬롯 결정
  │     일반/미니 적    : 주 슬롯 7개 중 무작위
  │     보스 (티어 챕터): 정해진 장신구 슬롯 (equipment §3.7)
  │     보스 (그 외)    : 주 슬롯 7개 중 무작위 + 등급 +1
  │     상점            : 사용자 선택
  │
  ├── 3. 등급 굴림
  │     affix_pool §5 챕터 묶음별 분포 적용
  │     예: 챕터 1~5 → Common 90% / Uncommon 10%
  │
  ├── 4. 주옵션 3줄 굴림
  │     슬롯 풀 (affix_pool §3) 60/30/10 가중
  │     중복 허용
  │
  ├── 5. 부옵션 3줄 굴림
  │     글로벌 풀 (본 문서 §3) 가중치
  │     중복 허용
  │
  └── 6. 신화 옵션 시도
        마일리지 += 1
        base_rate(0.005) + 마일리지/200 → 발현 시 풀에서 1종
```

### 4.1. 드랍원별 상세

#### 일반 적 (drop_chance 0.5%)

```
1000마리 처치 시 평균 5개 드랍.
챕터당 1000마리 가정 → 챕터당 5개.
대부분 Common 등급 (챕터 1~5).
```

#### 미니보스 (drop_chance 8%)

```
챕터당 3 미니보스 → 평균 0.24개 드랍 (≈ 4챕터에 1개).
등급은 일반 풀보다 +1 등급 가중 (Common 50% / Uncommon 35% / Rare 15%).
```

#### 메인 보스 (첫 클리어 100%)

```
첫 클리어 = 보장 1개 + 챕터 보상 별 조각 ✦.
티어 챕터(6~10/16~20/26~30): 정해진 장신구 슬롯에 첫 한 개.
그 외 챕터: 주 슬롯 7개 중 무작위 + 등급 +1 가중.
```

#### 메인 보스 (반복 30%)

```
같은 보스 재처치 시 30% 드랍.
첫 한 개와 같은 슬롯 풀에서 굴림.
파밍 동기 + 자부심.
```

#### 별 조각 상점

```
다락방 상점에서 별 조각 ✦으로 직접 구매:
- Common 슬롯 ✦ 5
- Uncommon ✦ 12
- Rare ✦ 30
- Epic ✦ 80
- Legendary ✦ 미판매 (보스 처치만 가능)
사용자가 슬롯 선택. 옵션은 미굴림 상태로 시작 → 별가루로 굴림.
```

### 4.2. 슬롯 결정 가중치 (일반/미니 적, 챕터 1~10)

| 슬롯 | 가중치 |
|---|---|
| 모자 / 상의 / 하의 / 망토 / 장갑 / 신발 | 각 14.3% (균등) |
| 무기 | 14.3% |

> 챕터 11+ 부터는 장신구 슬롯도 일정 확률(예: 5%)로 일반 적에서 드랍 (잠금 해제 챕터 한정).

---

## 5. 데이터 모델 확장

### 5.1. EquipmentInstance 6줄 구조

```csharp
[Serializable]
public class EquipmentInstance
{
    public string instanceId;
    public string templateId;
    public int enhanceLevel;
    public int failCount;

    // ✦ 6줄로 확장
    public Affix[] mainAffixes;   // 길이 3 (주옵션)
    public Affix[] subAffixes;    // 길이 3 (부옵션)

    public string mythicId;
    public bool isLocked;
    public History history;
}

[Serializable]
public class Affix
{
    public string id;
    public string target;
    public string op;
    public float value;
    public bool locked;
    public bool isSub;            // ✦ 신규: 부옵션 여부
}
```

### 5.2. SubAffixDefinition SO

```csharp
[CreateAssetMenu(menuName = "LittleHero/SubAffixDefinition")]
public class SubAffixDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public StatTarget target;
    public AffixOp op;
    public float[] minByRarity;
    public float[] maxByRarity;
    public float weight;          // 글로벌 풀 내 가중치
}
```

> 주옵션 SO(`AffixDefinition`)와 별도 SO. 12종 인스턴스 (§3.1 표 그대로).

### 5.3. ItemDropService

```csharp
public class ItemDropService
{
    public EquipmentInstance? TryDrop(DropContext ctx)
    {
        if (!RollDropChance(ctx)) return null;

        var slot = DetermineSlot(ctx);
        var rarity = RollRarity(ctx);
        var template = templateLookup.RandomBySlot(slot, rarity);

        var instance = new EquipmentInstance
        {
            instanceId = Guid.NewGuid().ToString(),
            templateId = template.id,
            enhanceLevel = 0,
            mainAffixes = new Affix[3],
            subAffixes = new Affix[3]
        };

        // 4. 주옵션 3줄
        var slotPool = pools.GetSlotPool(slot);
        for (int i = 0; i < 3; i++)
            instance.mainAffixes[i] = AffixRoller.RollMain(slotPool, rarity, rng);

        // 5. 부옵션 3줄
        for (int i = 0; i < 3; i++)
            instance.subAffixes[i] = AffixRoller.RollSub(globalSubPool, rarity, rng);

        // 6. 신화 시도
        TryRollMythic(instance, ctx.character);

        return instance;
    }
}

public class DropContext
{
    public DropSource source;       // Enemy, MiniBoss, Boss, BossRepeat, Shop
    public int chapterNumber;
    public EnemyData? enemy;
    public CharacterProgress character;
}

public enum DropSource { Enemy, MiniBoss, Boss, BossRepeat, Shop }
```

### 5.4. AffixRoller 확장

```csharp
public static class AffixRoller
{
    public static Affix RollMain(SlotAffixPool pool, Rarity r, Random rng)
    {
        // affix_pool_v1 §7.3 기존 로직 그대로
    }

    public static Affix RollSub(GlobalSubAffixPool pool, Rarity r, Random rng)
    {
        float roll = (float)rng.NextDouble() * pool.totalWeight;
        float acc = 0;
        foreach (var def in pool.affixes)
        {
            acc += def.weight;
            if (roll < acc)
            {
                int rIdx = (int)r;
                float min = def.minByRarity[rIdx];
                float max = def.maxByRarity[rIdx];
                float value = Lerp(min, max, rng.NextDouble());
                return new Affix
                {
                    id = def.id,
                    target = def.target.ToString(),
                    op = def.op.ToString(),
                    value = value,
                    isSub = true
                };
            }
        }
        throw new InvalidOperationException();
    }
}
```

---

## 6. 리롤 규칙 (분리)

### 6.1. 주옵션 리롤

| 잠금 라인 수 | 비용 (별가루) |
|---|---|
| 0 | ★ 30 |
| 1 | ★ 45 |
| 2 | ★ 60 |
| 3 | 리롤 불가 |

(affix_pool_v1 §4와 동일)

### 6.2. 부옵션 리롤

| 잠금 라인 수 | 비용 (별가루) |
|---|---|
| 0 | ★ 15 |
| 1 | ★ 22 |
| 2 | ★ 30 |
| 3 | 리롤 불가 |

> 부옵션은 가치가 낮아 비용도 약 절반.

### 6.3. 부옵션 등급 향상권 (별 조각 ✦ 소비)

```
별 조각 ✦ 10 → 부옵션 1줄을 한 등급 향상 (Common → Uncommon 등)
- Legendary 도달 시 추가 향상 불가
- 한 줄당 누적 향상 가능 (Common → Uncommon → Rare ...)
```

> 메이플 키우기의 "에디셔널 잠재 등급 상승" 차용. 영구 자원으로 점진적 강화.

---

## 7. UI 노출 (카드 5단 → 6단 확장)

```
┌────────────────────────────────────┐
│ 1. 정체성                           │  아이콘 + 이름
├────────────────────────────────────┤
│ 2. 히어로 스탯                      │  공격력·체력 등
├────────────────────────────────────┤
│ 3. 강화                             │  +N / 별가루
├────────────────────────────────────┤
│ 4. 주옵션 (보라색 배경)             │
│    ┌──────────────────────────┐   │
│    │ [🔒] 최종 데미지 +9.5%   │   │
│    │ [···] 데미지 감소 +6.2%  │   │
│    │ [···] 빠름 +4            │   │
│    └──────────────────────────┘   │
│    [ 주옵션 리롤 ★ 30 ]            │
├────────────────────────────────────┤
│ 5. 부옵션 (청록색 배경)             │
│    ┌──────────────────────────┐   │
│    │ [···] 마음 +3            │   │
│    │ [···] xp 획득 +5%        │   │
│    │ [···] 최대 HP +2.5%      │   │
│    └──────────────────────────┘   │
│    [ 부옵션 리롤 ★ 15 ]            │
│    [ 등급 향상 ✦ 10 ]               │
├────────────────────────────────────┤
│ 6. 신화 영역                        │  아빠의 마법
└────────────────────────────────────┘
```

### 시각 가이드

| 영역 | 배경색 | 강조 |
|---|---|---|
| 주옵션 | 파스텔 보라 (#A86CD4 80% 투명) | 굵은 텍스트 |
| 부옵션 | 파스텔 청록 (#5DB3D4 80% 투명) | 일반 텍스트 |
| 잠금 라인 | 황금 테두리 + 자물쇠 | |
| 미잠금 | 점선 테두리 | |

---

## 8. 시뮬레이션 예시

### 8.1. 챕터 1 미니보스 처치 (Enemy_StarCloud)

```
DropContext:
  source = MiniBoss
  chapterNumber = 1
  enemy = Enemy_StarCloud

Step 1: 드랍 굴림 = 0.05 (8% 통과 ✓)
Step 2: 슬롯 = 무기 (가중 균등)
Step 3: 등급 = Uncommon (50/35/15 분포에서 0.6 → Uncommon)
Step 4: 주옵션 (Weapon 풀)
  Line 1: roll=0.45 → attack_dmg → uniform(4%, 7%) = 5.4%
  Line 2: roll=0.81 → strength → uniform(2, 2) = +2
  Line 3: roll=0.93 → armor_pen → uniform(4%, 7%) = 5.1%
Step 5: 부옵션 (글로벌)
  Line 1: roll=0.12 → strength → uniform(1, 1) = +1
  Line 2: roll=0.55 → max_hp → uniform(2%, 3%) = 2.4%
  Line 3: roll=0.95 → drop_rate → uniform(4%, 6%) = 4.7%
Step 6: 신화 시도 → 0.005 → 미발현

결과: 반짝이는 무기
  ─ 주옵션 ─
    공격력 +5.4%
    힘 +2
    방어 관통력 +5.1%
  ─ 부옵션 ─
    힘 +1
    최대 HP +2.4%
    드랍률 +4.7%
```

### 8.2. 챕터 7 보스 첫 클리어 (안경 슬롯 해방)

```
DropContext:
  source = Boss (첫 클리어)
  chapterNumber = 7
  enemy = (챕터 7 보스)

Step 1: 드랍 = 100% 보장
Step 2: 슬롯 = 안경 (티어 1, equipment §3.7)
Step 3: 등급 = Common (티어 1 첫 한 개는 Common 보장)
Step 4: 주옵션 (안경 풀)
  Line 1: roll=0.30 → final_dmg → uniform(1%, 3%) = 2.1%
  Line 2: roll=0.70 → wisdom → uniform(1, 1) = +1
  Line 3: roll=0.65 → wisdom → uniform(1, 1) = +1   (중복!)
Step 5: 부옵션 (글로벌)
  Line 1: roll=0.08 → strength → +1
  Line 2: roll=0.20 → agility → +1
  Line 3: roll=0.85 → dust_gain → uniform(2%, 4%) = 3.2%

결과: 평범한 안경 (첫 안경, 자부심)
  ─ 주옵션 ─
    최종 데미지 +2.1%
    슬기 +1
    슬기 +1   (중복)
  ─ 부옵션 ─
    힘 +1
    빠름 +1
    별가루 획득 +3.2%
```

### 8.3. 부옵션 리롤 후 등급 향상

```
사용자 행동:
  1. 부옵션 리롤 시도 (잠금 0개) → ★ 15
     Line 1 → 마음 +1 (주옵션의 슬기와 시너지)
     Line 2 → 크리 확률 +0.3%
     Line 3 → 자원 회복 +0.08/s
  2. Line 1 (마음 +1) 잠금 → 다음 리롤 시 ★ 22
  3. 부옵션 등급 향상 사용 (Line 1 마음): ✦ 10
     Line 1: 마음 +1 → +2 (Common → Uncommon)
     잠금 유지

총 비용: ★ 15 + ✦ 10
```

---

## 9. chapter_curve_v1 곡선 영향

> 6줄 = 3줄 시점보다 **장비 가치 +30~48%** (§3.3 표).
> 챕터 곡선 곱셈 1.10 그대로 유지하되, **DPS 추정에 부옵션 반영**.

### 9.1. DPS 추정 보정 (chapter_curve §4.4 갱신 권장)

| Level | 챕터 | 기존 DPS (3줄) | 부옵션 보정 (6줄) |
|---|---|---|---|
| L10 | 5 | 26.7 | × 1.06 = 28.3 |
| L20 | 15 | 56.4 | × 1.09 = 61.5 |
| L30 | 25 | 123.7 | × 1.12 = 138.5 |
| L40 | 31 | 281.0 | × 1.14 = 320.3 |
| L50 | 35 | 362.5 | × 1.16 = 420.5 |

→ TTK 평균 **5~12% 단축**. 도파민 영역(30~60s)은 유지되며 챕터 26~30의 ⚠️ "약간 길음"이 해소됨.

### 9.2. 권장 후속 작업

- [ ] chapter_curve_v1 §4.4 의 DPS 표를 부옵션 반영으로 갱신
- [ ] §8.2 의 "보스 HP 950 → 800" 조정안은 **재검토** (부옵션으로 이미 단축됨)
- [ ] §7 권장 레벨 매트릭스 재검증

---

## 10. MVP 범위

### 10.1. MVP 포함

- 주옵션 23종 SO (affix_pool §2 그대로)
- **부옵션 12종 SO** (본 문서 §3.1 신규)
- ItemDropService + DropContext + 4가지 드랍원
- 주옵션·부옵션 분리 리롤 (★ 30 / ★ 15)
- EquipmentInstance 6줄 구조 + SaveData 마이그레이션
- 카드 UI 6단 (주/부 시각 분리)
- 챕터 1~5 (Common·Uncommon 풀, 일반 적·미니·보스 드랍)

### 10.2. MVP 제외 (후속)

- 부옵션 등급 향상권 (✦ 10/줄) — 메타 기능, Phase 1.x
- 별 조각 상점 직구매 (✦ 5~80) — 챕터 5 클리어 후 도입
- 동시 잠금 시각 가이드 (잠금 6개 모두 시각화)
- 부옵션 리롤 시 "확정" 옵션 (메이플 키우기 잠재 큐브)
- 분해 시 별가루 환산 (주옵션 평균 가치 + 부옵션 평균 가치)

---

## 11. 미해결 질문

- 부옵션 가치 30~50%가 주옵션 비중 70:30과 정합한가 (시뮬 필요)
- 부옵션 풀 12종 중 코어 4종이 60%인 가중이 적절한가 (캐릭터 진행과 시너지 측정)
- 일반 적 드랍률 0.5%가 적절한가 (Brotato는 1~2%, Maple Tycoon은 0.1%)
- 미니보스 드랍률 8%가 적절한가 (챕터당 0.24개 = 적은가)
- 보스 반복 처치 30%가 파밍 동기 부여에 충분한가
- 부옵션 리롤 비용 ★ 15이 챕터 1 별가루 수급 대비 적절한가
- 등급 향상권 ✦ 10/줄이 적절한가 (총 3줄 × 4단계 향상 = ✦ 120 — 챕터 5 보상 약 ✦ 25 대비)
- 부옵션 6줄로 시각·인지 부담 검증 (모바일 카드 6단)
- 신화 옵션이 부옵션 슬롯에도 발현 가능한가 (현재: 별도 Layer 5만)
- 분해 시 주/부 별도 환산 비율

---

## 12. 다음 단계

1. **SubAffixDefinition SO 12종 + GlobalSubAffixPool SO 작성** — `_Project/Data/SubAffixes/`
2. **EquipmentInstance 6줄로 마이그레이션** — 기존 SaveData 변환 스크립트 (현 3줄 → main 3 + sub 0)
3. **ItemDropService 구현** — DropContext + 결정 트리 (§4)
4. **AffixRoller.RollSub 추가** — 글로벌 풀 가중 굴림
5. **카드 UI 6단 + 주/부 시각 분리** — 보라/청록 배경
6. **chapter_curve_v1 갱신** — DPS 표 부옵션 반영 (§9.1)
7. **시뮬레이터 갱신** — 부옵션 평균 가치 포함 TTK 재산출

---

## 13. 출처

- [메이플 키우기 잠재능력/에디셔널 - 게임 팁 저장소](https://game.savetip.co.kr/%EB%A9%94%EC%9D%B4%ED%94%8C-%ED%82%A4%EC%9A%B0%EA%B8%B0-%EC%9E%A0%EC%9E%AC%EB%8A%A5%EB%A0%A5-%EB%B6%80%EC%9C%84%EB%B3%84-%EA%B3%A0%EC%9C%A0%EC%98%B5%EC%85%98-%EC%99%84%EB%B2%BD-%EA%B3%B5%EB%9E%B5/)
- [잠재능력/에디셔널 옵션 차이 - 메이플 키우기 마이너 갤러리](https://gall.dcinside.com/mgallery/board/view/?id=maplerpg&no=26893)
- [Diablo 4 Affix Tier - Maxroll](https://maxroll.gg/d4/getting-started/affix-tier-list)
- [affix_pool_v1](./affix_pool_v1.md) (주옵션 풀 — 본 문서 시점부터 명칭 통일)
- [equipment_v1 §3.5](./equipment_v1.md) (옵션 라인 모델 베이스)
