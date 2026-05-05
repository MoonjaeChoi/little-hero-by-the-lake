# 5020 — Starter Hero Visual (Sprint PRD)

> **문서번호**: 5020
> **시스템**: 시작 주인공 3종 V5 sticker 직업 정체성 재생성 (시우 / 가영 / 연우)
> **작성일**: 2026-05-06 (사용자 결정 반영판)
> **선행 디자인 문서**: `docs/prd/5020_starter_hero_visual_prd.md`
> **확정 옵션**: D1=시우/가영/연우 + D2-A idle 1장 + **D3-B id+파일명 정합** + D4-A 코드 분리 + **D5-B PSB reference 별도**
> **CLAUDE.md 정합**: §15 (SO 우선, 신규 코드 ≤ 60L, NUnit 7 신규)

---

## 1. 전체 개요

| Sprint | 작업 | commit | 예상 시간 | 의존성 |
|---|---|---|---|---|
| 1 | Prompt 표 (idle 3 + psb_ref 3, 총 6 prompt) | 1 | 45분 | — |
| 2 | ComfyUI 배치 — 6 PNG 생성 + 사용자 review | 1 | 90~150분 | Sprint 1 |
| **3a** | **SO/PNG 파일 rename** (D3-B `git mv`) | 1 | 20분 | Sprint 2 |
| **3b** | **SaveDataMigration 헬퍼** (D3-B id 매핑) | 1 | 30분 | Sprint 3a |
| 3c | Sprite pivot + displayName + 직업명 갱신 | 1 | 20분 | Sprint 3a (3b 와 병렬 가능) |
| 4 | StarterHeroAuditor + CharacterData/SaveDataMigration NUnit | 1 | 40분 | Sprint 3c |
| 5 | Play 검증 + changelog | 1 | 20분 | Sprint 4 |
| **합계** | | **7** | **4~5시간** (서버 시간 제외) | |

---

## 2. Sprint 1 — Prompt 표 (1 commit)

### 2.1 작업 — 6 prompt 표 작성

**파일**: `docs/prd/5020/5020_prompt_table.md` (신규)

**산출물**:
- 6 row × N col 표:

  | hero | 종류 | seed | BASE | 정체성 prompt | NEGATIVE | 출력 경로 |
  |---|---|---|---|---|---|---|
  | hero_warrior (시우) | idle | 50200 | 5008 BASE | 디자인 §4.1.1 idle | 디자인 §4.3 | Generated_v2/Characters/Char_HeroWarrior.png |
  | hero_warrior (시우) | psb_ref | 50210 | 5008 BASE | 디자인 §4.1.1 psb_ref | 디자인 §4.3 | References/Characters_PSB/Char_HeroWarrior_psb_ref.png |
  | hero_archer (가영) | idle | 50201 | … | … | … | Generated_v2/Characters/Char_HeroArcher.png |
  | hero_archer (가영) | psb_ref | 50211 | … | … | … | References/Characters_PSB/Char_HeroArcher_psb_ref.png |
  | hero_mage (연우) | idle | 50202 | … | … | … | Generated_v2/Characters/Char_HeroMage.png |
  | hero_mage (연우) | psb_ref | 50212 | … | … | … | References/Characters_PSB/Char_HeroMage_psb_ref.png |

- BASE prompt + NEGATIVE 공통 블록 (디자인 §4.2 / §4.3 답습)
- workflow: `.claude/skills/comfyui-image/assets/workflow_layerdiffuse.json`
- 해상도: **768 × 1024** (5008 baseline 정합)

### 2.2 검증 기준

- [ ] 6 prompt 표 작성 완료, 직업·종류·seed·경로 모두 명확
- [ ] idle prompt 3종: "three-quarter front view, both arms visible" 공통
- [ ] psb_ref prompt 3종: "T-pose, body parts clearly separated, plain transparent background" 공통
- [ ] 시드 namespace 분리: idle 50200~50202, psb_ref 50210~50212 (retry 충돌 방지)
- [ ] 캐릭터 이름이 prompt 안에 명시 (named Siu / Gayoung / Yeonwoo)

