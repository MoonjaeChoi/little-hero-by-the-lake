# Evaluator Checklist: 5020 Starter Hero Visual

> **Phase 6 Evaluator 운영용** — 본 체크리스트 만으로 PASS/FAIL 판정.
> Contract: `5020_starter_hero_visual_contract.md`

---

## 자동 검증 (스크립트 / MCP / Bash)

### C1 컴파일

- [ ] `mcp__UnityMCP__refresh_unity` 실행
- [ ] 5~10초 대기 후 `editor_state.isCompiling == false`
- [ ] `mcp__UnityMCP__read_console` errors=0
- [ ] 신규 4 .cs 파일 모두 컴파일 OK
  - [ ] `Core/SaveDataMigration.cs`
  - [ ] `Editor/StarterHeroAuditor.cs`
  - [ ] `Tests/Editor/CharacterDataIntegrityTests.cs`
  - [ ] `Tests/Editor/SaveDataMigrationTests.cs`
- [ ] 수정 2 .cs 파일 모두 컴파일 OK
  - [ ] `Core/SaveSystem.cs`
  - [ ] `Editor/ProjectValidator.cs`
- [ ] 라인 수 검증
  - [ ] SaveDataMigration ≤ 100L (목표 64L)
  - [ ] StarterHeroAuditor ≤ 100L (목표 80L)

### C2 Editor Test (NUnit 9 신규)

- [ ] Test Runner Edit Mode > Run All
- [ ] **117/117 PASS** (108 기존 + 9 신규)
- [ ] CharacterDataIntegrityTests 4/4 PASS
  - [ ] Warrior_HasStr2x_Id_HeroWarrior
  - [ ] Archer_HasAgi2x_Id_HeroArcher
  - [ ] Mage_HasWis2x_Id_HeroMage
  - [ ] AllStarterHeroes_HavePortraitWired
- [ ] SaveDataMigrationTests 5/5 PASS
  - [ ] MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior
  - [ ] MigrateCharacterId_BraveGirl_ReturnsHeroArcher
  - [ ] MigrateCharacterId_QuietFriend_ReturnsHeroMage
  - [ ] MigrateCharacterId_AlreadyNew_ReturnsSame
  - [ ] MigrateUnlockedCharacterIds_ArrayMapping_Works (D-B)

### C3 ProjectValidator

- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings 변동 ≤ 0 (시작 대비)
- [ ] 신규 위반 0건
- [ ] StarterHero_PortraitWired 룰 PASS (3 SO portrait != null)

### C4 Reflection 정적 검증

- [ ] `LittleHero.SaveDataMigration` Type 존재 (Assembly-CSharp)
  - [ ] `MigrateCharacterId(string) → string` static method
  - [ ] `Apply(SaveData) → SaveData` static method
  - [ ] `CharacterIdMap` static readonly Dictionary, count=3
- [ ] `LittleHero.EditorTools.StarterHeroAuditor` Type 존재 (Assembly-CSharp-Editor)
  - [ ] `Run()` static method with MenuItem attribute
- [ ] `LittleHero.Tests.CharacterDataIntegrityTests` (LittleHero.Tests.Editor)
  - [ ] 4 `[Test]` methods
- [ ] `LittleHero.Tests.SaveDataMigrationTests` (LittleHero.Tests.Editor)
  - [ ] 5 `[Test]` methods
- [ ] `SaveSystem.Load()` 본문 grep `SaveDataMigration.Apply` ≥ 1회 등장
  ```bash
  grep -c "SaveDataMigration.Apply" LittleHero/Assets/_Project/Scripts/Core/SaveSystem.cs
  # 1 이상이어야 함
  ```

### C5 SO + Asset 검증

#### CharacterData SO (3)
- [ ] `Char_HeroWarrior.asset` 존재 + id=hero_warrior + className=전사 + displayName="전사 시우" + str=2.0 + portrait!=null
- [ ] `Char_HeroArcher.asset` 존재 + id=hero_archer + className=궁수 + displayName="궁수 가영" + agi=2.0 + portrait!=null
- [ ] `Char_HeroMage.asset` 존재 + id=hero_mage + className=마법사 + displayName="마법사 연우" + wis=2.0 + portrait!=null
- [ ] 옛 SO 부재
  ```bash
  ls LittleHero/Assets/_Project/Data/Characters/Char_DefaultBoy.asset 2>&1 | grep -q "No such" && echo PASS
  ls LittleHero/Assets/_Project/Data/Characters/Char_BraveGirl.asset 2>&1 | grep -q "No such" && echo PASS
  ls LittleHero/Assets/_Project/Data/Characters/Char_QuietFriend.asset 2>&1 | grep -q "No such" && echo PASS
  ```

