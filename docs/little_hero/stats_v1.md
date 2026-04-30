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

| 스탯 | 단위 | 주 채널 |
|---|---|---|
| 최대 HP | 절대값 | 마음·장비·메타 |
| 공격력 | 절대값 | 힘·장비·메타 |
| 공격 속도 | 배수 | 빠름·장비 |
| 이동 속도 | 절대값 | 빠름·장비 |
| 사거리 | 절대값 | 슬기·장비 |
| 크리티컬 확률 | % | 슬기·장비 |
| 크리티컬 피해 | 배수 | 장비 (+자체 100% 시작) |
| 방어력 | 절대값 | 힘·장비 |
| 데미지 감소 | % | 장비·메타 |
| xp 획득량 | 배수 | 마음·장비 |
| 별가루 획득량 | 배수 | 장비·메타 |

### 데미지 버킷 (Diablo 4 + 메이플 키우기 영향)

> Diablo 4의 5 버킷 모델은 키즈에 복잡 → **3 버킷으로 단순화**. 메이플 키우기의 "데미지% 합산" 패턴 차용.

```
최종 데미지 = 공격력
            × (1 + 합산_데미지%)              ← 버킷 1
            × (1 + 보스_또는_일몹_데미지%)    ← 버킷 2
            × (1 + 크리피해 × 크리확률)        ← 버킷 3
```

각 버킷 안의 옵션은 **합산**(additive), 버킷끼리는 **곱셈**(multiplicative).

#### 버킷별 채널 예시

**버킷 1 (합산 데미지%)** — 모든 옵션이 더해진 후 1번 곱:
- 옵션 라인 "공격력 +5%"
- 신화 옵션 "공격력 +20%"
- 메타 업그레이드 "공격력 강화 Lv 5 = +5"
- 코어 스탯 힘 보너스

**버킷 2 (보스/일반 데미지%)** — 적 종류에 따라 곱:
- 옵션 라인 "보스에게 +10%"
- 메인 보스 vs 일반 적 별도 적용 (메이플 키우기 모델)

**버킷 3 (크리티컬)** — 자체 곱셈:
- 크리피해 1.5x (베이스) + 추가 % = 총 크리피해 배수
- 크리확률 N%
- 합쳐서 `(1 + crit_dmg × crit_chance)`

#### Diablo 4 vs Little Hero 단순화

| 채널 | Diablo 4 | Little Hero |
|---|---|---|
| Additive Damage | ✓ 1 버킷 | ✓ 버킷 1 |
| Multiplicative Damage | ✓ 1 버킷 (xValue%) | ❌ 폐기 — 키즈 복잡 |
| Vulnerable | ✓ 1 버킷 | ❌ 폐기 — 상태 효과 미도입 |
| Critical Damage | ✓ 1 버킷 | ✓ 버킷 3 |
| Attribute Bonus | ✓ 1 버킷 | 버킷 1에 흡수 (코어 스탯 → +Attack/HP 가산) |
| Boss/일몹 분리 | ❌ | ✓ 버킷 2 (메이플 키우기 모델) |

→ 결과: **3 버킷, 키즈가 직관적으로 이해 가능**. 후속 패치에서 버킷 1개 추가 (예: 원소 데미지)는 유연.

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

## 4. 장비 부위별 고유 옵션 매트릭스 (메이플 키우기 차용)

> 같은 슬롯이라도 **권장 옵션이 다름**. 각 슬롯의 정체성을 강화. 옵션 라인 3개 중 일부는 슬롯 고유, 일부는 자유.

### 주 슬롯 7개

| 슬롯 | 1차 옵션 (베이스 스탯 강조) | 2차 옵션 (자주 등장) | 3차 옵션 (가끔) |
|---|---|---|---|
| 모자 | 사거리 | 슬기 +N | 시야 |
| 상의 | 최대 HP | 마음 +N | 동시 공격 수 |
| 하의 | 방어력 | 마음 +N | 이동 속도 |
| 망토 | 회피 % | 데미지 감소 % | 빠름 +N |
| 장갑 | 공격 속도 | 크리티컬 확률 | 빠름 +N |
| 무기 | 공격력 | 크리티컬 피해 | 힘 +N |
| 신발 | 이동 속도 | 빠름 +N | 자취 효과 |

### 장신구 슬롯 5개

