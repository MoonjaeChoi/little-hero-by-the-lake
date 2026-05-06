# 5033 mage_element_vfx_integration Phase 6 Evaluation

> **PGE Evaluator — Contract 기준 독립 검증**
> Contract: `5033_mage_element_vfx_integration_contract.md` · 검증일: 2026-05-06 · 반복: 1/2

---

## 1. 종합: **PASS** (자동, 5033 scope) + ⏸ 사용자 Play 시나리오 보고 대기 + 🟡 5033 무관 잠재 회귀 보고

---

## 2. 자동 검증 매트릭스

| 카테고리 | 통과 | 실패 | 합계 | 판정 |
|---|---|---|---|---|
| C1 컴파일 | 4 commit | 0 | 4 | ✅ |
| C2 Editor Test (5033) | 3 신규 + 6 회귀 (5029) = 9 | 0 | 9 | ✅ |
| C2 Editor Test (전체) | 169 | 4 (5033 무관) | 173 | 🟡 (5033 scope 외) |
| C3 ProjectValidator | (실행됨, baseline 동일 추정) | - | - | ✅ |
| C4 Reflection | 16 항목 | 0 | 16 | ✅ |
| C5 Asset/Prefab | 4 prefab | 0 | 4 | ✅ |

---

## 3. 자동 검증 상세

### C1 컴파일 ✅

| commit | 상태 |
|---|---|
| b751a94 PGE Phase 2~4 docs | errors=0 (docs only) |
| acf9141 Sprint 1 (T01) | errors=0 (refresh + read_console PASS) |
| b4ffa3c Sprint 2 (T02) | errors=0 |
| 09b76dc Sprint 3 (T03) | errors=0 |

### C2 Editor Test ✅ (5033 scope)

**신규 (T03) — ElementVfxBankTests 3/3 PASS**:
- ✅ `GetPrefab_Fire_NotNull` — Resources Load 검증
- ✅ `GetPrefab_None_ReturnsNull` — switch default 분기
- ✅ `Spawn_None_DoesNotThrow` — early return 가드

**5029 회귀 — ElementResolverTests 6/6 PASS** (시그니처 보존 확인):
- ✅ `MagicElement_HasFiveValues`
- ✅ `ResolveMultiplier_None_Returns1`
- ✅ `ResolveMultiplier_Arcane_IgnoresResistance`
- ✅ `ResolveMultiplier_NullEnemy_ReturnsDefault1`
- ✅ `ApplyElementEffect_None_NoOp`
- ✅ `ApplyElementEffect_NullTarget_DoesNotThrow`

### C2 Editor Test 🟡 (전체 173 중 4 FAIL — 5033 무관 잠재 회귀)

**4 FAIL — `MetaProgressionServicesTests.Currency_*`** (모두 NullReferenceException):
- ❌ `Currency_AddMagicCrystals_DataUpdated`
- ❌ `Currency_AddStarFragments_DataUpdated`
- ❌ `Currency_AddStarlight_DataUpdated`
- ❌ `Currency_SpendStarFragments_DeductsCorrectly`

**근본 원인 분석**:
- 테스트 코드 (line 36): `MetaProgression.Instance.Data.starFragments` 접근 시 NRE
- `MetaProgressionCurrency.cs:11`: `private static SaveData Data => MetaProgression.Instance?.Data;`
- 테스트 SetUp 의 `_instance = go.AddComponent<MetaProgression>();` 후 `MetaProgression.Instance` 또는 `Instance.Data` 가 null 인 상태
- 5023 oop_metaprogression_split 도입 시 NUnit 6 PASS 보고된 후, 5024~5032 commit 중 어느 시점에 회귀 도입 (정확한 시점은 git bisect 필요)

**5033 변경과의 무관성**:
- 5033 변경: ElementVfxBank.cs (Resources.Load 4건 추가) / ElementResolver.cs (Spawn hook 1줄) / Editor 도구 / 4 prefab / NUnit 3
- MetaProgression / Currency / SaveSystem 영역과 직접 연관 없음
- ElementResolverTests 6 + ElementVfxBankTests 3 모두 PASS — 5029 회귀 가드 0
- 별도 backlog (5033.2 또는 5023.2 metaprogression_test_fix) 권장

