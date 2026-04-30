# Little Hero — 능력치 시스템 v1

> [← Little Hero 인덱스](./README.md) · [장비 v1](./equipment_v1.md) · [챕터 v2](./chapter_structure_v2.md)
>
> **Diablo 4의 4 코어 스탯 + 데미지 버킷 모델 + 메이플 키우기의 데미지 공식·부위별 고유 옵션**을 채용. 키즈 친화 톤으로 단순화 + 한글 스탯명.

---

## 1. 핵심 결정

### 4 코어 스탯 (캐릭터 레벨업 시 자동 +1씩)

> Diablo 4의 4 코어 스탯(STR/DEX/INT/WIL) 모델 차용. 키즈 친화 한국어 명칭으로 변환.

| 영문 | 한국어 (UI 노출) | 주 효과 |
|---|---|---|
| Strength | **힘** | 공격력 + 방어력 |
| Agility | **빠름** | 공격 속도 + 이동 속도 |
| Wisdom | **슬기** | 크리티컬 확률 + 사거리 |
| Spirit | **마음** | 최대 HP + xp 획득량 |

캐릭터 레벨업 시 **4 스탯 모두 +1씩** 자동 증가 (Diablo 4 패턴). 클래스/캐릭터마다 보너스 비중이 다를 수 있음 (`CharacterData`).

### 파생 스탯 (코어 + 장비 + 메타 업그레이드 합산)

> ✦ 메이플 키우기 옵션 채용. **단, 스킬 시스템(스킬 레벨·스킬 쿨감)은 미채용** — Little Hero는 자동 공격(`IAttackPattern` 틱) 모델이라 액티브 스킬 추상화 불필요. 명중·회피 제거. 보스/일몹 데미지 분리 제거.

| 스탯 | 단위 | 주 채널 |
|---|---|---|
| 최대 HP | 절대값 | 마음·장비·메타 |
| 공격력 | 절대값 | 힘·장비·메타 |
| 공격 속도 | 배수 | 빠름·장비·**모자 고유** |
| 이동 속도 | 절대값 | 빠름·장비 |
| 사거리 | 절대값 | 슬기·장비 |
| 크리티컬 확률 | % | 슬기·장비 |
| 크리티컬 피해 | 배수 | 장갑 고유 (+자체 100% 시작) |
| **최종 데미지** ✦ | % (곱) | 망토·하의·안경 고유 (메이플 키우기 핵심 채널) |
| **공격 대상 수** ✦ | 절대값 | 상의 고유 (Projectile count 등) |
| **방어 관통력** ✦ | % | 무기 고유 (적 방어력 무시) |
| 방어력 | 절대값 | 힘·장비 |
| 데미지 감소 | % | 장비·메타 |
| xp 획득량 | 배수 | 마음·장비 |
| 별가루 획득량 | 배수 | 장비·메타 |
| 동행 지속시간 ✦ | % | 신발·펫 (Summon/Orbit 패턴) |
| 드랍률 | % | 펫 |

### 데미지 버킷 ✦ 메이플 키우기 옵션 완전 채용

> Diablo 4의 5 버킷 모델은 키즈에 복잡 → **3 버킷으로 단순화**. 보스/일몹 분리는 폐기, 대신 **메이플 키우기의 "최종 데미지%"** 채널 채용.

```
최종 데미지 = 공격력
            × (1 + 합산_데미지%)              ← 버킷 1 (additive)
            × (1 + 최종_데미지%)              ← 버킷 2 (메이플 키우기 핵심)
            × (1 + 크리피해 × 크리확률)        ← 버킷 3 (크리티컬)
```

각 버킷 안의 옵션은 **합산**(additive), 버킷끼리는 **곱셈**(multiplicative).

#### 버킷별 채널 예시

**버킷 1 (합산 데미지%)** — 옵션 라인·신화·메타 업그레이드의 일반 + 데미지%:
- 옵션 라인 "공격력 +5%"
- 신화 옵션 "공격력 +20%"
- 메타 업그레이드 "공격력 강화 Lv 5 = +5"
- 코어 스탯 힘 보너스 (코어 → 공격력 가산)
- 무기·반지 등의 일반 데미지%

