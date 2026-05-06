# 5024 PRD 리뷰 (Critique)

> **검토자**: Claude (prd-critiquer Phase 2.1)
> **대상**: `5024_editor_test_expansion_prd.md` (디자인 + Sprint)
> **일시**: 2026-05-06

## 0. 결정 이력

| 일자 | 검토자 | 검토 |
|---|---|---|
| 2026-05-06 | Claude | 초기 — Phase 2.1 critique. 5023 후 base NUnit 126 (PRD 117 baseline 갱신 필요) |

---

## 1. 종합 점수: 9.0/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | §15.9 핵심 정적 service 테스트 정합 ✅ |
| PG-12 톤 | 10/10 | 코드 변경 0 — 영향 0 |
| 성능 | 10/10 | NUnit Test 만 — 런타임 0 영향 |
| 시스템 통합 | 9/10 | 5021/5022/5023 cascade 회귀 안전망 강화. base NUnit 117 → 126 (PRD 갱신) |
| Sprint 분해 | 8.5/10 | 4 Sprint 균형. RNG 결정성 + Test 100L 초과 일부 Discuss |

---

## 2. Accept 항목 (5)

### 2.1 ✅ 4 핵심 service 테스트 (§15.9 정합)
- DamageQuery (정적 service) / EquipmentManager (slot) / PlayerStats (recalc) / ItemDropService (드랍 분포)
- 모두 회귀 위험 영역 — 안전망 강화 적절

### 2.2 ✅ NUnit 117 → 144 (목표 140+ 충족)
- 27 신규 (DamageQuery 6 + EquipmentManager 7 + PlayerStats 6 + ItemDropService 8)
- 5023 후 base 126 → 153 갱신 필요 (PRD 미반영)

### 2.3 ✅ 코드 변경 0 (안전망만)
- 기존 service 동작 변경 X
- Test 추가만 — 회귀 위험 0

### 2.4 ✅ 5017 회귀 검증 흡수 (DropContext struct)
- ItemDropServiceTests Sprint 4 의 `DropContext_Struct_NoHeapAlloc` 케이스
- 5017 GC alloc 회귀 안전망 강화

### 2.5 ✅ 4 Sprint 직렬 의존성 X (병렬 가능)
- 각 Test 클래스 독립 — 병렬 가능 표기 필요

---

## 3. Reject 항목 (0)

(없음)

---

## 4. Discuss 항목 (3)

### 4.1 🟡 D1 — base NUnit 카운트 갱신 (117 → 126)

**배경**: PRD §3 의 NUnit 117 → 144 (+27) 목표는 5023 시작 전 baseline. 5023 종결 후 base 가 126 (5023 NUnit 6 추가) → 5024 후 153.

**옵션**:
- **A** (PRD 원안 유지): 117 → 144 표기, 실제 결과 153 일 수 있음
- **B** ★ Claude 권장: PRD 갱신 — base 126 → 153 (+27)
- **C**: 144 목표 그대로 두되 5024 종결 시 18 개만 추가 (조정)

**Discuss**: B 권장 — PRD 정확도 ↑.

### 4.2 🟡 D2 — RNG 결정성 (ItemDropService)

**배경**: PRD §6 알려진 위험 — "ItemDropService 내부 RNG 비결정성" + "UnityEngine.Random.InitState(seed) 명시". Sprint 4 의 `AffixRoll_Determinism_SameSeedSameResult` 테스트가 결정성 보장.

**고려**: ItemDropService 가 UnityEngine.Random 사용한다면 정적 state — Test 사이 영향 가능. SetUp 에서 InitState 호출 + TearDown 에서 복원 권장.

**옵션**:
- **A** (PRD 원안): 각 Test 가 InitState(seed) 명시
  - 장점: 명시적
  - 단점: 보일러플레이트 코드
- **B** ★ Claude 권장: SetUp 헬퍼 + TearDown 에서 원본 state 복원 (TestUtils 신규)
  - 장점: 깔끔, 상태 누수 0
  - 단점: TestUtils 신규 ~15L
- **C**: ItemDropService 가 결정적 시드 인자 받음 (코드 변경 — scope 외)

**Discuss**: B 권장.

### 4.3 🟡 D3 — Test 클래스 100L 권장 초과

**배경**: PRD §3 추정 라인 — DamageQueryTests 80L / EquipmentManagerTests 110L / PlayerStatsTests 90L / ItemDropServiceTests 140L. 일부 100L 권장 초과.

**옵션**:
- **A** (PRD 원안): 통합 유지 — Test 명세 특성상 한 클래스 OK
  - 장점: 같은 service 검증 응집도
  - 단점: §15.6 100L 권장 약간 초과 (140L)
- **B**: ItemDropServiceTests 분리 — RollDistributionTests + AffixRollTests 등
  - 장점: §15.6 정합
  - 단점: 파일 수 ↑
- **C** ★ Claude 권장: 통합 유지 + Test 클래스는 200L 룰만 검증 (100L 권장 미적용 — Test 명세 특성)

**Discuss**: C 권장.

---

## 5. CLAUDE.md §15 정책 위반 점검

| §15 항목 | 상태 |
|---|---|
| §15.1 SRP | ✅ 각 Test 클래스 단일 service |
| §15.6 100줄 | ⚠️ 일부 Test 100L+ (D3) — Test 특성 |
| §15.9 Test | ✅ 핵심 정적 service 우선 |
| §15.11 안티 | ✅ Mock 패턴 (ScriptableObject.CreateInstance) |

---

## 6. 권장 변경

### 즉시 변경 (Reject 없음 → 0)

(없음)

### 사용자 결정 후 변경 (Discuss)

- **D1 — base NUnit 갱신**: B 권장 (117 → 144 표기 → 126 → 153)
- **D2 — RNG 결정성**: B 권장 (SetUp 헬퍼)
- **D3 — Test 100L 초과**: C 권장 (통합 유지)

---

## 7. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성. 점수 9.0/10. Accept 5 / Reject 0 / Discuss 3 |
