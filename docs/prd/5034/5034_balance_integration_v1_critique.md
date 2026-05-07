# 5034 balance_integration_v1 PRD 리뷰

## 0. 결정 이력
- 2026-05-07: Claude (prd-critiquer) 1차 리뷰

## 1. 종합 점수: 8.6/10

| 카테고리 | 점수 | 비고 |
|---|---|---|
| OOP §15 정합 | 9/10 | BalanceConfig SO 활용, NUnit 명시, 100L 이내. 단 PlayerStats 신규 필드 SerializeField 명시 권장 |
| PG-12 톤 | 10/10 | 밸런스 디자인 자체는 톤 무관, "도파민 영역" / "탱커형/DPS형" 게임 어휘 적정 |
| 성능 | 8/10 | BalanceConfig 배열 35 가벼움. ApplyClassBonus 1회만 (PlayerStats sync) — Update 무영향 |
| 시스템 통합 | 8/10 | stats_v2 / weapon_v2 / gacha_v1 / character_progression_v1 정합. SaveData 마이그레이션 X 명시 |
| Sprint 분해 | 8/10 | 6 Sprint 균형. Sprint 4 (CurveSimulator 확장 1h) 살짝 큼 |

---

## 2. Accept 항목 (6)

### ✅ A1. 50:50 비중 검증 명시 (디자인 §1.1, 3.5)
- character_progression_v1 답습. 캐릭터 진행 50% / 장비 25% (강화) / 옵션 15% / 신화 10% 분포 보존.
- 5034 가 추가 명시: 슬롯 강화는 "장비 25%" 안에 흡수.

### ✅ A2. 메이플 정합 D4=B 옵션 (디자인 §3.4)
- 직업별 특수 스탯 곡선 (전사 STR+20% / 보스 +10%, 궁수 AGI+20% / 크리 +10%, 마법사 WIS+20% / AOE +15%).
- 단순 +Dmg 10% 대비 차별화 + 게임 시너지 강화.

### ✅ A3. Sprint 분해 6/3h15m 균형
- 각 Sprint 15~60분, 의존성 명확 (1→2→3→4→5→6).
- 1 commit/Sprint, 단일 책임.

### ✅ A4. 검증 시나리오 3 명확 (PRD §10)
- 챕터 10 (튜토리얼) / 챕터 20 (튜토리얼 종료) / 챕터 35 (엔드콘텐츠 진입) — 핵심 진행 분기점 모두 cover.

### ✅ A5. NUnit 5+ 명시 (PRD §7.1)
- BalanceMatrixTests / DpsCheckTests / WeaponClassBonusTests / StarforceMultiplierTableTests / CurveSimulatorTests
- §15.9 정합, NUnit 누적 +5

### ✅ A6. 위험 + 완화 5건 사전 명시 (디자인 §7)
- 곡선 재조정 후 SaveData 정합 / CurveSimulator 시뮬 vs 실 Play 차이 / 직업별 특수 스탯 도입 / 메이플 정합 검증 한계 / D4=B 기존 캐릭터 영향 — 모두 완화 방안 제시.

---

## 3. Reject 항목 (0)

명확한 §15 위반 / PG-12 톤 위반 / Magic Number 신규 도입 없음.

다만 **예방적 권고** (Discuss 로 분류):

- ⚠️ Sprint 3 의 PlayerStats 신규 필드 (`bossDamageBonus / critChanceBonus / aoeBonus`) 의 SerializeField + private 명시 권장 (§15.11) — D8 로 분리

---

## 4. Discuss 항목 (3 + PRD 7 = 10)

### 🟡 D1~D7 (PRD §5 자체 명시 — 권장 일괄 채택 가능)
- D1 챕터 매트릭스 — A 균형 권장
- D2 슬롯 곡선 — A 유지 권장
- D3 무기 +N — A damage 곱셈 권장
- D4 직업 시너지 — **B 메이플 정합 권장** ★
- D5 DPS check — B 0.7× 도전 가능 권장
- D6 캐시 진행 — A 50% 단축 권장
- D7 CurveSimulator — A 통합 권장

