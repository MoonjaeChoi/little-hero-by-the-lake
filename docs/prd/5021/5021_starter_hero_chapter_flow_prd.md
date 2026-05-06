# 5021 Sprint PRD — Starter Hero Chapter Flow

> 디자인: `docs/prd/5021_starter_hero_chapter_flow_prd.md`
> Sprint: 4 / commit: 5 / 시간: ~2h15m
> 의존성: 5020 (직접 cascade)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | CharacterPool SO 인프라 | 2 | 30분 |
| 2 | MetaProgression init 보강 + NUnit | 1 | 45분 |
| 3 | Attic.unity scene wiring (R3+R4) | 1 | 30분 |
| 4 | Play 검증 + changelog | 1 | 30분 |
| **합계** | | **5** | **2h15m** |

---

## 2. Sprint 1 — CharacterPool 인프라 (2 commit)

### 2.1 CharacterPool SO 클래스

**파일**: `LittleHero/Assets/_Project/Scripts/Data/CharacterPool.cs` (신규, ~15L)

```csharp
using UnityEngine;
namespace LittleHero
{
    [CreateAssetMenu(menuName = "Little Hero/Character Pool")]
    public class CharacterPool : ScriptableObject
    {
        public CharacterData[] characters;
        public CharacterData FindById(string id)
        {
            if (string.IsNullOrEmpty(id) || characters == null) return null;
            for (int i = 0; i < characters.Length; i++)
                if (characters[i] != null && characters[i].id == id) return characters[i];
            return null;
        }
    }
}
```

**검증**:
- 컴파일 OK
- 100줄 룰 ✅

### 2.2 CharacterPool 인스턴스 + ProjectValidator 룰

**파일**: `LittleHero/Assets/_Project/Data/Characters/CharacterPool_Default.asset` (신규)
- characters[]: 3 (Char_HeroWarrior / Char_HeroArcher / Char_HeroMage)

**파일**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs` (수정, +15L)
- `ValidateCharacterPool` 룰:
  - `CharacterPool` 인스턴스 1+ 존재
  - `characters` 길이 3, 모두 non-null, id 모두 hero_warrior/hero_archer/hero_mage 정합

**검증**:
- ProjectValidator 실행 errors=0
- StarterHero_PortraitWired (5020) 룰 회귀 X

### Sprint 1 commit
```
5021 Sprint 1 / T01 — CharacterPool SO 인프라 (1/5, 2/5)

[변경]
  + Data/CharacterPool.cs (15L SO 클래스 + FindById)
  + Data/Characters/CharacterPool_Default.asset (3 character wiring)
  ~ Editor/ProjectValidator.cs ValidateCharacterPool 룰 (+15L)
```

---

## 3. Sprint 2 — MetaProgression init 보강 (1 commit)

### 3.1 ResolveSelectedCharacterFromSave + Awake hook

**파일**: `LittleHero/Assets/_Project/Scripts/Meta/MetaProgression.cs` (수정, +15L)

```csharp
[SerializeField] private CharacterPool _characterPool;

private void Awake() {
    if (Instance != null) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
    Data = SaveSystem.Load();
    ResolveSelectedCharacterFromSave();   // ← 신규 hook
    StarlightGrantService.Bootstrap();
    ...
}

private void ResolveSelectedCharacterFromSave() {
    if (_characterPool == null) return;
    SelectedCharacter = _characterPool.FindById(Data?.selectedCharacterId);
}
```

### 3.2 NUnit 3 신규

**파일**: `LittleHero/Assets/_Project/Tests/Editor/MetaProgressionInitTests.cs` (신규, ~50L)
- `ResolveCharacter_EmptyId_RemainsNull`
- `ResolveCharacter_HeroWarrior_ResolvesToWarriorSO`
- `ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior` (5020 마이그레이션 + 5021 init 통합)

**검증**:
- NUnit 117 → 120 PASS
- C3 Validator errors=0

### Sprint 2 commit
```
5021 Sprint 2 / T02 — MetaProgression init 보강 + NUnit 3 (3/5)

[변경]
  ~ Meta/MetaProgression.cs (+15L) — _characterPool + ResolveSelectedCharacterFromSave + Awake hook
  + Tests/Editor/MetaProgressionInitTests.cs (50L NUnit 3)

[검증]
  - NUnit 117 → 120 PASS
  - 5020 마이그레이션 + 5021 init 통합 검증
