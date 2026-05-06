# 5023 Sprint PRD v2 — OOP MetaProgression Split

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5023_oop_metaprogression_split_prd.md` · Decisions: `5023_oop_metaprogression_split_decisions.md`
> 결정 3건 반영 (D1=C, D2=C, D3=B)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Loader + Bootstrapper 분리 (5021 ResolveSelectedCharacter 흡수) | 1 | 30분 |
| 2 | Character + Chapter service + 호출처 마이그레이션 (D1=C) + 정적 event (D3=B) | 1 | 45분 |
| 3 | Currency + Equipment service + 호출처 마이그레이션 | 1 | 60분 |
| 4 | Endgame + NUnit 6 + Validator 룰 (D2=C) + changelog | 1 | 30분 |
| **합계** | | **4** | **2h45m** |

---

## 2. Sprint 1 — Loader + Bootstrapper

### 2.1 MetaProgressionLoader

**파일**: `Meta/MetaProgressionLoader.cs` (~50L)
- `Load()` → SaveSystem.Load + SaveDataMigration.Apply (5020)
- `ResolveSelectedCharacterFromSave(CharacterPool, SaveData)` (5021 흡수)

### 2.2 MetaProgressionBootstrapper

**파일**: `Meta/MetaProgressionBootstrapper.cs` (~70L)
- BootstrapItemDropService (Editor)
- BootstrapIAP / BootstrapStarlightGrant

### 2.3 MetaProgression Awake 슬림화

```csharp
private void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
    Data = MetaProgressionLoader.Load();
    SelectedCharacter = MetaProgressionLoader.ResolveSelectedCharacterFromSave(_characterPool, Data);
    MetaProgressionBootstrapper.Run();
}
```

---

## 3. Sprint 2 — Character + Chapter (D3=B 정적 event)

### 3.1 MetaProgression event 정적 변환 (D3=B)

```csharp
// MetaProgression.cs
public static event System.Action<CharacterData> OnCharacterChanged;     // instance → static
public static event System.Action<CharacterClass> OnCharacterClassChanged;
public static event System.Action<MetaUpgradeData> OnUpgradeChanged;
public static event System.Action<EnemyData> OnEnemyDefeated;
public static event System.Action<int> OnStarFragmentsChanged;
```

### 3.2 MetaProgressionCharacter

**파일**: `Meta/MetaProgressionCharacter.cs` (~50L)
- `SelectCharacter(CharacterData)` 이전
- `MetaProgression.OnCharacterChanged?.Invoke(...)` 발행
- `ResolveCharacterClass` helper

### 3.3 MetaProgressionChapter

**파일**: `Meta/MetaProgressionChapter.cs` (~30L)
- `SelectedChapter` 위임
- `activeChapterId/Wave` 관리

### 3.4 호출처 마이그레이션 (D1=C Sprint 2)

```bash
grep -rn "MetaProgression.Instance.SelectCharacter\|MetaProgression.Instance.SelectedChapter" \
  LittleHero/Assets/_Project/Scripts/

# 추정 ~5 호출처
# AtticCharacterSelector / AtticChapterEntry / WondrousMapPanel 등
```

---

## 4. Sprint 3 — Currency + Equipment

### 4.1 MetaProgressionCurrency

**파일**: `Meta/MetaProgressionCurrency.cs` (~80L)
- AddStarFragment / SpendStarFragment
- AddStarDust / SpendStarDust
- AddMagicCrystal / SpendMagicCrystal
- AddStarlight / SpendStarlight

### 4.2 MetaProgressionEquipment

**파일**: `Meta/MetaProgressionEquipment.cs` (~70L)
- UnlockEquipment / GetEquipmentInstances
- LockEquipment (monetization §7.1)
- AddMythicMileage / GetMythicMileage

### 4.3 호출처 마이그레이션 (D1=C Sprint 3)

```bash
grep -rn "MetaProgression.Instance\." LittleHero/Assets/_Project/Scripts/
# 추정 ~20 호출처 (Currency / Equipment)
```

---

## 5. Sprint 4 — Endgame + NUnit + Validator (D2=C)

### 5.1 MetaProgressionEndgame

**파일**: `Meta/MetaProgressionEndgame.cs` (~50L)
- DailyBoss / WeeklyBoss 클리어 시각
- InfiniteDepth / Hardcore

### 5.2 MetaProgression.cs 최종

**파일**: `Meta/MetaProgression.cs` (~80L)
- Singleton + Awake + Data prop + 정적 events 만 보존

### 5.3 NUnit 6 신규

**파일**: `Tests/Editor/MetaProgressionServicesTests.cs` (~60L)
- Currency 4 (AddStarFragment / SpendStarDust 등)
- Character event 발행 2 (OnCharacterChanged)

### 5.4 ProjectValidator 룰 추가 (D2=C)

`Editor/ProjectValidator/Rules/SaveCallRule.cs` 또는 OOPPolicyRule 에 통합:
- 7 service 의 모든 변경 메서드 마지막에 `SaveSystem.Save` 호출 검증 (간단 grep)

---

## 6. 의존성 그래프

```
Sprint 1 (Loader/Bootstrap) → Sprint 2 (Char/Chapter+event) → Sprint 3 (Currency/Equip) → Sprint 4 (Endgame/Test/Validator)
```

---

## 7. 완료 정의

- [ ] MetaProgression.cs ≤ 100L
- [ ] 7 service 100L 미만
- [ ] 호출처 30+ 마이그레이션 완료 (grep 0건)
- [ ] 정적 event 5종 변환 완료 (D3=B)
- [ ] C2 NUnit 120 → 126 PASS
- [ ] C3 errors=0 + 신규 SaveCall 룰 PASS
- [ ] `/prd-evaluate 5023` PASS

---

## 8. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | v2 (Mediator). D1=C / D2=C / D3=B 반영. 4 Sprint / 4 commit / 2h45m |
