# Evaluator Checklist: 5021 Starter Hero Chapter Flow

> **PGE Phase 6 Evaluator 자동/수동 체크리스트.**
> Contract: `5021_starter_hero_chapter_flow_contract.md`

---

## 자동 검증 (스크립트 / Unity batchmode)

### C1 컴파일
- [ ] `verify_phase6.sh c1` 실행 → exit 0
- [ ] `error CS` 0건 (`grep -cE "error CS" /tmp/lh_verify_c1.log`)
- [ ] "Exiting batchmode successfully" 라인 확인

### C2 Editor Test (NUnit)
- [ ] `verify_phase6.sh c2` 실행 → exit 0
- [ ] `failed="0"` in `/tmp/lh_test_results.xml`
- [ ] total=120 (5020 117 + 5021 신규 3)
- [ ] `MetaProgressionInitTests` 클래스 4 [Test] 메서드 모두 PASS:
  - [ ] ResolveCharacter_EmptyId_RemainsNull
  - [ ] ResolveCharacter_HeroWarrior_ResolvesToWarriorSO
  - [ ] ResolveCharacter_AfterMigration_DefaultBoy_ResolvesToHeroWarrior
- [ ] 5020 NUnit 9 회귀 0 (CharacterDataIntegrityTests 4 + SaveDataMigrationTests 5)

### C3 ProjectValidator
- [ ] `verify_phase6.sh c3` 실행 → exit 0
- [ ] `[Validator] 완료. errors=0` 확인
- [ ] warnings ≤ 172 (5021 시작 baseline)
- [ ] 5021 신규 룰 `ValidateCharacterPool` 결과 errors=0
- [ ] 5020 룰 `ValidateStarterHeroPortraits` 회귀 0

### C4 Reflection 정적 검증
- [ ] CharacterPool 클래스 + characters 필드 + FindById 메서드:
  ```bash
  grep -nE "public class CharacterPool|public CharacterData\[\] characters|public CharacterData FindById" \
    LittleHero/Assets/_Project/Scripts/Data/CharacterPool.cs
  # 기대: 3 매치
  ```
- [ ] MetaProgression hook:
  ```bash
  grep -nE "_characterPool|ResolveSelectedCharacterFromSave" \
    LittleHero/Assets/_Project/Scripts/Meta/MetaProgression.cs
  # 기대: 4+ 매치 (필드 1 + 메서드 1 + Awake 호출 1 + 메서드 정의 1)
  ```
- [ ] NUnit 메서드 카운트:
  ```bash
  grep -cE "\[Test\]" LittleHero/Assets/_Project/Tests/Editor/MetaProgressionInitTests.cs
  # 기대: 3
  ```
- [ ] ProjectValidator 룰 추가:
  ```bash
  grep -nE "ValidateCharacterPool" LittleHero/Assets/_Project/Scripts/Editor/ProjectValidator.cs
  # 기대: 2 매치 (정의 + Run() 호출)
  ```

### C5 SO + Asset 검증
- [ ] CharacterPool_Default.asset 파일 존재:
  ```bash
  ls LittleHero/Assets/_Project/Data/Characters/CharacterPool_Default.asset
  ```
- [ ] CharacterPool 인스턴스 (C2 NUnit ResolveCharacter_HeroWarrior PASS 흡수)
- [ ] Attic.unity scene 검증:
  - [ ] MetaProgression._characterPool 가 fileID 0 아님 (wiring)
    ```bash
    grep -A 2 "Assembly-CSharp::LittleHero.MetaProgression" LittleHero/Assets/_Project/Scenes/Attic.unity | grep _characterPool
    # 기대: guid <CharacterPool_Default GUID>
    ```
  - [ ] AtticChapterEntry GameObject m_IsActive: 0 또는 component 제거
    ```bash
    grep -B 8 "Assembly-CSharp::LittleHero.AtticChapterEntry" LittleHero/Assets/_Project/Scenes/Attic.unity | head -10
    # GameObject m_IsActive: 0 또는 component 부재 확인
    ```
  - [ ] WondrousMapNode 카운트 ≥ 10 (이상적 35):
    ```bash
    grep -c "Assembly-CSharp::LittleHero.WondrousMapNode" LittleHero/Assets/_Project/Scenes/Attic.unity
    ```

