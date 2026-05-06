# 5022 PRD 리뷰 (Critique)

> **검토자**: Claude (prd-critiquer Phase 2.1)
> **대상**: `5022_oop_validator_split_prd.md` (디자인 + Sprint)
> **일시**: 2026-05-06

## 0. 결정 이력

| 일자 | 검토자 | 검토 |
|---|---|---|
| 2026-05-06 | Claude | 초기 — Phase 2.1 critique. 5021 종결 후 5022 진입 (독립 시스템) |

---

## 1. 종합 점수: 8.7/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.5/10 | 룰 자기위반 해소 + 모든 Rule 100L 미만. 일부 85L (PerformanceRule/WeaponBossRule) — 100L 룰은 충족이나 80L 권장은 약간 초과 |
| PG-12 톤 | 10/10 | 코드 리팩토링만 — 영향 0 |
| 성능 | 10/10 | Editor 도구 — 런타임 0 영향 |
| 시스템 통합 | 9/10 | 분리 전후 검증 결과 동일 명시. 5005 R5 (5021.1 갱신) cascade 영향 검토 필요 |
| Sprint 분해 | 8/10 | 4 Sprint 분배 균형. Sprint 1 의 임시 errors=0 상태 (룰 비어있음) 위험 — 사용자 결정 영역 |

---

## 2. Accept 항목 (6)

### 2.1 ✅ §15.6 100줄 룰 자기위반 해소
- 현재 ProjectValidator.cs 650L 가 §15.6 룰 자기 검사 위반
- 분리 후 모든 Rule 100L 미만 (40~85L) → 정합

### 2.2 ✅ Service Locator 정적 패턴 (§15.8)
- 옵션 B 채택 — 정적 Rule 클래스 + Run orchestrator 호출
- Strategy/Factory 보다 단순 — 룰 등록 동적 X (overhead 0)

### 2.3 ✅ Issue/Severity 별도 파일 (§15.1 SRP)
- public struct + enum 분리 — 모든 Rule 클래스가 import 가능
- ~15L 작은 파일

### 2.4 ✅ 분리 전후 결과 동일 명시
- C3 ProjectValidator errors=0, warnings 변동 0 (5021 시작 시점 172 → 5022 후 동일)
- 5022 Sprint 4 의 결과 비교 검증

### 2.5 ✅ NUnit 회귀 0 (117/117 PASS 유지)
- 분리는 코드 구조 변경만, 동작 변경 0 → 기존 NUnit 회귀 안전망 충분

### 2.6 ✅ MenuItem path 보존
- `Tools/LittleHero/Validate Project` 메뉴 위치 변경 X — 사용자 muscle memory 보존

---

## 3. Reject 항목 (0)

(없음 — Reject 사유 없음)

---

## 4. Discuss 항목 (3)

### 4.1 🟡 D1 — Sprint 1 의 임시 errors=0 상태 (위험 영역)

**배경**: PRD §3 Sprint 1 commit 메시지에 "분리 직후 검증 결과 errors=0/warnings=0 (Sprint 2/3 에서 룰 복원)" 명시됨. 즉 Sprint 1 commit ~ Sprint 3 commit 사이 검증 룰이 비어있는 상태.

**위험**: Sprint 1 commit 후 push 또는 다른 사용자 pull 시 Validator 가 비어있음 → 회귀 검증 X. 이 기간 동안 다른 작업의 회귀 발생 시 발견 못함.

**옵션**:
- **A** (PRD 원안): Sprint 1 → Sprint 2 → Sprint 3 직렬 진행, 일시 위험 허용 (권장 시간 ~1h45m 내 통과)
- **B** ★ Claude 권장: Sprint 1 + Sprint 2 (간단 6) 통합 — Sprint 1.1 = Issue 추출, Sprint 1.2 = orchestrator + 6 Rule 통합 commit
  - 장점: 항상 룰 작동 상태 보장
  - 단점: commit 통합 = 회수 유연성 약간 ↓
- **C**: Sprint 1 의 ProjectValidator 슬림화 시 모든 11 룰을 한 번에 이동 (1 commit으로 통째로 분리)
  - 장점: 통합 대규모 commit 으로 항상 정합
  - 단점: 큰 commit = 회수 어려움

