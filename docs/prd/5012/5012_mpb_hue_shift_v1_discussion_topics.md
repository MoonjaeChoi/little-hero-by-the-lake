# 5012 — MPB Hue Shift Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목.

## High Priority Items

### D1. Hue Shift 대상 — `data.tintColor` vs `SpriteRenderer.color`

- **Original Criticism**: PRD §3.2 `data.tintColor` hue shift → 5008 sprite (icon != null) 효과 0
- **Current Position**: PRD 그대로 진행 시 시각 다양성 효과 사실상 0
- **Discussion Points**:
  - **A**: PRD 그대로 (`data.tintColor` hue shift)
    - 장점: 코드 변경 최소, placeholder 원형 (icon=null) 만 영향
    - 단점: 5008 sprite (icon != null) 영향 0 → **5012 목적 자체 무효화**
  - **B (★권장)**: `SpriteRenderer.color` 직접 hue offset 적용
    - 장점: 5008 sprite 픽셀에 multiplicative tint → 색상 다양화 가능 (red sprite × cyan tint = darker reddish-cyan)
    - 단점: white sprite 무변화 (디자인 의도 — sprite 자체 색상이 강하면 변형 적음 OK)
    - 구현: EnemyController.cs:196 `sr.color = data.icon != null ? Color.white : data.tintColor;` → `sr.color = HueShift(data.icon != null ? Color.white : data.tintColor, OFFSETS[variationIndex]);`
  - **C**: Custom shader (`_HueShift` property + URP custom)
    - 장점: 완전한 hue rotation (per-pixel)
    - 단점: 셰이더 작성 + URP 호환성 부담 + 5012 scope 확대
- **Proposed Resolution Path**: **B 권장** — 5012 목적 (시각 다양성) 정합 + 셰이더 부담 X + EnemyController 1 라인 수정.
- **PRD Impact**: §3.2 hue shift 적용 라인 명시 (EnemyController.cs:196)

### D3. EnemyController.\_baseColor 캐시 회귀

- **Original Criticism**: flash 효과 \_baseColor 복귀 → hue shift 풀림
- **Current Position**: PRD §3.2 hue shift 적용 시점 모호. EnemyController.cs:68 `_baseColor = _sr.color` 가 hue shift 전이면 flash 후 white 로 복귀.
- **Discussion Points**:
  - **A**: \_baseColor 캐시 시점 변경 X (현 코드 그대로)
    - 장점: 코드 변경 최소
    - 단점: hue shift 후 flash → \_baseColor 가 hue 색상 (Initialize 순서에 따름) 또는 white (회귀 위험)
  - **B (★권장)**: hue shift 후 \_baseColor 갱신
    - 장점: flash 후 hue 색상 유지 (시각 일관성)
    - 단점: Initialize 코드 흐름 명시 필수
    - 구현: `_sr.color = HueShift(...); _baseColor = _sr.color;` 순서 강제
  - **C**: \_baseColor 자체를 EnemyHueVariation 결과로 직접 캐시
    - 장점: 명시적
    - 단점: 코드 추가 (B 와 동등)
- **Proposed Resolution Path**: **B 권장** — Initialize 끝부분에서 hue 적용 → \_baseColor 동시 갱신.
- **PRD Impact**: §3.2 \_baseColor 갱신 명시 (Sprint 2 작업 항목)

## Medium Priority Items

### D2. OFFSETS const → SO 화

- **Original Criticism**: §15.4 SO 우선 정책상 const → SO 권장
- **Current Position**: const 유지 시 디자이너 후속 조정 어려움 (코드 수정 필요)
- **Discussion Points**:
  - **A (★권장)**: const 유지
    - 장점: 단순, 5012 scope 최소
    - 단점: 후속 조정 시 코드 수정
    - §15.4 예외 정합 (5단계 고정, 변경 빈도 낮음 — magic number 라기보다 정착 상수)
  - **B**: `BalanceConfig.enemyHueOffsets float[5]` SO 추가
    - 장점: 디자이너 Inspector 조정 가능
    - 단점: +5L wiring (BalanceConfig 직접 호출, GameManager 캐시), Sprint 1 작업량 +10분
  - **C**: `EnemyVariationConfig` 신규 SO
    - 장점: 명시적
    - 단점: over-engineered (5 float 만)
- **Proposed Resolution Path**: **A 권장** — 5012 scope 최소, §15.4 예외 (5단계 정착), 후속 5012.1 에서 SO 화 가능.
- **PRD Impact**: 변경 X

## 요약

- Total Items: 3
- High: 2 (D1, D3)
- Medium: 1 (D2)
- 권장 mode: ai-assisted (Claude 추천 + 사용자 확인)
- 권장 결정 일괄: D1=B / D2=A / D3=B
