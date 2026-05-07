# T04: NUnit 6 — Variant SO + Chapter Wiring 검증

> **레이어**: Test / **의존**: T01+T03 / **예상**: 25분 / 1 commit
> **PRD 출처**: v2 §2.3

## 1. 작업 항목

### 1.1 신규 — `Tests/Editor/ChapterVariantTests.cs` (~110L)

NUnit 6 메서드 (PRD v2 §2.3):
1. `Variant_Stationary_Has_BehaviorType_Stationary`
2. `Variant_Splitter_Has_ChildData_Configured`
3. `Variant_Bomber_Has_ExplodeReaction`
4. `Chapter03_Wave10_MiniBossPool_Contains_Stationary`
5. `Chapter04_Wave5_MiniBossPool_Contains_Splitter`
6. `Chapter05_Wave15_MiniBossPool_Contains_Bomber`

각 테스트는 AssetDatabase.LoadAssetAtPath / FindAssets 답습. 5036 기존 NUnit 답습.

## 2. 검증 기준

- Test Runner Edit Mode > Run All — 6/6 PASS
- 기존 NUnit 회귀 0 (5035 답습 — 누적 NUnit count 증가)

## 3. commit 메시지

```
5037 T04 — ChapterVariantTests NUnit 6 (3/5)

[변경]
  - Tests/Editor/ChapterVariantTests.cs 신규 (~110L)
    - Variant SO 3 (Stationary/Splitter+Child/Bomber)
    - Chapter wiring 3 (CH3 wave10 / CH4 wave5 / CH5 wave15)

[검증]
  - NUnit 6/6 PASS
  - 기존 회귀 0

Co-Authored-By: Claude Opus 4.7 <noreply@anthropic.com>
```