**Discuss**: 사용자 결정.

### 4.2 🟡 D2 — Rule 카테고리 통합 단위

**배경**: PRD §4.1 의 Rule 클래스 통합:
- `EnemyRule` (80L) = EnemyPool + EnemyVisibilityCheck + EnemyHueVariationHook
- `WeaponBossRule` (85L) = WeaponSystem + BossSystem + BossEncounterContextAccess
- `PerformanceRule` (85L) = LitMotionMigration + GCAllocPatterns

이는 100L 룰은 충족하지만 80L 권장 (CLAUDE.md §15.6 권장 영역) 약간 초과.

**옵션**:
- **A** (PRD 원안): 통합 유지 — 같은 카테고리 (Enemy / Weapon-Boss / Performance) 내 응집도 ↑
- **B**: 더 세분화 — EnemyRule → 3 클래스 (EnemyPool/EnemyVisibility/EnemyHue) 등 — 모두 30~50L
  - 장점: 단일 책임 강화
  - 단점: 파일 수 +5 (총 16 vs 11), import overhead
- **C** ★ Claude 권장: 통합 유지 (옵션 A) — 같은 카테고리 응집도 우선, 80~85L 은 100L 룰 만족. 추후 비대 시 분리 가능.

**Discuss**: 사용자 결정.

### 4.3 🟡 D3 — 신규 NUnit 추가 여부

**배경**: PRD 가 NUnit 회귀 0 만 명시 (분리 동작 검증 NUnit 추가 X).

**위험**: 분리 자체 검증 NUnit 없음 — Sprint 4 비교 검증 (분리 전후 결과 동일) 이 사용자 수동 + 비교만.

**옵션**:
- **A** (PRD 원안): NUnit 신규 X, 사용자 수동 비교 검증
- **B**: ProjectValidatorTests 신규 — Run 메서드 호출 + Issue 카운트 검증 (errors=0)
  - 장점: 자동 회귀 안전망
  - 단점: NUnit Editor 컨텍스트 + Editor 어셈블리 복잡 (NUnit 117 모두 LittleHero.Tests.Editor 어셈블리 — Validator 는 LittleHero.EditorTools 어셈블리 — assembly reference 필요)
- **C**: ProjectValidatorRulesTests — 각 Rule 클래스의 Validate 메서드 reflection 호출 + Issue 0 검증 (소수 NUnit, 분리 전후 동일 검증)
- **D** ★ Claude 권장: 옵션 A 채택 (PRD 원안) — 분리는 동작 변경 0 이므로 기존 NUnit 117 회귀 안전망 충분. NUnit 추가는 5024 (editor_test_expansion) 에서 검토.

**Discuss**: 사용자 결정.

---

## 5. CLAUDE.md §15 정책 위반 점검

| §15 항목 | 상태 |
|---|---|
| §15.1 SRP | ✅ 각 Rule 단일 카테고리 |
| §15.1 OCP | ✅ switch 신규 0 |
| §15.4 SO 우선 | N/A (Editor 도구만) |
| §15.6 100줄 | ✅ 모든 클래스 100L 미만. 단 PerformanceRule/WeaponBossRule 85L (80L 권장 약간 초과) |
| §15.7 DI | ✅ 정적 List<Issue> 인자 통일 |
| §15.8 Service Locator | ✅ 정적 Rule 클래스 + orchestrator 호출 |
| §15.9 Test | ⚠️ NUnit 신규 X (Discuss D3) |
| §15.11 안티 | ✅ public 필드 0, Update X |
| §15.12 Validator | ✅ 룰 자기위반 해소 |

---

## 6. 권장 변경

### 즉시 변경 (Reject 없음 → 0)

(없음)

### 사용자 결정 후 변경 (Discuss)

- **D1 — Sprint 1 임시 errors=0**: 옵션 A/B/C → Sprint 1 commit 분배 재정의
- **D2 — Rule 통합 단위**: 옵션 A/B/C → 11 Rule 클래스 vs 16 분리
- **D3 — 신규 NUnit**: 옵션 A/B/C/D → Sprint 4 검증 방식

---

## 7. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.1 critique). 점수 8.7/10. Accept 6 / Reject 0 / Discuss 3 |
