# 5002 Boss Scene Separation — PRD 리뷰

## 0. 결정 이력
- 2026-05-04: prd-critiquer 1차 리뷰 (Phase 2.1)

## 1. 종합 점수: 9/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | DTO + IBossEncounterLauncher 구조 우수. ⚠️ BossEncounterRunner 170L 임계 |
| PG-12 톤 | 10/10 | 메커니즘 변경 — 톤 영향 0 |
| 성능 | 8/10 | ⚠️ 씬 전환 1회 추가 — LoadSceneAsync 도입 시점 미정 |
| 시스템 통합 | 8/10 | ⚠️ SaveData `PendingChapterClearedId` 마이그레이션 명시 부족 |
| Sprint 분해 | 10/10 | 8 Sprint × 10 commit 균형 우수, 의존성 그래프 명확, 시나리오 S1~S5 |

## 2. Accept 항목 (8)

- ✅ **옵션 C 채택 (챕터 보스만 분리 + §15 정합)** — 의식적 전환점 + 모바일 UX 단조로움 회피 + Validator 룰 동반
- ✅ **BossEncounterRequest DTO + IBossEncounterLauncher** — §15.7 DI 진입점. 정적 Default property 로 테스트 mock 주입 가능 → Service Locator + Lightweight DI 균형
- ✅ **Sprint 1~2 분리 commit (DTO 신규 → Mode.ChapterBoss 추가)** — 회귀 위험 격리. Daily/Weekly 회귀 없음을 단계마다 검증
- ✅ **BossLootGrantService.Apply 자동 hook 완성** (Sprint 4) — 5001 미완 부분 해결. 챕터 보스 처치 시 일관 보상 흐름
- ✅ **ProjectValidator 룰 추가** (Sprint 7) — `BossEncounterContext.Set` 외부 직접 호출 검사. §15.12 정책 검증 자동화 강화
- ✅ **NUnit 5+ 명시** — Launcher/Context/Service 핵심 정적 service 단위 테스트 (§15.9)
- ✅ **검증 시나리오 5 (S1~S5)** — 정상/실패/Daily 회귀/Weekly 회귀/BG 적용 — 회귀 방지 충분
- ✅ **ChapterRunner 종료 흐름** — wave==totalWaves 진입 시 SaveSession + Launcher.Launch + return — 단일 책임 유지

## 3. Reject 항목 (0)

(없음 — PG-12·SOLID·100줄 룰 모두 통과 또는 임계 내)

## 4. Discuss 항목 (5) 🟡

### D1 — BossEncounterRunner 라인 수 증가 (118L → ~170L)

**관찰**: §15.12 ProjectValidator 는 200L+ 경고. 170L 은 임계 안쪽이지만 §15.6 100줄 룰 기준 초과.

**옵션**:
- **A** ★권장: 그대로 진행 (~170L) — Validator 200L 경계 내, 이미 분리 시도 끝
- **B**: `ApplyChapterBackground` 을 정적 헬퍼 `BackgroundApplier.Apply(SpriteRenderer, ChapterData)` 로 추출 — Runner ~140L 감소. ChapterRunner 의 동일 메서드도 호출 가능 → DRY 개선
- **C**: Mode 분기를 Strategy 패턴 (`IBossModeHandler` 3 구현체) — 과한 추상화. Sprint 3 작업량 60분+ 증가

**권장 사유 (B)**: ChapterRunner.cs 에 `ApplyChapterBackground` (L130~148, 19줄) 이 이미 존재. 정적 헬퍼로 추출 시 양쪽 Runner 가 공유 → DRY 개선 + Runner 라인 분산. Sprint 3 작업량 5분 증가에 그침.

### D2 — ChapterCleared 패널 표시 시점

**관찰**: PRD §6.1 에 "Attic 복귀 직후 1회 toast (`PendingChapterClearedId` 플래그)" 명시. 그러나 SaveData 신규 필드 추가 명시 부족.

**옵션**:
- **A** ★권장: SaveData 에 `string pendingChapterClearedId` 신규 필드 추가 (기본 ""). Sprint 4 에서 `MarkChapterFirstCleared` 호출 시 동시 set, AtticController.Start 1회 표시 후 clear
- **B**: BossEncounter 씬에 ChapterClearedPanel 직접 추가 — 씬 전환 전 표시. SaveData 변경 X
- **C**: BossEncounterRunner 가 Attic 복귀 직전 LoadSceneAsync + ChapterClearedPanel 오버레이

**권장 사유 (A)**: 기존 `firstClearedChapters` 와 같은 SaveData 패턴 답습. 다락방 Attic 가 단일 진실원. SaveData 마이그레이션은 빈 문자열 기본값으로 자동.

