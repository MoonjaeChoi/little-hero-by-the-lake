# Sprint Contract: 5024 Editor Test Expansion

> **PGE Phase 4** · PRD v2 + Breakdown 기반 · 2026-05-06

---

## C1 컴파일 기준

- 신규/수정 .cs errors=0
- 신규: TestUtils.cs + 4 Test 클래스 (DamageQuery / EquipmentManager / PlayerStats / ItemDropService)
- 수정: 없음 (코드 변경 0)

## C2 Editor Test 기준

- 신규 NUnit 27 PASS (5023 후 126 → **153**)
- 기존 126 회귀 0
- 4 Test 클래스 명세 (PRD §4.2 답습)

## C3 ProjectValidator 기준

- errors=0 (5023 baseline 171 동일)
- warnings 변동 ≤ 0 (Test 추가만 — 영향 0 예상)

## C4 Reflection 검증

| Type | 검증 |
|---|---|
| `LittleHero.Tests.TestUtils` | InitDeterministic / RestoreRandom 정적 |
| `LittleHero.Tests.DamageQueryTests` | 6 [Test] |
| `LittleHero.Tests.EquipmentManagerTests` | 7 [Test] |
| `LittleHero.Tests.PlayerStatsTests` | 6 [Test] |
| `LittleHero.Tests.ItemDropServiceTests` | 8 [Test] |

## C5 SO+Asset

N/A (Test 만)

## C6 Play 시나리오

(Test 만 — Play 검증 N/A. NUnit Test Runner 결과 153/153 PASS 가 핵심)

---

## 합격 기준

### 최소
- C1 컴파일 ✅
- C2 NUnit 27 신규 PASS + 126 회귀 0 ✅
- C3 errors=0 ✅

### 완전
- 모든 자동 + Test 153/153 PASS

---

## Trade-off (decisions)

- D1=B → baseline 126 정합
- D2=B → TestUtils ~15L (RNG 결정성)
- D3=A → ItemDropServiceTests 140L 통합 유지 (200L 룰)

---

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 |
