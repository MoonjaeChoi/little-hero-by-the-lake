# Stats v2 — 무한 레벨 + 메이플 키우기 능력치 채용 (3 코어)

> **목표**: 레벨 cap 폐기 → 무한 누적. 메이플 키우기 능력치 시스템 채용.
> **3 코어 시스템** (힘·민첩·지혜) + 단일 데미지 공식 (보스/일반 분기 X).
> 마음(Spirit) 폐기 — 기능 흡수 (HP→힘, XP→지혜).

## 0. 결정 이력
- 2026-05-03: 사용자 결정 — L100 cap 폐기, 메이플 키우기 능력치 풀 채용
- 2026-05-03: 사용자 결정 — 보스/일반 분기 제거, **3 코어** (힘/민첩/지혜), 마음→지혜 흡수
- 본 문서: stats_v1 (4 코어) → v2 (3 코어, 무한)

## 1. 데미지 공식 (메이플 키우기 단순화)

```
total = (config.damage + PlayerStats.AttackDamage)
total *= (1 + AdditiveDamage% / 100)         // 합연산
total *= (1 + FinalDamage% / 100)            // 곱연산 (망토·하의·안경)
crit = Random < CritChance ? (1 + CritDamage% / 100) : 1
total *= crit
defense = enemy.armor × (1 - ArmorIgnore% / 100)
total *= 1 - defense / (defense + 1000)      // 메이플 풍 방어 공식
```

**보스/일반 분기 제거** — 단일 데미지 공식. 보스 차별화는 enemy.maxHealth × N + bossAttackDamage 곡선으로.

## 2. 3 코어 스탯

| 코어 | 한국어 | 메이플 매핑 | 100점당 효과 |
|---|---|---|---|
| **Strength** | 힘 | STR (전사) | 공격력 +10 / 방어력 +10 / **HP +50** (마음 흡수) |
| **Agility** | 민첩 | DEX (궁수) | 공속 +5% / 이속 +0.5 / **회피 +2%** (신규) |
| **Wisdom** | 지혜 | INT (마법사) | 크확 +2% / 사거리 +0.5 / **XP +5%** (마음 흡수) / 마법 데미지 +10 |

**Spirit (마음) 폐기**:
- HP +0.5 → 힘 흡수 (강인함 = HP 증가)
- XP +0.0005 → 지혜 흡수 (지능 = 학습 속도)
- ✅ 한국어 "마음" → "지혜" 톤 정합 (PIVOT v2 PG-12 카주얼 ARPG 표준)
- 4 코어 → 3 코어 단순화 (UI 가독성 ↑)

**무한 레벨**:
```
STR/AGI/WIS = Level × characterMultiplier  // cap X
```

## 3. 한국어 라벨 변경

| 영문 (코드) | v1 한국어 | v2 한국어 | 비고 |
|---|---|---|---|
| Strength | 힘 | **힘** (그대로) | |
| Agility | 빠름 | **민첩** | 카주얼 ARPG 표준 어휘 |
| Wisdom | 슬기 | **지혜** | 메이플 INT 표준 번역 |
| Spirit | 마음 | **(폐기)** | 기능 흡수 |

## 4. 신규 파생 스탯 (메이플 키우기 풀 채용)

총 24 파생 (v1 12 + v2 신규 12).

### 4.1 데미지 채널 (3, 보스/일반 분기 제거)
| 필드 | 한국어 | 영향 | op |
|---|---|---|---|
| AttackDamage | 공격력 | base ATK 가산 | ADD |
| AdditiveDamageBonus | 데미지% | 합연산 | MUL |
| FinalDamageBonus | 최종 데미지% | 곱연산 | MUL |

### 4.2 크리티컬 (4)
| 필드 | 한국어 | 베이스 | 영향 |
|---|---|---|---|
| CritChance | 크리 확률 | 5% | 0~100% cap |
| CritDamage | 크리 데미지 | 150% | 합연산 |
| MagicCritChance (신규) | 마법 크리 확률 | 0% | INT 비례 |
| MagicCritDamage (신규) | 마법 크리 데미지 | 0% | INT 비례 |

### 4.3 방어 (5)
| 필드 | 한국어 | 영향 |
|---|---|---|
| Armor | 방어력 | enemy ATK 감소 |
| DamageReduction | 피해 감소% | 받는 피해 곱 |
| EvasionRate (신규) | 회피% | roll 시 미스 (Agility 비례) |
| StatusResist (신규) | 상태이상 저항% | 둔화·기절·DOT 감소 |
| ArmorIgnore (신규) | 방어 무시% | 적 방어 무시 |

### 4.4 공격 (4)
| 필드 | 한국어 | 영향 | cap |
|---|---|---|---|
| AttackSpeed | 공격속도 | interval 곱셈 | **cap X** (메이플 150% 폐기) |
| Range | 사거리 | radius 가산 | X |
| ExtraAttackTargets | 다중 타깃 | maxTargets 가산 | X |
| ChainTargets (신규) | 연쇄 타깃 | chain count 가산 | X |

