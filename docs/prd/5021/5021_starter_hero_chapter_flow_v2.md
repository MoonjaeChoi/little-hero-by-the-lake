# 5021 Sprint PRD v2 — Starter Hero Chapter Flow

> **PRD v2 (최종, Mediator 확정)**
> 원본: `5021_starter_hero_chapter_flow_prd.md` · Rebuttal: `5021_starter_hero_chapter_flow_rebuttal.md` · Decisions: `5021_starter_hero_chapter_flow_decisions.md`
> 결정 2건 반영 (D1=B, D2=C). 다음 단계: `/prd-breakdown` (Phase 3)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | CharacterPool SO 인프라 (commit 1: 클래스 / commit 2: 인스턴스 + Validator) | 2 | 30분 |
| 2 | MetaProgression init 보강 + NUnit | 1 | 45분 |
| 3 | AtticChapterEntry 비활성화 + WondrousMapPanel wiring 검증 (D1=B) | 1 | 60분 |
| 4 | Play 검증 + changelog | 1 | 30분 |
| **합계** | | **5** | **2h45m** |

---

## 2. Sprint 1 — CharacterPool 인프라 (2 commit)

### 2.1 (commit 1) CharacterPool SO 클래스

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

#### Sprint 1 commit 1
```
5021 Sprint 1.1 / T01a — CharacterPool SO 클래스 (1/5)

[변경]
  + Data/CharacterPool.cs (15L SO 클래스 + FindById)

[검증]
  - 컴파일 errors=0
```

### 2.2 (commit 2) CharacterPool 인스턴스 + ProjectValidator 룰

**파일**: `LittleHero/Assets/_Project/Data/Characters/CharacterPool_Default.asset` (신규)
- characters[]: 3 (Char_HeroWarrior / Char_HeroArcher / Char_HeroMage GUID 매핑)

**파일**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs` (수정, +15L)
- `ValidateCharacterPool` 룰:
  - `CharacterPool` 인스턴스 1+ 존재
  - `characters` 길이 3, 모두 non-null, id 모두 hero_warrior/hero_archer/hero_mage 정합

**검증**:
- ProjectValidator 실행 errors=0
- StarterHero_PortraitWired (5020) 룰 회귀 X

#### Sprint 1 commit 2
```
5021 Sprint 1.2 / T01b — CharacterPool 인스턴스 + Validator 룰 (2/5)

[변경]
  + Data/Characters/CharacterPool_Default.asset (3 character wiring)
  ~ Editor/ProjectValidator.cs ValidateCharacterPool 룰 (+15L)

[검증]
  - ProjectValidator errors=0
  - 5020 StarterHero_PortraitWired 룰 회귀 X
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
- `ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior`

**검증**:
- NUnit 117 → 120 PASS
- C3 Validator errors=0

#### Sprint 2 commit
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

## 4. Sprint 3 — AtticChapterEntry 비활성화 + WondrousMapPanel 검증 (D1=B, 1 commit)

### 4.1 R4 wiring — MetaProgression._characterPool

**파일**: `Scenes/Attic.unity` (수정)
- MetaProgression GameObject 의 `_characterPool` field → CharacterPool_Default GUID 매핑

### 4.2 R3 fix — AtticChapterEntry 비활성화 (D1=B)

**조사 사전**:
- Attic.unity grep 결과 `WondrousMapNode` 10+ instance 확인됨 → 35 모두 존재 추정
- AtticChapterEntry._buttons[] 빈 배열 (5020.2 R2 와 동일 패턴) = dead code

**작업 (옵션 B)**:
1. **AtticChapterEntry GameObject 비활성화 또는 Component 제거**
   - scene YAML 에서 m_IsActive: 0 또는 component 항목 제거
   - 또는 `[SerializeField] private bool _legacyMode` 추가 후 default false (동작 안함)
2. **WondrousMapPanel wiring 검증**:
   - `_chapters[]` (35 ChapterData 참조) Inspector 또는 grep 검사
   - `_nodes[]` (35 WondrousMapNode 참조) Inspector 또는 grep 검사
3. **누락 시 보강**:
   - ChapterData SO 인덱스 35종 wiring (이미 wiring 됐을 가능성 높음)
   - WondrousMapNode 35 GameObject wiring

**검증**:
- Attic Play → WondrousMapPanel 진입 → 35 노드 표시 + 챕터 1 클릭 시 SceneTransitionService.Transition("Chapter") 정상

### 4.3 SO 필드 갱신 (T03 잔여)

(없음 — D2=C 분리로 commit 메시지 2개 모두 Sprint 1 에서 처리)

