# 챕터별 적 스탯 정리 (v1)

> **작성일**: 2026-05-04
> **출처**: `Editor/PhaseFContentSeed.cs` + `Editor/ChapterDataSeed.cs` + `Data/EnemyData.cs` + `Data/ChapterData.cs`
> **요약**: 35 챕터 = **5 카테고리 × 6 archetype** archetype 재사용 + `enemyStatMultiplier = 1.10^(N-1)` 챕터별 곱셈.

## 1. 데이터 모델

### 1.1 EnemyData SO 필드

| 필드 | 타입 | 설명 |
|---|---|---|
| `id` / `displayName` | string | 식별자 / 한국어 표시 |
| `maxHealth` | float | 베이스 체력 |
| `damage` | float | 베이스 공격력 |
| `armor` | float | 방어력 (메이플 식 `dmg × (1 - def/(def+1000))`) |
| `moveSpeed` | float | 이동 속도 (일반 2.8 / 미니 2.2 / 보스 1.6) |
| `xpDrop` / `starDustDrop` | float | 보상 |
| `scaleMultiplier` | float | 외형 크기 |
| `tintColor` | Color | 카테고리별 톤 |
| `bossTier` | enum | None / MiniBoss / ChapterBoss / DailyBoss / WeeklyBoss (5001) |
| `bossAttackType` | enum | Charge / Pulse / Spread / Summon / Vine / Tornado / BlackHole |
| `bossAttackCooldown` / `bossAttackDamage` / `bossAttackRange` | float | 보스 공격 파라미터 |
| `phase2Pattern` / `phase2Threshold` / `phase2SpeedMultiplier` | enum/float/float | 5001 보스 Phase 2 (HP 50% 이하 → ×1.3 속도) |

### 1.2 ChapterData SO 필드

| 필드 | 값 | 설명 |
|---|---|---|
| `chapterNumber` | 1~35 | 챕터 번호 |
| `enemyStatMultiplier` | `1.10^(N-1)` | 적 스탯 곱셈 (CH_01=1.0 ~ CH_35=25.55) |
| `starFragmentReward` | `round(5 × 1.10^(N-1))` | 첫 클리어 ✦ |
| `totalWaves` / `waveDuration` | 20 / 45초 | 웨이브 구조 |
| `enemyPool[3]` | EnemyData[] | 일반 적 3종 (카테고리별 Small/Medium/Large) |
| `miniBossPool[2]` | EnemyData[] | 미니보스 2종 (Wave 5/10/15) |
| `bossEnemy` | EnemyData | 챕터 보스 (Wave 20) |

## 2. 챕터 곱셈 메커니즘

런타임 적용 흐름 (`EnemySpawner.Spawn` → `EnemyController.Initialize(data, multi)`):

```
효과 HP   = data.maxHealth × chapter.enemyStatMultiplier
효과 ATK  = data.damage    × chapter.enemyStatMultiplier
효과 ARM  = data.armor     × chapter.enemyStatMultiplier (보호 곡선 동일 적용)
효과 XP   = data.xpDrop    × (1 + N × 0.1)              (PlayerProgress 챕터 XP 모디파이어)
```

| 챕터 | `multi` | 챕터 | `multi` | 챕터 | `multi` |
|---|---|---|---|---|---|
| 01 | 1.000 | 13 | 3.452 | 25 | 10.835 |
| 02 | 1.100 | 14 | 3.797 | 26 | 11.918 |
| 03 | 1.210 | 15 | 4.177 | 27 | 13.110 |
| 04 | 1.331 | 16 | 4.595 | 28 | 14.421 |
| 05 | 1.464 | 17 | 5.054 | 29 | 15.863 |
| 06 | 1.611 | 18 | 5.560 | 30 | 17.449 |
| 07 | 1.772 | 19 | 6.116 | 31 | 19.194 |
| 08 | 1.949 | 20 | 6.727 | 32 | 21.114 |
| 09 | 2.144 | 21 | 7.400 | 33 | 23.225 |
| 10 | 2.358 | 22 | 8.140 | 34 | 25.548 |
| 11 | 2.594 | 23 | 8.954 | 35 | **28.102** |
| 12 | 2.853 | 24 | 9.850 | — | — |

> **보정**: CH_35 `multi = 1.10^34 ≈ 25.55` (공식) vs 실런타임 `Mathf.Pow(1.10f, 34) = 28.10` — 부동소수 누적. 표는 실측치(28.10) 기준.
> **고난도 (CH 36~50)**: CH 21~35 클론 + 추가 ×2.0 곱셈 (PhaseFContentSeed F7).

## 3. 카테고리 매핑 (5 카테고리 × 6 archetype = 30 SO)

`PhaseFContentSeed.SelectPoolForChapter(n)` 분기:

