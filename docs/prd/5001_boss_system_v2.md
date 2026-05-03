# 5001 — Boss System v2 (Tier·Phase·Loot 분리)

> **PRD 번호**: 5001 (Little Hero 첫 번호 시스템 도입)
> **목표**: 보스 시스템 OOP 재설계. 각 라운드 미니보스 + 챕터 보스 + 일일/주간 보스 4 Tier 분리.
> CLAUDE.md §15 OOP 정책 정합 + Strategy + State 패턴 도입.

## 0. 결정 이력
- 2026-05-03: 사용자 요청 — 각 라운드 미니/보스 OOP 재설계 (prd-pipeline Phase 1 시연)
- PRD 번호 5001 부여 (Little Hero 첫 번호)

## 1. 현재 보스 시스템 분석

### 1.1 시스템 구조
- **IBossAttackPattern** + 7 구현체 (Charge/Pulse/Spread/Summon/Vine/Tornado/BlackHole)
- **EnemyData** 의 `isMiniB` / `isBoss` boolean + `bossAttackType` enum
- **51 Boss SO** (35 챕터 보스 + 5 카테고리 archetype × 미니보스 + 일일 7 + 주간 4)
- **BossEncounterService** — Daily/Weekly 회전 + 신화 보장
- **BossEncounterRunner** — 별도 씬

### 1.2 위반 / 한계 (CLAUDE.md §15 정책)
- ❌ **Tier 구분 약함** — `isMiniB` / `isBoss` boolean 만 (Daily/Weekly 분리 X)
- ❌ **Phase 미지원** — 모든 보스 단일 attack pattern (HP 50% 시 패턴 변경 없음)
- ❌ **Loot 분기** — 보스 처치 보상 EnemyData 에 X (BossEncounterService 만)
- △ **공격 패턴 1종/보스** — 다중 패턴 조합 불가 (예 보스 = Pulse + Summon)
- △ **EnemyController.CreateBossAttack switch** — Factory 미사용 (SOLID-O)

### 1.3 4 Tier 통합 부재
| Tier | 현재 | 차별 X |
|---|---|---|
| Mini Boss | isMiniB=true, 단일 attack | 챕터 보스와 같은 메커니즘 |
| Chapter Boss | isBoss=true, 단일 attack | Phase 1 만 |
| Daily Boss | isBoss=true (별 분리 X) | Daily 라벨 enum 없음 |
| Weekly Boss | isBoss=true (별 분리 X) | Weekly 라벨 enum 없음 |

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A enum 확장 | `BossTier` enum 추가 (None/Mini/Chapter/Daily/Weekly) | 단순, 호환 | Phase 미해결 |
| **B Strategy + State** ★ | BossTier + IBossPhase + Phase 1/2 전환 | OOP 정합, 확장성 ↑ | 4~5 commit |
| C 풀빌드 BehaviorTree | BehaviorTree + Composite 패턴 | AAA 수준 | 큰 변경 (10+ Sprint) |

## 3. 권장 — 옵션 B (Tier·Phase·Loot 분리)

### 3.1 신규 enum 2종

```
public enum BossTier {
    None = 0,         // 일반 적
    MiniBoss = 1,     // 5/10/15 웨이브
    ChapterBoss = 2,  // 20 웨이브
    DailyBoss = 3,    // Endgame Daily
    WeeklyBoss = 4,   // Endgame Weekly (신화 보장)
}

public enum BossPhase {
    Phase1 = 0,       // HP 100~50%
    Phase2 = 1,       // HP 50~0% (Chapter+ 만)
}
```

### 3.2 EnemyData 확장

```
public BossTier bossTier;              // None/Mini/Chapter/Daily/Weekly
public BossAttackType phase2Pattern;   // Phase 2 시 패턴 변경 (ChapterBoss+ 만)
public float phase2Threshold = 0.5f;  // HP 50% 기본
```

기존 `isMiniB` / `isBoss` boolean — `bossTier != None` 으로 대체 가능 (호환성 유지).

### 3.3 IBossPhase 인터페이스 (State 패턴)

```
public interface IBossPhase {
    void OnEnter(EnemyController boss);
    void Tick(EnemyController boss, float deltaTime);
    void OnExit(EnemyController boss);
    bool ShouldTransitionNext(EnemyController boss);
}
```

구현체 2:
- **BossPhase1Default** — 기본 attack pattern (data.bossAttackType)
- **BossPhase2Aggressive** — phase2Pattern + 1.3× 속도 + 텔레그라프 단축

### 3.4 BossPatternFactory (정적, SOLID-O)

기존 EnemyController.CreateBossAttack switch → Factory 추출:

```
public static class BossPatternFactory {
    public static IBossAttackPattern Create(BossAttackType type);  // 7 패턴
}
```

### 3.5 BossLootTable (ScriptableObject, §15.4 SO 우선)

```
[CreateAssetMenu] public class BossLootTable : ScriptableObject {
    public BossTier tier;
    public RewardEntry[] rewards;  // ★/✦/◇/⋆/EquipmentInstance
    public float guaranteedMythicChance;  // Weekly 1.0, Daily 0.05 등
}
```

EnemyData 에 `lootTable` 필드 추가. EnemyController.Die 시 BossLootTable 조회 + 보상 grant.

### 3.6 BossPhaseManager (정적 헬퍼)

