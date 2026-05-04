# 5011 — Endgame Boss Sprites Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 결정 필요한 미결 항목.

## High Priority Items

### 🔴 D2. 재생성 ROI — 기존 11 sprite healthy

**Original Criticism**: 기존 11 sprite 모두 metric healthy (transparent 30~80% + rgb_std ≥ 25 + 768×1024). "재생성" 만으로 ROI 불명확.

**Discussion Points**:

- **A**: **PRD 그대로 진행** (5008 baseline + 신규 prompt = 시각 차별성 강화 가정)
  - 장점: 일관 baseline + Daily/Weekly 톤 차별성 강화 + epic boss 강조
  - 단점: 사용자 만족도 X 시 회귀 위험
  - 비용: ~1.5h + 11 PNG 생성 + 사용자 review
- **B (★권장)**: **사용자 시각 review 먼저** — 기존 sprite 만족도 확인 후 결정
  - 장점: 정확한 ROI 평가, 회귀 위험 0
  - 단점: 즉시 진행 X
  - 절차: Pack Preview 또는 EnemyAtlas 에서 11 sprite 확인 → 만족도 보고 → 결정
- **C**: **부분 재생성** — 일부 representative 만 (월요일 + Light 등)
  - 장점: 빠른 시각 비교 가능
  - 단점: 일관성 ↓ (혼재 톤)
- **D**: **5011 보류 (RESERVED)** — 다른 작업 우선
  - 장점: 부담 X
  - 단점: endgame 시각 차별성 지연

**Proposed Resolution**: **B 권장** — 자산이 healthy 상태에서 prompt 차이만으로 재생성하는 것은 사용자 만족도 사전 확인 필요.

## Medium Priority Items

### 🟡 D3. EnemyData SO icon GUID 보존 정책

**Original Criticism**: PNG 만 갱신 시 .meta GUID 보존 정책 명시 X (5009 패턴 답습 가정).

**Discussion Points**:

- **A (★권장)**: GUID 보존 + 같은 PNG 경로 덮어쓰기 (SO 변경 X)
  - 5009 검증된 패턴 답습. 11 EnemyData SO icon path 그대로
- **B**: 기존 PNG 백업 후 재생성 — revert 안전성 ↑
  - DailyBoss_*_backup.png 같은 형태. 다만 commit churn ↑
- **C**: 신규 PNG 별도 경로 + EnemyData SO icon wiring 변경
  - 5009 패턴 위반 + 추가 작업

**Proposed Resolution**: **A** (5009 답습)

## 요약

- Total Items: 2 (D1 Accepted)
- High: 1 (D2 — 재생성 ROI)
- Medium: 1 (D3 — GUID 보존)
- 권장 mode: ai-assisted
- 권장 결정: D2=B (시각 review 먼저) + D3=A (GUID 보존)
