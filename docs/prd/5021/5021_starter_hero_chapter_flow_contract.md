# Sprint Contract: 5021 Starter Hero Chapter Flow

> **PGE Planner — Phase 4 Contract**
> PRD: `5021_starter_hero_chapter_flow_v2.md` (mediator 최종)
> Breakdown: `5021_starter_hero_chapter_flow_breakdown_00.md` + 5 태스크
> 생성일: 2026-05-06
> **목적**: 구현 전 객관 검증 기준 분리 정의 → Evaluator(Phase 6) 가 구현 코드를 보지 않고 결과만 검증.

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: 기존 어셈블리 (Assembly-CSharp / Assembly-CSharp-Editor / LittleHero.Tests.Editor) 모두 컴파일 OK

**신규 / 수정 파일**:
- (신규) `Data/CharacterPool.cs` ≤ 30L
- (신규) `Tests/Editor/MetaProgressionInitTests.cs` ≤ 60L
- (수정) `Meta/MetaProgression.cs` +15L (5023 cascade 일시 허용)
- (수정) `Editor/ProjectValidator.cs` +15L (ValidateCharacterPool 룰)

**검증 방법**:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c1
# 기대: error CS 0건 + "Exiting batchmode successfully"
```

---

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 수 | 위치 |
|---|---|---|
| `MetaProgressionInitTests` | 3 | `Tests/Editor/MetaProgressionInitTests.cs` |
| **합산 신규** | **3** | NUnit 117 → **120** |

### 2.1 MetaProgressionInitTests 3 명세

| Test 메서드 | 검증 |
|---|---|
| `ResolveCharacter_EmptyId_RemainsNull` | `pool.FindById("")` 와 `pool.FindById(null)` 모두 null 반환 |
| `ResolveCharacter_HeroWarrior_ResolvesToWarriorSO` | `pool.FindById("hero_warrior")` 가 Char_HeroWarrior SO 반환 (id=hero_warrior, className=전사) |
| `ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior` | `SaveDataMigration.MigrateCharacterId("default_boy")` → `pool.FindById(...)` → Char_HeroWarrior SO (5020+5021 통합) |

**검증 방법**:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c2 -f "MetaProgressionInitTests"
# 또는 전체:
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c2
# 기대: failed="0" in test-results.xml, total=120
```

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ≤ 0 (5021 시작 시점 기준 — 172)
- **C3.3**: 신규 위반 0건 (100줄+ / public 필드 / Update 멀티플)
- **C3.4 신규 룰**: `ValidateCharacterPool` 룰 PASS
  - CharacterPool 인스턴스 1+ 존재
  - characters[] 길이 3
  - id 매핑 (hero_warrior / hero_archer / hero_mage) 모두 존재

**검증 방법**:
```bash
bash ~/.claude/skills/little-hero-dev/scripts/verify_phase6.sh c3
# 기대: [Validator] 완료. errors=0, warnings ≤ 172
```

**Validator 룰 추가 명세**:
- Type: `LittleHero.EditorTools.ProjectValidator`
- 메서드: `private static void ValidateCharacterPool(List<Issue> issues)`
- 호출: `Run()` 내 `ValidateStarterHeroPortraits` 다음 위치

---

## 4. C4 Reflection 검증 (정적)

| Type | Member 검증 |
|---|---|
| `LittleHero.CharacterPool` (ScriptableObject) | `[CreateAssetMenu]` 부착, `characters` 필드 (CharacterData[]), `FindById(string) → CharacterData` |
| `LittleHero.MetaProgression` | `_characterPool` private SerializeField (CharacterPool), `ResolveSelectedCharacterFromSave()` private method |
| `LittleHero.MetaProgression.Awake` | 본문에 `ResolveSelectedCharacterFromSave` 호출 (string-level grep) |
| `LittleHero.Tests.MetaProgressionInitTests` | NUnit 클래스, 3 `[Test]` methods |
| `LittleHero.EditorTools.ProjectValidator.ValidateCharacterPool` | private static method (List<Issue> 인자) |

