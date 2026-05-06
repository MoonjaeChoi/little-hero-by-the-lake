# 5032 unity6_api_migration — 디자인 문서

> **목적**: Unity 6.4 deprecated API 마이그레이션 — Physics2D.OverlapCircleNonAlloc + FindObjectsSortMode + 1 unused field
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5031 T03 컴파일 검증에서 발견된 obsolete warning — 5032 정식 마이그레이션 |

## 1. 현재 시스템 분석

### 1.1 obsolete warning 분포 (사용자 보고)

| Warning | 위치 | 영향 |
|---|---|---|
| `Physics2D.OverlapCircleNonAlloc(Vector2, float, Collider2D[], int)` | DamageQuery.cs:31 | 핵심 — 데미지 query 매 frame |
| `Physics2D.OverlapCircleNonAlloc(Vector2, float, Collider2D[])` | ProjectileAttack.cs:82 | Projectile pierce 검사 |
| `Object.FindObjectsByType(Type, FindObjectsSortMode)` | SceneRule.cs:99 | Validator (Editor only) |
| `FindObjectsSortMode.None` | SceneRule.cs:99 | Validator (Editor only) |
| `EndgameMenuPanel._bossEncounterSceneName` unused (CS0414) | EndgameMenuPanel.cs:35 | 5005 잔존 (5031 cascade 정리 가능) |

### 1.2 Unity 6.4 신 API (공식 가이드 + PackageCache 검증)

**Physics2D.OverlapCircle (v6 권장)**:
```csharp
// Modern: ContactFilter2D + Collider2D[] (NonAlloc 대체)
public static int OverlapCircle(Vector2 point, float radius, 
    ContactFilter2D contactFilter, Collider2D[] results);
```

ContactFilter2D 는 `useTriggers` / `useLayerMask` / `layerMask` 등 구조체 — 정적 init 후 재사용 (GC 최소화).

**FindObjectsByType (v6 권장)**:
```csharp
// FindObjectsSortMode 폐기 → FindObjectsInactive 사용
public static Object[] FindObjectsByType(Type, FindObjectsInactive);
// 또는 단순
public static Object[] FindObjectsByType(Type);
```

### 1.3 영향 범위

| 영역 | 파일 | 변경 |
|---|---|---|
| Combat (hot path) | DamageQuery.cs | OverlapCircle + ContactFilter2D 정적 |
| Combat | ProjectileAttack.cs | OverlapCircle + ContactFilter2D 정적 |
| Editor Validator | SceneRule.cs | FindObjectsByType(Type) 단순 |
| UI | EndgameMenuPanel.cs | unused field 제거 |

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | 신규 v6 API 전면 적용 + ContactFilter2D 정적 init | warning 0, GC 0 | DamageQuery 시그니처 미세 변경 |
| B | `#pragma warning disable` suppress | 0 작업 | warning 가시성 ↓, 향후 v7 폐기 부담 |
| C | 점진 — DamageQuery 만 fix (SceneRule 후속) | 부분 | 일관성 ↓ |

★ Claude 권장: **A** — 5031 답습 (전면 마이그레이션), v6 GC 0 보장 (ContactFilter2D 구조체 재사용)

## 3. 권장 설계 (옵션 A)

### 3.1 DamageQuery.cs 변경

**기존** (deprecated):
```csharp
public static int LayerMask = ...;
private static readonly Collider2D[] _buffer = new Collider2D[64];

public static int FindEnemiesInCircle(Vector2 center, float radius, List<EnemyController> outResult) {
    int count = Physics2D.OverlapCircleNonAlloc(center, radius, _buffer, EnemyMask);
    // ...
}
```

**신 (v6)**:
```csharp
private static readonly Collider2D[] _buffer = new Collider2D[64];
private static ContactFilter2D _filter;
private static bool _filterInit;

public static int FindEnemiesInCircle(Vector2 center, float radius, List<EnemyController> outResult) {
    EnsureFilter();
    int count = Physics2D.OverlapCircle(center, radius, _filter, _buffer);
    // ...
}

private static void EnsureFilter() {
    if (_filterInit) return;
    _filter = new ContactFilter2D();
    _filter.SetLayerMask(EnemyMask);
    _filter.useTriggers = true;
    _filterInit = true;
}
```

### 3.2 ProjectileAttack.cs 변경

동일 패턴 — ContactFilter2D 정적 + OverlapCircle.

### 3.3 SceneRule.cs 변경

```csharp
// Before: FindObjectsByType(cmType, FindObjectsSortMode.None)
// After:
Object[] cms = Object.FindObjectsByType(cmType, FindObjectsInactive.Exclude);
```

### 3.4 EndgameMenuPanel.cs 변경

`_bossEncounterSceneName` unused field 제거 (5005 잔존 cleanup).

### 3.5 §15 정합

- §15.3 GC 최소화: ContactFilter2D 정적 init + 재사용 ✅
- §15.6 100L: 각 파일 변경 작음 (라인 추가 ~5L 미만)
- §15.11 안티 패턴: GC alloc 회피

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | DamageQuery + ProjectileAttack v6 OverlapCircle (hot path) | 1 | 30분 |
| 2 | SceneRule + EndgameMenuPanel v6 cleanup | 1 | 15분 |
| 3 | NUnit (DamageQueryTests 회귀) + Validator + changelog + CLAUDE.md | 1 | 15분 |
| **합계** | | **3** | **~1h** |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| ContactFilter2D `useTriggers` 기본값 변경 | 명시적 `useTriggers = true` (기존 OverlapCircleNonAlloc 동작 답습) |
| EnemyMask layerMask SetLayerMask vs layerMask 직접 set | `_filter.SetLayerMask(EnemyMask)` 사용 (공식 권장) |
| DamageQuery 시그니처 호환 | 외부 시그니처 보존 (FindEnemiesInCircle / FindNearest 등) |
| DamageQueryTests 6 NUnit 회귀 | 시그니처 보존, 회귀 0 추정 |
| Editor Inactive vs Sorted (SceneRule) | 기존 `FindObjectsSortMode.None` = unsorted, v6 `FindObjectsInactive.Exclude` 답습 |

## 6. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 A 권장, 3 sprint / 1h |

## 7. Sources

- Unity 6.4 docs: docs.unity3d.com/6000.4/.../Physics2D.OverlapCircle.html
- 5031 T03 컴파일 보고 (warning 발견)
- 5024 DamageQueryTests (회귀 안전망)
