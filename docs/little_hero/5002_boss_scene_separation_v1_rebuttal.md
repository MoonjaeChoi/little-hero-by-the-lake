# 5002 Boss Scene Separation — PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-04
> 입력: `5002_boss_scene_separation_v1_critique.md` (9/10, Accept 8 / Reject 0 / Discuss 5)

## 1. 요약 표

| ID | Category | Criticism Summary | Priority | Status | §15 Aligned |
|---|---|---|---|---|---|
| D1 | OOP §15.6 100줄 | BossEncounterRunner 170L 임계 | Medium | **Accepted** (옵션 B 자동) | ✅ |
| D2 | UX 흐름 | ChapterCleared 표시 시점 (3 옵션) | Medium | **Discuss** | △ |
| D3 | 스코프 | 미니보스(5/10/15) 씬 분리 후속 | Low | **Discuss** | △ |
| D4 | 성능/UX | LoadSceneAsync 본 5002 도입 여부 | Medium | **Discuss** | △ |
| D5 | 검증 회귀 | Phase 2 텔레그라프·Premium 피격 회귀 검증 위치 | Low | **Accepted** (옵션 A 자동) | ✅ |

## 2. 품질 메트릭

- 총 Discussion Points: 5
- **Accepted**: 2 (40%) — D1, D5 (§15 강화 + 검증 보강)
- **Discuss**: 3 (60%) — D2, D3, D4 (사용자 결정 사안)
- **Rejected**: 0
- §15 정합률: 100% (위반 0, 임계 1건은 자동 수용으로 해소)

## 3. Detailed Responses

### 3.1 D1 — BossEncounterRunner 170L 임계 (Medium / **Accepted 옵션 B**)

**Original Criticism**: §15.6 100줄 룰 기준, BossEncounterRunner 가 118L → ~170L 로 증가. ApplyChapterBackground 19줄이 추가되는 구조.

**Rebuttal**: **자동 수용 (옵션 B)**.
- 근거: §15.1 SRP — `ApplyChapterBackground` 은 "보스 전투 흐름" 책임이 아니라 "챕터 BG 적용" 책임. ChapterRunner.cs L130~148 에 동일 메서드가 이미 존재 → DRY 위반.
- §15.6 100줄 룰은 절대치가 아니나, 정적 헬퍼 추출이 SRP+DRY 둘 다 개선 → 비용 5분, 이익 명확.

**Proposed Action**:
- 신규 `Scripts/Core/BackgroundApplier.cs` (정적 헬퍼, ~25L):
  ```csharp
  public static class BackgroundApplier
  {
      public static void Apply(SpriteRenderer renderer, ChapterData chapter)
      {
          if (renderer == null || chapter?.coverArt == null) return;
          renderer.sprite = chapter.coverArt;
          renderer.sortingOrder = -100;
          renderer.drawMode = SpriteDrawMode.Tiled;
          renderer.tileMode = SpriteTileMode.Continuous;
          renderer.size = new Vector2(300f, 300f);
          renderer.transform.position = Vector3.zero;
          renderer.color = Color.white;
      }
  }
  ```
- BossEncounterRunner: `BackgroundApplier.Apply(_backgroundRenderer, chapter)` 단일 호출. 자체 메서드 제거.
- ChapterRunner: 기존 `ApplyChapterBackground` (L130~148) 도 같은 헬퍼 호출로 통합.

**PRD Impact (v2)**:
- Sprint 3.1 작업량 +5분 (40 → 45분)
- 신규 파일 1: `BackgroundApplier.cs`
- BossEncounterRunner 라인 수 ~170 → ~145 (예상 25L 감소)
- Sprint 5 ChapterRunner 변경에 BG 헬퍼 통합 1줄 추가
- 검증: ChapterRunner 의 BG 변경 회귀 X (시각적 동등성)

**Little Hero Context**: §15.4 SO 우선 변경 X (코드 리팩토링만). 기존 Tiled BG 패턴 보존.

---

### 3.2 D5 — Phase 2 회귀 검증 위치 (Low / **Accepted 옵션 A**)

**Original Criticism**: PRD §6.1 위험 표 에 "DontDestroyOnLoad 검증" 명시되었으나 검증 시나리오 S1~S5 에 Phase 2 텔레그라프·Premium 피격 회귀 검증 항목 부재.

**Rebuttal**: **자동 수용 (옵션 A)** — 단순 문서 보강.
- 근거: 검증 강화는 PRD 품질 문제. 추가 commit 없이 PRD 텍스트 보강만 필요.

**Proposed Action**: PRD §S1 (챕터 1 보스 처치) 검증 항목에 추가:
- "Phase 2 진입 (HP 50%) 시 텔레그라프 0.7s + 1.3× 속도 정상"
- "Critical 적중 시 slow-mo 0.08s + screen shake 정상"
- "VfxPool.SpawnHitStar / DamageFloater pop 정상"
- "DontDestroyOnLoad: GameManager / AudioManager / VfxPool 객체 BossEncounter 씬에서 참조 가능"

**PRD Impact (v2)**: §S1 검증 4 줄 추가. Sprint·commit 변동 없음.

---

### 3.3 D2 — ChapterCleared 표시 시점 (Medium / **Discuss**)

**Status**: discussion_topics.md 로 이동.

**이유**: 디자인 의도 결정 (사용자 권한). 옵션 A/B/C 모두 §15 정합. UX 차이만.

**Current Position (PRD v1)**: 모호 — "Attic 진입 후 1회 toast" 만 명시, SaveData 필드 추가 명시 부족.

**Resolution Path**: mediator 단계 사용자 결정.

---

### 3.4 D3 — 미니보스 씬 분리 후속 (Low / **Discuss**)

**Status**: discussion_topics.md 로 이동.

**이유**: 스코프 결정 (5002 vs 5003). 사용자 권한.

**Current Position (PRD v1)**: 디자인 §2 옵션 D (미니보스 포함) 비교 후 옵션 C 채택. 후속 시스템화 명시 부족.

**Resolution Path**: mediator 단계 사용자 결정.

---

### 3.5 D4 — LoadSceneAsync 도입 시점 (Medium / **Discuss**)

**Status**: discussion_topics.md 로 이동.

**이유**: 본 5002 스코프 결정. async + fade 는 모든 씬 전환에 일관 적용 권장 → 별 5004 최적화 sprint 도 정합.

**Current Position (PRD v1)**: 위험 표 에 "(향후)" 만 명시, 본 sprint 도입 여부 미결.

**Resolution Path**: mediator 단계 사용자 결정.

## 4. PRD v2 적용 변경 요약

수용 항목만 v2_prd.md 에 반영. 미결 항목 (D2/D3/D4) 은 v2 미반영, mediator 후 최종.

| ID | 변경 위치 | 변경 |
|---|---|---|
| D1 | §3.1 신규 파일 명세 | `Scripts/Core/BackgroundApplier.cs` 추가 (25L) |
| D1 | §3.5 BossEncounterRunner 분기 | `ApplyChapterBackground` → `BackgroundApplier.Apply` 위임 |
| D1 | Sprint 3.1 산출물 | 정적 헬퍼 호출로 변경 + 작업량 +5분 |
| D1 | Sprint 5 ChapterRunner 변경 | 기존 `ApplyChapterBackground` → `BackgroundApplier.Apply` 통합 |
| D5 | §S1 검증 시나리오 | Phase 2 텔레그라프·Premium 피격·DontDestroyOnLoad 4 줄 추가 |

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal). Accept 2 / Discuss 3 / Reject 0
