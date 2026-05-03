# 5003 Mini Boss Scene Separation — PRD 리뷰

## 0. 결정 이력
- 2026-05-04: prd-critiquer 1차 리뷰 (Phase 2.1)
- 입력: 5003_*_v1.md (디자인) + 5003_*_v1_prd.md (옵션 B 채택 5 Sprint × 5 commit)

## 1. 종합 점수: 8/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 10/10 | 5002 인프라 재사용. 신규 클래스 0. switch 4분기 허용 범위 |
| PG-12 톤 | 10/10 | 메커니즘 변경 — 톤 영향 0 |
| 성능 | 8/10 | ⚠️ 씬 전환 1회/챕터 추가 (옵션 A 시 3회 추가) |
| 시스템 통합 | 9/10 | 5002 회귀 검증 명시. SaveData 변경 X |
| Sprint 분해 | 8/10 | ⚠️ Sprint 분해는 균형. 그러나 **권장 옵션 (E 분리 X) 과 본 PRD 채택 옵션 (B) 의 불일치** — 사용자 결정 재확인 필요 |

## 2. Accept 항목 (6)

- ✅ **5002 인프라 최대 재사용** — Launcher / Context / Runner / BackgroundApplier 그대로. 신규 클래스 0, 기존 변경 최소.
- ✅ **BalanceConfig 토글 (separateLastMiniBoss / separateAllMiniBosses)** — 디자이너 후속 조정 가능 (§15.4 SO 우선 + Magic Number 회피).
- ✅ **Chapter 씬 복귀 + wave 재개 흐름** — 5002 SaveSession + activeChapterWave 패턴 답습. 미니보스 처치 후 Attic X.
- ✅ **CompleteMiniBoss 와 CompleteChapterBoss 의 차이 명시** — RaiseChapterCleared 호출 X / pendingChapterClearedId set X. 챕터 클리어 아님.
- ✅ **NUnit 8→10 (Mode_Enum_Has_MiniBoss + CompleteMiniBoss_Null_NoOp)** — 5002 패턴 답습 (§15.9).
- ✅ **5 Sprint 분해 균형** — 각 1 commit / 15~25분. 의존성 그래프 명확.

## 3. Reject 항목 (0)

(없음)

## 4. Discuss 항목 (3) 🟡

### D1 — 옵션 채택 재확인 (디자인 권장 vs PRD 가정 불일치)

**관찰**: 디자인 §2 권장 = **옵션 E (분리 X)** — UX 단조로움 회피. 그러나 본 PRD 는 **옵션 B (wave 15만 분리)** 가정으로 작성.

**옵션**:
- **A** ★권장 (디자인 권장 답습): 옵션 E 채택 → **본 PRD 폐기 또는 RESERVED 영구 보존**. 5002 종료 상태 그대로.
- **B**: 옵션 B 채택 (현 PRD 그대로 진행) — 보스 빌드업 효과. 씬 전환 1회/챕터 추가.
- **C**: 옵션 A 채택 (모든 미니보스 분리) — Sprint 3 의 `wave==15` → `wave % 5 == 0` 변경. 4 transitions/챕터.
- **D**: 옵션 D 채택 (BossEncounter 재사용 + 일반 적 같이 spawn) — 미니보스 spawn 흐름 차이 ↓. 씬 분리 의미 약화.

**권장 사유 (A=옵션 E)**: 디자인 §2 자체 권장. 5002 의도 (미니보스 = 양념) 보존. UX 단조로움 회피. PRD 작성은 "예약" 단계 — 사용자가 진행 명시했으므로 옵션 변경 가능.

**Critical**: 사용자가 `/prd-pipeline --prd 5003` 호출 → 진행 의도 명확. 그러나 옵션 채택은 mediator 단계에서 명시 결정 필요.

### D2 — Sprint 5 의 changelog/CLAUDE.md 갱신 시점

**관찰**: 본 PRD 는 5 Sprint 1시간 40분으로 매우 작은 변경. CLAUDE.md §11 마일스톤 등재 가치 검토 필요.

**옵션**:
- **A** ★권장: §11 마일스톤 행 추가 (5001/5002 패턴 답습) — 일관성 우선
- **B**: §11 마일스톤 미등재 (작은 변경) — §14 회전만
- **C**: 옵션 E 채택 시 본 Sprint 5 자체 폐기

**권장 사유 (A)**: 일관성. 5002 도 작은 시스템이지만 §11 등재.

### D3 — 옵션 토글 시 회귀 위험 (BalanceConfig)

**관찰**: `separateLastMiniBoss` / `separateAllMiniBosses` 두 토글 — 잘못된 조합 시 회귀 위험.

**옵션**:
- **A** ★권장: 단일 enum `MiniBossSeparationMode { None, LastOnly, All }` 로 통합 — 잘못된 조합 X
- **B**: 두 boolean 그대로 + ChapterRunner 에서 우선순위 `separateAllMiniBosses` > `separateLastMiniBoss` 명시
- **C**: 단일 boolean `separateMiniBosses` + 별 enum `which {Last, All}` 분리

**권장 사유 (A)**: 단일 진실원, 잘못된 조합 불가능. 5002 의 enum 패턴 답습 (Mode.ChapterBoss 등).

## 5. CLAUDE.md §15 정책 위반 점검

| §15.x | 항목 | 결과 |
|---|---|---|
| §15.1 SOLID-S | BossEncounterContext.Mode 단일 책임 | ✅ |
| §15.1 SOLID-O | switch 4분기 (Daily/Weekly/ChapterBoss/MiniBoss) | ✅ 허용 범위 |
| §15.4 SO 우선 | BalanceConfig 토글 (Magic Number 회피) | ✅ (D3 옵션 A 채택 시 강화) |
| §15.6 100줄 | BossEncounterRunner 145L → ~165L (200L 경계 내) | ✅ |
| §15.7 DI | 기존 Launcher 재사용 | ✅ |
| §15.9 Test | NUnit 2개 추가 (8→10) | ✅ |
| §15.11 안티 패턴 | public 필드 X, Update X | ✅ |
| §15.12 Validator | 기존 룰 그대로 | ✅ |

## 6. 권장 변경

### 즉시 변경 (rebuttal 자동 수용)

- D3 = A: 단일 enum `MiniBossSeparationMode { None, LastOnly, All }` 로 통합 (§15 정합 강화)

### 사용자 결정 후 변경 (mediator)

- D1: **옵션 채택 재확인** (E=분리 X / B=wave 15 / A=모두 / D=BossEncounter 재사용)
- D2: §11 마일스톤 등재 여부 (옵션 E 채택 시 폐기)

## 7. 변경 이력

- 2026-05-04: 초기 리뷰 (Phase 2.1) — 종합 8/10 / Accept 6 / Reject 0 / Discuss 3
- **핵심**: 디자인 권장 (E) vs PRD 채택 (B) 의 불일치 → mediator 단계 사용자 결정 필요
