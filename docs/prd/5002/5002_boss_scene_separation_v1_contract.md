# Sprint Contract: 5002 Boss Scene Separation

> **PGE Planner — Phase 4 Contract**
> PRD: `5002_boss_scene_separation_v1_v2_prd.md` (mediator 확정)
> Breakdown: `5002_*_breakdown_00.md` (10 태스크)
> 생성일: 2026-05-04

---

## 1. C1 컴파일 기준

- **C1.1**: 모든 신규 .cs 파일 컴파일 errors=0 (각 commit 시점)
- **C1.2**: 도메인 리로드 후 5~10초 내 안정화 (`editor_state.isCompiling == false`)
- **C1.3**: T01~T10 각 commit 후 errors=0 유지

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 5  # T04, T06 은 10초
mcp__UnityMCP__read_console (errors)  # 0 이어야 함
```

**Pass/Fail**:
- ✅ 모든 commit errors=0 → PASS
- ❌ 1개 commit이라도 errors>0 → FAIL → Phase 5 재진입

---

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 수 | 위치 |
|---|---|---|
| `BossSceneSeparationTests` | 7+ | `Tests/Editor/BossSceneSeparationTests.cs` |

### 2.1 BossSceneSeparationTests 명세 (T09)

- `Launcher_Default_NotNull` — `BossEncounterLauncher.Default != null`
- `Mode_Enum_Has_ChapterBoss` — `BossEncounterContext.Mode.ChapterBoss` enum 값 존재
- `Launcher_Launch_FillsContext_ChapterBoss` — Mock 주입 후 Launch → req 전달 검증
- `Launcher_Launch_FillsContext_Daily` — Daily 모드 동일 검증
- `CompleteChapterBoss_Null_NoOp` — null 인자 → 예외 없이 return
- `Context_Clear_ResetsAllFields` — Set 후 Clear → 5 필드 모두 null
- `BackgroundApplier_Null_NoOp` — null renderer / null chapter → no-op
- `BossEncounterRequest_Has_AllFields` — Reflection 6 필드 존재

**검증 방법**: Test Runner Edit Mode > Run All → 7+ PASS, 기존 46 회귀 X (총 53+)

**Pass/Fail**:
- ✅ 신규 7+ 모두 PASS + 기존 46 회귀 X → PASS
- ❌ 1개라도 FAIL → FAIL → Phase 5 재진입

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ±0 또는 감소 (기존 5001 종료 시점 기준)
- **C3.3**: 신규 룰 — `BossEncounterContext.Set` 외부 직접 호출 0건 (Launcher/Runner/Context.cs 외)
- **C3.4**: BossEncounterRunner ≤ 200L (D1 통합 후 ~145L)
- **C3.5**: ChapterRunner ≤ 200L (D1 통합 후 ~185L)
- **C3.6**: EndgameMenuPanel ≤ 200L (T08 정리 후 ~150L)
- **C3.7**: AtticController ~415L 허용 (D2 추가, 후속 분리 task 등록 — 5002 스코프 외)

**검증 방법**: `Tools/LittleHero/Validate Project` 실행 → 콘솔 출력 비교

**Validator 룰 추가 명세** (T09):
- `BossEncounterContext.Set` 외부 직접 호출 검사 (BossEncounterLauncher / BossEncounterRunner / BossEncounterContext.cs 외 0건)

**Pass/Fail**:
- ✅ errors=0 + 신규 룰 0 위반 → PASS
- ❌ errors>0 또는 신규 룰 1+ 위반 → FAIL

---

## 4. C4 Reflection 검증 (정적)

| Type | Method/Field |
|---|---|
| `LittleHero.BossEncounterRequest` (struct) | 6 필드: mode / boss / endgameMode / chapter / mythicPool / returnSceneName |
| `LittleHero.IBossEncounterLauncher` (interface) | `Launch(BossEncounterRequest)` 메서드 |
| `LittleHero.BossEncounterLauncher` (static class) | `Default` property + `Launch(BossEncounterRequest)` 정적 메서드 |
| `LittleHero.BossEncounterContext.Mode` (enum) | `Daily / Weekly / ChapterBoss` |
| `LittleHero.BossEncounterContext` (static class) | `PendingChapter / PendingReturnScene` 정적 필드 신규 |
| `LittleHero.BackgroundApplier` (static class) | `Apply(SpriteRenderer, ChapterData)` 정적 메서드 |
| `LittleHero.BossEncounterService` (static class) | `CompleteChapterBoss(EnemyData, ChapterData)` 정적 메서드 |
| `LittleHero.MetaProgressionData` (또는 SaveData) | `pendingChapterClearedId` (string) 필드 |
| `LittleHero.AtticController` | `HandlePendingChapterCleared` 메서드 (private 가능 — reflection 으로 BindingFlags.NonPublic) |

**검증 방법** (script):
```csharp
var t = System.Type.GetType("LittleHero.IBossEncounterLauncher, Assembly-CSharp");
return t != null && t.GetMethod("Launch") != null;
```

**Pass/Fail**:
- ✅ 9 Type/Method/Field 모두 존재 → PASS
- ❌ 1개라도 누락 → FAIL → 해당 태스크 재구현

---

## 5. C5 SO + SaveData 검증

| 항목 | 검증 |
|---|---|
| MetaProgressionData JSON 직렬화 | `pendingChapterClearedId` 필드 — save/load 1회 round-trip 후 빈 문자열 또는 챕터 id 유지 |
| 기존 SaveData 호환 | 기존 save.json 로드 시 `pendingChapterClearedId` 필드 부재 → 빈 문자열 기본값 자동 (Unity Serialization) |
| EnemyData 회귀 | 기존 51 보스 SO `bossTier`, `lootTable` wiring 회귀 X |
| BossLootTable 회귀 | 4 인스턴스 (MiniBoss/ChapterBoss/DailyBoss/WeeklyBoss) 회귀 X |
| BossEncounter.unity 씬 | `_backgroundRenderer` SerializeField 가 Background.SpriteRenderer 참조 |

**검증 방법**:
- Unity 새 SaveData 생성 → 챕터 1 보스 처치 → save.json 에 `pendingChapterClearedId == "ch1"` 확인
- 다락방 진입 후 → `pendingChapterClearedId == ""` 확인 (1회 toast 후 clear)
- BossEncounter 씬 Inspector — `_backgroundRenderer` 드래그 wiring 확인

**Pass/Fail**:
- ✅ 모든 항목 통과 → PASS
- ❌ SaveData 직렬화 실패 또는 wiring 누락 → FAIL

---

## 6. C6 Play 검증 시나리오

PRD §검증 시나리오 S1~S5 인용. (D5 Rebuttal 자동 수용 보강 포함)

### 시나리오 S1: 챕터 1 보스 처치 (정상 흐름)

**사전 조건**:
- 챕터 1 SO + bossEnemy + lootTable wiring
- BossEncounter 씬 Background SpriteRenderer wiring (T05)
- 새 SaveData 또는 챕터 1 미클리어 상태

**단계**:
1. Attic → 챕터 1 시작 → wave 1~19 진행 (미니보스 5/10/15 정상 등장)
2. wave 20 진입 직전 BossEncounter 씬 전환
3. BossEncounter 씬: 챕터 1 BG (red_riding_hood coverArt) + 보스 단독 spawn + Phase 1 패턴
4. HP 50% 도달 → Phase 2 전환 + 1.3× 속도
5. 보스 처치 → 별 흩어짐 VFX 2초 → Attic 복귀

**기대 결과**:
- 콘솔 `[BossEncounter] 시작 — type=ChapterBoss boss=...`
- 콘솔 `[BossEncounterService] CompleteChapterBoss: ... (chapter 1)`
- 콘솔 `[BossLootGrantService] Applied ChapterBoss (4 rewards)`
- SaveData `firstClearedChapters` bit 0 set
- SaveData `pendingChapterClearedId == "ch1"` (직후), Attic 진입 후 `""`
- StarFragments / Starlight / MagicCrystal 증가
- Attic 진입 후 ChapterClearedPanel 1회 toast 표시
- **(D5)** Phase 2 진입 시 텔레그라프 0.7s + 1.3× 속도 정상
- **(D5)** Critical 적중 시 slow-mo 0.08s + screen shake 정상
- **(D5)** VfxPool.SpawnHitStar / DamageFloater pop 정상
- **(D5)** DontDestroyOnLoad: GameManager / AudioManager / VfxPool 객체 BossEncounter 씬에서 참조 가능

---

### 시나리오 S2: 챕터 보스 사망 (실패 흐름)

**사전 조건**: S1 사전 조건 동일

**단계**:
1. wave 20 진입 → BossEncounter 씬
2. 의도적 사망 (Player HP=0)

**기대 결과**:
- 콘솔 `[BossEncounter] 사망 — 보상 없이 Attic 복귀`
- SaveData `firstClearedChapters` 변동 X
- SaveData `pendingChapterClearedId == ""` 유지
- BossLootGrantService.Apply 호출 X
- 챕터 1 재진입 시 wave 20 또는 첫 wave 부터 재개 (`activeChapterId` 처리 검증)

---

### 시나리오 S3: 일일 보스 (회귀 — Daily)

**사전 조건**:
- Endgame 잠금 해제 (CH 35 클리어 또는 firstClearedChapters bit 35 set)
- 일일 보스 가용 (오늘 미도전)

**단계**:
1. Attic → Endgame 메뉴 → 일일 보스 클릭
2. BossEncounter 씬 (BG 미적용 — 기본 또는 빈 sprite)
3. 보스 처치
4. Attic 복귀

**기대 결과**:
- 콘솔 `[EndgameMenu] 일일 보스: ...`
- BossEncounterLauncher.Launch(Daily req) 호출 (정적 Context.Set 직접 호출 X — Launcher 내부만)
- Starlight 보상 증가
- 일일 마크 set (다음 KST 0시까지 재도전 X)
- 챕터 BG 미적용 (BossEncounterContext.PendingChapter == null → BackgroundApplier no-op)

---

### 시나리오 S4: 주간 보스 신화 발현 (회귀 — Weekly)

**사전 조건**:
- Endgame 잠금 해제
- 주간 보스 가용
- 장착 인스턴스 중 신화 미발현 1+개

**단계**:
1. Endgame 메뉴 → 주간 보스 클릭 → 처치

**기대 결과**:
- WeeklyBossResult.success=true / mythicGranted != null
- AtticController.HandleReturnFromBossEncounter 1회 발동 → Mythic Overlay 1회
- LastMythicDefinition Clear (재진입 시 중복 X)
- 주간 마크 set

---

### 시나리오 S5: 챕터 BG 적용 검증 (5002 신규)

**사전 조건**: 챕터 5 (오즈의 마법사) 미클리어 또는 진입 가능

**단계**:
1. 챕터 5 진입 → wave 20 보스 진입
2. BossEncounter 씬 진입 후 Inspector 확인

**기대 결과**:
- BossEncounter 씬 Background.SpriteRenderer.sprite == ChapterData(chapter_5).coverArt
- sortingOrder == -100
- drawMode == Tiled
- size == (300, 300)
- color == white

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (모든 commit)
- [ ] Editor Test 7+/7+ PASS (BossSceneSeparationTests)
- [ ] 기존 NUnit 46 회귀 X (총 53+)
- [ ] ProjectValidator errors=0 / 신규 룰 0 위반
- [ ] 기존 시스템 회귀 X (5001 boss_system_v2 / weapon_v2 / stats_v2 동작 유지)
- [ ] 60fps 유지 (BossEncounter 씬 단일 보스, 부담 X)
- [ ] 빌드 (Unity → Build) 성공 (해당 시)

## 8. 합격 기준

**최소 합격**:
- C1 컴파일 ✅
- C2 Test 7/7 PASS ✅
- C3 Validator errors=0 ✅
- 시나리오 S1, S2 PASS ✅

**완전 합격**:
- 모든 자동 카테고리 (C1~C5) ✅
- 시나리오 S1~S5 모두 PASS ✅
- 비기능 요구사항 모두 ✅

## 9. Trade-off (mediator 결정 기반)

- **D1 채택 (BackgroundApplier 추출)**: ChapterRunner 의 기존 ApplyChapterBackground 도 통합 → DRY 강화. Trade-off: 정적 헬퍼 추가 1 파일.
- **D2 채택 (SaveData pendingChapterClearedId)**: SaveData 필드 +1, 마이그레이션 자동. Trade-off: AtticController.Start 책임 +1.
- **D3 채택 (5003 후속 명시)**: 5002 스코프 명확. Trade-off: 미니보스 분리는 별 sprint 검토.
- **D4 채택 (5004 후속 명시)**: 5002 스코프 명확. Trade-off: 본 sprint sync 전환 1회 (~1초) UX 부담.
- **D5 채택 (S1 검증 보강)**: 회귀 검증 강화. Trade-off: PRD 텍스트 4줄 추가만.

## 10. Phase 6 Evaluator 진입 조건

- T01~T10 10 commit 완료 (push 가능)
- 본 Contract 의 모든 자동 카테고리 검증 후 → 시나리오 S1~S5 사용자 보고 대기
- FAIL 시 → 수정 지시 → Phase 5 재진입 (max 2회)

## 11. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract). C1~C6 / 합격 기준 / Trade-off
