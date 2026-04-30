# Little Hero — 챕터 곡선·동조 검증 v1

> [← Little Hero 인덱스](./README.md) · [장비 v1](./equipment_v1.md) · [스탯 v1](./stats_v1.md) · [옵션 풀 v1](./affix_pool_v1.md) · [캐릭터 진행 v1](./character_progression_v1.md) · [챕터 v2](./chapter_structure_v2.md)
>
> 35 챕터 적 스탯 곡선과 플레이어 DPS·생존 곡선을 정합 검증. **chapter_structure_v2 §5의 placeholder 곡선을 본 문서가 대체**한다.

---

## 1. 핵심 결정

- **적 스탯 곱셈**: `multiplier(N) = 1.10^(N-1)` — 챕터 1을 베이스로 챕터 35 = ×25.55.
- **보스/미니/일반 비율은 EnemyData SO에 내장** — 챕터별 곱셈은 한 채널만 (`ChapterData.enemyStatMultiplier`).
- **TTK(Time-to-kill) 목표**:
  - 일반 적: **0.5 ~ 2초** (런타임에 우글거림 유지)
  - 미니보스 (W5/10/15): **5 ~ 15초**
  - 메인 보스 (W20): **30 ~ 90초**
- **권장 레벨 매트릭스** (character_progression §6 정합): 챕터 N 진입 ≈ L(N + 9~13).
- **플레이어 데미지 = 캐릭터 진행 50% + 장비 50%** — 비중 검증의 진단 도구.
- **MVP 적용 범위**: 챕터 1~10 (레벨 1~30, 티어 1 장신구 해방까지).

---

## 2. 곡선 공식

### 2.1. 챕터별 적 스탯 곱셈

```
ChapterData.enemyStatMultiplier(N) = 1.10^(N-1)
ChapterData.xpDropMultiplier(N)    = 1.08^(N-1)   (캐릭터 진행 §5와 정합)
```

| 챕터 N | 곱셈 | xp 곱셈 |
|---|---|---|
| 1 | × 1.000 | × 1.000 |
| 5 | × 1.464 | × 1.360 |
| 10 | × 2.358 | × 1.999 |
| 15 | × 3.797 | × 2.937 |
| 20 | × 6.115 | × 4.316 |
| 25 | × 9.850 | × 6.341 |
| 30 | × 15.86 | × 9.317 |
| 35 | × 25.55 | × 13.69 |

> 곱셈 비율 1.10/1.08은 시작점 가설. §8의 시뮬 결과로 보정.

### 2.2. 적 종류별 베이스 스탯 (현재 SO 데이터)

| 종류 | 평균 HP | 평균 ATK | 비율 (vs 일반) |
|---|---|---|---|
| 일반 적 (Basic·Rabbit·Bear·Firefly·Pumpkin) | 44 | 5 | × 1.0 |
| 미니보스 (StarCloud·BeanGiant·CardSoldier) | 277 | 12 | × 6.3 (HP) / × 2.4 (ATK) |
| 메인 보스 (BigWolf·CloudGiant·QueenHearts) | 950 | 25 | × 21.6 (HP) / × 5.0 (ATK) |

> 베이스는 챕터 1 기준. 챕터 N 적용 시 `× 1.10^(N-1)` 곱셈.

---

## 3. 35 챕터 적 스탯 매트릭스

> 베이스 (HP=44, 일반 ATK=5)에 챕터 곱셈 적용. 미니/보스는 베이스 비율 그대로.

| 챕터 N | 일반 HP | 일반 ATK | 미니 HP | 미니 ATK | 보스 HP | 보스 ATK |
|---|---|---|---|---|---|---|
| 1 | 44 | 5 | 277 | 12 | 950 | 25 |
| 5 | 64 | 7 | 405 | 18 | 1,391 | 37 |
| 10 | 104 | 12 | 653 | 28 | 2,240 | 59 |
| 15 | 167 | 19 | 1,052 | 46 | 3,607 | 95 |
| 20 | 269 | 31 | 1,694 | 73 | 5,809 | 153 |
| 25 | 433 | 49 | 2,728 | 118 | 9,358 | 246 |
| 30 | 698 | 79 | 4,393 | 190 | 15,067 | 397 |
| 35 | 1,124 | 128 | 7,074 | 307 | 24,272 | 639 |

