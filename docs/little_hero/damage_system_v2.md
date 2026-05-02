# Damage System v2 — 충돌 폐기 + Auto-Target + 메이플 키우기 다중 타깃

> **목표**: Projectile-Collider 충돌 기반 데미지 → **Physics2D.OverlapCircleNonAlloc + Distance 직접 query** 기반으로 전환.
> 메이플 키우기 (MapleStory Idle RPG) 의 자동 공격 + 다중 타깃 보정 패턴 차용.
> 현재 PIVOT v4 (cap 300) + 7 공격 패턴 모두 적용.

## 0. 결정 이력

- 2026-05-03: 사용자 요청 — 충돌 대안 데미지 + 메이플 키우기 분석
- 본 문서: 옵션 B 권장 (충돌 폐기 + 다중 타깃 보정)

## 1. 현재 시스템 분석 — Projectile-Collider 충돌

### 1.1 동작 흐름
```
PlayerController → ProjectileAttack.Tick → Projectile prefab spawn (Rigidbody2D + Collider2D)
  ↓ 매 프레임 Rigidbody2D.linearVelocity 이동
EnemyController (Collider2D)
  ↓ 충돌 발생 시 Unity Physics2D 가 자동 OnTriggerEnter2D 호출
Projectile.OnTriggerEnter2D → DamageSystem.DealDamage(enemy, damage)
```

### 1.2 비용
- **Physics2D 매 프레임 broadphase**: 모든 Rigidbody2D × Collider2D 페어 ≤ N×M
- **GC**: Unity Physics 내부 페어 cache, 매 enter/exit 시 message dispatch
- **중복 hit 방지**: `_hitEnemies` HashSet 추적 (Projectile 마다)
- **Pierce/Chain**: pierceCount 처리 시 추가 페어 검사

### 1.3 cap 300 + 7 패턴 동시 환경
- Projectile 50 + Aura 1 + Pulse 1 + Trail 1 + Orbit 4 + Rain 5 + Summon 3 = ~65 active "데미지 객체"
- × 적 250 = 16,250 페어 매 프레임 broadphase
- Unity 6 Physics2D 최적화로 ~1ms 수준 (안전), 그러나 모바일 50% 여유 고려 시 압박

## 2. 메이플 키우기 (MapleStory Idle RPG) 데미지 시스템

### 2.1 핵심 패턴
| 항목 | 값 |
|---|---|
| 공격 모드 | 자동 공격 + 스킬 + 패시브 |
| 다중 타깃 | 베이스 공격 시 적 N마리 동시 hit = **26% 감소 데미지** |
| Stat scaling | 1 STR/DEX/INT/LUK = 1 Attack (직업별) |
| Attack Speed cap | **150%** (초과분 multiplicative 적용) |
| 무기 효과 | equipped (장착 시) + inventory (소지 시) 분리 |
| AoE 우선 | idle farming 효율 위해 다중 타깃 메인 |

### 2.2 시사점 (Little Hero 적용)
- ✅ **자동 공격** = 본 프로젝트 이미 적용 (auto-shooter)
- ✅ **다중 타깃 보정** = 메이플 26% 패턴 → Little Hero 도입 가능
- ✅ **AoE 우선** = PIVOT v3 작은 무리 패러다임 정합
- ✅ **Attack Speed cap** = stats_v1 §3 이미 정의, runtime cap 추가 필요
- ⚠️ **무기별 effect 분리** = 본 프로젝트는 단일 effect

## 3. Unity 충돌 대안 데미지 방식 비교

### 3.1 후보 5종 비교

| 방식 | 메모리 GC | CPU | 코드 복잡도 | Unity 권장 |
|---|---|---|---|---|
| **OnTriggerEnter2D** (현재) | 페어 캐시 GC | 중 | 낮 (자동) | 적 ≤ 100 |
| **Physics2D.OverlapCircle** | 0 | 낮 | 낮 | 단일 검사 |
| **Physics2D.OverlapCircleAll** | array allocate | 낮 | 낮 | 일회성 |
| **Physics2D.OverlapCircleNonAlloc** | **0** (사전 buffer) | 낮 | 중 | **추천** — 매 프레임 |
| **Distance check (sqrMagnitude)** | 0 | 매우 낮 | 낮 | Layer 필터 X |
| **Spatial Hashing** | 0 | 최저 | 높 | 1k+ enemies |

### 3.2 Vampire Survivors 패턴 (참조)
- Unity Physics2D **사용 안 함**
- Custom physics + 단일 draw call 풀 매니저
- **20,000 bullets** 동시 60fps
- 모든 데미지 = sqrMagnitude distance check (LayerMask 폐기)

