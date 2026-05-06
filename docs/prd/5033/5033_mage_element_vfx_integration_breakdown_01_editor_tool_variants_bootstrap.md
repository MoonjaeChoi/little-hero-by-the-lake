# T01: Editor 도구 + 4 prefab variant + ElementVfxBank Bootstrap

> **레이어**: Editor + Resources + Logic
> **의존성**: 없음 (5029 ElementResolver / VfxPool 기존 클래스 의존)
> **예상 시간**: ~35분 / 1 commit
> **PRD 출처**: `5033_mage_element_vfx_integration_v2.md` § 2 (Sprint 1)
> **Decisions 적용**: D1=B (Editor 도구) + D3=B (EnsureLoaded lazy) + R1 (SerializeField static 제거) + R2 (const SPAWN_LIFETIME_SECONDS)

---

## 1. 작업 항목

### 1.1 신규 파일 — `Editor/SetupElementVfxVariants.cs`

**경로**: `LittleHero/Assets/_Project/Scripts/Editor/SetupElementVfxVariants.cs`

**클래스 명세**:
```csharp
using UnityEditor;
using UnityEngine;
using System.IO;

public static class SetupElementVfxVariants
{
    private const string MENU_PATH = "Tools/LittleHero/Setup Element VFX Variants";
    private const string OUTPUT_DIR = "Assets/_Project/Resources/VFX/Element";

    private static readonly (string element, string sourcePath)[] _variants = new[]
    {
        ("Fire",      "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/AoE effects/Red energy explosion.prefab"),
        ("Ice",       "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/AoE effects/Snow AOE.prefab"),
        ("Lightning", "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Hits and explosions/Electro hit.prefab"),
        ("Arcane",    "Assets/_ThirdParty/HovlStudio/MagicEffectsFree/Prefabs/Magic circles/Magic circle.prefab"),
    };

    [MenuItem(MENU_PATH)]
    public static void Setup()
    {
        if (!Directory.Exists(OUTPUT_DIR))
            Directory.CreateDirectory(OUTPUT_DIR);

        int created = 0;
        foreach (var (element, sourcePath) in _variants)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null) {
                Debug.LogWarning($"[SetupElementVfxVariants] 원본 prefab 누락: {sourcePath}");
                continue;
            }

            var variantPath = $"{OUTPUT_DIR}/{element}.prefab";
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            PrefabUtility.SaveAsPrefabAsset(instance, variantPath);
            UnityEngine.Object.DestroyImmediate(instance);
            created++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[SetupElementVfxVariants] {created} prefab variant 생성 완료 → {OUTPUT_DIR}");
    }
}
```

**예상 라인**: ~50L (100줄 이내 ✅)
**§15 정합**: Editor 정적 도구 (DI 무관), 메뉴 패턴 (CLAUDE.md §16 답습)

### 1.2 신규 파일 — Resources/VFX/Element/ 4 prefab

**경로**: `LittleHero/Assets/_Project/Resources/VFX/Element/`

| 파일 | 원본 |
|---|---|
| `Fire.prefab` | Red energy explosion variant |
| `Ice.prefab` | Snow AOE variant |
| `Lightning.prefab` | Electro hit variant |
| `Arcane.prefab` | Magic circle variant |

**생성 방법**:
1. Unity Editor 에서 메뉴 `Tools/LittleHero/Setup Element VFX Variants` 실행
2. Console 로그 확인 — `[SetupElementVfxVariants] 4 prefab variant 생성 완료`
3. Project 창 — Resources/VFX/Element/ 4 prefab 존재 확인

### 1.3 갱신 파일 — `Core/ElementVfxBank.cs`

**경로**: `LittleHero/Assets/_Project/Scripts/Core/ElementVfxBank.cs`

**갱신 명세** (R1 + R2 + D3=B 적용):

```csharp
using UnityEngine;

public static class ElementVfxBank
{
    // R2: Magic Number → const
    private const float SPAWN_LIFETIME_SECONDS = 3f;

    // R1: SerializeField 제거 (static 무효)
    private static GameObject _firePrefab;
    private static GameObject _icePrefab;
    private static GameObject _lightningPrefab;
    private static GameObject _arcanePrefab;
    private static bool _initialized;

    // D3=B: EnsureLoaded lazy (Bootstrap RuntimeInitializeOnLoadMethod 폐기)
    public static void EnsureLoaded()
    {
        if (_initialized) return;
        _firePrefab      = Resources.Load<GameObject>("VFX/Element/Fire");
        _icePrefab       = Resources.Load<GameObject>("VFX/Element/Ice");
        _lightningPrefab = Resources.Load<GameObject>("VFX/Element/Lightning");
        _arcanePrefab    = Resources.Load<GameObject>("VFX/Element/Arcane");
        _initialized = true;
    }

    public static GameObject GetPrefab(MagicElement element)
    {
        EnsureLoaded();
        return element switch
        {
            MagicElement.Fire      => _firePrefab,
            MagicElement.Ice       => _icePrefab,
            MagicElement.Lightning => _lightningPrefab,
            MagicElement.Arcane    => _arcanePrefab,
            _ => null,
        };
    }

    // 5029 기존 메서드 — 보존
    public static Color GetColor(MagicElement element) => /* 5029 동일 */;

    // T02 에서 추가될 Spawn 메서드 자리
}
```

**예상 라인**: 갱신 후 ~70L (T02 의 Spawn 메서드 포함 시 ~85L, 100줄 이내 ✅)

