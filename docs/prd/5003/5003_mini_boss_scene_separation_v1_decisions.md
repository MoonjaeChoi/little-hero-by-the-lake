# 5003 Mini Boss Scene Separation — Mediator Decisions

> **Phase 2.3 Mediator 산출** — discussion_topics 의 2 미결 항목 결정.
> **모드**: ai-assisted (Claude 권장 일괄 채택)
> **사용자 결정**: "권장안대로" (2026-05-04)
> 결정: **D1=E, D2=C** → 5003 PRD 폐기 (RESERVED 영구 보존), 5002 종료 상태 유지.

## 결정 표

| ID | 토픽 | 결정 | 모드 | 결과 |
|---|---|---|---|---|
| **D1** | 디자인 옵션 채택 | **E (분리 X)** — 디자인 §2 권장 답습 | ai-assisted (권장 채택) | **PRD 폐기 / RESERVED 영구 보존** |
| **D2** | §11 마일스톤 등재 | **C (자동 폐기)** — D1 결과 따름 | ai-assisted (권장 채택) | Sprint 5 자체 미실행 |

## 결정 상세

### D1 = E — 분리 X (디자인 §2 권장 답습)

**채택 이유**:
- 디자인 §2 자체 권장 (UX 단조로움 회피)
- 5002 의도 보존 — 미니보스 = "챕터 진행 중 양념"
- 사용자 권장안 일괄 채택 의지
- 보스 빌드업 효과는 옵션 B 의 가치이지만 — **디자인 권장이 더 우선** (UX 안정 + 구현 비용 0)

**결과**:
- Phase 3 Breakdown / Phase 4 Contract / Phase 5 Implement / Phase 6 Evaluate **모두 진행 X**
- 5003 디자인 + PRD + critique + rebuttal + discussion_topics + decisions + v2_prd 산출 **그대로 보존** (RESERVED 영구)
- 5002 종료 상태 유지 — 미니보스 5/10/15 모두 챕터 씬에서 일반 적과 함께 spawn

**미래 재진입 조건**: 사용자가 향후 옵션 B/A/D 채택 의사를 명시할 때 본 PRD 재활성화 가능. 단, **본 PRD 의 옵션 가정은 B 였음** — 옵션 변경 시 Sprint 분해 재작성 필요.

### D2 = C — 자동 폐기 (D1 결과 따름)

**채택 이유**: D1=E 결정으로 Sprint 5 자체 미실행 → §11 마일스톤 등재 토픽 자동 폐기.

**결과**: CLAUDE.md §11 마일스톤 / §12 시스템 / §13 후속 / §14 회전 변동 X.

## v2_prd 최종 상태

`5003_*_v2_prd.md` 는 **RESERVED 영구** 로 변경 — 디자인 권장 (E) 채택 명시. Sprint 1~5 명세 보존 (미래 옵션 변경 시 활용 가능).

## 후속 사용자 결정 시 재진입

```
사용자 발화: "5003 진행 (옵션 B)"  → /prd-pipeline 5003 --from breakdown
                                    → Phase 3 부터 진행 (Sprint 1~5 활용)
사용자 발화: "5003 옵션 A 변경"   → PRD v3 작성 (Sprint 분해 재작성)
사용자 발화: "5003 폐기"           → docs/prd/5003/ 삭제 (또는 _archive 이동)
```

## 재진입 결정 (2026-05-04)

사용자 발화: `/prd-pipeline 5003 --from breakdown` — RESERVED 해제 + Phase 3 진행.

**갱신 결정**:
- **D1: E → B** (분리 X → wave 15만 분리). 옵션 B = PRD v2 가정 그대로. 보스 빌드업 효과 채택.
- **D2: C → A** (자동 폐기 → §11 마일스톤 등재). D1 변경에 따른 자동 갱신.

**진행 사유**: 사용자가 명시적으로 `--from breakdown` 호출 → RESERVED 영구 해제 의지. 5 Sprint × 5 commit × 1시간 40분 진행.

## 변경 이력

- 2026-05-04 (1차): 사용자 권장안 일괄 채택. D1=E, D2=C → RESERVED 영구.
- 2026-05-04 (2차 / 재진입): D1=E → D1=B, D2=C → D2=A. Phase 3~6 진행.