| 카테고리 | 챕터 범위 | 톤 색상 (RGB) | 표시 이름 prefix | 베이스 scale |
|---|---|---|---|---|
| **Meta** (마을) | 1~5 | `#D9C7A6` (따뜻한 갈색) | "마을의 친구" | 1.00 |
| **Nature** (숲) | 6~14 | `#8CC78C` (푸른 자연) | "숲의 친구" | 1.00 |
| **Animal** (동물) | 15~20 | `#F2C780` (따뜻한 동물) | "동물 친구" | 1.05 |
| **Civ** (도시) | 21~27 | `#D98CA6` (도시·인공) | "도시의 친구" | 1.00 |
| **Fantasy** (환상) | 28~35 | `#A68CF2` (환상 보라) | "환상의 친구" | 1.10 |

## 4. Archetype 6 역할 베이스 스탯 (모든 카테고리 공통)

`MakeArchetypeSet(category, tint, baseScale)` — 6 EnemyData 생성. 카테고리에 관계없이 **베이스 수치는 동일**, tint·displayName·scale 만 차이.

| 역할 | id 패턴 | HP | ATK | armor | xp | dust | scale 배율 | bossTier |
|---|---|---:|---:|---:|---:|---:|---:|---|
| **Small** (작은 친구) | `Enemy_<Cat>_Small` | 30 | 5 | 0 | 2 | 1 | ×0.90 | None |
| **Medium** (친구) | `Enemy_<Cat>_Medium` | 50 | 8 | 0 | 3 | 2 | ×1.00 | None |
| **Large** (큰 친구) | `Enemy_<Cat>_Large` | 80 | 12 | 5 | 5 | 3 | ×1.15 | None |
| **MiniA** (수호자) | `Enemy_<Cat>_MiniA` | 200 | 20 | 15 | 12 | 8 | ×1.30 | **MiniBoss** |
| **MiniB** (큰 수호자) | `Enemy_<Cat>_MiniB` | 280 | 24 | 20 | 16 | 10 | ×1.40 | **MiniBoss** |
| **Boss** (으뜸 친구) | `Enemy_<Cat>_Boss` | 700 | 36 | 30 | 50 | 25 | ×1.70 | **ChapterBoss** |

> **moveSpeed**: 일반 (Small/Medium/Large) 2.8 / 미니보스 (MiniA/MiniB) 2.2 / 보스 1.6 (자동 결정).
> **공격 패턴**: 일반·미니보스는 근접 추적 (`bossAttackType=None`). 챕터 보스만 카테고리별 패턴 (§6 참조).

## 5. 챕터별 적용 예시 (실효 스탯)

### 5.1 CH_01 호숫가 작은 마을 (Meta, multi=1.00)

| 역할 | 표시 이름 | HP | ATK | armor |
|---|---|---:|---:|---:|
| Small | 마을의 친구 작은 친구 | 30 | 5 | 0 |
| Medium | 마을의 친구 친구 | 50 | 8 | 0 |
| Large | 마을의 친구 큰 친구 | 80 | 12 | 5 |
| MiniA | 마을의 친구 수호자 | 200 | 20 | 15 |
| MiniB | 마을의 친구 큰 수호자 | 280 | 24 | 20 |
| **Boss** | **마을의 친구 으뜸 친구** | **700** | **36** | **30** |

### 5.2 CH_05 아빠의 비밀 기지 (Meta, multi=1.464)

| 역할 | HP | ATK | armor |
|---|---:|---:|---:|
| Small | 44 | 7.3 | 0 |
| Medium | 73 | 11.7 | 0 |
| Large | 117 | 17.6 | 7.3 |
| MiniA | 293 | 29.3 | 22.0 |
| MiniB | 410 | 35.1 | 29.3 |
| **Boss** | **1,025** | **52.7** | **43.9** |

### 5.3 CH_15 토끼굴 깊은 곳 (Animal, multi=4.177)

| 역할 | HP | ATK | armor |
|---|---:|---:|---:|
| Small | 125 | 20.9 | 0 |
| Medium | 209 | 33.4 | 0 |
| Large | 334 | 50.1 | 20.9 |
| MiniA | 835 | 83.5 | 62.7 |
| MiniB | 1,170 | 100.2 | 83.5 |
| **Boss** | **2,924** | **150.4** | **125.3** |

### 5.4 CH_35 별과 별 사이 (Fantasy, multi=28.10)

| 역할 | HP | ATK | armor |
|---|---:|---:|---:|
| Small | 843 | 140.5 | 0 |
| Medium | 1,405 | 224.8 | 0 |
| Large | 2,248 | 337.2 | 140.5 |
| MiniA | 5,621 | 562.0 | 421.5 |
| MiniB | 7,869 | 674.4 | 562.0 |
| **Boss** | **19,672** | **1,011.7** | **843.1** |

## 6. 챕터 보스 공격 패턴 (BossAttackType)

### 6.1 enum 정의

| Enum | 패턴 | 설명 |
|---|---|---|
| `None` | 근접 추적만 | 일반 적·미니보스 |
| `Charge` | 직선 돌진 | 1.5초 텔레그래프 후 돌진 |
| `Pulse` | 광역 폭발 | 자기 중심 원형 펄스 |
| `Spread` | 부채꼴 탄막 | 다발 발사 |
| `Summon` | 소환 | 작은 적 그룹 소환 |
| `Vine` | 식물 함정 | 지면 함정 설치 |
| `Tornado` | 회전 영역 | 지속 데미지 (DailyBoss) |
| `BlackHole` | 흡인 + 폭발 | 흡수 후 폭발 (WeeklyBoss) |

