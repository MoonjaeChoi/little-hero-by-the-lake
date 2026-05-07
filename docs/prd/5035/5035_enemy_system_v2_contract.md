# Sprint Contract: 5035 enemy_system_v2

> PGE Phase 4 · 6 카테고리

## C1 컴파일
모든 commit (T01~T07) errors=0

## C2 Editor Test (NUnit 12)
- EnemyBehaviorFactoryTests (1 — 6 case not null)
- FollowBehaviorTests (1)
- StationaryBehaviorTests (1 — 위치 고정)
- ChargerBehaviorTests (2 — 텔레그래프 + 돌진)
- SplitterBehaviorTests (1)
- RangedBehaviorTests (1 — 거리 유지)
- BomberBehaviorTests (2)
- SplitterReactionTests (1 — 3마리 분열)
- ExplodeReactionTests (1 — 1.5u 데미지)
- EnemyDeathReactionFactoryTests (1)

## C3 ProjectValidator
- errors=0
- warnings 변동 ±0
- 신규 200L+ 파일 0

## C4 Reflection
| Type | Method/Field |
|---|---|
| `IEnemyBehavior` | Initialize / Tick / OnEnter / OnExit |
| `IDeathReaction` | OnDeath |
| `EnemyBehaviorType` enum | Follow/Stationary/Charger/Splitter/Ranged/Bomber |
| `EnemyDeathReactionType` enum | None/Splitter/Explode |
| `EnemyBehaviorFactory.Create` | (EnemyBehaviorType) → IEnemyBehavior |
| `EnemyDeathReactionFactory.Create` | (EnemyDeathReactionType) → IDeathReaction |
| `EnemyData` | behaviorType, deathReactionType, splitterChildData, splitterChildCount, bomberRadius, ... |

## C5 SO 검증
- EnemyComposition + WaveComposition SO 클래스 존재
- ChapterData.waves 길이 20 (CH1~5 시범 시드)

## C6 Play 시나리오

### 시나리오 1 — Stationary 적
- CH3 wave 5 → Stationary 적 + 발사체 공격 (1.5초 간격)

### 시나리오 2 — Splitter (곰형)
- CH4 wave 10 → Splitter 사망 → 작은 적 3마리 분열 + Hierarchy 잔존 0

### 시나리오 3 — Bomber 자폭
- CH5 wave 15 → Bomber 가까이 → 1초 텔레그래프 → 폭발 1.5u

## 합격 기준
- 자동 모두 ✅ + 시나리오 1+2+3 PASS

## 변경 이력
- 2026-05-07: 초기 (Phase 4)