**검증 방법** (script 또는 reflection):
```bash
# CharacterPool 클래스 + 메서드 grep
grep -nE "public class CharacterPool|public CharacterData\[\] characters|public CharacterData FindById" \
  LittleHero/Assets/_Project/Scripts/Data/CharacterPool.cs

# MetaProgression hook
grep -nE "_characterPool|ResolveSelectedCharacterFromSave" \
  LittleHero/Assets/_Project/Scripts/Meta/MetaProgression.cs

# NUnit 메서드 카운트
grep -cE "\[Test\]" LittleHero/Assets/_Project/Tests/Editor/MetaProgressionInitTests.cs
# 기대: 3
```

---

## 5. C5 SO + Asset 검증

### 5.1 CharacterPool 인스턴스

| SO 파일 | 필드 검증 |
|---|---|
| `CharacterPool_Default.asset` | characters[] 길이 3, [0]=hero_warrior, [1]=hero_archer, [2]=hero_mage (순서 무관, 모두 존재) |

**검증 방법**: NUnit C2 (3 케이스) 흡수 + ProjectValidator C3.4 흡수.

### 5.2 Attic.unity scene wiring

| 필드 | 검증 |
|---|---|
| `MetaProgression._characterPool` | CharacterPool_Default GUID 정합 |
| `AtticChapterEntry` GameObject | `m_IsActive: 0` (D1=B 비활성화) |
| `WondrousMapPanel._chapters[]` | 35 ChapterData 모두 wiring (또는 추후 보강) |
| `WondrousMapPanel._nodes[]` | 35 WondrousMapNode 모두 wiring (또는 추후 보강) |

**검증 방법** (Bash grep):
```bash
# MetaProgression._characterPool wiring
grep -A 2 "AtticController\|MetaProgression$" LittleHero/Assets/_Project/Scenes/Attic.unity | grep -A 2 "_characterPool"
# 기대: fileID 11400000 + CharacterPool_Default GUID

# AtticChapterEntry 비활성화
grep -B 2 -A 6 "AtticChapterEntry$" LittleHero/Assets/_Project/Scenes/Attic.unity
# 기대: m_IsActive: 0 (또는 component 제거)

# WondrousMapNode 카운트 (이상적 35)
grep -c "WondrousMapNode$" LittleHero/Assets/_Project/Scenes/Attic.unity
# 기대: 35 (또는 ≥ 10 — 부분 wiring 도 acceptable, 5021.1 backlog)
```

### 5.3 비-자산 정합성

- [ ] `Data/CharacterPool.cs` 파일 존재
- [ ] `Data/Characters/CharacterPool_Default.asset` 파일 존재
- [ ] `Tests/Editor/MetaProgressionInitTests.cs` 파일 존재

---

## 6. C6 Play 검증 시나리오 (사용자 보고)

### 시나리오 1 — Attic 진입 + SelectedCharacter 자동 resolve

**사전 조건**:
- 5021 commit 5건 모두 완료
- save.json `selectedCharacterId="hero_warrior"` (5020 결과)
- Unity Editor Play Mode

**단계**:
1. Attic.unity 씬 Play 시작
2. Console 관찰
3. AtticCharacterSelector UI 관찰

**기대 결과**:
- Console: `[MetaProgression] ItemDrop bootstrap: templates=N slotPools=12` (정상)
- Console: Warning `_characterPool 미wiring` 없음 (정상)
- AtticCharacterSelector "선택: 전사 시우" 라벨 즉시 표시 (5020 시나리오 1 정합)
- 빨간 에러 0건

**검증 방법**: 사용자 Console + UI 보고

### 시나리오 2 — Chapter 직접 Play (5020 C6 2b 자동 PASS)

**사전 조건**:
- 5021 commit 5건 모두 완료
- save.json hero_warrior 보존
- Unity Editor (Domain Reload 후 새 Play session)

**단계**:
1. Attic Play → 1초 대기 → Stop
2. File > Open Scene → Chapter.unity
3. Play ▶
4. PlayerController spawn 위치 관찰

**기대 결과**:
- MetaProgression.Awake → `ResolveSelectedCharacterFromSave` → SelectedCharacter set (warrior)
- PlayerController.Start → `if (character != null)` 분기 진입 → `sr.sprite = character.portrait` 적용
- **Player sprite 가 시우 캐릭터 portrait** (노란 둥근 원 X)
- Color: warrior tintColor 적용 정상
- 좌우 이동 + flipX 정상

**검증 방법**: 사용자 시각 보고 — "노란 원 → 시우 sprite 정상 노출" 확인

### 시나리오 3 — WondrousMapPanel 챕터 진입 (D1=B 검증)

