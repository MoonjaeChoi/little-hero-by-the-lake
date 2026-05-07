# 5035 Sprint PRD — enemy_system_v2 (v2 최종)

> Mediator 확정 · D1=A, D2=B, D3=B, D4=B, D5=A, D6=A, D7=A, D8=B
> 다음: `/prd-breakdown` (Phase 3)

## 1. 결정 적용
PRD v1 그대로 유지. 권장 8/8 채택이라 변동 X.

원본 PRD: `docs/prd/5035/5035_enemy_system_v2_prd.md` 참조.

## 2. Sprint 표 (7 Sprint / ~3h 30m)
| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | IEnemyBehavior + IDeathReaction interface + EnemyData enum/필드 | 1 | 30분 |
| 2 | FollowBehavior 마이그레이션 + 2 Factory | 1 | 30분 |
| 3 | 4 신규 behavior (Stationary/Charger/Ranged/Bomber) | 1 | 45분 |
| 4 | SplitterReaction + ExplodeReaction (D3=B 3마리, D4=B 1.5u) | 1 | 30분 |
| 5 | EnemyComposition + WaveComposition SO + ChapterData 통합 (D6=A) | 1 | 30분 |
| 6 | EnemyController 통합 + EnemyAI 폐기 (D5=A) | 1 | 30분 |
| 7 | NUnit 12 (D8=B) + changelog + CLAUDE.md | 1 | 30분 |

## 3. 변경 이력
- 2026-05-07: v2 (Mediator) — 권장 8/8 채택