**버킷 2 (최종 데미지%)** — 메이플 키우기의 가장 강력한 채널:
- **망토** 고유: "최종 데미지 +%" (Epic 이상)
- **하의** 고유: "최종 데미지 +%" (Epic 이상)
- **안경** 고유: "최종 데미지 +%" (Epic 이상, 메이플 키우기 얼굴장식 매칭)
- 신화 옵션의 "최종 데미지" 류
- 두 번째 곱연산 채널이 빌드 차별화의 핵심

**버킷 3 (크리티컬)** — 자체 곱셈:
- 크리피해 1.5x (베이스) + **장갑** 고유 "크리티컬 데미지" 추가 %
- 크리확률 N% (장갑·반지 옵션)
- `(1 + crit_dmg × crit_chance)`

#### Diablo 4 vs 메이플 키우기 vs Little Hero

| 채널 | Diablo 4 | 메이플 키우기 | Little Hero |
|---|---|---|---|
| Additive Damage | ✓ 1 버킷 | ✓ "데미지%" 합산 | ✓ 버킷 1 |
| Multiplicative Damage | ✓ 1 버킷 | ✓ "최종 데미지%" 별도 곱 | ✓ 버킷 2 (메이플 키우기 모델 채용) |
| Vulnerable | ✓ 1 버킷 | ❌ | ❌ 폐기 |
| Critical Damage | ✓ 1 버킷 | ✓ (크리피해×크확) | ✓ 버킷 3 |
| Attribute Bonus | ✓ 1 버킷 | ❌ | 버킷 1에 흡수 |
| Boss/일몹 분리 | ❌ | ✓ (보공·일몹뎀) | ❌ ✦ **제거 (사용자 결정)** |
| 명중/회피 | ❌ | ✓ (PvP·아레나) | ❌ ✦ **제거 (사용자 결정)** |
| 스킬 레벨 | ❌ | ✓ (스킬계수 곱) | ❌ ✦ **제거 (사용자 결정)** — 자동공격 모델, 액티브 스킬 미도입 |
| 스킬 쿨감 | ❌ | ✓ (모자 고유) | ❌ ✦ **제거** — 모자는 "공격 속도 +%"로 대체 |

→ 결과: **3 버킷, 메이플 키우기의 "최종 데미지%" 채널이 빌드 시너지의 핵심**. 망토+하의+안경 트리오의 시너지가 자연스럽게 나옴 (세트 효과 없이도). **스킬 시스템 미도입** — Brotato/Vampire Survivors처럼 자동공격(`IAttackPattern` 틱) 모델이라 액티브 스킬·쿨다운 추상화 불필요.

---

## 2. 설계 원칙

### 키즈 친화 단순화

- 스탯 이름은 **한글 한 음절~두 음절** ("힘"·"빠름"·"슬기"·"마음")
- 숫자는 절대값 + 직관적 % (Diablo 4의 `Value%` vs `xValue%` 구분 단순화)
- 계산 공식은 노출하지 않고 **결과 스탯만 카드에 표시** (UI에서 "공격력 145 → +8" 같은 변화량 강조)

### 메이플 키우기 차용

- **부위별 고유 옵션** — 같은 옵션이 어떤 슬롯에서는 더 잘 등장 (§4 매트릭스)
- **곱연산 버킷 + 합연산 안** — 데미지 공식의 핵심 패턴
- **장비 슬롯 강화** (전체 슬롯이 함께 강화되는 모델은 후속 패치 선택)

### Diablo 4 차용

- **4 코어 스탯, 매 레벨 +1씩** — 캐릭터 성장이 결정론적
- **버킷 분리** — 옵션 별 영향이 다름 (합산 vs 곱셈)
- **크리티컬 상시 곱연산** — 잭팟 도파민 채널

### 절대 가져오지 않을 것

- ❌ Diablo 4의 Resistance 체계 (Fire/Cold/Lightning/Poison/Shadow) — 키즈에 과복잡
- ❌ 메이플 키우기의 명중·회피 (PvP 콘텐츠 전제) — 단일플레이라 불필요
- ❌ 상태 효과 도트(Bleed/Poison) — 비폭력 톤 위반