### 4.5 회복·재생 (3)
| 필드 | 한국어 | 영향 |
|---|---|---|
| MaxHealth | 최대 체력 | 힘 비례 +50/100 |
| HpRegenPerSec (신규) | 체력 재생/초 | 자동 회복 |
| LifestealRatio (신규) | 흡혈% | cap 50% |

### 4.6 메타 (5)
| 필드 | 한국어 | 영향 |
|---|---|---|
| XpGain | 경험치 획득% | 합연산 |
| DustGain | 별가루 획득% | 합연산 |
| DropRate | 드랍률% | 일반 드랍 곱 |
| CompanionDuration | 동료 지속% | summon 효과 |
| CompanionDamage (신규) | 동료 데미지% | 펫·소환 데미지 곱 |

## 5. 무한 레벨 곡선 (chunked)

| 레벨 | HP/lv | ATK/lv | Speed/5lv | Crit/5lv |
|---|---|---|---|---|
| L1~50 | +10 | +1.0 | +0.05 | +1% |
| L51~100 | +5 | +0.5 | 0 | 0 |
| L101~200 | +3 | +0.3 | 0 | 0 |
| L201~500 | +2 | +0.2 | 0 | 0 |
| L501~1000 | +1 | +0.1 | 0 | 0 |
| L1001+ | +0.5 | +0.05 | 0 | 0 |

## 6. 캐릭터별 직업 차별화

| 캐릭터 | 직업 | 주스탯 | 부스탯 |
|---|---|---|---|
| default_boy | 전사 | **힘 2.0×** | 민첩/지혜 0.5× |
| brave_girl | 궁수 | **민첩 2.0×** | 힘/지혜 0.5× |
| quiet_friend | 마법사 | **지혜 2.0×** | 힘/민첩 0.5× |

## 7. PlayerStats v2 신규 필드 (12개)

```csharp
// 폐기: SpiritRaw, Spirit, _spirit*

// 신규 데미지 채널 (보스/일반 분기 제거 — 단일 채널)

// 신규 크리티컬
public float MagicCritChance { get; private set; }
public float MagicCritDamage { get; private set; }

// 신규 방어
public float EvasionRate { get; private set; }
public float StatusResist { get; private set; }
public float ArmorIgnore { get; private set; }

// 신규 공격
public int ChainTargets { get; private set; }

// 신규 회복
public float HpRegenPerSec { get; private set; }
public float LifestealRatio { get; private set; }  // cap 0.5

// 신규 메타
public float CompanionDamage { get; private set; }

// 신규 마법
public float MagicDamage { get; private set; }
public float MagicDamageMul { get; private set; }
```

## 8. 코어 통합 흡수 공식

기존 마음(Spirit) 효과를 힘(STR)·지혜(WIS)에 흡수:

| v1 (Spirit) | v2 흡수 | 변경 효과 |
|---|---|---|
| HP +0.5 (100당 +50) | 힘 +0.5 (100당 +50) | default_boy L100 STR=100 → HP +50 (변경 X) |
| XP +0.0005 (100당 +5%) | 지혜 +0.0005 | default_boy WIS=100 → XP +5% |

**전투 stat (HP/공격/방어) 가 힘 1개 코어로 통합** = 전사 직업 더 강력 + 단순.
**지능 stat (크확/사거리/XP/마법) 가 지혜 1개로 통합** = 마법사 직업 더 강력 + 단순.

## 9. AffixDefinition / SubAffixDefinition StatTarget enum 변경

```csharp
public enum StatTarget
{
    // 기존
    FinalDamage, AttackDamage, CritDamage, CritChance, AttackSpeed,
    ArmorPenetration, ExtraAttackTargets,
    MaxHealth, Armor, DamageReduction, HpRegen,
    MoveSpeed, Range,
    Strength, Agility, Wisdom, AllStats,  // Spirit 제거
    XpGain, DustGain, DropRate, CompanionDuration,

    // v2 신규
    EvasionRate, StatusResist, ArmorIgnore,
    ChainTargets, LifestealRatio, CompanionDamage,
    MagicDamage, MagicCritChance, MagicCritDamage,
}
```

기존 SO 마이그레이션:
- StatTarget.Spirit 사용 라인 → Wisdom 또는 Strength 로 매핑
- AllStats 는 4 → 3 코어 동시 가산으로 변경 (Spirit 제외)

## 10. 데미지 공식 (DamageDealer.EnhanceDamage v2)

```csharp
public static float EnhanceDamage(float baseDamage, EnemyController target = null)
{
    var s = GetStats();
    if (s == null) return baseDamage;

    float dmg = baseDamage + s.AttackDamage;
    dmg *= 1f + s.AdditiveDamageBonus / 100f;     // 합연산
    dmg *= 1f + s.FinalDamageBonus / 100f;        // 곱연산
    // 보스/일반 분기 제거 — 단일 공식

    return dmg;
}
```

**DamageSystem.DealDamage 갱신**:
```csharp
// armor reduction 메이플 풍
float defense = target.Data.armor * (1f - stats.ArmorIgnore / 100f);
final *= 1f - defense / (defense + 1000f);
```

