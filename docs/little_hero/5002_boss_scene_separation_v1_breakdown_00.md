# 5002 Boss Scene Separation Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 (mediator 확정) → 구현 태스크 분해
> 일시: 2026-05-04 · 총 10 태스크 / 10 commit / 약 3시간 35분
> 입력: `5002_boss_scene_separation_v1_v2_prd.md` (D1+D5 자동 수용 + D2/D3/D4 mediator 결정 반영)

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | BossEncounterRequest DTO 신규 | Logic (Core) | - | 15분 | _01_BossEncounterRequest_DTO.md |
| T02 | IBossEncounterLauncher + 정적 BossEncounterLauncher | Logic (Core) | T01 | 15분 | _02_BossEncounterLauncher.md |
| T03 | BossEncounterContext.Mode.ChapterBoss + 필드 2개 | Logic (Core) | T01, T02 | 15분 | _03_BossEncounterContext_extension.md |
| T04 | BackgroundApplier 정적 헬퍼 + BossEncounterRunner ChapterBoss 분기 | Logic (Core+Waves) | T03 | 35분 | _04_BackgroundApplier_Runner_branch.md |
| T05 | BossEncounter.unity Background SpriteRenderer wiring | Editor (Scene) | T04 | 10분 | _05_BossEncounter_scene_wiring.md |
| T06 | CompleteChapterBoss + SaveData pendingChapterClearedId + AtticController toast (D2) | Logic (Core+Hub+Meta) | T03 | 30분 | _06_CompleteChapterBoss_SaveData_Attic.md |
| T07 | ChapterRunner — wave==totalWaves Launcher 전환 + BackgroundApplier 통합 (D1) | Logic (Waves) | T02, T04 | 30분 | _07_ChapterRunner_transition.md |
| T08 | EndgameMenuPanel Launcher 위임 (Daily/Weekly 정적 Context.Set 제거) | UI | T02 | 20분 | _08_EndgameMenuPanel_launcher.md |
| T09 | BossSceneSeparationTests NUnit 5+ + ProjectValidator 룰 | Test | T01~T08 | 30분 | _09_Tests_Validator.md |
| T10 | docs 통합 — changelog_v1.md + CLAUDE.md §12/§13/§14 | Docs | T01~T09 | 15분 | _10_docs_changelog_CLAUDE.md |

## 2. 의존성 그래프

```
T01 (Request DTO) ───┐
                     ├─→ T03 (Context Mode/필드) ──┬─→ T04 (BackgroundApplier+Runner) ──┬─→ T05 (Scene wiring)
T02 (Launcher) ──────┤                            │                                    ├─→ T07 (ChapterRunner)
                     │                            └─→ T06 (Service+SaveData+Attic)     │
                     │                                                                  │
                     └─→ T08 (EndgameMenuPanel) ───────────────────────────────────────┤
                                                                                       │
                                                                                       └─→ T09 (Test + Validator) → T10 (docs)
```

## 3. 병렬 가능성

- **그룹 1 (인프라, 30분)**: T01 → T02 (T01 의존, 1 사이클 내)
- **그룹 2 (Context 확장, 15분)**: T03 (T01+T02 의존)
- **그룹 3 (Runner+Service+ChapterRunner+Endgame, 병렬 가능)**:
  - T04 + T05 (씬 wiring) — 직렬
  - T06 (Service) — T03 의존, T04/T05 와 병렬 가능
  - T07 (ChapterRunner) — T02+T04 의존
  - T08 (EndgameMenuPanel) — T02 의존, T03 와 병렬 가능
- **그룹 4 (검증·문서, 직렬)**: T09 → T10

단일 작업자: 순차 진행 권장 (~3시간 35분)

## 4. 구현 순서 권장 (단일 작업자)

