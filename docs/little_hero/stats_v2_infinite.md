# Stats v2 — 무한 레벨 + 메이플 키우기 능력치 채용

> **목표**: 레벨 100 cap 폐기 → 무한 누적. 메이플 키우기·메이플스토리의 능력치 시스템(20+ 파생 스탯) 모두 채용.
> 데미지 공식: 합연산(데미지%) × 곱연산(최종) × 보스/일반 분기 × 방무/방어.

## 0. 결정 이력
- 2026-05-03: 사용자 결정 — L100 cap 폐기, 메이플 키우기 능력치 풀 채용
- 본 문서: stats_v1 → stats_v2 디자인 마이그레이션

## 1. 메이플 키우기 데미지 공식 (출처 종합)

```
총 데미지 = (공격력)
          × (공격속도 + 1)
          × (데미지% + 1)         ← 합연산 (Damage)
          × (스탯비례데미지% + 1)  ← STR/DEX/INT/LUK 합산
          × (보스데미지% + 1) OR (일반몹데미지% + 1)
          × (최소+최대 데미지)/2
          × (크뎀 × 크확 + 1)     ← 합연산
          × 무기 상수
          × (1 - 방무 비율)        ← 방어 무시
```

**Little Hero 단순화 적용**:
```
total = (config.damage + PlayerStats.AttackDamage)
total *= (1 + AdditiveDamage% / 100)         // 합연산
total *= (1 + FinalDamage% / 100)            // 곱연산
total *= isBoss ? (1 + BossDamage% / 100)
                : (1 + NormalDamage% / 100)
crit = Random < CritChance ? (1 + CritDamage% / 100) : 1
total *= crit
defense = enemy.armor × (1 - ArmorIgnore% / 100)
total *= 1 - defense / (defense + 1000)
return total
```

## 2. 4 코어 스탯 (기존 보존)

| 코어 | 한국어 | 메이플 STR/DEX/INT/LUK 매핑 | 기능 |
|---|---|---|---|
| Strength | 힘 | STR | 공격력 +0.1 / 방어력 +0.1 (100점당) |
| Agility | 빠름 | DEX | 공속 +0.0005 / 이속 +0.005 |
| Wisdom | 슬기 | INT | 크확 +0.0002 / 사거리 +0.005 |
| Spirit | 마음 | LUK | HP +0.5 / XP +0.0005 |

**무한 레벨**: `STR/AGI/WIS/SPR = Level × characterMultiplier` 그대로 무한 누적 (cap 폐기).

## 3. 신규 파생 스탯 (메이플 키우기 풀 채용)

기존 12 + 신규 14 = **26 파생 스탯**.

### 3.1 데미지 채널 (5)
| 필드 | 한국어 | 영향 | op |
|---|---|---|---|
| AttackDamage | 공격력 | base ATK 가산 | ADD |
| AdditiveDamageBonus | 데미지% | 합연산 곱 | MUL |
| FinalDamageBonus | 최종 데미지% | 최종 곱연산 | MUL |
| BossDamage (신규) | 보스 데미지% | 보스전만 합 | MUL |
| NormalDamage (신규) | 일반몹 데미지% | 일반전만 합 | MUL |

### 3.2 크리티컬 (4)
| 필드 | 한국어 | 베이스 | 영향 |
|---|---|---|---|
| CritChance | 크리 확률 | 5% | 0~100% cap |
| CritDamage | 크리 데미지 | 150% | 합연산 (cap X) |
| MagicCritChance (신규) | 마법 크리 확률 | 0% | INT 비례 |
| MagicCritDamage (신규) | 마법 크리 데미지 | 0% | INT 비례 |

### 3.3 방어 (5)
| 필드 | 한국어 | 영향 |
|---|---|---|
| Armor | 방어력 | enemy ATK 감소 |
| DamageReduction | 피해 감소% | 받는 피해 곱 |
| EvasionRate (신규) | 회피% | roll 시 미스 |
| StatusResist (신규) | 상태이상 저항% | 둔화·기절·DOT 감소 |
| ArmorIgnore (신규, 메이플 방무) | 방어 무시% | 적 방어 무시 |

### 3.4 공격 (4)
| 필드 | 한국어 | 영향 | cap |
|---|---|---|---|
| AttackSpeed | 공격속도 | interval 곱셈 | **cap X (메이플 150% 폐기)** |
| Range | 사거리 | radius 가산 | X |
| ExtraAttackTargets | 다중 타깃 | maxTargets 가산 | X |
| ChainTargets (신규) | 연쇄 타깃 | chain count 가산 | X |