### 6.2 5001 Phase 2 (보스 HP 50% 이하)

- `phase2Pattern`: 보통 `None` (Phase 1 패턴 유지) — `phase2Threshold = 0.5` (HP 50%) → `phase2SpeedMultiplier = 1.3` (공격 속도 ×1.3)
- `BossPhaseManager.Update` 가 `phase2Threshold` 도달 시 `BossPhase2Aggressive.OnEnter` 호출 → `attackInterval × 1.3`
- 현재 시드 단계에서 모든 챕터 보스는 동일한 phase2 (cooldown ×1/1.3) — 카테고리별 Phase 2 패턴 분화는 후속 콘텐츠 작업

## 7. Endgame 보스 (F4 일일 + F5 주간)

### 7.1 DailyBoss 7종 (요일 회전, KST 0시 리셋)

| id | displayName | HP | ATK | armor | dust | bossTier |
|---|---|---:|---:|---:|---:|---|
| `DailyBoss_00` | 월요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |
| `DailyBoss_01` | 화요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |
| `DailyBoss_02` | 수요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |
| `DailyBoss_03` | 목요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |
| `DailyBoss_04` | 금요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |
| `DailyBoss_05` | 토요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |
| `DailyBoss_06` | 일요일 큰 친구 | 5,000 | 50 | 50 | 50 | DailyBoss |

> tint `#FFB94D` (노랑) / scale 1.5×. 7종 모두 베이스 동일 — 콘텐츠 분화는 후속.

### 7.2 WeeklyBoss 4종 (4주 사이클)

| id | displayName | HP | ATK | armor | dust | bossTier |
|---|---|---:|---:|---:|---:|---|
| `WeeklyBoss_00` | 환상의 거대 친구 1주 | 25,000 | 150 | 80 | 200 | WeeklyBoss |
| `WeeklyBoss_01` | 환상의 거대 친구 2주 | 25,000 | 150 | 80 | 200 | WeeklyBoss |
| `WeeklyBoss_02` | 환상의 거대 친구 3주 | 25,000 | 150 | 80 | 200 | WeeklyBoss |
| `WeeklyBoss_03` | 환상의 거대 친구 4주 | 25,000 | 150 | 80 | 200 | WeeklyBoss |

> tint `#FF8C8C` (빨강) / scale 2.2×. mythicChance 1.0 (BossLootTable.WeeklyBoss).

## 8. SO 인스턴스 분포 (요약)

| 폴더 | 개수 | 구성 |
|---|---:|---|
| `Data/Enemies/Chapters/CH##_*.asset` | 210 | 35 챕터 × 6 역할 (개별 인스턴스 보존, V1 모델) |
| `Data/Enemies/Enemy_<Cat>_*.asset` | 30 | 5 카테고리 × 6 archetype (V2 재사용 모델 — 챕터가 참조) |
| `Data/Enemies/DailyBoss_##.asset` | 7 | F4 일일 보스 |
| `Data/Enemies/WeeklyBoss_##.asset` | 4 | F5 주간 보스 |
| `Data/Enemies/` (V1 11종) | 11 | 초기 동화책 적 (보존) |
| **누적** | **262** | EnemyData 인스턴스 |
| `Data/Chapters/V2/CH_##_*.asset` | 35 | V2 챕터 |
| `Data/Chapters/V2/CH_##_Hardcore_*.asset` | 15 | F7 고난도 (CH 36~50) |
| **챕터 누적** | **50** | ChapterData 인스턴스 |

## 9. 한계 + 후속 작업

- **챕터별 archetype 차별화 X** — 현재 5 카테고리 안에서 Small/Medium/Large/MiniA/MiniB/Boss 베이스 동일. 챕터별 고유 외형/패턴은 디자인 후속 작업.
- **DailyBoss 7종 / WeeklyBoss 4종 베이스 동일** — 요일/주차별 분화는 후속.
- **챕터 보스 공격 패턴 매핑 미명시** — `bossAttackType` 필드는 EnemyData 에 있으나 시드 단계에서 모두 default(None?). 카테고리별 패턴(Charge/Vine/Spread 등) 와이어링은 후속 콘텐츠.
- **V1 11종 적** (초기 동화책) 은 V2 모델과 병존 — `Chapter_01_RedHood` 등 폐기 모델로 보존, 신규 챕터에서 사용 X.
- **Hardcore 15 챕터** (CH 36~50): CH 21~35 클론 + 추가 ×2.0 곱셈 → 실효 multi = `2.0 × 1.10^(N-21)` (N=36~50).

## 10. 변경 이력

- 2026-05-04: 초기 작성 (`PhaseFContentSeed.cs` + `ChapterDataSeed.cs` 추출 기반)
