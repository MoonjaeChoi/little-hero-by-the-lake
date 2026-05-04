# 5007 damage_hit_effect_v2 PRD Rebuttal (Phase 2.2)

> **검토자**: Claude Code (자동) · **일시**: 2026-05-04
> **Critique**: `5007_damage_hit_effect_v2_critique.md` (8.9/10, Accept 12 / Reject 0 / Discuss 4)

## 1. 요약 표

| Category | Criticism | Priority | Status | Action |
|---|---|---|---|---|
| Sprint 분해 | 3.2 1 commit 너무 큼 (8 IAttackPattern + DamageDealer + EquipmentManager) | High | Discuss | mediator |
| 결합도 | EnemyController.OnHit 의 PlayerStats 의존성 | Medium | Discuss | mediator |
| 인프라 | CinematicEffectsConfig prefab wiring 방식 | Medium | Discuss | mediator |
| 시각 안전 | cap 2.0× 적용 위치 (Resolver vs SO) | Low | Discuss | mediator |

## 2. 품질 메트릭

- 총 Discussion Points: 4
- Accepted (PRD v1 의 Accept 12 → 자동 v2 반영): 0 (rebuttal 단계 변경 X — Critique 의 Accept 는 이미 PRD v1 에 반영됨)
- Rejected: 0
- Discuss: 4 (100%)
- §15 정합률: 100%

## 3. Detailed Responses

### 3.1 Sprint 3.2 commit 분할 (High)

**Original**: 1 commit 으로 8 IAttackPattern + DamageDealer + EquipmentManager 일괄 마이그레이션
- **Rebuttal**: 변경량 ~150L 추정. 1 commit 에 묶으면 회수 어려움. 2 분할이 단일 책임 원칙에 부합.
- **Status**: Discuss → mediator
- **Resolution**: D1 권장 = B (2 분할: 인프라 + 8 IAttackPattern)

### 3.2 PlayerStats 의존성 (Medium)

**Original**: EnemyController.OnHit 가 PlayerStats.Instance 를 query 해서 avgDamage 획득 → DamageSystem 에서 처리하면 의존성 X
- **Rebuttal**: 권장 B 가 더 깨끗 (DamageSystem 이 이미 PlayerStats 참조 중)
- **Status**: Discuss → mediator
- **Resolution**: D2 권장 = B (DamageSystem 이 OnHit 호출 시 avgDamage 전달)

### 3.3 CinematicEffectsConfig wiring (Medium)

**Original**: Sprint 4.2 의 "정적 로딩 (Resources)" 명세 모호
- **Rebuttal**: 137 prefab SerializeField 부담 vs Resources.Load 정적 cache. 후자가 §15.7 DI + 운영 효율 양쪽 우수.
- **Status**: Discuss → mediator
- **Resolution**: D3 권장 = A (Resources.Load 정적 cache + RuntimeInitializeOnLoadMethod)

### 3.4 cap 2.0× 위치 (Low)

**Original**: max punch scale ceiling 적용 위치 미명시
- **Rebuttal**: 안전 cap 은 hardcode (잘못된 SO 값으로 사용자 경험 망가지지 않도록). Resolver 내부 Mathf.Min(2.0f, ...) + 주석 명시.
- **Status**: Discuss → mediator
- **Resolution**: D4 권장 = B (Resolver hardcode, 향후 필요시 SO 화)

## 4. PRD v2 적용 변경 요약

(Rebuttal 단계 — Accept 0건이므로 v2 = v1 동일. mediator 결정 후 최종 v2.)

## 5. 변경 이력

- 2026-05-04: 초기 작성 (Phase 2.2 Rebuttal — Discuss 4건 mediator 분리)
