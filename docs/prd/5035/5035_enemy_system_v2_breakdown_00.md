# 5035 enemy_system_v2 Implementation Breakdown — 개요

> Phase 3 산출 · 7 태스크 / 7 commit / ~3h 30m

## 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 시간 |
|---|---|---|---|---|
| **T01** | IEnemyBehavior + IDeathReaction interface + EnemyData 갱신 | Data + Logic | - | 30분 |
| **T02** | FollowBehavior + 2 Factory + EnemyAI 폐기 | Logic | T01 | 30분 |
| **T03** | 4 신규 behavior (Stationary/Charger/Ranged/Bomber) | Logic | T02 | 45분 |
| **T04** | SplitterReaction + ExplodeReaction | Logic | T01 | 30분 |
| **T05** | EnemyComposition + WaveComposition SO + ChapterData 통합 | Data + Editor | T01 | 30분 |
| **T06** | EnemyController 통합 + WaveManager 호환 | Logic | T02~T05 | 30분 |
| **T07** | NUnit 12 + changelog + CLAUDE.md | Test + Docs | T01~T06 | 30분 |

## 의존성 그래프
```
T01 ─┬─→ T02 ─→ T03 ─┬─→ T06 ─→ T07
     ├─→ T04 ────────┤
     └─→ T05 ────────┘
```

## §15 정합
| T | §15 정합 | 예상 라인 |
|---|---|---|
| T01 | §15.1 ISP / §15.4 SO ✅ | interface 17L + EnemyData +20L |
| T02 | §15.1 OCP Factory ✅ | FollowBehavior 40L + Factory 2개 50L |
| T03 | §15.6 100L (각 ~50L) ✅ | 4 behavior × 50L |
| T04 | §15.6 100L (각 ~30L) ✅ | 2 reaction × 30L |
| T05 | §15.4 SO ✅ | 2 SO 클래스 ~55L + Editor 50L |
| T06 | §15.7 DI ✅ | EnemyController +30L (위임) |
| T07 | §15.9 Test ✅ | NUnit 12 ~150L |

## 변경 이력
- 2026-05-07: 초기 (Phase 3)