```

---

## 4. Sprint 3 — Attic.unity scene wiring (1 commit)

### 4.1 R4 wiring — MetaProgression._characterPool

**파일**: `LittleHero/Assets/_Project/Scenes/Attic.unity` (수정)
- MetaProgression GameObject 의 `_characterPool` field → CharacterPool_Default GUID 매핑

### 4.2 R3 wiring — AtticChapterEntry._buttons[] (조사 + 결정 후 적용)

**사전 조사**:
```bash
grep -nE "m_Name: ChapterBtn|m_Name: Ch[0-9]" LittleHero/Assets/_Project/Scenes/Attic.unity
```

**A) ChapterButton GameObject 존재 시** — scene YAML 직접 wiring (5020.2 패턴):
```yaml
_buttons:
- chapter: {fileID: 11400000, guid: <Chapter01 GUID>, type: 2}
  button: {fileID: <ChapterBtn_ch01 Button fileID>}
- ...
```

**B) GameObject 미존재 시** — WondrousMapPanel 이 production path 인 것으로 결정, AtticChapterEntry는 dead code → 제거 또는 비활성화 (사용자 결정 대기).

### Sprint 3 commit
```
5021 Sprint 3 / T03 — Attic.unity wiring R3+R4 (4/5)

[변경]
  ~ Scenes/Attic.unity:
    - MetaProgression._characterPool → CharacterPool_Default GUID
    - AtticChapterEntry._buttons[] (옵션 A) 또는 Component 비활성화 (옵션 B)

[검증]
  - Unity 콘솔 errors=0
  - Play 시 MetaProgression.Awake → SelectedCharacter 자동 set
```

---

## 5. Sprint 4 — Play 검증 + changelog (1 commit)

### 5.1 Play 검증 시나리오 (사용자 보고)

**시나리오 1**: Attic 직접 진입
1. save.json: `selectedCharacterId="hero_warrior"` (5020 결과)
2. Attic Play ▶
3. Console: `[MetaProgression]` bootstrap 로그
4. **기대**: AtticCharacterSelector.Start 가 호출되기 전에 이미 SelectedCharacter set (Awake → Start 순서)
5. UI "선택: 전사 시우" 즉시 표시

**시나리오 2**: Chapter 직접 Play
1. save.json: hero_warrior 보존
2. Chapter.unity 직접 Open Scene → Play ▶
3. **기대**: PlayerController.Start 시 character != null → portrait 정상 노출 (둥근 원 → 시우 sprite)

**시나리오 3**: 5020 마이그레이션 + 5021 init 통합
1. save.json 옛 형식 sed 주입 (`default_boy`)
2. Attic Play → Stop
3. **기대**: save.json `hero_warrior` 변환 + 다음 Play 시 자동 resolve

### 5.2 changelog

**파일**: `docs/little_hero/changelog_v1.md` 최상단 + CLAUDE.md §11 마일스톤 + §14

### Sprint 4 commit
```
5021 Sprint 4 / T04 — Play 검증 + changelog (5/5) — 5021 종결

[변경]
  ~ docs/little_hero/changelog_v1.md (5021 항목)
  ~ CLAUDE.md §11 마일스톤 + §14 직전 5건

[검증]
  - 시나리오 1~3 PASS (사용자 보고)
  - 5020 C6 시나리오 2b 자동 PASS
```

---

## 6. 의존성 그래프

```
Sprint 1 (SO 인프라) → Sprint 2 (init 보강) → Sprint 3 (scene wiring) → Sprint 4 (검증)
```

병렬 가능 X (의존성 직렬). Sprint 1 의 SO 인스턴스 없으면 Sprint 2 wiring 의미 X.

---

## 7. 완료 정의 (Definition of Done)

- [ ] `Data/CharacterPool.cs` 컴파일 + ProjectValidator 인식
- [ ] `CharacterPool_Default.asset` 3 character wiring 검증
- [ ] `MetaProgression.cs` Awake 보강 + NUnit 3 PASS
- [ ] `Attic.unity` scene wiring 적용 + Unity errors=0
- [ ] Play 시나리오 1~3 사용자 보고 PASS
- [ ] changelog + CLAUDE.md 갱신
- [ ] `/prd-evaluate 5021` PASS

---

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Sprint 3 Attic.unity wiring 시 fileID 오타 → scene 손상 | `git diff` 검증 + Unity 콘솔 errors 확인 후 commit |
| MetaProgression Awake 가 CharacterPool null 받음 (Inspector wiring 누락) | NUnit 1 (null pool 처리 케이스) + ProjectValidator 룰 |
| CharacterData GUID 변경 (이론상 0, 실수 시) | 5020 GUID 매핑 표 (`docs/prd/5020/5020_png_guid_map.md`) 재활용 |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 Sprint PRD — 4 Sprint / 5 commit / 2h15m |