### D3 — 미니보스(5/10/15) 씬 분리 후속

**관찰**: 디자인 §2 에 옵션 D (미니보스 포함) 비교. 본 5002 는 옵션 C (챕터 보스만).

**옵션**:
- **A** ★권장: 본 5002 에서 다루지 않음. 디자인 §6 에 "5003 미니보스 씬 분리" 후속 항목으로 명시
- **B**: 5003 후속 시스템 PRD 생성 (3 commit 정도 예상) — 본 5002 완료 후 검토

**권장 사유 (A)**: 5002 는 옵션 C 그대로 — 후속은 사용자 결정 시 별 PRD. 5003 시스템화 시점도 사용자 결정.

### D4 — LoadSceneAsync 도입 시점

**관찰**: PRD §6.1 위험 표 에 "LoadSceneAsync (향후)" — 본 5002 sprint 에서 동기/비동기 결정 필요.

**옵션**:
- **A**: 본 5002 Sprint 3 에서 도입 — 씬 전환 부드러움 (검은 fade-in 0.3s 추가)
- **B** ★권장: 5002 는 sync `SceneManager.LoadScene` 그대로. 후속 최적화 sprint (5004 정도) 에서 LoadSceneAsync + fade-in 일괄 적용
- **C**: Sprint 8 에 LoadSceneAsync 마이그레이션 추가 commit 1 추가

**권장 사유 (B)**: 5002 는 분리 자체가 목표. async + fade 는 별 최적화 (모든 씬 전환에 일관 적용 권장). 5002 commit 10 → 11 증가는 scope creep.

### D5 — Phase 2 텔레그라프·Premium 피격 회귀 검증

**관찰**: PRD §6.1 위험 표 에 "DontDestroyOnLoad 검증" 명시. 그러나 검증 시나리오 S1~S5 에 명시 부족.

**옵션**:
- **A** ★권장: S1 에 "Phase 2 진입 시 텔레그라프·VfxPool.SpawnHitStar 정상 동작" 명시 추가
- **B**: 별 검증 시나리오 S6 추가
- **C**: Sprint 7 NUnit 에 `DontDestroyOnLoad_VfxPool_PersistsAcrossScenes` 추가 — Editor Test 한계 (Play Mode 만 검증 가능)

**권장 사유 (A)**: S1 은 챕터 1 보스 처치 정상 흐름이므로 Phase 2 + Premium 피격을 자연스럽게 검증.

## 5. CLAUDE.md §15 정책 위반 점검

| §15.x | 항목 | 결과 |
|---|---|---|
| §15.1 SRP | Launcher/Runner/Service 책임 분리 | ✅ 통과 |
| §15.1 OCP | switch 3분기 (Mode) | ✅ 통과 (3분기는 허용 범위) |
| §15.1 LSP | IBossEncounterLauncher 구현체 호환 | ✅ 통과 |
| §15.1 ISP | DTO 단일 목적 | ✅ 통과 |
| §15.1 DIP | DTO + interface 진입점 | ✅ 통과 |
| §15.4 SO 우선 | BossLootTable 4 SO 그대로 활용 | ✅ 통과 |
| §15.5 EventBus | GameEvents.RaiseChapterCleared 정적 EventBus | ✅ 통과 |
| §15.6 100줄 | BossEncounterRunner ~170L | 🟡 임계 — D1 검토 |
| §15.7 DI | Launcher.Default + Request DTO | ✅ 통과 |
| §15.9 Test | NUnit 5+ 신규 | ✅ 통과 |
| §15.11 안티 | public 필드 X, Update in N X, Instantiate X | ✅ 통과 |
| §15.12 Validator | BossEncounterContext.Set 외부 직접 호출 룰 신규 | ✅ 통과 (강화) |

## 6. 권장 변경

### 즉시 변경 (rebuttal 자동 수용)

(없음 — Reject 항목 0)

### 사용자 결정 후 변경 (mediator)

- **D1**: ApplyChapterBackground 정적 헬퍼 추출 여부 (옵션 A/B/C)
- **D2**: ChapterCleared 표시 시점 (옵션 A/B/C)
- **D3**: 미니보스 씬 분리 후속 시스템화 (옵션 A/B)
- **D4**: LoadSceneAsync 본 5002 도입 여부 (옵션 A/B/C)
- **D5**: Phase 2 텔레그라프 회귀 검증 위치 (옵션 A/B/C)

## 7. 변경 이력

- 2026-05-04: 초기 리뷰 작성 (Phase 2.1) — 종합 9/10 / Accept 8 / Reject 0 / Discuss 5