**5029 보존 사항**:
- `GetColor(MagicElement)` 메서드 시그니처 그대로
- 기존 `Spawn` 메서드 시그니처 보존 (T02 에서 본격 구현)

---

## 2. 검증 기준

### 2.1 컴파일
- T01 commit 전: `mcp__UnityMCP__refresh_unity` + 10초 대기
- `mcp__UnityMCP__read_console` errors=0
- ElementVfxBank.cs / SetupElementVfxVariants.cs 컴파일 성공

### 2.2 Editor 도구 동작
- Unity Editor 메뉴 `Tools/LittleHero > Setup Element VFX Variants` 클릭
- Console: `[SetupElementVfxVariants] 4 prefab variant 생성 완료 → Assets/_Project/Resources/VFX/Element`
- Project 창: 4 prefab 존재 확인

### 2.3 Reflection (Phase 6 Contract 미리보기)
- Type `ElementVfxBank` 존재
- Field `SPAWN_LIFETIME_SECONDS` (private const float, 값 3f) 존재
- Method `EnsureLoaded()` (public static void) 존재
- Method `GetPrefab(MagicElement)` (public static GameObject) 존재
- Field `_firePrefab/_icePrefab/_lightningPrefab/_arcanePrefab` (private static GameObject) 존재 — `[SerializeField]` 미부착 확인

### 2.4 Resources Load (런타임 검증)
- `ElementVfxBank.EnsureLoaded()` 호출 후
- `ElementVfxBank.GetPrefab(MagicElement.Fire)` != null
- `ElementVfxBank.GetPrefab(MagicElement.Ice)` != null
- `ElementVfxBank.GetPrefab(MagicElement.Lightning)` != null
- `ElementVfxBank.GetPrefab(MagicElement.Arcane)` != null
- `ElementVfxBank.GetPrefab(MagicElement.None)` == null

### 2.5 ProjectValidator
- `Tools/LittleHero/Validate Project` 실행
- warnings 변동 0 (5023 baseline 대비)

### 2.6 회귀 (5029 보존)
- ElementResolver, ElementResolverTests 6 NUnit 변동 X (시그니처 보존)
- VfxPool API 변동 X
- ElementVfxBank.GetColor 동작 변동 X

---

## 3. commit 메시지

```
5033 Sprint 1 — Resources prefab variant + ElementVfxBank Bootstrap (1/3)

[변경]
  + Editor/SetupElementVfxVariants.cs (~50L, Tools/LittleHero 메뉴) [D1=B]
  + Resources/VFX/Element/Fire.prefab      (Red energy explosion variant)
  + Resources/VFX/Element/Ice.prefab       (Snow AOE variant)
  + Resources/VFX/Element/Lightning.prefab (Electro hit variant)
  + Resources/VFX/Element/Arcane.prefab    (Magic circle variant)
  - Core/ElementVfxBank.cs 갱신
    + SPAWN_LIFETIME_SECONDS const (R2)
    + EnsureLoaded() lazy (D3=B, Bootstrap RuntimeInitializeOnLoadMethod 폐기)
    + GetPrefab(MagicElement) → Resources prefab 분기
    - SerializeField 제거 (R1, static 무효)

[검증]
  - Editor 메뉴 실행 → 4 prefab 생성 확인
  - GetPrefab(Fire/Ice/Lightning/Arcane) != null
  - 컴파일 errors=0
  - 5029 ElementResolver 시그니처 보존 (NUnit 6 회귀 0)

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```

---

## 4. 관련 문서

- PRD v2 § 2 Sprint 1
- 디자인 § 3.2 ElementVfxBank
- Decisions D1=B (Editor 도구) / D3=B (EnsureLoaded lazy)
- Rebuttal R1 (SerializeField 제거) / R2 (const)
- CLAUDE.md §16 Editor 메뉴 패턴

---

## 5. 알려진 위험

| 위험 | 완화 |
|---|---|
| Hovl 원본 prefab 경로 변경 시 LoadAssetAtPath null | `_variants` 배열 hardcode + null 가드 + Console 경고 |
| Editor 메뉴 실행 누락 → prefab 0 → T02 검증 실패 | T01 작업 흐름에 메뉴 실행 단계 명시 + Project 창 확인 |
| Resources prefab variant 원본 변경 시 부서짐 | Hovl `_ThirdParty` 외부 에셋 변경 X (사용자 import 후 고정) |
| `[SerializeField] static` 잔존 | Reflection 검증으로 부착 여부 확인 (`HasCustomAttribute<SerializeField>` == false) |

---

## 6. Unity MCP 안전 패턴

```
1. SetupElementVfxVariants.cs 작성 (Edit/Write 도구)
2. ElementVfxBank.cs 갱신 (Edit 도구, 부분 변경)
3. mcp__UnityMCP__refresh_unity
4. 10초 대기 (Editor 도구 + Logic 갱신 — domain reload 큼)
5. mcp__UnityMCP__read_console (errors=0 확인)
6. Editor 메뉴 실행 (mcp__UnityMCP__execute_menu_item "Tools/LittleHero/Setup Element VFX Variants")
7. mcp__UnityMCP__read_console (Console 로그 확인)
8. Project 창 4 prefab 존재 확인 (find_in_file 또는 manage_asset)
9. git commit
10. 다음 태스크 진입 전 5초 추가 대기
```
