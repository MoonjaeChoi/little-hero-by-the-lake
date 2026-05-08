# 5040 main_menu_revamp PRD Discussion Topics

> **Phase 2.2 산출** — mediator 결정 필요 미결 3건.

## Medium Priority

### D1. 11 Panel 흡수 — 통합 vs 보존
- 옵션 A: prefab reuse + Tab instantiate (logic 보존)
- 옵션 B: Tab 별 신규 view (작업 ↑)
- 옵션 C: hybrid (main view 보존, modal 폐기)
- **권장**: 옵션 A

### D2. TabContent 동적 변경 — Pool vs SetActive
- 옵션 A: 모두 SetActive (메모리 ↑, 빠름)
- 옵션 B: Instantiate + Destroy (GC ↑)
- 옵션 C: ObjectPool
- **권장**: 옵션 A

### D3. S6 분해 — 통합 vs 분리
- 옵션 A: 1 commit (큰 변경)
- 옵션 B: S6a + S6b 분리 (안전성 ↑)
- **권장**: 옵션 B

## 요약
- Total: 3 / Medium: 3
- 권장 mode: ai-assisted

권장: **D1=A / D2=A / D3=B**
