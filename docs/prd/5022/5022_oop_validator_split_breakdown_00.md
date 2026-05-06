# 5022 OOP Validator Split — Implementation Breakdown 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> **일시**: 2026-05-06
> **총**: 4 태스크 / 4 commit / 2h15m
> **의존성**: 없음 (독립)

---

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 예상 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | Sprint 1.1 — Issue/Severity 추출 + 폴더 구조 | Logic | — | 15분 | `_01_issue_extraction.md` |
| T02 | Sprint 1.2 — orchestrator + 6 간단 Rule (D1=B) | Logic | T01 | 45분 | `_02_orchestrator_simple_rules.md` |
| T03 | Sprint 2 — 복잡 5 Rule 분리 | Logic | T02 | 45분 | `_03_complex_rules.md` |
| T04 | Sprint 3 — NUnit 회귀 + 결과 비교 + changelog | Test+Docs | T03 | 30분 | `_04_regression_changelog.md` |
| **합계** | | | | **2h15m** | |

---

## 2. 의존성 그래프

```
T01 (Issue 추출) → T02 (orchestrator + 6 간단) → T03 (복잡 5) → T04 (검증 + changelog)
```

직렬. T01 의 Issue/Severity public 정의가 모든 Rule 클래스 의존.

---

## 3. 병렬 가능성

병렬 가능 0 — 모든 태스크 직렬 의존.

---

## 4. 구현 순서 권장

```
T01 (15분) → T02 (45분) → T03 (45분) → T04 (30분) = 2h15m
```

Sprint 1+2 통합 (D1=B) 으로 항상 11 룰 작동 보장 — T02 시점에 5 stub 임시 보존.

---

## 5. CLAUDE.md §15 정합 자동 검증

| 태스크 | §15 적용 | 검증 |
|---|---|---|
| T01 | Issue.cs 신규 ~15L — §15.6 100줄 룰 ✅, §15.1 SRP ✅ | ✅ |
| T02 | orchestrator ~250L (5 stub 임시) + 6 Rule 40~55L — §15.6 100줄 룰 ✅ | ✅ |
| T03 | 5 Rule 50~85L — §15.6 100줄 룰 ✅, ProjectValidator 최종 ~80L | ✅ |
| T04 | docs 만 — 직접 적용 X | ✅ |

→ **§15 위반 0건** (D2=A 의 80~85L 일시 허용 = 100L 룰 충족).

---

## 6. Unity MCP 안전 전략

| 태스크 | MCP 호출 | 안전 패턴 |
|---|---|---|
| T01 | refresh_unity 1회 + 5~10s + read_console | C# 신규 클래스 컴파일 검증 |
| T02 | refresh_unity + 5~10s + read_console | 큰 변경 (~600L) — 안정화 대기 |
| T03 | refresh_unity + 5~10s + read_console | 5 stub 제거 + 5 Rule 신규 |
| T04 | 0 (docs only) | — |

---

## 7. 4 레이어 분류

| 레이어 | 태스크 | 개수 |
|---|---|---|
| Logic | T01, T02, T03 | 3 |
| Test+Docs | T04 | 1 |

---

## 8. 변경 이력

| 일자 | 작성자 | 변경 |
|---|---|---|
| 2026-05-06 | Claude (prd-breakdown Phase 3) | 초기 — 4 태스크 / 4 commit, D1=B 통합 반영 |