---

## 3. 캐릭터 레벨업 곡선 (Part 1b + 새 코어 스탯)

### 레벨업 시 자동 증가

| 스탯 | 레벨당 증가 | 비고 |
|---|---|---|
| 최대 HP | +10 | (Part 1b 정합) |
| 공격력 | +1 | (Part 1b 정합) |
| 이동 속도 | +0.05 | 5 레벨마다 |
| 크리티컬 확률 | +1% | 5 레벨마다 |
| **힘** | +1 | ✦ 신규 (4 코어) |
| **빠름** | +1 | ✦ 신규 |
| **슬기** | +1 | ✦ 신규 |
| **마음** | +1 | ✦ 신규 |

### 코어 스탯의 파생 효과

| 코어 | 100점당 효과 |
|---|---|
| 힘 | 공격력 +10, 방어력 +10 |
| 빠름 | 공격 속도 +5%, 이동 속도 +0.5 |
| 슬기 | 크리티컬 확률 +2%, 사거리 +0.5 |
| 마음 | 최대 HP +50, xp 획득 +5% |

(레벨 50 시 코어 스탯이 50씩 → 추가 보너스 약 절반치)

> 위 수치는 시작점 가설. 시뮬·플레이 테스트로 보정.

### 캐릭터 클래스별 보너스 비중 (CharacterData)

기존 캐릭터 3종에 매핑:

| 캐릭터 | 힘 | 빠름 | 슬기 | 마음 | 컨셉 |
|---|---|---|---|---|---|
| 용감한 소년 | × 1.0 | × 1.0 | × 1.0 | × 1.0 | 균형 |
| 씩씩한 소녀 | × 1.2 | × 0.9 | × 0.9 | × 1.2 | 탱커 (HP·공격력↑) |
| 조용한 친구 | × 0.9 | × 1.2 | × 1.2 | × 0.9 | 빠른 공속·크리티컬 |

레벨업 시 `+1 × 배수`로 코어 스탯이 증가 (소수점은 누적·반올림).

---

## 4. 장비 부위별 고유 옵션 매트릭스 ✦ 메이플 키우기 채용 (스킬 제외)

> 메이플 키우기의 부위별 고유옵션을 매핑. **명중·회피 제거**, **보스데미지/일몹데미지 제거**, **스킬 레벨·스킬 쿨감 제거** (Little Hero는 자동공격 모델). Little Hero 12 슬롯에 맞춰 어깨/벨트는 폐기, 펫 슬롯은 신규.

### 주 슬롯 7개 — 메이플 키우기 매칭

| Little Hero 슬롯 | 메이플 키우기 매칭 | **고유 옵션 (1차)** | 2차 옵션 | 3차 옵션 |
|---|---|---|---|---|
| 모자 | 모자(투구) | **공격 속도 +%** | 슬기 +N | 사거리 |
| 상의 | 상의 | **공격 대상 수 +N** | 마음 +N | 최대 HP +% |
| 하의 | 하의 | **최종 데미지 +%** ★ | 마음 +N | 방어력 |
| 망토 | 망토 | **최종 데미지 +%** ★ | 데미지 감소 % | 빠름 +N |
| 장갑 | 장갑 | **크리티컬 피해 +%** ★ | 크리티컬 확률 | 빠름 +N |
| 무기 | 무기 | **공격력 +%** | 힘 +N | 방어 관통력 % |
| 신발 | 신발 | **이동 속도 / 동행 지속시간 +%** | 빠름 +N | 자취 효과 |

★ = 메이플 키우기 우선 큐브 4부위 (망토·하의·장갑·반지) 중 3개. 스펙업 핵심 부위.

> 모자: 메이플 키우기 원본은 "스킬 쿨감"이 핵심. Little Hero는 액티브 스킬 미도입이라 **공격 속도 +%**로 대체 — `IAttackPattern.attackInterval` 단축 효과는 동일.

