# Evaluator Checklist: 5033 mage_element_vfx_integration

> Phase 6 Evaluator 가 사용. Phase 5 Generator (구현자) 와 분리된 단일 진실원.
> Pass/Fail 자동 판정 (자동 검증) + 사용자 보고 (수동 시나리오).

---

## 자동 검증 (스크립트 / MCP 자동화)

### C1 컴파일 (3 항목)
- [ ] C1.1 T01 commit 후 `read_console` errors=0
- [ ] C1.2 T02 commit 후 `read_console` errors=0
- [ ] C1.3 T03 commit 후 `read_console` errors=0
- [ ] C1.4 `editor_state.isCompiling == false` (각 commit 후)
- [ ] C1.5 Unity 6 obsolete warning 신규 0 (5032 baseline 답습)

### C2 Editor Test (NUnit, 3+회귀)
- [ ] C2.1 `ElementVfxBankTests` 클래스 존재 (Reflection)
- [ ] C2.2 `GetPrefab_Fire_NotNull` PASS
- [ ] C2.3 `GetPrefab_None_ReturnsNull` PASS
- [ ] C2.4 `Spawn_None_DoesNotThrow` PASS
- [ ] C2.5 5029 `ElementResolverTests` 6 모두 PASS (회귀 0)
- [ ] C2.6 5031 `IAPServiceTests` 7 모두 PASS (회귀 0)
- [ ] C2.7 5032 `DamageQueryTests` 6 모두 PASS (회귀 0)
- [ ] C2.8 그 외 NUnit 모두 PASS (전체 회귀 0)
- [ ] C2.9 NUnit 누적 +3 (baseline 실측 + 3)

**자동 실행**:
```
mcp__UnityMCP__run_tests
# 또는 Test Runner > Edit Mode > Run All
```

### C3 ProjectValidator
- [ ] C3.1 `Tools/LittleHero/Validate Project` 실행
- [ ] C3.2 errors=0
- [ ] C3.3 warnings ≤ 172 (5023 baseline) — 변동 ±0
- [ ] C3.4 신규 100줄+ 위반 0
- [ ] C3.5 신규 public 필드 0
- [ ] C3.6 신규 Update() 0
- [ ] C3.7 신규 Singleton 직접 참조 0

### C4 Reflection 정적 검증
**ElementVfxBank**:
- [ ] C4.1 Type `ElementVfxBank` 존재
- [ ] C4.2 `SPAWN_LIFETIME_SECONDS` (private const float, 값 3f) 존재
- [ ] C4.3 `_firePrefab/_icePrefab/_lightningPrefab/_arcanePrefab` (private static GameObject) — **`[SerializeField]` 미부착**
- [ ] C4.4 `_initialized` (private static bool) 존재
- [ ] C4.5 `EnsureLoaded()` (public static void) 존재
- [ ] C4.6 `GetPrefab(MagicElement)` (public static GameObject) 존재
- [ ] C4.7 `Spawn(MagicElement, Vector3)` (public static void) 존재 (5029 시그니처)
- [ ] C4.8 `GetColor(MagicElement)` (public static Color) 존재 (5029 보존)
- [ ] C4.9 **`Bootstrap()` 또는 `[RuntimeInitializeOnLoadMethod]` 부착 필드/메서드 부재** (D3=B 결정)

**ElementResolver**:
- [ ] C4.10 Type `ElementResolver` 존재 (5029 보존)
- [ ] C4.11 `ApplyElementEffect(EnemyController, MagicElement, float)` 시그니처 보존
- [ ] C4.12 본문에 `ElementVfxBank.Spawn` 호출 존재 (정적 분석 / 코드 검색)

**SetupElementVfxVariants**:
- [ ] C4.13 Type `SetupElementVfxVariants` 존재
- [ ] C4.14 `Setup()` 메서드 + `[MenuItem("Tools/LittleHero/Setup Element VFX Variants")]` attribute
- [ ] C4.15 `_variants` (private static readonly) 4 element

**ElementVfxBankTests**:
- [ ] C4.16 Type `ElementVfxBankTests` 존재
- [ ] C4.17 3 메서드 모두 `[Test]` attribute 부착

### C5 Asset/Prefab 검증
- [ ] C5.1 `Assets/_Project/Resources/VFX/Element/Fire.prefab` 존재
- [ ] C5.2 Fire.prefab 의 source = Hovl Red energy explosion (variant)
- [ ] C5.3 `Assets/_Project/Resources/VFX/Element/Ice.prefab` 존재 + Snow AOE variant
- [ ] C5.4 `Assets/_Project/Resources/VFX/Element/Lightning.prefab` 존재 + Electro hit variant
- [ ] C5.5 `Assets/_Project/Resources/VFX/Element/Arcane.prefab` 존재 + Magic circle variant

