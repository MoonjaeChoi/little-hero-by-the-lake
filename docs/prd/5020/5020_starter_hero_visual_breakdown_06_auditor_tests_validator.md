# T06: Sprint 4 — StarterHeroAuditor + NUnit 9 + Validator 룰

> **레이어**: Test
> **의존성**: T04 (SaveDataMigration) + T05 (sprite pivot + displayName)
> **예상 시간**: 40분 / 1 commit
> **PRD 출처**: v2 §차분 5/6 (NUnit 117) + Sprint PRD §7

---

## 1. 작업 항목

### 1.1 StarterHeroAuditor (Editor 도구)

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/StarterHeroAuditor.cs` (신규)

**명세** (5008 EnemySpriteAuditor 답습):

```csharp
using UnityEngine;
using UnityEditor;

namespace LittleHero.EditorTools
{
    /// <summary>
    /// 5020 — 시작 주인공 3종 sprite + SO 정합성 일괄 점검.
    /// </summary>
    public static class StarterHeroAuditor
    {
        private static readonly string[] HeroPaths = new[]
        {
            "Assets/_Project/Data/Characters/Char_HeroWarrior.asset",
            "Assets/_Project/Data/Characters/Char_HeroArcher.asset",
            "Assets/_Project/Data/Characters/Char_HeroMage.asset",
        };

        [MenuItem("Tools/LittleHero/Audit Starter Hero Sprite Quality")]
        public static void Run()
        {
            int pass = 0, fail = 0;
            foreach (var path in HeroPaths)
            {
                if (AuditOne(path)) pass++; else fail++;
            }
            EditorUtility.DisplayDialog("Starter Hero Audit",
                $"PASS: {pass}/3\nFAIL: {fail}/3", "OK");
        }

        private static bool AuditOne(string assetPath)
        {
            // 검사 항목:
            // 1. SO 로드 OK
            // 2. portrait != null
            // 3. PNG 해상도 768×1024
            // 4. transparent ratio 5~50%
            // 5. alignment Custom + pivot ≈ (0.5, 0.1)
            // 6. id 정합 (hero_warrior / hero_archer / hero_mage)
            // 7. className 정합 (전사/궁수/마법사)
            // 결과 Console + return bool
            ...
        }
    }
}
```

**예상 라인**: ~80L (CLAUDE.md §15.6 ✅)

### 1.2 CharacterDataIntegrityTests

**경로**: `LittleHero/Assets/_Project/Tests/Editor/CharacterDataIntegrityTests.cs` (신규)

**4 NUnit 케이스**:
```csharp
[Test] public void Warrior_HasStr2x_Id_HeroWarrior() {...}
[Test] public void Archer_HasAgi2x_Id_HeroArcher() {...}
[Test] public void Mage_HasWis2x_Id_HeroMage() {...}
[Test] public void AllStarterHeroes_HavePortraitWired() {...}
```

각 케이스 ~10 라인 → ~50L 총

### 1.3 SaveDataMigrationTests

**경로**: `LittleHero/Assets/_Project/Tests/Editor/SaveDataMigrationTests.cs` (신규)

**5 NUnit 케이스** (rebuttal v2 차분 5):
```csharp
[Test] public void MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior() {...}
[Test] public void MigrateCharacterId_BraveGirl_ReturnsHeroArcher() {...}
[Test] public void MigrateCharacterId_QuietFriend_ReturnsHeroMage() {...}
[Test] public void MigrateCharacterId_AlreadyNew_ReturnsSame() {...}
[Test] public void MigrateUnlockedCharacterIds_ArrayMapping_Works() {...}  // D-B
```

각 케이스 ~10~12 라인 → ~60L 총

### 1.4 ProjectValidator 룰 추가

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs` (수정)

**신규 룰** `StarterHero_PortraitWired`:
- 3 SO 각각 portrait 필드 null 검사
- 위반 시 Validate Project 메뉴에서 Error 보고