### 2.3 완료 조건

- prompt 표 파일 작성 완료
- 사용자 prompt 표 quick review (5분) 통과

### 2.4 commit 메시지

```
5020 Sprint 1 / S01 — Starter Hero prompt table 6장 (1/7)

[변경]
  + docs/prd/5020/5020_prompt_table.md (3 hero × 2 종류 = 6 prompt)

[결정]
  - D1: 전사 시우 / 궁수 가영 / 마법사 연우 (사용자 결정)
  - D5-B: idle 3 + psb_ref 3 (Phase B PSB 출발 자료)

[검증]
  - 시드 namespace: idle 50200~50202, psb_ref 50210~50212
  - 캐릭터 이름 prompt 명시 (named Siu/Gayoung/Yeonwoo)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — ComfyUI 배치 6 PNG (1 commit)

### 3.1 작업 — Batch script 작성 + 실행

**파일**: `scripts/5020_starter_hero_batch.py` (5019 답습)

**산출물**:
- 6 출력 일괄 생성 함수
- 시드 namespace: 50200~50202 (idle), 50210~50212 (psb_ref)
- 출력:
  - `Assets/_Project/Art/Generated_v2/Characters/Char_Hero{Warrior,Archer,Mage}.png`
  - `Assets/_Project/Art/References/Characters_PSB/Char_Hero{Warrior,Archer,Mage}_psb_ref.png` (폴더 신규 생성)
- 실패 시 seed +1 retry (최대 3회)
- CLI: `python3 5020_starter_hero_batch.py [hero_id] [kind]` — 단일 retry 가능

**스크립트 골격** (5019 답습):
```python
HEROES = {
    "HeroWarrior": {
        "name": "시우 / Siu",
        "idle":    {"seed": 50200, "out": "Generated_v2/Characters/Char_HeroWarrior.png",
                    "prompt": "...young boy hero named Siu, light leather chest plate..."},
        "psb_ref": {"seed": 50210, "out": "References/Characters_PSB/Char_HeroWarrior_psb_ref.png",
                    "prompt": "...young boy hero T-pose, arms straight to sides..."},
    },
    "HeroArcher": {...},
    "HeroMage":   {...},
}
```

### 3.2 사용자 review iteration

5011 답습:
1. 1차 6장 생성 → 사용자 시각 review
2. "어른 톤" / "직업 식별 약함" / "비율 깨짐" 등 피드백 시 prompt 보강 + retry
3. healthy 6/6 도달 시 종결
4. 부분 실패 시 단일 retry: `python3 5020_starter_hero_batch.py HeroArcher idle`

### 3.3 검증 기준

- [ ] PNG 6장 모두 생성됨
- [ ] 각 PNG 768 × 1024 RGBA, transparent background
- [ ] 단일 캐릭터 (multiple/twin 없음)
- [ ] idle 3장: 직업별 무기 식별 가능 (검·활·지팡이) + 어린이 비율 (3-head)
- [ ] psb_ref 3장: T-pose 또는 명확한 부위 분리, 무기·모자 분리 visible
- [ ] PG-12 톤
- [ ] 사용자 시각 review "직업 정체성 + 캐릭터 이름 톤 정합" 승인

### 3.4 완료 조건

- 사용자 OK
- `Generated_v2/Characters/` 6 파일 (구 3 PNG 와 신 3 PNG 공존 — Sprint 3a 에서 구 PNG 정리)

### 3.5 commit 메시지

```
5020 Sprint 2 / S02 — ComfyUI 6 PNG (idle 3 + psb_ref 3) (2/7)

[변경]
  + scripts/5020_starter_hero_batch.py (5019 답습 — 6 출력)
  + Generated_v2/Characters/Char_HeroWarrior.png (시우, idle)
  + Generated_v2/Characters/Char_HeroArcher.png  (가영, idle)
  + Generated_v2/Characters/Char_HeroMage.png    (연우, idle)
  + References/Characters_PSB/Char_HeroWarrior_psb_ref.png (시우 T-pose)
  + References/Characters_PSB/Char_HeroArcher_psb_ref.png  (가영 T-pose)
  + References/Characters_PSB/Char_HeroMage_psb_ref.png    (연우 T-pose)

