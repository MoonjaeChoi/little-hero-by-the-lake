# 5031 PRD v2

> Mediator 확정 · D1=A 검증 + D2=B Mock + D3=B 분리

## Sprint 분해 (D2+D3 채택)

| Sprint | 작업 | commit | 시간 |
|---|---|---|---|
| 1 | IAPService v5 + IAPProductCatalog + IAPPurchaseHandler 분리 (D3=B) | 1 | 80분 |
| 2 | 호출처 회귀 + Receipt v5 호환 검증 (D1=A) | 1 | 30분 |
| 3 | IIAPGateway 인터페이스 + UnityIAPGateway + MockIAPGateway + NUnit (D2=B) | 1 | 60분 |
| 4 | #pragma 제거 + changelog + CLAUDE.md | 1 | 15분 |
| **합계** | | **4** | **~3h** |

## D3=B 신규 분리 (Sprint 1)

| 파일 | 책임 | 라인 |
|---|---|---|
| `Core/IAP/IAPProductCatalog.cs` | ID const 6 + ProductDefinition list + ApplyGrant 매핑 | ~50L |
| `Core/IAP/IAPPurchaseHandler.cs` | OnPurchasePending/Failed 핸들러 | ~50L |
| `Core/IAPService.cs` (facade) | Singleton + Bootstrap + Gateway + 위임 | **~80L** |

## D2=B 신규 인터페이스 (Sprint 3)

| 파일 | 책임 | 라인 |
|---|---|---|
| `Core/IAP/IIAPGateway.cs` | interface (Connect/FetchProducts/PurchaseProduct + 이벤트) | ~30L |
| `Core/IAP/UnityIAPGateway.cs` | v5 StoreController wrapper | ~80L |
| `Core/IAP/MockIAPGateway.cs` | Test 용 (Editor only) | ~50L |
| `Tests/Editor/IAPServiceTests.cs` | NUnit 5+ (Mock 활용) | ~70L |

## 합계 신규 6 파일 + 1 facade 갱신

원본 PRD 본문 + Sprint 4개 + 신규 6 파일.
