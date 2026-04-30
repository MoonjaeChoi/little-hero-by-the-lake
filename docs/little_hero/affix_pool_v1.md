# Little Hero — L4 옵션 풀 카탈로그 v1

> [← Little Hero 인덱스](./README.md) · [장비 v1](./equipment_v1.md) · [스탯 v1](./stats_v1.md) · [챕터 v2](./chapter_structure_v2.md)
>
> 5층 아이템 아키텍처의 **Layer 4 (옵션 라인 × 3)** 정의. 슬롯별 affix 풀 + 등급별 정확한 수치 + 굴림/리롤 규칙.

---

## 1. 핵심 결정

- 옵션 라인은 슬롯마다 **3개**.
- 각 라인은 **슬롯 풀에서 가중치 추출** — 1차 60% / 2차 30% / 3차 10%.
- 같은 affix가 다른 라인에서 **중복 허용** (메이플 키우기 모델, 운이 좋으면 빌드 자부심).
- 굴림 결과 수치는 등급별 `[min, max]` 범위에서 균등 추출.
- 리롤 비용: 잠금 안 한 라인 1개당 별가루 ★30. 라인 잠금 시 추가 +50%.
- 등급은 **베이스 등급에 종속** — Common 망토는 Common 풀 수치만 굴림. 강화·신화로 등급이 오르지는 않음.
- **3-티어 챕터(6~10/16~20/26~30) 정합** — 티어별 등장 등급 분포(§5).

---

## 2. Affix 마스터 정의

> Layer 4의 모든 옵션은 아래 23종 affix 풀에서 추출. `target` 은 `PlayerStats` 필드, `op` 은 `ADD` 또는 `MUL` (% 곱연산).

### 2.1. 데미지 채널

| ID | 이름 | target | op | Common | Uncommon | Rare | Epic | Legendary |
|---|---|---|---|---|---|---|---|---|
| `affix_final_dmg` | 최종 데미지 +% | `FinalDamageBonus` | MUL | 1~3 | 3~5 | 5~8 | 8~12 | 12~18 |
| `affix_attack_dmg` | 공격력 +% | `AttackDamage` (合) | MUL | 2~4 | 4~7 | 7~12 | 12~18 | 18~26 |
| `affix_crit_dmg` | 크리티컬 피해 +% | `CritDamage` | ADD | 4~8 | 8~14 | 14~22 | 22~32 | 32~46 |
| `affix_crit_chance` | 크리티컬 확률 +% | `CritChance` | ADD | 0.5~1.5 | 1.5~3 | 3~5 | 5~7 | 7~10 |
| `affix_attack_speed` | 공격 속도 +% | `AttackSpeed` | MUL | 2~4 | 4~6 | 6~10 | 10~14 | 14~20 |
| `affix_armor_pen` | 방어 관통력 +% | `ArmorPenetration` | MUL | 2~4 | 4~7 | 7~12 | 12~18 | 18~26 |
| `affix_extra_targets` | 공격 대상 수 +N | `ExtraAttackTargets` | ADD | +1 | +1 | +1 | +2 | +3 |

### 2.2. 방어·생존

| ID | 이름 | target | op | Common | Uncommon | Rare | Epic | Legendary |
|---|---|---|---|---|---|---|---|---|
| `affix_max_hp` | 최대 HP +% | `MaxHealth` | MUL | 2~4 | 4~7 | 7~11 | 11~16 | 16~24 |
| `affix_armor` | 방어력 +N | `Armor` | ADD | 3~7 | 7~13 | 13~22 | 22~33 | 33~50 |
| `affix_dmg_reduction` | 데미지 감소 +% | `DamageReduction` | ADD | 1~2 | 2~4 | 4~6 | 6~9 | 9~14 |
| `affix_hp_regen` | 자원 회복 +/s | `HpRegen` | ADD | 0.1~0.2 | 0.2~0.4 | 0.4~0.7 | 0.7~1.0 | 1.0~1.5 |

### 2.3. 이동·사거리

| ID | 이름 | target | op | Common | Uncommon | Rare | Epic | Legendary |
|---|---|---|---|---|---|---|---|---|
| `affix_move_speed` | 이동 속도 + | `MoveSpeed` | ADD | 0.05~0.15 | 0.15~0.25 | 0.25~0.40 | 0.40~0.60 | 0.60~0.90 |
| `affix_range` | 사거리 + | `Range` | ADD | 0.1~0.3 | 0.3~0.5 | 0.5~0.8 | 0.8~1.2 | 1.2~1.8 |

### 2.4. 코어 스탯