[검증]
  - 6/6 healthy (768×1024, RGBA)
  - 직업 정체성 + 어린이 비율 + PG-12 톤 사용자 승인
  - psb_ref T-pose Phase B 출발 자료 OK

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3a — D3-B 파일 rename (1 commit)

### 4.1 작업 — `git mv` 일괄 (GUID 보존 핵심)

**SO rename**:
```bash
git mv LittleHero/Assets/_Project/Data/Characters/Char_DefaultBoy.asset       Char_HeroWarrior.asset
git mv LittleHero/Assets/_Project/Data/Characters/Char_DefaultBoy.asset.meta  Char_HeroWarrior.asset.meta
git mv LittleHero/Assets/_Project/Data/Characters/Char_BraveGirl.asset        Char_HeroArcher.asset
git mv LittleHero/Assets/_Project/Data/Characters/Char_BraveGirl.asset.meta   Char_HeroArcher.asset.meta
git mv LittleHero/Assets/_Project/Data/Characters/Char_QuietFriend.asset      Char_HeroMage.asset
git mv LittleHero/Assets/_Project/Data/Characters/Char_QuietFriend.asset.meta Char_HeroMage.asset.meta
```

**구 PNG 정리** (Sprint 2 에서 신 PNG 가 신규 파일로 생겼으므로):
```bash
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_DefaultBoy.png
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_DefaultBoy.png.meta
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_DefaultBoy_idle.png
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_DefaultBoy_idle.png.meta
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_BraveGirl.png
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_BraveGirl.png.meta
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_BraveGirl_idle.png
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_BraveGirl_idle.png.meta
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_QuietFriend.png
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_QuietFriend.png.meta
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_QuietFriend_idle.png
git rm LittleHero/Assets/_Project/Art/Generated_v2/Characters/Char_QuietFriend_idle.png.meta
```

> 참고: `References/Selected_Characters/` 의 옛 reference PNG 는 보존 (history 자료).

**SO 필드 갱신** (rename 후):
- `Char_HeroWarrior.asset`: `id: hero_warrior`, `displayName: 전사 시우`
- `Char_HeroArcher.asset`: `id: hero_archer`, `displayName: 궁수 가영`
- `Char_HeroMage.asset`: `id: hero_mage`, `displayName: 마법사 연우`
- `portrait: {fileID: ..., guid: <신 PNG GUID>, type: 3}` — 신 PNG GUID 로 업데이트

> portrait GUID 매핑: Sprint 2 신규 PNG `.meta` 의 GUID 확인 후 SO 의 portrait 필드에 입력 (Unity Editor 에서 drag&drop 또는 YAML 직접 편집).

### 4.2 검증 기준

- [ ] `git status` 에서 6 파일 R (rename) + 12 파일 D (delete, 구 PNG + meta)
- [ ] Unity 콘솔: 도메인 리로드 후 에러 0
- [ ] CharacterData Inspector 에서 portrait 미리보기 정상 (신 PNG)
- [ ] AssetDatabase.LoadAssetAtPath 로 신 경로 (Char_HeroWarrior.asset) 로드 성공

### 4.3 완료 조건

- 파일 rename + GUID 보존 + portrait wiring ✅

### 4.4 commit 메시지