> 모든 값은 `Mathf.RoundToInt(base × 1.10^(N-1))`.

---

## 4. 플레이어 DPS 추정

### 4.1. DPS 공식 (단순화)

```
DPS = AttackDamage_total
    × (1 / AvgInterval)
    × (1 + AdditiveDamageBonus)         [버킷 1]
    × (1 + FinalDamageBonus)            [버킷 2]
    × (1 + CritChance × (CritDamage-1)) [버킷 3]
    × (1 / (1 + EffectiveArmor / 100))  [방어력]
```

### 4.2. 레벨별 캐릭터 베이스 (장비 0)

| Level | AttackDamage | AttackSpeed | 1/Interval | 코어 보너스 | 베이스 DPS |
|---|---|---|---|---|---|
| L1 | 10 | 1.0 | 1.0 | (코어 미적용) | 10.0 |
| L5 | 15 | 1.0 | 1.0 | 힘 5 → +0.5 atk | 15.5 |
| L10 | 20 | 1.10 | 1.10 | 힘 10 → +1 | 23.1 |
| L15 | 25 | 1.10 | 1.10 | 힘 15 → +1.5 | 29.2 |
| L20 | 30 | 1.20 | 1.20 | 힘 20 → +2 | 38.4 |
| L25 | 35 | 1.20 | 1.20 | 힘 25 → +2.5 | 45.0 |
| L30 | 40 | 1.30 | 1.30 | 힘 30 → +3 | 55.9 |
| L35 | 45 | 1.30 | 1.30 | 힘 35 → +3.5 | 63.1 |
| L40 | 50 | 1.40 | 1.40 | 힘 40 → +4 | 75.6 |
| L45 | 55 | 1.40 | 1.40 | 힘 45 → +4.5 | 83.3 |
| L50 | 60 | 1.50 | 1.50 | 힘 50 → +5 | 97.5 |

> AttackDamage = `CharacterData.AttackDamage(10) + Level × 1 + 힘 × 0.1`
> AttackSpeed = `1.0 + (Level/5) × 0.05` (5 레벨마다 +0.05)
> 베이스 DPS = `AttackDamage × AttackSpeed`

### 4.3. 장비 보너스 평균 (티어별)

> affix_pool §5 등급 분포의 가중 평균으로 산출.

| 티어 | 챕터 | Bucket 1 (합산 데미지%) | Bucket 2 (최종 데미지%) | Bucket 3 (크리 추가) |
|---|---|---|---|---|
| 진입 | 1~5 | +10% | +0% | +5% |
| T1 | 6~15 | +25% | +5% | +12% |
| T2 | 16~25 | +50% | +18% | +25% |
| T3 | 26~35 | +90% | +35% | +45% |

> Bucket 1 = 무기·반지·목걸이 affix 합산 + 코어 힘 보너스 (×1.5 가산).
> Bucket 2 = 망토·하의·안경 트리오 affix 합산.
> Bucket 3 = 크리확률 × (크리피해-1).

### 4.4. 레벨+장비 통합 DPS

| Level | 챕터 | 베이스 DPS | × 버킷 1 | × 버킷 2 | × 버킷 3 | **최종 DPS** |
|---|---|---|---|---|---|---|
| L1 | 1 | 10.0 | × 1.10 | × 1.00 | × 1.05 | 11.5 |
| L10 | 5 | 23.1 | × 1.10 | × 1.00 | × 1.05 | 26.7 |
| L15 | 10 | 29.2 | × 1.25 | × 1.05 | × 1.12 | 42.9 |
| L20 | 15 | 38.4 | × 1.25 | × 1.05 | × 1.12 | 56.4 |
| L25 | 20 | 45.0 | × 1.50 | × 1.18 | × 1.25 | 99.6 |
| L30 | 25 | 55.9 | × 1.50 | × 1.18 | × 1.25 | 123.7 |
| L35 | 28 | 63.1 | × 1.90 | × 1.35 | × 1.45 | 234.6 |
| L40 | 31 | 75.6 | × 1.90 | × 1.35 | × 1.45 | 281.0 |
| L45 | 33 | 83.3 | × 1.90 | × 1.35 | × 1.45 | 309.6 |
| L50 | 35 | 97.5 | × 1.90 | × 1.35 | × 1.45 | 362.5 |

