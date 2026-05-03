# Evaluator Checklist: 5001 — Boss System v2

> **Phase 4 산출** — Evaluator (Phase 6) 가 사용할 자동/수동 체크리스트
> **Contract**: `5001_boss_system_v2_contract.md`

## 자동 검증 (Phase 6 — prd-evaluate 자동)

### C1 컴파일

- [ ] `mcp__UnityMCP__refresh_unity` 호출
- [ ] 5초 대기 (도메인 리로드 안정화)
- [ ] `mcp__UnityMCP__read_console` errors=0
- [ ] `editor_state.isCompiling == false`

### C2 Editor Test (NUnit) — BossSystemTests

- [ ] `BossSystemTests` 클래스 존재 (Tests/Editor/BossSystemTests.cs)
- [ ] Test Runner > Edit Mode > Run All
- [ ] BossPatternFactory_Create_Charge_ReturnsChargePattern PASS
- [ ] BossPatternFactory_Create_Pulse_ReturnsPulsePattern PASS
- [ ] BossPatternFactory_Create_Spread_ReturnsSpreadPattern PASS
- [ ] BossPatternFactory_Create_Summon_ReturnsSummonPattern PASS
- [ ] BossPatternFactory_Create_Vine_ReturnsVinePattern PASS
- [ ] BossPatternFactory_Create_Tornado_ReturnsTornadoPattern PASS
- [ ] BossPatternFactory_Create_BlackHole_ReturnsBlackHolePattern PASS
- [ ] BossPhase1_ShouldTransitionNext_OnHpBelowThreshold_ChapterBoss_ReturnsTrue PASS
- [ ] BossPhase2_OnEnter_AppliesSpeedMultiplier_1_3 PASS
- [ ] BossLootTable_Tier_MatchesAllInstances PASS

### C3 ProjectValidator

- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings ≤ 21 (기존 기준)
- [ ] 신규 룰 `Boss_BossTier_NotNone` PASS
- [ ] 신규 룰 `Boss_LootTable_NotNull` PASS
- [ ] 신규 룰 `Boss_AttackType_NotNone` PASS

### C4 Reflection 정적 검증

- [ ] `LittleHero.BossTier` enum 5 값 (None/MiniBoss/Chapter/Daily/Weekly) 존재
- [ ] `LittleHero.BossPhase` enum 2 값 존재
- [ ] `LittleHero.IBossPhase` interface 4 메서드 (OnEnter/Tick/OnExit/ShouldTransitionNext)
- [ ] `LittleHero.BossPhase1Default` sealed class (IBossPhase 구현)
- [ ] `LittleHero.BossPhase2Aggressive` sealed class (IBossPhase 구현 + `_speedMultiplier` SerializeField)
- [ ] `LittleHero.BossPatternFactory` static class + `Create(BossAttackType)` 메서드
- [ ] `LittleHero.BossLootTable` SO + 3 필드 (tier / Rewards / guaranteedMythicChance)
- [ ] `LittleHero.BossPhaseManager` static + `CreateInitial`, `Update` 메서드
- [ ] `LittleHero.BossLootGrantService` static + `Apply(BossLootTable)` 메서드

### C5 SO 검증

#### BossLootTable 4 인스턴스
- [ ] LootTable_MiniBoss.asset 존재 (tier=MiniBoss, mythicChance=0)
- [ ] LootTable_ChapterBoss.asset 존재 (tier=ChapterBoss, mythicChance=0.05)
- [ ] LootTable_DailyBoss.asset 존재 (tier=DailyBoss, mythicChance=0.1)
- [ ] LootTable_WeeklyBoss.asset 존재 (tier=WeeklyBoss, mythicChance=1.0)

#### EnemyData 51 보스 분포
- [ ] 51 보스 SO 모두 bossTier != None
- [ ] MiniBoss N (isMiniB=true 매핑)
- [ ] ChapterBoss M (path Boss/CH*)
- [ ] DailyBoss 7 (path DailyBoss/*)
- [ ] WeeklyBoss 4 (path WeeklyBoss/*)
- [ ] ChapterBoss/Daily/Weekly 모두 lootTable != null
- [ ] 모든 보스 bossAttackType != None

## 수동 검증 (사용자 Play Mode)

### C6.1 시나리오 1 — MiniBoss 처치 (Phase 1 만, D3=A)

- [ ] 챕터 1 → Wave 5 진입
- [ ] MiniBoss 출현
- [ ] HP 0 처치
- [ ] 콘솔 로그 `[BossLootGrantService] Applied LootTable_MiniBoss` 확인
- [ ] SaveData StarDust +50, RainbowOrb +1 확인
- [ ] Phase 전환 X (Phase 2 안 들어감)

### C6.2 시나리오 2 — ChapterBoss Phase 전환 (D2=C 검증)

- [ ] 챕터 1 → Wave 20 진입
- [ ] ChapterBoss 출현 (Phase 1)
- [ ] HP 50% 도달
- [ ] 콘솔 로그 `[Boss] Phase 1 → Phase 2 (HP 50%)` 확인
- [ ] 콘솔 로그 `[BossPhase2.OnEnter] Aggressive mode (×1.3 speed)` 확인
- [ ] 시각 변화 — 공격 속도 1.3× 적용 (체감)
- [ ] 패턴 변경 (예 Pulse → Vine)
- [ ] 처치 후 보상 ★200 + ✦1 + ◇1 확인

### C6.3 시나리오 3 — WeeklyBoss 신화 보장

- [ ] Endgame 잠금 해제 (CH 35 클리어)
- [ ] 주간 보스 진입
- [ ] WeeklyBoss Phase 1 → Phase 2 전환
- [ ] 처치
- [ ] 콘솔 로그 `[MythicMileageService] Mythic guaranteed (chance=1.0)` 확인
- [ ] 신화 발현 토스트 + MythicEffectOverlay 무지개 fade
- [ ] 보상 ★500 + ⋆20 확인

## 비기능 검증

- [ ] 기존 weapon_v2 가챠 회귀 X (10연차 정상)
- [ ] 기존 stats_v2 데미지 공식 정상 (PIVOT v4 다중 보정)
- [ ] 기존 Daily/Weekly Endgame 호환 (BossEncounterService.HandleDailyBossClear)
- [ ] 60fps 유지 (cap 300 환경, Profiler 권장)
- [ ] 빌드 (Unity Build) 성공 (해당 시)

## 합격 판정

| 결과 | 조건 |
|---|---|
| **최소 합격** | 자동 모두 ✅ + C2 8/10+ + 시나리오 1, 2 PASS |
| **완전 합격** | 자동 모두 ✅ (C2 10/10) + 시나리오 1, 2, 3 모두 PASS + 비기능 모두 ✅ |
| **FAIL** | 자동 검증 1개라도 ❌ → Phase 5 재진입 (반복 max 2회) |

## 변경 이력
- 2026-05-04: 초기 작성 (Phase 4 Contract Checklist)
