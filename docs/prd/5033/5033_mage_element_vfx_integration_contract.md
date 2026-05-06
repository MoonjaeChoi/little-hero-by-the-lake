# Sprint Contract: 5033 mage_element_vfx_integration

> **PGE Planner — Phase 4 Contract**
> PRD: `5033_mage_element_vfx_integration_v2.md` (mediator 확정) · breakdown: `5033_mage_element_vfx_integration_breakdown_00.md`
> 생성일: 2026-05-06 · Generator (Phase 5) 와 분리된 단일 진실원

---

## 0. Scope 명세

| 시스템 | Hovl Magic Effects FREE 4 prefab → ElementVfxBank 통합 |
|---|---|
| 신규 파일 | 6 (Editor 1 + Resources 4 + Test 1) |
| 수정 파일 | 2 (ElementVfxBank.cs / ElementResolver.cs) |
| 신규 commit | 3 (Sprint 1 / 2 / 3) |
| 결정 사항 | D1=B Editor 도구 / D2=A Sprint cascade / D3=B EnsureLoaded lazy |

---

## 1. C1 컴파일 기준

- **C1.1**: T01 commit 후 신규 .cs 2 (`Editor/SetupElementVfxVariants.cs` + 갱신된 `Core/ElementVfxBank.cs`) errors=0
- **C1.2**: T02 commit 후 갱신 .cs 2 (`Core/ElementVfxBank.cs` + `Gameplay/Combat/ElementResolver.cs`) errors=0
- **C1.3**: T03 commit 후 신규 .cs 1 (`Tests/Editor/ElementVfxBankTests.cs`) errors=0
- **C1.4**: 도메인 리로드 안정화 (`editor_state.isCompiling == false`)
- **C1.5**: warning 변동 — Unity 6 obsolete 경고 신규 0 (5032 baseline 정합)

**검증 방법**:
```
mcp__UnityMCP__refresh_unity
sleep 5~10  # T01 후 10s, T02/T03 후 5s
mcp__UnityMCP__read_console (errors + warnings)  # errors=0 필수
```

---

## 2. C2 Editor Test 기준 (NUnit)

### 2.1 신규 Test 클래스

| Test 클래스 | 신규 메서드 수 | 목적 |
|---|---|---|
| `ElementVfxBankTests` | **3** | Resources Load + None 가드 + Spawn 안전성 |

### 2.2 ElementVfxBankTests 명세

- **`GetPrefab_Fire_NotNull`** — `ElementVfxBank.GetPrefab(MagicElement.Fire) != null` (T01 Resources/VFX/Element/Fire.prefab Load 검증)
- **`GetPrefab_None_ReturnsNull`** — `ElementVfxBank.GetPrefab(MagicElement.None) == null` (switch default 분기)
- **`Spawn_None_DoesNotThrow`** — `Assert.DoesNotThrow(() => ElementVfxBank.Spawn(MagicElement.None, Vector3.zero))` (early return 가드)

### 2.3 회귀 — 기존 NUnit

| 기존 클래스 | 메서드 수 | 회귀 기대 |
|---|---|---|
| `ElementResolverTests` (5029) | 6 | **모두 PASS** (시그니처 보존, ApplyElementEffect 1줄 추가만) |
| `IAPServiceTests` (5031) | 7 | 모두 PASS (변동 X) |
| `DamageQueryTests` (5032) | 6 | 모두 PASS (변동 X) |
| 그 외 NUnit | ~150 | 모두 PASS (변동 X) |

### 2.4 NUnit 누적 변동

- baseline (5032 직후): 실측 필요. PRD 명시 178.
- T03 후: baseline + 3 (= PRD 명시 181)

> **참고**: CLAUDE.md §14 의 NUnit 카운트 (5031 171, 5032 회귀 0) 와 PRD 명시 178 이 불일치 — Phase 6 Evaluate 시 실측 baseline 으로 보정.

**검증 방법**:
- Test Runner > Edit Mode > Run All → 신규 3 PASS + 기존 회귀 0
- 또는 `mcp__UnityMCP__run_tests --filter "ElementVfxBankTests"` + 기존 회귀 별도 실행

---

## 3. C3 ProjectValidator 기준

