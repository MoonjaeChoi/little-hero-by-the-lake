# 5021 — Starter Hero Chapter Flow

> **목적**: 5020 evaluation 에서 발견된 5005 attic_decomposition_v1 회귀 (R3 + R4) 해소.
> **5020 직접 cascade**: C6 시나리오 2b "Chapter 진입 후 portrait 노출" 미해결의 근본 원인 fix.
> **scope**: 데이터 path 정합 (Attic→Chapter wiring + MetaProgression init).
> **out of scope**: 신규 게임플레이 / 신규 SO / UI 디자인 변경.

---

## 1. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5020 Phase 6 evaluation 에서 5005 R3/R4 회귀 발견, 5022 backlog 등재 후 5021 으로 승격 |

---

## 2. 현재 시스템 분석

### 2.1 R3 — AtticChapterEntry._buttons[] 빈 배열

**증거**:
```
$ grep -B 1 -A 6 "AtticChapterEntry$" LittleHero/Assets/_Project/Scenes/Attic.unity
  m_EditorClassIdentifier: Assembly-CSharp::LittleHero.AtticChapterEntry
  _buttons: []                    ← 빈 배열
  _chapterSceneName: Chapter
```

`AtticChapterEntry.cs:20-44`:
```csharp
[SerializeField] private ChapterButton[] _buttons;

private void OnEnable() {
    if (_buttons != null) {
        for (int i = 0; i < _buttons.Length; i++) { ... onClick.AddListener ... }
    }
}
```

`_buttons.Length == 0` → loop 0회 실행 → click listener 등록 0건 → 사용자 click 시 동작 0.

### 2.2 R4 — MetaProgression.Awake init 책임 한계

`MetaProgression.cs:37-51`:
```csharp
private void Awake() {
    Instance = this;
    DontDestroyOnLoad(gameObject);
    Data = SaveSystem.Load();              // SaveData 로드 + SaveDataMigration.Apply (5020 T04)
    StarlightGrantService.Bootstrap();
    try { IAPService.Bootstrap(); } catch { }
    try { BootstrapItemDropService(); } catch { }
}
// ※ SelectedCharacter 미resolve — null 잔존
```

**책임 분담** (5005 attic_decomposition_v1):
- MetaProgression: SaveData 로드 + Bootstrap
- AtticCharacterSelector.Start: `selectedCharacterId` → `SelectedCharacter` resolve

→ Attic 우회 진입 (Chapter 직접 Play, 또는 신규 path) 시 `SelectedCharacter` null → PlayerController 가 portrait 미적용 → "노란 둥근 원" 잔존.

### 2.3 영향 범위

- **5020 C6 시나리오 2b**: PASS 불가 (사용자 보고 ✅ 검증됨)
- **5022 (oop_validator_split) 의 후속 효과**: 책임 분담 일관성
- **출시 후 사용자 경험**: 매번 Attic 거쳐야 함 — UX 제약

---

## 3. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | R3 만 fix (Attic→Chapter wiring 만 복원) | 작은 commit, 5020 시나리오 2b 정상 path 통과 | Chapter 직접 Play 시 여전히 미적용 (개발/디버그 불편) |
| B (★권장) | R3 + R4 둘 다 fix | 정상 path + Editor 디버그 둘 다 동작 | commit +1 (~30분 추가) |
| C | R3+R4+ WondrousMapPanel 통합 검증 | production path 명확 | scope 확장 — 5021 약식 cycle 깨짐, 5022 분리 권장 |

**권장 옵션 B** — 5020 cascade 의 의미가 "정상 + 디버그 둘 다 동작" 이라는 사용자 기대 정합.

---

## 4. 권장 설계 (CLAUDE.md §15 정합)

### 4.1 R3 fix — AtticChapterEntry._buttons[] wiring

**조사 선행**: Attic.unity 의 ChapterButton GameObject (예 `ChapterBtn_ch01`) 존재 여부 확인:
- 존재 → scene YAML 직접 wiring (5020.2 패턴 답습)
- 미존재 → 사용자 결정 (button 신규 생성 vs WondrousMapPanel production path 채택)

**예상 산출** (button 존재 시):
```yaml
_buttons:
- chapter: {fileID: 11400000, guid: <Chapter01.asset GUID>, type: 2}
  button: {fileID: <ChapterBtn_ch01 Button fileID>}
- ...
```

