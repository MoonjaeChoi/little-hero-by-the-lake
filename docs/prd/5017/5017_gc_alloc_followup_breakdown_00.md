# 5017 — Implementation Breakdown 개요

> **Phase 3 산출** — 5 commit / ~2.5h

## 1. 태스크

| ID | 제목 | 레이어 | 시간 |
|---|---|---|---:|
| T01 | DamageNumberFloater FormatDamage cache + AudioManager 정리 | Logic 핫 | 30분 |
| T02 | DropContext class → struct + 호출처 갱신 | Logic | 35분 |
| T03 | EnemyController wobble 임계 0.005f setter cache | Logic 핫 | 40분 |
| T04 | ProjectValidator 5017 룰 + NUnit 4 (DamageNumberFloaterFormatTests) | Test | 30분 |
| T05 | Mac Player 재측정 + docs (changelog + CLAUDE.md §11/§14) | Docs | 30분 |

## 2. 의존성

```
T01, T02, T03 (영역 독립) → T04 (Validator + NUnit) → T05 (재측정 + docs)
```

## 3. CLAUDE.md §15 정합

- T01: §15.4 Cache 패턴 + StringBuilder 정적
- T02: §15.11 GC 0 (struct)
- T03: native interop 절감 (60fps 마진)
- T04: §15.9 NUnit + §15.12 Validator
- T05: changelog + 마일스톤

## 4. 변경 이력
- 2026-05-06: 초기 작성
