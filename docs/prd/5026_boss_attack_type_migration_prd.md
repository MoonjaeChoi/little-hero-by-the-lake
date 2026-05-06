# 5026 boss_attack_type_migration — 디자인 문서

> **목적**: 137 보스 SO 중 96개의 `bossAttackType=None` 잔존 → BossTier + 카테고리 기반 일괄 마이그레이션
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5025 evaluation 후속 — Validator C3 warnings 171 분석 결과 96 (56%) 가 bossAttackType=None (BossSystem rule) 발견 |

## 1. 현재 시스템 분석

### 1.1 보스 SO 분포 (137건)

| Tier | enum 값 | 개수 | bossAttackType=None |
|---|---|---|---|
| MiniBoss | 1 | 83 | **83** (100%) |
| ChapterBoss | 2 | 43 | **13** (30%) |
| DailyBoss | 3 | 7 | 0 ✅ |
| WeeklyBoss | 4 | 4 | 0 ✅ |
| **합계** | | **137** | **96** (70%) |

**핵심 발견**:
- DailyBoss + WeeklyBoss 11개는 wiring 완료 (5001 boss_system_v2 시점)
- ChapterBoss 30/43 도 wiring 완료
- **MiniBoss 83 + ChapterBoss 13 = 96 미설정**

### 1.2 96 미설정 SO 위치 분포

```
tier=1 (MiniBoss):
  - Data/Chapters/ (5009/5019 chapter override): 70
  - Data/Enemies/ (V2 archetype): 13
tier=2 (ChapterBoss):
  - Data/Chapters/: 5
  - Data/Enemies/: 8
```

### 1.3 BossAttackType enum (7 + None)

`Scripts/Gameplay/Enemies/BossAttacks/IBossAttackPattern.cs:14`:

```csharp
public enum BossAttackType {
    None,
    Charge,      // Animal — 1.5s telegraph → 직선 돌진
    Pulse,       // Civ — 광역 폭발
    Spread,      // Fantasy — 부채꼴 탄막 (사이클 2)
    Summon,      // Meta — 작은 적 소환 (사이클 2)
    Vine,        // Nature — 식물 함정 (사이클 2)
    Tornado,     // Daily — 회전 영역 (사이클 3)
    BlackHole,   // Weekly — 흡인 + 폭발 (사이클 3)
}
```

### 1.4 5008/5019 카테고리 → 챕터 매핑 (이미 정착)

| 카테고리 | 챕터 범위 | 권장 attack |
|---|---|---|
| Meta | CH 1~5 | **Summon** |
| Nature | CH 6~14 | **Vine** |
| Animal | CH 15~20 | **Charge** |
| Civ | CH 21~27 | **Pulse** |
| Fantasy | CH 28~35 | **Spread** |

V2 archetype 파일명 prefix: `Enemy_Meta_*`, `Enemy_Nature_*`, `Enemy_Animal_*`, `Enemy_Civ_*`, `Enemy_Fantasy_*` — 직접 카테고리 추출 가능.

V1 (RedHood/BeanGiant 등) — CH 1~3 소속 → Meta 카테고리.

### 1.5 위반 영향

- ProjectValidator Warnings: **96 / 171** (56%)
- 보스 처치 시 `IBossAttackPattern` 인스턴스화 미작동 → MiniBoss 83 + ChapterBoss 13 가 일반 적 처럼 행동
- 5001 boss_system_v2 의 BossPatternFactory 효과 일부 누수

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | Editor 메뉴 일괄 마이그레이션 (카테고리 패턴 매칭 + Tier 별 default) | 96 → 0, 5001 답습, 디자이너 후속 조정 가능, NUnit 회귀 검증 | 패턴 매칭 mismatch 위험 (V1 11건 등 special case) |
| B | 개별 SO Inspector 수동 wiring | 정밀 제어 | 96 SO × 1분 = 1.6h 노동 + 회귀 가능 |
| C | bossAttackType 필드 폐기 + 항상 Charge default | 단순화 | 시각적 다양성 손실, 5001 IBossAttackPattern 7종 인프라 무용지물 |

## 3. 권장 설계 (옵션 A)

### 3.1 신규 파일 명세

