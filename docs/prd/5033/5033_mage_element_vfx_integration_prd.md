# 5033 Sprint PRD — mage_element_vfx_integration

> **PRD v1 (초안)** · 디자인: `docs/prd/5033_mage_element_vfx_integration_prd.md`
> 옵션 A 채택 — 4 element 1 prefab 매핑

---

## 1. 전체 개요

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 4 prefab variant 생성 + ElementVfxBank Bootstrap | 1 | 30분 |
| 2 | ElementVfxBank.Spawn 분기 + DamageDealer hook | 1 | 30분 |
| 3 | NUnit 회귀 + changelog + CLAUDE.md | 1 | 15분 |
| **합계** | | **3** | **~1h 15m** |

**참고**: Sprint 3a (사용자 Play Mode 시각 검증) 는 Sprint 1+2 commit 후 사용자 환경 검증 — PRD scope 외.

---

## 2. Sprint 1 — Resources prefab variant + Bootstrap (1 commit, 30분)

### 2.1 신규 prefab variant 4

**위치**: `LittleHero/Assets/_Project/Resources/VFX/Element/`

| 파일 | 원본 (Hovl) | element |
|---|---|---|
| `Fire.prefab` | `_ThirdParty/.../AoE effects/Red energy explosion.prefab` | Fire |
| `Ice.prefab` | `_ThirdParty/.../AoE effects/Snow AOE.prefab` | Ice |
| `Lightning.prefab` | `_ThirdParty/.../Hits and explosions/Electro hit.prefab` | Lightning |
| `Arcane.prefab` | `_ThirdParty/.../Magic circles/Magic circle.prefab` | Arcane |

**생성 방법** — 사용자 Unity Editor 작업:
```
1. Project 창에서 Hovl prefab 선택 (예 Red energy explosion)
2. 우클릭 → Create → Prefab Variant
3. 새 prefab 을 Resources/VFX/Element/ 로 이동
4. Rename: "Fire.prefab"
5. 4 element 반복
```

**대안** — Claude Editor 도구 자동 생성 (`Tools/LittleHero/Setup Element VFX Variants`):
- AssetDatabase.CopyAsset 또는 PrefabUtility.SaveAsPrefabAsset
- 4 prefab 일괄 생성

### 2.2 ElementVfxBank 갱신

```csharp
public static class ElementVfxBank
{
    private static GameObject _firePrefab;
    private static GameObject _icePrefab;
    private static GameObject _lightningPrefab;
    private static GameObject _arcanePrefab;
    private static bool _initialized;

    public static void EnsureLoaded()
    {
        if (_initialized) return;
        _firePrefab      = Resources.Load<GameObject>("VFX/Element/Fire");
        _icePrefab       = Resources.Load<GameObject>("VFX/Element/Ice");
        _lightningPrefab = Resources.Load<GameObject>("VFX/Element/Lightning");
        _arcanePrefab    = Resources.Load<GameObject>("VFX/Element/Arcane");
        _initialized = true;
    }

    public static GameObject GetPrefab(MagicElement element) {
        EnsureLoaded();
        return element switch {
            MagicElement.Fire      => _firePrefab,
            MagicElement.Ice       => _icePrefab,
            MagicElement.Lightning => _lightningPrefab,
            MagicElement.Arcane    => _arcanePrefab,
            _ => null,
        };
    }

    public static Color GetColor(MagicElement element) => /* 5029 동일 */;
}
```

### 2.3 검증
- 컴파일 errors=0
- 4 prefab Resources/VFX/Element/ 위치 확인
- ElementVfxBank.GetPrefab(MagicElement.Fire) != null

#### Sprint 1 commit

```
5033 Sprint 1 — Resources prefab variant + ElementVfxBank Bootstrap (1/3)

[변경]
  + Resources/VFX/Element/Fire.prefab      (Red energy explosion variant)
  + Resources/VFX/Element/Ice.prefab       (Snow AOE variant)
  + Resources/VFX/Element/Lightning.prefab (Electro hit variant)
  + Resources/VFX/Element/Arcane.prefab    (Magic circle variant)
  - Core/ElementVfxBank.cs 갱신
    + EnsureLoaded() — Resources.Load 4 prefab
    + GetPrefab(MagicElement) → 분기

[검증]
  - 4 prefab Resources/VFX/Element/ 정합
  - GetPrefab(Fire/Ice/Lightning/Arcane) != null

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 3. Sprint 2 — Spawn 분기 + DamageDealer hook (1 commit, 30분)

### 3.1 ElementVfxBank.Spawn 갱신

```csharp
public static void Spawn(MagicElement element, Vector3 position)
{
    if (element == MagicElement.None) return;
    var prefab = GetPrefab(element);
    if (prefab == null) return;
    var instance = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
    UnityEngine.Object.Destroy(instance, 3f);  // 3초 후 자동 정리
}
```

### 3.2 ElementResolver.ApplyElementEffect hook

```csharp
public static void ApplyElementEffect(EnemyController target, MagicElement element, float baseDamage)
{
    if (target == null || target.IsDead) return;
    if (element == MagicElement.None) return;

    // 5033 — element VFX spawn (placeholder StarBurst → Hovl prefab)
    ElementVfxBank.Spawn(element, target.transform.position);

    switch (element) { /* 5029 동일 */ }
}
```

### 3.3 검증
- Play Mode 마법사 무기 활성화 시 element VFX 발동
- 적 처치 시 Fire prefab spawn (Red energy explosion 시각)
- 3초 후 instance Destroy (메모리 누수 X)

#### Sprint 2 commit

```
5033 Sprint 2 — ElementVfxBank.Spawn 분기 + ElementResolver hook (2/3)