| ID | 이름 | target | op | Common | Uncommon | Rare | Epic | Legendary |
|---|---|---|---|---|---|---|---|---|
| `affix_strength` | 힘 +N | `Strength` | ADD | +1 | +2 | +4 | +7 | +12 |
| `affix_agility` | 빠름 +N | `Agility` | ADD | +1 | +2 | +4 | +7 | +12 |
| `affix_wisdom` | 슬기 +N | `Wisdom` | ADD | +1 | +2 | +4 | +7 | +12 |
| `affix_spirit` | 마음 +N | `Spirit` | ADD | +1 | +2 | +4 | +7 | +12 |
| `affix_all_stats` | 모든 스탯 +N | (4 코어 동시) | ADD | +1 | +1 | +2 | +3 | +5 |

### 2.5. 메타·드랍

| ID | 이름 | target | op | Common | Uncommon | Rare | Epic | Legendary |
|---|---|---|---|---|---|---|---|---|
| `affix_xp_gain` | xp 획득 +% | `XpGainMultiplier` | MUL | 2~4 | 4~7 | 7~10 | 10~14 | 14~20 |
| `affix_dust_gain` | 별가루 획득 +% | `DustGainMultiplier` | MUL | 4~8 | 8~14 | 14~22 | 22~32 | 32~46 |
| `affix_drop_rate` | 드랍률 +% | `DropRateMultiplier` | MUL | 4~8 | 8~14 | 14~22 | 22~32 | 32~46 |
| `affix_companion_dur` | 동행 지속시간 +% | `CompanionDurationBonus` | MUL | 4~8 | 8~14 | 14~22 | 22~32 | 32~46 |

> 등급별 비율 ≈ **1.5~1.7배 지수 곡선**. 메이플 키우기 "Epic 이상에서 비약적 상승" 패턴 정합.

> 수치는 시작점 가설. 시뮬·플레이테스트로 보정 필요(§7).

---

## 3. 슬롯별 풀 매핑

> 각 슬롯은 1차/2차/3차 affix 3개로 풀이 고정. 라인 굴림 시 **가중치 6:3:1**로 추출.

### 3.1. 주 슬롯 7개

| 슬롯 | 1차 (60%) | 2차 (30%) | 3차 (10%) |
|---|---|---|---|
| 모자 (Hat) | `affix_attack_speed` | `affix_wisdom` | `affix_range` |
| 상의 (Top) | `affix_extra_targets` | `affix_spirit` | `affix_max_hp` |
| 하의 (Bottom) | `affix_final_dmg` ★ | `affix_spirit` | `affix_armor` |
| 망토 (Cloak) | `affix_final_dmg` ★ | `affix_dmg_reduction` | `affix_agility` |
| 장갑 (Gloves) | `affix_crit_dmg` ★ | `affix_crit_chance` | `affix_agility` |
| 무기 (Weapon) | `affix_attack_dmg` | `affix_strength` | `affix_armor_pen` |
| 신발 (Shoes) | `affix_move_speed` | `affix_agility` | `affix_companion_dur` |

★ = 메이플 키우기 우선 큐브 매핑 (스펙업 핵심 부위).

### 3.2. 장신구 슬롯 5개

| 슬롯 | 1차 (60%) | 2차 (30%) | 3차 (10%) |
|---|---|---|---|
| 귀걸이 (Earring) | `affix_max_hp` | `affix_spirit` | `affix_hp_regen` |
| 안경 (Glasses) | `affix_final_dmg` ★ | `affix_wisdom` | `affix_range` |
| 목걸이 (Necklace) | `affix_attack_dmg` | `affix_all_stats` | `affix_xp_gain` |
| 반지 (Ring) | `affix_all_stats` | `affix_crit_chance` | `affix_dust_gain` |
| 펫 (Pet) | `affix_companion_dur` | `affix_drop_rate` | `affix_hp_regen` |

★ = 안경에 "최종 데미지" 부여 — 메이플 키우기 얼굴장식의 강력 옵션 매칭.

### 3.3. 1차 affix 분포 검증

| 1차 affix | 등장 슬롯 수 | 비고 |
|---|---|---|
| `affix_final_dmg` | 3 (하의·망토·안경) | 최강 채널, 트리오 시너지 |
| `affix_attack_dmg` | 2 (무기·목걸이) | 버킷 1 메인 |
| `affix_max_hp` | 2 (상의 3차·귀걸이 1차) | 생존 |
| `affix_attack_speed` | 1 (모자) | 모자 정체성 |
| `affix_extra_targets` | 1 (상의) | 광역 빌드 |
| `affix_crit_dmg` | 1 (장갑) | 장갑 정체성 |
| `affix_move_speed` | 1 (신발) | 신발 정체성 |
| `affix_all_stats` | 1 (반지) | 반지 정체성 |
| `affix_companion_dur` | 1 (펫) | 펫 정체성 |

