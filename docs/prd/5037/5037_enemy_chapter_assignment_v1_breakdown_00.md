# 5037 enemy_chapter_assignment_v1 Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-07 · 총 5 태스크 / 5 commit / 약 1h55m

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존 | 시간 | 파일 |
|---|---|---|---|---|---|
| T01 | 6 Variant EnemyData SO + Variants 폴더 (Seeder 1차 실행으로 일괄 생성) | Data | T02 | 25m | _01_*.md |
| T02 | ChapterVariantSeeder + VariantSoFactory 분리 + Validator 룰 | Editor | - | 35m | _02_*.md |
| T03 | CH3/4/5 wiring (Seeder.SeedAndWire 호출 1회로 자동) | Data | T01,T02 | 15m | _03_*.md |
| T04 | NUnit 6 (Variant SO + Chapter wiring 검증) | Test | T03 | 25m | _04_*.md |
| T05 | changelog + CLAUDE.md §11 §14 갱신 | Docs | T04 | 15m | _05_*.md |

## 2. 의존성 그래프

```
T02 (Seeder + Factory + Rule) ─┐
                               ├─→ T01 (SO 일괄 — Seeder.SeedAndWire) ──→ T03 (Chapter wiring)
                               │                                              ↓
                               └────────────────────────────────────────→ T04 (Test) ──→ T05 (Docs)
```

> T01 + T03 가 사실상 동일 도구 1회 실행 — Seeder.SeedAndWire() 1회 호출이 두 단계 일괄 처리. Sprint S1+S3 통합 commit 가능.

## 3. 병렬 가능성

- **T02 단독** (도구 신규)
- **T01 + T03 동시** (도구 1회 실행 후 .asset 일괄 생성/wiring)
- **T04 → T05** 직렬

## 4. 구현 순서 권장

1. **T02** — Seeder + Factory + Rule 신규 (35m, 컴파일 검증)
2. **T01 + T03** — Tools/LittleHero/Seed Chapter Enemy Variants 1회 실행 → SO 6개 + 챕터 3개 wiring (15m, 시범 commit)
3. **T04** — NUnit 6 작성 + 회귀 (25m)
4. **T05** — changelog + CLAUDE.md (15m)

총 1h30m (병렬 시) / 1h55m (직렬 시)

## 5. CLAUDE.md §15 정합 사전 검증

- T02 Seeder ~60L ✅ 100L 룰
- T02 Factory ~60L ✅
- T02 Rule ~50L ✅
- T04 Test ~110L (NUnit 예외 — 100L 룰 비적용)

## 6. Unity MCP 안전 전략

- T02 컴파일 1회 (Editor 스크립트만)
- T01+T03 시드 도구 실행 (메뉴 Tools/LittleHero) — Editor 도메인 reload 1회
- T04 NUnit 컴파일 1회 + Test Runner 실행

각 사이클 5~10초 대기. 도구 실행 후 SaveAssets+Refresh 자동.

## 7. 변경 이력
- 2026-05-07: 초기 작성 (Phase 3 Breakdown)
