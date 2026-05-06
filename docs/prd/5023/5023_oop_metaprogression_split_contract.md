# Sprint Contract: 5023 OOP MetaProgression Split

> **PGE Planner — Phase 4 Contract**
> PRD: `5023_oop_metaprogression_split_v2.md`
> 생성일: 2026-05-06

---

## 1. C1 컴파일 기준

- errors=0 모든 신규/수정 파일

**신규/수정**:
- (신규) `Meta/MetaProgressionLoader.cs` ≤ 60L
- (신규) `Meta/MetaProgressionBootstrapper.cs` ≤ 80L
- (신규) `Meta/MetaProgressionCharacter.cs` ≤ 60L
- (신규) `Meta/MetaProgressionChapter.cs` ≤ 40L
- (신규) `Meta/MetaProgressionCurrency.cs` ≤ 90L
- (신규) `Meta/MetaProgressionEquipment.cs` ≤ 80L
- (신규) `Meta/MetaProgressionEndgame.cs` ≤ 60L
- (신규) `Tests/Editor/MetaProgressionServicesTests.cs` ≤ 70L (NUnit 6)
- (수정) `Meta/MetaProgression.cs` (346L → ~80L) — Singleton + Awake + Data + 정적 events
- (수정) 호출처 30+ (Singleton.Instance.X → Service.X 또는 정적 event)
- (수정) `Editor/ProjectValidator/Rules/<X>.cs` — SaveCall 룰 추가 (D2=C)

---

## 2. C2 Editor Test 기준

- NUnit 6 신규 PASS (120 → 126)
- 기존 120 회귀 0

**MetaProgressionServicesTests 6 명세**:
- `Currency_AddStarFragment_DataUpdated`
- `Currency_SpendStarDust_DataUpdated`
- `Currency_AddStarlight_DataUpdated`
- `Currency_AddMagicCrystal_DataUpdated`
- `Character_OnCharacterChanged_StaticEvent_Fires`
- `Character_SelectCharacter_UpdatesData`

---

## 3. C3 ProjectValidator 기준

- errors=0 (5022 baseline 동일)
- warnings 변동 ≤ 0 (172 baseline) — 또는 +1 (5023 신규 SaveCall 룰)
- D2=C 신규 룰 PASS

---

## 4. C4 Reflection 검증

| Type | 검증 |
|---|---|
| `LittleHero.MetaProgressionLoader` (static) | `Load()` / `ResolveSelectedCharacterFromSave(CharacterPool, SaveData)` |
| `LittleHero.MetaProgressionBootstrapper` (static) | `Run()` |
| `LittleHero.MetaProgressionCharacter` (static) | `SelectCharacter(CharacterData)` |
| `LittleHero.MetaProgressionChapter` (static) | `SetSelectedChapter` 또는 `SelectedChapter` setter |
| `LittleHero.MetaProgressionCurrency` (static) | `AddStarFragment(int)` / `SpendStarFragment` / `AddStarDust` / `AddMagicCrystal` / `AddStarlight` |
| `LittleHero.MetaProgressionEquipment` (static) | `UnlockEquipment` / `LockEquipment` / `AddMythicMileage` |
| `LittleHero.MetaProgressionEndgame` (static) | `MarkDailyBossClear` / `MarkWeeklyBossClear` 등 |
| `LittleHero.MetaProgression` (static event) | `OnCharacterChanged` (D3=B static) |

---

## 5. C5 SO+Asset 검증

(N/A — 코드 리팩토링만)

---

## 6. C6 Play 시나리오

### 시나리오 1 — Currency 변경 (StarFragment 적용)

1. Attic Play
2. 적 처치 → StarFragment +X
3. **기대**: Console 로그 + SaveData starFragments 증가

### 시나리오 2 — Character 선택 → static event 발행

1. Attic Play → 시우 클릭
2. **기대**: AtticCharacterSelector "선택: 전사 시우" 라벨 즉시 갱신 (정적 event 구독자 영향 0)

### 시나리오 3 — 5021 시나리오 회귀 0

1. save.json 옛 id 주입 → Play → 자동 마이그레이션 (5020+5021 cascade)

---

## 7. 합격 기준

### 최소
- C1 ✅ + C2 NUnit 6 ✅ + C3 errors=0 ✅
- C6 시나리오 1+2

### 완전
- 모든 카테고리 + C6 시나리오 1+2+3

---

## 8. Trade-off

- D1=C Sprint 별 분리 → 위험 격리
- D2=C ProjectValidator 룰 추가 → 자동 회귀 안전망
- D3=B 정적 event → 구독자 호환성 ↑

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 6 카테고리 + 3 시나리오 + 합격 2단 |
