# 5024 Editor Test Expansion — Breakdown 개요

> **Phase 3** · 4 태스크 / 4 commit / 5h40m
> 의존성: 5023 종결 (base NUnit 126)

---

## 태스크 목록

| ID | 제목 | NUnit | 시간 |
|---|---|---|---|
| T01 | DamageQueryTests + TestUtils (D2=B) | 6 | 70분 |
| T02 | EquipmentManagerTests | 7 | 90분 |
| T03 | PlayerStatsTests | 6 | 60분 |
| T04 | ItemDropServiceTests + changelog | 8 | 130분 |

**합계**: 27 신규 NUnit, **126 → 153**, 5h40m / 4 commit.

---

## 의존성

```
T01 (DamageQuery + TestUtils) ─┬→ T04 (ItemDropService — TestUtils 활용)
T02 (EquipmentManager) ────────┤
T03 (PlayerStats) ─────────────┘
```

T01 의 TestUtils 가 T04 의 RNG 결정성 의존. 다른 태스크는 병렬 가능.

---

## §15 정합

- §15.9 단위 테스트 정합 ✅
- §15.6 Test 클래스 200L 룰만 적용 (100L 권장 미적용 — D3=A)
- 코드 변경 0 (안전망만)

---

## 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기 — 4 태스크 / 27 NUnit |