```
5020 Sprint 3a / S03a — D3-B SO/PNG 파일 rename (3/7)

[변경]
  R Char_DefaultBoy.asset   → Char_HeroWarrior.asset (id: hero_warrior)
  R Char_BraveGirl.asset    → Char_HeroArcher.asset  (id: hero_archer)
  R Char_QuietFriend.asset  → Char_HeroMage.asset    (id: hero_mage)
  D 구 Char_*_idle.png + Char_*.png 6 파일 (Sprint 2 신 PNG 로 대체)
  ~ 3 SO portrait GUID 신 PNG 로 갱신

[검증]
  - Unity 콘솔 에러 0 (도메인 리로드 후)
  - CharacterData portrait 미리보기 정상
  - git status: 6 R + 12 D

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. Sprint 3b — SaveDataMigration (1 commit)

### 5.1 작업 — SaveDataMigration 헬퍼 신규

**파일**: `LittleHero/Assets/_Project/Scripts/Core/SaveDataMigration.cs` (신규)

**산출물**:
```csharp
namespace LittleHero
{
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
            // 추가 필드 (unlocked characters list 등) 도 동일 매핑
            return data;
        }
    }
}
```

**SaveSystem 진입부 hook** (1 줄 추가):
```csharp
// SaveSystem.Load() 내부, deserialize 직후
data = SaveDataMigration.Apply(data);
```

**라인 수**: ≤ 60L (CLAUDE.md §15.6)

### 5.2 검증 기준

- [ ] 신규 클래스 컴파일 OK
- [ ] SaveSystem hook 1 줄 추가 (≤ 5L)
- [ ] 기존 SaveData (`default_boy` 저장) 로드 시 `hero_warrior` 로 자동 변환
- [ ] 신 SaveData (`hero_warrior` 저장) 로드 시 변경 없음

### 5.3 완료 조건

- SaveDataMigration.cs 작성
- SaveSystem hook 적용
- 컴파일 PASS

### 5.4 commit 메시지

```
5020 Sprint 3b / S03b — SaveDataMigration D3-B id 매핑 헬퍼 (4/7)

[변경]
  + Core/SaveDataMigration.cs (≤ 60L, 정적 헬퍼, §15.7 DI)
  ~ Core/SaveSystem.cs Load() 내부 1 줄 (Apply hook)

[검증]
  - 컴파일 OK
  - 구 id (default_boy/brave_girl/quiet_friend) 자동 매핑 검증 (Sprint 4 NUnit)
  - SaveSystem 단일 진입부 hook (§15.1 SRP)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 6. Sprint 3c — Sprite Import + displayName (1 commit)

### 6.1 작업 6c-1 — Pivot 일괄 변경

**대상**: 3 idle PNG 의 `.meta` 파일 (`Char_HeroWarrior.png.meta` 등)

**변경**:
```yaml
# Before (5008 default)
spritePivot: {x: 0.5, y: 0.5}
alignment: 0  # Center

# After (5020 보고서 §3-A)
spritePivot: {x: 0.5, y: 0.1}
alignment: 9  # Custom (또는 7=Bottom Center)
```

> psb_ref PNG 는 변경 X (in-game 미사용).

수동 편집 가능 (Unity Sprite Editor → Pivot Custom (0.5, 0.1) → Apply).

### 6.2 작업 6c-2 — displayName 갱신 (이미 Sprint 3a 에서 적용됨, 재검증)

| SO | displayName |
|---|---|
| `Char_HeroWarrior.asset` | **전사 시우** |
| `Char_HeroArcher.asset` | **궁수 가영** |
| `Char_HeroMage.asset` | **마법사 연우** |

> Sprint 3a 에서 이미 적용했으면 이 작업 skip 가능. 미적용 시 본 Sprint 에서 처리.

### 6.3 작업 6c-3 — `References/Characters_PSB/` 빌드 제외

옵션 A (간단): `.gitignore` 또는 Unity AssetBundle 제외 라벨
옵션 B (권장): 폴더명을 `Editor/References/Characters_PSB/` 로 이동 — Unity 자동 빌드 제외

> Editor 폴더 룰: Unity 가 빌드 시 자동 제외 (Editor 어셈블리 전용). PSB reference 는 Editor 작업용 자료이므로 정합.

### 6.4 검증 기준

- [ ] 3 idle sprite pivot (0.5, 0.1) 적용 — Sprite Editor 에서 확인
- [ ] CharacterData Inspector 에서 displayName 한글 정상 노출
- [ ] `References/Characters_PSB/` 빌드 제외 적용 (Editor/ 이동 또는 gitignore)
- [ ] Unity 콘솔 에러 0

### 6.5 commit 메시지

