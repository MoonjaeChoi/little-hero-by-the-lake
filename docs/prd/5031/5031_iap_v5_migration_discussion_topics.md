# 5031 Discussion Topics

> Phase 2.2 · 합계 **3**

## D1. Receipt 검증 v5 호환 (Medium)

CrossPlatformValidator 가 v5 PendingOrder.Receipt 호환 여부 미확정.

- **A** ★: Sprint 2 시점 검증 + 미호환 시 5031.1 분리
- **B**: 처음부터 5031.1 분리 (Sprint 추가)
- **C**: Receipt 검증 임시 비활성화 (validated=true 강제) — v4 와 같은 비엄격 정책

★ Claude 권장: A — Sprint 2 검증 후 결정 (조사 비용 작음)

## D2. NUnit Test 전략 (Medium)

Singleton + Unity IAP 의존 → 단위 테스트 어려움.

- **A** ★: const 검증만 (3 NUnit, IAPServiceTests const + GrantStarlight 보너스 계산)
- **B**: Mock IStoreController (큰 작업, 5031 scope 초과)
- **C**: Test 생략 (Sprint 3 = #pragma 제거 + docs 만)

★ Claude 권장: A — const 검증 + 보너스 계산 단위 (Singleton 제약 우회)

## D3. IAPService 150L 추가 분리 (Low)

- **A** ★: 응집 유지 (~150L, 200L 룰 통과)
- **B**: IAPProductCatalog (ProductDefinition + ID const) + IAPPurchaseHandler 분리

★ Claude 권장: A — 5022 SceneRule 192L 답습, 응집 우선

## 권장 답변

```
D1=A, D2=A, D3=A
```

또는 "권장 그대로 진행".