### 🟡 D8. PlayerStats 신규 필드 가시성

**Current**: PRD §3 Sprint 3 코드 예시에 `stats.AddBossDamageBonus(0.10f)` — getter/setter 가시성 미명시.

**옵션**:
| 옵션 | 설명 | 장점 | 단점 |
|---|---|---|---|
| A | `[SerializeField] private float _bossDamageBonus;` + `public float BossDamageBonus => _bossDamageBonus;` | §15.2 정합, Inspector 편집 가능 | Boilerplate ↑ |
| **B (★권장)** | `private float _bossDamageBonus;` + `Add/Remove` 메서드 | §15.2 정합, 코드 명시적 | Inspector 편집 X |
| C | `public float bossDamageBonus;` (raw) | 단순 | §15.11 안티 |

**Claude 권장**: B — Add/Remove 메서드 (직업 변경 시 차감 가능). 5034 의도 (직업 시너지 동적 적용) 정합.

### 🟡 D9. Sprint 4 작업량 (CurveSimulator 확장 1h)

**Current**: PRD §1 Sprint 4 = 1h (다른 Sprint 15~30분 대비 큼).

**옵션**:
| 옵션 | 설명 |
|---|---|
| A | 그대로 1h 유지 (단일 commit) |
| **B (★권장)** | Sprint 4a (슬롯 통합 30분) + Sprint 4b (무기 + affix 통합 30분) — 2 commit 분리 |
| C | Sprint 4 폐기 — CurveSimulator 미통합 (D7=C 결정) |

**Claude 권장**: B — 분리 시 작업량 균형. 단 의존성 동일 (Sprint 1+2+3 의존).

### 🟡 D10. 5034.1 후속 backlog 적합도

**Current**: PRD §9 5034.1 V매트릭스 / 5차 스킬 (메이플 본판 답습).

**옵션**:
- A: 진행 (메이플 충실 답습)
- **B (★권장)**: 폐기 (LittleHero 단일 캐릭터 게임 — V매트릭스 부적합)
- C: 변형 (별가루 메타 스킬 트리 신규 — Brotato 답습)

**Claude 권장**: B — 단일 캐릭터 게임 톤 정합 우선. 또는 C 추후 별도 backlog (5034 scope 외).

---

## 5. CLAUDE.md §15 정책 위반 점검

| 정책 | 결과 |
|---|---|
| §15.1 SRP | ✅ 통과 (BalanceConfig SO / Service / UI 분리) |
| §15.4 SO 우선 | ✅ 통과 (BalanceConfig 매트릭스 SO 직렬화) |
| §15.6 100L | ✅ 통과 (변경 1~2 파일 신규, 50~100L) |
| §15.7 DI | ✅ 통과 (PlayerStats Add/Remove 메서드 — D8=B 시) |
| §15.9 Test | ✅ NUnit 5+ 명시 |
| §15.11 안티 | △ D8 결정 따라 — public 필드 회피 |

---

## 6. 권장 변경

### 6.1 즉시 변경 (Reject)
없음.

### 6.2 사용자 결정 후 변경 (Discuss)
1. D1~D7 — PRD 권장 일괄 채택 또는 변경
2. D8 — PlayerStats 가시성 (권장 B Add/Remove 메서드)
3. D9 — Sprint 4 분리 (권장 B 2 commit)
4. D10 — 5034.1 후속 적합도 (권장 B 폐기)

### 6.3 후속 backlog 정합
- 5034.1 V매트릭스: D10=B 결정 시 폐기
- 5034.2 시즌 패스 통합: 별도 backlog 보존
- 5034.3 직업 잠재: 별도 backlog 보존

---

## 7. 변경 이력
- 2026-05-07: 초기 작성 (Phase 2.1 Critique). 종합 8.6/10. Accept 6 / Reject 0 / Discuss 10.