### 장신구 슬롯 5개 — 메이플 키우기 매칭

| Little Hero 슬롯 | 메이플 키우기 매칭 | **고유 옵션 (1차)** | 2차 옵션 | 3차 옵션 |
|---|---|---|---|---|
| 귀걸이 | 귀걸이 | **최대 HP +%** | 마음 +N | 자원 회복 |
| 안경 | 얼굴장식 | **최종 데미지 +%** ★ | 슬기 +N | 시야 확장 |
| 목걸이 | 펜던트 | **데미지 +%** | 모든 스탯 +N | xp 획득 +% |
| 반지 | 반지 | **모든 스탯 +N** | 크리티컬 확률 | 별가루 획득 +% |
| 펫 | 동료/펫 | **동행 지속시간 +%** | 드랍률 +% | 자동 회복 |

★ = 메이플 키우기 우선 큐브 핵심 옵션(최종 데미지) 매칭.

> 목걸이/반지: 메이플 키우기 원본은 "스킬 레벨 +1"이 핵심. Little Hero는 자동공격이라 스킬 추상화 없음 → 데미지 % 와 모든 스탯 +N 으로 대체. 결과적으로 **버킷 1(데미지%) 강화 슬롯**으로 정체성 부여.

### 메이플 키우기 옵션 → Little Hero 메커닉 매핑

| 메이플 키우기 옵션 | Little Hero 적용 |
|---|---|
| **최종 데미지 +%** | 데미지 버킷 2 곱연산 (망토·하의·안경) |
| **크리티컬 피해 +%** | 버킷 3에 가산 (장갑) |
| **공격 속도 +%** | `IAttackPattern.attackConfig.attackInterval` 단축 (모자·빠름 코어) |
| **공격 대상 수 +N** | `ProjectileAttack.count` 또는 `OrbitAttack.count` 가산 (상의) |
| **방어 관통력 %** | 적 방어력에 (1 - penetration) 적용 (무기) |
| **동행 지속시간 +%** | `SummonAttack`/`OrbitAttack` 동료의 lifetime 가산 (신발·펫, ※ 영구 동행 시 이펙트 지속시간으로 대체 가능) |
| **드랍률 +%** | 적 처치 시 별가루·xp·장신구 드랍 배수 (펫) |
| 스킬 레벨 +1 | ❌ **제거 (사용자 결정)** — 자동공격 모델, 스킬 추상화 미도입 |
| 스킬 쿨감 % | ❌ **제거** — 공격 속도 %로 대체 |
| 보스 데미지 +% | ❌ **제거 (사용자 결정)** |
| 일몹 데미지 +% | ❌ **제거 (사용자 결정)** |
| 명중·회피 | ❌ **제거 (사용자 결정)** |

### 옵션 풀 등급별 분포 (3-티어 정합)

장신구 3-티어와 정합 (chapter_structure_v2 §4):

| 등급 | 1차 옵션 | 2차 옵션 | 3차 옵션 | 비고 |
|---|---|---|---|---|
| Common (티어 1, 챕터 6~10) | 작은 값 (예: 최종뎀 +1%) | 미약 | 미약 | 슬롯 첫 해방 |
| Uncommon (티어 2, 16~20) | 중간 (최종뎀 +3%) | 작은 | 미약 | |
| Rare (티어 2~3) | 큰 (최종뎀 +5%) | 중간 | 작은 | |
| Epic (티어 3, 26~30) | 매우 큰 (최종뎀 +8%) | 큰 | 중간 | 메이플 키우기 "에픽 이상부터 핵심 고유옵션" 패턴 |
| Legendary | 정점 (최종뎀 +12%) + 신화 옵션 발현 | 큰 | 큰 | |

> 메이플 키우기 패턴: **에픽 이상부터 고유옵션의 위력이 비약적**으로 상승. 1차 옵션은 등급에 따라 거의 지수적으로 증가.

---

## 5. 적 스탯 (단순화)

키즈 게임이니 적 스탯은 최소 4종.

