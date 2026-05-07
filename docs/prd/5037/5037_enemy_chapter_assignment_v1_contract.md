# Sprint Contract: 5037 enemy_chapter_assignment_v1

> **PGE Planner — Phase 4 Contract**
> PRD: 5037_enemy_chapter_assignment_v1_v2.md · breakdown: _breakdown_00.md
> 생성일: 2026-05-07

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 (Seeder/Factory/Rule + ChapterVariantTests) 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5초 내 안정화

**검증**: `mcp__UnityMCP__refresh_unity` → 5초 → `read_console` errors=0

---

## 2. C2 Editor Test (NUnit) 기준

| Test 클래스 | 신규 메서드 | 목적 |
|---|---|---|
| `ChapterVariantTests` | 6 | Variant SO + Chapter wiring 검증 |

### 2.1 명세 (PRD v2 §2.3)

- `Variant_Stationary_Has_BehaviorType_Stationary` — `Variants/Variant_Stationary_Sentry.asset` behaviorType==Stationary
- `Variant_Splitter_Has_ChildData_Configured` — splitterChildData != null + splitterChildCount==3
- `Variant_Bomber_Has_ExplodeReaction` — deathReactionType==Explode + bomberRadius==1.5f
- `Chapter03_Wave10_MiniBossPool_Contains_Stationary` — chapterNumber 3 의 ChapterData.miniBossPool 에 `Variant_Stationary_Sentry` 포함
- `Chapter04_Wave5_MiniBossPool_Contains_Splitter` — chapterNumber 4 의 miniBossPool 에 `Variant_Splitter_Bear`
- `Chapter05_Wave15_MiniBossPool_Contains_Bomber` — chapterNumber 5 의 miniBossPool 에 `Variant_Bomber_Mine`

**합격**: 6/6 PASS

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: 신규 룰 `EnemyVariantConsistencyRule` 호출
- **C3.3**: warnings 변동 — `Variant_Stationary_Sentry` + `Variant_Ranged_Archer` 의 projectilePrefab=null 로 인한 **의도된 warning 2건** 만 추가 허용 (5037.1 cascade 보강 명시).

**검증**: `Tools/LittleHero/Validate Project` 실행 → warnings 변동 ≤ +2 (의도된 projectilePrefab null 만)

---

## 4. C4 Reflection 검증

| Type | Member |
|---|---|
| `LittleHero.EditorTools.ChapterVariantSeeder` (정적) | `SeedAndWire` (정적), `LoadChapterByNumber` (정적) |
| `LittleHero.EditorTools.VariantSoFactory` (internal 정적) | `EnsureAllVariants` (정적) |
| `LittleHero.EditorTools.EnemyVariantConsistencyRule` | `Name` (Property), `Validate` (Method) — IValidationRule 구현 |

**검증**: reflection 으로 Type/Method/Property 존재 확인.

---

## 5. C5 SO 검증

| SO | 인스턴스 | 필드 검증 |
|---|---|---|
| Variant_Stationary_Sentry | 1 | behaviorType==1, maxHealth==30, damage==5 |
| Variant_Charger_Bull | 1 | behaviorType==2, chargerInterval==3, chargerSpeedMultiplier==3 |
| Variant_Splitter_Bear | 1 | behaviorType==3, splitterChildData!=null, splitterChildCount==3 |
| Variant_Splitter_BearChild | 1 | behaviorType==0, maxHealth==12 |
| Variant_Ranged_Archer | 1 | behaviorType==4, rangedKeepDistance==4 |
| Variant_Bomber_Mine | 1 | behaviorType==5, deathReactionType==2, bomberRadius==1.5f |
| Chapter (chapterNumber 3) | ≥1 | miniBossPool 에 `Variant_Stationary_Sentry` 포함 |
| Chapter (chapterNumber 4) | ≥1 | miniBossPool 에 `Variant_Splitter_Bear` 포함 |
| Chapter (chapterNumber 5) | ≥1 | miniBossPool 에 `Variant_Bomber_Mine` 포함 |

**검증**: AssetDatabase.LoadAssetAtPath 또는 NUnit C2 의 일부와 중복 (간이 검증).

---

## 6. C6 Play 검증 시나리오 (수동)

### 시나리오 1: CH3 wave 10 — Stationary

**사전**: DevUnlockAllChapters=true 또는 firstClearedChapters bit 1+2 set
**단계**: AtticController → 신기한 지도 → CH3 진입 → wave 10
**기대**:
- mini-boss `Variant_Stationary_Sentry` 출현
- 위치 고정 (Rigidbody2D linearVelocity ≈ 0)
- 콘솔에 `[StationaryBehavior] Initialize` 또는 동등 (5035 답습)
- ※ projectilePrefab null → 발사체 없음 (의도) — 데미지 없음

### 시나리오 2: CH4 wave 5 — Splitter

**단계**: CH4 진입 → wave 5 → Splitter 처치
**기대**:
- mini-boss `Variant_Splitter_Bear` 출현
- 처치 시 `Variant_Splitter_BearChild` 3마리 0.5u 원형 분산
- 자식들이 Follow behavior 로 추적

### 시나리오 3: CH5 wave 15 — Bomber

**단계**: CH5 진입 → wave 15 → 가까이 접근
**기대**:
- mini-boss `Variant_Bomber_Mine` 출현
- 추적 + 1.5u 진입 시 1s warning vfx
- 자폭 → ExplodeReaction (별 흩어짐 + 데미지)

---

## 7. 비기능 요구사항

- [ ] 기존 5035 IEnemyBehavior 회귀 0 (NUnit 12 PASS 유지)
- [ ] 5036 DevUnlockAllChapters 동작 유지
- [ ] 60fps 유지 (Splitter 1→4 분열 시도 cap 영향 무시 — wave 5 / miniBoss 단일)
- [ ] 빌드 (Unity Build) 성공 (해당 시)

## 8. 합격 기준

**최소 합격**:
- C1 ✅, C2 6/6, C3 errors=0, C4 모두 통과, C5 6 SO 통과, 시나리오 1+2 PASS

**완전 합격**:
- 모든 카테고리 ✅, 시나리오 1+2+3 모두 PASS, 비기능 모두 ✅

## 9. Trade-off (mediator 결정 기반)

- **D1 통합** → ProjectValidator 룰 1 클래스 50L, 단일 응집 + §15.6 정합. SRP 강화 손실 작음.
- **D2 chapterNumber 스캔** → 파일명 무관, 35 챕터 reuse 강함. AssetDatabase 스캔 비용 무시 가능 (Editor only).
- **D3 정적 룰 (5037 본체)** → 단순성 우선, 5037.1 cascade SO 도입.

## 10. 변경 이력
- 2026-05-07: 초기 작성 (Phase 4 Contract)
