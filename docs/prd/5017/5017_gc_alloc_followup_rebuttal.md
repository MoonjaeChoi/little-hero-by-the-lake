# 5017 PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-06

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action |
|---|---|---|---|---|
| 메모리 누수 | FormatDamage cache cap 1024 | Medium | **Discuss → D1** | 사용자 결정 |
| 호환성 | DropContext struct by-value 위험 | Medium | **Discuss → D2** | 사용자 결정 |
| 임계값 | wobble 변동 임계 0.001f 작음 | Low | **Discuss → D3** | 사용자 결정 |
| Accept 8 | 모든 Sprint 영역 + cache + struct | - | Accepted | v2 그대로 |

## 2. 품질 메트릭

- 총 Discussion Points: 11 (8 Accept + 3 Discuss)
- Accepted: 8 (73%)
- Rejected: 0 (0%)
- Discuss: 3 (27%)
- §15 정합률: 100%

## 3. v2 적용 변경

Accept 항목은 모두 PRD 그대로 — v2 변경 0. Discuss 3건만 mediator 후 적용.

## 4. 변경 이력
- 2026-05-06: 초기 작성 (Phase 2.2 Rebuttal)
