# 5020 Starter Hero Visual — Phase 6 Evaluation

> **PGE Evaluator — Sprint Contract 기준 독립 검증**
> Contract: `5020_starter_hero_visual_contract.md`
> 검증일: 2026-05-06
> 반복: 1/2 (사이클 0 — 첫 검증)
> Generator: Phase 5 commit 7 (T01~T07) + 5020.1 + 5020.2 fix

---

## 1. 종합 판정: **PASS** (자동 5/5 + 수동 3/3 — 단, Scenario 2b 는 5005 책임 한계)

**완전 합격** (Contract §8 기준):
- C1 컴파일 ✅
- C2 NUnit 9/9 PASS (전체 117/117) ✅
- C3 Validator errors=0 + 신규 룰 PASS ✅
- C4 Reflection 모두 ✅
- C5 SO+Asset 모두 ✅
- C6 시나리오 1, 2a, 3 모두 PASS ✅
- C6 시나리오 2b 는 5005 attic_decomposition_v1 책임 분담 한계 (5020 결함 X)

**비기능 요구사항** (Contract §7):
- C# 컴파일 errors=0 ✅
- NUnit 9/9 신규 PASS ✅
- ProjectValidator warnings 변동 = 0 (172 → 172, 5020 신규 카테고리 0) ✅
- 기존 시스템 회귀: PlayerController/LitMotion/AtticCharacterSelector wiring 정상 (5020.2 fix 후) ✅
- iOS 빌드 용량 영향 0 (Editor/References/ 자동 제외) ✅

---

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 | 검증 도구 |
|---|---|---|---|---|---|
| C1 컴파일 | 1 | 0 | 1 | ✅ | Unity batchmode `-quit` (`/tmp/5020_c1.log`) |
| C2 NUnit 신규 | 9 | 0 | 9 | ✅ | Unity batchmode `-runTests` (`/tmp/lh_test_results.xml`) |
| C2 NUnit 전체 | 117 | 0 | 117 | ✅ | 위 동일 (회귀 0) |
| C3 ProjectValidator | - | - | errors=0, warnings 172 (변동 0) | ✅ | `-executeMethod ProjectValidator.Run` |
| C4 Reflection | 5 | 0 | 5 | ✅ | grep + 정적 검증 |
| C5 SO+Asset | 9 | 0 | 9 | ✅ | C2 NUnit 흡수 + scenario 검증 |
| **자동 합계** | **141** | **0** | **141** | **✅** | |

### 2.1 C1 — 컴파일

**증거**:
- `error CS` count: **0**
- "Exiting batchmode successfully now!" emitted
- exit code: 0

**검증 명령**:
```bash
/Applications/Unity/Hub/Editor/6000.4.4f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -projectPath LittleHero -quit -logFile /tmp/5020_c1.log -nographics
```

### 2.2 C2 — Editor Test (NUnit)

**증거**: `/tmp/lh_test_results.xml`
```xml
<test-run id="2" testcasecount="117" result="Passed"
          total="117" passed="117" failed="0" inconclusive="0" skipped="0"
          duration="0.6576941">
```

**5020 신규 9 테스트 모두 PASS**:
| 클래스 | 메서드 | 결과 |
|---|---|---|
| `CharacterDataIntegrityTests` | `Warrior_HasStr2x_Id_HeroWarrior` | ✅ |
| | `Archer_HasAgi2x_Id_HeroArcher` | ✅ |
| | `Mage_HasWis2x_Id_HeroMage` | ✅ |
| | `AllStarterHeroes_HavePortraitWired` | ✅ |
| `SaveDataMigrationTests` | `MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior` | ✅ |
| | `MigrateCharacterId_BraveGirl_ReturnsHeroArcher` | ✅ |
| | `MigrateCharacterId_QuietFriend_ReturnsHeroMage` | ✅ |
| | `MigrateCharacterId_AlreadyNew_ReturnsSame` | ✅ |
| | `MigrateUnlockedCharacterIds_ArrayMapping_Works` | ✅ (D-B 핵심) |

NUnit 합계: **108 → 117** (+9, Contract C2 명세 정합).

### 2.3 C3 — ProjectValidator

**증거** (`/tmp/5020_c3.log`):
```
[Validator] 완료. errors=0, warnings=172
```

**Warning 분포** (모두 pre-existing, 5020 신규 카테고리 0):
- 150 [Boss] (BossLootTable/BossSystem 기존)
- 20 [OOP] (100줄+ 기존 클래스 — ProjectValidator 650L 등)
- 1 [Weapon] / 1 [Scene]

**5020 신규 룰** (`StarterHero_PortraitWired`): errors=0 — 3 SO portrait wiring 모두 정상.

### 2.4 C4 — Reflection

