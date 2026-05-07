# 5037 enemy_chapter_assignment_v1 — Sprint PRD v2 (최종)

> **PRD v2 (Mediator 확정)** — 원본 `_prd.md` + Rebuttal `_rebuttal.md` + Decisions `_decisions.md`
> 결정 3건 (D1=A, D2=C, D3=A) + Seeder 분리 자동 수용 반영. 다음 단계: /prd-breakdown
> 일시: 2026-05-07 · 5 Sprint / 5 commit / 약 2h00m

---

## 1. Sprint 표

| # | 영역 | 작업 | 산출 | commit | 시간 |
|---|---|---|---|---|---|
| S1 | Data | 6 Variant SO + Variants 폴더 | 6 .asset (Stationary/Charger/Splitter+Child/Ranged/Bomber) | 1 | 25m |
| S2 | Editor | 시드 도구 (분리) + Validator 룰 1 | `ChapterVariantSeeder.cs` (60L) + `VariantSoFactory.cs` (60L) + `EnemyVariantConsistencyRule.cs` (50L) | 1 | 35m |
| S3 | Data | CH3/4/5 wiring (chapterNumber 스캔) | Chapter_03/04/05.asset miniBossPool 갱신 | 1 | 15m |
| S4 | Test | NUnit (Variant SO + Rule 검증) | `ChapterVariantTests.cs` (6 NUnit) | 1 | 25m |
| S5 | Docs | changelog + CLAUDE.md §11 §14 | `changelog_v1.md` + CLAUDE.md | 1 | 15m |

**합계**: 5 Sprint / 5 commit / 1h55m

---

## 2. 신규 클래스 명세

### 2.1 Editor — Seeder 분리 (D1=A 자동 + Seeder 분리)