```
Step 1 (15분):  T01 BossEncounterRequest DTO
Step 2 (15분):  T02 BossEncounterLauncher
Step 3 (15분):  T03 Context.Mode.ChapterBoss + PendingChapter/PendingReturnScene
Step 4 (35분):  T04 BackgroundApplier + BossEncounterRunner 분기  ← MCP refresh 후 5~10s 대기
Step 5 (10분):  T05 BossEncounter.unity 씬 wiring                ← 씬 저장 후 10s+ 대기
Step 6 (30분):  T06 CompleteChapterBoss + SaveData + AtticController
Step 7 (30분):  T07 ChapterRunner wave==totalWaves Launcher 전환
Step 8 (20분):  T08 EndgameMenuPanel Launcher 위임
Step 9 (30분):  T09 NUnit 5+ + ProjectValidator 룰
Step 10 (15분): T10 changelog + CLAUDE.md §12/§13/§14

총: 약 3시간 35분
```

## 5. CLAUDE.md §15 정합 자동 검증

| 태스크 | §15 정합 |
|---|---|
| T01 BossEncounterRequest | §15.4 SO 우선 (struct DTO 예외) ✅ / §15.11 public 필드 (DTO struct 예외) ✅ |
| T02 BossEncounterLauncher | §15.1 SOLID-D (interface 진입점) ✅ / §15.7 DI (Default property) ✅ |
| T03 Context 확장 | 정적 전역 — 기존 호환성 유지. 외부 직접 R/W 는 T09 Validator 룰로 차단 |
| T04 BackgroundApplier + Runner | §15.1 SRP (BG 단일 책임) ✅ / §15.6 100줄 (Runner ~145L) ✅ / DRY |
| T05 Scene wiring | Editor 작업 — 코드 정합 영향 X |
| T06 CompleteChapterBoss + SaveData + Attic | §15.1 SRP (Service 단일 메서드) ✅ / §15.5 EventBus (RaiseChapterCleared) ✅ |
| T07 ChapterRunner 전환 | §15.6 100줄 (~189 → ~175L) ✅ / DRY (BackgroundApplier 통합) |
| T08 EndgameMenuPanel | §15.7 DI 진입점 정리 (정적 Context 직접 R/W 제거) ✅ / §15.6 (174 → ~150L) ✅ |
| T09 Test + Validator | §15.9 단위 테스트 (5+) ✅ / §15.12 Validator 룰 신규 ✅ |
| T10 docs | 정합 영향 X |

## 6. Unity MCP 안전 전략

| 태스크 | 위험 | 완화 |
|---|---|---|
| T01~T03 | 낮음 (코드만) | 각 commit 후 refresh + 5초 대기 |
| **T04** | **높음** (다중 파일 변경) | refresh + 10초 대기 + read_console errors 확인 |
| **T05** | **높음** (씬 변경) | 씬 저장 후 10초+ 대기 + Inspector wiring 확인 |
| T06 | 중간 (다중 파일) | refresh + 10초 대기 |
| T07 | 중간 (ChapterRunner) | refresh + 10초 대기 |
| T08 | 낮음 | refresh + 5초 대기 |
| T09 | 낮음 (Test) | Test Runner 실행 후 PASS 확인 |
| T10 | 낮음 (docs) | 검증 불필요 |

전체 사이클 동안 SIGSEGV 회피 — 한 commit 단위로 분리.

## 7. 검증 체크리스트 (Phase 6 Contract 입력)

- C1 컴파일: 모든 commit 후 errors=0
- C2 Editor Test: 46 → 51+ (BossSceneSeparation 5+)
- C3 Validator: warnings 변동 0 또는 감소 (BossEncounterContext.Set 외부 호출 룰 신규)
- C4 Reflection:
  - Type 존재: BossEncounterRequest / IBossEncounterLauncher / BackgroundApplier
  - Method 존재: CompleteChapterBoss / Launch / Apply / HandlePendingChapterCleared
  - Enum: BossEncounterContext.Mode.ChapterBoss
- C5 SO 검증: SaveData 마이그레이션 자동 — pendingChapterClearedId 빈 문자열
- C6 Play 시나리오: S1~S5 (PRD §검증 시나리오)

## 8. 변경 이력

- 2026-05-04: 초기 작성 (Phase 3 Breakdown). 10 태스크 분해.
