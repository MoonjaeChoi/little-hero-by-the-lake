# T03: Sprint 2 — MetaProgression init 보강 + NUnit 3

> **레이어**: Logic + Test
> **의존성**: T01 (CharacterPool 클래스)
> **예상 시간**: 45분 / 1 commit (T02 와 병렬 가능)
> **PRD 출처**: v2 §3

---

## 1. 작업 항목

### 1.1 기존 파일 수정

**경로**: `LittleHero/Assets/_Project/Scripts/Meta/MetaProgression.cs`

**변경**:

```csharp
public class MetaProgression : MonoBehaviour
{
    public static MetaProgression Instance { get; private set; }
    public CharacterData SelectedCharacter { get; private set; }
    // ... 기존 필드

    [SerializeField] private CharacterPool _characterPool;  // ← 신규

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Data = SaveSystem.Load();
        ResolveSelectedCharacterFromSave();   // ← 신규 hook (Data 로드 직후)
        StarlightGrantService.Bootstrap();
        try { IAPService.Bootstrap(); } catch (System.Exception e) { Debug.LogWarning($"[MetaProgression] IAP bootstrap skip: {e.Message}"); }
        try { BootstrapItemDropService(); } catch (System.Exception e) { Debug.LogWarning($"[MetaProgression] ItemDrop bootstrap skip: {e.Message}"); }
    }

    // ← 신규 메서드
    private void ResolveSelectedCharacterFromSave()
    {
        if (_characterPool == null)
        {
            Debug.LogWarning("[MetaProgression] _characterPool 미wiring — Inspector 확인");
            return;
        }
        SelectedCharacter = _characterPool.FindById(Data?.selectedCharacterId);
    }

    // ... 기존 메서드 유지
}
```

**예상 변경**: +15L (필드 1 + 메서드 1 + Awake 1줄). 기존 331L → 346L (5023 cascade 분리 예정)

### 1.2 신규 NUnit Test

**경로**: `LittleHero/Assets/_Project/Tests/Editor/MetaProgressionInitTests.cs`

**3 NUnit 케이스**:

```csharp
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace LittleHero.Tests
{
    public class MetaProgressionInitTests
    {
        private const string POOL_PATH = "Assets/_Project/Data/Characters/CharacterPool_Default.asset";

        private CharacterPool LoadPool()
        {
            var pool = AssetDatabase.LoadAssetAtPath<CharacterPool>(POOL_PATH);
            Assert.IsNotNull(pool, $"{POOL_PATH} 로드 실패");
            return pool;
        }

        [Test]
        public void ResolveCharacter_EmptyId_RemainsNull()
        {
            var pool = LoadPool();
            Assert.IsNull(pool.FindById(""), "빈 id → null");
            Assert.IsNull(pool.FindById(null), "null id → null");
        }

        [Test]
        public void ResolveCharacter_HeroWarrior_ResolvesToWarriorSO()
        {
            var pool = LoadPool();
            var c = pool.FindById("hero_warrior");
            Assert.IsNotNull(c);
            Assert.AreEqual("hero_warrior", c.id);
            Assert.AreEqual("전사", c.className);
        }

        [Test]
        public void ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior()
        {
            // 5020 마이그레이션 + 5021 init 통합 검증
            var migrated = SaveDataMigration.MigrateCharacterId("default_boy");
            Assert.AreEqual("hero_warrior", migrated);
            var pool = LoadPool();
            var c = pool.FindById(migrated);
            Assert.IsNotNull(c);
            Assert.AreEqual("hero_warrior", c.id);
        }
    }
}
```

**예상 라인**: ~50L

---

## 2. 검증 기준

### 2.1 자동 검증 (refresh_unity + 5~10s + read_console + run_tests)
- MetaProgression.cs 컴파일 OK
- MetaProgressionInitTests.cs 컴파일 OK
- Unity 콘솔 errors=0
- §15.6: MetaProgression +15L = 346L (5023 cascade 일시 허용)

### 2.2 NUnit 실행
- MetaProgressionInitTests 3/3 PASS
- **합산 NUnit 117 → 120 (+3)**

### 2.3 회귀
- 기존 SaveSystem.Load + SaveDataMigration.Apply (5020) 동작 정상
- IAP / ItemDropService bootstrap 영향 0
- AtticCharacterSelector.Start 의 ResolveCharacterById fallback 정상

---

## 3. commit 메시지

```
5021 Sprint 2 / T03 — MetaProgression init 보강 + NUnit 3 (3/5)

[변경]
  ~ Meta/MetaProgression.cs (+15L) — 5023 cascade 분리 예정 일시 허용
    - [SerializeField] private CharacterPool _characterPool
    - ResolveSelectedCharacterFromSave() 메서드 신규
    - Awake() Data 로드 직후 hook 호출
  + Tests/Editor/MetaProgressionInitTests.cs (~50L NUnit 3)
    - ResolveCharacter_EmptyId_RemainsNull
    - ResolveCharacter_HeroWarrior_ResolvesToWarriorSO
    - ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior (5020 통합)

[검증]
  - 컴파일 errors=0
  - NUnit 117 → 120 PASS
  - 5020 마이그레이션 + 5021 init 통합 검증
  - 5023 cascade 정합 명시 (MetaProgression 분리 예정)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 §3
- T01 (CharacterPool 클래스 의존)
- 5020 SaveDataMigration (T03 의 ResolveCharacter_AfterMigration NUnit 통합)
- 5023 MetaProgression split (cascade 분리 예정)

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| _characterPool null (Inspector wiring 누락) | LogWarning + early return — NRE 0, T04 scene wiring 에서 보강 |
| Awake 순서 (MetaProgression 보다 다른 component 가 SelectedCharacter 참조) | DontDestroyOnLoad + Singleton 패턴 — 다른 component Start 보다 Awake 먼저 |
| SaveDataMigration.Apply 와 race condition | SaveSystem.Load 가 Apply 호출 후 반환 — 동기 보장 |
| 5023 분리 시 Awake hook 손실 가능성 | 5023 PRD §4.1 Loader 가 흡수 명시 — cascade 정합 |