```
5020 Sprint 3c / S03c — pivot 발 밑 + PSB ref 빌드 제외 (5/7)

[변경]
  ~ 3 idle PNG .meta — pivot (0.5, 0.5) → (0.5, 0.1)
  ~ References/Characters_PSB/ → Editor/References/Characters_PSB/ (빌드 제외)

[검증]
  - Sprite Editor pivot 적용 확인
  - displayName "전사 시우 / 궁수 가영 / 마법사 연우" 정상
  - PSB ref Editor 폴더 자동 빌드 제외

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 7. Sprint 4 — Auditor + Tests (1 commit)

### 7.1 작업 7-1 — StarterHeroAuditor

**파일**: `LittleHero/Assets/_Project/Scripts/Editor/StarterHeroAuditor.cs` (신규, 5008 답습)

**산출물**:
- 메뉴: `Tools/LittleHero/Audit Starter Hero Sprite Quality`
- 검사 항목 (3 hero 각각):
  - PNG 존재 (`Char_HeroWarrior.png` 등)
  - 해상도 768 × 1024
  - transparent ratio 5~50%
  - alignment = Custom + pivot ≈ (0.5, 0.1)
  - CharacterData portrait wiring null 검사
  - id 정합 (`hero_warrior` / `hero_archer` / `hero_mage`)
  - className 정합 (`전사` / `궁수` / `마법사`)

**라인**: ≤ 80L (CLAUDE.md §15.6)

### 7.2 작업 7-2 — CharacterDataIntegrityTests

**파일**: `LittleHero/Assets/_Project/Tests/Editor/CharacterDataIntegrityTests.cs` (신규)

**산출물** — NUnit 4 케이스:
1. `Warrior_HasStr2x_Id_HeroWarrior` — Char_HeroWarrior.asset
2. `Archer_HasAgi2x_Id_HeroArcher` — Char_HeroArcher.asset
3. `Mage_HasWis2x_Id_HeroMage` — Char_HeroMage.asset
4. `AllStarterHeroes_HavePortraitWired` — 3 SO portrait != null

```csharp
[Test]
public void Warrior_HasStr2x_Id_HeroWarrior() {
    var data = AssetDatabase.LoadAssetAtPath<CharacterData>(
        "Assets/_Project/Data/Characters/Char_HeroWarrior.asset");
    Assert.That(data.id,                  Is.EqualTo("hero_warrior"));
    Assert.That(data.className,           Is.EqualTo("전사"));
    Assert.That(data.strengthMultiplier,  Is.EqualTo(2f));
    Assert.That(data.agilityMultiplier,   Is.EqualTo(0.5f));
    Assert.That(data.wisdomMultiplier,    Is.EqualTo(0.5f));
}
```

### 7.3 작업 7-3 — SaveDataMigrationTests

**파일**: `LittleHero/Assets/_Project/Tests/Editor/SaveDataMigrationTests.cs` (신규)

**산출물** — NUnit 3 케이스:
1. `MigrateCharacterId_DefaultBoy_ReturnsHeroWarrior`
2. `MigrateCharacterId_BraveGirl_ReturnsHeroArcher`
3. `MigrateCharacterId_QuietFriend_ReturnsHeroMage`
4. `MigrateCharacterId_AlreadyNew_ReturnsSame` — `hero_warrior` 입력 시 그대로
5. `MigrateCharacterId_Empty_ReturnsEmpty` — empty/null 안전

### 7.4 작업 7-4 — ProjectValidator 룰 추가 (선택)

**파일**: `LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs` (수정)

새 룰 `StarterHero_PortraitWired`:
- 3 SO 각각 portrait 필드 null 검사
- 위반 시 Validate Project 메뉴에서 경고

### 7.5 검증 기준

- [ ] StarterHeroAuditor 실행 → 3/3 PASS (id + className + portrait + pivot)
- [ ] NUnit 추가 8 케이스 (4 + 4) PASS — 총 NUnit 108 → 116
- [ ] ProjectValidator StarterHero_PortraitWired 룰 PASS
- [ ] 콘솔 에러 0

### 7.6 commit 메시지

```
5020 Sprint 4 / S04 — Auditor + NUnit 8 + Validator 룰 (6/7)

