# 5026 Sprint PRD v2 — boss_attack_type_migration

> **PRD v2 (최종, Mediator 확정)** · D1=B, D2=A, D3=A

→ 본문은 `5026_boss_attack_type_migration_prd.md` 와 동일 + D1=B 매핑 갱신.

## D1=B V1 11종 시각 정합 매핑

```csharp
private static readonly Dictionary<string, BossAttackType> V1NameMap = new() {
    // Animal (Charge) — 동물형
    { "Enemy_Bear",         BossAttackType.Charge },
    { "Enemy_BigWolf",      BossAttackType.Charge },
    { "Enemy_Rabbit",       BossAttackType.Charge },
    // Nature (Vine) — 식물·사물
    { "Enemy_BeanGiant",    BossAttackType.Vine   },
    { "Enemy_CloudGiant",   BossAttackType.Vine   },
    { "Enemy_Pumpkin",      BossAttackType.Vine   },
    // Civ (Pulse) — 카드
    { "Enemy_QueenHearts",  BossAttackType.Pulse  },
    { "Enemy_CardSoldier",  BossAttackType.Pulse  },
    // Meta (Summon) — 별·곤충·기본
    { "Enemy_StarCloud",    BossAttackType.Summon },
    { "Enemy_Firefly",      BossAttackType.Summon },
    { "Enemy_Basic",        BossAttackType.Summon },
};
```

`ResolveByName` 내부에서 V1NameMap 우선 lookup, miss 시 V2 archetype regex.

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | v2 (Mediator). D1=B + D2=A + D3=A |
