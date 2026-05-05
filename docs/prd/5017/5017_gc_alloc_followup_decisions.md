# 5017 — PRD Decisions

> **Phase 2.3 Mediator 산출** — 사용자 "권장안대로" 일괄 채택.
> 모드: ai-assisted · 일시: 2026-05-06

## 1. 요약

| Priority | Item | Decision |
|---|---|---|
| Medium | D1 | **A** cap 1024 (30 KB 영구) |
| Medium | D2 | **A** struct 변환 + 컴파일 검증 |
| Medium | D3 | **B** wobble 임계 0.005f |

## 2. 결정 + Trade-off

### 2.1 D1=A FormatDamage cache cap 1024

- **Rationale**: 30 KB 영구 비용 < 매 hit alloc 절감 가치. cap 1024 도달 후 stop 누적 — 일반 게임 환경에서 1024 종류 damage 값 도달 어려움 (실제 데이터 검증 필요)
- **PRD Impact**: 변경 X (현재 PRD 그대로)
- **Trade-off**: LRU 미적용 — 1024 stop 후 새 값 cache miss

### 2.2 D2=A DropContext struct 변환

- **Rationale**: 단순 변환 + 컴파일 검증으로 안전 보장. 일반적 by-value 호출만 사용 시 변경 lost 위험 작음
- **PRD Impact**: 변경 X (현재 PRD 그대로)
- **Trade-off**: 호출처 컴파일 검증 필수. 위험 발견 시 C (ObjectPool) 폴백

### 2.3 D3=B wobble 임계 0.005f

- **Rationale**: 0.001f → 0.005f 5배 ↑ — 시각 차이 미미 (wobble amplitude 0.04f 의 12.5% 임계) + native interop 절감 효과 ↑
- **PRD Impact**: §3 Sprint 3 의 임계 0.001f → 0.005f
- **Trade-off**: 0.005f 미만 변동 frame setter 생략 → 약간 stutter 가능 (실제로는 미미)

## 3. v2 PRD 변경 diff

- §3 Sprint 3 EnemyController.UpdateAnim:
  - `Mathf.Abs(wobble - _lastWobble) > 0.001f` → `> 0.005f`
  - `Mathf.Abs(_punchScale - _lastPunch) > 0.001f` → `> 0.005f`

## 4. 변경 이력
- 2026-05-06: 초기 작성 (사용자 권장안 일괄 채택)
