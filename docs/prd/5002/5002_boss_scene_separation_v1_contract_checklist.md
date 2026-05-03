# Evaluator Checklist: 5002 Boss Scene Separation

> **Phase 6 Evaluator 진입용 체크리스트** — Contract 의 검증 기준을 자동/수동으로 분리.
> 입력: `5002_*_contract.md` (C1~C6 정의)

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일

- [ ] `mcp__UnityMCP__refresh_unity` 후 errors=0
- [ ] `editor_state.isCompiling == false` (5~10초 폴링)
- [ ] T01~T10 10 commit 모두 errors=0 (각 시점)

### C2 Editor Test (NUnit)

- [ ] Test Runner > Edit Mode > Run All
- [ ] `BossSceneSeparationTests` 클래스 존재
- [ ] 7+ 메서드 모두 PASS:
  - [ ] `Launcher_Default_NotNull`
  - [ ] `Mode_Enum_Has_ChapterBoss`
  - [ ] `Launcher_Launch_FillsContext_ChapterBoss`
  - [ ] `Launcher_Launch_FillsContext_Daily`
  - [ ] `CompleteChapterBoss_Null_NoOp`
  - [ ] `Context_Clear_ResetsAllFields`
  - [ ] `BackgroundApplier_Null_NoOp`
  - [ ] `BossEncounterRequest_Has_AllFields`
- [ ] 기존 NUnit 46 회귀 X (총 53+ 통과)

### C3 ProjectValidator

- [ ] `Tools/LittleHero/Validate Project` 실행
- [ ] errors=0
- [ ] warnings 변동 ±0 또는 감소
- [ ] 신규 룰 — `BossEncounterContext.Set` 외부 직접 호출 0건
  - [ ] BossEncounterLauncher.cs 외 호출 0
  - [ ] BossEncounterRunner.cs 외 호출 0
  - [ ] BossEncounterContext.cs 외 호출 0
- [ ] 라인 수 검증:
  - [ ] BossEncounterRunner ≤ 200L (~145L 예상)
  - [ ] ChapterRunner ≤ 200L (~185L 예상)
  - [ ] EndgameMenuPanel ≤ 200L (~150L 예상)

### C4 Reflection 정적 검증

- [ ] `LittleHero.BossEncounterRequest` (struct) 6 필드
  - [ ] mode / boss / endgameMode / chapter / mythicPool / returnSceneName
- [ ] `LittleHero.IBossEncounterLauncher` (interface) — `Launch(BossEncounterRequest)` 메서드
- [ ] `LittleHero.BossEncounterLauncher` (static) — `Default` property + `Launch(req)` 메서드
- [ ] `LittleHero.BossEncounterContext.Mode` enum — `Daily / Weekly / ChapterBoss` 3 값
- [ ] `LittleHero.BossEncounterContext` — `PendingChapter / PendingReturnScene` 정적 필드
- [ ] `LittleHero.BackgroundApplier` (static) — `Apply(SpriteRenderer, ChapterData)` 메서드
- [ ] `LittleHero.BossEncounterService` — `CompleteChapterBoss(EnemyData, ChapterData)` 정적
- [ ] `LittleHero.MetaProgressionData` — `pendingChapterClearedId` (string) 필드
- [ ] `LittleHero.AtticController` — `HandlePendingChapterCleared` 메서드 (BindingFlags.NonPublic)

### C5 SO + SaveData 검증

- [ ] (자동) MetaProgressionData JSON round-trip 1회 — pendingChapterClearedId 보존
- [ ] (자동) 기존 save.json 호환 — 필드 부재 시 빈 문자열 자동 (Newtonsoft 또는 Unity JSON)
- [ ] (수동) BossEncounter.unity Inspector — `_backgroundRenderer` Background.SpriteRenderer 참조
- [ ] EnemyData 51 보스 회귀 X (`bossTier`, `lootTable` wiring 그대로)
- [ ] BossLootTable 4 인스턴스 회귀 X

