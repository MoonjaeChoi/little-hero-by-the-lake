# 5023 PRD 리뷰 (Critique)

> **검토자**: Claude (prd-critiquer Phase 2.1)
> **대상**: `5023_oop_metaprogression_split_prd.md` (디자인 + Sprint)
> **일시**: 2026-05-06

## 0. 결정 이력

| 일자 | 검토자 | 검토 |
|---|---|---|
| 2026-05-06 | Claude | 초기 — Phase 2.1 critique. 5021 cascade 핵심 (ResolveSelectedCharacterFromSave 흡수) |

---

## 1. 종합 점수: 8.5/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9.0/10 | 7 service 분리 § 15.1 SRP 정합. 모든 service 100L 미만. Singleton 잔존 (§15.7 partial DI) |
| PG-12 톤 | 10/10 | 코드 리팩토링만 — 영향 0 |
| 성능 | 9/10 | 정적 service 호출 — 추가 overhead 미미 |
| 시스템 통합 | 7.5/10 | 호출처 30+ 마이그레이션 — 회귀 위험 ↑. Singleton 직접 참조 → 위임 (§15.7 정합 ↑) |
| Sprint 분해 | 8.5/10 | 4 Sprint 균형 OK. Sprint 2/3 의 호출처 마이그레이션 작업량 추정 모호 |

---

## 2. Accept 항목 (5)

### 2.1 ✅ 7 service 분리 (§15.1 SRP)
- Loader / Bootstrapper / Character / Chapter / Currency / Equipment / Endgame — 단일 책임 명확
- 모든 service 100L 미만 (50~80L 추정)

### 2.2 ✅ 5021 cascade — Loader 흡수
- 5021 의 ResolveSelectedCharacterFromSave 가 MetaProgressionLoader 로 자연스럽게 이동
- 5023 PRD §4.1 명시 — cascade 정합

### 2.3 ✅ Service Locator 정적 패턴 (§15.8)
- 5005 attic_decomposition_v1 + 5022 oop_validator_split 답습
- MetaProgression.Instance 단일 진입점 보존 (Data prop 만)

### 2.4 ✅ NUnit 6 신규 명시 (§15.9)
- MetaProgressionServicesTests — Currency 4 + Character event 2

### 2.5 ✅ 호출처 마이그레이션 grep 검증
- PRD §6 알려진 위험 — Sprint 별 grep 검증 명시

---

## 3. Reject 항목 (0)

(없음)

---

## 4. Discuss 항목 (3)

### 4.1 🟡 D1 — 호출처 마이그레이션 전략 (30+)

**배경**: PRD §3 의 옵션 B 채택 — 모든 비즈니스 로직 service 위임. 기존 `MetaProgression.Instance.AddStarFragment(10)` 같은 호출처 30+ 마이그레이션 필요.

**옵션**:
- **A** (PRD 원안): 즉시 마이그레이션 — 호출처 일괄 sed 또는 IDE refactor
  - 장점: 깔끔한 분리, deprecated 코드 0
  - 단점: 30+ 호출처 한꺼번에 → 누락 시 컴파일 에러 가능성
- **B**: deprecated proxy 보존 — `MetaProgression.AddStarFragment` 가 `MetaProgressionCurrency.AddStarFragment` 호출
  - 장점: 점진 마이그레이션, 위험 ↓
  - 단점: deprecated 코드 잔존 (5023.1 cleanup 필요)
- **C** ★ Claude 권장: 즉시 마이그레이션 + Sprint 별 commit 분리
  - Sprint 2 = Character + Chapter 호출처 (~5)
  - Sprint 3 = Currency + Equipment 호출처 (~20)
  - Sprint 4 = Endgame 호출처 (~5)
  - 장점: 깔끔 + Sprint 별 위험 격리

**Discuss**: 사용자 결정.

### 4.2 🟡 D2 — SaveSystem.Save 호출 위치

**배경**: PRD 가 각 service 메서드 마지막 SaveSystem.Save 호출 명시 (§4.1 예제). 그러나 분산 호출 = 누락 시 SaveData 미보존 위험.

**옵션**:
- **A** (PRD 원안): 각 service 메서드가 SaveSystem.Save 직접 호출 (분산)
  - 장점: 즉시 영속, 명시적
  - 단점: 누락 위험, 다중 호출 (성능 영향 미미하지만)
- **B**: MetaProgression.Save() 정적 헬퍼 + 각 service 가 dirty flag 만 set
  - 장점: 누락 0, 중앙 트리거
  - 단점: 명시적 Save() 호출 필요 (실수 가능)
- **C** ★ Claude 권장: PRD 원안 (옵션 A) + ProjectValidator 룰 (service Save 호출 검증)
  - 장점: 즉시 영속 + 자동 회귀 안전망
  - 단점: 룰 작성 추가 (5023.1 검토)

**Discuss**: 사용자 결정.

### 4.3 🟡 D3 — event 위치 (OnCharacterChanged 등)

**배경**: PRD §4.1 의 MetaProgressionCharacter 가 OnCharacterChanged event 흡수. 기존 구독자 (LimitedShopService 등) 영향.

**옵션**:
- **A** (PRD 원안): event 를 service 클래스로 이동 — `MetaProgressionCharacter.OnCharacterChanged`
  - 장점: 책임 일관 (Character 책임 = Character event)
  - 단점: 구독자 호출처 변경 (`MetaProgression.Instance.OnCharacterChanged` → `MetaProgressionCharacter.OnCharacterChanged`)
- **B** ★ Claude 권장: event 를 MetaProgression 정적으로 이동 (proxy 패턴) — `MetaProgression.OnCharacterChanged` 정적 event, service 가 발행
  - 장점: 구독자 호출처 변경 0 (단순 instance → 정적)
  - 단점: event 발행 책임은 service, 정의는 MetaProgression — 책임 분산
- **C**: GameEvents 정적 EventBus 로 이동 (CLAUDE.md §15.5 정합)
  - 장점: 일관 패턴 (다른 게임플레이 event 와 동일)
  - 단점: 구독자 일괄 마이그레이션

**Discuss**: 사용자 결정.

---

## 5. CLAUDE.md §15 정책 위반 점검

| §15 항목 | 상태 |
|---|---|
| §15.1 SRP | ✅ 7 service 단일 책임 |
| §15.6 100줄 | ✅ 모든 service 100L 미만 (50~80L) |
| §15.7 DI | △ Singleton 잔존 (Instance.Data 참조) — partial DI |
| §15.8 Service Locator | ✅ 정적 service 패턴 |
| §15.9 Test | ✅ NUnit 6 신규 |
| §15.11 안티 | ✅ public 필드 0 (SerializeField + private) |

---

## 6. 권장 변경

### 즉시 변경 (Reject 없음 → 0)

(없음)

### 사용자 결정 후 변경 (Discuss)

- **D1 — 호출처 마이그레이션**: A/B/C → Sprint 분해 결정
- **D2 — SaveSystem.Save 위치**: A/B/C → service 패턴 결정
- **D3 — event 위치**: A/B/C → 구독자 영향 결정

---

## 7. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 작성 (Phase 2.1 critique). 점수 8.5/10. Accept 5 / Reject 0 / Discuss 3 |
