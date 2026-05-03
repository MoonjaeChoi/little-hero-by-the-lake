# 5002 Boss Scene Separation — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.
> 입력: `5002_*_critique.md` (Discuss 5건) → rebuttal 자동 수용 2건 제외 → 본 문서 3건.

## High Priority Items

(없음)

## Medium Priority Items

### D2 — ChapterCleared 패널 표시 시점

- **Original Criticism**: PRD v1 §6.1 위험 표 에 "Attic 복귀 후 1회 toast (`PendingChapterClearedId` 플래그)" 명시. 그러나 SaveData 신규 필드 추가 명시 부족 + 표시 위치 옵션 결정 필요.

- **Current Position (PRD v1)**: 모호. SaveData `pendingChapterClearedId` 신규 필드 미명시. ChapterClearedPanel 표시 책임 불분명.

- **Discussion Points**:
  - **옵션 A** ★권장: Attic 복귀 후 1회 toast (`SaveData.pendingChapterClearedId` 신규 필드).
    - 장점: 단일 진실원 (Attic). 기존 `firstClearedChapters` 패턴 답습. SaveData 마이그레이션 빈 문자열 자동.
    - 단점: SaveData 필드 +1 (마이그레이션 1줄). 1프레임 지연 가능.
  - **옵션 B**: BossEncounter 씬에 ChapterClearedPanel 직접 추가 — 씬 전환 전 표시.
    - 장점: SaveData 변경 X. 즉시 표시.
    - 단점: BossEncounter 씬에 ChapterClearedPanel UI prefab 추가 wiring 필요. 보스 씬 책임 늘어남 (§15.1 SRP 약화).
  - **옵션 C**: BossEncounterRunner 가 LoadSceneAsync + ChapterClearedPanel 오버레이 후 Attic 진입.
    - 장점: 자연스러운 UX 흐름.
    - 단점: 옵션 D4 (LoadSceneAsync) 와 결합. async 미도입 시 적용 불가.

- **Proposed Resolution Path** (Claude 권장): **A** — SaveData 단일 진실원, §15 정합, D4 결정과 무관.

- **PRD Impact**:
  - 옵션 A 채택 시: Sprint 4 (CompleteChapterBoss) 에서 `data.pendingChapterClearedId = chapter.id; SaveSystem.Save(data);` 추가. AtticController.Start 에 `HandleReturnFromBossEncounter` 다음으로 `HandlePendingChapterCleared` 1회 표시 + clear 호출. SaveData (MetaProgressionData) 필드 1 추가 + 마이그레이션 자동 (Newtonsoft 기본값).
  - 옵션 B 채택 시: BossEncounter 씬 wiring 변경 (Sprint 3.2 작업량 +10분). ChapterClearedPanel prefab 인스턴스 추가.
  - 옵션 C 채택 시: D4 옵션 A (5002 도입) 와 결합 필수.

---

### D3 — 미니보스(5/10/15) 씬 분리 후속

- **Original Criticism**: 디자인 §2 옵션 D (미니보스 포함 분리) 비교 후 옵션 C 채택. 후속 시스템화 결정 필요.

- **Current Position (PRD v1)**: 디자인 §6 위험 표 에 명시 X. 5002 스코프 외 후속 작업 의미 모호.

- **Discussion Points**:
  - **옵션 A** ★권장: 본 5002 에서 다루지 않음. 디자인 §6 에 "5003 미니보스 씬 분리 (사용자 결정 시)" 후속 항목으로 명시. 5002 종료 후 사용자 검토.
    - 장점: 5002 스코프 명확. 미니보스 분리는 UX 단조로움 위험 → 사용자 검토 단계 필요.
    - 단점: 5003 시스템화 시점 모호.
  - **옵션 B**: 5003 후속 PRD 즉시 생성 (3 commit 정도 예상) — 본 5002 완료 후 자동 진행.
    - 장점: 미니보스 분리 검토 자동화.
    - 단점: 사용자가 미니보스 분리 원하지 않을 수 있음 → 헛 PRD.

- **Proposed Resolution Path** (Claude 권장): **A** — 5002 종료 후 사용자 결정.

- **PRD Impact**:
  - 옵션 A 채택 시: 디자인 §6 에 후속 항목 1줄 추가. PRD Sprint·commit 변동 X.
  - 옵션 B 채택 시: Sprint 8 docs 작업에 5003 PRD 생성 추가. 또는 별 사이클.

---

### D4 — LoadSceneAsync 본 5002 도입 여부

- **Original Criticism**: PRD v1 §6.1 위험 표 에 "LoadSceneAsync (향후)" — 본 5002 도입 여부 미결.

- **Current Position (PRD v1)**: 모든 씬 전환 sync `SceneManager.LoadScene`. 씬 전환 1회 추가 (~1초) 위험 명시.

- **Discussion Points**:
  - **옵션 A**: 본 5002 Sprint 3 에서 LoadSceneAsync + 검은 fade-in 0.3s 도입.
    - 장점: 5002 부터 부드러운 UX. 모바일 60fps drop 위험 감소.
    - 단점: Sprint 3 작업량 +20분. 모든 씬 전환 (Chapter→Boss→Attic) 일관 적용 필요 → 4곳 변경.
  - **옵션 B** ★권장: 5002 는 sync 그대로. 후속 최적화 sprint (5004 정도) 에서 LoadSceneAsync + fade-in 모든 씬 전환 일괄 적용.
    - 장점: 5002 스코프 = 분리 자체. async + fade 는 별 최적화 (Chapter↔Attic, Attic↔InfiniteTower 등 모든 전환에 일관 적용 권장).
    - 단점: 5002 사이에 sync 전환 1회 (~1초) UX 부담.
  - **옵션 C**: Sprint 8 에 LoadSceneAsync 마이그레이션 1 commit 추가 (5002 내 9 commit + 1).
    - 장점: 5002 안에 통합. 후속 sprint 부담 X.
    - 단점: scope creep — 본 5002 = 분리 자체.

- **Proposed Resolution Path** (Claude 권장): **B** — 5002 스코프 보존, 별 최적화 sprint.

- **PRD Impact**:
  - 옵션 A 채택 시: Sprint 3 작업량 +20분 (40 → 60분). BossEncounterRunner.LoadSceneAfterDelay → async coroutine. EndgameMenuPanel / AtticController / ChapterRunner 도 동시 수정 → +1 commit.
  - 옵션 B 채택 시: PRD §6.1 위험 표 에 "후속 5004 sprint" 명시. 5002 sprint·commit 변동 X.
  - 옵션 C 채택 시: Sprint 8 에 commit 1 추가 (5002 = 11 commit).

## Low Priority Items

(자동 수용으로 0건)

## 요약

- **Total Items**: 3 (D2 / D3 / D4)
- **High**: 0 / **Medium**: 3 / **Low**: 0
- **권장 mode**: ai-assisted (Claude 추천 + 사용자 확인)
- **Claude 권장 일괄 채택 시**: D2=A, D3=A, D4=B
- **사용자 결정 입력 형식 예**: `D2=A, D3=A, D4=B` 또는 `권장 그대로`
