# 5028 PRD 리뷰

> Phase 2.1 Critique · 일시: 2026-05-06

## 1. 종합 점수: 8.8/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | facade 패턴 + 신규 10 파일 모두 200L 이내 |
| 회귀 안전성 | 9/10 | 5027 답습 + facade 시그니처 보존 |
| Sprint 분해 | 9/10 | 4 commit 합리, Sprint 1+2+3 병렬 가능 |
| 단위 테스트 | 8/10 | 기존 NUnit 158 회귀 검증 + StarforceServiceTests 10 보존 |
| 효과 | 8/10 | warnings -3 (73→70). 5028.1 UI 분리 (-5) 후속 |

## 2. Accept (5)

- ✅ A1: 옵션 B (Top 3 service 만) — 5027 답습, 안전
- ✅ A2: EquipmentManager MonoBehaviour facade — Awake/Update 보존
- ✅ A3: LimitedShopService 22 메서드 시그니처 보존
- ✅ A4: StarforceService TryEnhance 응집 유지 + helper 분리
- ✅ A5: 5028.1 UI Panel 후속 분리 (Inspector wiring 위험 회피)

## 3. Reject (0)

(없음)

## 4. Discuss (3)

- 🟡 **D1 [Medium]**: EquipmentPatternActivator + InstanceSyncer POCO vs 정적 service — facade 가 instance 보유 vs 정적 위임?
- 🟡 **D2 [Medium]**: StarforceService facade 140L — 100L 권장 초과. TryEnhance 추가 분리 (TryEnhanceCore + ResultBuilder) vs 응집 유지?
- 🟡 **D3 [Low]**: Sprint 4 (Validator + docs) 통합 vs 분리?

## 5. §15 정합

- §15.1 SRP: ✅ 단일 책임 명확
- §15.6 100L: △ StarforceService facade 140L (D2)
- §15.7 DI: ✅ 5027 답습
- §15.9 Test: ✅ 회귀 안전망 + 기존 NUnit 158
- §15.11 안티 패턴: ✅
