# Sprint Contract: 5003 Mini Boss Scene Separation

> **PGE Planner — Phase 4 Contract**
> PRD: `5003_*_v2_prd.md` (재진입, 옵션 B 채택)
> Breakdown: `5003_*_breakdown_00.md` (5 태스크)
> 생성일: 2026-05-04

## 1. C1 컴파일 기준

- **C1.1**: 5 commit 모두 errors=0
- **C1.2**: 도메인 리로드 5~10초 내 안정화

**Pass/Fail**: ✅ 모든 commit errors=0 → PASS / ❌ 1개 commit 이라도 errors>0 → FAIL

## 2. C2 Editor Test 기준 (NUnit)

| Test 클래스 | 신규 메서드 | 위치 |
|---|---|---|
| `BossSceneSeparationTests` (확장) | 2 | `Tests/Editor/BossSceneSeparationTests.cs` |

**신규 NUnit (2)**:
- `Mode_Enum_Has_MiniBoss` — Mode.MiniBoss enum 값 존재
- `CompleteMiniBoss_Null_NoOp` — null 인자 → 예외 없이 return

**검증 방법**: Test Runner Edit Mode > Run All → 5002 8/8 + 5003 신규 2 = 10/10 PASS

**Pass/Fail**: ✅ 신규 2 PASS + 기존 8 회귀 X → PASS

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0
- **C3.2**: warnings 변동 ±0 (5002 룰 그대로)
- **C3.3**: 신규 룰 없음 (기존 BossEncounterContext.Set 외부 호출 검사 — `MiniBoss` 모드 추가도 같은 룰 통과)
- **C3.4**: BossEncounterRunner ~165L (200L 경계 내)
- **C3.5**: ChapterRunner ~195L (200L 임계 — 모니터링)

## 4. C4 Reflection 검증 (정적)

| Type | Member |
|---|---|
| `LittleHero.MiniBossSeparationMode` (enum) | `None / LastOnly / All` 3 값 |
| `LittleHero.BossEncounterContext.Mode` (enum) | `Daily / Weekly / ChapterBoss / MiniBoss` 4 값 |
| `LittleHero.BalanceConfig` | `miniBossSeparation` (MiniBossSeparationMode) 필드 |
| `LittleHero.BossEncounterService` | `CompleteMiniBoss(EnemyData, ChapterData)` 정적 메서드 |

## 5. C5 SO + SaveData 검증

- **C5.1**: BalanceConfig SO 마이그레이션 — `miniBossSeparation` 신규 필드 default `None` (Unity Serialization 자동)
- **C5.2**: 옵션 B 채택 시 사용자가 BalanceConfig Inspector 에서 `LastOnly` 로 변경 (테스트용)
- **C5.3**: 5002 SaveData (`pendingChapterClearedId`) 회귀 X
- **C5.4**: 기존 EnemyData / BossLootTable 회귀 X

## 6. C6 Play 검증 시나리오

### S1 — 옵션 B 정상 흐름 (BalanceConfig.miniBossSeparation = LastOnly)

1. 챕터 1 wave 1~14 진행 (미니보스 5/10 챕터 씬 정상)
2. wave 15 직전 BossEncounter 씬 전환 + 챕터 1 BG 적용
3. 미니보스 단독 spawn → 처치
4. Chapter 씬 복귀 → wave 16~19 진행
5. wave 20 → 챕터 보스 (5002 흐름)

**검증**:
- 콘솔 `[BossEncounter] 시작 — type=MiniBoss boss=...`
- 콘솔 `[BossEncounterService] CompleteMiniBoss: ...`
- 콘솔 `[BossLootGrantService] Applied MiniBoss (1 reward)`
- StarFragments / StarDust 증가 (firstClearedChapters 변동 X)
- Chapter 씬 wave 16 정상 진행 (activeChapterWave 재개)

### S2 — 옵션 A 토글 (separateAllMiniBosses = All)

1. BalanceConfig.miniBossSeparation = All 설정
2. 챕터 1 → wave 5/10/15 모두 BossEncounter 씬 전환
3. **검증**: 4 transitions (5/10/15 + 보스 20)

### S3 — 옵션 None 회귀 (분리 X, 5002 종료 상태)

1. BalanceConfig.miniBossSeparation = None 설정
2. 챕터 1 wave 5/10/15 챕터 씬 그대로 (5002 흐름)
3. **검증**: 분리 X — 5002 종료 상태 회귀 검증

### S4 — 5002 회귀 (Daily/Weekly/ChapterBoss)

1. Endgame 일일/주간 + 챕터 보스 (wave 20)
2. **검증**: 5002 흐름 회귀 X

## 7. 비기능 요구사항

- [ ] 컴파일 errors=0 (5 commit)
- [ ] NUnit 신규 2 PASS + 기존 8 회귀 X (총 10)
- [ ] Validator errors=0 / warnings 변동 0
- [ ] 5002 회귀 X (S4)
- [ ] 60fps 유지 (씬 전환 1회/챕터 — 부담 거의 없음)

## 8. 합격 기준

**최소**: 자동 (C1~C5) + S1, S4 PASS
**완전**: 자동 (C1~C5) + S1~S4 모두 PASS

## 9. Trade-off (mediator 결정 기반)

- **D1 = B 재결정**: RESERVED 영구 해제. 보스 빌드업 효과 채택. 씬 전환 1회/챕터 추가 (~1초).
- **D2 = A**: §11 마일스톤 등재 (Sprint 5).
- **D3 = Accept (자동)**: 단일 enum `MiniBossSeparationMode` 통합 — 잘못된 조합 회피.

## 10. 변경 이력

- 2026-05-04: 초기 작성 (Phase 4 Contract 재진입).