---

## 5. TTK (Time-to-kill) 검증

### 5.1. 메인 보스 TTK

> 권장 레벨에서 메인 보스 처치까지의 예상 초.

| 챕터 N | 권장 Lv | 보스 HP | DPS | **TTK (초)** | 평가 |
|---|---|---|---|---|---|
| 1 | L1 | 950 | 11.5 | 82 | ⚠️ 권장 미달 (L10 진입 가정) |
| 1 | L10 | 950 | 26.7 | 36 | ✅ 정상 |
| 5 | L13 | 1,391 | 35 | 40 | ✅ 정상 |
| 10 | L20 | 2,240 | 56 | 40 | ✅ 정상 |
| 15 | L25 | 3,607 | 100 | 36 | ✅ 정상 |
| 20 | L32 | 5,809 | 160 | 36 | ✅ 정상 |
| 25 | L37 | 9,358 | 250 | 37 | ✅ 정상 |
| 30 | L43 | 15,067 | 305 | 49 | ⚠️ 약간 길음 |
| 35 | L50 | 24,272 | 363 | 67 | ✅ 정상 (마지막 도전) |

> TTK 30~60초가 보스 전투의 **도파민 영역**. 권장 미달 시 80초+ → "더 강해진 후 다시 도전" UX.

### 5.2. 미니보스 TTK (W5/10/15)

| 챕터 N | 권장 Lv | 미니 HP | DPS | **TTK (초)** |
|---|---|---|---|---|
| 1 | L10 | 277 | 26.7 | 10.4 |
| 10 | L20 | 653 | 56 | 11.7 |
| 20 | L32 | 1,694 | 160 | 10.6 |
| 30 | L43 | 4,393 | 305 | 14.4 |
| 35 | L50 | 7,074 | 363 | 19.5 |

> 미니보스 5~15초가 적정. 챕터 35는 약간 길어지는데 마지막 도전이라 허용.

### 5.3. 일반 적 TTK

| 챕터 N | DPS | 일반 HP | TTK (초) |
|---|---|---|---|
| 1 (L10) | 26.7 | 44 | 1.65 |
| 10 (L20) | 56 | 104 | 1.86 |
| 20 (L32) | 160 | 269 | 1.68 |
| 30 (L43) | 305 | 698 | 2.29 |
| 35 (L50) | 363 | 1,124 | 3.10 |

> 일반 적 TTK 1~3초 — Brotato/Vampire Survivors 우글거림 정합. 챕터 35에서 살짝 길지만 OK.

---

## 6. 플레이어 생존 검증

### 6.1. 플레이어 HP 곡선

| Level | HP from CharacterData | + Level (×10) | + 마음 코어 (×0.5) | 장비 HP +% (avg) | **총 HP** |
|---|---|---|---|---|---|
| L1 | 100 | +10 | +0 | × 1.00 | 110 |
| L10 | 100 | +100 | +5 | × 1.05 | 215 |
| L20 | 100 | +200 | +10 | × 1.10 | 341 |
| L30 | 100 | +300 | +15 | × 1.15 | 478 |
| L40 | 100 | +400 | +20 | × 1.20 | 624 |
| L50 | 100 | +500 | +25 | × 1.20 | 750 |

### 6.2. 적 ATK ↔ 플레이어 생존 (메인 보스 1방 대비)

| 챕터 N | 권장 Lv | 보스 ATK | 데미지 감소 | 실 데미지 | 플레이어 HP | **버틸 횟수** |
|---|---|---|---|---|---|---|
| 1 | L10 | 25 | -5% | 23.8 | 215 | 9.0 회 |
| 10 | L20 | 59 | -10% | 53.1 | 341 | 6.4 회 |
| 20 | L32 | 153 | -18% | 125.5 | 460 | 3.7 회 |
| 30 | L43 | 397 | -25% | 297.8 | 590 | 2.0 회 |
| 35 | L50 | 639 | -30% | 447.3 | 750 | 1.7 회 |

> 마지막 챕터에서는 보스 공격 1~2회만 버틸 수 있음 → 회피·이동·동행 데미지 감소 옵션의 전략적 중요성.

---