### 3.3 Little Hero (cap 300) 권장
- **Physics2D.OverlapCircleNonAlloc** + LayerMask "Enemy"
- 매 프레임 64 buffer (cap 300 의 일부분)
- 0 GC + Layer 필터 + Unity 친화

## 4. 권장 설계 — 옵션 B (충돌 폐기 + 다중 타깃)

### 4.1 핵심 아이디어
```
모든 데미지 객체 (7 패턴) 의 Collider2D + Rigidbody2D 제거
  ↓ 매 프레임 Tick (또는 attack interval)
DamageQuery.FindEnemiesInCircle(position, radius, maxTargets)
  ↓ Physics2D.OverlapCircleNonAlloc (LayerMask "Enemy", buffer[64])
타깃 list 거리 정렬 + maxTargets 자르기
  ↓ DamageDealer.ApplyMulti(targets, baseDamage, multiTargetPenalty)
타깃 i = baseDamage × penalty^i (i=0 100%, i=1 60%, i=2 40%, ...)
  ↓ DamageSystem.DealDamage 호출 (PIVOT v4 OnHit + crit 자동)
```

### 4.2 메이플 키우기 다중 타깃 보정 (Little Hero 풍)

| 동시 hit 수 | 배수 | 비고 |
|---|---|---|
| 1마리 | 100% | 일반 |
| 2마리 | 80% | -20% |
| 3마리 | 70% | -30% |
| 4마리 | 60% | -40% (cap) |
| 5+ | 60% | cap 유지 |

**Little Hero 조정 이유**:
- 메이플 키우기 26% 보정은 너무 가혹 (idle farming 메인)
- Little Hero 는 active gameplay → 60% cap 으로 완화
- ExtraAttackTargets 스탯 (메이플 채널) 으로 추가 타깃 시 보정 동일 적용

### 4.3 PlayerStats 통합
- **CritChance** (이미 5% baseline) + **CritDamage** 1.5× — DamageSystem 자동 적용 (PIVOT v4)
- **AttackSpeed** cap 150% — DamageQuery.SetAttackSpeed 에서 Mathf.Clamp(speed, 0.1f, 1.5f)
- **ExtraAttackTargets** — DamageQuery.maxTargets 가산
- **AdditiveDamageBonus / FinalDamageBonus** — DamageDealer.ApplyMulti 내부 버킷 합산

## 5. 신규 클래스 설계

### 5.1 `DamageQuery` (정적 헬퍼)
```csharp
public static class DamageQuery {
    private static readonly Collider2D[] _buffer = new Collider2D[64];
    private static readonly List<EnemyController> _result = new List<EnemyController>(64);
    private static int _enemyLayerMask = -1;

    public static int FindEnemiesInCircle(Vector2 center, float radius, List<EnemyController> outResult) {
        if (_enemyLayerMask < 0) _enemyLayerMask = LayerMask.GetMask("Enemy");
        outResult.Clear();
        int count = Physics2D.OverlapCircleNonAlloc(center, radius, _buffer, _enemyLayerMask);
        for (int i = 0; i < count; i++) {
            var enemy = _buffer[i].GetComponent<EnemyController>();
            if (enemy != null && !enemy.IsDead) outResult.Add(enemy);
        }
        return outResult.Count;
    }

    public static EnemyController FindNearest(Vector2 center, float radius) {
        FindEnemiesInCircle(center, radius, _result);
        EnemyController nearest = null;
        float minSq = float.MaxValue;
        foreach (var e in _result) {
            float sq = ((Vector2)e.transform.position - center).sqrMagnitude;
            if (sq < minSq) { minSq = sq; nearest = e; }
        }
        return nearest;
    }

    public static int FindNearestN(Vector2 center, float radius, int n, List<EnemyController> outResult) {
        FindEnemiesInCircle(center, radius, outResult);
        outResult.Sort((a, b) => {
            float sa = ((Vector2)a.transform.position - center).sqrMagnitude;
            float sb = ((Vector2)b.transform.position - center).sqrMagnitude;
            return sa.CompareTo(sb);
        });
        if (outResult.Count > n) outResult.RemoveRange(n, outResult.Count - n);
        return outResult.Count;
    }
}
```

### 5.2 `DamageDealer` (정적 헬퍼)
```csharp
public static class DamageDealer {
    private static readonly float[] _multiTargetPenalty = { 1f, 0.8f, 0.7f, 0.6f, 0.6f };

    public static void ApplyMulti(List<EnemyController> targets, float baseDamage) {
        for (int i = 0; i < targets.Count; i++) {
            float penalty = i < _multiTargetPenalty.Length ? _multiTargetPenalty[i] : 0.6f;
            DamageSystem.DealDamage(targets[i], baseDamage * penalty);
        }
    }

    public static void ApplySingle(EnemyController target, float damage) {
        DamageSystem.DealDamage(target, damage);
    }
}
```

