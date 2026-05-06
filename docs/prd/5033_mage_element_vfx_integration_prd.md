# 5033 mage_element_vfx_integration — 디자인 문서

> **목적**: Hovl Studio Magic Effects FREE 통합 — 5029 ElementVfxBank placeholder → 실 prefab
> **상태**: PRD (디자인 + Sprint)
> **시작**: 2026-05-06

## 0. 결정 이력

| 일자 | 결정 |
|---|---|
| 2026-05-06 | 5029.2 backlog 정식 진행. Hovl Studio Magic Effects FREE 무료 에셋 채택 (사용자 검색 + 결정) |
| 2026-05-06 | 위치: `Assets/_ThirdParty/HovlStudio/MagicEffectsFree/` 분리 (CLAUDE.md §3 정합) |

## 1. 현재 시스템 분석

### 1.1 Hovl Magic Effects FREE — 76 prefab 식별 완료

| 폴더 | 개수 | 핵심 prefab |
|---|---|---|
| AoE effects | 13 | Snow AOE / Meteors AOE / Red energy explosion / Laser AOE |
| Character auras | 7 | Buff / Debuff / Lightning aura / Star aura |
| Environment | 4 | Crystal effect (blue/green/red) + Teleport |
| Hits and explosions | 8 | Electro/Explosion/Snow/Star/Holy/Stones hit |
| Magic circles | 4 | Magic circle / Magic circle 2 / Healing / Freeze circle |
| Magic shields | 3 | blue/pink/yellow |
| Portals | 4 | blue/green/red/yellow |
| Slash effects | 6 | Charge slash + Electro/Snow/Stone slash |
| Smoke effects | 9 | (보조, 사용 X) |
| Sparks | 18 | flashing/explode/loop × 6 색상 |
| **합계** | **76** | |

### 1.2 5029 ElementVfxBank 현재 상태

```csharp
public static class ElementVfxBank {
    public static Color GetColor(MagicElement element) => ...;
    public static void Spawn(MagicElement element, Vector3 position) {
        if (element == MagicElement.None) return;
        VfxPool.SpawnStarBurst(position);  // placeholder
    }
}
```

placeholder — element 무관하게 SpawnStarBurst. 5033 에서 element 별 Hovl prefab 분기.

### 1.3 VfxPool API 한계

`Core/VfxPool.cs` 156L:
- SpawnWarning / SpawnStarBurst / SpawnHitStar 정적 — 특수 prefab 만
- 일반 `Register(id, prefab)` + `Spawn(id, position)` 미존재 → 신규 추가 필요

## 2. 옵션 비교

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | 4 element 1 prefab 매핑 + ElementVfxBank.Spawn 직접 Instantiate | 단순, 작업 1 commit | Pool 미사용 (GC 발생) |
| B | VfxPool 신규 Register/Spawn API + element 별 prefab 등록 | GC 0 (Pool), 5029 답습 | 작업 시간 ↑ |
| C | 9 무기 별 prefab 매핑 (Tome → Magic circle 등 무기 시각) | 시각 다양화 ↑ | 9 prefab × 매핑 복잡 |

★ Claude 권장: **A (즉시) → B (Sprint 2 cascade)** — 우선 시각 통합 + 후속 Pool 정착

## 3. 권장 설계

### 3.1 4 Element 매핑 (Top picking)

| Element | Hovl prefab | 경로 |
|---|---|---|
| **Fire** 🔥 | Red energy explosion | `_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/AoE effects/Red energy explosion.prefab` |
| **Ice** ❄️ | Snow AOE | `_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/AoE effects/Snow AOE.prefab` |
| **Lightning** ⚡ | Electro hit | `_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Hits and explosions/Electro hit.prefab` |
| **Arcane** ✨ | Magic circle | `_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Magic circles/Magic circle.prefab` |

### 3.2 ElementVfxBank 갱신 (Sprint 1)