| 스탯 | 설명 |
|---|---|
| 최대 HP | 처치 난이도 |
| 공격력 | 플레이어가 받는 피해 |
| 이동 속도 | 추적 속도 |
| 방어력 | 데미지 감소 (선택, 보스만) |

**적 종류별 가중치**:
- 일반: 베이스 (챕터 N의 stat_multiplier)
- 미니보스: 베이스 × 2~3
- 메인 보스: 베이스 × 5~10

(chapter_structure_v2 §5 곡선과 정합)

---

## 6. 데이터 모델

### `PlayerStats` 확장 ✦ 메이플 키우기 옵션 반영

```csharp
public class PlayerStats : MonoBehaviour
{
    // 기존 필드 (유지)
    public float MaxHealth { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackDamage { get; private set; }
    public float AttackSpeed { get; private set; }
    public float Range { get; private set; }
    public float XpGainMultiplier { get; private set; }

    // ✦ 신규 — 코어 스탯
    public int Strength { get; private set; }
    public int Agility { get; private set; }
    public int Wisdom { get; private set; }
    public int Spirit { get; private set; }

    // ✦ 신규 — 파생 스탯 (메이플 키우기 채용)
    public float CritChance { get; private set; }      // 0.0 ~ 1.0
    public float CritDamage { get; private set; }      // 1.5 = 베이스
    public float Armor { get; private set; }
    public float DamageReduction { get; private set; } // 0.0 ~ 1.0
    public float DustGainMultiplier { get; private set; }
    public float DropRateMultiplier { get; private set; }     // 펫 고유

    // ✦ 메이플 키우기 핵심 채널 (스킬 시스템 제외)
    public float FinalDamageBonus { get; private set; }       // 망토·하의·안경 (버킷 2)
    public int   ExtraAttackTargets { get; private set; }     // 상의 (Projectile.count 가산)
    public float ArmorPenetration { get; private set; }       // 무기 (적 방어력 무시)
    public float CompanionDurationBonus { get; private set; } // 신발·펫 (Summon/Orbit lifetime)
    // SkillLevelBonus, SkillCooldownReduction 폐기 — 자동공격 모델, 공격 속도/공격력%로 흡수

    // 데미지 버킷 % 합산값
    public float AdditiveDamageBonus { get; private set; }    // 버킷 1
    // BossDamageBonus, NormalEnemyDamageBonus 폐기 (사용자 결정)
}
```

### 데미지 계산 헬퍼

```csharp
public static class DamageCalculator
{
    public static float CalculateDamage(PlayerStats stats, EnemyController target, AttackPatternConfig pattern)
    {
        // 버킷 1: 합산 데미지%
        float bucket1 = 1f + stats.AdditiveDamageBonus;

        // 버킷 2: 최종 데미지% (메이플 키우기 핵심)
        float bucket2 = 1f + stats.FinalDamageBonus;

        // 버킷 3: 크리티컬 (확률 × 피해 곱셈)
        float bucket3 = 1f + stats.CritChance * (stats.CritDamage - 1f);

        float dmg = stats.AttackDamage * bucket1 * bucket2 * bucket3;

        // 적 방어력 적용 (방어 관통력 반영)
        float effectiveArmor = target.Data.armor * (1f - stats.ArmorPenetration);
        dmg = dmg / (1f + effectiveArmor / 100f);

        return dmg;
    }
}
```

### 패턴 연동 (공격 속도·공격 대상 수)

```csharp
// IAttackPattern 구현 시 PlayerStats 참조
public class ProjectileAttack : IAttackPattern
{
    public void Tick(float deltaTime)
    {
        // 공격 속도% (모자 고유 + 빠름 코어)는 attackInterval 직접 단축
        float interval = _config.attackInterval / Mathf.Max(0.1f, _stats.AttackSpeed);
        _timer += deltaTime;
        if (_timer < interval) return;
        // ...
        int count = _config.count + _stats.ExtraAttackTargets;  // 상의 고유 옵션 가산
        // ...
    }
}
```

### `CharacterData` 코어 스탯 비중