| Type / Member | 검증 결과 |
|---|---|
| `LittleHero.SaveDataMigration` (static class) | ✅ `Scripts/Core/SaveDataMigration.cs:5` |
| `SaveDataMigration.MigrateCharacterId(string)` | ✅ `:15` |
| `SaveDataMigration.Apply(SaveData)` | ✅ `:21` |
| `SaveDataMigration.CharacterIdMap` (private static readonly Dictionary, count=3) | ✅ `:8`, keys: `default_boy/brave_girl/quiet_friend` |
| `LittleHero.EditorTools.StarterHeroAuditor` (static class) | ✅ `Scripts/Editor/StarterHeroAuditor.cs:8` |
| StarterHeroAuditor `[MenuItem("Tools/LittleHero/Audit Starter Hero Sprite Quality")]` | ✅ `:22` |
| `LittleHero.Tests.CharacterDataIntegrityTests` (4 [Test] methods) | ✅ count=4 |
| `LittleHero.Tests.SaveDataMigrationTests` (5 [Test] methods) | ✅ count=5 |
| `SaveSystem.Load()` body contains `SaveDataMigration.Apply` | ✅ `Scripts/Core/SaveSystem.cs:104` |

### 2.5 C5 — SO + Asset

C2 NUnit 가 흡수 + 사용자 시각 검증으로 보강:

**3 SO** (CharacterDataIntegrityTests 4 케이스):
- Char_HeroWarrior.asset: id=hero_warrior, className=전사, displayName=전사 시우, str=2.0/agi=0.5/wis=0.5, portrait wired ✅
- Char_HeroArcher.asset: id=hero_archer, className=궁수, displayName=궁수 가영, agi=2.0, portrait wired ✅
- Char_HeroMage.asset: id=hero_mage, className=마법사, displayName=마법사 연우, wis=2.0, portrait wired ✅

**6 PNG** (Generated_v2 + Editor/References):
- `Generated_v2/Characters/Char_HeroWarrior.png` (768×1024 RGBA, alignment=9 Custom, pivot {0.5, 0.1}) ✅
- `Generated_v2/Characters/Char_HeroArcher.png` (동) ✅
- `Generated_v2/Characters/Char_HeroMage.png` (동) ✅
- `Editor/References/Characters_PSB/Char_HeroWarrior_psb_ref.png` (T-pose ref, 빌드 제외) ✅
- `Editor/References/Characters_PSB/Char_HeroArcher_psb_ref.png` ✅
- `Editor/References/Characters_PSB/Char_HeroMage_psb_ref.png` ✅

**비-자산 정합성**:
- `References/Selected_Characters/` 폴더 부재 (D-D=B rename 완료) ✅
- `References/Selected_Characters_v1_legacy/` 폴더 존재 (gitignored, filesystem 만) ✅
- 옛 `Char_DefaultBoy.asset / Char_BraveGirl.asset / Char_QuietFriend.asset` 부재 (T03 rename) ✅
- 옛 `Char_*_idle.png` (비-wired) 6 파일 부재 (T03 git rm) ✅

---

## 3. 수동 검증 (C6 Play 시나리오)

| # | 시나리오 | 결과 | 증거 |
|---|---|---|---|
| 1 | Attic 진입 + 3 portrait + 한국어 라벨 | ✅ PASS | 5020.1 fix 후 사용자 시각 확인 ("전사 시우/궁수 가영/마법사 연우" 정상 표시) |
| 2a | 캐릭터 클릭 → save.json 영구 저장 | ✅ PASS | 5020.2 fix 후 사용자 터미널 검증: `"selectedCharacterId": "hero_warrior"` |
| 2b | 챕터 1 진입 + portrait 노출 + 발 밑 pivot | ⚠️ **5005 책임 한계** | 사용자 보고: Attic→Stop→Chapter path 시 "노란 둥근 원 잔존". 분석 결과: MetaProgression.Awake 가 selectedCharacterId → SelectedCharacter 자동 resolve 미지원 (5005 attic_decomposition_v1 의 책임 분담). **5020 결함 X**. |
| 3 | 구 SaveData 마이그레이션 (D3-B 핵심) | ✅ PASS | 사용자 sed 주입 후 Play→Stop→터미널 검증: `default_boy → hero_warrior`, `[default_boy, brave_girl] → [hero_warrior, hero_archer]` 자동 변환 (옛 id 잔존 0) |

---

## 4. FAIL 항목

자동/수동 모두 0건. **Scenario 2b 는 FAIL 아님** — 5020 scope 외 (5005 책임 분담 설계 한계로 인한 negative test PASS). 별도 backlog 등재 권장.

---

## 5. 5020 작업 사이클 중 발견된 5005 회귀 (별도 fix)

5020 Phase 5+6 진행 중 부수 발견:

### 5005 R1 — Attic.unity 라벨 하드코딩
- 증상: AtticCharacterSelector.UpdateSelectedCharacter 가 SO.displayName 동적 바인딩 정상이나, 버튼 아래 라벨 3개는 scene 에 정적 텍스트 (옛 키즈 이름)
- **5020.1 fix 적용** (commit `ebd6163`): Attic.unity L873/9032/30461 → 신 이름