EnemyController 의 phase 전환 관리:

```
public static class BossPhaseManager {
    public static IBossPhase CreateInitial(EnemyController boss);
    public static void TransitionTo(EnemyController boss, BossPhase newPhase);
}
```

### 3.7 EnemyController 갱신

기존 단일 `_bossAttack` → `_currentPhase` (IBossPhase) 로 변경:
- Initialize: BossPhaseManager.CreateInitial → Phase1
- UpdateAnim: _currentPhase.Tick + ShouldTransitionNext 검사
- Die: BossLootTable 보상 적용

## 4. CLAUDE.md §15 정합 검증

| 정책 | 적용 |
|---|---|
| §15.1 SRP | Phase 분리 / Factory 분리 / LootTable 분리 |
| §15.1 OCP | 신규 Phase 추가 시 기존 코드 변경 X |
| §15.1 LSP | IBossPhase 모든 구현체 호환 |
| §15.1 ISP | IBossPhase 작은 인터페이스 (4 메서드) |
| §15.4 SO 우선 | BossLootTable SO + EnemyData 필드 |
| §15.6 100줄 | 신규 클래스 모두 100줄 이내 |
| §15.8 Strategy | IBossAttackPattern 그대로 + IBossPhase 신규 |
| §15.9 Test | BossPatternFactory + Phase 전환 단위 테스트 |
| §15.12 Validator | 모든 보스 SO bossTier != None 검증 |

## 5. 마이그레이션 (51 Boss SO)

### 5.1 자동 enum 매핑

| 현재 | bossTier |
|---|---|
| isMiniB=true | MiniBoss |
| isBoss=true (CH 1~35) | ChapterBoss |
| isBoss=true (Enemy_DailyBoss_*) | DailyBoss |
| isBoss=true (Enemy_WeeklyBoss_*) | WeeklyBoss |

### 5.2 Phase 2 Pattern 매핑 (제안)

ChapterBoss / DailyBoss / WeeklyBoss 의 phase 2 패턴 (현재 패턴 + 보강):
- Charge → Charge + Summon (다중 적 소환)
- Pulse → Pulse + Vine (광역 + 영역)
- Spread → Spread (탄막 +50%)
- Summon → Summon + Tornado
- Vine → Vine + Pulse
- Tornado → Tornado (지속 +2s)
- BlackHole → BlackHole (흡인 +50%)

미니보스는 Phase 2 X (단일 phase).

### 5.3 BossLootTable 인스턴스 (4 Tier)

- `LootTable_MiniBoss` — 별가루 50★ + 무지개 ✦ 1
- `LootTable_ChapterBoss` — ★ 200 + ✦ N (CH 매핑) + 마법 결정 ◇ 1
- `LootTable_DailyBoss` — ★ 100 + 별빛 ⋆ 5
- `LootTable_WeeklyBoss` — ★ 500 + ⋆ 20 + 신화 보장 (mythicChance=1.0)

## 6. 신규 파일 (Sprint 1.1~1.4)

```
Scripts/Data/
├── BossTier.cs (enum) — 신규
├── BossPhase.cs (enum) — 신규
└── BossLootTable.cs (SO) — 신규

Scripts/Gameplay/Enemies/
├── BossPhaseManager.cs (정적) — 신규
├── BossPhases/
│   ├── IBossPhase.cs — 신규
│   ├── BossPhase1Default.cs — 신규
│   └── BossPhase2Aggressive.cs — 신규
└── BossPatternFactory.cs (정적) — 신규
```

EnemyController + EnemyData + BossEncounterService 갱신.

## 7. 시스템 통합 (기존 시스템 영향)

| 시스템 | 영향 | 검증 |
|---|---|---|
| BossEncounterService | Daily/Weekly 처리 그대로, BossTier enum 으로 안전 분기 | 호환 OK |
| MythicEffectRuntime | 보스 처치 보장 → BossLootTable.guaranteedMythicChance 활용 | 갱신 |
| F4·F5 보스 SO | bossTier 자동 매핑 + Phase 2 Pattern 채움 | 시간 5분 |
| stats_v2 | 보스 데미지 그대로 (PIVOT v4 보스/일반 분기 X) | 정합 |
| weapon_v2 | 보스 처치 시 WeaponClassBonus 영향 X | 변동 X |

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| 51 Boss SO 마이그레이션 시 누락 | Editor 메뉴 + 자동 분류 + 검증 grep |
| Phase 2 패턴 밸런스 (1.3× 속도) | CurveSimulator 후속 시뮬 |
| 미니보스 Phase 미지원 | bossTier == MiniBoss 시 Phase 2 스킵 |
| 기존 isMiniB / isBoss boolean 잔존 코드 | bossTier 호환 헬퍼 (IsMiniBoss/IsBoss) 추가 |

## 9. 변경 이력
- 2026-05-03: 초기 작성 (5001 boss_system_v2 첫 번호)

## 10. 참조

- `boss_attack_v1.md` — 7 패턴 + 46 보스 매핑 (현재)
- `endgame_v1.md` — Daily/Weekly 보스 메커니즘
- `weapon_system_v2.md` — 직업·카테고리 분류 패턴 답습
- `stats_v2_infinite.md` — PIVOT v4 보스/일반 분기 제거
- `CLAUDE.md` §15 OOP 정책