> 트리오·중복은 **빌드 시너지 의도**, 단일 등장은 **슬롯 정체성 의도**.

---

## 4. 라인 굴림·리롤 규칙

### 4.1. 신규 드랍 시 굴림

```
for line in [1, 2, 3]:
    affix = weighted_pick(slot.pool, weights=[0.6, 0.3, 0.1])
    value = uniform(affix.min[rarity], affix.max[rarity])
    item.lines[line] = Affix(affix.id, affix.op, value, locked=False)
```

- 같은 affix가 라인 2개에 중복 등장 가능.
- 결과 stat 합산은 `EffectResolver` (Part 4)에서 ADD/MUL 분리 적용.

### 4.2. 리롤

```
cost = 30 ★ × locked_multiplier
locked_multiplier = 1 + (locked_lines_count × 0.5)

for line in unlocked_lines:
    affix = weighted_pick(slot.pool, weights=[0.6, 0.3, 0.1])
    value = uniform(affix.min[rarity], affix.max[rarity])
```

- 잠금 1개: 비용 ×1.5 (45 ★)
- 잠금 2개: 비용 ×2.0 (60 ★)
- 잠금 3개: 리롤 불가 (전부 잠겨있음)

### 4.3. 라인 잠금 토글

- 무료. 잠금 상태 토글이 리롤 비용에 반영.
- 잭팟 ARPG Part 2의 "잠금 시 추가 비용" 모델 그대로.

---

## 5. 챕터 티어별 등급 분포

> chapter_structure_v2 §4 + equipment_v1 §3.7 정합.

| 챕터 묶음 | 슬롯 | 드랍 등급 분포 |
|---|---|---|
| 1 ~ 5 (메타·기억) | 주 슬롯만 | Common 90% / Uncommon 10% |
| 6 ~ 10 (티어 1, 슬롯 해방) | 주 + 첫 장신구 | Common 70% / Uncommon 30% |
| 11 ~ 15 (자연 전환) | 주 + 티어 1 장신구 | Common 50% / Uncommon 40% / Rare 10% |
| 16 ~ 20 (티어 2, 향상 장신구) | 주 + 향상 장신구 | Uncommon 50% / Rare 40% / Epic 10% |
| 21 ~ 25 (동물·문명 전환) | 주 + 티어 2 장신구 | Uncommon 30% / Rare 50% / Epic 20% |
| 26 ~ 30 (티어 3, 정점 장신구) | 주 + 정점 장신구 | Rare 40% / Epic 50% / Legendary 10% |
| 31 ~ 35 (환상 정점) | 주 + 티어 3 장신구 | Rare 20% / Epic 60% / Legendary 20% |

> Legendary 등급은 **신화 옵션 발현 시 별도** — 베이스 등급으로서의 Legendary는 챕터 26+에서만.

---

## 6. 시뮬레이션 예시

### 6.1. Common 모자 1개 굴림

```
slot.pool = [attack_speed (60%), wisdom (30%), range (10%)]
rarity = Common

Line 1: roll = 0.42 → attack_speed
        value = uniform(2%, 4%) = 3.1%
Line 2: roll = 0.78 → wisdom
        value = uniform(1, 1) = +1
Line 3: roll = 0.95 → range
        value = uniform(0.1, 0.3) = +0.18

결과: 평범한 모자
  - 공격 속도 +3.1%
  - 슬기 +1
  - 사거리 +0.18
```

### 6.2. Epic 망토 1개 굴림

```
slot.pool = [final_dmg (60%), dmg_reduction (30%), agility (10%)]
rarity = Epic

Line 1: roll = 0.31 → final_dmg
        value = uniform(8%, 12%) = 10.5%
Line 2: roll = 0.55 → final_dmg (중복!)
        value = uniform(8%, 12%) = 9.2%
Line 3: roll = 0.88 → dmg_reduction
        value = uniform(6%, 9%) = 7.4%

결과: 별빛의 망토
  - 최종 데미지 +10.5%
  - 최종 데미지 +9.2%   ← 중복으로 합산 → 19.7% 단일 곱연산
  - 데미지 감소 +7.4%

(망토·하의·안경 트리오에 같은 식으로 + 메타 보너스 적용)
```

### 6.3. 라인 1개 잠금 + 리롤

