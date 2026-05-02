# Little Hero — 캐릭터 진행 v1

> [← Little Hero 인덱스](./README.md) · [장비 v1](./equipment_v1.md) · [스탯 v1](./stats_v1.md) · [옵션 풀 v1](./affix_pool_v1.md) · [챕터 v2](./chapter_structure_v2.md) · [원본 잭팟 Part 1b](../design/part_1b_character_progression.md)
>
> 잭팟 ARPG Part 1b의 결정론적 진행 축을 Little Hero 35 챕터에 매핑. **장비(L4 옵션)·강화(L2~L3)와는 별개의 50% 비중 채널**.

---

## 1. 핵심 결정

- **결정론**: 적 처치·챕터 클리어로 XP 자동 적립 → 레벨업 → 베이스 스탯 자동 +. 잭팟 RNG 없음.
- **레벨 캡**: MVP 30 (챕터 1~10 커버), 정식 100 (35 챕터 + endgame 콘텐츠). L51~L100 = 장기 그라인드 영역.
- **자동 적립**: 별도 화면·버튼 없이 누적. 인지 부담 0.
- **사망 시 XP 후퇴 없음** — 자부심 보호 (Part 1b 정합).
- **장비:캐릭터 = 50:50 비중** — 한쪽 과하면 자부심·빌드 다양성 모두 약해짐.
- **레벨업 시 4 코어 스탯(힘/빠름/슬기/마음) +1씩** (`CharacterData` 비중 적용).
- **레벨업 시 HP +10 / 공격력 +1** (Part 1b 정합).
- **5 레벨마다 부수 보너스** (이동 속도, 크리 확률).
- **레벨업은 도파민**: 별 파티클 + 카운트업 + 따뜻한 종소리 (비폭력 톤).

---

## 2. 설계 원칙

### 2.1. 결정론 vs 잭팟의 분리

| 채널 | 메커니즘 | 비중 |
|---|---|---|
| 캐릭터 진행 (이 문서) | XP → 레벨업 → 결정론 +N | 50% |
| 장비 강화 (L2~L3) | 별가루 → +0~+15 결정론 / +16~+25 잭팟 | 25% |
| 옵션 라인 (L4) | 굴림·리롤 잭팟 | 15% |
| 신화 옵션 (L5) | 마일리지 메가 잭팟 | 10% |

→ **갈면 무조건 오르는 영역(50%)** 이 항상 보장되어 있어야 좌절 차단.

### 2.2. 자동 적립 — UI 부담 0

- HUD 상단에 **얇은 XP 바**(2~3px). 평소엔 거의 안 보임.
- 레벨업 시점에만 **0.5초간 큼지막한 별 + "Lv N!" 텍스트** 노출.
- 메뉴 진입·확인 동작 없음. Brotato·Vampire Survivors 모델.

### 2.3. 챕터 깊이와의 동조

- 챕터 N 진입 시 **권장 레벨 ≈ N + 9** (예: 챕터 1 → L10, 챕터 26 → L35).
- 레벨이 부족해도 강화 +/장비 등급으로 보완 가능 (50:50 균형).
- 권장 레벨보다 5 이상 낮으면 보스 클리어 어려움 → "더 먼저 챕터 도전" UX 힌트.

---

## 3. XP 곡선 ✦ Piecewise (L1~L50 빠름, L51~L100 장기)

### 3.1. 공식

```
L1~L50  : next_level_xp(L) = round(50 × 1.15^L)
L51~L100: next_level_xp(L) = round(L50_value × 1.05^(L - 50))
                            = round(54,208 × 1.05^(L - 50))
```

> L1~L50 은 35 챕터 첫 클리어 페이스(빠른 성장). L51~L100 은 endgame 그라인드 페이스(반복·일일·주간 콘텐츠로 점진).
> 1.15 → 1.05 로 가파름이 1/3 수준으로 완화 — "레벨업 자체보다 장비 강화"로 무게 이동.