### 3.5 회복·재생 (3)
| 필드 | 한국어 | 영향 |
|---|---|---|
| MaxHealth | 최대 체력 | base HP |
| HpRegenPerSec | 체력 재생/초 | 자동 회복 |
| LifestealRatio (신규) | 흡혈% | 가한 데미지의 N% 회복 |

### 3.6 메타 채널 (5)
| 필드 | 한국어 | 영향 |
|---|---|---|
| XpGain | 경험치 획득% | 합연산 |
| DustGain | 별가루 획득% | 합연산 (메이플 메소) |
| DropRate | 드랍률% | 일반 드랍 곱 |
| CompanionDuration | 동료 지속% | summon 효과 |
| CompanionDamage (신규) | 동료 데미지% | 펫·소환 데미지 곱 |

## 4. 무한 레벨 곡선 (chunk 기반)

### 4.1 레벨업 보너스 (chunked)
| 레벨 | HP/lv | ATK/lv | Speed/5lv | Crit/5lv | 비고 |
|---|---|---|---|---|---|
| L1~50 | +10 | +1.0 | +0.05 | +1% | fast (기존) |
| L51~100 | +5 | +0.5 | 0 | 0 | medium (기존 대비 보강) |
| L101~200 | +3 | +0.3 | 0 | 0 | slow |
| L201~500 | +2 | +0.2 | 0 | 0 | very slow |
| L501~1000 | +1 | +0.1 | 0 | 0 | idle |
| L1001+ | +0.5 | +0.05 | 0 | 0 | endgame |

L1000 도달 시 누적: HP +740 + 600 + 600 + 500 = **+2,440 HP**, ATK +49+25+30+60+50 = **+214 ATK** (코어 별도).

### 4.2 코어 누적 (무한 선형)
```
STR/AGI/WIS/SPR = Level × characterMultiplier
```
L1000 + char default(mul=1) → **각 코어 1,000**.
- HP: 1000 × 0.5 = +500
- ATK: 1000 × 0.1 = +100
- Armor: 1000 × 0.1 = +100
- AS: 1000 × 0.0005 = +50% (cap 폐기)
- Crit: 1000 × 0.0002 = +20%
- Range: 1000 × 0.005 = +5

### 4.3 XP 곡선 무한
```
L1~50: threshold = 50 × 1.15^L  (기존)
L51~100: threshold = prev × 1.05^(L-50)  (기존 둔화)
L101+: threshold = prev × 1.02^(L-100)  (신규, 매우 둔화)
```
**도달 시간 예상** (현 PIVOT v4 마리당 0.5 XP):
- L100 도달: ~챕터 35 클리어
- L500 도달: 약 200시간 idle play
- L1000 도달: 약 2,000시간 (메이플 키우기 풍 endgame)

## 5. PlayerStats v2 — 신규 필드 14개

```csharp
// 신규 데미지 채널
public float BossDamage { get; private set; }      // % (보스전)
public float NormalDamage { get; private set; }    // % (일반전)

// 신규 크리티컬
public float MagicCritChance { get; private set; }
public float MagicCritDamage { get; private set; }

// 신규 방어
public float EvasionRate { get; private set; }
public float StatusResist { get; private set; }
public float ArmorIgnore { get; private set; }     // 메이플 방무

// 신규 공격
public int ChainTargets { get; private set; }

// 신규 회복
public float HpRegenPerSec { get; private set; }
public float LifestealRatio { get; private set; }

// 신규 메타
public float CompanionDamage { get; private set; }

// 마법 데미지 채널 (지팡이·룬 무기 전용)
public float MagicDamage { get; private set; }
public float MagicDamageMul { get; private set; }

// L100 cap 폐기 — Level 무한
public int Level { get; private set; } = 1;  // 기존
```

## 6. 캐릭터 별 STR/DEX/INT/LUK 비중 (직업 답습)

| 캐릭터 | 직업 매칭 | 주스탯 mul | 부스탯 mul | 비고 |
|---|---|---|---|---|
| default_boy | 전사 | STR 2.0 | AGI/WIS/SPR 0.5 | 메이플 STR 4× 절반 |
| brave_girl | 궁수 | AGI 2.0 | STR/WIS/SPR 0.5 | 빠름 주스탯 |
| quiet_friend | 마법사 | WIS 2.0 | STR/AGI/SPR 0.5 | 슬기 주스탯 |

기존 1.0 / 1.0 / 1.0 / 1.0 → 직업 차별화. **본 변경은 character_progression v2 와 함께**.

## 7. 데미지 공식 (DamageDealer.EnhanceDamage v2)

