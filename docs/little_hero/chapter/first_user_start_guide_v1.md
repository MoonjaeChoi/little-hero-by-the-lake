# 최초 사용자 시작 가이드 — Little Hero by the Lake (v1)

> **작성일**: 2026-05-04
> **출처**: `EditorBuildSettings.scenes` + `MetaProgression.cs` + `AtticCharacterSelector.cs` + `AtticChapterEntry.cs` + `BossEncounterLauncher.cs` + `ChapterRunner.cs` + `Char_*.asset` + `PhaseFContentSeed.cs`
> **요약**: save.json 부재 상태에서 신규 플레이어가 다락방 → 캐릭터 선택 → CH_01 → Wave 20 보스 → 첫 클리어까지 진행하는 흐름.

## 1. 게임 시작 (앱 실행)

| 단계 | 동작 | 코드 |
|---|---|---|
| 1 | Unity 가 첫 씬 **`Attic.unity`** (다락방 허브) 로드 | `EditorBuildSettings.scenes[0]` |
| 2 | `MetaProgression.Awake()` 가 `SaveSystem.Load()` 호출 | save.json 부재 → `new SaveData()` 반환 |
| 3 | 모든 영구 데이터 default 값으로 초기화 | level=1, summonLevel=1, 화폐=0, 챕터 잠금 |
| 4 | StarlightGrantService / IAPService / ItemDropService 백그라운드 부트스트랩 | — |

빌드 씬 순서 (`ProjectSettings/EditorBuildSettings.asset`):
1. `Assets/_Project/Scenes/Attic.unity` ← 진입점
2. `Assets/_Project/Scenes/Chapter.unity`
3. `Assets/_Project/Scenes/InfiniteTower.unity`
4. `Assets/_Project/Scenes/BossEncounter.unity`

## 2. 첫 진입 — 다락방 (Attic 씬)

다락방 GameObject 에 부착된 7 컴포넌트 (5005 책임 분리 결과):

```
AtticController          가챠/한정상점 진입 + RefreshLimitedShopBadge + BGM
AtticCurrencyView        ✦◇⋆◈ 4 화폐 표시 (모두 0)
AtticCharacterSelector   캐릭터 강제 선택 prompt ← 첫 사용자 진입점
AtticChapterEntry        챕터 버튼 (CH_01 만 활성)
AtticEndgameMenuView     Endgame 메뉴 (잠금 — CH 35 첫 클리어 필요)
AtticPanelToggleManager  패널 자동 토글 (한 번에 하나만)
AtticReturnHandler       보스 씬 복귀 처리 (첫 진입 시 noop)
```

**다락방 BGM**: Joe Hisaishi 풍 nostalgic 자동 재생 (`AudioManager.PlayBgm("attic_nostalgic")`).

## 3. 캐릭터 강제 선택 (첫 사용자 전용)

`AtticCharacterSelector.Start()` 가 `SaveData.selectedCharacterId` 빈 문자열 감지 → **"영웅을 선택하세요!"** 표시. 챕터 진입은 캐릭터 선택 후에만 가능.

| 캐릭터 | id | 직업 | STR / AGI / WIS multiplier | 가격 |
|---|---|---|---:|---:|
| 용감한 소년 | `default_boy` | **전사** | **2.0** / 0.5 / 0.5 | 무료 |
| 씩씩한 소녀 | `brave_girl` | **궁수** | 0.5 / **2.0** / 0.5 | 무료 |
| 조용한 친구 | `quiet_friend` | **마법사** | 0.5 / 0.5 / **2.0** | 무료 |

선택 시 `MetaProgression.SelectCharacter(character)` → `Data.selectedCharacterId` 저장 + `OnCharacterChanged` 이벤트 → 무기 직업 검증 (weapon_v2). 첫 시작은 인벤토리가 비어있으므로 직업 변경 충돌 없음.

## 4. 챕터 1 진입 — "호숫가 작은 마을"

캐릭터 선택 후 **CH_01 버튼** (다른 챕터는 `firstClearedChapters` 비트로 잠금) 클릭:

| 단계 | 동작 |
|---|---|
| 1 | `AtticChapterEntry.HandleStart(CH_01)` → `MetaProgression.SelectedChapter = CH_01` |
| 2 | `SceneTransitionService.Transition("Chapter")` (5004 fade-out 0.3s + LoadSceneAsync) |
| 3 | Chapter 씬 로드 → `ChapterRunner.Begin()` |
| 4 | `enemyStatMultiplier = 1.0` (CH_01 베이스), `totalWaves = 20`, `waveDuration = 45초` |