[변경]
  + Editor/StarterHeroAuditor.cs (~80L, 5008 답습)
  + Tests/Editor/CharacterDataIntegrityTests.cs (NUnit 4)
  + Tests/Editor/SaveDataMigrationTests.cs (NUnit 4)
  ~ Editor/ProjectValidator.cs (StarterHero_PortraitWired 룰 추가)

[검증]
  - Auditor 3/3 PASS
  - NUnit 108 → 116 PASS
  - Validator 룰 PASS
  - 콘솔 에러 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 8. Sprint 5 — Play 검증 + changelog (1 commit)

### 8.1 Play 검증 시나리오

**시나리오 1: Attic 진입 → 3 캐릭터 portrait 확인**
1. Attic 씬 Play 시작
2. AtticCharacterSelector 진입
3. **기대**: 3 portrait V5 sticker 직업 정체성 + 한국어 이름 라벨 ("전사 시우" 등)
4. 좌우 스와이프 → 3 캐릭터 부드럽게 전환

**시나리오 2: 캐릭터 선택 → 챕터 1 진입**
1. 시우(전사) 선택 → 챕터 1
2. **기대**: PlayerController 가 신규 portrait 사용, 발 밑 pivot 으로 정상 배치
3. 이동 시 flipX 좌우 반전 정상

**시나리오 3: 구 SaveData 마이그레이션**
1. 사전 준비: `Tools/LittleHero/Reset SaveData` 후 직접 save.json 편집:
   ```json
   { "selectedCharacterId": "default_boy", ... }
   ```
2. Game 재시작 → SaveSystem.Load() → SaveDataMigration.Apply()
3. **기대**:
   - 콘솔 NRE 없이 정상 로드
   - selectedCharacterId 가 `hero_warrior` 로 자동 변환
   - Attic 진입 시 시우(전사) 가 선택된 상태로 노출

### 8.2 changelog 추가

`docs/little_hero/changelog_v1.md` 최상단:

```markdown
- 2026-05-06: **5020 starter_hero_visual** — 시작 주인공 3종(전사 시우/궁수 가영/마법사 연우) V5 sticker 직업 정체성 재생성 (D1=시우/가영/연우 / D2-A idle 1장 / D3-B id+파일명 정합 / D4-A 코드 분리 / D5-B PSB ref 별도, 7 commit + Phase 1~4 docs). **사전조사**: SO className(전사/궁수/마법사) ↔ portrait(키즈) 미스매치 발견. **신규 산출**: docs/prd/5020/5020_prompt_table.md (6 prompt) + scripts/5020_starter_hero_batch.py (5019 답습) + Core/SaveDataMigration.cs (D3-B id 매핑) + Editor/StarterHeroAuditor.cs + 2 NUnit 클래스. **결과**: 6/6 PNG 갱신 (idle 3 + psb_ref 3), 3 SO rename (Char_DefaultBoy → Char_HeroWarrior 등), id `default_boy` → `hero_warrior` SaveData 자동 마이그레이션, sprite pivot (0.5, 0.1) 발 밑 적용, displayName "전사 시우/궁수 가영/마법사 연우". NUnit 108 → 116. PSB reference 는 Editor/ 폴더로 빌드 제외. Phase 4 출시 전 시각·데이터 정합성 핵심
```

### 8.3 commit 메시지

```
5020 Sprint 5 / S05 — Play 검증 + changelog 7/7 — 5020 종결

[변경]
  ~ docs/little_hero/changelog_v1.md (5020 항목)
  ~ CLAUDE.md §11 마일스톤 표 (선택)

[검증]
  - Play 시나리오 1~3 통과
  - SaveData 구 id (default_boy) → 신 id (hero_warrior) 자동 변환
  - 5020 전체 commit 7종 완료

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 9. 의존성 그래프

```
Sprint 1 (prompt 표 6장)
   ↓
Sprint 2 (ComfyUI 6 PNG + retry)  ← 사용자 시각 review 게이트
   ↓
Sprint 3a (git mv rename + GUID 보존)
   ↓
