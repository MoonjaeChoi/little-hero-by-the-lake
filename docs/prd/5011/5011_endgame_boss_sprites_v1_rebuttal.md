# 5011 — Endgame Boss Sprites Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 자동 적용
> **검토자**: Claude Code · 일시: 2026-05-05

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action |
|---|---|---|---|---|
| 시스템 통합 | D1 PRD §1 위치 부정확 (Enemies → Bosses) | High | Accepted | v2 즉시 정정 |
| 시스템 통합 | D2 재생성 ROI — 기존 11 healthy | High | Discuss | discussion_topics 분리 |
| 시스템 통합 | D3 EnemyData SO icon GUID 보존 | Medium | Discuss | discussion_topics 분리 |

## 2. 품질 메트릭

- 총 Discussion Points: 3
- Accepted: 1 (D1 위치 정정)
- Rejected: 0
- Discuss: 2 (D2, D3)

## 3. Detailed Responses

### 3.1 D1 — PRD §1 위치 정정 (Accepted)

- **Status**: 즉시 v2 반영
- **Action**: §1.1 + Sprint commit 메시지 경로 `Enemies/` → `Bosses/`

### 3.2 D2 — 재생성 ROI (Discuss → topics)

- **이유**: 11 sprite 모두 metric healthy. 사용자 시각 review 필요.
- **Resolution Path**: mediator 결정

### 3.3 D3 — GUID 보존 (Discuss → topics)

- **이유**: PNG 덮어쓰기 시 .meta GUID 보존 (5009 패턴) — 안전. 다만 사용자 명시 권장.
- **Resolution Path**: mediator 결정 (옵션 A 권장)