[변경]
  - Core/ElementVfxBank.cs
    + Spawn(MagicElement, Vector3) — Hovl prefab Instantiate + 3s Destroy
  - Gameplay/Combat/ElementResolver.cs
    + ApplyElementEffect 진입 시 ElementVfxBank.Spawn 호출

[검증]
  - 마법사 무기 Fire/Ice/Lightning/Arcane 활성화 시 Hovl prefab 발동
  - 메모리 누수 0 (3s Destroy)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. Sprint 3 — NUnit + docs (1 commit, 15분)

### 4.1 NUnit 회귀

ElementResolverTests 6 NUnit 회귀 0 (시그니처 보존).

선택 — 신규 ElementVfxBankTests:
- `GetPrefab_Fire_NotNull` (Resources Load 검증)
- `Spawn_None_DoesNotThrow`

NUnit 누적: 178 → 180 (+2, 선택).

### 4.2 docs

- `changelog_v1.md` +5033
- `CLAUDE.md` §14 직전 5건

#### Sprint 3 commit

```
5033 Sprint 3 — NUnit 2 + changelog + CLAUDE.md (3/3)

[변경]
  + Tests/Editor/ElementVfxBankTests.cs (NUnit 2, 선택)
  - changelog_v1.md +5033
  - CLAUDE.md §14 갱신

[NUnit 누적] 178 → 180 (선택, +2)

[5033 종결]
  - Hovl Magic Effects FREE 통합 완료
  - 4 element 시각 효과 활성
  - 5029 placeholder → 실 prefab

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 5. 의존성 그래프

```
Sprint 1 (prefab variant + Bootstrap)
   ↓
Sprint 2 (Spawn 분기 + ElementResolver hook)
   ↓
Sprint 3 (NUnit + docs)
```

순차.

## 6. 완료 정의 (DoD)

- [ ] Resources/VFX/Element/{Fire,Ice,Lightning,Arcane}.prefab 4건
- [ ] ElementVfxBank.GetPrefab + Spawn 분기
- [ ] ElementResolver.ApplyElementEffect 진입 시 Hovl spawn
- [ ] 3s 자동 Destroy (메모리 누수 X)
- [ ] 컴파일 errors=0
- [ ] ElementResolverTests 6 회귀 0
- [ ] 사용자 Play Mode 시각 검증 (Sprint 1+2 후)

## 7. 검증 시나리오

### 시나리오 1 — Resources Load
Play Mode 진입 → Console: `[ElementVfxBank] EnsureLoaded` 또는 GetPrefab 호출 시 prefab 로드 확인

### 시나리오 2 — 마법사 element 시각
1. Attic → 마법사 캐릭터 → CH1 진입
2. weapon_firebottle (Fire) 활성화
3. 적 처치 시 → **Red energy explosion VFX 시각 발동** ★
4. 3초 후 자동 사라짐

### 시나리오 3 — URP 2D 호환
- 분홍/검정 색 표시 시 → URP material 변환 필요
- 정상 색감 → PASS

### 시나리오 4 — PG-12 톤
- Red energy explosion 만화풍 ✅ / 사실적 ❌ — 사용자 결정
- 미합격 시 prefab 교체 (Sprint cascade)

## 8. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Hovl prefab 3D mesh URP 2D 호환 | Sprint 3 사용자 검증 + Convert URP Materials |
| Resources prefab variant 원본 변경 시 부서짐 | Hovl 원본 변경 X (외부 에셋), variant 안전 |
| Instantiate + Destroy GC 발생 | 3s Destroy 단순화 (5033.1 후속 — VfxPool Register/Spawn 정착) |
| PG-12 톤 미합격 | 사용자 검증 후 prefab 교체 가능 (4 GameObject ref 만 변경) |

## 9. 5033.1 후속 backlog (선택)

- VfxPool Register/Spawn API 신규 (GC 0 정착)
- 9 무기 별 prefab 매핑 (시각 다양화 ↑)
- Sparks 보조 효과 추가 (sub-VFX)

## 10. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 옵션 A (4 element 1 prefab 매핑) |