- **C3.1**: errors=0 (5023 baseline 답습)
- **C3.2**: warnings 변동 ±0 (baseline 172, OOPPolicyRule)
- **C3.3**: 신규 위반 0건
  - SetupElementVfxVariants ~50L (100줄 룰 ✅)
  - ElementVfxBank 누적 ~85L (100줄 룰 ✅)
  - ElementVfxBankTests ~25L (100줄 룰 ✅)
  - public 필드 신규 0
  - Update() 신규 0
  - Singleton 직접 참조 신규 0

**검증 방법**: `Tools/LittleHero/Validate Project` 메뉴 실행 → 콘솔 출력 5023 baseline (172 warnings) 와 비교.

---

## 4. C4 Reflection 검증 (정적)

### 4.1 ElementVfxBank Type 검증

| 항목 | 기대 | 검증 위치 |
|---|---|---|
| Type `ElementVfxBank` (public static class) | 존재 | T01 |
| Field `SPAWN_LIFETIME_SECONDS` (private const float) | 값 `3f` (R2) | T01 |
| Field `_firePrefab` (private static GameObject) | 존재, **`[SerializeField]` 미부착** (R1) | T01 |
| Field `_icePrefab/_lightningPrefab/_arcanePrefab` | 동일 (R1) | T01 |
| Field `_initialized` (private static bool) | 존재 | T01 |
| Method `EnsureLoaded()` (public static void) | 존재 (D3=B) | T01 |
| Method `GetPrefab(MagicElement)` (public static GameObject) | 존재, switch 4 분기 | T01 |
| Method `Spawn(MagicElement, Vector3)` (public static void) | 5029 시그니처 보존 + 본체 갱신 | T02 |
| Method `GetColor(MagicElement)` (public static Color) | 5029 보존 | (변동 X) |
| Method `Bootstrap()` 또는 `[RuntimeInitializeOnLoadMethod]` | **존재 X** (D3=B 결정 — 폐기) | T01 |

### 4.2 ElementResolver Type 검증

| 항목 | 기대 | 검증 위치 |
|---|---|---|
| Type `ElementResolver` (5029 보존) | 존재 | T02 |
| Method `ApplyElementEffect(EnemyController, MagicElement, float)` | 5029 시그니처 보존 | T02 |
| 본문 정적 분석 — `ElementVfxBank.Spawn(element, target.transform.position)` 호출 존재 | 1줄 추가 | T02 |

### 4.3 SetupElementVfxVariants Type 검증

| 항목 | 기대 | 검증 위치 |
|---|---|---|
| Type `SetupElementVfxVariants` (public static class) | 존재 | T01 |
| Method `Setup()` (`[MenuItem("Tools/LittleHero/Setup Element VFX Variants")]`) | 존재 + MenuItem attribute | T01 |
| Field `_variants` (private static readonly tuple array) | 4 element | T01 |

### 4.4 ElementVfxBankTests Type 검증

| 항목 | 기대 | 검증 위치 |
|---|---|---|
| Type `ElementVfxBankTests` | 존재 | T03 |
| Method `GetPrefab_Fire_NotNull` (`[Test]`) | 존재 | T03 |
| Method `GetPrefab_None_ReturnsNull` (`[Test]`) | 존재 | T03 |
| Method `Spawn_None_DoesNotThrow` (`[Test]`) | 존재 | T03 |

**검증 방법** (script):
```csharp
var t = System.Type.GetType("ElementVfxBank, Assembly-CSharp");
Assert.IsNotNull(t);
var spawnLifetime = t.GetField("SPAWN_LIFETIME_SECONDS",
    BindingFlags.NonPublic | BindingFlags.Static);
Assert.IsNotNull(spawnLifetime);
Assert.AreEqual(3f, spawnLifetime.GetValue(null));

var firePrefab = t.GetField("_firePrefab", BindingFlags.NonPublic | BindingFlags.Static);
Assert.IsFalse(firePrefab.GetCustomAttributes(typeof(SerializeField), false).Any());
```

---

## 5. C5 SO/Asset 검증

> **참고**: 5033 은 SO 신규 X, prefab 4건 신규.

### 5.1 Resources prefab 검증