#### A. `ChapterVariantSeeder.cs` (진입점, ~60L)

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ChapterVariantSeeder.cs`

```csharp
public static class ChapterVariantSeeder {
    [MenuItem("Tools/LittleHero/Seed Chapter Enemy Variants")]
    public static void SeedAndWire() {
        EnsureVariantsFolder();
        VariantSoFactory.EnsureAllVariants();          // S1 — 6 Variant SO 생성/갱신
        WireChapter(3, 10, "Variant_Stationary_Sentry");   // S3 — 시범 wiring
        WireChapter(4,  5, "Variant_Splitter_Bear");
        WireChapter(5, 15, "Variant_Bomber_Mine");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static void EnsureVariantsFolder() { ... }
    private static void WireChapter(int chapterNum, int wave, string variantId) {
        var ch = LoadChapterByNumber(chapterNum);
        if (ch == null) { Debug.LogWarning(...); return; }
        var variant = AssetDatabase.LoadAssetAtPath<EnemyData>(...);
        if (Array.IndexOf(ch.miniBossPool, variant) >= 0) return;  // 멱등
        ArrayUtility.Add(ref ch.miniBossPool, variant);
        EditorUtility.SetDirty(ch);
    }

    /// D2=C — chapterNumber 스캔 (35 챕터 reuse).
    public static ChapterData LoadChapterByNumber(int chapterNum) {
        var guids = AssetDatabase.FindAssets("t:ChapterData");
        foreach (var g in guids) {
            var ch = AssetDatabase.LoadAssetAtPath<ChapterData>(AssetDatabase.GUIDToAssetPath(g));
            if (ch != null && ch.chapterNumber == chapterNum) return ch;
        }
        return null;
    }
}
```

**예상 라인**: ~60 (§15.6 100L 정합 ✅)

#### B. `VariantSoFactory.cs` (정적 헬퍼, ~60L)

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/VariantSoFactory.cs`

```csharp
internal static class VariantSoFactory {
    private const string VARIANTS_PATH = "Assets/_Project/Data/Enemies/Variants";

    public static void EnsureAllVariants() {
        EnsureStationary("Variant_Stationary_Sentry", "포대 보초", 30f, 5f);
        EnsureCharger   ("Variant_Charger_Bull",      "돌진 황소",  60f, 8f);
        var child = EnsureFollow("Variant_Splitter_BearChild", "작은 곰", 12f, 2f);
        EnsureSplitter ("Variant_Splitter_Bear",      "분열 곰",  40f, 4f, child);
        EnsureRanged   ("Variant_Ranged_Archer",      "원거리 궁수", 25f, 6f);
        EnsureBomber   ("Variant_Bomber_Mine",        "지뢰",     20f, 8f);
    }

    private static EnemyData EnsureBase(string id, string display, float hp, float dmg) {
        var path = $"{VARIANTS_PATH}/{id}.asset";
        var so = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (so == null) { so = ScriptableObject.CreateInstance<EnemyData>(); AssetDatabase.CreateAsset(so, path); }
        so.id = id; so.displayName = display; so.maxHealth = hp; so.damage = dmg;
        EditorUtility.SetDirty(so);
        return so;
    }

    private static void EnsureStationary(...) { var so = EnsureBase(...); so.behaviorType = EnemyBehaviorType.Stationary; ... }
    private static void EnsureCharger(...) { ... }
    private static EnemyData EnsureFollow(...) { ... return so; }
    private static void EnsureSplitter(string id, ..., EnemyData child) { ... so.splitterChildData = child; }
    private static void EnsureRanged(...) { ... }
    private static void EnsureBomber(...) { ... so.deathReactionType = EnemyDeathReactionType.Explode; so.bomberRadius = 1.5f; }
}
```

**예상 라인**: ~60 (§15.6 정합 ✅)

### 2.2 ProjectValidator 룰 — D1=A 단일 클래스

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator/EnemyVariantConsistencyRule.cs`

```csharp
internal sealed class EnemyVariantConsistencyRule : IValidationRule {
    public string Name => "EnemyVariant.Consistency";
    public void Validate(ValidationContext ctx) {
        foreach (var so in ctx.LoadAllAssets<EnemyData>()) {
            if (so.behaviorType == EnemyBehaviorType.Splitter && so.splitterChildData == null)
                ctx.Warning(so, $"[{so.name}] Splitter behavior requires splitterChildData");
            if ((so.behaviorType == EnemyBehaviorType.Stationary || so.behaviorType == EnemyBehaviorType.Ranged)
                && so.projectilePrefab == null)
                ctx.Warning(so, $"[{so.name}] {so.behaviorType} behavior requires projectilePrefab (5037.1 cascade)");
        }
    }
}
```

**예상 라인**: ~50 (§15.6 정합 ✅)

> **5022 답습**: orchestrator `ProjectValidator.cs` 의 IValidationRule 인터페이스 + ctx.LoadAllAssets / ctx.Warning 활용.

### 2.3 Test — `ChapterVariantTests.cs`

**경로**: `LittleHero/Assets/_Project/Tests/Editor/ChapterVariantTests.cs`
**예상 라인**: ~110 (NUnit 클래스 — 100L 룰 예외)

**6 메서드**:
1. `Variant_Stationary_Has_BehaviorType_Stationary`
2. `Variant_Splitter_Has_ChildData_Configured` (splitterChildData != null + count==3)
3. `Variant_Bomber_Has_ExplodeReaction` (deathReactionType==Explode + bomberRadius==1.5f)
4. `Chapter03_Wave10_MiniBossPool_Contains_Stationary`
5. `Chapter04_Wave5_MiniBossPool_Contains_Splitter`
6. `Chapter05_Wave15_MiniBossPool_Contains_Bomber`

> wave 단위 검사는 ChapterData.miniBossPool 에 변형 SO 가 포함되었는지로 대체 (wave-level 정확 분기는 5038 의 옵션 B 도입 시).

---

## 3. SO 인스턴스 (S1)

### 3.1 6 Variant 인스턴스

```
Assets/_Project/Data/Enemies/Variants/
  ├ Variant_Stationary_Sentry.asset       (HP 30, dmg 5, speed 0, behaviorType=1, projectilePrefab=null*)
  ├ Variant_Charger_Bull.asset            (HP 60, dmg 8, speed 2.5, behaviorType=2, chargerInterval=3, chargerSpeedMultiplier=3)
  ├ Variant_Splitter_Bear.asset           (HP 40, dmg 4, speed 1.5, behaviorType=3, splitterChildData=Variant_Splitter_BearChild, splitterChildCount=3)
  ├ Variant_Splitter_BearChild.asset      (HP 12, dmg 2, speed 1.8, behaviorType=0/Follow)
  ├ Variant_Ranged_Archer.asset           (HP 25, dmg 6, speed 1.2, behaviorType=4, projectilePrefab=null*, rangedKeepDistance=4)
  └ Variant_Bomber_Mine.asset             (HP 20, dmg 8, speed 1.8, behaviorType=5, deathReactionType=2/Explode, bomberRadius=1.5, bomberDamage=30, bomberTelegraphSeconds=1)
```

*projectilePrefab: 5037 본체에서는 null (런타임 가드 — 5035 IEnemyBehavior 가 null 검사). 5037.1 cascade 시 Projectile prefab 별도.

### 3.2 ChapterData wiring (S3) — D2=C 스캔

Seeder 가 chapterNumber 매칭으로 ChapterData SO 발견 + miniBossPool 에 추가:

```
chapterNumber=3 → ChapterData (예 Chapter_03_Alice.asset) miniBossPool[] += Variant_Stationary_Sentry
chapterNumber=4 → ChapterData (chapterNumber 스캔) miniBossPool[] += Variant_Splitter_Bear
chapterNumber=5 → ChapterData (chapterNumber 스캔) miniBossPool[] += Variant_Bomber_Mine
```

> 파일명 무관 (D2=C 결정).

---

## 4. Sprint 의존성

```
S1 (Variant SO 6) ─┐
                   ├─→ S3 (Chapter wiring) ──┬─→ S4 (Test) ──→ S5 (Docs)
S2 (Seeder + Rule)─┘                         │
                                             │
            (S2 의 Validator 룰 → S4 회귀 검증)
```

**병렬 가능**: S1 + S2 동시
**직렬**: S2 → S3 (Seeder 가 wiring 도구) → S4 → S5

> 실제 S1 + S2 + S3 는 단일 시드 도구 1회 실행으로 일괄 적용 → 사실상 순차 (도구 호출 1회).

---

## 5. 검증 시나리오 (Phase 6 C6 반영)

### 시나리오 1: CH3 wave 10 진입

**사전 조건**: SaveData firstClearedChapters bit 1+2 set 또는 DevUnlockAllChapters
**단계**:
1. AtticController → 신기한 지도 → CH3 진입
2. wave 1~9 진행
3. wave 10 시작
**기대**:
- mini-boss 출현 (`Variant_Stationary_Sentry`)
- 위치 고정 (rb.linearVelocity≈0)
- 콘솔 로그: 적 spawn 시 `EnemyController.Initialize` + IEnemyBehavior=StationaryBehavior

### 시나리오 2: CH4 wave 5 진입 → 처치

**기대**:
- mini-boss `Variant_Splitter_Bear` 출현
- 처치 시 자식 3마리 (`Variant_Splitter_BearChild`) 0.5u 원형 분산
- 콘솔 로그: SplitterReaction.OnDeath 동작

### 시나리오 3: CH5 wave 15 진입 → bomberRadius 진입

**기대**:
- mini-boss `Variant_Bomber_Mine` 출현
- 추적 + 1.5u 진입 시 1s telegraph (warning vfx)
- 자폭 → ExplodeReaction → 데미지 + 별 흩어짐

---

## 6. CLAUDE.md §15 OOP 정합 (mediator 후 100% 정합)

| 정책 | 본 PRD v2 |
|---|---|
| §15.1 SRP | Seeder + Factory 분리 (자동 수용) ✅ |
| §15.4 SO 우선 | 5 archetype 변형 SO ✅. 룰 매트릭스 정적 (D3=A — 5037.1 cascade SO) △ |
| §15.6 100L | Seeder 60L + Factory 60L + Rule 50L 모두 정합 ✅ |
| §15.7 DI | Editor 정적 — 불필요 ✅ |
| §15.9 Test | NUnit 6 신규 ✅ |
| §15.11 안티 | public 필드 X / Update X (Editor) ✅ |
| §15.12 Validator | 신규 룰 1 (D1=A 통합 50L) ✅ |

---

## 7. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| projectilePrefab null → Stationary/Ranged 발사체 X | IEnemyBehavior 가드 + 5037.1 cascade 에서 Projectile prefab 추가 |
| Seeder 멱등성 — 중복 wiring | `Array.IndexOf(pool, variant) >= 0` 검사 ✅ |
| Splitter 자식 분열 시 GC | 5035 답습 — Object.Instantiate (5035.1 backlog Pool) |
| ChapterData 파일명 변동 | chapterNumber 스캔 (D2=C) ✅ |
| 변형 SO icon 미설정 | 본체에서 base SO icon reuse — 5039 backlog 차별화 |
| 룰 매트릭스 정적 (D3=A) | 5037.1 cascade 에서 ChapterVariantRule SO 도입 |

---

## 8. 후속 작업 (Backlog)

- **5037.1**: CH6~15 wiring + Ranged 도입 + Projectile prefab 신규 + ChapterVariantRule SO (D3 cascade)
- **5037.2**: CH16~25 wiring + Charger 도입
- **5037.3**: CH26~35 + Endgame Daily/Weekly Boss reuse
- **5038**: ChapterData.waves[20] WaveComposition 통합 (옵션 B 본격 채택)
- **5039**: Variant SO 의 sprite/tint 차별화

---

## 9. 변경 이력
- 2026-05-07: 초기 작성 (Phase 1 Generate)
- 2026-05-07: PRD v2 확정 (Phase 2.3 Mediator) — D1=A / D2=C / D3=A + Seeder 분리 자동 수용
