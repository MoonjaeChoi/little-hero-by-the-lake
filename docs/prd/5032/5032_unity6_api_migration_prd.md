# 5032 Sprint PRD — unity6_api_migration

> **PRD v1 (초안)** · 디자인: `docs/prd/5032_unity6_api_migration_prd.md`
> 옵션 A 채택 — 신규 v6 API 전면 적용

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | DamageQuery + ProjectileAttack v6 OverlapCircle | 1 | 30분 |
| 2 | SceneRule + EndgameMenuPanel cleanup | 1 | 15분 |
| 3 | NUnit 회귀 + changelog + CLAUDE.md | 1 | 15분 |
| **합계** | | **3** | **~1h** |

---

## 2. Sprint 1 — DamageQuery + ProjectileAttack v6 (1 commit, 30분)

### 2.1 DamageQuery.cs

**변경**: Physics2D.OverlapCircleNonAlloc → Physics2D.OverlapCircle (ContactFilter2D 정적)

```csharp
private static ContactFilter2D _filter;
private static bool _filterInit;

private static void EnsureFilter() {
    if (_filterInit) return;
    _filter = new ContactFilter2D();
    _filter.SetLayerMask(EnemyMask);
    _filter.useTriggers = true;
    _filterInit = true;
}

// FindEnemiesInCircle
EnsureFilter();
int count = Physics2D.OverlapCircle(center, radius, _filter, _buffer);
```

### 2.2 ProjectileAttack.cs

동일 패턴. Pierce 검사 시 ContactFilter2D 정적.

### 2.3 검증
- 컴파일 errors=0 + CS0618 warning 0 (Physics2D)
- DamageQueryTests 6 NUnit 회귀 0 (시그니처 보존)
- GC 0 (ContactFilter2D struct 재사용)

#### Sprint 1 commit
```
5032 Sprint 1 — DamageQuery + ProjectileAttack v6 OverlapCircle (1/3)

[변경]
  - Combat/DamageQuery.cs
    + ContactFilter2D _filter 정적 (EnsureFilter init)
    * Physics2D.OverlapCircleNonAlloc → OverlapCircle
  - Equipment/ProjectileAttack.cs 동일 패턴

[검증]
  - CS0618 OverlapCircleNonAlloc warning 0
  - DamageQueryTests 6 회귀 0
  - GC 0 (ContactFilter2D struct 재사용)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — SceneRule + EndgameMenuPanel cleanup (1 commit, 15분)

### 3.1 SceneRule.cs

```csharp
// Before
Object[] cms = Object.FindObjectsByType(cmType, FindObjectsSortMode.None);
// After
Object[] cms = Object.FindObjectsByType(cmType, FindObjectsInactive.Exclude);
```

### 3.2 EndgameMenuPanel.cs

`_bossEncounterSceneName` unused field 제거 (5005 잔존).

#### Sprint 2 commit
```
5032 Sprint 2 — SceneRule + EndgameMenuPanel v6 cleanup (2/3)

[변경]
  - Editor/ProjectValidator/Rules/SceneRule.cs
    * FindObjectsSortMode.None → FindObjectsInactive.Exclude
  - UI/EndgameMenuPanel.cs
    - _bossEncounterSceneName unused field 제거 (5005 잔존)

[검증]
  - CS0618 FindObjectsSortMode warning 0
  - CS0414 _bossEncounterSceneName warning 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — NUnit + docs (1 commit, 15분)

### 4.1 NUnit 회귀

DamageQueryTests 6 NUnit 회귀 0 검증 (코드 변경 0).

### 4.2 docs

- `changelog_v1.md` +5032
- `CLAUDE.md` §14 직전 5건

#### Sprint 3 commit
```
5032 Sprint 3 — NUnit + changelog + CLAUDE.md (3/3)

[검증]
  - DamageQueryTests 6 회귀 0
  - CS0618 + CS0414 warning 0

[변경]
  - changelog_v1.md +5032
  - CLAUDE.md §14 갱신

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. 의존성 그래프

```
Sprint 1 (DamageQuery + ProjectileAttack)
   ↓
Sprint 2 (SceneRule + EndgameMenuPanel)
   ↓
Sprint 3 (NUnit + docs)
```

순차.

## 6. 완료 정의 (DoD)

- [ ] DamageQuery + ProjectileAttack OverlapCircle (v6)
- [ ] ContactFilter2D 정적 init + 재사용 (GC 0)
- [ ] SceneRule FindObjectsInactive.Exclude
- [ ] EndgameMenuPanel _bossEncounterSceneName 제거
- [ ] CS0618 + CS0414 warning 0
- [ ] DamageQueryTests 6 회귀 0
- [ ] changelog + CLAUDE.md

## 7. 검증 시나리오

### 시나리오 1 — 컴파일
errors=0 + CS0618 + CS0414 warning 0

### 시나리오 2 — Play Mode 데미지 검증
- Chapter 1 진입 → 적 처치 → 콘솔 오류 0 + GC 변화 0

### 시나리오 3 — Editor Validator
Tools/LittleHero/Validate Project → SceneRule 동작 정상 (Cinemachine 검증)

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| ContactFilter2D useTriggers 기본 false → 트리거 적 missed | 명시적 useTriggers=true (기존 OverlapCircleNonAlloc 답습) |
| _filter.SetLayerMask vs layerMask 직접 set | SetLayerMask 메서드 (공식 권장) |
| DamageQuery 시그니처 외부 호환 | FindEnemiesInCircle / FindNearest 시그니처 보존 |

## 9. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 A (3 sprint / 1h) |