```csharp
public static float EnhanceDamage(float baseDamage, EnemyController target = null)
{
    var s = GetStats();
    if (s == null) return baseDamage;

    // 1. base + AttackDamage 합산
    float dmg = baseDamage + s.AttackDamage;

    // 2. 합연산 (Damage%)
    dmg *= 1f + s.AdditiveDamageBonus / 100f;

    // 3. 곱연산 (FinalDamage%)
    dmg *= 1f + s.FinalDamageBonus / 100f;

    // 4. 보스/일반 분기
    bool isBoss = target != null && target.Data != null && target.Data.isBoss;
    if (isBoss) dmg *= 1f + s.BossDamage / 100f;
    else        dmg *= 1f + s.NormalDamage / 100f;

    return dmg;
}
```

**DamageSystem 갱신**:
- ApplyDamage(target, baseDamage):
  - 보스/일반 분기 (target.Data.isBoss)
  - Crit roll
  - Armor reduction (메이플 공식)
  - Mythic 효과
  - Lifesteal 발동

## 8. 마이그레이션 단계

### Sprint 1 — 인프라 (3 commit)
1. **stats_v2_infinite.md** 디자인 (이 문서)
2. PlayerStats v2 — 14 신규 필드 + 무한 레벨 곡선 (chunked)
3. BalanceConfig 갱신 — 레벨업 chunk 곡선

### Sprint 2 — EffectResolver 통합 (2 commit)
4. EffectResolver → PlayerStats 통합 — affix MUL% / FinalDamage% / BossDamage% 등 채널 모두 PlayerStats 갱신
5. AffixDefinition / SubAffixDefinition StatTarget enum 확장 (BossDamage, NormalDamage, EvasionRate, StatusResist, ArmorIgnore, ChainTargets, HpRegen, Lifesteal, CompanionDamage, MagicDamage 등 10 신규)

### Sprint 3 — DamageSystem 갱신 (2 commit)
6. DamageDealer.EnhanceDamage v2 — 보스/일반 분기 + ArmorIgnore + Armor reduction 메이플 공식
7. DamageSystem.DealDamage — Lifesteal 적용 + Mythic 통합

### Sprint 4 — UI (2 commit)
8. CharacterInfoPanel — 26 파생 스탯 표시
9. InventoryDetailView — 신규 affix 라벨 (StatTargetLabel 한국어 추가)

### Sprint 5 — 검증 (1 commit)
10. CurveSimulator — L1000 시뮬 + 데미지 공식 검증 + simulation_report.md 갱신

## 9. UI 변경 (CharacterInfoPanel)

기존 11 파생 → **26 파생** 표시. 카테고리별 분류:
- 데미지 (5) / 크리티컬 (4) / 방어 (5) / 공격 (4) / 회복 (3) / 메타 (5)
- 4 코어 + Level 표시

## 10. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 무한 레벨 → XP 곡선 너무 빠름 (L500 즉시 도달) | L101+ 1.02^(L-100) 곱셈 둔화 |
| 데미지 곱연산 폭주 (FinalDmg 50% × BossDmg 100%) | balance simulation L1000 환경 보스 HP 100M~1B 곡선 |
| affix MUL% 누적 → cap 0% (CritChance 100% 이상) | CritChance Mathf.Clamp(0, 1) |
| 마법 데미지 채널 → 지팡이만 사용 | 무기 attackPattern.useMagic = true 플래그 추가 (옵션) |
| Lifesteal 무한 회복 → 불멸 | cap 50% (메이플 표준) |

## 11. 변경 이력
- 2026-05-03: 초기 작성 (stats_v1 → v2 무한 레벨)

## 12. 다음 단계
1. **본 문서 사용자 검토** + 결정 (Sprint 단계 진행 / 추가 조정)
2. Sprint 1 — PlayerStats v2 + BalanceConfig 곡선 (즉시 가능)
3. 후속 Sprint — UI 갱신 별도

## 13. 참조

### Sources
- [메이플 키우기/시스템 — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%20%ED%82%A4%EC%9A%B0%EA%B8%B0/%EC%8B%9C%EC%8A%A4%ED%85%9C)
- [메이플스토리 데미지 공식 총정리 — fmkorea](https://www.fmkorea.com/8336709224)
- [메이플스토리/스탯 — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%EC%8A%A4%ED%86%A0%EB%A6%AC/%EC%8A%A4%ED%83%AF)
- [전투력과 특수 스탯 — 메이플스토리 공식 가이드](https://maplestory.nexon.com/Guide/N23GameInformation/377406)
- [메이플 키우기 초보자 가이드 — BlueStacks](https://www.bluestacks.com/ko/blog/game-guides/maplestory-idle-rpg/mpsir-beginner-guide-ko.html)
- [메이플 용사단 키우기 : 방치 RPG — 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%20%EC%9A%A9%EC%82%AC%EB%8B%A8%20%ED%82%A4%EC%9A%B0%EA%B8%B0%20:%20%EB%B0%A9%EC%B9%98%20RPG)
