# 5023 OOP MetaProgression Split — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> **일시**: 2026-05-06
> **총**: 4 태스크 / 4 commit / 2h45m

---

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 |
|---|---|---|---|---|
| T01 | Sprint 1 — Loader + Bootstrapper 분리 (5021 흡수) | Logic | — | 30분 |
| T02 | Sprint 2 — Character + Chapter service + 정적 event (D3=B) + Sprint 2 호출처 | Logic | T01 | 45분 |
| T03 | Sprint 3 — Currency + Equipment service + Sprint 3 호출처 | Logic | T02 | 60분 |
| T04 | Sprint 4 — Endgame + NUnit 6 + SaveCall Validator 룰 (D2=C) + changelog | Test+Docs | T03 | 30분 |
| **합계** | | | | **2h45m** |

---

## 2. 의존성 그래프

```
T01 (Loader/Bootstrap) → T02 (Char/Chapter+event) → T03 (Currency/Equip) → T04 (Endgame/Test/Validator)
```

직렬. 각 Sprint 가 후속 호출처 마이그레이션 baseline.

---

## 3. CLAUDE.md §15 정합

| 태스크 | §15 | 검증 |
|---|---|---|
| T01 | §15.1 SRP / §15.6 100줄 (Loader 50L + Bootstrapper 70L) | ✅ |
| T02 | §15.5 정적 event (D3=B) | ✅ |
| T03 | §15.4 SO 우선 (화폐 변경 → SaveSystem.Save) | ✅ |
| T04 | §15.9 NUnit + §15.12 Validator 룰 (D2=C) | ✅ |

---

## 4. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 태스크, D1=C+D2=C+D3=B 반영 |
