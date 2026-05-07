# 5035 Sprint PRD — enemy_system_v2

> **PRD v1 (초안)** · 디자인: `docs/prd/5035_enemy_system_v2.md`
> 권장 채택: D1=A, D2=B, D3=B, D4=B, D5=A, D6=A, D7=A, D8=B (mediator 단계)

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | IEnemyBehavior + IDeathReaction interface + EnemyData enum/필드 | 1 | 30분 |
| 2 | FollowBehavior (기존 EnemyAI 마이그레이션) + EnemyBehaviorFactory + EnemyDeathReactionFactory | 1 | 30분 |
| 3 | 4 신규 behavior (Stationary/Charger/Ranged/Bomber) — D1=A | 1 | 45분 |
| 4 | SplitterReaction + ExplodeReaction + Factory — D3=B 3마리, D4=B 1.5u | 1 | 30분 |
| 5 | EnemyComposition + WaveComposition SO + ChapterData 통합 — D6=A | 1 | 30분 |
| 6 | EnemyController 통합 (behavior Tick + OnDeath reaction) | 1 | 30분 |
| 7 | NUnit 12 (D8=B) + changelog + CLAUDE.md | 1 | 30분 |
| **합계** | | **7** | **~3h 30m** |

## 2. Sprint 1 — IEnemyBehavior + IDeathReaction interface

### 2.1 신규 파일
- `Scripts/Gameplay/Enemies/IEnemyBehavior.cs` (~10L)
- `Scripts/Gameplay/Enemies/IDeathReaction.cs` (~7L)

### 2.2 EnemyData 갱신 (Data 레이어)
- `Scripts/Data/EnemyData.cs` +10 필드 (behaviorType / deathReactionType / splitterChildData / splitterChildCount / bomberRadius / chargerInterval / rangedFireInterval / projectilePrefab)
- `EnemyBehaviorType` enum: Follow=0, Stationary=1, Charger=2, Splitter=3, Ranged=4, Bomber=5
- `EnemyDeathReactionType` enum: None=0, Splitter=1, Explode=2

## 3. Sprint 2 — FollowBehavior + Factory

### 3.1 FollowBehavior (기존 EnemyAI 마이그레이션, D5=A)
- `Scripts/Gameplay/Enemies/Behaviors/FollowBehavior.cs` (~40L)
- 기존 EnemyAI.Update 의 추적 로직 그대로
- EnemyAI 폐기 (D5=A) — 호출처 EnemyController 1 파일만

### 3.2 Factory
- `Scripts/Gameplay/Enemies/Internal/EnemyBehaviorFactory.cs` (~30L) — 6 case
- `Scripts/Gameplay/Enemies/Internal/EnemyDeathReactionFactory.cs` (~20L) — 3 case

## 4. Sprint 3 — 4 신규 Behavior

### 4.1 StationaryBehavior (~50L) — D2=B 발사체 공격
- 위치 고정 (transform.position 고정)
- rangedFireInterval 마다 발사체 spawn (DamageQuery.FindNearest 활용)

### 4.2 ChargerBehavior (~60L)
- 평소 추적 (FollowBehavior 답습)
- chargerInterval 마다 1초 텔레그래프 후 플레이어 위치로 빠른 돌진 (~3x 속도)
- 5033 ElementVfxBank 또는 VfxPool Spawn Warning circle

### 4.3 RangedBehavior (~50L)
- 일정 거리 (예 4u) 유지하며 후퇴
- rangedFireInterval 마다 발사체 spawn

### 4.4 BomberBehavior (~55L)
- 추적 (FollowBehavior 답습)
- 플레이어 거리 ≤ bomberRadius (1.5u, D4=B) 시 1초 텔레그래프 → ExplodeReaction 트리거
- OnDeath 시 자동 ExplodeReaction (즉 자폭)

## 5. Sprint 4 — Death Reactions

### 5.1 SplitterReaction (~30L) — D3=B 3마리
- OnDeath 시 EnemyData.splitterChildData 의 prefab 을 splitterChildCount 번 spawn
- spawn 위치: deathPosition ± 0.5u 분산
- VfxPool 활용 (재사용)