```
사용자가 Line 1 잠금 (10.5% 만족, 다른 두 라인 리롤 시도)
비용 = 30 ★ × 1.5 = 45 ★

Line 1 (잠금): 최종 데미지 +10.5%
Line 2 → 새로 굴림: roll = 0.05 → final_dmg
        value = uniform(8%, 12%) = 11.8%
Line 3 → 새로 굴림: roll = 0.65 → dmg_reduction
        value = uniform(6%, 9%) = 6.1%

결과: 별빛의 망토 (리롤 후)
  - 최종 데미지 +10.5%  (잠금)
  - 최종 데미지 +11.8%  ← 더 좋아짐
  - 데미지 감소 +6.1%   ← 살짝 나빠짐
```

---

## 7. 데이터 모델 (구현 가이드)

### 7.1. AffixDefinition SO

```csharp
[CreateAssetMenu(menuName = "LittleHero/AffixDefinition")]
public class AffixDefinition : ScriptableObject
{
    public string id;                  // "affix_final_dmg"
    public string displayName;         // "최종 데미지"
    public StatTarget target;          // enum
    public AffixOp op;                 // ADD | MUL

    [Header("등급별 [min, max] 수치")]
    public float[] minByRarity = new float[5];  // [Common..Legendary]
    public float[] maxByRarity = new float[5];

    [Header("표시")]
    public string suffix = "%";        // 표시 접미사 ("%" | "" | "/s")
    public int displayDecimals = 1;
}

public enum AffixOp { ADD, MUL }
public enum StatTarget {
    FinalDamage, AttackDamage, CritDamage, CritChance, AttackSpeed,
    ArmorPenetration, ExtraAttackTargets,
    MaxHealth, Armor, DamageReduction, HpRegen,
    MoveSpeed, Range,
    Strength, Agility, Wisdom, Spirit, AllStats,
    XpGain, DustGain, DropRate, CompanionDuration,
}
```

### 7.2. SlotAffixPool SO

```csharp
[CreateAssetMenu(menuName = "LittleHero/SlotAffixPool")]
public class SlotAffixPool : ScriptableObject
{
    public SlotType slot;
    public AffixDefinition tier1;      // 60% 가중치
    public AffixDefinition tier2;      // 30%
    public AffixDefinition tier3;      // 10%

    [Header("가중치 (합 = 1.0)")]
    public float weightTier1 = 0.6f;
    public float weightTier2 = 0.3f;
    public float weightTier3 = 0.1f;
}
```

### 7.3. AffixRoller (런타임)

```csharp
public static class AffixRoller
{
    public static Affix RollLine(SlotAffixPool pool, Rarity rarity, System.Random rng)
    {
        float roll = (float)rng.NextDouble();
        AffixDefinition picked =
            roll < pool.weightTier1 ? pool.tier1 :
            roll < pool.weightTier1 + pool.weightTier2 ? pool.tier2 :
            pool.tier3;

        int r = (int)rarity;
        float min = picked.minByRarity[r];
        float max = picked.maxByRarity[r];
        float value = Mathf.Lerp(min, max, (float)rng.NextDouble());

        return new Affix
        {
            target = picked.target.ToString(),
            op = picked.op.ToString(),
            value = value,
            locked = false
        };
    }

    public static int RerollCost(EquipmentInstance instance, int baseCost = 30)
    {
        int locked = instance.affixes.Count(a => a.locked);
        float multiplier = 1f + locked * 0.5f;
        return Mathf.RoundToInt(baseCost * multiplier);
    }
}
```

### 7.4. SO 인스턴스 갯수

- **AffixDefinition × 23종** (§2 표 그대로) — `Assets/_Project/Data/Affixes/`
- **SlotAffixPool × 12종** (12 슬롯) — `Assets/_Project/Data/SlotPools/`

각 SlotAffixPool은 위 23종 중 3개를 참조.

---

## 8. UI 노출

### 8.1. 카드 5단의 Layer 4 영역 (equipment_v1 §5.1 정합)

```
┌────────────────────────────────────┐
│ 4. 옵션 라인 × 3                   │
│  ┌───────────────────────────────┐ │
│  │ [🔒]  최종 데미지 +10.5%      │ │  잠금 라인 (자물쇠 아이콘)
│  ├───────────────────────────────┤ │
│  │ [···] 최종 데미지 +9.2%       │ │  미잠금 (점선 테두리)
│  ├───────────────────────────────┤ │
│  │ [···] 데미지 감소 +7.4%       │ │
│  └───────────────────────────────┘ │
│                                    │
│  [ 리롤 ★ 45 ]   ※ 1 라인 잠금   │
└────────────────────────────────────┘
```