```csharp
// ProjectValidator.cs 안에 추가
private static void ValidateStarterHeroPortraits(List<string> errors)
{
    string[] paths = { ".../Char_HeroWarrior.asset", ".../Char_HeroArcher.asset", ".../Char_HeroMage.asset" };
    foreach (var path in paths)
    {
        var data = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
        if (data == null) errors.Add($"5020: {path} 로드 실패");
        else if (data.portrait == null) errors.Add($"5020: {path} portrait 미wiring");
    }
}
```

**라인 수**: +20L (전체 ProjectValidator 변동 미미)

---

## 2. 검증 기준

### 2.1 자동 (refresh_unity + 10s + read_console + run_tests)
- [ ] StarterHeroAuditor.cs 컴파일 OK
- [ ] CharacterDataIntegrityTests.cs 컴파일 OK
- [ ] SaveDataMigrationTests.cs 컴파일 OK
- [ ] ProjectValidator.cs 컴파일 OK
- [ ] Unity 콘솔 errors=0

### 2.2 NUnit 실행 (Test Runner / 배치)
- [ ] CharacterDataIntegrityTests 4/4 PASS
- [ ] SaveDataMigrationTests 5/5 PASS
- [ ] **합산 NUnit 108 → 117 (+9)**

### 2.3 Auditor 실행
- [ ] `Tools/LittleHero/Audit Starter Hero Sprite Quality` 메뉴 클릭
- [ ] 3/3 PASS (PNG 해상도 + transparent ratio + pivot + portrait wiring + id/className 정합)

### 2.4 Validator 실행
- [ ] `Tools/LittleHero/Validate Project` 클릭
- [ ] StarterHero_PortraitWired 룰 PASS (errors 0)

---

## 3. commit 메시지

```
5020 Sprint 4 / T06 — Auditor + NUnit 9 + Validator 룰 (6/7)

[변경]
  + Editor/StarterHeroAuditor.cs (~80L, 5008 답습)
    - Tools/LittleHero/Audit Starter Hero Sprite Quality 메뉴
    - 3 SO × 7 항목 검사 (해상도/transparent/pivot/wiring/id/className)
  + Tests/Editor/CharacterDataIntegrityTests.cs (~50L, NUnit 4)
  + Tests/Editor/SaveDataMigrationTests.cs (~60L, NUnit 5 — D-B 매핑 포함)
  ~ Editor/ProjectValidator.cs (StarterHero_PortraitWired 룰 +20L)

[검증]
  - 컴파일 errors=0
  - StarterHeroAuditor 3/3 PASS
  - NUnit 108 → 117 PASS (+9)
  - ProjectValidator StarterHero_PortraitWired 룰 PASS
  - §15.6 100줄 룰 준수 (Auditor ≤80L, Tests ≤60L)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- v2 §차분 5 (NUnit 정정) + §차분 6 (NUnit 합산 117)
- Rebuttal §3.4 (D-B unlockedCharacterIds 매핑)
- 5008 EnemySpriteAuditor (Tools/LittleHero/Audit Enemy Sprite Quality) — 답습 패턴
- ProjectValidator.cs 기존 룰 패턴

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| StarterHeroAuditor 100줄 초과 (5008 EnemySpriteAuditor 도 ~120L 였던 사례) | 검사 로직 7 항목 — 단순 함수 호출 분리, 80L 이내 가능 |
| NUnit Test 어셈블리 컴파일 실패 (using/네임스페이스) | LittleHero.Tests.Editor 어셈블리 정합 — refresh + 10s + 검증 |
| ProjectValidator 룰 추가 후 기존 룰 회귀 | 기존 룰 호출 위치 영향 X (메서드 추가만) |
| Auditor PNG 해상도 검사 시 PSB ref PNG 까지 검사 | HeroPaths 만 명시 (Editor/References/ 제외) |
