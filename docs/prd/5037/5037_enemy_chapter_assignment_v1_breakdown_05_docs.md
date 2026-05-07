# T05: changelog + CLAUDE.md §11 §14 갱신

> **레이어**: Docs / **의존**: T04 / **예상**: 15분 / 1 commit
> **PRD 출처**: v2 §1 Sprint 표

## 1. 작업 항목

### 1.1 `docs/little_hero/changelog_v1.md`

신규 항목 prepend:
```
## 2026-05-07 — 5037 enemy_chapter_assignment_v1
6 Variant SO + Seeder/Factory 분리 + Validator 룰 + CH3/4/5 시범 wiring (PGE 6-Phase, 5 commit, NUnit +6).
PGE Phase 2: Critique 8.6/10 → Mediator D1=A 1클래스 통합 + D2=C chapterNumber 스캔 + D3=A 정적 룰.
```

### 1.2 CLAUDE.md §11 마일스톤

5건 maintain — 가장 오래된 항목 1건 제거 + 신규 추가:
```
| **enemy_chapter_assignment_v1** | 6 Variant SO + Seeder/Factory 분리 + Validator 룰 + CH3/4/5 시범 wiring + chapterNumber 스캔 (5037, 5 commit, NUnit +6, ai-assisted 3/3 채택) | ✅ 2026-05-07 |
```

### 1.3 CLAUDE.md §14 5건 maintain

가장 오래된 1건 제거 + 신규 추가 (5037 = 가장 신규).

## 2. 검증 기준

- changelog_v1.md prepend 1건
- CLAUDE.md §11 5건 유지 + §14 5건 유지

## 3. commit 메시지

```
5037 T05 — changelog + CLAUDE.md §11 §14 (4/5 — Phase 5 종결)

[변경]
  - docs/little_hero/changelog_v1.md prepend 1건
  - CLAUDE.md §11 마일스톤 5건 maintain
  - CLAUDE.md §14 핵심 결정 5건 maintain

[검증]
  - 마일스톤 5건 (가장 오래된 1건 제거)
  - 핵심 결정 5건 (5037 = 신규, 가장 오래된 제거)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