- 잠금 토글: 자물쇠 아이콘 탭으로 ON/OFF.
- 리롤 비용: 잠금 수에 따라 실시간 갱신.
- 리롤 후 결과: 1초 내 카운트업 + 별가루 차감 애니메이션 (equipment_v1 §5.2).

### 8.2. 옵션 시각 가이드

| op | 표시 |
|---|---|
| ADD (절대값) | `+12` (회색 +) |
| ADD (%) | `+5%` (파스텔 청록) |
| MUL (%) | `×1.10` 대신 `+10%` 노출 (어린이 친화), 내부적으로 곱연산 |

> "곱셈"이라는 어휘는 노출 X. 모두 `+%` 로 통일하되, 시각 강조(파스텔 보라 = 강함)로 채널 차별.

---

## 9. MVP 범위

### 9.1. MVP 포함

- **Affix 23종 SO** (§7.1 정의대로 모두 작성)
- **SlotAffixPool 12종 SO** (§3 매핑대로)
- **AffixRoller** + 신규 드랍 시 자동 굴림
- **리롤 UI** (잠금 토글 + 비용 표시 + 결과 연출)
- 등급 5단계 모두 (수치 표 §2)
- 챕터 1~5 (Common·Uncommon 풀만 사용)

### 9.2. MVP 제외 (후속)

- Affix별 추가 효과 (자취 효과·시야 확장 등 비수치형) — Phase 1.x
- 옵션 라인 4개 이상 — 시즌 보상 검토
- Affix 추가 (속성 데미지 등) — 4번째 데미지 버킷 도입 시
- 라인별 등급 차이 — 현재는 아이템 등급 = 모든 라인 동일 등급

---

## 10. 미해결 질문

- 등급별 수치 비율 1.5~1.7배가 키즈 게임에 적절한가? (시뮬 필요)
- 라인 가중치 6:3:1이 적절한가? (메이플 키우기는 약 5:3:2 추정)
- 같은 affix 중복 허용 — 빌드 자부심 vs 다양성 위배 균형
- 리롤 기본 비용 30 ★이 챕터 1 기준 적절한가? (챕터 1 평균 별가루 획득량 검증)
- 잠금 비용 +50%/라인이 적절한가? 메이플 키우기는 +100% (잠금당 2배)
- `affix_extra_targets` 의 상한 (Epic +2 / Legendary +3 — 화면 폭발 방지)
- 자취 효과·시야 확장 등 **비수치형 affix** 를 별도 풀로 분리할지 (현재는 누락)
- AffixRoller에 시드 노출 여부 (개발자 디버그 vs 사용자 운 전조)
- 신화 옵션 발현 시 라인 1개 자동 잠금? (잭팟 ARPG Part 2 모델)

---

## 11. 다음 단계

1. **AffixDefinition SO 23종 일괄 생성** — 에디터 스크립트로 `_Project/Data/Affixes/` 채움
2. **SlotAffixPool SO 12종 작성** — 슬롯 매핑 검증
3. **AffixRoller + 단위 테스트** — 가중치·범위 굴림 정확성
4. **Inspector 도구** — Affix 풀 한눈에 보기 (등급 ×슬롯 매트릭스 시각화)
5. **시뮬레이터** — 챕터 1~35 진행 시 평균 affix 가치 추적
6. **카드 UI Layer 4 영역** — 잠금 토글 + 리롤 비용 + 결과 연출
7. **chapter_curve_v1.md** 의 적 스탯 곡선과 정합 검증 (캐릭터 dps 곡선이 챕터 적 hp 곡선을 따라가는지)

---

## 12. 출처

§2 수치 곡선 설계 시 참조:

- [메이플 키우기 잠재능력/옵션 - 게임 팁 저장소](https://game.savetip.co.kr/%EB%A9%94%EC%9D%B4%ED%94%8C-%ED%82%A4%EC%9A%B0%EA%B8%B0-%EC%9E%A0%EC%9E%AC%EB%8A%A5%EB%A0%A5-%EB%B6%80%EC%9C%84%EB%B3%84-%EA%B3%A0%EC%9C%A0%EC%98%B5%EC%85%98-%EC%99%84%EB%B2%BD-%EA%B3%B5%EB%9E%B5/)
- [Diablo 4 Affix Tier List - Maxroll](https://maxroll.gg/d4/getting-started/affix-tier-list)
- [Vampire Survivors weapon evolution stats - Reddit](https://www.reddit.com/r/VampireSurvivors/comments/yvfb6e/full_weapon_stats_table/)
- 잭팟 ARPG 설계서 Part 2 (강화·옵션 라인) + Part 4 (효과 리졸버)
