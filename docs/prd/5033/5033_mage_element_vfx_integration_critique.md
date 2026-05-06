# 5033 mage_element_vfx_integration PRD 리뷰

## 0. 결정 이력
- 2026-05-06: Claude (prd-critiquer) 1차 리뷰

## 1. 종합 점수: 8.7/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 8/10 | ⚠️ `[SerializeField] static` + Magic Number `3f` Destroy hardcode |
| PG-12 톤 | 9/10 | Red energy explosion 강렬도 사용자 검증 명시 (위험 § 5) |
| 성능 | 7/10 | ⚠️ Instantiate + Destroy GC 발생 — 적 ~4000/챕터 처치 시 누적 |
| 시스템 통합 | 10/10 | 5029 ElementResolver 시그니처 보존, NUnit 6 회귀 0 |
| Sprint 분해 | 9/10 | 30+30+15분 균형, 의존성 순차, 검증 시나리오 4 충분 |

---

## 2. Accept 항목 (5)

### ✅ A1. 옵션 A 채택 — 4 element 1 prefab 단순 매핑
- **사유**: 5029 placeholder (StarBurst) → 실 prefab 점진적 진화. 5033.1 후속 (VfxPool / 9 무기 매핑) 로 cascade 가능.
- **5029 답습 정합**: ElementResolver.ApplyElementEffect 시그니처 보존, ElementVfxBankTests/ElementResolverTests 회귀 0.

### ✅ A2. prefab variant 패턴 (Hovl 원본 보존)
- **사유**: `_ThirdParty/HovlStudio/...` 외부 에셋 변경 X, Resources/VFX/Element/ 에 variant 4 생성 — 원본 갱신 시 자동 반영 + Hovl 라이선스 안전.

### ✅ A3. 3 Sprint 작업량 균형 (~1h 15m)
- Sprint 1 (30분) + Sprint 2 (30분) + Sprint 3 (15분) — 1 commit/Sprint.
- 의존성 그래프 명확: Sprint 1 (prefab) → Sprint 2 (Spawn 분기) → Sprint 3 (NUnit + docs).

### ✅ A4. 검증 시나리오 4 충분
- 시나리오 1 Resources Load / 시나리오 2 마법사 element 시각 / 시나리오 3 URP 2D 호환 / 시나리오 4 PG-12 톤 — 자동 + 사용자 검증 분리.

### ✅ A5. 위험 + 완화 5건 사전 명시
- URP 2D 호환 / prefab variant 부서짐 / GC / 앱 용량 / PG-12 톤 — 모두 완화 방안 + 사용자 결정 분기 명시.

---

## 3. Reject 항목 (2)

### ❌ R1. `[SerializeField] static` 안티 패턴 — 디자인 § 3.2

**위치**: `docs/prd/5033_mage_element_vfx_integration_prd.md` 라인 77-81
```csharp
[SerializeField]
private static GameObject _firePrefab;     // ← static 에 SerializeField 무효
private static GameObject _icePrefab;
```

**문제**:
- Unity SerializeField 는 instance 필드만 작동. **static 필드는 Inspector 노출 X / serialize X**.
- 디자인 코드와 Sprint PRD § 2.2 코드 불일치 (PRD 에서는 `[SerializeField]` 없음 → PRD 가 정답).

**필수 수정**:
- 디자인 § 3.2 의 `[SerializeField]` 제거 → Sprint PRD § 2.2 (정답) 와 일치.
- 또는 `[SerializeField]` 제거 + 주석 "// Bootstrap 으로 lazy load".

### ❌ R2. Magic Number `3f` Destroy hardcode — §15.11 안티 패턴

**위치**: PRD § 3.1 라인 122 + 디자인 § 3.2 라인 102
```csharp
UnityEngine.Object.Destroy(instance, 3f);  // ← Magic Number
```

**문제**:
- `3f` 의미 불명 + 튜닝 시 hardcode 변경 필요.
- §15.11 "Magic Number 금지" 위반.

**필수 수정**:
```csharp
private const float SPAWN_LIFETIME_SECONDS = 3f;
// ...
UnityEngine.Object.Destroy(instance, SPAWN_LIFETIME_SECONDS);
```

또는 element 별 차등 (Fire 2.5s / Ice 4s / Lightning 1.5s / Arcane 3.5s) — 추후 검토 (5033.1 cascade).

---

## 4. Discuss 항목 (3)

### 🟡 D1. prefab variant 생성 방법

**현재**: PRD § 2.1 + 디자인 § 3.3 양쪽에 두 옵션 병기 — 결정 필요.

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | 사용자 Unity Editor 수동 (우클릭 → Create → Prefab Variant × 4) | 가시적 + 사용자 검증 즉시 | 사용자 시간 소비 |
| **B (★권장)** | Claude Editor 도구 자동 — `Tools/LittleHero/Setup Element VFX Variants` | 결정론적 + 재현 가능 + 자동화 | Editor 도구 1 신규 |
| C | `AssetDatabase.CopyAsset` 직접 — Sprint 1 작업 시 일회성 코드 | 빠름 | Editor 도구 분리 X (재실행 어려움) |