### 3.2. 레벨별 요구 XP·누적 XP (L1~L100)

| Level | 다음 레벨 요구 XP | 누적 XP |
|---|---|---|
| L1 → L2 | **50** ★ | 50 |
| L5 → L6 | 101 | 388 |
| L10 → L11 | 202 | 1,168 |
| L20 → L21 | 818 | 5,893 |
| L30 → L31 | 3,313 | 25,001 |
| L40 → L41 | 13,399 | 102,331 |
| L50 → L51 | 54,208 | 415,199 |
| L60 → L61 | 88,313 | 1,107,654 |
| L70 → L71 | 143,866 | 2,236,924 |
| L80 → L81 | 234,375 | 4,076,180 |
| L90 → L91 | 381,805 | 7,073,068 |
| L99 → L100 | 612,419 | 11,395,000 |

★ L1 → L2 는 50으로 하드 코딩 (접근성).

> 누적 XP L100 = ~11.4M XP (35 챕터 첫 클리어 ~400k의 28배). endgame 콘텐츠 6~12개월 분량.

---

## 4. 레벨업 보상 ✦ L51+ 둔화 (장비 중심으로 이동)

### 4.1. L1~L50 자동 증가 (현재 그대로)

| 스탯 | 레벨당 | 비고 |
|---|---|---|
| **힘** (Strength) | +1 × `CharacterData.strengthMultiplier` | 4 코어 |
| **빠름** (Agility) | +1 × `CharacterData.agilityMultiplier` | 4 코어 |
| **슬기** (Wisdom) | +1 × `CharacterData.wisdomMultiplier` | 4 코어 |
| **마음** (Spirit) | +1 × `CharacterData.spiritMultiplier` | 4 코어 |
| 최대 HP | +10 | Part 1b 정합 |
| 공격력 | +1 | Part 1b 정합 |

### 4.2. 5 레벨마다 부수 보너스 (L1~L50)

| 스탯 | 5 레벨당 |
|---|---|
| 이동 속도 | +0.05 |
| 크리티컬 확률 | +1% |

### 4.3. L51~L100 보상 (둔화 — 장비 중심으로 이동)

| 스탯 | 레벨당 (L51+) | 변화 |
|---|---|---|
| **4 코어** (힘/빠름/슬기/마음) | +1 × CharacterData multi | **유지** |
| 최대 HP | +5 (절반) | 10 → 5 |
| 공격력 | +0 (제거) | 1 → 0 |
| 이동 속도 / 크리 확률 | +0 (제거) | 매 5 레벨 보너스 폐지 |

> L51+ 에서는 **장비 강화·옵션 굴림이 주된 성장 채널**. 레벨업은 코어 +1 + HP +5만 (소소).
> "캐릭터는 거의 다 컸다 — 이제 장비를 빛내자" 라는 톤.

### 4.4. 보스에게 받는 데미지 보정 (L51+)

L51~L100 사이의 적은 endgame 콘텐츠로 **장비 정점에 도달한 플레이어**를 가정.
적 stat × 1.10^(N-1) 곡선은 chapter 35 = ×25.55 까지로 충분 — endgame 적은 별도 곡선 (endgame_v1 §3 참조).

### 4.5. 코어 100점당 효과 (stats_v1 §3 재인용)

| 코어 | 100점당 효과 |
|---|---|
| 힘 | 공격력 +10, 방어력 +10 |
| 빠름 | 공격 속도 +5%, 이동 속도 +0.5 |
| 슬기 | 크리티컬 확률 +2%, 사거리 +0.5 |
| 마음 | 최대 HP +50, xp 획득 +5% |

> 레벨 50 시 코어 ≈ 50씩 → 100점당 효과의 약 절반치 적용. 캐릭터 비중에 따라 50~60 사이 변동.

### 4.6. 캐릭터 클래스별 비중 (stats_v1 §3 재인용)

