# 5034 balance_integration_v1 Implementation Breakdown — 개요

> **Phase 3 산출** — PRD v2 → 구현 태스크 분해
> 일시: 2026-05-07 · 총 7 태스크 / 7 commit / ~3h 30m

## 1. 태스크 목록

| ID | 제목 | 레이어 | 의존성 | 시간 | 파일 |
|---|---|---|---|---|---|
| **T01** | BalanceConfig 권장 매트릭스 + Editor 시드 | Data + Editor | - | 30분 | `_01_balance_matrix.md` |
| **T02** | StarforceMultiplierTable 검증 (D2=A 유지) | Logic | - | 15분 | `_02_starforce_verify.md` |
| **T03** | PlayerStats 3 필드 + Add/Remove + ApplyClassBonus | Logic | - | 30분 | `_03_class_bonus.md` |
| **T04a** | CurveSimulator 슬롯 통합 | Editor | T01 | 30분 | `_04a_curve_slot.md` |
| **T04b** | CurveSimulator 무기 + affix 통합 | Editor | T04a | 30분 | `_04b_curve_weapon.md` |
| **T05** | SlotStarforceView + EnhanceModal 권장 ⭐ 표시 | UI | T01 | 30분 | `_05_ui_recommended.md` |
| **T06** | NUnit 5 + Validator + changelog + CLAUDE.md | Test + Docs | T01~T05 | 30분 | `_06_test_docs.md` |

## 2. 의존성 그래프

```
T01 (BalanceConfig) ─┐
T02 (Starforce 검증) ┤ (병렬 가능)
T03 (ClassBonus) ────┘
   ↓
T04a (Curve 슬롯) ─→ T04b (Curve 무기+affix)
   ↓
T05 (UI 권장 ⭐)
   ↓
T06 (NUnit + docs)
```

## 3. CLAUDE.md §15 정합

| 태스크 | §15 정합 | 예상 라인 |
|---|---|---|
| T01 | §15.4 SO 우선 ✅ | BalanceConfig +30L / Editor 시드 +50L |
| T02 | 변동 X | 검증만 |
| T03 | §15.7 DI Add/Remove ✅ / §15.11 안티 회피 ✅ | PlayerStats +30L / ApplyClassBonus 신규 25L |
| T04a/4b | Editor 도구 (200L 룰 적용) | CurveSimulator +60L total |
| T05 | UI 일관성 ✅ | SlotStarforceView +15L / EnhanceModal +10L |
| T06 | §15.9 NUnit 5 ✅ | NUnit 5 +80L |

## 4. Unity MCP 안전 (CLAUDE.md §13)

각 commit 사이 5~10s 대기. T03 (PlayerStats 신규 필드) 후 도메인 리로드 ~10s.

## 5. 변경 이력
- 2026-05-07: 초기 작성 (Phase 3 Breakdown). 7 태스크 / 7 commit / ~3h 30m.