**Claude 권장**: **B** — Editor 메뉴 도구로 분리하면 5033.1 (9 무기 매핑) cascade 시 재사용 가능. CLAUDE.md §16 "Editor 메뉴 (Tools/LittleHero)" 패턴 정합.

### 🟡 D2. GC 0 정착 시점

**현재**: PRD § 9 5033.1 후속 backlog 로 명시 — 결정 필요.

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| **A (★권장)** | Sprint 1+2 단순 Instantiate + Destroy → 5033.1 후속 cascade VfxPool | Sprint 단순 + 시각 검증 우선, 5033 ~1h15m 유지 | 적 ~4000/챕터 처치 시 GC 누적 (~수백 KB) |
| B | Sprint 1+2 부터 VfxPool.Register/Spawn API 추가 + element 등록 | GC 0 정착, 5029 VfxPool 답습 | 작업 시간 ↑ (~30분 추가), API 검증 필요 |
| C | A 진행 + Sprint 2 끝에 Profiler GC 측정 → 50KB/s 초과 시 자동 cascade | 데이터 기반 결정 | Sprint 일정 가변 |

**Claude 권장**: **A** — 5033 의 핵심 가치는 시각 통합 (placeholder → 실 prefab) — 사용자 시각 검증 우선. GC 우려는 5033.1 별도 sprint 로 분리하면 회수 용이. `gc_alloc_followup` (CLAUDE.md §11) 마일스톤과도 정합.

### 🟡 D3. EnsureLoaded vs Bootstrap

**현재**: 디자인 § 3.2 는 `[RuntimeInitializeOnLoadMethod] Bootstrap`, PRD § 2.2 는 `EnsureLoaded()` lazy — 일관성 보정 필요.

| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | `[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)] Bootstrap` (디자인) | Scene 진입 전 1회 보장 | 모든 씬에 강제 로드 (Attic 같은 마법사 비활성 씬도) |
| **B (★권장)** | `EnsureLoaded()` lazy + GetPrefab/Spawn 첫 호출 시 init (PRD) | 마법사 안 쓰는 씬은 로드 X, Test 친화 (Init 호출 가능) | 첫 호출 미세 지연 (Resources.Load 4건 ≈ 수 ms) |

**Claude 권장**: **B** — PRD § 2.2 가 정답. 디자인 § 3.2 의 `Bootstrap` 을 `EnsureLoaded()` 로 통일. NUnit 테스트 시 명시 호출 가능 + Attic 등 비마법사 씬 부담 X.

---

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 |
|---|---|
| §15.1 SRP — 단일 책임 | ✅ 통과 (ElementVfxBank: element → prefab 매핑 + spawn) |
| §15.1 OCP — switch | △ MagicElement enum switch 4 분기 — 4 element 고정이라 Factory 과함 (5029 답습 OK) |
| §15.2 네이밍 | ✅ 통과 (`_firePrefab` `_initialized` `_camelCase` 정합) |
| §15.3 GC 최소화 | ⚠️ Instantiate + Destroy 발생 — D2 결정 필요 |
| §15.4 SO 우선 | △ Resources prefab 직접 (SO 분리 안함). 4 element 고정이라 hardcode 허용 — 9 무기 매핑 (5033.1) 시 SO 정착 권장 |
| §15.6 100줄 | ✅ 통과 (~60L 추정) |
| §15.7 DI | ✅ 통과 (D3=B 채택 시 EnsureLoaded 명시 init) |
| §15.9 Test | ✅ 통과 (NUnit 2 선택, ElementResolverTests 6 회귀 0 명시) |
| §15.11 안티 | ❌ R1 SerializeField static / R2 Magic Number `3f` |

---

## 6. 권장 변경

### 6.1 즉시 변경 (Reject)

1. 디자인 § 3.2 의 `[SerializeField]` 제거 (R1) — PRD § 2.2 와 통일.
2. PRD § 3.1 + 디자인 § 3.2 의 `3f` 를 `SPAWN_LIFETIME_SECONDS` const 분리 (R2).

### 6.2 사용자 결정 후 변경 (Discuss)

3. D1 prefab variant 생성 — Editor 도구 (권장 B) vs 사용자 수동 (A) vs AssetDatabase.CopyAsset 일회성 (C).
4. D2 GC 0 정착 시점 — Sprint 1+2 단순 (권장 A) vs VfxPool 즉시 정착 (B) vs Profiler 측정 후 (C).
5. D3 init 패턴 — Bootstrap (A, 디자인) vs EnsureLoaded lazy (권장 B, PRD).

### 6.3 후속 backlog (5033.1) 정합 확인

PRD § 9 명시 — VfxPool Register/Spawn / 9 무기 매핑 / Sparks 보조. D2=A 채택 시 GC 정착도 5033.1 로 합산 권장.

---

## 7. 변경 이력
- 2026-05-06: 초기 작성 (Phase 2.1 Critique)