```csharp
public class CharacterData : ScriptableObject {
    // ... 기존 필드
    [Header("코어 스탯 비중")]
    public float strengthMultiplier = 1.0f;
    public float agilityMultiplier = 1.0f;
    public float wisdomMultiplier = 1.0f;
    public float spiritMultiplier = 1.0f;
}
```

---

## 7. UI 노출 가이드 (Part 5 정합)

캐릭터 정보 화면 (다락방 또는 인벤토리):

```
┌── 작은 영웅 (용감한 소년 Lv 12) ──────────┐
│                                            │
│   힘     ▮▮▮▮▮▮▮▮▮▮▮▮  12               │
│   빠름   ▮▮▮▮▮▮▮▮▮▮▮▮  12               │
│   슬기   ▮▮▮▮▮▮▮▮▮▮▮▮  12               │
│   마음   ▮▮▮▮▮▮▮▮▮▮▮▮  12               │
│                                            │
│   ──────────────────────                   │
│                                            │
│   체력           220 / 220                 │
│   공격력         145    (+45 장비)         │
│   공격 속도      1.2     (+20%, 모자)      │
│   크리티컬       12%  ×  165%              │
│   최종 데미지    +18%   (망토·하의·안경)   │
│   공격 대상 수   2       (기본+1, 상의)    │
│   이동 속도      5.5                       │
│   방어력         24                        │
│   방어 관통력    12%    (무기)             │
│                                            │
│   별가루 획득 +10%   xp 획득 +5%   드랍률 +20% │
└────────────────────────────────────────────┘
```

핵심:
- 코어 스탯 4개를 **최상단에** 큼지막하게 (자부심·캐릭터 정체성)
- 파생 스탯은 그 아래 일반 텍스트
- 변화량(`+N` 또는 `+N%`)은 다른 색으로 강조 (장비 효과·메타 업그레이드 출처 구분)

---

## 8. MVP 범위

**MVP 포함**:
- 4 코어 스탯 + 레벨업 자동 증가
- 파생 스탯 (HP·공격력·공격속도·이동속도·사거리·크리확률·크리피해·방어력·데미지감소·xp획득·별가루획득·**최종 데미지%·공격 대상 수·방어 관통력·동행 지속시간·드랍률**)
- 3 데미지 버킷 (합산 / 최종 데미지% / 크리티컬)
- 부위별 옵션 매트릭스 (12 슬롯)
- 캐릭터 정보 화면
- 적 스탯 4종 (HP·공격력·이동속도·방어력)

**MVP 제외 (후속)**:
- 원소 데미지 (불·얼음 등) — 4번째 데미지 버킷
- 회피·반사 메커니즘 (망토 슬롯의 옵션은 보유하되 효과 발동은 단순화)
- 상태 효과 (Bleed·Stun) — 비폭력 톤 위반 가능성
- **액티브 스킬 시스템 (스킬 레벨·쿨다운·쿨감)** — 자동 사격 모델 유지, 빌드 깊이는 12 슬롯·옵션·강화로 확보

---

## 9. 미해결 질문

- 코어 스탯 100점당 효과 수치 (시뮬 필요)
- 레벨업 시 코어 스탯 +1이 키즈 게임에 충분한가? (Diablo 4도 +1)
- 옵션 라인 1차/2차/3차 비율 (예: 1차 60% / 2차 30% / 3차 10%)
- 등급별 1차 옵션 정확한 수치 (Common 최종뎀+1% / Epic 최종뎀+8% 등)
- 캐릭터 클래스 보너스 비중 (1.2/0.9 가설 검증)
- ✅ 명중·회피 → **제거 결정 (사용자)**
- ✅ 보스/일몹 데미지 분리 → **제거 결정 (사용자)**
- ✅ 메이플 키우기 옵션 → **부분 채용 결정 (사용자)** — 스킬 레벨/쿨감 제외
- ✅ 스킬 시스템(스킬 레벨·스킬 쿨감) → **미채용 결정 (사용자)** — Brotato/VS 스타일 자동공격, 빌드 깊이는 12 슬롯·옵션·강화로 확보
- 공격 대상 수 상한 (모든 적 동시 공격 방지)
- 공격 속도 상한 (`attackInterval` 최소값, 예: 0.05초)
- 동행 지속시간 — Little Hero는 영구 동행이라 의미 약함 (펫 이펙트 지속시간으로 대체?)
- 상태 효과 도입 가능성 (비폭력 톤 호환)