| 캐릭터 | 힘 | 빠름 | 슬기 | 마음 | 정체성 |
|---|---|---|---|---|---|
| 용감한 소년 | × 1.0 | × 1.0 | × 1.0 | × 1.0 | 균형 |
| 씩씩한 소녀 | × 1.2 | × 0.9 | × 0.9 | × 1.2 | 탱커 (HP·공격력↑) |
| 조용한 친구 | × 0.9 | × 1.2 | × 1.2 | × 0.9 | 빠른 공속·크리티컬 |

레벨업 시 `floor(coreLevel × multiplier)` 누적 (소수점 버림 → 정수 절단 방지를 위해 `누적 후 floor`).

---

## 5. XP 획득량

### 5.1. 출처별 베이스 XP ✦ 시뮬 결과 반영 (2026-04-30)

> 초안 5/50/200/N×100 은 시뮬에서 CH 5 클리어 시 L32 도달 (권장 L18 대비 +14 과잉).
> 절반 이하로 축소 → 권장 레벨 매트릭스(§6) 정합.

| 출처 | 베이스 XP | 변경 | 비고 |
|---|---|---|---|
| 일반 적 처치 | **2** | 5 → 2 (40%) | 챕터당 ~1,000마리 가정 |
| 미니보스 (W5/10/15) | **20** | 50 → 20 (40%) | 챕터당 3마리 |
| 메인 보스 (W20) | **80** | 200 → 80 (40%) | 챕터당 1마리 |
| 챕터 클리어 보너스 | **챕터 # × 40** | N×100 → N×40 (40%) | 1회성, 첫 클리어만 |
| 추억(Memory) 발견 | **40** | 100 → 40 (40%) | 1회성, 4종 |

### 5.2. 챕터 깊이 모디파이어

```
enemy_xp_actual = base_xp × (1 + chapter_N × 0.1)
```

| 챕터 N | 모디파이어 | 일반 적 XP | 미니보스 XP | 메인 보스 XP |
|---|---|---|---|---|
| 1 | × 1.1 | 5.5 | 55 | 220 |
| 10 | × 2.0 | 10 | 100 | 400 |
| 20 | × 3.0 | 15 | 150 | 600 |
| 30 | × 4.0 | 20 | 200 | 800 |
| 35 | × 4.5 | 22.5 | 225 | 900 |

> 챕터 깊이가 깊을수록 적도 강하지만(`stat_multiplier`) XP도 비례 증가 → **레벨 곡선이 챕터 곡선에 동조**.

### 5.3. 챕터별 예상 XP 총량 (시뮬)

가정: 챕터당 일반 적 1,000마리 / 미니보스 3 / 메인 보스 1 / 클리어 보너스 1.

```
챕터_XP(N) = 1000 × enemy_xp(N)
           + 3 × miniboss_xp(N)
           + 1 × boss_xp(N)
           + N × 100
```

| 챕터 N | 일반 적 | 미니보스 | 메인 보스 | 클리어 | **총 XP** |
|---|---|---|---|---|---|
| 1 | 5,500 | 165 | 220 | 100 | **5,985** |
| 5 | 7,500 | 225 | 300 | 500 | **8,525** |
| 10 | 10,000 | 300 | 400 | 1,000 | **11,700** |
| 15 | 12,500 | 375 | 500 | 1,500 | **14,875** |
| 20 | 15,000 | 450 | 600 | 2,000 | **18,050** |
| 25 | 17,500 | 525 | 700 | 2,500 | **21,225** |
| 30 | 20,000 | 600 | 800 | 3,000 | **24,400** |
| 35 | 22,500 | 675 | 900 | 3,500 | **27,575** |

35 챕터 첫 클리어 누적 ≈ **560,000 XP** → 레벨 캡 50 (415k 필요)을 넘김 → **여유 있게 캡 도달 가능**.

> 적 마릿수 1,000은 가설. 실제 `WaveManager` 데이터로 검증 필요.

---

## 6. 권장 레벨 매트릭스 (35 챕터)

> 챕터 N 진입 시점의 권장 레벨. 챕터 N 클리어 후 도달 레벨은 **+5~10 정도 상승**한다고 가정.

