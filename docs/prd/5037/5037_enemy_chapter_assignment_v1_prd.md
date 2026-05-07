# 5037 enemy_chapter_assignment_v1 — Sprint PRD

> **Phase 1 Generate 산출** — 디자인 `docs/prd/5037_enemy_chapter_assignment_v1.md` 답습
> 일시: 2026-05-07 · 5 Sprint / 5 commit / 약 2h00m

---

## 1. Sprint 표

| # | 영역 | 작업 | 산출 | commit | 시간 |
|---|---|---|---|---|---|
| S1 | Data | 6 Variant SO + Variants 폴더 | 6 .asset (Stationary/Charger/Splitter+Child/Ranged/Bomber) | 1 | 25m |
| S2 | Editor | 시드 도구 + Validator 룰 | `ChapterVariantSeeder.cs` + 2 ProjectValidator 룰 | 1 | 30m |
| S3 | Data | CH3/4/5 wiring (시범) | Chapter_03/04/05.asset miniBossPool 갱신 | 1 | 15m |
| S4 | Test | NUnit (Variant SO + Rule 검증) | `ChapterVariantTests.cs` (6 NUnit) | 1 | 25m |
| S5 | Docs | changelog + CLAUDE.md §11 §14 | `changelog_v1.md` + CLAUDE.md | 1 | 15m |

**합계**: 5 Sprint / 5 commit / 1h50m

---

## 2. 신규 클래스 명세

### 2.1 Editor — `ChapterVariantSeeder.cs`

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ChapterVariantSeeder.cs`
**예상 라인**: ~120 (100줄+ 위반 — `Variants` + `Wiring` 두 정적 클래스로 분리 / 또는 부분 메서드 그룹화)

**명세**:
```csharp
public static class ChapterVariantSeeder {
    [MenuItem("Tools/LittleHero/Seed Chapter Enemy Variants")]
    public static void SeedAndWire();  // S1 + S3 통합 진입점

    private static EnemyData EnsureVariant(string id, string display, EnemyBehaviorType bt, ...);
    private static void WireChapterMiniBoss(int chapterNum, int wave, EnemyData variant);
    private static EnemyData LoadOrCreateVariant(string assetPath);
}
```

**주의**: §15.6 100줄 룰 — 메서드 짧게 분리 (개별 30~40 라인). 클래스 내 정적 헬퍼는 영역 grouped.

### 2.2 ProjectValidator 룰 신규 2종

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator/EnemyVariantConsistencyRule.cs` (5022 답습)
**예상 라인**: ~50

**명세**:
- `behaviorType == Splitter` 시 `splitterChildData != null` 검사
- `behaviorType ∈ {Stationary, Ranged}` 시 `projectilePrefab != null` 검사
- 위반 시 warning (errors=0 보장 — 5037 본체에서는 새 SO 만 점검)

### 2.3 Test — `ChapterVariantTests.cs`

**경로**: `LittleHero/Assets/_Project/Tests/Editor/ChapterVariantTests.cs`
**예상 라인**: ~110 (NUnit 클래스 — 100L 룰 예외)

**6 메서드**:
1. `Variant_Stationary_Has_BehaviorType_Stationary` — SO 로드 + 검사
2. `Variant_Splitter_Has_ChildData_Configured`
3. `Variant_Bomber_Has_ExplodeReaction`
4. `Chapter03_Wave10_MiniBossPool_Contains_Stationary`
5. `Chapter04_Wave5_MiniBossPool_Contains_Splitter`
6. `Chapter05_Wave15_MiniBossPool_Contains_Bomber`

> wave 단위 검사는 ChapterData.miniBossPool 에 변형 SO 가 포함되었는지로 대체 (wave-level 정확 분기는 5038 의 옵션 B 도입 시).

---

## 3. SO 인스턴스 (S1)

### 3.1 6 Variant 인스턴스

```
Assets/_Project/Data/Enemies/Variants/
  ├ Variant_Stationary_Sentry.asset    (HP 30, dmg 5, speed 0, behaviorType=1, projectilePrefab=null* — S2 완료 후 wiring)
  ├ Variant_Charger_Bull.asset         (HP 60, dmg 8, speed 2.5, behaviorType=2)
  ├ Variant_Splitter_Bear.asset        (HP 40, dmg 4, speed 1.5, behaviorType=3, splitterChildData=Variant_Splitter_BearChild)
  ├ Variant_Splitter_BearChild.asset   (HP 12, dmg 2, speed 1.8, behaviorType=0/Follow)
  ├ Variant_Ranged_Archer.asset        (HP 25, dmg 6, speed 1.2, behaviorType=4, projectilePrefab=null*)
  └ Variant_Bomber_Mine.asset          (HP 20, dmg 8, speed 1.8, behaviorType=5, deathReactionType=2/Explode, bomberRadius=1.5)
```

