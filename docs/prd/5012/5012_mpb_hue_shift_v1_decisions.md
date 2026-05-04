# 5012 — MPB Hue Shift PRD Decisions

> **Phase 2.3 Mediator 산출** — 미결 항목 3건 결정 기록
> 모드: ai-assisted · 일시: 2026-05-04 · 사용자 결정: "권장안대로"

## 1. 요약 표

| Priority | Item | Topic | Decision | Status |
|---|---|---|---|---|
| High | D1 | Hue shift 대상 | B (`SpriteRenderer.color` multiplicative tint) | Done |
| Medium | D2 | OFFSETS SO 화 | A (const 유지) | Done |
| High | D3 | \_baseColor 캐시 시점 | B (hue 적용 후 \_baseColor 갱신) | Done |

## 2. Detailed Decisions

### 2.1 D1 — Hue shift 대상

- **Decision**: B — `SpriteRenderer.color` 직접 multiplicative tint
- **Mode**: ai-assisted (Claude 권장 + 사용자 "권장안대로")
- **Rationale**: 5008 LayerDiffuse RGBA sprite 30 archetype 모두 `data.icon != null` → tintColor 무관. SpriteRenderer.color = HueShift(...) multiplicative tint 가 sprite 픽셀에 곱연산 → 색상 다양화 가능. 셰이더 부담 X.
- **PRD Impact**:
  - §3.2: hue shift 적용 라인 명시 — EnemyController.cs:196 `sr.color = HueShift(sr.color, OFFSETS[variationIndex])`
  - Sprint 2 commit 메시지 갱신
- **Trade-off**: white sprite 무변화 (디자인 의도 — sprite 자체 색상 강하면 변형 적음 OK)

### 2.2 D2 — OFFSETS const 유지

- **Decision**: A — const 유지
- **Mode**: ai-assisted
- **Rationale**: 5012 scope 최소. 5단계 정착 상수로 §15.4 예외 정합. 후속 5012.1 에서 SO 화 가능.
- **PRD Impact**: 변경 X
- **Trade-off**: 디자이너 후속 조정 시 코드 수정 필요 (변경 빈도 낮아 허용)

### 2.3 D3 — \_baseColor 갱신 시점

- **Decision**: B — hue shift 후 `_baseColor = _sr.color` 갱신
- **Mode**: ai-assisted
- **Rationale**: flash 효과 (line 174~184) 가 \_baseColor 로 복귀 — hue 색상 유지하려면 \_baseColor 가 hue 결과로 캐시되어야. Initialize 끝부분에서 hue 적용 → \_baseColor 동시 갱신 명시.
- **PRD Impact**:
  - §3.2 Sprint 2 작업 항목 명시:
    1. (기존) `_baseColor = _sr != null ? _sr.color : Color.white;` (line 68)
    2. (신규) `if (variationIndex > 0 && data.bossTier == BossTier.None) { _sr.color = HueShift(_sr.color, OFFSETS[variationIndex]); _baseColor = _sr.color; }`
- **Trade-off**: Initialize 코드 흐름 명시 필수 (구현 시 회귀 방지)

## 3. 통계

- Total Items: 3
- AI Recommendations Followed: 3 / 3 (100%)
- User Overrides: 0
- 모드: ai-assisted (사용자 응답: "권장안대로")

## 4. v2_prd.md 변경 diff 요약

(rebuttal 초안 → mediator 최종)

- §3.2 Sprint 2 — hue 적용 라인 명시 + \_baseColor 갱신 명시 (D1+D3)
- §3.1 OFFSETS const 유지 (D2 — 변경 X)
- Sprint 2 commit 메시지 갱신

## 5. 변경 이력
- 2026-05-04: 초기 작성 (Phase 2.3 Mediator).