## 11. 마이그레이션 단계 (5 Sprint)

### Sprint 1 — 인프라 (3 commit)
1. **PlayerStats v2** — Spirit 폐기 + 12 신규 필드 + 무한 레벨 chunk 곡선
2. **CharacterData** — strengthMultiplier·agilityMultiplier·wisdomMultiplier 만 (spiritMultiplier 폐기)
3. **3 캐릭터 SO 갱신** — default_boy(힘 2.0)·brave_girl(민첩 2.0)·quiet_friend(지혜 2.0)

### Sprint 2 — Affix 마이그레이션 (2 commit)
4. **StatTarget enum 확장** — Spirit 제거 + 9 신규
5. **AffixDefinition 22종 + SubAffixDefinition 12종** — Spirit 라인 → Wisdom·Strength 매핑

### Sprint 3 — EffectResolver + DamageSystem (2 commit)
6. **EffectResolver → PlayerStats 통합** — affix MUL% / FinalDmg% 모두 PlayerStats 갱신
7. **DamageDealer.EnhanceDamage v2** — 보스/일반 분기 제거 + ArmorIgnore + Lifesteal

### Sprint 4 — UI (2 commit)
8. **CharacterInfoPanel** — 3 코어 + 24 파생 표시
9. **StatTargetLabel 한국어** — "민첩" / "지혜" 갱신 + 신규 9 라벨

### Sprint 5 — 검증 (1 commit)
10. **CurveSimulator L1000** + simulation_report.md

## 12. 영향 받는 파일

### 코드 (15 파일)
- `Scripts/Gameplay/Player/PlayerStats.cs` — 핵심 갱신
- `Scripts/Data/CharacterData.cs` — spiritMultiplier 제거
- `Scripts/Data/AffixDefinition.cs` — StatTarget enum 갱신
- `Scripts/Gameplay/Equipment/EffectResolver.cs` — PlayerStats 통합
- `Scripts/Gameplay/Combat/DamageDealer.cs` — 공식 v2
- `Scripts/Gameplay/Combat/DamageSystem.cs` — armor reduction
- `Scripts/UI/CharacterInfoPanel.cs` — 24 파생 표시
- `Scripts/UI/StatTargetLabel.cs` — 신규 9 한국어
- `Scripts/UI/InventoryDetailView.cs` — 라벨 갱신
- `Scripts/Editor/CurveSimulator.cs` — L1000 시뮬

### 데이터 (40+ SO)
- `Data/Characters/Char_*.asset` × 3 — multiplier 갱신
- `Data/Affixes/*.asset` × 22 — Spirit 라인 매핑
- `Data/SubAffixes/*.asset` × 12 — Spirit 라인 매핑
- `Data/BalanceConfig.asset` — chunk 곡선 갱신

## 13. UI 변경 미리보기

### CharacterInfoPanel v2
```
[캐릭터 portrait]
용감한 소년 (전사)  Lv 100

═══ 4 코어 ═══
힘 STR    : 200    ← 주스탯 (×2.0)
민첩 AGI  :  50    ← 부스탯 (×0.5)
지혜 WIS  :  50    ← 부스탯 (×0.5)

═══ 데미지 (3) ═══
공격력      : 84
데미지%     : +25%
최종 데미지%: +18%

═══ 크리티컬 (4) ═══
크리 확률  : 11%
크리 데미지: 165%
마법 크리확: 0%
마법 크뎀  : 0%

═══ 방어 (5) ═══
방어력     : 20
피해 감소% : 0%
회피%      : 1%   ← 민첩 비례
상태이상저항: 0%
방어 무시% : 0%

═══ 공격 (4) ═══
공격속도   : 1.05× (+5%)
사거리     : 5.5
다중 타깃  : 0
연쇄 타깃  : 0

═══ 회복 (3) ═══
최대 체력  : 940
HP 재생/s  : 0
흡혈%      : 0%

═══ 메타 (5) ═══
경험치 획득: +5%
별가루 획득: +50%
드랍률     : 0%
동료 지속  : 0%
동료 데미지: 0%
```

## 14. 변경 이력
- 2026-05-03: 초기 작성 (4 코어 26 파생)
- 2026-05-03: **사용자 결정 — 3 코어 (힘/민첩/지혜) + 보스/일반 분기 제거 + 마음 폐기**
  - 26 파생 → 24 파생
  - Spirit (마음) 기능 흡수: HP→힘, XP→지혜
  - 한국어 라벨: 빠름→민첩, 슬기→지혜

## 15. 참조

### Sources
- [메이플 키우기/시스템 — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%20%ED%82%A4%EC%9A%B0%EA%B8%B0/%EC%8B%9C%EC%8A%A4%ED%85%9C)
- [메이플스토리 데미지 공식 — fmkorea](https://www.fmkorea.com/8336709224)
- [메이플스토리/스탯 — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%EC%8A%A4%ED%86%A0%EB%A6%AC/%EC%8A%A4%ED%83%AF)
