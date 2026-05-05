# T04: Sprint 3b — SaveDataMigration 헬퍼 + SaveSystem hook

> **레이어**: Logic
> **의존성**: T03 (SO id 변경 완료)
> **예상 시간**: 35분 / 1 commit (T05 와 병렬 가능)
> **PRD 출처**: v2 §차분 4 (D-B unlockedCharacterIds 추가)

---

## 1. 작업 항목

### 1.1 신규 파일

**경로**: `LittleHero/Assets/_Project/Scripts/Core/SaveDataMigration.cs`

**클래스 명세** (CLAUDE.md §15.7 정적 헬퍼):

```csharp
using System.Collections.Generic;

namespace LittleHero
{
    /// <summary>
    /// 5020 D3-B — 옛 키즈 character id → 신 직업 정합 id 자동 매핑.
    /// SaveSystem.Load() 진입부에서 1회성 적용. SaveData 호환성 보존.
    /// </summary>
    public static class SaveDataMigration
    {
        // D3-B (5020): 옛 키즈 id → 신 직업 정합 id
        private static readonly Dictionary<string, string> CharacterIdMap = new()
        {
            { "default_boy",   "hero_warrior" },
            { "brave_girl",    "hero_archer" },
            { "quiet_friend",  "hero_mage" },
        };

        public static string MigrateCharacterId(string oldId)
        {
            if (string.IsNullOrEmpty(oldId)) return oldId;
            return CharacterIdMap.TryGetValue(oldId, out var newId) ? newId : oldId;
        }

        public static SaveData Apply(SaveData data)
        {
            if (data == null) return data;
            data.selectedCharacterId = MigrateCharacterId(data.selectedCharacterId);

            // D-B (rebuttal): unlockedCharacterIds[] 일괄 매핑
            if (data.unlockedCharacterIds != null)
                for (int i = 0; i < data.unlockedCharacterIds.Length; i++)
                    data.unlockedCharacterIds[i] = MigrateCharacterId(data.unlockedCharacterIds[i]);

            return data;
        }
    }
}
```

**예상 라인**: ~64L (CLAUDE.md §15.6 100줄 룰 ✅)

### 1.2 기존 파일 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Core/SaveSystem.cs`

**변경**: `Load()` 메서드 내 1줄 추가:

```csharp
public static SaveData Load()
{
    if (!File.Exists(SavePath))
        return new SaveData();
    string json = File.ReadAllText(SavePath);
    var data = JsonUtility.FromJson<SaveData>(json);
    return SaveDataMigration.Apply(data);  // ← 신규 1줄 (D3-B 자동 마이그레이션)
}
```

**변경 라인**: +1 (variable 분리는 +1 추가, 합산 +2 가능 — 4줄 이내)

---

## 2. 검증 기준

### 2.1 자동 검증 (refresh_unity + 5~10s + read_console)
- [ ] SaveDataMigration.cs 컴파일 OK
- [ ] SaveSystem.cs 컴파일 OK (수정 후)
- [ ] Unity 콘솔 errors=0
- [ ] §15.6: SaveDataMigration 라인 수 ≤ 100 (수동 확인)

### 2.2 회귀 (SaveSystem 단일 진입점)
- [ ] 신규 SaveData (`hero_warrior` 저장) 로드 → 변경 없음
- [ ] 빈 SaveData (selectedCharacterId = null) 로드 → NRE 없음
- [ ] 자동 검증은 T06 SaveDataMigrationTests 5 케이스에서 보장

---

## 3. commit 메시지

```
5020 Sprint 3b / T04 — SaveDataMigration D3-B + unlockedCharacterIds (4/7)

[변경]
  + Core/SaveDataMigration.cs (~64L 정적 헬퍼, §15.6 + §15.7 DI)
    - CharacterIdMap (default_boy → hero_warrior 등 3쌍)
    - MigrateCharacterId(string) — 단일 id 변환
    - Apply(SaveData) — selectedCharacterId + unlockedCharacterIds[] 일괄
  ~ Core/SaveSystem.cs Load() — SaveDataMigration.Apply hook 1줄

[검증]
  - 컴파일 errors=0
  - 100줄 룰 준수 (~64L)
  - SaveSystem 단일 진입부 hook (§15.1 SRP)
  - NUnit 5 케이스는 T06 (Sprint 4) 에서 검증

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- v2 §차분 4 (D-B unlockedCharacterIds 매핑)
- Rebuttal §3.4 (D-B 사실 검증 — SaveData 전수 조사)
- SaveSystem.cs:14~78 (SaveData 클래스 전체 필드)
- MetaProgression.cs:42 `Data = SaveSystem.Load()` (단일 진입점)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| SaveSystem.Load() 단일 진입점 외 다른 deserialize 코드 존재 | grep 로 다른 진입점 0건 확인 (rebuttal D-B 단계 검증 완료) |
| SaveData 미래 character ID 필드 추가 시 매핑 누락 | SaveDataMigration.Apply 에 추가 필드 매핑 추가 (간단) |
| Migration 후 동일 SaveData 재 Load 시 idempotent | TryGetValue → 이미 신 id 면 그대로 (idempotent ✅) |
