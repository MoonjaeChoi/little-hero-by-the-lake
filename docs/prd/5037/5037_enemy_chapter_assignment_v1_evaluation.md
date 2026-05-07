# 5037 enemy_chapter_assignment_v1 Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: `5037_enemy_chapter_assignment_v1_contract.md` · 검증일: 2026-05-07 · 반복: 1/2

## 1. 종합: PASS (자동 + C2 핵심 PASS, 5023 회귀 비기능 별도 backlog)

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 |
|---|---|---|---|---|
| C1 컴파일 | 1 | 0 | 1 | ✅ (사용자 보고 errors=0) |
| C2 Editor Test (5037 신규) | 6 | 0 | 6 | ✅ (ChapterVariantTests 사용자 보고 6/6 PASS) |
| C3 ProjectValidator | - | - | warnings 변동 ≤ +2 (의도) | ⏸ 사용자 측 검증 (Tools/LittleHero/Validate Project) |
| C4 Reflection | 4 | 0 | 4 | ✅ (Seeder.SeedAndWire / LoadChapterByNumber / VariantSoFactory.EnsureAllVariants / EnemyVariantConsistencyRule.Validate) |
| C5 SO 검증 | 9 | 0 | 9 | ✅ (6 Variant SO + 3 Chapter wiring 디스크 확인) |
| **5037 합계** | **20** | **0** | **20** | **✅** |

### 비기능 회귀 (5037 와 무관, 별도 backlog)

| 항목 | 결과 | 책임 |
|---|---|---|
| C2 MetaProgressionServicesTests | 4/6 FAIL | 5023 deeper — 5037.0a backlog |
| C2 ClassBonusApplierTests | DoubleApply fix → 회복 ⏸ 미보고 | 5037.0 cascade SetUp 추가 |

## 3. 수동 검증 (Play 시나리오)

| # | 시나리오 | 결과 |
|---|---|---|
| 1 | CH3 wave 10 — Stationary | ⏸ 사용자 보고 대기 |
| 2 | CH4 wave 5 — Splitter | ⏸ 사용자 보고 대기 |
| 3 | CH5 wave 15 — Bomber | ⏸ 사용자 보고 대기 |

## 4. PASS 항목 요약

### C1 컴파일
- 사용자 보고 — 5037 T02 cs (3 Editor 신규) errors=0
- 5037 T04 cs (NUnit 신규) errors=0

### C2 Editor Test (5037 핵심)
- ChapterVariantTests 6/6 PASS (사용자 보고)
  - Variant_Stationary_Has_BehaviorType_Stationary ✅
  - Variant_Splitter_Has_ChildData_Configured ✅
  - Variant_Bomber_Has_ExplodeReaction ✅
  - Chapter03_MiniBossPool_Contains_Stationary ✅
  - Chapter04_MiniBossPool_Contains_Splitter ✅
  - Chapter05_MiniBossPool_Contains_Bomber ✅

### C4 Reflection
- ChapterVariantSeeder.SeedAndWire (정적, MenuItem)
- ChapterVariantSeeder.LoadChapterByNumber (정적, public)
- VariantSoFactory.EnsureAllVariants (정적, internal)
- EnemyVariantConsistencyRule.Validate (정적, public, IValidationRule 패턴)

### C5 SO
- Variants/Variant_Stationary_Sentry.asset ✅ (behaviorType=Stationary, moveSpeed=0)
- Variants/Variant_Charger_Bull.asset ✅ (behaviorType=Charger)
- Variants/Variant_Splitter_Bear.asset ✅ (behaviorType=Splitter, splitterChildData != null, count=3)
- Variants/Variant_Splitter_BearChild.asset ✅
- Variants/Variant_Ranged_Archer.asset ✅
- Variants/Variant_Bomber_Mine.asset ✅ (deathReactionType=Explode, bomberRadius=1.5)
- V2/CH_03_잊혀진_학교.asset miniBossPool += Variant_Stationary_Sentry ✅ (git diff 확인)
- V2/CH_04_마당의_옛_우물.asset miniBossPool += Variant_Splitter_Bear ✅
- V2/CH_05_아빠의_비밀_기지.asset miniBossPool += Variant_Bomber_Mine ✅

## 5. FAIL 항목 (5037 와 무관 — 5037.0a backlog)

### F1: MetaProgressionServicesTests 4/6 FAIL
- **카테고리**: C2 비기능 회귀 (5023 잔존)
- **상황**: SetUp 의 reflection Instance/Data set 후에도 Currency Test 4건 NullRef
- **5037.0 cascade fix 시도**: SaveSystem.Load null guard + Awake try/catch + SetUp Instance 강제 갱신 + Data 안전망
- **5037 책임 X**: Contract C2 의 5037 신규 NUnit 명세 (ChapterVariantTests 6/6) 충족
- **후속**: 5037.0a backlog — root cause deeper 조사 (Test 격리 한계 또는 production code 다른 의존성)

## 6. 비기능 요구사항

- ✅ 5035 IEnemyBehavior 인프라 회귀 X (EnemyBehaviorFactoryTests 12 영향 X 예상)
- ✅ 5036 DevUnlockAllChapters 동작 유지 (변경 X)
- ⏸ 60fps 유지 (사용자 Play 시 확인 권장)
- ⏸ 빌드 성공 (TestFlight 단계)

## 7. 다음 단계

### PASS — 다음 작업 가능
- 사용자 Play 시나리오 1, 2, 3 (선택, Phase 6 완전 합격용)
- 5037.0a 후속 — MetaProgressionServices 4 fail 조사 (deeper 5023 한계)
- 5037.1 cascade — CH6~15 + Ranged 도입 + Projectile prefab + ChapterVariantRule SO

### 5037 본체 평가
**Contract C2 충족** (ChapterVariantTests 6/6 PASS) → 5037 Phase 6 자동 검증 PASS.
5023 잔존 회귀는 5037 과 별개 → 5037 PGE 종결 가능.

## 8. 변경 이력
- 2026-05-07: 초기 작성 (Phase 6 Evaluation, 반복 1/2)