**`Editor/BossAttackTypeMigrator.cs`** (~80L)

```csharp
namespace LittleHero.EditorTools {
    public static class BossAttackTypeMigrator {
        // 카테고리 → BossAttackType 매핑 (5008/5019 정착 패턴)
        private static readonly Dictionary<string, BossAttackType> CategoryMap = new() {
            { "Meta",    BossAttackType.Summon  },
            { "Nature",  BossAttackType.Vine    },
            { "Animal",  BossAttackType.Charge  },
            { "Civ",     BossAttackType.Pulse   },
            { "Fantasy", BossAttackType.Spread  },
        };

        // V1 fairytale (CH 1~3) — Meta 분류
        private static readonly HashSet<string> V1FairytaleNames = new() {
            "Enemy_BeanGiant", "Enemy_Bear", "Enemy_BigWolf", "Enemy_CardSoldier",
            "Enemy_CloudGiant", "Enemy_Firefly", "Enemy_Pumpkin", "Enemy_QueenHearts",
            "Enemy_Rabbit", "Enemy_StarCloud", "Enemy_Basic"
        };

        [MenuItem("Tools/LittleHero/Migrate Boss Attack Type")]
        public static void Run() {
            // ... 96 SO 일괄 갱신, EditorUtility.DisplayDialog 보고
        }

        public static BossAttackType ResolveByName(string assetName) { ... }
        public static BossAttackType ResolveByChapterId(string chapterId) { ... }
    }
}
```

### 3.2 매핑 알고리즘

1. **Asset name regex** (V2 archetype): `Enemy_(Meta|Nature|Animal|Civ|Fantasy)_(MiniA|MiniB|Boss)` → CategoryMap
2. **Chapter prefix** (chapter override): `CH(\d{2})_(MiniA|MiniB|Boss)` → 챕터번호 → 카테고리 → CategoryMap
3. **V1 fairytale** (V1FairytaleNames 포함): Meta (Summon)
4. **fallback**: Charge (default)

### 3.3 §15 정합

- §15.1 SRP: 정적 헬퍼만 (단일 책임)
- §15.4 SO 우선: 매핑 dictionary 도 SO 화 가능 (BalanceConfig 확장 — 후속 검토)
- §15.6 100줄 룰: ~80L ✅
- §15.9 Test: NUnit 4 (ResolveByName Meta/Nature/Animal/Civ/V1Fairytale)
- §15.12 Validator: 기존 WeaponBossRule.ValidateBossSystem 자동 0 검증

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Editor/BossAttackTypeMigrator + 96 SO 일괄 적용 | 1 | 30분 |
| 2 | NUnit Test 4 + Validator 회귀 검증 | 1 | 30분 |
| 3 | changelog + CLAUDE.md §14 | 1 | 10분 |
| **합계** | | **3** | **1h 10m** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| V1 11종 매핑 누락 | V1FairytaleNames HashSet 명시 + NUnit 검증 |
| Hardcore 챕터 (CH36~50) 매핑 — V2 (CH21~35) ×2 같은 카테고리 ? | Chapter prefix `CH(\d{2})` 매칭 시 (n-1)%5 분할 — Hardcore 구분 X (CH36→CH21 같은 카테고리) |
| EnemyData 기존 wiring 덮어쓰기 위험 | bossAttackType=None 만 갱신 (기존 wiring 보존) |
| 디자이너 후속 조정 필요 | Editor Inspector 에서 자유롭게 변경 가능 (SO field) |

## 6. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 96 미설정 분석 + 옵션 A 권장 (3 Sprint / 1h10m) |

## 7. Sources

- 5025 evaluation Validator C3 warnings 171 분석 결과
- 5001 boss_system_v2 마이그레이션 패턴 (`Tools/LittleHero/Migrate Boss Tier`)
- 5008 LayerDiffuse RGBA 30 archetype 카테고리 (Meta/Nature/Animal/Civ/Fantasy)
- 5019 enemy_chapter_design 35 챕터 narrative 매핑
- BossAttackType enum 코드 주석 (Animal=Charge / Civ=Pulse / Fantasy=Spread / Meta=Summon / Nature=Vine)
