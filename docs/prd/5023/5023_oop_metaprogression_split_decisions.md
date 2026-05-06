# 5023 PRD Decisions

> **Phase 2.3 Mediator** — 미결 3건 결정
> 모드: ai-assisted · 일시: 2026-05-06

---

## 1. 요약 표

| Priority | Item | Decision | Status |
|---|---|---|---|
| High | D1 | 호출처 마이그레이션 | **C (Claude 권장)** — Sprint 별 분리 | Done |
| Medium | D2 | SaveSystem.Save 위치 | **C (Claude 권장)** — PRD 원안 + Validator 룰 추가 | Done |
| Medium | D3 | event 위치 | **B (Claude 권장)** — 정적 event 보존 (호환성 우선) | Done |

사용자 결정: **"권장 그대로 진행"** (AI 권장 채택률 100%).

---

## 2. Detailed Decisions

### 2.1 D1 — Sprint 별 호출처 마이그레이션 (옵션 C)

- **Decision**: 즉시 마이그레이션 + Sprint 별 분리
- **Rationale**: 한 Sprint 내 같은 책임 (Character/Chapter / Currency/Equipment / Endgame) 의 호출처 마이그레이션 → 위험 격리
- **PRD Impact**: PRD §5 Sprint 분해 그대로 (이미 Sprint 별 분리)

### 2.2 D2 — SaveSystem.Save 분산 + Validator 룰 (옵션 C)

- **Decision**: PRD 원안 (각 service 메서드 직접 호출) + ProjectValidator 룰 추가
- **Rationale**: 즉시 영속 + 자동 회귀 안전망. 누락 위험 ↓
- **PRD Impact**:
  - Sprint 4 에 ProjectValidator 룰 추가 (~15L) — Service Save 호출 검증
  - 신규 Rule: `MetaProgressionSaveRule` 또는 `WeaponBossRule` 에 통합

### 2.3 D3 — 정적 event 보존 (옵션 B)

- **Decision**: MetaProgression 의 OnCharacterChanged / OnCharacterClassChanged / OnUpgradeChanged / OnEnemyDefeated event 를 instance → **static** 변경
- **Rationale**: 구독자 호출처 변경 최소화 (`MetaProgression.Instance.OnCharacterChanged` → `MetaProgression.OnCharacterChanged` — instance 만 제거)
- **PRD Impact**:
  - MetaProgression.cs 의 event 정의 instance → static
  - Service 가 `MetaProgression.OnCharacterChanged?.Invoke(c)` 발행
  - 구독자 (LimitedShopService 등) `MetaProgression.Instance.X` → `MetaProgression.X` 일괄 변경 (~5 호출처)

---

## 3. 통계

- Total: 3 / Followed: 3 / Override: 0
- 모드: ai-assisted

---

## 4. 변경 이력

| 일자 | 변경 |
|---|---|
| 2026-05-06 | 초기. D1=C / D2=C / D3=B. 권장 채택 100% |