**자동 실행**:
```csharp
foreach (var (name, sourceName) in new[] {("Fire","Red energy explosion"), ("Ice","Snow AOE"), ("Lightning","Electro hit"), ("Arcane","Magic circle")}) {
    var p = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Resources/VFX/Element/{name}.prefab");
    Assert.IsNotNull(p);
    var source = PrefabUtility.GetCorrespondingObjectFromSource(p);
    Assert.AreEqual(sourceName, source?.name);
}
```

---

## 수동 검증 (사용자 Play Mode)

### C6.1 시나리오 1 — Resources Load
- [ ] Play Mode 진입
- [ ] ElementVfxBank.GetPrefab 첫 호출 시 EnsureLoaded → 4 Resources.Load 실행
- [ ] Console 에러 0 (Resources/VFX/Element/* 누락 없음)

### C6.2 시나리오 2 — 마법사 element 시각 (★ 핵심)
- [ ] Attic → 마법사 캐릭터 선택
- [ ] CH1 진입
- [ ] weapon_firebottle (Fire) 활성화
- [ ] 적 처치 시 → Red energy explosion VFX 시각 발동 ★
- [ ] SPAWN_LIFETIME_SECONDS (3초) 후 자동 사라짐
- [ ] 적 ~100 처치 후 Hierarchy "*Element*(Clone)*" 잔존 0
- [ ] 4 element 모두 검증 (Fire / Ice / Lightning / Arcane)

### C6.3 시나리오 3 — URP 2D 호환
- [ ] 4 element prefab 색감 정상 (분홍/검정 X)
- [ ] FAIL 시: `Edit > Render Pipeline > Universal RP > Convert URP Materials` cascade fix

### C6.4 시나리오 4 — PG-12 톤
- [ ] Fire (Red energy explosion) 만화풍 합격 (별 흩어짐 / 색 강조)
- [ ] Ice (Snow AOE) 만화풍 합격
- [ ] Lightning (Electro hit) 만화풍 합격
- [ ] Arcane (Magic circle) 환상적 합격
- [ ] FAIL 시: 4 GameObject ref 교체 cascade fix

---

## 비기능 검증

- [ ] BG1 60fps 유지 (Profiler 권장 — iPhone 12 이상)
- [ ] BG2 GC 누적 — 적 ~4000/챕터 처치 시 측정 (D2=A 인지: 5033.1 cascade 예정)
- [ ] BG3 메모리 누수 0 (Hierarchy "*Element*" 잔존 0)
- [ ] BG4 5029 ElementResolver / ElementResolverTests 회귀 0
- [ ] BG5 5031 IAPService 회귀 0
- [ ] BG6 5032 DamageQuery / SceneRule 회귀 0

---

## 합격 판정

### 최소 합격 ✅
- C1 (컴파일) 모두 ✅
- C2 (Test) 모두 ✅
- C3 (Validator) 모두 ✅
- C4 (Reflection) 모두 ✅
- C5 (Asset) 모두 ✅
- C6.1 + C6.2 시나리오 PASS

### 완전 합격 ✅✅
- 최소 합격 + C6.3 + C6.4 모두 PASS
- 비기능 BG1~BG6 모두 ✅
- 5033.1 후속 backlog 갱신

### FAIL → Phase 5 재진입
- C1~C5 자동 검증 1개라도 ❌ → Evaluator 가 수정 지시 발행 (file:line 단위) → Generator 가 수정 → Phase 6 재검증 (max 2회 반복)
- C6 시나리오 FAIL → cascade fix (URP Convert / prefab 교체)

---

## Evaluator 실행 가이드

```bash
# 1. 자동 검증 일괄
mcp__UnityMCP__refresh_unity
sleep 10
mcp__UnityMCP__read_console
mcp__UnityMCP__run_tests
mcp__UnityMCP__execute_menu_item "Tools/LittleHero/Validate Project"

# 2. C4 Reflection 검증 (script)
mcp__UnityMCP__execute_code -- C# script with reflection assertions

# 3. C5 Asset 검증 (script)
mcp__UnityMCP__execute_code -- AssetDatabase.LoadAssetAtPath × 4

# 4. C6 시나리오 보고 — 사용자 Play Mode 후 보고
```

---

## 변경 이력
- 2026-05-06: 초기 작성 (Phase 4 Contract Checklist)