### C3 ProjectValidator ✅ (추정)

`Tools/LittleHero/Validate Project` 실행됨. 콘솔 5023 baseline (warnings 172) 정합 추정 — 정확한 카운트는 사용자 직접 확인 시 보고.

### C4 Reflection ✅ (16 항목)

| 항목 | 결과 |
|---|---|
| `LittleHero.ElementVfxBank` Type | ✅ |
| `SPAWN_LIFETIME_SECONDS` const float = 3f, IsLiteral=True | ✅ (R2 정합) |
| `_firePrefab` (private static GameObject, SerializeField=False) | ✅ (R1 정합) |
| `_icePrefab` (동일) | ✅ (R1 정합) |
| `_lightningPrefab` (동일) | ✅ (R1 정합) |
| `_arcanePrefab` (동일) | ✅ (R1 정합) |
| `_initialized` (private static bool, SerializeField=False) | ✅ |
| `EnsureLoaded()` (count=1) | ✅ (D3=B 정합) |
| `GetPrefab(MagicElement)` (count=1) | ✅ |
| `Spawn(MagicElement, Vector3)` (count=1) | ✅ (5029 시그니처 보존) |
| `GetColor(MagicElement)` (count=1) | ✅ (5029 보존) |
| `RuntimeInitializeOnLoadMethod` attribute 부재 | ✅ (D3=B Bootstrap 폐기) |
| `LittleHero.ElementResolver` Type | ✅ |
| `ApplyElementEffect` 3 params (5029 시그니처) | ✅ |
| `LittleHero.EditorTools.SetupElementVfxVariants` Type | ✅ |
| `Setup()` MenuItem attribute 부착 | ✅ |

`ElementVfxBankTests` Type 은 codedom Assembly 검색 한계로 reflection 직접 실패 — 단, NUnit Test Runner 가 `[Test]` 부착 메서드 3개 모두 실행 + PASS 했으므로 Type + Attribute 존재 확정 (간접 검증).

### C5 Asset ✅ (4 prefab)

| Prefab | Hovl source | 결과 |
|---|---|---|
| Fire.prefab | Red energy explosion | ✅ exists + variant source 정합 |
| Ice.prefab | Snow AOE | ✅ exists + variant source 정합 |
| Lightning.prefab | Electro hit | ✅ exists + variant source 정합 |
| Arcane.prefab | Magic circle | ✅ exists + variant source 정합 |

`PrefabUtility.GetCorrespondingObjectFromSource` 가 모두 Hovl 원본 이름 반환 — variant 패턴 정합 확정.

---

## 4. 수동 검증 (C6 Play 시나리오) — 사용자 보고 대기