### 5.2 ExplodeReaction (~30L) — D4=B 1.5u
- OnDeath 시 deathPosition 반경 bomberRadius 내 플레이어 + 적 데미지 (DamageQuery.FindEnemiesInCircle)
- VfxPool.SpawnWarning 시각 효과
- PG-12 톤 — 별 흩어짐 X (잔인 회피)

## 6. Sprint 5 — Composition SO

### 6.1 EnemyComposition.cs + WaveComposition.cs (Data 레이어)
- `Scripts/Data/EnemyComposition.cs` (~25L)
- `Scripts/Data/WaveComposition.cs` (~30L)
- enum SpawnPattern { Continuous, Burst, Timed }
- enum WaveType { Standard, Rush, Elite, Boss, SplitterRush }

### 6.2 ChapterData 통합
- `Scripts/Data/ChapterData.cs` 의 waves 배열 (WaveComposition[20]) 추가
- 기존 enemyPool / bossPool 등 보존 (호환성)

### 6.3 Editor 시드 (D7=A CH1~5 시범)
- `Editor/SeedWaveCompositions.cs` — CH1~5 의 WaveComposition 5×20=100 SO 자동 생성

## 7. Sprint 6 — EnemyController 통합

### 7.1 EnemyController 갱신 (~+30L)
- Awake/Initialize 시 EnemyBehaviorFactory.Create + Initialize(this, data, target)
- Update 시 _behavior.Tick(Time.deltaTime)
- Die 시 EnemyDeathReactionFactory.Create(data.deathReactionType).OnDeath(this, transform.position)

### 7.2 EnemyAI 폐기 (D5=A)
- `Scripts/Gameplay/Enemies/EnemyAI.cs` 삭제
- 사용처 1~2 파일 마이그레이션

## 8. Sprint 7 — NUnit 12 + docs

### 8.1 NUnit 12 (D8=B)
- `EnemyBehaviorFactoryTests` (1 — 6 case 모두 not null)
- `FollowBehaviorTests` (1 — 추적 방향 정합)
- `StationaryBehaviorTests` (1 — 위치 고정)
- `ChargerBehaviorTests` (2 — 텔레그래프 + 돌진 속도)
- `SplitterBehaviorTests` (1 — Splitter 자체는 추적, OnDeath 별도)
- `RangedBehaviorTests` (1 — 거리 유지)
- `BomberBehaviorTests` (2 — 텔레그래프 + 폭발)
- `SplitterReactionTests` (1 — 3마리 분열 검증)
- `ExplodeReactionTests` (1 — 반경 1.5u 데미지)
- `EnemyDeathReactionFactoryTests` (1 — 3 case)

### 8.2 docs
- `changelog_v1.md` +5035
- `CLAUDE.md` §14 + §11 마일스톤

## 9. 의존성 그래프

```
Sprint 1 (interface + enum) ─┐
                              ├─→ Sprint 2 (Follow + Factory)
                              │       ↓
                              │   Sprint 3 (4 신규 behavior) ─┐
                              │                                ├─→ Sprint 6 (Controller 통합)
                              ├─→ Sprint 4 (Reactions) ────────┤             ↓
                              │                                │       Sprint 7 (NUnit + docs)
                              └─→ Sprint 5 (Composition SO) ───┘
```

## 10. 완료 정의 (DoD)

- [ ] IEnemyBehavior + IDeathReaction interface
- [ ] 6 behavior + 3 reaction (1 None + 2 신규)
- [ ] EnemyBehaviorFactory + EnemyDeathReactionFactory
- [ ] EnemyData 11 필드 + 2 enum
- [ ] EnemyComposition + WaveComposition SO
- [ ] ChapterData 통합 + Editor 시드 CH1~5
- [ ] EnemyController 통합 + EnemyAI 폐기
- [ ] NUnit 12 PASS
- [ ] 컴파일 errors=0
- [ ] 사용자 Play Mode (시나리오 1+2+3)

## 11. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-07 | 초기 — 7 Sprint / ~3h 30m / NUnit 12 |