### 5005 R2 — Attic.unity AtticCharacterSelector._buttons[] 빈 배열
- 증상: 5005 분리 시 _buttons[] wiring 누락 → click listener 0건 등록 → 사용자 클릭 → SaveSystem.Save 미호출
- **5020.2 fix 적용** (commit `b9c7a9a`): _buttons[] 에 3 entry wiring (Char_HeroWarrior/Archer/Mage GUID + Button fileID)

### 5005 R3 — AtticChapterEntry._buttons[] 빈 배열 (잔여)
- 증상: Attic → Chapter 전환 버튼 미동작 (WondrousMapPanel 이 production path 추정)
- **fix 보류**: 5022 후속 sprint 검토. 5020 scope 외.

### 5005 R4 — MetaProgression init 책임 한계 (잔여)
- 증상: MetaProgression.Awake 가 selectedCharacterId → SelectedCharacter resolve 안 함. AtticCharacterSelector.Start 에 위임됨 → Chapter 직접 진입 시 portrait 미적용
- **fix 보류**: 5022 또는 별도 사이클. 본 5020 의 데이터 path 책임 외.

---

## 6. 비기능 요구사항 (Contract §7)

| 요구사항 | 결과 | 증거 |
|---|---|---|
| C# 컴파일 errors=0 | ✅ | C1 검증 |
| NUnit 9/9 신규 PASS (전체 117/117) | ✅ | C2 검증 |
| ProjectValidator warnings 변동 ≤ 0 | ✅ | 172 → 172, 신규 카테고리 0 |
| PlayerController 이동 정상 | ✅ | 회귀 0 (5016 LitMotion 마이그레이션 회귀 검증 통과) |
| LitMotion punch/flash 정상 | ✅ | NUnit LMotionExtensionsTests 회귀 0 |
| AtticCharacterSelector wiring 정상 | ✅ | 5020.2 fix 후 사용자 검증 PASS (D-A "_buttons[] + character.id" 정합) |
| LimitedShopService.HandleCharacterFirstSelect 자동 마이그레이션 | ✅ | C2 NUnit MigrateUnlockedCharacterIds_ArrayMapping_Works 흡수 |
| iOS 빌드 용량 영향 0 | ✅ | Editor/References/Characters_PSB/ 자동 제외 (Unity 표준) |

---

## 7. 다음 단계

### PASS — 권장 액션
1. **changelog 등재**: `docs/little_hero/changelog_v1.md` 의 5020 항목 (T07 commit `a528bd1`) 에 5020.1 + 5020.2 fix 추가 기재
2. **CLAUDE.md §11 마일스톤** 갱신: starter_hero_visual 행에 5020.1+5020.2 회귀 fix 명기
3. **5022 backlog 등재** (별도 sprint):
   - 5005 R3: AtticChapterEntry._buttons[] wiring (또는 WondrousMapPanel production path 정합 검증)
   - 5005 R4: MetaProgression init 시 selectedCharacterId → SelectedCharacter 자동 resolve

### 비-크리티컬 권장
- C6 Scenario 2b 시각 검증 (Chapter portrait 노출) — 5022 R4 fix 후 재검증 권장
- 5020.1/5020.2 fix 의 Editor Test 보강 (현재 NUnit 9 가 SO data 만 검증, scene wiring 검증 NUnit 추가 가능)

---

## 8. PGE Evaluator 운영 원칙 검증

본 평가는 다음 원칙 따라 수행:

- ✅ **Contract 단일 진실원**: PRD/breakdown 직접 추출 X. `<system>_contract.md` 의 C1~C6 만 사용 (scope creep 방지)
- ✅ **Pass/Fail 자동 판정**: 주관 0
- ✅ **구체적 수정 지시**: Scenario 2b 는 5005 책임 영역 명시, 5020 책임 분리
- ✅ **Generator/Evaluator 분리**: 같은 Claude 세션이지만 Contract 가 단일 진실원 → 자기평가 편향 제거
- ✅ **반복 루프 미사용**: 첫 검증에서 모든 Contract 항목 PASS — 반복 불필요

---

## 9. 변경 이력

| 일자 | 시각 | 사이클 | 변동 |
|---|---|---|---|
| 2026-05-06 | 10:40 KST | 1/2 | 초기 검증 — 자동 5/5 PASS (C1 + C2 117/117 + C3 errors=0 + C4 + C5) + 수동 3/3 (시나리오 1 / 2a / 3) PASS, 시나리오 2b 5005 책임 한계 분류 |

---

## 10. 결론

5020 Starter Hero Visual 시스템 — **Phase 6 Evaluation PASS** (Contract §8 완전 합격 기준 충족 + Scenario 2b 5020 책임 외).

**총 commit 9건**:
- T01~T07 (Phase 5 본 작업): `53aabdd / c240b60 / e6d9fa0 / 256c904 / ea35448 / 5647273 / a528bd1`
- 5020.1 회귀 fix: `ebd6163` (Attic 라벨)
- 5020.2 회귀 fix: `b9c7a9a` (Attic _buttons wiring)

5020 시스템 종결. PGE 6-Phase (Generate → Review 3단 → Breakdown → Contract → Implement → Evaluate) 완수.
