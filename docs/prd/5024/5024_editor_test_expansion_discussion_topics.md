# 5024 PRD Discussion Topics

> **Phase 2.2** — mediator 결정 필요. 합계 **3** (Medium 2, Low 1)

---

## D1. base NUnit 갱신 (117 → 126 baseline)

- **A**: PRD 117 baseline 그대로
- **B** ★ Claude 권장: PRD 갱신 (5023 후 base 126 → 153)

## D2. RNG 결정성 (ItemDropService Test)

- **A**: 각 Test 가 InitState(seed) 명시 (보일러플레이트)
- **B** ★ Claude 권장: SetUp/TearDown 헬퍼 (TestUtils 신규 ~15L)
- **C**: ItemDropService 코드 변경 (scope 외)

## D3. Test 클래스 100L 권장 초과

- **A** (=C): 통합 유지 — Test 클래스 200L 룰 만 적용 (100L 권장 미적용)
- **B**: ItemDropServiceTests (140L) 분리 — RollDistributionTests + AffixRollTests

## 권장 답변

```
D1=B, D2=B, D3=A
```

또는 "권장 그대로 진행".
