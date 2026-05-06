# 5031 Decisions

> Phase 2.3 Mediator · ai-assisted · 2026-05-06

| Item | Decision | 영향 |
|---|---|---|
| D1 | **A** Sprint 2 검증 + 미호환 시 5031.1 | Receipt v5 호환 검증 |
| D2 | **B** Mock IStoreController (큰 작업) | Sprint 3 추가 (60분) — IIAPGateway 인터페이스 추출 + UnityIAPGateway/MockIAPGateway |
| D3 | **B** IAPProductCatalog + IAPPurchaseHandler 분리 | Sprint 1 추가 작업 (~20분) |

사용자 결정. **D2+D3 채택으로 작업 시간 +1h** (2h → 3h).

## v2 변경

- D2=B 로 Sprint 3 = Mock + NUnit (60분)
- D3=B 로 Sprint 1 IAPService 분리 (IAPProductCatalog + IAPPurchaseHandler)
- 합계: 4 commit / ~3h
