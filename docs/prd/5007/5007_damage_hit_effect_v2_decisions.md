# 5007 damage_hit_effect_v2 — Mediator Decisions (Phase 2.3)

> **사용자 결정**: 2026-05-04 "권장안대로" — Claude 권장안 4건 일괄 채택
> **Mode**: ai-assisted (기본값)

## 1. 결정 요약

| # | 주제 | 결정 | 영향 |
|---|---|---|---|
| **D1** | Sprint 3.2 commit 분할 | **B**: 2 commit 분할 (DamageDealer 인프라 + 8 IAttackPattern) | 11 → 12 commit, 시간 +10분 |
| **D2** | avgDamage 전달 위치 | **B**: DamageSystem 이 OnHit 인자로 전달 | EnemyController PlayerStats 의존성 0 |
| **D3** | Config prefab wiring | **A**: Resources.Load 정적 cache | 137 prefab wiring 부담 X |
| **D4** | cap 2.0× 위치 | **B**: Resolver 내부 Mathf.Min hardcode | 안전 cap, SO 잘못된 값 방어 |

## 2. PRD v2 적용 변경

### 2.1 Sprint 분해 (D1=B 채택)

**v1 (8 Sprint × 11 commit / 5h40m)**:
```
Sprint 3 — DamageSystem / DamageDealer 시그니처 확장 (2 commit)
  3.1 DamageSystem.DealDamage 시그니처 확장
  3.2 DamageDealer + 8 IAttackPattern 일괄 마이그레이션  ← 1 commit (큰)
```

**v2 (8 Sprint × 12 commit / 5h50m)**:
```
Sprint 3 — DamageSystem / DamageDealer 시그니처 확장 (3 commit)
  3.1 DamageSystem.DealDamage 시그니처 확장 (+ avgDamage 전달, D2)
  3.2 DamageDealer + EquipmentManager.GetActiveWeaponCategory 인프라 (분할 ①)
  3.3 8 IAttackPattern 일괄 마이그레이션 (분할 ②)
```

### 2.2 avgDamage 전달 (D2=B 채택)

**기존 시그니처**:
```csharp
DamageSystem.DealDamage(EnemyController enemy, float damage)
EnemyController.OnHit(float damage, bool isCrit)
HitEffectResolver.Resolve(cfg, enemyData, damage, isCrit, weaponCategory?, avgPlayerDamage?)
```

**v2 시그니처**:
```csharp
// DamageSystem 이 PlayerStats 한곳에서 query 후 전달
DamageSystem.DealDamage(EnemyController enemy, float damage, WeaponCategory? weaponCategory = null)
{
    // ... crit 판정, armor 등
    float avgDmg = PlayerStats.Instance?.AttackDamage ?? 0f;
    enemy.OnHit(finalDamage, isCrit, weaponCategory, avgDmg);
    // ...
}

EnemyController.OnHit(float damage, bool isCrit,
                     WeaponCategory? weaponCategory = null,
                     float avgPlayerDamage = 0f)
{
    var resolved = HitEffectResolver.Resolve(_config, _data, damage, isCrit, weaponCategory, avgPlayerDamage);
    // ... resolved 적용
}
```

### 2.3 Resources.Load 정적 cache (D3=A 채택)

**신규 파일**: `Scripts/Gameplay/Combat/CinematicEffectsConfigLoader.cs` (~20L)

```csharp
public static class CinematicEffectsConfigLoader
{
    private static CinematicEffectsConfig _cache;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        _cache = Resources.Load<CinematicEffectsConfig>("Combat/CinematicEffectsConfig");
        if (_cache == null)
            Debug.LogWarning("[CinematicEffectsConfigLoader] Resources/Combat/CinematicEffectsConfig 누락");
    }

    public static CinematicEffectsConfig Get() => _cache;
}
```

**EnemyController 사용**:
```csharp
private CinematicEffectsConfig Config => CinematicEffectsConfigLoader.Get();
```

> SO asset 위치: `Assets/_Project/Resources/Combat/CinematicEffectsConfig.asset` (Resources/ 하위 필수)

### 2.4 cap 2.0× hardcode (D4=B 채택)

**HitEffectResolver.Resolve 내부**:
```csharp
const float MAX_PUNCH_SCALE = 2.0f;  // PG-12 시각 멀미 방지 cap (D4=B 채택)
return new ResolvedHitEffect {
    PunchScale     = Mathf.Min(MAX_PUNCH_SCALE, basePunch * profile.punchMul * intensityFactor),
    // ...
};
```

## 3. v2 PRD 변경 요약

- §1 Sprint 표: 11 → 12 commit, 5h40m → 5h50m
- §2 Sprint 3: 2 commit → 3 commit (3.2 분할)
- §2 Sprint 4.2: 적 Prefab wiring → CinematicEffectsConfigLoader 정적 cache
- §3.2 신규 파일: CinematicEffectsConfigLoader.cs (~20L) 추가
- §3.4 §15 정합: §15.7 DI — Loader 정적, Initialize() 패턴 (Singleton 직접 X)
- HitEffectResolver: MAX_PUNCH_SCALE = 2.0f hardcode 명시

## 4. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.3 Mediator, "권장안대로", 4건 일괄 B/B/A/B)