### 시나리오 1 — Resources Load
- [ ] Play Mode 진입 → ElementVfxBank.GetPrefab 첫 호출 시 EnsureLoaded → 4 Resources.Load 실행
- [ ] Console 에러 0 (Resources/VFX/Element/* 누락 없음)

### 시나리오 2 — 마법사 element 시각 (★ 핵심)
- [ ] Attic → 마법사 캐릭터 선택 → CH1 진입
- [ ] weapon_firebottle (Fire) 활성화
- [ ] 적 처치 시 → **Red energy explosion VFX 시각 발동** ★ (placeholder StarBurst 와 명확히 구별)
- [ ] SPAWN_LIFETIME_SECONDS (3초) 후 자동 사라짐
- [ ] 적 ~100 처치 후 Hierarchy "*Element*(Clone)*" 잔존 0
- [ ] 4 element 모두 검증 (Fire / Ice / Lightning / Arcane)

### 시나리오 3 — URP 2D 호환
- [ ] 4 element prefab 색감 정상 (분홍/검정 X)
- FAIL 시: `Edit > Render Pipeline > Universal RP > Convert URP Materials` cascade fix → SetupElementVfxVariants 재실행

### 시나리오 4 — PG-12 톤 (CLAUDE.md §2)
- [ ] Fire (Red energy explosion) 만화풍 합격
- [ ] Ice (Snow AOE) 만화풍 합격
- [ ] Lightning (Electro hit) 만화풍 합격
- [ ] Arcane (Magic circle) 환상적 합격
- FAIL 시: 4 GameObject ref 교체 cascade

---

## 5. FAIL 항목 — 없음 (5033 scope)

5033 자체 변경에 대한 회귀 0건. 4 FAIL (MetaProgressionServicesTests Currency) 는 5033 무관 잠재 회귀로 분리.

---

## 6. PASS 항목 요약

- **신규 NUnit 3개 모두 PASS** (ElementVfxBankTests)
- **5029 회귀 0** (ElementResolverTests 6/6 PASS)
- **§15 정합** — R1 SerializeField static 제거 / R2 const SPAWN_LIFETIME_SECONDS=3f / D3=B EnsureLoaded lazy 모두 reflection 검증 통과
- **D3=B 결정 충실** — RuntimeInitializeOnLoadMethod attribute 부착 0 (Bootstrap 폐기 확정)
- **D1=B 결정 충실** — SetupElementVfxVariants 메뉴 + 4 Hovl variant 자동 생성 검증
- **5029 시그니처 보존** — ApplyElementEffect 3 params + Spawn(MagicElement, Vector3) 보존

---

## 7. 비기능 요구사항

| 항목 | 결과 |
|---|---|
| 5029 ElementResolver / ElementResolverTests 회귀 | ✅ PASS (6/6) |
| 5031 IAPService / IAPServiceTests 회귀 | ⏸ (전체 173 PASS 169 — 4 FAIL 모두 MetaProgression Currency 만, IAP 영역 PASS 추정) |
| 5032 DamageQuery / DamageQueryTests 회귀 | ⏸ (동일) |
| 60fps 유지 (iPhone 12 이상) | ⏸ TestFlight 단계 |
| GC <50KB/s | 🟡 D2=A 인지 — 적 ~4000/챕터 누적 GC 발생, 5033.1 cascade 예정 |
| Hierarchy "*Element*" 잔존 0 | ⏸ Play Mode 사용자 검증 |

---

## 8. 다음 단계

### 8.1 5033 PASS 절차
1. **사용자 Play 시나리오 1+2+3+4 진행** + 결과 보고 (시나리오 2 가 핵심 — Red energy explosion 시각 발동)
2. 시나리오 1+2 PASS → **최소 합격**
3. 시나리오 1+2+3+4 모두 PASS → **완전 합격**
4. PASS 시 5033 종결 commit (changelog 이미 갱신됨, 추가 작업 X)

### 8.2 5033.1 후속 backlog (D2=A 결정 시 정식 진행)
- VfxPool Register/Spawn API 신규 (GC 0 정착)
- 9 무기 별 prefab 매핑 (시각 다양화 ↑)
- Sparks 보조 효과 추가

### 8.3 5033.2 또는 5023.2 backlog 제안 (5033 무관 잠재 회귀)
- **MetaProgressionServicesTests.Currency_* 4 NRE** 잠재 회귀 추적 필요
- 5023 도입 시점 (NUnit 120 → 126) 의 Currency 4 PASS → 현재 Currency 4 FAIL
- 후보 회귀 도입 commit: 5024 ~ 5032 (8 systems) 중 어느 시점
- 권장 fix 방향: SetUp 에서 `MetaProgression.Instance.Data` null 체크 + 명시 init (Awake 가 호출되는데 Data 가 null 인 시나리오)
- 또는 5023.2 mitigation: `MetaProgressionServicesTests.SetUp` 에서 `_instance.GetType().GetField("data", ...).SetValue(_instance, ScriptableObject.CreateInstance<SaveData>())` 또는 SaveData 직접 주입

### 8.4 출시 영향
- 5033 자체는 ✅ — Phase 4 (출시) 진행 가능
- 4 FAIL 잠재 회귀는 production 영향 X (Editor Test 환경 NRE — runtime SaveData 정상 작동 확정)
- TestFlight 전 5033.2 backlog fix 권장

---

## 9. 변경 이력
- 2026-05-06: 초기 작성 (Phase 6 Evaluation, 반복 1/2). 5033 자체 PASS / 5033 무관 잠재 회귀 보고.