*projectilePrefab: 5035 시 별도 prefab 미생성 → 본체에서는 null 허용 (런타임 가드 — IEnemyBehavior 가 null 검사). 5037.1 cascade 시 Projectile prefab 별도.

### 3.2 ChapterData wiring (S3)

```
Chapter_03_Alice.asset       miniBossPool[] += Variant_Stationary_Sentry
Chapter_04_Aladdin.asset     miniBossPool[] += Variant_Splitter_Bear   (찾기 — 만일 파일명 다르면 chapterNumber=4 매칭)
Chapter_05_<TBD>.asset       miniBossPool[] += Variant_Bomber_Mine     (chapterNumber=5)
```

> `Chapter_05_*.asset` 파일명 — `find` 후 정확 명 결정 (S3 시 분석).

---

## 4. Sprint 의존성

```
S1 (Variant SO 6) ─┐
                   ├─→ S3 (Chapter wiring) ──┬─→ S4 (Test) ──→ S5 (Docs)
S2 (Seeder + Rule)─┘                         │
                                             │
            (S2 의 Validator 룰 → S4 회귀 검증)
```

**병렬 가능**: S1 + S2 동시 (자동 — 둘 다 ChapterData 변경 X)
**직렬**: S3 → S4 → S5 (Test 가 wiring 검증)

---

## 5. 검증 시나리오 (PRD → Phase 6 Contract C6 반영)

### 시나리오 1: CH3 wave 10 진입

**사전 조건**: SaveData firstClearedChapters bit 1+2 set (CH3 unlock) 또는 DevUnlockAllChapters
**단계**:
1. AtticController → 신기한 지도 → CH3 진입
2. wave 1~9 진행
3. wave 10 시작
**기대**:
- mini-boss 출현 (`Variant_Stationary_Sentry`)
- 위치 고정 + 1.5s 주기 발사체 (없으면 데미지 0 — 5037.1 cascade 후 정상)
- 콘솔 로그: `[StationaryBehavior] OnEnter` 또는 동등

### 시나리오 2: CH4 wave 5 진입 → 처치

**기대**:
- mini-boss `Variant_Splitter_Bear` 출현
- 처치 시 자식 3마리 (`Variant_Splitter_BearChild`) 0.5u 원형 분산
- 콘솔 로그: `[SplitterReaction] OnDeath, spawned 3 children`

### 시나리오 3: CH5 wave 15 진입 → bomberRadius 진입

**기대**:
- mini-boss `Variant_Bomber_Mine` 출현
- 추적 + 1.5u 진입 시 1s telegraph (warning vfx)
- 자폭 → ExplodeReaction → 데미지 + 별 흩어짐
- 콘솔 로그: `[BomberBehavior] Telegraph start` + `[ExplodeReaction] OnDeath`

---

## 6. CLAUDE.md §15 OOP 정합

| 정책 | 본 PRD |
|---|---|
| §15.1 SRP | Seeder 의 Variants/Wiring 분리 그룹화 (한 정적 클래스 100줄 이내 검토 — 위반 시 두 클래스로 분할) |
| §15.4 SO 우선 | 5 archetype 변형이 SO. 룰 매트릭스도 SO 화 검토 (5037.1 cascade 시) |
| §15.6 100줄 | Seeder ~120 → 두 클래스 분리 |
| §15.7 DI | Seeder 정적 (Editor only) — DI 불필요 |
| §15.9 Test | NUnit 6 신규 (Variant SO 검증 + ChapterData wiring 검증) |
| §15.11 안티 | public 필드 X / Update X (Editor only) |
| §15.12 Validator | 신규 룰 2 (EnemyVariantConsistency / ProjectileConsistency) |

---

## 7. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| projectilePrefab null → Stationary/Ranged 발사체 X | IEnemyBehavior 가드 + 5037.1 cascade 에서 Projectile prefab 추가 |
| 시드 도구 멱등성 — 중복 wiring 위험 | `if (Array.Contains(pool, variant)) return;` 검사 |
| Splitter 자식 분열 시 GC | 5035 답습 — `Object.Instantiate` (5035.1 backlog 에서 Pool 도입) |
| ChapterData 파일명 변동 | chapterNumber 필드로 매칭 (이름 무관) |
| 변형 SO icon 미설정 | 본체에서는 base SO icon reuse — 5039 backlog 에서 차별화 |

---

## 8. 변경 이력
- 2026-05-07: 초기 작성 (Phase 1 Generate)
