# 5023 Sprint PRD — OOP MetaProgression Split

> 디자인: `docs/prd/5023_oop_metaprogression_split_prd.md`
> Sprint: 4 / commit: 4 / 시간: ~2h15m
> 의존성: 5021 (ResolveSelectedCharacter 수용)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | Loader + Bootstrapper 분리 | 1 | 30분 |
| 2 | Character + Chapter 분리 | 1 | 30분 |
| 3 | Currency + Equipment 분리 | 1 | 45분 |
| 4 | Endgame 분리 + NUnit 회귀 | 1 | 30분 |
| **합계** | | **4** | **2h15m** |

---

## 2. Sprint 1 — Loader + Bootstrapper

### 2.1 MetaProgressionLoader

**파일**: `Meta/MetaProgressionLoader.cs` (신규, ~50L)
- `Load()` → SaveSystem.Load + SaveDataMigration.Apply (5020 흡수)
- `ResolveSelectedCharacterFromSave(CharacterPool, SaveData)` (5021 흡수)

### 2.2 MetaProgressionBootstrapper

**파일**: `Meta/MetaProgressionBootstrapper.cs` (신규, ~70L)
- `BootstrapItemDropService` (Editor) 이전
- `BootstrapIAP` / `BootstrapStarlightGrant` 이전

### 2.3 MetaProgression.Awake 슬림화

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

### Sprint 1 commit
```
5023 Sprint 1 / T01 — Loader + Bootstrapper 분리 (1/4)

[변경]
  + Meta/MetaProgressionLoader.cs (50L)
  + Meta/MetaProgressionBootstrapper.cs (70L)
  ~ Meta/MetaProgression.cs Awake 슬림화 (~80L → ~30L)

[검증]
  - C1 컴파일 errors=0
  - 5020 마이그레이션 + 5021 init NUnit PASS
```

---

## 3. Sprint 2 — Character + Chapter service

### 3.1 MetaProgressionCharacter

**파일**: `Meta/MetaProgressionCharacter.cs` (신규, ~50L)
- `SelectCharacter(CharacterData)` 이전
- `OnCharacterChanged` event 보존 (구독자 영향 X)
- `ResolveCharacterClass` helper

### 3.2 MetaProgressionChapter

**파일**: `Meta/MetaProgressionChapter.cs` (신규, ~30L)
- `SelectedChapter` setter
- `activeChapterId` / `activeChapterWave` 관리

### 3.3 호출처 마이그레이션

```bash
grep -rn "MetaProgression.Instance.SelectCharacter\|MetaProgression.Instance.SelectedChapter" \
  LittleHero/Assets/_Project/Scripts/
# 호출처 일괄 sed 또는 IDE refactor
```

### Sprint 2 commit
```
5023 Sprint 2 / T02 — Character + Chapter service (2/4)

[변경]
  + Meta/MetaProgressionCharacter.cs (50L)
  + Meta/MetaProgressionChapter.cs (30L)
  ~ MetaProgression.cs (SelectCharacter / SelectedChapter 제거)
  ~ N 호출처 마이그레이션 (예 AtticCharacterSelector / AtticChapterEntry)

[검증]
  - 컴파일 errors=0
  - C2 NUnit 117 회귀 0
```

---

## 4. Sprint 3 — Currency + Equipment service

### 4.1 MetaProgressionCurrency

**파일**: `Meta/MetaProgressionCurrency.cs` (신규, ~80L)
- AddStarFragment / SpendStarFragment
- AddStarDust / SpendStarDust (PIVOT v2 ★ 영구화)
- AddMagicCrystal / SpendMagicCrystal
- AddStarlight / SpendStarlight
- 각각 SaveSystem.Save 호출 + GameEvents 발행

### 4.2 MetaProgressionEquipment

**파일**: `Meta/MetaProgressionEquipment.cs` (신규, ~70L)
- UnlockEquipment / GetEquipmentInstances
- LockEquipment / IsEquipmentLocked (monetization §7.1)
- AddMythicMileage / GetMythicMileage

### 4.3 호출처 마이그레이션

CurrencyService / GachaService / EquipmentManager 등에서 호출. 일괄 마이그레이션.

### Sprint 3 commit
```
5023 Sprint 3 / T03 — Currency + Equipment service (3/4)

[변경]
  + Meta/MetaProgressionCurrency.cs (80L)
  + Meta/MetaProgressionEquipment.cs (70L)
  ~ MetaProgression.cs 화폐/장비 메서드 제거
  ~ N 호출처 마이그레이션

[검증]
  - 컴파일 errors=0
  - C2 NUnit 117 회귀 0
  - Play 시나리오: 별가루 처치 시 ★ 누적, 가챠 소환 시 ⋆ 차감
```

---

## 5. Sprint 4 — Endgame + NUnit 회귀

### 5.1 MetaProgressionEndgame

**파일**: `Meta/MetaProgressionEndgame.cs` (신규, ~50L)
- DailyBoss / WeeklyBoss 클리어 시각
- InfiniteDepth 갱신
- Hardcore 챕터 클리어 비트마스크

### 5.2 MetaProgression.cs 최종

**파일**: `Meta/MetaProgression.cs` (수정, ~80L)
- Singleton + Awake + Data + SelectedCharacter + SelectedChapter prop 만 보존
- 모든 비즈니스 로직 service 위임

### 5.3 NUnit 신규

**파일**: `Tests/Editor/MetaProgressionServicesTests.cs` (신규, ~60L)
- MetaProgressionCurrency 화폐 누적 NUnit 4
- MetaProgressionCharacter event 발행 NUnit 2
- 합산 117 → 123 PASS

### Sprint 4 commit
```
5023 Sprint 4 / T04 — Endgame + NUnit 회귀 (4/4) — 5023 종결

[변경]
  + Meta/MetaProgressionEndgame.cs (50L)
  + Tests/Editor/MetaProgressionServicesTests.cs (60L NUnit 6)
  ~ Meta/MetaProgression.cs 최종 (~80L, Singleton + Awake + props)

[검증]
  - C2 NUnit 117 → 123 PASS
  - C3 Validator OOP warning 변동 (MetaProgression 100L 미만 → 0)
  - changelog + CLAUDE.md
```

---

## 6. 의존성 그래프

```
Sprint 1 (Loader/Bootstrap) → Sprint 2 (Char/Chapter) → Sprint 3 (Currency/Equip) → Sprint 4 (Endgame/Test)
```

직렬. 각 Sprint 가 다음 Sprint 의 호출처 마이그레이션 baseline.

---

## 7. 완료 정의

- [ ] MetaProgression.cs ≤ 100L
- [ ] 7 service 모두 100L 미만
- [ ] 호출처 30+ 마이그레이션 완료 (grep 0건)
- [ ] C2 NUnit 117 → 123 PASS
- [ ] C3 OOP warning MetaProgression 항목 제거
- [ ] `/prd-evaluate 5023` PASS

---

## 8. 알려진 위험

| 위험 | 완화 |
|---|---|
| 호출처 마이그레이션 누락 | Sprint 별 grep 검증 |
| Save 트리거 누락 | service 메서드 마지막 SaveSystem.Save 명시 |
| event delegate 구독자 동작 깨짐 | OnCharacterChanged 이전 시 grep 으로 구독자 확인 |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 Sprint / 4 commit / 2h15m |