---

## 수동 검증 (사용자 Play Mode)

### C6.1 시나리오 1 — Attic 진입 자동 resolve
- [ ] Attic.unity Play ▶ → 1초 후 Console 확인
- [ ] `[MetaProgression] ItemDrop bootstrap` 정상
- [ ] `_characterPool 미wiring` Warning 0건
- [ ] AtticCharacterSelector "선택: 전사 시우" 라벨 즉시 표시
- [ ] 빨간 에러 0건

### C6.2 시나리오 2 — Chapter 직접 Play (5020 C6 2b 자동 PASS)
- [ ] Attic Play → 1초 대기 → Stop
- [ ] File > Open Scene → Chapter.unity
- [ ] Play ▶ → 시우 캐릭터 sprite 정상 노출
- [ ] **노란 둥근 원 X** (이게 5020 C6 2b 의 fail 케이스 였음 — 본 fix 로 PASS)
- [ ] flipX + 좌우 이동 정상

### C6.3 시나리오 3 — WondrousMapPanel 챕터 진입 (D1=B 검증)
- [ ] Attic Play → 다락방 또는 WondrousMapPanel 활성화 path 클릭
- [ ] 35 노드 표시 (또는 잠금 상태 정상 — 챕터 1 만 unlock)
- [ ] 챕터 1 노드 클릭 → SceneTransitionService.Transition("Chapter") Console 로그
- [ ] Chapter 씬 fade-in + portrait 정상
- [ ] (만약 35 노드 일부 누락 시) 5021.1 backlog 등재

### C6.4 시나리오 4 — 마이그레이션 + init 통합
- [ ] sed 주입:
  ```bash
  SAVE="$HOME/Library/Application Support/Bandibul/LittleHero/save.json"
  sed -i '' 's|"selectedCharacterId": "hero_warrior"|"selectedCharacterId": "default_boy"|' "$SAVE"
  grep selectedCharacterId "$SAVE"
  # 확인: "default_boy"
  ```
- [ ] Unity Editor Attic.unity Play ▶ → 1초 → Stop
- [ ] 터미널 재확인:
  ```bash
  grep selectedCharacterId "$SAVE"
  # 기대: "hero_warrior" (자동 변환)
  ```
- [ ] Console NRE 0건
- [ ] Attic UI "선택: 전사 시우" (자동 resolve)

---

## 비기능 검증

- [ ] 5020 C6 시나리오 1 (라벨) 회귀 X
- [ ] 5020 C6 시나리오 3 (마이그레이션) 회귀 X
- [ ] 5016 LitMotion punch/flash 정상 (Player 회귀)
- [ ] 60fps 유지 (Profiler 권장 — 본 fix 가 perf 영향 0 예상)

---

## 합격 판정

### 최소 합격
- 자동 검증 5/5 ✅
- 수동 시나리오 1, 2 ✅ (Attic 자동 resolve + Chapter portrait 노출)

### 완전 합격
- 자동 검증 5/5 ✅
- 수동 시나리오 1, 2, 3, 4 모두 ✅
- 비기능 요구사항 모두 ✅

### FAIL 판정
- 자동 검증 1개라도 ❌ → Phase 5 수정 후 재검증 (max 2회 반복)
- 수동 시나리오 2 (Chapter portrait) ❌ → 5005 R4 fix 미작동 → MetaProgression Awake 순서 재검토
- 수동 시나리오 3 (WondrousMapPanel) ❌ → 5021.1 별도 fix backlog 등재
