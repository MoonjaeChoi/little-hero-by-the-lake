# T02: Sprint 1.2 — CharacterPool 인스턴스 + ProjectValidator 룰

> **레이어**: Data + Test
> **의존성**: T01 (CharacterPool 클래스 컴파일 후 SO 인스턴스 생성 가능)
> **예상 시간**: 15분 / 1 commit
> **PRD 출처**: v2 §2.2

---

## 1. 작업 항목

### 1.1 신규 SO 인스턴스

**경로**: `LittleHero/Assets/_Project/Data/Characters/CharacterPool_Default.asset`

**생성 방법**:
- Unity Editor: Project 창 → 우클릭 → Create > Little Hero > Character Pool
- 또는 `mcp__UnityMCP__execute_code` 로 자동 생성:
  ```csharp
  var pool = ScriptableObject.CreateInstance<CharacterPool>();
  pool.characters = new[] {
      AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Characters/Char_HeroWarrior.asset"),
      AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Characters/Char_HeroArcher.asset"),
      AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/_Project/Data/Characters/Char_HeroMage.asset"),
  };
  AssetDatabase.CreateAsset(pool, "Assets/_Project/Data/Characters/CharacterPool_Default.asset");
  AssetDatabase.SaveAssets();
  ```

**검증**:
- 3 character (warrior/archer/mage) 모두 wiring
- GUID 보존 (5020 후 정합)

### 1.2 ProjectValidator 룰 추가

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs` (수정, +15L)

**신규 룰** `ValidateCharacterPool`:
```csharp
private static void ValidateCharacterPool(List<Issue> issues)
{
    string[] guids = AssetDatabase.FindAssets("t:LittleHero.CharacterPool");
    if (guids.Length == 0)
    {
        issues.Add(new Issue(Severity.Error, "5021 CharacterPool", "CharacterPool 인스턴스 0건 — 5021 init 동작 안함"));
        return;
    }
    foreach (var g in guids)
    {
        var path = AssetDatabase.GUIDToAssetPath(g);
        var pool = AssetDatabase.LoadAssetAtPath<CharacterPool>(path);
        if (pool == null) continue;
        if (pool.characters == null || pool.characters.Length != 3)
            issues.Add(new Issue(Severity.Error, "5021 CharacterPool", $"{path}: characters 길이={pool.characters?.Length} 기대 3"));
        else
        {
            var ids = pool.characters.Select(c => c?.id).ToHashSet();
            string[] expected = { "hero_warrior", "hero_archer", "hero_mage" };
            foreach (var id in expected)
                if (!ids.Contains(id)) issues.Add(new Issue(Severity.Error, "5021 CharacterPool", $"{path}: id '{id}' 누락"));
        }
    }
}
```

`Run()` 내 호출 추가:
```csharp
ValidateStarterHeroPortraits(issues);  // 5020 기존
ValidateCharacterPool(issues);         // 5021 T02 신규
```

---

## 2. 검증 기준

### 2.1 자동 검증
- `mcp__UnityMCP__refresh_unity` 후 5s
- `mcp__UnityMCP__read_console` errors=0
- `Tools/LittleHero/Validate Project` 실행 → errors=0

### 2.2 SO 정합
- CharacterPool_Default.asset 의 characters[3] 모두 non-null
- id 매핑: hero_warrior / hero_archer / hero_mage 모두 존재

### 2.3 회귀
- 5020 StarterHero_PortraitWired 룰 PASS 유지
- 다른 ProjectValidator 룰 영향 X

---

## 3. commit 메시지

```
5021 Sprint 1.2 / T02 — CharacterPool 인스턴스 + Validator 룰 (2/5)

[변경]
  + Data/Characters/CharacterPool_Default.asset (3 character wiring)
  ~ Editor/ProjectValidator.cs ValidateCharacterPool 룰 (+15L)
    - characters 길이 3 검증
    - id 매핑 (hero_warrior/hero_archer/hero_mage) 검증

[검증]
  - ProjectValidator errors=0
  - 5020 StarterHero_PortraitWired 룰 회귀 X
  - 3 character 모두 wiring 확인

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 §2.2
- T01 (CharacterPool 클래스 의존)
- 5020 ValidateStarterHeroPortraits (Validator 패턴 답습)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| CharacterData GUID 변경 (5020 후 정합) | 5020 매핑 표 (`docs/prd/5020/5020_png_guid_map.md`) 재활용 |
| Validator 룰 통합 시 기존 룰 회귀 | Run() 호출 위치만 추가, 기존 룰 변경 0 |
| LinQ `.ToHashSet()` Unity 6 호환성 | C# 8+ 정합, 컴파일 검증으로 확인 |
