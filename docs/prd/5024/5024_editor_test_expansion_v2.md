# 5024 Sprint PRD v2 — Editor Test Expansion

> **PRD v2 (최종, Mediator 확정)**
> 원본 + Rebuttal + Decisions (D1=B, D2=B, D3=A)

---

## 1. 전체 개요

| Sprint | 작업 | 신규 NUnit | 누적 | commit | 시간 |
|---|---|---|---|---|---|
| 0 (사전) | TestUtils 헬퍼 (D2=B) | 0 | 126 | (Sprint 1 통합) | 10분 |
| 1 | DamageQueryTests | 6 | 126 → 132 | 1 | 60분 |
| 2 | EquipmentManagerTests | 7 | 132 → 139 | 1 | 90분 |
| 3 | PlayerStatsTests | 6 | 139 → 145 | 1 | 60분 |
| 4 | ItemDropServiceTests + TestUtils | 8 | 145 → 153 | 1 | 130분 |
| **합계** | | **27** | **126 → 153** | **4** | **5h40m** |

(D1=B baseline 갱신, D2=B TestUtils 는 Sprint 4 commit 통합)

---

## 2~9. Sprint 내용

→ 원본 PRD `5024_editor_test_expansion_prd.md` 와 본문 동일.

**D2=B 추가**: Sprint 4 ItemDropServiceTests 에 TestUtils.cs (RNG SetUp/TearDown 헬퍼) 신규 포함.

```csharp
// Tests/Editor/TestUtils.cs (~15L)
public static class TestUtils {
    private static UnityEngine.Random.State _savedState;
    public static void InitDeterministic(int seed) {
        _savedState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed);
    }
    public static void RestoreRandom() {
        UnityEngine.Random.state = _savedState;
    }
}
```

각 RNG 의존 Test 클래스의 SetUp/TearDown 에서 호출.

---

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | v2 (Mediator). D1=B baseline 126→153 + D2=B TestUtils + D3=A 통합 유지 |