| Asset | 경로 | 검증 |
|---|---|---|
| Fire.prefab | `Assets/_Project/Resources/VFX/Element/Fire.prefab` | 존재 + Hovl Red energy explosion variant (PrefabUtility.GetCorrespondingObjectFromSource != null) |
| Ice.prefab | `Assets/_Project/Resources/VFX/Element/Ice.prefab` | 존재 + Snow AOE variant |
| Lightning.prefab | `Assets/_Project/Resources/VFX/Element/Lightning.prefab` | 존재 + Electro hit variant |
| Arcane.prefab | `Assets/_Project/Resources/VFX/Element/Arcane.prefab` | 존재 + Magic circle variant |

**검증 방법**:
```csharp
var fire = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Resources/VFX/Element/Fire.prefab");
Assert.IsNotNull(fire);
var source = PrefabUtility.GetCorrespondingObjectFromSource(fire);
Assert.IsNotNull(source);  // variant 확인
Assert.AreEqual("Red energy explosion", source.name);
```

### 5.2 5029 ElementVfxBank.GetColor 회귀

GetColor(Fire/Ice/Lightning/Arcane) 4 색상 5029 답습 — 변동 X.

---

## 6. C6 Play 검증 시나리오

### 시나리오 1 — Resources Load (자동 + Play 진입)

**사전 조건**:
- T01 + T02 commit 완료
- 4 prefab Resources/VFX/Element/ 위치

**단계**:
1. Play Mode 진입 (어떤 씬이든 — 마법사 캐릭터 활성 씬 권장)
2. ElementResolver.ApplyElementEffect 첫 호출 (또는 명시 호출 `ElementVfxBank.GetPrefab(MagicElement.Fire)`)
3. EnsureLoaded() 첫 진입 시 4 Resources.Load 실행

**기대 결과**:
- Console: prefab 4 모두 로드 성공 (null 없음)
- Resources.LoadAsync 미세 지연 후 prefab return
- `_initialized = true` 전환

---

### 시나리오 2 — 마법사 element 시각 (★ 핵심)

**사전 조건**:
- T01 + T02 commit 완료
- 마법사 직업 캐릭터 활성
- weapon_firebottle (또는 Fire element 무기) 장착

**단계**:
1. Attic → 마법사 캐릭터 선택 → CH1 진입
2. weapon_firebottle (Fire) 자동 활성화 (12 슬롯 weapon_v2 정합)
3. 적 1마리 처치 시 → ElementResolver.ApplyElementEffect(target, Fire, baseDamage) 호출
4. ElementVfxBank.Spawn(Fire, target.transform.position) 호출
5. **Red energy explosion VFX prefab Instantiate** ★

**기대 결과**:
- 적 처치 위치에 Red energy explosion 시각 발동 (사용자 시각 확인)
- 시각적으로 5029 placeholder (StarBurst) 와 명확히 다른 효과
- SPAWN_LIFETIME_SECONDS (3초) 후 자동 사라짐 (Hierarchy Element_Fire(Clone) GameObject 소멸)
- 메모리 누수 0 — 적 ~100 처치 후 Hierarchy 잔존 0

**검증 방법**: 사용자가 Play Mode 에서 시각 확인 + Hierarchy GameObject 잔존 검사.

---

### 시나리오 3 — URP 2D 호환

**사전 조건**:
- 시나리오 2 진행 중

**단계**:
1. 시나리오 2 의 Red energy explosion 발동 시 색상 관찰
2. 분홍 / 검정 (URP 미호환 셰이더) 표시 시 미합격
3. 정상 색감 (붉은/노란 폭발) 표시 시 PASS

**기대 결과 (PASS)**:
- 정상 붉은/노란 폭발 색감

**기대 결과 (FAIL)**:
- 분홍/검정 표시 → 사용자 결정 필요:
  - **fix**: `Edit > Render Pipeline > Universal RP > Convert URP Materials` 일괄 변환 → SetupElementVfxVariants 재실행 → T01 prefab cascade 재생성
  - **bypass**: 다른 prefab 으로 교체 (PRD § 8 위험 완화 답습)

---

### 시나리오 4 — PG-12 톤 (CLAUDE.md §2)

**사전 조건**:
- 시나리오 2 진행 중 + 시나리오 3 PASS

**단계**:
1. 4 element 모두 차례로 시각 검증 (Fire/Ice/Lightning/Arcane)
2. 각 prefab 의 시각 강도 평가
3. 만화풍 ✅ / 사실적 ❌ 판정