**CLAUDE.md §15 정합**:
- §15.1 SRP 유지 (AtticChapterEntry 책임 변경 0)
- §15.4 SO 우선 (ChapterData 35종 기존 SO 활용)

### 4.2 R4 fix — MetaProgression init 책임 보강

**옵션 1 (권장)** — Awake 보강:
```csharp
private void Awake()
{
    ...
    Data = SaveSystem.Load();
    ResolveSelectedCharacterFromSave();  // 신규 5L
    StarlightGrantService.Bootstrap();
    ...
}

private void ResolveSelectedCharacterFromSave()
{
    if (string.IsNullOrEmpty(Data?.selectedCharacterId)) return;
    var allChars = AssetDatabase.FindAssets("t:CharacterData") /* Editor */
                   또는 Resources.LoadAll<CharacterData>("Characters") /* Build */;
    SelectedCharacter = allChars.FirstOrDefault(c => c.id == Data.selectedCharacterId);
}
```

**문제**: AssetDatabase 는 Editor 전용. Build 에서는 Resources 폴더 또는 ScriptableObject 인덱스 SO 필요.

**옵션 2 (Build 호환)** — CharacterPool SO 인덱스 신규:
```csharp
// Data/CharacterPool.asset (신규)
[CreateAssetMenu]
public class CharacterPool : ScriptableObject
{
    public CharacterData[] characters;  // 3 (warrior/archer/mage)
}

// MetaProgression
[SerializeField] private CharacterPool _characterPool;

private void ResolveSelectedCharacterFromSave()
{
    if (_characterPool == null || string.IsNullOrEmpty(Data?.selectedCharacterId)) return;
    foreach (var c in _characterPool.characters)
        if (c.id == Data.selectedCharacterId) { SelectedCharacter = c; return; }
}
```

**옵션 2 추천** — Build 호환 + §15.4 (SO 우선).

### 4.3 신규 / 수정 파일

| 파일 | 변동 | 라인 |
|---|---|---|
| `Data/CharacterPool.cs` (신규) | `CreateAssetMenu` SO + `CharacterData[]` | ~15L |
| `Data/Characters/CharacterPool_Default.asset` (신규) | 3 character wiring | YAML |
| `Meta/MetaProgression.cs` (수정) | `_characterPool` 필드 + `ResolveSelectedCharacterFromSave` | +15L |
| `Scenes/Attic.unity` (수정) | `MetaProgression._characterPool` wiring + `AtticChapterEntry._buttons[]` wiring | YAML |
| `Tests/Editor/MetaProgressionInitTests.cs` (신규) | NUnit 3 (selectedCharacterId 빈/존재/마이그레이션 후) | ~50L |
| `Editor/ProjectValidator.cs` (수정) | `ValidateCharacterPool` 룰 | +15L |

---

## 5. 마이그레이션 단계 (Sprint 분해)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | CharacterPool SO 클래스 + 인스턴스 + ProjectValidator 룰 | 2 | 30분 |
| 2 | MetaProgression.ResolveSelectedCharacterFromSave + Awake hook + NUnit 3 | 1 | 45분 |
| 3 | Attic.unity scene wiring (R3 + R4) | 1 | 30분 |
| 4 | Play 검증 (시나리오 1~3) + changelog | 1 | 30분 |
| **합계** | | **5** | **2시간 15분** |

---

## 6. 알려진 위험

| 위험 | 완화 |
|---|---|
| AtticChapterEntry 의 ChapterButton GameObject 가 scene 에 미존재 | 사전 조사 후 사용자 결정 (옵션 C 로 escalate 가능) |
| CharacterPool SO 가 다른 시스템 (LimitedShopService 등) 과 conflict | grep 으로 기존 character lookup path 0 confirm |
| MetaProgression Singleton race (Awake vs AtticCharacterSelector.Start) | Awake 단계에서 먼저 resolve → 나중 Start 가 동일 결과 보장 (idempotent) |
| Build 모드 Resources.LoadAll 또는 SerializeField 셋업 미완 | CharacterPool SerializeField 필수 — Inspector wiring 검증 NUnit |

---

## 7. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초기 — 5020 evaluation 의 5005 R3/R4 backlog → 5021 승격 |
