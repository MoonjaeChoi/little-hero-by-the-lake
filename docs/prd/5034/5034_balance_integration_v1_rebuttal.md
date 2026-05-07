# 5034 balance_integration_v1 PRD Rebuttal

> **Phase 2.2 Rebuttal** — prd-rebuttal 스킬 적용
> **검토자**: Claude Code (자동) / **일시**: 2026-05-07

## 1. 요약 표

| Category | Criticism Summary | Priority | Status | Action | §15 Aligned |
|---|---|---|---|---|---|
| Accept A1 | 50:50 비중 검증 명시 | — | Confirmed | 변동 X | ✅ |
| Accept A2 | 메이플 정합 D4=B | — | Confirmed | discussion_topics 결정 후 v2 | ✅ |
| Accept A3 | Sprint 분해 균형 | — | Confirmed | 변동 X | ✅ |
| Accept A4 | 검증 시나리오 3 | — | Confirmed | 변동 X | ✅ |
| Accept A5 | NUnit 5+ 명시 | — | Confirmed | 변동 X | ✅ |
| Accept A6 | 위험 + 완화 5건 | — | Confirmed | 변동 X | ✅ |
| 디자인 옵션 | D1 챕터 매트릭스 (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| 디자인 옵션 | D2 슬롯 곡선 (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| 디자인 옵션 | D3 무기 +N (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| 디자인 옵션 | D4 직업 시너지 (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| 디자인 옵션 | D5 DPS check (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| 디자인 옵션 | D6 캐시 진행 (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| 디자인 옵션 | D7 CurveSimulator (A/B/C) | Medium | **Discuss** | mediator → v2 | △ |
| §15.11 가시성 | D8 PlayerStats 신규 필드 | Medium | **Discuss** | mediator (권장 B Add/Remove 메서드) | △ |
| Sprint 분해 | D9 Sprint 4 분리 | Low | **Discuss** | mediator (권장 B 2 commit) | △ |
| 후속 backlog | D10 5034.1 적합도 | Low | **Discuss** | mediator (권장 B 폐기) | △ |

---

## 2. 품질 메트릭

- **Total Discussion Points**: 16 (Accept 6 + Discuss 10 + Reject 0)
- **Accepted (수정 반영)**: 0건 (PRD 자체 그대로 — 디자인 결정 후 v2 확정)
- **Confirmed (이미 PRD 반영)**: 6건 (A1~A6)
- **Discuss (사용자 결정 필요)**: 10건 (D1~D10)
- **Rejected**: 0건
- **§15 정합률**: PRD 자체 9/10 (D8 결정 시 10/10)

---

## 3. Detailed Responses

### 3.1 A1~A6 — Confirmed (이미 PRD 반영)

| Item | Confirmation |
|---|---|
| A1 50:50 비중 검증 | character_progression_v1 답습, PRD §1.1 명시 |
| A2 메이플 정합 D4=B | PRD §3.4 옵션 A/B/C 명시, mediator 결정 후 v2 |
| A3 Sprint 분해 균형 | PRD §1 Sprint 표 명시 |
| A4 검증 시나리오 3 | PRD §10 명시 |
| A5 NUnit 5+ | PRD §7.1 명시 |
| A6 위험 + 완화 5 | 디자인 §7 명시 |

→ 모두 PRD 그대로 진행. 변동 X.

### 3.2 D1~D7 — 디자인 옵션 (Discuss)

PRD §5 자체에 7 디자인 옵션이 명시되어 있음. 각각 A/B/C 옵션 + Claude 권장 명시. mediator 단계에서 사용자 일괄 결정 (예 "권장 그대로", "D4=A" 등).

**Status**: Discuss → discussion_topics.md 로 이동
**이유**: 디자인 결정 사안 (Claude 단독 결정 X — 게임 의도 사용자 권한)
**Resolution Path**: mediator ai-assisted 모드 — Claude 권장 + 사용자 응답 대기

### 3.3 D8 — PlayerStats 신규 필드 가시성 [§15.11 예방적]

**Original Criticism** (critique § 4 D8):
> Sprint 3 의 PlayerStats 신규 필드 (`bossDamageBonus / critChanceBonus / aoeBonus`) 가시성 미명시. §15.11 안티 패턴 회피 (public 필드 X).

- **Status**: Discuss (3 옵션) → discussion_topics
- **이유**: Inspector 편집 가능 vs 코드 명시적 vs 단순 — 사용자 게임 디자인 의도 반영
- **Claude 권장**: B (Add/Remove 메서드) — §15.2 / §15.7 정합 + 직업 변경 시 차감 가능
- **PRD Impact**: Sprint 3 코드 예시 갱신 — 가시성 명시

### 3.4 D9 — Sprint 4 작업량 분리 [Sprint 분해]

**Original Criticism** (critique § 4 D9):
> Sprint 4 (CurveSimulator 확장 1h) 다른 Sprint 15~30분 대비 큼.

- **Status**: Discuss (3 옵션)
- **Claude 권장**: B (Sprint 4a/4b 분리, 2 commit) — 작업량 균형 + 회수 용이
- **PRD Impact**: §1 Sprint 표 갱신 (6 → 7 commit), 의존성 그래프 변동 X

### 3.5 D10 — 5034.1 후속 backlog 적합도

**Original Criticism** (critique § 4 D10):
> 5034.1 V매트릭스 / 5차 스킬 (메이플 본판 답습) — LittleHero 단일 캐릭터 게임 적합도 ↓.

- **Status**: Discuss (3 옵션)
- **Claude 권장**: B (폐기) — LittleHero 게임 톤 정합 우선
- **PRD Impact**: §9 5034.1 항목 삭제 또는 변경

---

## 4. PRD v2 적용 변경 요약

(Accept 6건 — 모두 PRD 자체 그대로 = 변동 X. Discuss 10건 mediator 결정 후 v2 최종 확정.)

| Phase | 변경 |
|---|---|
| Rebuttal (현재) | PRD 자체 변동 X. v2 = PRD 그대로 (디자인 옵션 미결) |
| Mediator (다음) | D1~D10 결정 → v2 확정 (예 D4=B 시 §3.4 메이플 정합 명시 등) |

---

## 5. 변경 이력
- 2026-05-07: 초기 작성 (Phase 2.2 Rebuttal). Accept 6 / Discuss 10 / Reject 0. 디자인 결정 사안 mediator 위임.