## 5. Wave 1 시작 — 자동 전투

플레이어는 **가상 조이스틱 (좌하단)** 으로 이동만, **공격은 자동** (Auto-Shooter Idle):

| 시스템 | 첫 사용자 영향 |
|---|---|
| **PlayerStats v2 (3 코어)** | Lv1 STR/AGI/WIS = `1 × multiplier` (캐릭터별 1×2.0 / 1×0.5 / 1×0.5) |
| **무기 (시작 무기)** | EquipmentManager 자동 장착 (default attackPattern) |
| **데미지 v2** | `DamageDealer.EnhanceDamage = (config.damage + AttackDamage) × (1 + bonus%)` |
| **PIVOT v4 cinematic** | 동시 적 cap 300, 크리티컬 5% baseline + slow-mo 0.08s + screen shake |
| **Memory Pickup** | 일정 확률 사진/메모/스티커/낙서 등장 (4종 추억) |

## 6. Wave 5 / 10 / 15 — 미니보스

`enemyPool[0~2]` (Small/Medium/Large) → wave 5 진입 시 미니보스 (`miniBossPool[0]`) 1마리 + 일반 적. **미니보스 씬 분리는 기본값 None** (`BalanceConfig.miniBossSeparation`, 5003 옵션) — Chapter 씬 안에서 처리.

| Wave | 출현 | 처리 |
|---|---|---|
| 1~4, 6~9, 11~14, 16~19 | 일반 적 3종 | Chapter 씬 |
| **5, 10, 15** | 미니보스 1 + 일반 | Chapter 씬 (LastOnly 토글 시 wave 15만 BossEncounter 분리) |

## 7. Wave 20 — 챕터 보스 (BossEncounter 씬)

`ChapterRunner` 가 wave 20 직전 `BossEncounterLauncher.Launch(ChapterBoss)` 호출 (5002):

| 단계 | 동작 |
|---|---|
| 1 | Chapter 씬 → fade-out → BossEncounter 씬 로드 |
| 2 | `BackgroundApplier` 가 챕터 BG 동일하게 적용 |
| 3 | 챕터 보스 (CH_01 = `Enemy_Meta_Boss` 마을의 친구 으뜸 친구, HP 700/ATK 36/armor 30) |
| 4 | HP 50% 도달 시 `BossPhase2Aggressive.OnEnter` (5001) — `attackInterval × 1.3` |
| 5 | 보스 처치 → `BossLootGrantService.Apply(ChapterBoss)` — ★ 200 + ✦ 1 + ◇ 1 + 신화 0.05 확률 |
| 6 | `BossEncounterService.CompleteChapterBoss(CH_01)` → `firstClearedChapters` 비트 0 ON + `pendingChapterClearedId = "CH_01"` |
| 7 | Attic 씬 복귀 (fade) |

## 8. 첫 클리어 후 — 다락방 복귀

| 컴포넌트 | 동작 |
|---|---|
| **AtticChapterEntry** | `pendingChapterClearedId` 감지 → `RaiseChapterCleared(CH_01)` 1회 toast (5002 D2) |
| **StarlightGrantService** | CH_01 첫 클리어 → ⋆ 2 자동 지급 (`chapterStarlightGranted[0]` 비트 ON) |
| **CurrencyService** | ✦ 5 (CH_01 starFragmentReward) + 다른 보상 누적 |
| **AtticChapterEntry** | CH_02 버튼 활성 (`firstClearedChapters` 비트 0 ON 으로 잠금 해제) |
| **AtticReturnHandler** | 신화 발현 시 `MythicEffectOverlay` 1회 토스트 (확률 0.05) |

## 9. 진행 루프 (CH_02 ~ CH_35)

```
다락방 (캐릭터/장비 정비)
   ↓ 챕터 N 선택
Chapter 씬 (Wave 1~19 + 미니보스 5/10/15)
   ↓ Wave 20
BossEncounter 씬 (Phase 1 → Phase 2)
   ↓ 보스 처치 (pendingChapterClearedId)
다락방 (1회 toast + 다음 챕터 잠금 해제)
```