**사전 조건**:
- Attic Play (시나리오 1 상태에서 이어서 가능)

**단계**:
1. Attic 에서 다락방 진입 또는 WondrousMapPanel 활성화 path 클릭
2. 35 챕터 노드 표시 (또는 잠금 상태에 따라 일부)
3. 챕터 1 노드 클릭

**기대 결과**:
- 35 노드 모두 표시 (또는 잠금 상태 정상 — 챕터 1 만 잠금 해제)
- 챕터 1 클릭 → SceneTransitionService.Transition("Chapter") 호출
- Chapter 씬 fade-in + 로드
- PlayerController spawn 정상 (시나리오 2 와 동일)

**검증 방법**: 사용자 시각 + Console (`[SceneTransition]` 로그)

### 시나리오 4 — 5020 마이그레이션 + 5021 init 통합

**사전 조건**:
- Unity Editor Stop
- save.json sed 주입 (옛 형식)

**단계**:
1. 터미널:
   ```bash
   SAVE="$HOME/Library/Application Support/Bandibul/LittleHero/save.json"
   sed -i '' 's|"selectedCharacterId": "hero_warrior"|"selectedCharacterId": "default_boy"|' "$SAVE"
   grep selectedCharacterId "$SAVE"
   # 기대: "default_boy"
   ```
2. Unity Editor → Attic.unity 열고 Play ▶
3. 1초 대기 → Stop
4. 터미널 재확인:
   ```bash
   grep selectedCharacterId "$SAVE"
   # 기대: "hero_warrior" (자동 변환 + 자동 resolve + SelectCharacter 트리거 시 SaveSystem.Save)
   ```

**기대 결과**:
- Console NRE 0건
- save.json `selectedCharacterId: "hero_warrior"` 으로 자동 변환
- MetaProgression.SelectedCharacter 가 warrior 로 set (Awake 시점)
- Attic UI "선택: 전사 시우" 표시

**검증 방법**: 사용자 터미널 + Console + UI 보고

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0
- [ ] NUnit 3/3 신규 PASS (전체 120/120)
- [ ] ProjectValidator warnings 변동 ≤ 0 (172 기준)
- [ ] 기존 시스템 회귀 X:
  - 5020 마이그레이션 정상
  - 5020 NUnit 9 (CharacterDataIntegrityTests + SaveDataMigrationTests) PASS 유지
  - AtticCharacterSelector wiring 정상 (5020.2 fix 보존)
  - LimitedShopService.HandleCharacterFirstSelect 영향 X
- [ ] iOS 빌드 영향 0 (CharacterPool SO 추가만)

---

## 8. 합격 기준

### 최소 합격
- C1 컴파일 ✅
- C2 NUnit 3/3 PASS ✅
- C3 Validator errors=0 ✅
- C4 Reflection 모두 ✅
- C5 SO + scene wiring 모두 ✅
- C6 시나리오 1, 2 PASS (Attic 자동 resolve + Chapter 직접 portrait — 5020 C6 2b 핵심)

### 완전 합격
- 모든 카테고리 ✅
- C6 시나리오 1, 2, 3, 4 모두 PASS
- 비기능 요구사항 모두 ✅

---

## 9. Trade-off (mediator decisions 기반)

- **D1=B** (AtticChapterEntry 비활성화 + WondrousMapPanel 단일화) → Sprint 3 +30분 비용. dead code 정리 + path 명확. 5005 attic_decomposition_v1 정신 정합.
- **D2=C** (Sprint 1 의 2 commit 분리) → 회수 용이성 ↑ vs 1 commit 의 단순함 손실.
- **5023 cascade 일시 허용** (MetaProgression +15L = 346L) → 5023 Loader 분리 시 흡수 명시. §15.6 위반 일시.

---

## 10. Evaluator 운영

Phase 6 Evaluator 는 본 Contract 만 단일 진실원으로 사용:
- PRD / breakdown / decisions 직접 추출 X (scope creep 방지)
- Contract 의 Pass/Fail 기준만 평가
- FAIL 시 구체 수정 지시 (어느 카테고리 / 어느 항목 / 어느 파일)
- 반복 루프 max 2회 (FAIL → Phase 5 재진입)

---

## 11. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-contract Phase 4) | 초기 — 6 카테고리 (C1~C6), NUnit 3 신규, Play 시나리오 4, 합격 기준 2단 |