| 챕터 묶음 | 챕터 # | 진입 권장 Lv | 클리어 후 도달 Lv | 비고 |
|---|---|---|---|---|
| 1 ~ 5 (메타·기억) | 1~5 | L1~L10 | L13~L18 | 진입·정서. 장신구 없음 |
| 6 ~ 10 (티어 1 해방) | 6~10 | L13~L20 | L20~L25 | 첫 장신구 슬롯 해방 |
| 11 ~ 15 (자연 전환) | 11~15 | L20~L25 | L25~L30 | 티어 1 장신구 적응 |
| 16 ~ 20 (티어 2 향상) | 16~20 | L25~L32 | L32~L37 | 향상 장신구 드랍 |
| 21 ~ 25 (동물·문명) | 21~25 | L32~L37 | L37~L42 | 티어 2 적응 |
| 26 ~ 30 (티어 3 정점) | 26~30 | L37~L43 | L43~L47 | 정점 장신구 드랍 |
| 31 ~ 35 (환상 정점) | 31~35 | L43~L48 | L48~L50 (캡) | 마지막 도전 |

**규칙**:
- 챕터 N 진입 권장 ≈ **L(N + 9)** ~ **L(N + 13)** 범위 (캡 50 한계).
- 권장보다 5 이하 낮으면 보스 클리어 부담 → 이전 챕터 반복으로 XP·장비 보강.
- 권장보다 5 이상 높으면 약간의 시너지 손실 (XP 보너스가 모디파이어로 약간만 가산).

> 정확한 보스 HP·공격력은 `chapter_curve_v1.md` (다음 단계)에서 결정.

---

## 7. 데이터 모델

### 7.1. `PlayerProgress` 클래스 (확장)

```csharp
public class PlayerProgress : MonoBehaviour
{
    public int Level { get; private set; }
    public int Xp { get; private set; }
    public int NextLevelXp { get; private set; }

    // 4 코어 누적값 (캐릭터 비중 반영 후)
    public int Strength { get; private set; }
    public int Agility { get; private set; }
    public int Wisdom { get; private set; }
    public int Spirit { get; private set; }

    // 레벨업 직접 +N 누적값
    public float HealthBonusFromLevel { get; private set; }   // +10/Lv
    public float AttackBonusFromLevel { get; private set; }   // +1/Lv
    public float MoveSpeedBonusFromLevel { get; private set; }// +0.05/5Lv
    public float CritChanceBonusFromLevel { get; private set; }// +0.01/5Lv

    public const int LEVEL_CAP_MVP = 30;
    public const int LEVEL_CAP_FULL = 100;          // ✦ 50 → 100 (endgame 포함)
    public const int LEVEL_CURVE_BREAK = 50;        // 곡선 변곡점

    public static int CalcNextLevelXp(int currentLevel)
    {
        if (currentLevel == 1) return 50;
        if (currentLevel <= LEVEL_CURVE_BREAK)
            return Mathf.RoundToInt(50f * Mathf.Pow(1.15f, currentLevel));
        // L51+ 1.05 가파름 (장기 그라인드)
        float at50 = 50f * Mathf.Pow(1.15f, LEVEL_CURVE_BREAK);
        return Mathf.RoundToInt(at50 * Mathf.Pow(1.05f, currentLevel - LEVEL_CURVE_BREAK));
    }
}
```

### 7.2. XP 적립 핸들러