**챕터별 난이도**: `enemyStatMultiplier = 1.10^(N-1)` 로 자동 스케일 (CH_01 = 1.0 → CH_35 = 28.10×).

## 10. 부수 진행 (자유 시점)

| 시스템 | 첫 사용자 진입 |
|---|---|
| **무기 가챠** | 다락방 가챠 버튼 (별빛 ⋆ 100/단판 — 첫 사용자는 일일 로그인 5⋆ 누적 필요) |
| **장비 강화** | InventoryPanel 의 EnhanceModal — ★ (별가루) + ◇ 16+ (마법 결정) |
| **마법의돌** | SlotStarforcePanel — 12 슬롯 ⭐ 강화 (마법의돌 조각 ◈) |
| **추억 일기** | 다락방 DiaryPanel — 챕터 진행 중 발견한 4종 표시 |
| **도감** | BestiaryPanel — 처치한 적 자동 등록 + 챕터별 100% 시 ⋆ 50 |
| **시즌 패스** | 시즌 진행도 누적 (free 100⋆ + premium 500⋆ 등) |

## 11. Endgame 잠금 해제 (CH 35 클리어 후)

`firstClearedChapters` 35비트 모두 ON 또는 추억 4종 발견 → `AtticEndgameMenuView.Refresh()` 가 `_endgameMenuRoot.SetActive(true)` 자동 활성:

| 모드 | 잠금 해제 조건 |
|---|---|
| **일일 보스** 7종 | KST 0시 리셋 (요일별 회전) |
| **주간 보스** 4종 | 4주 사이클 (mythicChance 1.0 보장) |
| **무한 다락방** | 깊이 무제한 (×1.05^d) |
| **고난도 챕터** CH 36~50 | CH 21~35 클론 + ×2.0 추가 곱셈 |

## 12. 마지막 페이지 (히든 챕터)

추억 4종 (사진/메모/스티커/낙서) 모두 발견 → 다락방 동화책 마지막 페이지 언락 → **진엔딩**: 작은 영웅이 사실은 어린 시절의 아빠였다는 것을 깨닫는 순간.

---

## 첫 사용자 5분 체험 타임라인

```
앱 실행 (0:00)
  → 다락방 로드 + Joe Hisaishi BGM
  → "영웅을 선택하세요!" prompt

캐릭터 선택 (0:30)
  → 용감한 소년 / 씩씩한 소녀 / 조용한 친구 (모두 무료)

CH_01 진입 (1:00)
  → fade-out → Chapter 씬

Wave 1~4 자동 전투 (1:00 ~ 4:00)
  → 일반 적 3종 (HP 30/50/80) 처치 → ★ 별가루 누적

Wave 5 미니보스 (4:00 ~ 4:30)
  → 마을의 친구 수호자 (HP 200/ATK 20/armor 15)

Wave 20 보스 (15:00 전후, idle 진행)
  → fade-out → BossEncounter 씬
  → 마을의 친구 으뜸 친구 (Phase 1 → Phase 2)
  → 처치 → 보상 + 신화 5% 확률 + ✦ 5

다락방 복귀 (15:30)
  → ChapterClearedPanel 1회 toast
  → ⋆ 2 + ✦ 5 누적
  → CH_02 잠금 해제
```

## 13. SaveData 초기 상태 (default — `new SaveData()`)

| 필드 | 값 |
|---|---|
| `level / xp` | 1 / 0 |
| `starFragments` (✦) | 0 |
| `magicCrystals` (◇) | 0 |
| `starlight` (⋆) | 0 |
| `starDust` (★) | 0 |
| `starforceShards` (◈) | 0 |
| `summonLevel` | 1 |
| `totalGachaCount` | 0 |
| `firstClearedChapters` | 0 (모든 챕터 잠금) |
| `infiniteModeMaxDepth` | 0 |
| `selectedCharacterId` | "" (빈 문자열 — 강제 선택 트리거) |
| `pendingChapterClearedId` | "" |
| `seasonId / seasonPoints` | 0 / 0 |
| `slotStarforceLevels[12]` | 모두 0 |
| `equipmentInstances[]` | 빈 배열 |
| `discoveredMemoryIds[]` | 빈 배열 |
| `defeatedEnemyIds[]` | 빈 배열 |

## 14. 변경 이력

- 2026-05-04: 초기 작성 (5005 attic_decomposition_v1 직후 — 7 Attic 컴포넌트 흐름 반영)