## 7. 권장 레벨 ↔ 챕터 정합 검증

> character_progression §6 + 본 문서 §5 통합.

| 챕터 묶음 | 챕터 # | 권장 Lv 진입 | 권장 Lv 클리어 | 보스 TTK | 평가 |
|---|---|---|---|---|---|
| 메타·기억 | 1~5 | L1~L13 | L13~L18 | 36~40s | ✅ |
| 티어 1 해방 | 6~10 | L13~L20 | L20~L25 | 36~40s | ✅ |
| 자연 전환 | 11~15 | L20~L25 | L25~L30 | 36~40s | ✅ |
| 티어 2 향상 | 16~20 | L25~L32 | L32~L37 | 36~40s | ✅ |
| 동물·문명 | 21~25 | L32~L37 | L37~L42 | 37~40s | ✅ |
| 티어 3 정점 | 26~30 | L37~L43 | L43~L47 | 40~49s | ⚠️ 살짝 긴 |
| 환상 정점 | 31~35 | L43~L50 | L50 (캡) | 50~67s | ✅ (마지막 도전) |

> chapter_structure_v2 §5의 placeholder 곡선(L52까지)은 **본 문서가 대체**한다 (캡 50으로 정합).

---

## 8. 곡선 조정 권장사항

### 8.1. 현재 곡선의 우려점

1. **챕터 26~30 약간 긴 보스 TTK (40~49s)** — 티어 3 장비 드랍 챕터가 도파민 정점이어야 하는데 약간 긴 편.
2. **L1으로 챕터 1 진입 시 TTK 80s** — 권장 미달 가정이라 OK이지만, 첫 플레이 경험이 답답할 수 있음.

### 8.2. 권장 조정

| 항목 | 현재 | 권장 | 이유 |
|---|---|---|---|
| 보스 베이스 HP | 950 | **800** | TTK 30~40s 영역으로 압축. 도파민. |
| 챕터 곱셈 | 1.10 | **유지** | 35 챕터에서 ×25.55 적정 |
| Bucket 2 (T3) 평균 | +35% | **+45%** | 티어 3 장신구 드랍 시 보상감 확대 |
| 마음 코어 효과 | +50 HP/100 | **+80 HP/100** | 챕터 30+ 생존 강화 |

### 8.3. 캐릭터 클래스별 곡선 보정

| 캐릭터 | 영향 | 챕터 1~10 | 챕터 30~35 |
|---|---|---|---|
| 용감한 소년 (균형) | DPS 100% / HP 100% | TTK 36s / 9회 버팀 | TTK 50s / 2회 버팀 |
| 씩씩한 소녀 (탱커) | DPS 95% / HP 115% | TTK 38s / 10회 | TTK 53s / 2.3회 |
| 조용한 친구 (속도) | DPS 110% / HP 90% | TTK 33s / 8회 | TTK 45s / 1.8회 |

> 클래스 비중 1.2/0.9 차이가 ±10% DPS·HP로 변환 — 빌드 정체성은 살리되 솔로 클리어 가능성은 모두 유지.

---

## 9. 데이터 적용 가이드

### 9.1. EnemySpawner 런타임 적용

```csharp
public class EnemySpawner : MonoBehaviour
{
    public GameObject Spawn(EnemyData data, Vector3 pos)
    {
        var enemy = Instantiate(data.prefab, pos, Quaternion.identity);
        var controller = enemy.GetComponent<EnemyController>();

        // ✦ 챕터 곱셈 적용
        var chapter = MetaProgression.SelectedChapter;
        float multi = chapter.enemyStatMultiplier;

        controller.MaxHealth = Mathf.RoundToInt(data.maxHealth * multi);
        controller.AttackDamage = Mathf.RoundToInt(data.attackDamage * multi);
        controller.XpDrop = Mathf.RoundToInt(data.xpDrop * Mathf.Pow(1.08f, chapter.chapterNumber - 1));
        // moveSpeed는 곱셈 없이 SO 그대로 (시각 일관성)

        controller.Initialize(data);
        return enemy;
    }
}
```

### 9.2. ChapterData 인스턴스 생성

35개 ChapterData SO 일괄 생성 시:

```csharp
chapter.enemyStatMultiplier = Mathf.Pow(1.10f, chapter.chapterNumber - 1);
chapter.xpDropMultiplier   = Mathf.Pow(1.08f, chapter.chapterNumber - 1);
```

> 미리 계산해서 SO에 저장 (런타임 `Mathf.Pow` 매번 호출 회피).

### 9.3. 시뮬레이터 (검증용)

```csharp
public static class CurveSimulator
{
    public static float SimulateBossTTK(int chapterN, int playerLevel, AccessoryTier tier)
    {
        float bossHp = 950 * Mathf.Pow(1.10f, chapterN - 1);
        float dps = EstimateDps(playerLevel, tier);
        return bossHp / dps;
    }

    static float EstimateDps(int level, AccessoryTier tier)
    {
        float baseAttack = 10 + level * 1 + (level * 0.1f);
        float attackSpeed = 1.0f + (level / 5) * 0.05f;
        var bonus = GetBucketBonus(tier);   // §4.3 표
        return baseAttack * attackSpeed
             * (1 + bonus.bucket1)
             * (1 + bonus.bucket2)
             * (1 + bonus.bucket3);
    }
}
```

---

## 10. MVP 범위

### 10.1. MVP 포함 (챕터 1~10)

- 챕터 1~10 ChapterData SO 작성 (multiplier 1.0 ~ 2.36)
- EnemySpawner의 곱셈 적용 로직
- 미니보스/보스 비율 (SO 내장)
- 권장 레벨 매트릭스 (UI 힌트는 후속)
- 보스 TTK 36~40초 정합

### 10.2. MVP 제외 (후속)

- 챕터 11~35 (챕터당 1주 콘텐츠 페이스)
- 곡선 조정 (§8.2 권장사항) — 시뮬 데이터 후 적용
- 클래스별 보스 튜닝 (현재는 균형 기준)
- 동적 난이도 조정 (DDA) — 키즈 타깃이라 비도입
- 챕터 권장 레벨 미달 시 UX 경고 (다음 단계)

---

## 11. 미해결 질문

- 곱셈 1.10이 35 챕터에 너무 가파른지 (×25.55가 마지막 도전감 vs 진행 부담)
- 일반 적 TTK 3초 (챕터 35) — 우글거림 유지에 적절한가
- 챕터 30+ 보스 TTK 49s를 줄일 다른 채널 (장신구 t3 +50%? 신화 옵션 보장?)
- 클래스 비중 ±10%가 빌드 정체성에 충분한가 (±20% 검토)
- 시뮬레이터에 RNG (크리·affix 굴림) 도입 여부 (현재는 평균값)
- 적 방어력(`armor`) 도입 시기 (현재는 0 가정)
- 챕터별 wave 30sec 가정의 검증 (실제 `WaveManager.waveDuration` 측정)
- 권장 레벨 미달 시 "이전 챕터 반복" 외 보강 수단 (메타 업그레이드만으로 충분?)
- 보스 베이스 HP 800 vs 950 결정 (§8.2 권장은 800)

---

## 12. 다음 단계

1. **`ChapterData` 35개 SO 일괄 생성** — `enemyStatMultiplier` 미리 계산
2. **`EnemySpawner` 챕터 곱셈 통합** — §9.1 코드 적용
3. **`CurveSimulator` Editor 도구** — 챕터 × 레벨 × 티어 → TTK 표 생성
4. **시뮬 결과 기반 §8.2 조정 적용** — 보스 HP 800 / Bucket 2 T3 +45%
5. **권장 레벨 미달 시 UX 처리** — 챕터 진입 모달에 "조금 더 강해진 후 도전?" 안내
6. **`economy_v1.md` 작성** — 강화 비용 곡선이 챕터 진행과 동조하는지 검증

---

## 13. 출처

- [chapter_structure_v2 §5](./chapter_structure_v2.md) (placeholder 곡선 — 본 문서가 대체)
- [character_progression_v1 §6](./character_progression_v1.md) (권장 레벨 매트릭스)
- [affix_pool_v1 §5](./affix_pool_v1.md) (티어별 등급 분포 → 평균 affix 가치)
- [stats_v1 §6](./stats_v1.md) (DamageCalculator 공식)
- 실제 EnemyData SO (LittleHero/Assets/_Project/Data/Enemies/)
