# 5025 UI Korean Polish — Breakdown 개요

> Phase 3 · 4 태스크 / 4 commit / 2h30m

---

## 태스크 목록

| ID | 제목 | 시간 |
|---|---|---|
| T01 | KoreanLabelAuditor + whitelist (D1=B) | 30분 |
| T02 | Sprint 2 Attic + HUD spot fix | 30분 |
| T03 | Sprint 3 Map + Inv + Equip + Gacha | 60분 |
| T04 | Sprint 4 Endgame + Forge + Memory + 폰트 + changelog | 30분 |

---

## 의존성

```
T01 (Auditor) → T02/T03/T04 (Auditor 보고서 기반 spot fix)
```

T01 의 Auditor 보고서가 T02~T04 의 baseline. T02~T04 는 사용자 시각 review 의존.

---

## §15 정합

- §15.4 SO 우선 (한국어 displayName SO 필드)
- §15.6 Auditor ~80L
- §15.12 Editor 도구 인프라

---

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 태스크 |
