# Sprint Contract: 5020 Starter Hero Visual

> **PGE Planner — Phase 4 Contract**
> PRD: `5020_starter_hero_visual_v2.md`
> Breakdown: `5020_starter_hero_visual_breakdown_00.md` + 7 태스크
> 생성일: 2026-05-06
> **목적**: 구현 전에 객관 검증 기준 분리 정의 → Evaluator(Phase 6) 가 구현 코드를 보지 않고 결과만 검증.

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: 기존 어셈블리 (Assembly-CSharp / Assembly-CSharp-Editor / LittleHero.Tests.Editor) 모두 컴파일 OK

**신규 / 수정 파일**:
- (신규) `Core/SaveDataMigration.cs` ≤ 64L
- (신규) `Editor/StarterHeroAuditor.cs` ≤ 80L
- (신규) `Tests/Editor/CharacterDataIntegrityTests.cs` ≤ 50L
- (신규) `Tests/Editor/SaveDataMigrationTests.cs` ≤ 60L
- (수정) `Core/SaveSystem.cs` Load() +1~2 줄
- (수정) `Editor/ProjectValidator.cs` +20 줄 (StarterHero_PortraitWired 룰)

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 10  # 또는 isCompiling 폴링
mcp__UnityMCP__read_console (errors)  # 0 이어야 함
```

---

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 수 | 위치 |
|---|---|---|
| `CharacterDataIntegrityTests` | 4 | `Tests/Editor/CharacterDataIntegrityTests.cs` |
| `SaveDataMigrationTests` | 5 | `Tests/Editor/SaveDataMigrationTests.cs` |
| **합산 신규** | **9** | NUnit 108 → **117** |

### 2.1 CharacterDataIntegrityTests 4 명세

| Test 메서드 | 검증 |
|---|---|
| `Warrior_HasStr2x_Id_HeroWarrior` | Char_HeroWarrior.asset: id=`hero_warrior`, className=`전사`, str=2.0, agi=0.5, wis=0.5 |
| `Archer_HasAgi2x_Id_HeroArcher` | Char_HeroArcher.asset: id=`hero_archer`, className=`궁수`, agi=2.0, str=0.5, wis=0.5 |
| `Mage_HasWis2x_Id_HeroMage` | Char_HeroMage.asset: id=`hero_mage`, className=`마법사`, wis=2.0, str=0.5, agi=0.5 |
| `AllStarterHeroes_HavePortraitWired` | 3 SO 의 portrait != null |

### 2.2 SaveDataMigrationTests 5 명세

| Test 메서드 | 검증 |
|---|---|
| `MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior` | `MigrateCharacterId("default_boy") == "hero_warrior"` |
| `MigrateCharacterId_BraveGirl_ReturnsHeroArcher` | `MigrateCharacterId("brave_girl") == "hero_archer"` |
| `MigrateCharacterId_QuietFriend_ReturnsHeroMage` | `MigrateCharacterId("quiet_friend") == "hero_mage"` |
| `MigrateCharacterId_AlreadyNew_ReturnsSame` | `MigrateCharacterId("hero_warrior") == "hero_warrior"` (idempotent) |
| `MigrateUnlockedCharacterIds_ArrayMapping_Works` | `Apply(data)` 후 `data.unlockedCharacterIds` 가 `["hero_warrior", "hero_archer"]` 으로 변환 (D-B) |

**검증 방법**: Test Runner Edit Mode > Run All → **9/9 PASS** (전체 117/117)

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ≤ 0 (5020 시작 시점 기준 비교)
- **C3.3**: 신규 위반 0건 (100줄+ / public 필드 / Update in N)
- **C3.4 신규 룰**: `StarterHero_PortraitWired` 룰 PASS (3 SO portrait != null)

**검증 방법**:
1. `Tools/LittleHero/Validate Project` 실행
2. 콘솔 출력 비교 (errors / warnings 카운트)
3. StarterHero_PortraitWired 항목이 errors 에 등장 X

**Validator 룰 추가 명세**:
- Type: `LittleHero.EditorTools.ProjectValidator`
- 메서드: `private static void ValidateStarterHeroPortraits(List<string> errors)` (또는 유사)
- 호출: `Tools/LittleHero/Validate Project` 메뉴에서 자동

---

## 4. C4 Reflection 검증 (정적)

| Type | Member 검증 |
|---|---|
| `LittleHero.SaveDataMigration` (static class) | `MigrateCharacterId(string)` static method (returns string), `Apply(SaveData)` static method (returns SaveData) |
| `LittleHero.SaveDataMigration.CharacterIdMap` | static readonly Dictionary<string, string>, count=3 (default_boy/brave_girl/quiet_friend keys) |
| `LittleHero.EditorTools.StarterHeroAuditor` (static class) | `Run()` static method with `[MenuItem("Tools/LittleHero/Audit Starter Hero Sprite Quality")]` |
| `LittleHero.Tests.CharacterDataIntegrityTests` | NUnit class, 4 `[Test]` methods |
| `LittleHero.Tests.SaveDataMigrationTests` | NUnit class, 5 `[Test]` methods |
| `LittleHero.SaveSystem.Load()` | 본문에 `SaveDataMigration.Apply` 호출 (string-level grep) |

**검증 방법** (script 또는 reflection):
```csharp
var t = System.Type.GetType("LittleHero.SaveDataMigration, Assembly-CSharp");
Assert.That(t, Is.Not.Null);
Assert.That(t.GetMethod("MigrateCharacterId"), Is.Not.Null);
Assert.That(t.GetMethod("Apply"), Is.Not.Null);

