# 5034 balance_integration_v1 PRD Discussion Topics

> **Phase 2.2 산출** — mediator (Phase 2.3) 에서 결정 필요한 미결 항목 10건.
> **권장 mode**: ai-assisted (Claude 권장 + 사용자 응답 대기)

---

## High Priority Items

(없음)

---

## Medium Priority Items (8)

### D1. 챕터 진행 매트릭스 (디자인 §3.1)

- **Current Position**: PRD §3.1 — 챕터 N → 권장 Lv N+9 / 권장 무기 ~N×0.7 / 권장 ⭐ ~N×0.6
- **Discussion Points**:
  | 옵션 | 설명 | 장점 | 단점 |
  |---|---|---|---|
  | **A (★권장)** | 균형 (1챕터당 +0.5⭐, +0.7+N) | 메이플 답습, 점진적 | — |
  | B | 가속 (1챕터당 +1⭐) | 빠른 진행 | 캐시 유인 ↓ |
  | C | 둔화 (1챕터당 +0.3⭐) | 캐시 유인 ↑ | 무료 사용자 답답 |
- **Claude 권장**: **A** — 균형 (메이플 답습)

### D2. 슬롯 강화 곡선 (디자인 §3.2)

- **Current Position**: 5033.4 적용 — ⭐0=0% / ⭐15=100% / ⭐20=200%
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | **A (★권장)** | 현재 유지 (직관, ⭐15=2배) |
  | B | 강화 (⭐15=150% / ⭐20=300%) |
  | C | 평탄화 (⭐0~⭐10 빠르게, ⭐10~⭐20 느림) |
- **Claude 권장**: **A** — 직관 + 검증 단순

### D3. 무기 강화 +N 곡선 (디자인 §3.3)

- **Current Position**: 5033.4 — attackConfig.damage 곱셈 (StarforceMultiplierTable 답습)
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | **A (★권장)** | attackConfig.damage 곱셈 (+25 = 2배) |
  | B | 모든 stat 곱셈 (+25 = 1.75배 균등) |
  | C | STR 코어 추가 (+25 = +50 STR) |
- **Claude 권장**: **A** — 5033.4 답습, 단순

### D4. 직업 시너지 (디자인 §3.4) ★ 핵심

- **Current Position**: WeaponClassBonus +10% (단순) — 메이플 정합 부족
- **Discussion Points**:
  | 옵션 | 설명 | 장점 | 단점 |
  |---|---|---|---|
  | A | 단순 강화 (+15% 일괄) | 작업 단순 | 차별화 부족 |
  | **B (★권장)** | 직업별 특수 스탯 (전사 STR+20% / 보스+10%, 궁수 AGI+20% / 크리+10%, 마법사 WIS+20% / AOE+15%) | 메이플 정합 + 차별화 | PlayerStats 3 신규 필드 |
  | C | 매트릭스 (계정 단위 버프) | 메이플 충실 답습 | 단일 캐릭터 게임 적합도 ↓ |
- **Claude 권장**: **B** — 메이플 정합 + 차별화

### D5. DPS check 기준 (디자인 §3.5)

- **Current Position**: PRD §3.5 — DPS_player ≥ DPS_target × 1.0 (권장 진행) / × 0.7 (권장 미달 도전 가능)
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | A | 1.0× 강제 (권장 진행 미달 시 진입 금지) |
  | **B (★권장)** | 0.7× 도전 가능 (현재 PRD 명시) |
  | C | 1.2× 여유 (도전 영역 ↓) |
- **Claude 권장**: **B** — 도전 가능 (UX 친화)

### D6. 캐시 vs 무료 진행 비율 (디자인 §3.6)

- **Current Position**: PRD §3.6 — 무료 35 챕터 ~30~40h, 캐시 50% 단축
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | **A (★권장)** | 50% 단축 (메이플 표준) |
  | B | 30% 단축 (캐시 유인 ↓) |
  | C | 무관 (캐시 X) |
- **Claude 권장**: **A** — 메이플 표준

### D7. CurveSimulator 통합 (디자인 §1.3 결함 #4)

- **Current Position**: PRD Sprint 4 — 슬롯 + 무기 + affix 통합 시뮬
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | **A (★권장)** | 슬롯 + 무기 + affix 모두 통합 |
  | B | 슬롯만 (작업 간소화) |
  | C | 미통합 (Sprint 4 폐기) |
- **Claude 권장**: **A** — 종합 검증

### D8. PlayerStats 신규 필드 가시성 (§15.11 예방)

- **Current Position**: Sprint 3 코드 예시에 `stats.AddBossDamageBonus(0.10f)` — 가시성 미명시
- **Discussion Points**:
  | 옵션 | 설명 | 장점 | 단점 |
  |---|---|---|---|
  | A | `[SerializeField] private + public property` | Inspector 편집 | Boilerplate ↑ |
  | **B (★권장)** | `private + Add/Remove 메서드` | §15.2/§15.7 정합 | Inspector 편집 X |
  | C | `public float bossDamageBonus;` (raw) | 단순 | §15.11 안티 |
- **Claude 권장**: **B** — Add/Remove 메서드 (직업 변경 시 차감 가능)

---

## Low Priority Items (2)

### D9. Sprint 4 작업량 분리 (Sprint 분해)

- **Current Position**: PRD §1 Sprint 4 = 1h (다른 Sprint 15~30분 대비 큼)
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | A | 1h 그대로 유지 (단일 commit) |
  | **B (★권장)** | Sprint 4a (슬롯 30분) + 4b (무기+affix 30분) — 2 commit 분리 |
  | C | Sprint 4 폐기 (D7=C 결정 시) |
- **Claude 권장**: **B** — 작업량 균형 + 회수 용이

### D10. 5034.1 후속 backlog 적합도

- **Current Position**: PRD §9 — 5034.1 V매트릭스 / 5차 스킬 (메이플 본판 답습)
- **Discussion Points**:
  | 옵션 | 설명 |
  |---|---|
  | A | 진행 (메이플 충실 답습) |
  | **B (★권장)** | 폐기 (LittleHero 단일 캐릭터 게임 부적합) |
  | C | 변형 (별가루 메타 스킬 트리 신규 — Brotato 답습) |
- **Claude 권장**: **B** — 단일 캐릭터 게임 톤 정합 우선

---

## 요약

- **Total Items**: 10 (Medium 8 + Low 2)
- **Claude 권장 일괄**: D1=A, D2=A, D3=A, D4=B, D5=B, D6=A, D7=A, D8=B, D9=B, D10=B
- **권장 mode**: **ai-assisted** — Claude 권장 응답 후 사용자 확인. 자연어 응답 (예 "권장 그대로", "D4=A, 나머지 권장") 모두 가능.