```csharp
public static class ElementVfxBank {
    [SerializeField]
    private static GameObject _firePrefab;     // Red energy explosion
    private static GameObject _icePrefab;      // Snow AOE
    private static GameObject _lightningPrefab; // Electro hit
    private static GameObject _arcanePrefab;   // Magic circle

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap() {
        _firePrefab      = Resources.Load<GameObject>("VFX/Element/Fire");
        _icePrefab       = Resources.Load<GameObject>("VFX/Element/Ice");
        _lightningPrefab = Resources.Load<GameObject>("VFX/Element/Lightning");
        _arcanePrefab    = Resources.Load<GameObject>("VFX/Element/Arcane");
    }

    public static void Spawn(MagicElement element, Vector3 position) {
        var prefab = element switch {
            MagicElement.Fire      => _firePrefab,
            MagicElement.Ice       => _icePrefab,
            MagicElement.Lightning => _lightningPrefab,
            MagicElement.Arcane    => _arcanePrefab,
            _ => null,
        };
        if (prefab == null) return;
        var instance = Object.Instantiate(prefab, position, Quaternion.identity);
        Object.Destroy(instance, 3f);  // 3초 후 자동 정리
    }
}
```

### 3.3 Hovl prefab → Resources 폴더 link

**문제**: Hovl prefab 은 `_ThirdParty/HovlStudio/.../Prefabs/...` 위치. Resources.Load 는 `Resources/` 만 검색.

**해결**: 4 prefab 의 .meta 파일 GUID 를 사용하여 별도 Resources prefab wrap:
```
Assets/_Project/Resources/VFX/Element/
├── Fire.prefab          (Hovl Red energy explosion 참조 또는 instance 복제)
├── Ice.prefab           (Snow AOE)
├── Lightning.prefab     (Electro hit)
└── Arcane.prefab        (Magic circle)
```

또는 `AssetDatabase.LoadAssetAtPath` (Editor only) → 빌드 시 Resources copy.

권장: **prefab Variant** — 4 element prefab 을 Resources/VFX/Element/ 에 prefab variant 로 생성 (Hovl 원본 참조 보존).

### 3.4 §15 정합

- §15.3 GC 최소화: Sprint 1 단순 Instantiate (3초 destroy) → Sprint 2 Pool 권장
- §15.4 SO 우선: prefab Resources 로드 (정적 ref)
- §15.6 100L: ElementVfxBank ~50L
- §15.7 DI: Bootstrap 1회 init
- §15.9 Test: ElementVfxBank Resources 로드 NUnit (선택)

## 4. Sprint 분해

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | 4 prefab variant 생성 (Resources/VFX/Element/) + ElementVfxBank Bootstrap | 1 | 30분 |
| 2 | ElementVfxBank.Spawn → element prefab 분기 + DamageDealer/ElementResolver hook | 1 | 30분 |
| 3 | URP 2D 호환 + PG-12 시각 검증 (사용자 Play Mode) | (검증) | 사용자 시간 |
| 4 | NUnit 회귀 + changelog + CLAUDE.md | 1 | 15분 |
| **합계** | | **3** | **~1h 15m** + 사용자 검증 |

## 5. 알려진 위험 + 완화

| 위험 | 완화 |
|---|---|
| Hovl prefab 3D mesh — URP 2D 깨짐 | Sprint 3 사용자 시각 검증 + 깨짐 시 Convert URP Materials |
| Resources/VFX/Element/ prefab variant — Hovl 원본 변경 시 부서짐 | prefab variant 사용 (참조 + override 만) |
| GC 발생 (Instantiate + Destroy) | Sprint 2 cascade — VfxPool 정착 후 Pool 사용 |
| 앱 용량 (Hovl 36.4 MB) | 본 PRD 와 무관 (사용자 import 단계) |
| PG-12 톤 — Red energy explosion 강렬 | 사용자 검증 후 미합격 시 다른 prefab 교체 |

## 6. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 element 1 prefab 매핑 (옵션 A), 3 sprint / 1h15m |

## 7. Sources

- 5029 ElementVfxBank placeholder (5029.2 backlog)
- Hovl Studio Magic Effects FREE Asset Store
- 76 prefab grep 결과 — `_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/`
- Unity Resources.Load + prefab variant 패턴