var dict = (System.Collections.IDictionary)t.GetField("CharacterIdMap",
    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
Assert.That(dict.Count, Is.EqualTo(3));
```

또는 Bash grep:
```bash
grep -q "SaveDataMigration.Apply" LittleHero/Assets/_Project/Scripts/Core/SaveSystem.cs && echo PASS
```

---

## 5. C5 SO + Asset 검증

### 5.1 CharacterData SO (3 신규 rename)

| SO 파일 | 필드 검증 |
|---|---|
| `Char_HeroWarrior.asset` | id=`hero_warrior`, className=`전사`, displayName=`전사 시우`, portrait != null, str=2.0, agi=0.5, wis=0.5, tintColor=(1.0, 0.85, 0.55) |
| `Char_HeroArcher.asset` | id=`hero_archer`, className=`궁수`, displayName=`궁수 가영`, portrait != null, agi=2.0, str=0.5, wis=0.5, tintColor=(0.95, 0.65, 0.85) |
| `Char_HeroMage.asset` | id=`hero_mage`, className=`마법사`, displayName=`마법사 연우`, portrait != null, wis=2.0, str=0.5, agi=0.5, tintColor=(0.55, 0.85, 0.95) |

**검증 방법**: NUnit C2.1 (4 케이스) 가 자동 검증.

### 5.2 PNG 자산 검증

| 파일 | 검증 |
|---|---|
| `Generated_v2/Characters/Char_HeroWarrior.png` | 768×1024 RGBA, alignment=Custom, pivot≈(0.5, 0.1) |
| `Generated_v2/Characters/Char_HeroArcher.png` | 동 |
| `Generated_v2/Characters/Char_HeroMage.png` | 동 |
| `Editor/References/Characters_PSB/Char_HeroWarrior_psb_ref.png` | 768×1024 RGBA (pivot 검증 X — 사용 안 함) |
| `Editor/References/Characters_PSB/Char_HeroArcher_psb_ref.png` | 동 |
| `Editor/References/Characters_PSB/Char_HeroMage_psb_ref.png` | 동 |

**검증 방법**: StarterHeroAuditor 실행 → 3/3 PASS + Bash 추가 검증:
```bash
ls LittleHero/Assets/_Project/Art/Editor/References/Characters_PSB/Char_Hero*_psb_ref.png | wc -l  # 3 이어야 함
ls LittleHero/Assets/_Project/Art/References/Selected_Characters_v1_legacy/ 2>/dev/null  # 폴더 rename 확인
```

### 5.3 비-자산 정합성

- [ ] `References/Selected_Characters/` 폴더 부재 (D-D=B rename 완료)
- [ ] `References/Selected_Characters_v1_legacy/` 폴더 존재
- [ ] 옛 `Char_DefaultBoy.asset` / `Char_BraveGirl.asset` / `Char_QuietFriend.asset` 부재 (rename 완료)
- [ ] 옛 `Char_*_idle.png` (비-wired) 부재 (T01 매핑 결과 git rm)

---

## 6. C6 Play 검증 시나리오 (사용자 보고)

### 시나리오 1 — Attic 진입 + 3 portrait 한국어 이름 라벨

**사전 조건**:
- Phase 5 commit 7건 모두 완료
- Unity Editor Play Mode

**단계**:
1. Attic.unity 씬 Play 시작
2. AtticCharacterSelector 진입
3. 좌우 스와이프 또는 캐릭터 버튼 클릭

**기대 결과**:
- 3 portrait 모두 V5 sticker 직업 정체성 (검 / 활 / 지팡이)
- displayName 라벨: "선택: 전사 시우" / "선택: 궁수 가영" / "선택: 마법사 연우"
- 폰트 잘림 없음 (한글 정상)
- 콘솔 에러 0건

**검증 방법**: 사용자 시각 + 콘솔 보고

### 시나리오 2 — 캐릭터 선택 → 챕터 1 진입 + pivot 정합

**사전 조건**: 시나리오 1 통과

**단계**:
1. 시우(전사) 선택
2. 챕터 1 진입
3. PlayerController spawn 위치 확인
4. 좌우 이동 → flipX 정상

**기대 결과**:
- PlayerController 신규 portrait 사용
- 발 밑 pivot (0.5, 0.1) 으로 정상 배치 (캐릭터 발이 ground 와 정확히 맞닿음)
- 이동 시 flipX 좌우 반전 정상
- LitMotion punch/flash 효과 정상 (회귀 X)

**검증 방법**: 사용자 시각 보고

### 시나리오 3 — 구 SaveData 마이그레이션 (D3-B 핵심)

**사전 조건**:
- Phase 5 commit 7건 모두 완료
- Unity Editor 또는 Build

**단계**:
1. `Tools/LittleHero/Reset SaveData` 실행 (또는 직접 save.json 삭제)
2. save.json 직접 편집 (옛 SaveData 시뮬):
   ```json
   {
     "selectedCharacterId": "default_boy",
     "unlockedCharacterIds": ["default_boy", "brave_girl"]
   }
   ```
3. Game 재시작
4. SaveSystem.Load() → SaveDataMigration.Apply() 자동 호출
5. Attic 진입

**기대 결과**:
- 콘솔 NRE 없이 정상 로드
- `selectedCharacterId` 가 `hero_warrior` 로 자동 변환
- `unlockedCharacterIds` 가 `["hero_warrior", "hero_archer"]` 로 자동 변환
- Attic 진입 시 시우(전사) 가 선택된 상태로 노출

**검증 방법**: 사용자 콘솔 + SaveData 파일 직접 확인

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0
- [ ] NUnit 9/9 신규 PASS (전체 117/117)
- [ ] ProjectValidator warnings 변동 ≤ 0
- [ ] 기존 시스템 회귀 X:
  - PlayerController 이동 정상
  - LitMotion punch/flash 정상
  - AtticCharacterSelector wiring 정상 (직렬화 _buttons[] array — D-A 사실 결정)
  - LimitedShopService.HandleCharacterFirstSelect 자동 마이그레이션 받음
- [ ] iOS 빌드 용량 영향 0 (Editor/References/Characters_PSB/ 자동 제외)

---

## 8. 합격 기준

### 최소 합격
- C1 컴파일 ✅
- C2 NUnit 9/9 PASS ✅
- C3 Validator errors=0 ✅
- C4 Reflection 모두 ✅
- C5 SO + Asset 모두 ✅
- C6 시나리오 1, 3 PASS (시각 ID + SaveData 마이그레이션 핵심)

### 완전 합격
- 모든 카테고리 ✅
- C6 시나리오 1, 2, 3 모두 PASS
- 비기능 요구사항 모두 ✅

---

## 9. Trade-off (mediator decisions 기반)

- **D-C=A** (별도 PR — 5020.1 후속) → character_progression_v1.md stale 1~2일 잔존. 본 5020 단일 책임 (자산 + SO + 코드) 유지.
- **D-D=B** (Selected_Characters_v1_legacy rename) → Sprint 3a +1줄 +5분. 명확 분리 ↑ vs Claude 권장 [A 보존] 의 단순함 손실.
- **D-B (rebuttal)** unlockedCharacterIds 매핑 추가 → SaveDataMigration ≤60L → ≤64L. 100줄 룰 여전히 준수. SaveData 호환성 ↑.
- **D5-B** (PSB ref 별도 출력) → ComfyUI 6 PNG (3 → 6). 시간 +60분. Phase B (5022) PSB rig 출발 자료 확보.

---

## 10. Evaluator 운영

Phase 6 Evaluator 는 본 Contract 만 단일 진실원 으로 사용:
- PRD / breakdown / decisions 직접 추출 X (scope creep 방지)
- Contract 의 Pass/Fail 기준만 평가
- FAIL 시 구체 수정 지시 (어느 카테고리 / 어느 항목 / 어느 파일)
- 반복 루프 max 2회 (FAIL → Phase 5 재진입)

---

## 11. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-contract Phase 4) | 초기 — 6 카테고리 (C1~C6), NUnit 9 신규, Play 시나리오 3, 합격 기준 2단 |