## 수동 검증 (사용자 Play Mode)

### C6.1 시나리오 S1 — 챕터 1 보스 정상 처치

- [ ] 챕터 1 wave 1~19 진행 (미니보스 5/10/15 OK)
- [ ] wave 20 직전 BossEncounter 씬 전환
- [ ] BossEncounter 씬 BG = chapter_1.coverArt (red_riding_hood)
- [ ] Phase 1 보스 패턴 정상
- [ ] HP 50% 시 Phase 2 전환 + 1.3× 속도
- [ ] 보스 처치 → Attic 복귀
- [ ] 콘솔 로그 4종 확인:
  - [ ] `[BossEncounter] 시작 — type=ChapterBoss`
  - [ ] `[BossEncounterService] CompleteChapterBoss`
  - [ ] `[BossLootGrantService] Applied ChapterBoss`
- [ ] SaveData `firstClearedChapters` bit 0 set
- [ ] StarFragments / Starlight / MagicCrystal 증가
- [ ] Attic 진입 후 ChapterClearedPanel 1회 toast (D2)
- [ ] **(D5)** Phase 2 텔레그라프 0.7s + 1.3× 속도
- [ ] **(D5)** Critical 적중 slow-mo + screen shake
- [ ] **(D5)** VfxPool / DamageFloater pop
- [ ] **(D5)** DontDestroyOnLoad 객체 BossEncounter 씬에서 참조 가능

### C6.2 시나리오 S2 — 챕터 보스 사망

- [ ] wave 20 진입 → 의도 사망
- [ ] 콘솔 `[BossEncounter] 사망 — 보상 없이 Attic 복귀`
- [ ] SaveData firstClearedChapters 변동 X
- [ ] SaveData pendingChapterClearedId == "" 유지
- [ ] BossLootGrantService.Apply 호출 X

### C6.3 시나리오 S3 — 일일 보스 회귀

- [ ] Endgame 메뉴 일일 보스 클릭 → BossEncounter 씬
- [ ] 챕터 BG 미적용 (PendingChapter == null)
- [ ] 보스 처치 → Starlight 증가 + 일일 마크
- [ ] 콘솔 `[EndgameMenu] 일일 보스`
- [ ] BossEncounterLauncher.Launch(Daily req) 호출 (정적 Context.Set 직접 호출 X)

### C6.4 시나리오 S4 — 주간 보스 신화 발현 회귀

- [ ] Endgame 주간 보스 처치
- [ ] 신화 발현 (장착 인스턴스 중 미발현 1+ 시)
- [ ] Attic 복귀 후 Mythic Overlay 1회
- [ ] LastMythicDefinition Clear (재진입 시 중복 X)

### C6.5 시나리오 S5 — 챕터 BG 적용 (5002 신규)

- [ ] 챕터 5 보스 진입
- [ ] BossEncounter 씬 BG = chapter_5_oz.coverArt
- [ ] sortingOrder == -100, drawMode == Tiled, size == (300, 300)

## 비기능 검증

- [ ] 기존 5001 boss_system_v2 회귀 X (Phase 1/2 / BossLootTable / IBossPhase)
- [ ] 기존 weapon_v2 가챠 회귀 X
- [ ] 기존 stats_v2 데미지 공식 회귀 X
- [ ] 60fps 유지 (BossEncounter 씬 단일 보스 — 부담 거의 없음)
- [ ] iOS 빌드 (해당 시)

## 합격 판정

| 조건 | 판정 |
|---|---|
| 자동 (C1~C5) 모두 ✅ + 수동 S1, S2 ✅ | **최소 합격 (PASS)** |
| 자동 (C1~C5) 모두 ✅ + 수동 S1~S5 모두 ✅ | **완전 합격 (PASS)** |
| 자동 (C1~C5) 1개라도 ❌ | **FAIL** → Phase 5 재진입 (max 2회) |
| 자동 모두 ✅ + 수동 1개라도 ❌ | **FAIL** → 수정 지시 후 재검증 |