#### Sprint 3 commit
```
5021 Sprint 3 / T03 — AtticChapterEntry 비활성화 + WondrousMapPanel 검증 (4/5)

[변경 — D1=B mediator 결정]
  ~ Scenes/Attic.unity:
    - MetaProgression._characterPool → CharacterPool_Default GUID
    - AtticChapterEntry GameObject 비활성화 (m_IsActive: 0) — dead code 정리
    - WondrousMapPanel _chapters[] / _nodes[] wiring 상태 검증 (필요 시 보강)

[검증]
  - Unity 콘솔 errors=0
  - Play 시 MetaProgression.Awake → SelectedCharacter 자동 set
  - WondrousMapPanel 35 노드 정상 (사용자 시각 review)
```

---

## 5. Sprint 4 — Play 검증 + changelog (1 commit)

### 5.1 Play 검증 시나리오 (사용자 보고)

**시나리오 1**: Attic 직접 진입
1. save.json: `selectedCharacterId="hero_warrior"` (5020 결과)
2. Attic Play ▶
3. **기대**: AtticCharacterSelector.Start 호출 전 이미 SelectedCharacter set (Awake → Start 순서)
4. UI "선택: 전사 시우" 즉시 표시

**시나리오 2**: Chapter 직접 Play
1. save.json: hero_warrior 보존
2. Chapter.unity 직접 Open Scene → Play ▶
3. **기대**: PlayerController.Start 시 character != null → portrait 정상 노출 (둥근 원 → 시우 sprite)

**시나리오 3**: WondrousMapPanel 챕터 진입 (D1=B 검증)
1. Attic Play → WondrousMapPanel 35 노드 표시
2. 챕터 1 클릭 → SceneTransitionService.Transition("Chapter")
3. **기대**: Chapter 씬 로드 + portrait 정상 노출

**시나리오 4**: 5020 마이그레이션 + 5021 init 통합
1. save.json 옛 형식 sed 주입 (`default_boy`)
2. Attic Play → Stop
3. **기대**: save.json `hero_warrior` 변환 + 다음 Play 시 자동 resolve

### 5.2 changelog

**파일**: `docs/little_hero/changelog_v1.md` 최상단 + CLAUDE.md §11 마일스톤 + §14

#### Sprint 4 commit
```
5021 Sprint 4 / T04 — Play 검증 + changelog (5/5) — 5021 종결

[변경]
  ~ docs/little_hero/changelog_v1.md (5021 항목)
  ~ CLAUDE.md §11 마일스톤 + §14 직전 5건

[검증]
  - 시나리오 1~4 PASS (사용자 보고)
  - 5020 C6 시나리오 2b 자동 PASS (D1=B AtticChapterEntry 정리 후 WondrousMapPanel 단일 path)
```

---

## 6. 의존성 그래프

```
Sprint 1.1 (SO 클래스) → Sprint 1.2 (인스턴스+Validator) → Sprint 2 (init) → Sprint 3 (wiring+D1=B) → Sprint 4 (검증)
```

병렬 가능 X (의존성 직렬). Sprint 1.1 SO 클래스 컴파일 후 Sprint 1.2 인스턴스 생성 가능.

---

## 7. 완료 정의 (Definition of Done)

- [ ] `Data/CharacterPool.cs` 컴파일 + ProjectValidator 인식
- [ ] `CharacterPool_Default.asset` 3 character wiring 검증
- [ ] `MetaProgression.cs` Awake 보강 + NUnit 3 PASS
- [ ] `Attic.unity` MetaProgression._characterPool wiring 적용
- [ ] **AtticChapterEntry 비활성화** + WondrousMapPanel 35 노드 wiring 검증 (D1=B)
- [ ] Play 시나리오 1~4 사용자 보고 PASS
- [ ] changelog + CLAUDE.md 갱신
- [ ] `/prd-evaluate 5021` PASS

---

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Sprint 3 Attic.unity wiring 시 fileID 오타 → scene 손상 | `git diff` 검증 + Unity 콘솔 errors 확인 후 commit |
| MetaProgression Awake 가 CharacterPool null 받음 (Inspector wiring 누락) | NUnit 1 (null pool 처리 케이스) + ProjectValidator 룰 |
| **WondrousMapPanel 35 노드 wiring 누락 발견** (D1=B 비용) | Sprint 3 시간 60분 내 수동 보강 가능. 만약 35 모두 누락 시 +30~60분 추가 (5021.1 별도 fix) |
| AtticChapterEntry 비활성화 후 다른 코드가 참조 | grep "AtticChapterEntry" 으로 사용처 0 confirm 후 commit |

---

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | v2 초안 (Rebuttal 단계). 자동 수용 0건. D1/D2 mediator 대기 |
| 2026-05-06 | **v2 최종 (Mediator)** — D1=B + D2=C 반영. Sprint 3 60분 (D1 cascade), Sprint 1 commit 2개 명시 (D2). 합계 2h45m / 5 commit |