```csharp
// MonoBehaviour 또는 Manager
private void Awake()
{
    GameEvents.OnEnemyKilled += HandleEnemyKilled;
    GameEvents.OnChapterCleared += HandleChapterCleared;
    GameEvents.OnMemoryDiscovered += HandleMemoryDiscovered;
}

void HandleEnemyKilled(EnemyData enemy)
{
    int chapterN = GameManager.Instance.CurrentChapterNumber;
    float modifier = 1f + chapterN * 0.1f;
    int baseXp = enemy.isBoss ? 200 : enemy.isMiniBoss ? 50 : 5;
    int gain = Mathf.RoundToInt(baseXp * modifier);
    AddXp(gain);
}

void HandleChapterCleared(ChapterData chapter)
{
    AddXp(chapter.chapterNumber * 100);
}

void HandleMemoryDiscovered(MemoryData _) => AddXp(100);

void AddXp(int amount)
{
    if (Level >= LEVEL_CAP_FULL) return;
    Xp += amount;
    while (Xp >= NextLevelXp && Level < LEVEL_CAP_FULL)
    {
        Xp -= NextLevelXp;
        Level += 1;
        ApplyLevelUpRewards();
        NextLevelXp = CalcNextLevelXp(Level);
        GameEvents.RaiseLevelUp(Level);
    }
}
```

### 7.3. 레벨업 보상 적용

```csharp
void ApplyLevelUpRewards()
{
    var character = MetaProgression.SelectedCharacter;

    // 4 코어 (누적 후 floor — 소수점 절단 보호)
    Strength = Mathf.FloorToInt(Level * character.strengthMultiplier);
    Agility = Mathf.FloorToInt(Level * character.agilityMultiplier);
    Wisdom = Mathf.FloorToInt(Level * character.wisdomMultiplier);
    Spirit = Mathf.FloorToInt(Level * character.spiritMultiplier);

    // 직접 +N
    HealthBonusFromLevel = Level * 10f;
    AttackBonusFromLevel = Level * 1f;
    MoveSpeedBonusFromLevel = (Level / 5) * 0.05f;
    CritChanceBonusFromLevel = (Level / 5) * 0.01f;
}
```

### 7.4. SaveData 통합

```csharp
[Serializable]
public class SaveData
{
    // ... 기존 필드
    public int level = 1;
    public int xp = 0;
    // 코어 4종은 level + character mult로 항상 재계산되므로 저장 불필요.
}
```

> 사망 시 XP 후퇴 없음 — `OnPlayerDied` 핸들러에서 `level/xp` 미터치.

---

## 8. UI 노출

### 8.1. HUD XP 바 (인게임)

```
┌──────────────────────────────────────────────────────┐
│ ❤❤❤❤❤❤❤❤❤❤  220/220                                  │
│ ▮▮▮▮▮▮▮▯▯▯▯▯  Lv 12  ※ XP 165 / 268                  │
│                                                       │
│ 별가루 ★ 47       웨이브 13/20                         │
└──────────────────────────────────────────────────────┘
```

- 얇은 회색 바 + 채워질 때만 파스텔 노랑.
- 평소엔 시야 방해 X. 레벨업 직전 1~2초만 깜빡임.

### 8.2. 레벨업 연출 (0.8초)

| 시점 | 시각 | 사운드 |
|---|---|---|
| 0.0s | 화면 중앙 "Lv 12 → 13!" 큰 텍스트 | "쨍-" 따뜻한 종 |
| 0.1s | 별 파티클 16개 폭발 | |
| 0.3s | 코어 스탯 카운트업 (힘 11→12 등) | "또르륵" 카운트 사운드 |
| 0.6s | 텍스트 페이드아웃 | |
| 0.8s | HUD XP 바 다음 레벨 요구치로 갱신 | |

> 게임 진행은 멈추지 않음 (0.8초 이펙트 동안 자동 공격 계속).

### 8.3. 캐릭터 정보 화면 (다락방, stats_v1 §7 정합)

```
┌── 작은 영웅 (용감한 소년 Lv 12) ──────────┐
│                                            │
│   힘     ▮▮▮▮▮▮▮▮▮▮▮▮  12 (+0 클래스)    │
│   빠름   ▮▮▮▮▮▮▮▮▮▮▮▮  12                │
│   슬기   ▮▮▮▮▮▮▮▮▮▮▮▮  12                │
│   마음   ▮▮▮▮▮▮▮▮▮▮▮▮  12                │
│                                            │
│   ──────────────────────                   │
│                                            │
│   체력           340 / 340  (+120 레벨)    │
│   공격력         145    (+12 레벨, +33 장비) │
│   ...                                      │
│                                            │
│   다음 레벨까지: ▮▮▮▮▮▮▮▯▯▯▯▯ 165 / 268    │
└────────────────────────────────────────────┘
```