**기대 결과 (PASS)**:
- Red energy explosion 만화풍 폭발 (별 흩어짐 / 색 강조)
- Snow AOE 만화풍 얼음 / Electro hit 만화풍 번개 / Magic circle 환상적 마법진
- PG-12 톤 정합 ✅

**기대 결과 (FAIL)**:
- 사실적 폭발 / 어두운 톤 / 잔인 표현 미합격
- prefab 4 GameObject ref 만 다른 Hovl prefab 으로 교체 (예 Fire → Magic circle 2 등) — PRD § 8 답습

---

## 7. 비기능 요구사항

- [ ] C# 컴파일 errors=0 (3 commit 모두)
- [ ] Editor Test 신규 3/3 PASS + 기존 회귀 0
- [ ] ProjectValidator warnings ≤ 172 (5023 baseline)
- [ ] 5029 ElementResolver 시그니처 보존 (NUnit 6 회귀 0)
- [ ] 5033 신규 — public 필드 0, Update() 0, Singleton 직접 참조 0
- [ ] 적 ~100 처치 후 Hierarchy "*Element*" 잔존 0 (메모리 누수 검증)

## 8. 합격 기준

### 최소 합격
- C1 컴파일 ✅
- C2 Test 신규 3/3 + 회귀 0 ✅
- C3 Validator errors=0 + warnings 변동 0 ✅
- C4 Reflection 모든 항목 ✅
- C5 4 prefab Resources 위치 ✅
- 시나리오 1 + 2 PASS

### 완전 합격
- 모든 C1~C5 카테고리 ✅
- 시나리오 1 + 2 + 3 + 4 모두 PASS
- 비기능 요구사항 모두 ✅
- 5033.1 후속 backlog 갱신 (D2=A cascade)

### FAIL 처리
- C1~C5 자동 검증 1개라도 ❌ → Phase 5 재진입 (수정 지시 발행, max 2회 반복)
- 시나리오 3 FAIL (URP 분홍) → cascade fix (Convert URP Materials → SetupElementVfxVariants 재실행)
- 시나리오 4 FAIL (PG-12 톤) → cascade fix (prefab 교체)

---

## 9. Trade-off (mediator 결정 기반)

(decisions.md 참조)

| 결정 | Trade-off |
|---|---|
| **D1=B Editor 도구** | Editor 도구 ~50L 신규 + Sprint 1 시간 30→35분 ↑. 일회성 사용자 수동 (옵션 A) 대비 +5분 비용. **회수**: 5033.1 (9 무기 매핑) cascade 시 동일 도구 재사용. |
| **D2=A Sprint cascade GC** | 적 ~4000/챕터 처치 시 GC 누적 (~수백 KB) 발생. **수용 사유**: 5033 핵심 가치는 시각 통합 우선, GC 정착은 5033.1 별도 Sprint 분리 시 회수 용이. |
| **D3=B EnsureLoaded lazy** | 첫 호출 시 Resources.Load 4건 미세 지연 (~ms). **수용 사유**: NUnit 친화 + 비마법사 씬 부담 X. |

---

## 10. 5029 보존 검증 (회귀 가드)

| 5029 시스템 | 5033 영향 | 검증 |
|---|---|---|
| MagicElement enum 4 값 | 변동 X | C4 reflection |
| ElementResolver.ApplyElementEffect 시그니처 | 변동 X | C4 reflection + ElementResolverTests 6 회귀 |
| ElementResolver.ApplyElementEffect switch 4 분기 | 변동 X (ElementVfxBank.Spawn 호출 1줄만 추가) | 정적 분석 |
| ElementResolverTests 6 NUnit | 변동 X (모두 PASS) | C2 |
| ElementVfxBank.GetColor | 변동 X | 정적 분석 |
| ElementVfxBank.Spawn 시그니처 | 변동 X (본체 placeholder → Hovl) | C4 reflection |
| 9 mage weapon SO (5029) | 변동 X | (검사 불필요) |
| weapon_thunder_wand SO (5029) | 변동 X | (검사 불필요) |
| EnemyData.elementResistance[4] | 변동 X | (검사 불필요) |

---

## 11. 변경 이력
- 2026-05-06: 초기 작성 (Phase 4 Contract). 6 카테고리 + 4 시나리오 + 9 회귀 가드.