---

## 10. 다음 단계

1. **PlayerStats 클래스 확장** — 4 코어 + 11 파생 + 3 버킷
2. **DamageCalculator 헬퍼** — 3 버킷 곱셈 로직
3. **EnemyData에 isMiniB·isBoss 활용** — 보스 데미지 분리 (이미 있음)
4. **부위별 옵션 매트릭스 → Affix 풀 SO** (옵션 풀 카탈로그)
5. **캐릭터 정보 UI 화면** — 코어 스탯 4 강조 디자인
6. **시뮬레이터** — 코어 스탯 100점당 효과·옵션 라인 비율 검증
7. **CharacterData 비중 필드 추가** — 클래스별 강점

---

## 11. 검색 출처

### Diablo 4
- [Stats & Attributes | Diablo 4 Wiki Fextralife](https://diablo4.wiki.fextralife.com/Stats+&+Attributes)
- [Stats for Beginners Diablo 4 - Maxroll.gg](https://maxroll.gg/d4/getting-started/stats-for-beginners)
- [In-Depth Damage Guide in Diablo 4 - Maxroll.gg](https://maxroll.gg/d4/resources/in-depth-damage-guide)
- [Diablo 4 Damage Buckets Explained - Mobalytics](https://mobalytics.gg/diablo-4/guides/damage-buckets-deep-dive)
- [Damage Strategies Guide - Wowhead](https://www.wowhead.com/diablo-4/guide/gameplay/damage-buckets)
- [Defense Guide: Toughness, Armor, Resistances - Wowhead](https://www.wowhead.com/diablo-4/guide/gear/defenses-armor-elemental-resistance-damage-reduction)

### 메이플 키우기 / 메이플스토리
- [메이플 키우기/시스템 - 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%20%ED%82%A4%EC%9A%B0%EA%B8%B0/%EC%8B%9C%EC%8A%A4%ED%85%9C)
- [메이플스토리/스탯 - 나무위키](https://namu.wiki/w/%EB%A9%94%EC%9D%B4%ED%94%8C%EC%8A%A4%ED%86%A0%EB%A6%AC/%EC%8A%A4%ED%83%AF)
- [메이플 키우기 데미지 계산기 - 게임 팁 저장소](https://game.savetip.co.kr/maple-idldamage-calculator/)
- [(분석) 스탯 효율 분석 - 메이플 키우기 마이너 갤러리](https://m.dcinside.com/board/maplerpg/138389)
- [메이플 키우기 잠재능력 부위별 고유옵션 - 게임 팁 저장소](https://game.savetip.co.kr/%EB%A9%94%EC%9D%B4%ED%94%8C-%ED%82%A4%EC%9A%B0%EA%B8%B0-%EC%9E%A0%EC%9E%AC%EB%8A%A5%EB%A0%A5-%EB%B6%80%EC%9C%84%EB%B3%84-%EA%B3%A0%EC%9C%A0%EC%98%B5%EC%85%98-%EC%99%84%EB%B2%BD-%EA%B3%B5%EB%9E%B5/)
- [전투력과 특수 스탯 - 메이플스토리 가이드](https://maplestory.nexon.com/Guide/N23GameInformation/377406)
- [잠재능력/옵션 목록 - 나무위키](https://namu.wiki/w/%EC%9E%A0%EC%9E%AC%EB%8A%A5%EB%A0%A5/%EC%98%B5%EC%85%98%20%EB%AA%A9%EB%A1%9D)
- [잠재 부위별 고유옵션 정보 - 메이플 키우기 마이너 갤러리](https://gall.dcinside.com/mgallery/board/view/?id=maplerpg&no=26893)
- [어떤장비가 더 쌔지는지 구별법이랑 데미지관련팁 - 메이플 키우기 갤러리](https://m.dcinside.com/board/maplerpg/47961)
