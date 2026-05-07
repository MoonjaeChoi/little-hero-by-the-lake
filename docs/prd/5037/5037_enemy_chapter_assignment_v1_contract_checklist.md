# Evaluator Checklist: 5037 enemy_chapter_assignment_v1

## 자동 검증

### C1 컴파일
- [ ] `mcp__UnityMCP__refresh_unity` errors=0
- [ ] isCompiling=false (5초 내)

### C2 Editor Test (NUnit 6)
- [ ] `ChapterVariantTests.Variant_Stationary_Has_BehaviorType_Stationary` PASS
- [ ] `ChapterVariantTests.Variant_Splitter_Has_ChildData_Configured` PASS
- [ ] `ChapterVariantTests.Variant_Bomber_Has_ExplodeReaction` PASS
- [ ] `ChapterVariantTests.Chapter03_Wave10_MiniBossPool_Contains_Stationary` PASS
- [ ] `ChapterVariantTests.Chapter04_Wave5_MiniBossPool_Contains_Splitter` PASS
- [ ] `ChapterVariantTests.Chapter05_Wave15_MiniBossPool_Contains_Bomber` PASS

### C3 ProjectValidator
- [ ] errors=0
- [ ] warnings 변동 ≤ +2 (projectilePrefab null 의도 warning)
- [ ] `EnemyVariantConsistencyRule` 호출 (룰 list 등록)

### C4 Reflection
- [ ] `ChapterVariantSeeder.SeedAndWire` (정적)
- [ ] `ChapterVariantSeeder.LoadChapterByNumber` (정적)
- [ ] `VariantSoFactory.EnsureAllVariants` (정적)
- [ ] `EnemyVariantConsistencyRule.Name + Validate`

### C5 SO 검증
- [ ] 6 Variant SO 모두 존재 (Variants/ 폴더)
- [ ] 필드 wiring 정확 (behaviorType / splitterChildData / bomberRadius 등)
- [ ] CH3/4/5 ChapterData.miniBossPool 변형 SO 포함

## 수동 검증 (사용자 Play Mode)

### C6.1 CH3 wave 10 — Stationary
- [ ] mini-boss Stationary 출현
- [ ] 위치 고정 (linearVelocity ≈ 0)

### C6.2 CH4 wave 5 — Splitter
- [ ] mini-boss Splitter 출현
- [ ] 처치 시 자식 3마리 0.5u 분산

### C6.3 CH5 wave 15 — Bomber
- [ ] mini-boss Bomber 출현
- [ ] 1.5u 진입 시 telegraph + 자폭

## 비기능 검증

- [ ] 5035 IEnemyBehavior 회귀 0 (NUnit 12 PASS)
- [ ] 5036 DevUnlockAllChapters 동작 유지

## 합격 판정

- 자동 모두 ✅ + 시나리오 1+2 ✅ → **최소 합격**
- 자동 모두 ✅ + 시나리오 1+2+3 모두 ✅ → **완전 합격**
- 자동 1개라도 ❌ → **FAIL** (Phase 5 수정)