Sprint 3b (SaveDataMigration)  ┐
Sprint 3c (pivot + displayName) ├→ Sprint 4 (Auditor + NUnit 8)
                                ↓
                              Sprint 5 (Play 검증 + changelog)
```

병렬: Sprint 3b 와 3c 는 독립 (3a 후 동시 가능). 모두 직선 의존이라도 OK.

---

## 10. Definition of Done

- [ ] 6 PNG 생성 (idle 3 in Generated_v2 + psb_ref 3 in Editor/References)
- [ ] V5 sticker 직업 정체성 + 어린이 이름 톤 명확 (사용자 시각 review 승인)
- [ ] SO 3종 rename (Char_HeroWarrior/Archer/Mage), id 정합
- [ ] portrait GUID 보존, wiring 정상
- [ ] sprite pivot (0.5, 0.1) 적용 (in-game idle 만)
- [ ] displayName "전사 시우 / 궁수 가영 / 마법사 연우"
- [ ] SaveDataMigration 헬퍼 + SaveSystem hook + NUnit 4 PASS
- [ ] StarterHeroAuditor 3/3 PASS
- [ ] CharacterDataIntegrityTests 4 PASS (NUnit 108 → 116)
- [ ] ProjectValidator StarterHero_PortraitWired 룰 PASS
- [ ] Play 시나리오 1~3 통과 (특히 구 SaveData 마이그레이션)
- [ ] PSB reference 빌드 제외 적용
- [ ] changelog 추가
- [ ] Unity 콘솔 에러 0

---

## 11. 알려진 위험 + 완화 (D3-B + D5-B 재정리)

| 위험 | 영향 | 완화 |
|---|---|---|
| ComfyUI 비율 흔들림 | 직업 차별화 약화 | seed 고정 (idle/psb_ref 분리) + retry |
| `git mv` 실패 (.meta 누락) → GUID 변경 → wiring NRE | Player Spawn NRE | Sprint 3a 명령 명시 + Unity 콘솔 에러 0 검증 |
| 구 SaveData 사용자 캐릭터 정보 끊김 | 캐릭터 미선택 상태 | Sprint 3b SaveDataMigration + NUnit 4 검증 + Play 시나리오 3 |
| PSB reference 가 빌드 포함 | iOS 빌드 +1.5~2 MB | Sprint 3c — Editor/References/Characters_PSB/ 로 이동 (Unity 자동 제외) |
| Sprint 3 분할로 commit 트리 복잡화 | review 부담 ↑ | 3a/3b/3c 단일 책임 commit |
| character_progression_v1.md 미갱신 | 디자인 ↔ 데이터 추가 불일치 | 5020 범위 외 — 별도 문서 갱신 PR 권장 |
| displayName 한글 폰트 잘림 (Attic UI) | 시각 깨짐 | Play 시나리오 1 에서 검증, 폰트/max width 조정 |

---

## 12. Phase 4 출시 영향

- **시각 + 데이터 정합성**: SO id/className/portrait/displayName 4축 모두 직업 정합 → 마케팅 screenshot 일관
- **성능**: idle 3 PNG 만 in-game (5018 ASTC 6×6 자동), PSB ref 는 Editor/ 자동 제외 → 빌드 용량 영향 0
- **SaveData**: D3-B 마이그레이션 헬퍼로 베타 사용자 자동 호환 → 강제 reset 불필요
- **후속 PRD**:
  - **5021** (제안): Phase A 절차적 모션 코드 — PlayerController/EnemyController idle bob/anticipation/landing squash 5종
  - **5022** (출시 후): Phase B 주인공 PSB skeletal rig — 본 PRD `Editor/References/Characters_PSB/` PSB reference PNG 가 SAM segmentation 입력 자료

---

## 13. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-generator) | 초안 — 권장 D1-B/D2-A/D3-A/D4-A/D5-A, 4~5 commit |
| 2026-05-06 | Claude (사용자 결정 반영) | **확정**: D1=시우/가영/연우 / D2-A / **D3-B** / D4-A / **D5-B**. Sprint 3 → 3a/3b/3c, NUnit 108 → 116, 7 commit |