| 슬롯 | 1차 옵션 | 2차 옵션 | 3차 옵션 |
|---|---|---|---|
| 귀걸이 | 마음 +N | 최대 HP +% | 자원 회복 |
| 안경 | 명중 (시야) | 슬기 +N | 보스 표시 |
| 목걸이 | 데미지 % | 모든 스탯 +N | xp 획득 +% |
| 반지 | 모든 스탯 +% | 크리티컬 확률 | 별가루 획득 +% |
| 펫 | 자동 회복 | 드랍률 +% | 동행 데미지 |

### 옵션 풀 등급별 분포 (3-티어 정합)

장신구 3-티어와 정합 (chapter_structure_v2 §4):

| 등급 | 옵션 라인 수치 범위 |
|---|---|
| Common (티어 1, 챕터 6~10) | 1차: 작은 값 / 2차·3차: 미약 |
| Uncommon (티어 2, 16~20) | 1차: 중간 / 2차: 작은 / 3차: 미약 |
| Rare (티어 2~3) | 1차: 큰 / 2차: 중간 / 3차: 작은 |
| Epic (티어 3, 26~30) | 1차: 매우 큰 / 2차: 큰 / 3차: 중간 |
| Legendary (시즌·신화 옵션) | 모든 라인 큰 + 신화 옵션 발현 |

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

### `PlayerStats` 확장

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

    // ✦ 신규 — 파생 스탯
    public float CritChance { get; private set; }      // 0.0 ~ 1.0
    public float CritDamage { get; private set; }      // 1.5 = 베이스
    public float Armor { get; private set; }
    public float DamageReduction { get; private set; } // 0.0 ~ 1.0
    public float DustGainMultiplier { get; private set; }

    // 데미지 버킷 % 합산값
    public float AdditiveDamageBonus { get; private set; }    // 버킷 1
    public float BossDamageBonus { get; private set; }        // 버킷 2 (보스 시)
    public float NormalEnemyDamageBonus { get; private set; } // 버킷 2 (일반 시)
}
```

### 데미지 계산 헬퍼

```csharp
public static class DamageCalculator
{
    public static float CalculateDamage(PlayerStats stats, EnemyController target)
    {
        float baseAttack = stats.AttackDamage;

        // 버킷 1: 합산 데미지%
        float bucket1 = 1f + stats.AdditiveDamageBonus;

        // 버킷 2: 보스/일반 분리
        float bucket2 = 1f + (target.Data.isBoss || target.Data.isMiniB
            ? stats.BossDamageBonus
            : stats.NormalEnemyDamageBonus);

        // 버킷 3: 크리티컬 (확률 × 피해 곱셈)
        float bucket3 = 1f + stats.CritChance * (stats.CritDamage - 1f);
        // 또는 실제 발생 시: bucket3_actual = isCrit ? stats.CritDamage : 1f

        return baseAttack * bucket1 * bucket2 * bucket3;
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
│   공격 속도      1.2     (+20%)            │
│   크리티컬       12%  ×  165%              │
│   이동 속도      5.5                       │
│   방어력         24                        │
│                                            │
│   별가루 획득 +10%   xp 획득 +5%          │
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
- 11개 파생 스탯 모두 (HP·공격력·공격속도·이동속도·사거리·크리확률·크리피해·방어력·데미지감소·xp획득·별가루획득)
- 3 데미지 버킷
- 부위별 옵션 매트릭스 (12 슬롯)
- 캐릭터 정보 화면
- 적 스탯 4종 (HP·공격력·이동속도·방어력)

**MVP 제외 (후속)**:
- 원소 데미지 (불·얼음 등) — 4번째 데미지 버킷
- 회피·반사 메커니즘 (망토 슬롯의 옵션은 보유하되 효과 발동은 단순화)
- 상태 효과 (Bleed·Stun) — 비폭력 톤 위반 가능성
- 클래스별 스킬 시스템 — 현재는 자동 사격만

---

## 9. 미해결 질문

- 코어 스탯 100점당 효과 수치 (시뮬 필요)
- 레벨업 시 코어 스탯 +1이 키즈 게임에 충분한가? (Diablo 4도 +1)
- 옵션 라인 1차/2차/3차 비율 (예: 1차 60% / 2차 30% / 3차 10%)
- 부위별 옵션 매트릭스의 정확한 수치
- 캐릭터 클래스 보너스 비중 (1.2/0.9 가설 검증)
- 명중·회피를 도입할지 (현재는 미도입 권장)
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
