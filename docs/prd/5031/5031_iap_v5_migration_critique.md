# 5031 PRD 리뷰

> Phase 2.1 Critique · 2026-05-06

## 1. 종합 점수: 8.4/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | facade 보존 + 단일 책임 ✅ |
| 회귀 안전성 | 8/10 | 호출처 5 파일 시그니처 보존, 단 Editor Fake Store v5 동작 미검증 |
| Sprint 분해 | 8/10 | 3 commit 합리, 단 v5 API 시그니처 검증 시간 추정 어려움 |
| 검증 시나리오 | 7/10 | iOS 실 구매는 TestFlight 까지 미룸 (검증 부분 제한) |
| 위험 관리 | 8/10 | Receipt 검증 v5 호환 불확실 (5031.1 후속 anchor) |

## 2. Accept (5)

- ✅ A1: facade 보존 (호출처 5 파일 변경 0)
- ✅ A2: v5 비동기 3단계 (Connect / FetchProducts / FetchPurchases)
- ✅ A3: 이벤트 기반 (OnPurchasePending / Failed / ProductsFetched)
- ✅ A4: ProductDefinition list (ConfigurationBuilder 대체)
- ✅ A5: #pragma 제거 (v5 전체 API → CS0618 0)

## 3. Reject (0)

(없음)

## 4. Discuss (3)

- 🟡 **D1 [Medium]**: Receipt 검증 v5 호환 — Sprint 2 에서 검증? 또는 5031.1 즉시 분리?
- 🟡 **D2 [Medium]**: NUnit Test 3 (Sprint 3) — IAPService 가 Singleton + Static IAP API 의존 → 단위 테스트 어려움. const 검증만 vs Mock 시도 vs 생략?
- 🟡 **D3 [Low]**: IAPService 150L 100L 권장 초과 — 추가 분리 (IAPProductCatalog / IAPPurchaseHandler) vs 응집?

## 5. §15 정책

- §15.1 SRP: ✅ IAP 단일 책임
- §15.6 100L: △ ~150L (200L 룰 통과)
- §15.7 DI: ✅ Singleton + Bootstrap
- §15.9 Test: △ Singleton 제약 (NUnit 부분적)
- §15.11 안티 패턴: ✅