- 레벨 출처와 장비 출처를 색으로 구분 (레벨 = 청록, 장비 = 보라).

---

## 9. MVP 범위

### 9.1. MVP 포함

- L1 ~ L30 (챕터 1~10 커버)
- 4 코어 자동 +1 + 캐릭터 비중 (3 클래스)
- 매 레벨 HP +10 / 공격력 +1
- 5 레벨마다 이속 +0.05 / 크리 +1%
- XP 곡선 50 × 1.15^L
- HUD XP 바 + 레벨업 0.8초 연출
- 캐릭터 정보 화면 (Lv·코어·다음 레벨까지)
- SaveData 통합 (사망 시 후퇴 없음)

### 9.2. MVP 제외 (후속)

- L31 ~ L50 (챕터 11~35 도달 시 도입)
- 시즌 캡 +5
- 칭호·도감·펫 잠금 해제 (특정 레벨 보상)
- 클래스 4번째 (균형 외 선택지 확장)
- 레벨업 시 슬롯 추가·기능 해제 (현재 모두 자동)
- 권장 레벨 미달 시 UX 힌트 (난이도 경고)

---

## 10. 미해결 질문

- L1 → L2 의 50/58 표시 일관성 (현재 50으로 하드 코딩, 공식상 58)
- XP 곡선 1.15 계수가 35 챕터에 적절한가 (시뮬 필요)
- 챕터 1,000마리 가정의 검증 (실제 `WaveManager.totalEnemiesPerWave` 측정)
- 레벨업 시 자동 공격 일시 중지 vs 계속 (현재: 계속)
- 캐릭터 50 : 장비 50 비중 검증 (시뮬레이터 빌드)
- 추억 발견 100 XP가 적절한지 (1회성이라 영향 작음)
- 사망 시 XP 패널티 도입 가능성 (현재: 없음, Part 1b 정합)
- 레벨업 효과음·파티클의 정확한 에셋
- 권장 레벨 미달 시 UX 처리 (자동 추천 챕터 제시?)
- 클래스 비중 1.2/0.9가 빌드 다양성에 충분한가 (1.5/0.7 등 극단값 검토)

---

## 11. 다음 단계

1. **`PlayerProgress` MonoBehaviour 구현** — XP 적립 + 레벨업 트랜잭션 + EventBus 통합
2. **`GameEvents.OnLevelUp` 활성화** — 기존 `OnLevelUp(int)` 이벤트 핸들러 연결
3. **HUD XP 바 + 레벨업 연출** — 별 파티클 + 카운트업 + 사운드
4. **캐릭터 정보 화면 (다락방)** — stats_v1 §7 와이어프레임 구현
5. **시뮬레이터** — 35 챕터 진행 시 평균 도달 레벨, 50:50 비중 검증
6. **`chapter_curve_v1.md` 작성** — 적 HP/공격력 곡선이 본 권장 레벨 매트릭스를 따라가는지 검증
7. **`CharacterData` 비중 필드 추가** — `strengthMultiplier`/`agilityMultiplier`/`wisdomMultiplier`/`spiritMultiplier`

---

## 12. 출처

- 잭팟 ARPG [Part 1b — 캐릭터 진행](../design/part_1b_character_progression.md) (XP 공식·트랜잭션·50:50 비중 가이드)
- [stats_v1 §3](./stats_v1.md) (4 코어 / 코어 100점당 효과 / 클래스 비중)
- [equipment_v1 §3.7](./equipment_v1.md) (35 챕터 3-티어 정합)
- [chapter_structure_v2 §5](./chapter_structure_v2.md) (적 stat_multiplier 곡선)
