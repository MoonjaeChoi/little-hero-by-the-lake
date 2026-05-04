# 5007 damage_hit_effect_v2 — Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 4 항목.

## High Priority Items

### D1. Sprint 3.2 commit 분할

- **Original**: 1 commit 으로 8 IAttackPattern + DamageDealer + EquipmentManager.GetActiveWeaponCategory 일괄 마이그레이션 (~150L 변경)
- **Discussion Points**:
  - **A (현재 PRD)**: 1 commit (3.2)
  - **B (권장 ★)**: **2 commit 분할** — 3.2 (DamageDealer + EquipmentManager 인프라) + 3.3 (8 IAttackPattern 일괄)
  - **C**: 8 commit 분할 (각 IAttackPattern 별) — 과도
- **Proposed**: B — 회수성 ↑, 단일 책임 commit. 총 11 → 12 commit.
- **PRD Impact**: Sprint 3 = 2 commit → 3 commit. 총 시간 50분 → 60분 (+10분).

## Medium Priority Items

### D2. avgDamage 전달 위치 (PlayerStats 의존성)

- **Original**: HitEffectResolver.Resolve 가 `avgPlayerDamage` 인자 받음. 호출자 (EnemyController) 가 PlayerStats query 해야 함 → 결합도 ↑
- **Discussion Points**:
  - **A**: EnemyController.OnHit 가 PlayerStats.Instance.AttackDamage query (Singleton 직접 참조)
  - **B (권장 ★)**: **DamageSystem.DealDamage 가 OnHit 호출 시 avgDamage 인자로 전달** (DamageSystem 은 이미 PlayerStats 참조 중)
  - **C**: DamageSystem 정적 cached avgDamage 변수
- **Proposed**: B — EnemyController 의 PlayerStats 의존성 0, DamageSystem 한곳에서만 query
- **PRD Impact**: OnHit signature 가 `OnHit(damage, isCrit, weaponCategory?, avgDamage?)` — 인자 1개 추가

### D3. CinematicEffectsConfig prefab wiring

- **Original**: Sprint 4.2 가 "정적 로딩 (Resources)" 모호 명세
- **Discussion Points**:
  - **A (권장 ★)**: **Resources.Load 정적 cache + RuntimeInitializeOnLoadMethod** — 137 prefab wiring 부담 X
  - **B**: 137 prefab SerializeField wiring (Inspector 작업 부담 ↑)
  - **C**: ScriptableObject Singleton (정적 Instance) — §15.7 DI 위반
- **Proposed**: A — Resources.Load 1회 정적 cache, runtime 초기화 시 자동 lookup
- **PRD Impact**: Sprint 4.2 의 "적 Prefab — wiring" 작업 X → CinematicEffectsConfigLoader 정적 헬퍼 추가 (~20L)

## Low Priority Items

### D4. cap 2.0× punch ceiling 적용 위치

- **Original**: §5 위험 항목에 "cap 2.0× 권장" 명시했으나 위치 미정
- **Discussion Points**:
  - **A**: SO 에 `maxPunchScale = 2.0f` 필드 추가 + Resolver 에서 Mathf.Min
  - **B (권장 ★)**: **Resolver 내부 hardcode `Mathf.Min(2.0f, ...)`** + 주석 "PG-12 시각 멀미 방지 cap"
  - **C**: cap 무시 — 비추천 (시각 과장)
- **Proposed**: B — 안전 cap 은 hardcode (잘못된 SO 값으로 인한 UX 망가짐 방지), 향후 필요시 SO 화
- **PRD Impact**: HitEffectResolver 내부 1줄 추가 (`punchScale = Mathf.Min(2.0f, basePunch * profile.punchMul * intensityFactor);`)

## 요약

- Total: 4
- High: 1 / Medium: 2 / Low: 1
- 권장 mode: **ai-assisted**
- **권장 일괄**: D1=B / D2=B / D3=A / D4=B

## 결정 입력 형식

```
"권장안 그대로"        → D1=B / D2=B / D3=A / D4=B 일괄
"D1=A, 나머지 권장"    → 1 commit 유지
"권장 + D3=B"         → wiring 옵션만 변경
```