### 5.3 LayerMask 설정 (Unity Editor)
- `Edit > Project Settings > Physics2D > Layer Collision Matrix`
- 신규 Layer "Enemy" 추가 (Layer 8)
- 기존 EnemyController prefab 의 Layer = "Enemy" 강제
- Player Layer 와 Enemy Layer 충돌 = OFF (적-탄환 충돌 폐기)

## 6. 7 패턴 마이그레이션

### 6.1 변경 표

| 패턴 | 현재 | v2 변경 | 다중 타깃 |
|---|---|---|---|
| **Projectile** | Rigidbody2D 이동 + OnTriggerEnter | Tick 매 프레임 OverlapCircleNonAlloc(자기 위치, 0.3) → 적 발견 시 ApplySingle + 자기 비활성 | 단일 (pierceCount 시 maxTargets) |
| **Aura** | Collider2D Trigger + OnTriggerStay | Tick 매 프레임 OverlapCircleNonAlloc(player, _radius) → ApplyMulti (delay 적용) | 다중 (제한 없음) |
| **Pulse** | OnTriggerEnter + AOE | Tick interval 시 OverlapCircleNonAlloc(player, _radius) → ApplyMulti | 다중 |
| **Trail** | TrailField Trigger | TrailField Tick OverlapCircleNonAlloc → ApplyMulti | 다중 |
| **Orbit** | Collider2D Trigger | OrbitOrb Tick OverlapCircleNonAlloc(자기, 0.5) → ApplySingle | 단일 |
| **Rain** | Projectile 답습 | Projectile 패턴 그대로 | 단일 |
| **Summon** | Companion Trigger | Companion Tick FindNearest → ApplySingle | 단일 |

### 6.2 마이그레이션 사이클
- **사이클 1** — 신규 클래스 (DamageQuery + DamageDealer + LayerMask 설정)
- **사이클 2** — Projectile 전환 (가장 큰 비중) + Rain
- **사이클 3** — Aura/Pulse/Trail (다중 타깃 핵심)
- **사이클 4** — Orbit/Summon (단일 타깃)
- **사이클 5** — 검증 (Profiler GC + frame budget)

## 7. 성능 비교 (예상)

| 지표 | v1 (충돌) | v2 (Query) |
|---|---|---|
| **GC/sec** | ~50KB (페어 cache) | **~5KB** (-90%) |
| **Physics2D broadphase** | 16k 페어/frame | 0 (Layer 분리) |
| **OverlapCircle 비용** | N/A | ~0.3ms (65 query × 5μs) |
| **frame budget** | ~3ms | **~1ms** (-67%) |

## 8. 메이플 키우기 다중 타깃 시각 효과

각 타깃 hit 시 PIVOT v4 OnHit 자동 호출 — 다중 타깃 시:
- 1차 타깃: 100% 데미지 = 큰 흰 숫자
- 2차+: 80~60% 데미지 = 작은 흰 숫자
- Critical 5% 시 골드 큰 숫자 + slow-mo

다중 타깃 정렬: 거리순 (가까운 적 먼저). 시각적으로 자연스러운 "wave" 데미지.

## 9. 구현 위험 + 완화

| 위험 | 완화 |
|---|---|
| LayerMask 미설정 시 모든 적 발견 X | 사이클 1 에서 Editor 자동 설정 + 검증 |
| OverlapCircleNonAlloc buffer 64 부족 | Cap 300 의 일부 검색 → 64 충분, 부족 시 buffer 확장 |
| Projectile 시각 (Rigidbody2D 제거 후 이동) | Tick 매 프레임 Vector3 += direction × speed × dt |
| 충돌 폐기 후 적이 player 충돌 시 데미지 X | 별도 PlayerDamageReceiver 패턴 유지 (보수적) |
| 다중 타깃 정렬 GC | List<EnemyController> 정적 buffer + Sort in-place |

## 10. 변경 이력

- 2026-05-03: 초기 작성 (v2 충돌 폐기 + 메이플 키우기 다중 타깃)

## 11. 다음 단계

1. **본 문서 사용자 검토** + 결정 (즉시 사이클 1 / 추가 조정)
2. **사이클 1 구현** — DamageQuery + DamageDealer + LayerMask
3. **사이클 2~4** — 7 패턴 마이그레이션
4. **사이클 5** — Profiler GC + frame budget 검증