#### PNG 자산 (6)
- [ ] `Char_HeroWarrior.png` 768×1024 RGBA, alignment=Custom, pivot≈(0.5, 0.1)
- [ ] `Char_HeroArcher.png` 동
- [ ] `Char_HeroMage.png` 동
- [ ] `Editor/References/Characters_PSB/Char_HeroWarrior_psb_ref.png` 768×1024 RGBA (pivot 검증 X)
- [ ] `Editor/References/Characters_PSB/Char_HeroArcher_psb_ref.png` 동
- [ ] `Editor/References/Characters_PSB/Char_HeroMage_psb_ref.png` 동
- [ ] StarterHeroAuditor 실행 → 3/3 PASS

#### 폴더 정합 (D-D=B)
- [ ] `References/Selected_Characters/` 폴더 부재
  ```bash
  ls LittleHero/Assets/_Project/Art/References/Selected_Characters 2>&1 | grep -q "No such" && echo PASS
  ```
- [ ] `References/Selected_Characters_v1_legacy/` 폴더 존재
  ```bash
  ls LittleHero/Assets/_Project/Art/References/Selected_Characters_v1_legacy/ | wc -l  # ≥ 3
  ```
- [ ] 비-wired 옛 PNG (`Char_*_idle.png` 6 파일) 부재 (T01 매핑 결과)

---

## 수동 검증 (사용자 Play Mode 보고)

### C6.1 시나리오 1 — Attic 3 portrait + 한국어 이름 라벨

- [ ] Attic.unity Play 시작 OK
- [ ] AtticCharacterSelector 진입 — 3 캐릭터 버튼 표시
- [ ] portrait 3장 모두 V5 sticker + 직업 정체성 (검/활/지팡이) 명확
- [ ] displayName 라벨 "선택: 전사 시우" / "선택: 궁수 가영" / "선택: 마법사 연우"
- [ ] 폰트 잘림 없음 (한글 정상)
- [ ] 콘솔 에러 0건

### C6.2 시나리오 2 — 캐릭터 선택 → 챕터 1 진입

- [ ] 시우(전사) 선택 → "선택: 전사 시우" 라벨 갱신
- [ ] 챕터 1 진입 OK
- [ ] PlayerController 신규 portrait 사용 (시각 확인)
- [ ] 발 밑 pivot — 캐릭터 발이 ground 와 정확히 맞닿음
- [ ] 좌우 이동 → flipX 정상
- [ ] LitMotion 효과 회귀 X (punch/flash 동작)

### C6.3 시나리오 3 — 구 SaveData 마이그레이션 (D3-B 핵심)

- [ ] `Tools/LittleHero/Reset SaveData` 실행
- [ ] save.json 옛 형식 직접 편집:
  ```json
  {
    "selectedCharacterId": "default_boy",
    "unlockedCharacterIds": ["default_boy", "brave_girl"]
  }
  ```
- [ ] Game 재시작 → SaveSystem.Load() 호출
- [ ] 콘솔 NRE 없이 정상 로드
- [ ] save.json 다음 저장 시 자동 변환 확인:
  ```bash
  grep "selectedCharacterId" ~/Library/Application\ Support/Anthropic/LittleHero/save.json
  # "selectedCharacterId": "hero_warrior" 이어야 함
  grep "unlockedCharacterIds" ~/Library/Application\ Support/.../save.json
  # ["hero_warrior", "hero_archer"] 이어야 함
  ```
- [ ] Attic 진입 시 시우(전사) 자동 선택 상태

---

## 비기능 검증

- [ ] 기존 시스템 회귀 X:
  - [ ] PlayerController 이동 정상
  - [ ] LitMotion punch/flash 정상
  - [ ] AtticCharacterSelector wiring 정상 (D-A 사실)
  - [ ] LimitedShopService.HandleCharacterFirstSelect 자동 마이그레이션 받음
  - [ ] MetaProgression.SelectCharacter 정상
- [ ] iOS 빌드 용량 영향 0 (Editor/References/Characters_PSB/ 자동 제외)
- [ ] (선택) 60fps 유지 (Profiler 확인)

---

## 합격 판정

### 최소 합격 ✅
- 자동 검증 C1, C2, C3, C4, C5 모두 ✅
- 수동 시나리오 C6.1 (시각 정체성) + C6.3 (SaveData 마이그레이션) ✅

### 완전 합격 ✅✅
- 자동 검증 모두 ✅
- 수동 시나리오 C6.1, C6.2, C6.3 모두 ✅
- 비기능 검증 모두 ✅

### FAIL ❌
- 자동 검증 C1~C5 중 1개라도 실패 → **Phase 5 재진입** (max 2회)
- 수동 시나리오 C6.1 또는 C6.3 실패 → **Phase 5 재진입**
- C6.2 만 실패 → **부분 합격** (수정 권고, 종결 가능 — 사용자 결정)

---

## 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-contract Phase 4) | 초기 — 자동 5 카테고리 + 수동 3 시나리오 + 비기능 |
